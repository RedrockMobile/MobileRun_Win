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
        private Accelerometer accelerometer;
        private DeviceUseTrigger trigger;
        private DispatcherTimer timer;
        private BackgroundTaskRegistration registration;

        public RunPage()
        {
            this.InitializeComponent();

            accelerometer = Accelerometer.GetDefault();
            if (accelerometer != null)
            {
                trigger = new DeviceUseTrigger();
                timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) };
                timer.Tick += Timer_Tick;
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile file = await folder.GetFileAsync("postions_list.txt");
                if (file != null)
                {
                    report.Text = "";
                    string lines = File.ReadAllText(file.Path);
                    report.Text = lines;
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //if (await StartBackgroundTask())
            //    new MessageDialog("后台任务启动成功", "约跑");
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
