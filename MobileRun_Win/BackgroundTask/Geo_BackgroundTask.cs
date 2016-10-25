using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Storage;
using Windows.UI.Xaml;

namespace BackgroundTask
{
    public sealed class Geo_BackgroundTask : IBackgroundTask, IDisposable
    {
        private bool started;

        private Geolocator geolocator;
        private Accelerometer accelerometer;
        private System.Threading.Timer timer;
        private BackgroundTaskDeferral Deferral;

        public void Dispose()
        {
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            accelerometer = Accelerometer.GetDefault();
            geolocator = new Geolocator() { DesiredAccuracy = PositionAccuracy.High/*, ReportInterval = 5000, MovementThreshold = 10*/ };

            if (accelerometer != null && geolocator != null)
            {
                started = false;
                uint minreport = accelerometer.MinimumReportInterval;
                accelerometer.ReportInterval = (minreport > 10) ? minreport : 10;

                accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

                Deferral = taskInstance.GetDeferral();

                taskInstance.Canceled += TaskInstance_Canceled;

                timer = new System.Threading.Timer(new System.Threading.TimerCallback(Timer_Tick), this, 0, 5000);
            }
        }

        private async void Timer_Tick(object obj)
        {
            if (!(bool)ApplicationData.Current.LocalSettings.Values["is_app_active"])
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync();
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                if (folder != null)
                {
                    StorageFile file = await folder.CreateFileAsync("postions_list.txt", started ? CreationCollisionOption.OpenIfExists : CreationCollisionOption.ReplaceExisting);
                    string position = geoposition.Coordinate.Point.Position.Altitude.ToString() +
                                    "," + geoposition.Coordinate.Point.Position.Latitude.ToString() +
                                    "," + geoposition.Coordinate.Point.Position.Longitude.ToString() +
                                    "," + DateTime.Now.ToString();
                    File.AppendAllLines(file.Path, new List<string>() { position }, Encoding.UTF8);
                    started = true;
                }
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (accelerometer != null && geolocator != null)
            {
                accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                accelerometer.ReportInterval = 0;
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            Deferral.Complete();
        }

        private void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
        }
    }
}
