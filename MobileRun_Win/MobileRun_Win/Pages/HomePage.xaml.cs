using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace MobileRun_Win.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        ViewModel.HomePageViewModel viewmodel;

        public HomePage()
        {
            this.InitializeComponent();
            viewmodel = new ViewModel.HomePageViewModel();
            this.DataContext = viewmodel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                StatusBar.GetForCurrentView().ForegroundColor = Windows.UI.Colors.Black;
                StatusBar.GetForCurrentView().BackgroundColor = Windows.UI.Color.FromArgb(255, 242, 246, 247);
                StatusBar.GetForCurrentView().BackgroundOpacity = 1;
            }
            catch (Exception)
            {
            }
            RotateRing_SB.RepeatBehavior = new Windows.UI.Xaml.Media.Animation.RepeatBehavior { Count = 3 };
            RotateRing_SB.Begin();
        }

        private void start_but_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pages.RunPage));
        }

        private void start_but_canvas_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void rankings_but_canvas_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void history_but_canvas_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
