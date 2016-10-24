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

namespace BackgroundTask
{
    public sealed class Geo_BackgroundTask : IBackgroundTask, IDisposable
    {
        private bool started;

        private Geolocator geolocator;
        private Accelerometer accelerometer;
        private BackgroundTaskDeferral Deferral;

        public void Dispose()
        {
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            accelerometer = Accelerometer.GetDefault();
            geolocator = new Geolocator();

            if (accelerometer != null && geolocator != null)
            {
                started = false;
                uint minreport = accelerometer.MinimumReportInterval;
                accelerometer.ReportInterval = (minreport > 10) ? minreport : 10;
                geolocator.ReportInterval = 5000;

                accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                geolocator.PositionChanged += Geolocator_PositionChanged;

                Deferral = taskInstance.GetDeferral();

                taskInstance.Canceled += TaskInstance_Canceled;
            }
        }

        private async void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (folder != null)
            {
                StorageFile file = await folder.CreateFileAsync("postions_list.txt", started ? CreationCollisionOption.OpenIfExists : CreationCollisionOption.ReplaceExisting);
                string position = args.Position.Coordinate.Point.Position.Altitude.ToString() + "," + args.Position.Coordinate.Point.Position.Latitude.ToString() + "," + args.Position.Coordinate.Point.Position.Longitude.ToString();
                File.AppendAllLines(file.Path, new List<string>() { position }, Encoding.UTF8);
                started = true;
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (accelerometer != null && geolocator != null)
            {
                accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                geolocator.PositionChanged -= Geolocator_PositionChanged;
                accelerometer.ReportInterval = 0;
                geolocator.ReportInterval = 0;
            }
            Deferral.Complete();
        }

        private void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
        }
    }
}
