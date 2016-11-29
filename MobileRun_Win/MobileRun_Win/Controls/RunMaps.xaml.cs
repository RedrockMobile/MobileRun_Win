using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MobileRun_Win.Controls
{
    public sealed partial class RunMaps : UserControl
    {
        private bool is_adding_new_positions_from_back;

        private DateTime last_date;
        private Geolocator geolocator;
        private List<BasicGeoposition> lines;

        public string[] NewPositionsFromBack //后台传来的新位置
        {
            get
            {
                return ((string[])GetValue(NewPositionsFromBackProperty));
            }
            set
            {
                SetValue(NewPositionsFromBackProperty, value);
            }
        }
        private static readonly DependencyProperty NewPositionsFromBackProperty = DependencyProperty.Register("NewPositionsFromBack", typeof(string[]), typeof(RunMaps), new PropertyMetadata(null, new PropertyChangedCallback(GetNewPositionsFromBack)));

        private static async void GetNewPositionsFromBack(DependencyObject d, DependencyPropertyChangedEventArgs e) //处理后台传来的新位置
        {
            RunMaps rm = d as RunMaps;
            if (rm != null)
            {
                try
                {
                    await rm.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        rm.is_adding_new_positions_from_back = true;
                        for (int i = 0; i < rm.NewPositionsFromBack.Length; i++)
                        {
                            string[] new_params = rm.NewPositionsFromBack[i].Split(',');
                            rm.last_date = Convert.ToDateTime(new_params[3]);
                            if (rm.PositionJundge(Convert.ToDouble(new_params[1]), Convert.ToDouble(new_params[2])))
                                rm.AddNewPolyline(new_params[0], new_params[1], new_params[2]);
                        }
                        MapControl.SetLocation((rm.maps.Children[0] as Grid), new Geopoint(rm.lines[rm.lines.Count - 1])); //将定位点设置到新的位置
                        rm.is_adding_new_positions_from_back = false;
                    });
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    StorageFile file = await folder.GetFileAsync("postions_list.txt");
                    if (file != null)
                    {
                        await file.DeleteAsync();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public RunMaps()
        {
            this.InitializeComponent();
            is_adding_new_positions_from_back = false;
            lines = new List<BasicGeoposition>();
            GetLoaction();
        }

        private void AddNewPolyline(string altitude, string latitude, string longitude) //在地图中绘制新的路径
        {
            BasicGeoposition new_position = new BasicGeoposition //新添加的位置
            {
                Altitude = Convert.ToDouble(altitude),
                Latitude = Convert.ToDouble(latitude),
                Longitude = Convert.ToDouble(longitude)
            };
            AddNewPolyline(new_position);
        }

        private async void AddNewPolyline(BasicGeoposition new_position) //在地图中绘制新的路径
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lines.Add(new_position); //将新的位置添加到点集合中
                MapPolyline temp_line = new MapPolyline() //创建新的MapPolyline以绘制路径
                {
                    StrokeColor = Colors.Purple,
                    StrokeThickness = 5,
                    StrokeDashed = false
                };
                temp_line.Path = new Geopath(new List<BasicGeoposition>() //添加起始点和终点以设置MapPolyline的路径
                    {
                        lines[lines.Count - 2],
                        lines[lines.Count - 1]
                    });
                maps.MapElements.Add(temp_line); //将MapPolyline添加到地图控件中
            });
        }

        private async void GetLoaction()
        {
            GeolocationAccessStatus access_status = await Geolocator.RequestAccessAsync();
            switch (access_status) //判断位置授权状态
            {
                case GeolocationAccessStatus.Unspecified: //未授权获取地理位置
                    {
                        new MessageDialog("您未授权约跑获取设备的地理位置\n请前往设置", "约跑");
                        await Launcher.LaunchUriAsync(new Uri("ms-settings://privacy/location"));
                    }; break;
                case GeolocationAccessStatus.Denied: //已拒绝获取地理位置
                    {
                        new MessageDialog("您已拒绝约跑获取设备的地理位置\n请前往设置", "约跑");
                        await Launcher.LaunchUriAsync(new Uri("ms-settings://privacy/location"));
                    }; break;
            }
            if (access_status != GeolocationAccessStatus.Allowed)
                return;
            geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.High, DesiredAccuracyInMeters = 0, ReportInterval = 5000 }; //将精确度设置成最高，并且每5s通知一次位置变化
            Geoposition first_location = await geolocator.GetGeopositionAsync();
            BasicGeoposition basicgeoposition = new BasicGeoposition
            {
                Altitude = first_location.Coordinate.Point.Position.Altitude,
                Latitude = first_location.Coordinate.Point.Position.Latitude,
                Longitude = first_location.Coordinate.Point.Position.Longitude
            };
            lines.Add(basicgeoposition);
            last_date = DateTime.Now; //将此刻的时间保存
            geolocator.PositionChanged += Geolocator_PositionChanged;
            maps.Center = new Geopoint(basicgeoposition);
            MapControl.SetLocation((maps.Children[0] as Grid), maps.Center); //将定位点设置到地图中心
        }

        private bool PositionJundge(double latitude, double longtitude)
        {
            if (lines.Count > 0) //获取上一次的位置并且计算两点距离
            {
                double last_latitude = lines[lines.Count - 1].Latitude;
                double last_longitude = lines[lines.Count - 1].Longitude;
                double distance = GetDistance(last_latitude, last_longitude, latitude, longtitude);
                if (PositionJundge(distance))
                {
                    last_date = DateTime.Now;
                    Debug.WriteLine(distance);
                    return true;
                }
            }
            return false;
        }

        private bool PositionJundge(double distance) //判断距离改变是否合理
        {
            TimeSpan time_delta = DateTime.Now - last_date;
            if ((distance) / (time_delta.TotalSeconds) > 5 || distance > 30) //判断标准：人正常跑步的速度为4m/s左右
                return false;
            else
            {
                this.distance.Text = "位置移动了" + distance + "m";
                return true;
            }
        }

        private double GetDistance(double la1, double lon1, double la2, double lon2) //获得两坐标点之间的距离，单位：m
        {
            la1 = 90 - la1;
            la2 = 90 - la2;
            double temp_c = Math.Sin(la1) * Math.Sin(la2) * Math.Cos(lon1 - lon2) + Math.Cos(la1) * Math.Cos(la2);
            return (6371.004 * Math.Acos(temp_c) * Math.PI / 180 * 1000);
        }

        private async void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args) //当定位获得的位置发生变化时
        {
            if (!is_adding_new_positions_from_back) //如果没有在添加后台传来的位置
            {
                try
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        BasicGeoposition now_position = new BasicGeoposition //当前的位置
                        {
                            Altitude = args.Position.Coordinate.Point.Position.Altitude,
                            Latitude = args.Position.Coordinate.Point.Position.Latitude,
                            Longitude = args.Position.Coordinate.Point.Position.Longitude
                        };
                        if (!PositionJundge(now_position.Latitude, now_position.Longitude))
                            return;
                        AddNewPolyline(now_position);
                        MapControl.SetLocation((maps.Children[0] as Grid), new Geopoint(now_position)); //将定位点设置到新的位置
                    });
                }
                catch (Exception)
                {
                }
            }
        }

        private async void find_me_Click(object sender, RoutedEventArgs e) //重新定位并将定位点设置在地图中心
        {
            try
            {
                Geoposition position = await geolocator.GetGeopositionAsync();
                if (PositionJundge(position.Coordinate.Point.Position.Latitude, position.Coordinate.Point.Position.Longitude))
                {
                    last_date = DateTime.Now;
                    MapControl.SetLocation((maps.Children[0] as Grid), position.Coordinate.Point);
                    maps.Center = position.Coordinate.Point;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
