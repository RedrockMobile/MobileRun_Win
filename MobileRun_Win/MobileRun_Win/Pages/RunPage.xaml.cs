using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace MobileRun_Win.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RunPage : Page
    {
        private int new_positions_file_length; //后台用于记录坐标的文件的总行数

        private Accelerometer accelerometer;
        private DeviceUseTrigger trigger;
        private DispatcherTimer timer;
        private BackgroundTaskRegistration registration;

        public RunPage()
        {
            this.InitializeComponent();

            new_positions_file_length = 0;
            accelerometer = Accelerometer.GetDefault();
            if (accelerometer != null)
            {
                trigger = new DeviceUseTrigger();
                timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) };
                timer.Tick += Timer_Tick;
                App.Current.Suspending += Current_Suspending;
                App.Current.Resuming += Current_Resuming;
            }
        }

        private void Current_Resuming(object sender, object e) //在APP会到前台之前注明APP在前台
        {
            ApplicationData.Current.LocalSettings.Values["is_app_active"] = true;
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e) //在APP挂起之前注明APP不在前台
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            ApplicationData.Current.LocalSettings.Values["is_app_active"] = false;
            deferral.Complete();
        }

        private async void Timer_Tick(object sender, object e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile file = await folder.GetFileAsync("postions_list.txt");
                if (file != null)
                {
                    //string lines = File.ReadAllText(file.Path);
                    //List<string> lines = (List<string>)(await FileIO.ReadLinesAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8));
                    string[] lines = File.ReadAllLines(file.Path, System.Text.Encoding.UTF8);
                    if (lines.Length != new_positions_file_length)
                    {
                        maps.NewPositionsFromBack = lines;
                        new_positions_file_length = lines.Length;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private async Task<bool> StartBackgroundTask()
        {
            bool started = false;

            foreach (var item in BackgroundTaskRegistration.AllTasks.Values) //只能允许一个后台任务实例存在，因此要先结束之前的任务
            {
                if (App.geolocator_task_name == item.Name)
                {
                    ((BackgroundTaskRegistration)item).Unregister(true);
                    break;
                }
            }

            BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            switch (backgroundAccessStatus)
            {
                case BackgroundAccessStatus.Unspecified:
                case BackgroundAccessStatus.Denied: break; //无法运行后台任务

                default: break; //后台任务已经注册
            }

            if (trigger != null)
            {
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder() { Name = App.geolocator_task_name, TaskEntryPoint = App.geolocator_task_entrypoint };
                builder.SetTrigger(trigger);
                registration = builder.Register();
                registration.Completed += Registration_Completed;

                try
                {
                    DeviceTriggerResult result = await trigger.RequestAsync(accelerometer.DeviceId);
                    switch (result) //判断触发器请求的结果
                    {
                        case DeviceTriggerResult.Allowed: { started = true; }; break;
                        case DeviceTriggerResult.LowBattery: break;
                        case DeviceTriggerResult.DeniedBySystem: break;
                        case DeviceTriggerResult.DeniedByUser: break;
                    }
                }
                catch (Exception) //之前的后台任务仍然在运行
                {
                    foreach (var item in BackgroundTaskRegistration.AllTasks.Values)
                    {
                        if (App.geolocator_task_name == item.Name)
                        {
                            item.Unregister(true);
                            break;
                        }
                    }
                }
            }

            return started;
        }

        private async void Registration_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args) //注册的后台任务结束
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    args.CheckResult();
                }
                catch (Exception)
                {
                }
            });

            if (registration != null)
            {
                registration.Unregister(false);
                registration = null;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (await StartBackgroundTask())
                timer.Start();
        }
    }
}
