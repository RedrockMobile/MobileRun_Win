using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MobileRun_Win.HttpRequests
{
    public class MobileRunHttpClient
    {
        public static async Task<string> LoginRequest(string stuId, string pwd)
        {
            HttpClient httpclient = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
            string result = null;
            try
            {
                param.Add(new KeyValuePair<string, string>(Params.APIParams.stuId, stuId));
                param.Add(new KeyValuePair<string, string>(Params.APIParams.pwd, pwd));
                response = await httpclient.PostAsync(API.MobileRunAPI.Login, new FormUrlEncodedContent(param));
                result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }
    }
}
