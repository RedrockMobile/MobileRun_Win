using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Newtonsoft.Json;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace MobileRun_Win.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                StatusBar.GetForCurrentView().ForegroundColor = Windows.UI.Colors.White;
                StatusBar.GetForCurrentView().BackgroundColor = Windows.UI.Color.FromArgb(255, 69, 61, 126);
                StatusBar.GetForCurrentView().BackgroundOpacity = 1;
            }
            catch (Exception)
            {
            }
        }

        private async Task Login()
        {
            if (System.String.IsNullOrWhiteSpace(student_num.Text) || System.String.IsNullOrWhiteSpace(password.Password))
            {
                await new MessageDialog("用户名或密码不能为空！", "登录失败").ShowAsync();
                return;
            }
            string json = await HttpRequests.MobileRunHttpClient.LoginRequest(student_num.Text, password.Password);
            if (json != null)
            {
                try
                {
                    Model.User user = JsonConvert.DeserializeObject<Model.User>(json);
                    App.settings.Values[Params.ClientParams.user_classNum] = user.data.classNum;
                    App.settings.Values[Params.ClientParams.user_college] = user.data.college;
                    App.settings.Values[Params.ClientParams.user_gender] = user.data.gender;
                    App.settings.Values[Params.ClientParams.user_grade] = user.data.grade;
                    App.settings.Values[Params.ClientParams.user_major] = user.data.major;
                    App.settings.Values[Params.ClientParams.user_name] = user.data.name;
                    App.settings.Values[Params.ClientParams.user_token] = user.data.token;
                    this.Frame.Navigate(typeof(Pages.HomePage));
                }
                catch (Exception)
                {
                    await new MessageDialog("请重试！", "登录失败").ShowAsync();
                    return;
                }
            }
            else
            {
                await new MessageDialog("请重试！", "登录失败").ShowAsync();
                return;
            }
        }

        private async void login_but_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private void student_num_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                password.Focus(FocusState.Keyboard);
            }
        }

        private async void password_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await Login();
            }
        }
    }
}
