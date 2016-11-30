using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileRun_Win.ViewModel
{
    public class HomePageViewModel
    {
        private string _user_name;
        public string UserName
        {
            get
            {
                _user_name = App.settings.Values[Params.ClientParams.user_name].ToString();
                return _user_name;
            }
        }
    }
}
