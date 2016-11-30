using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileRun_Win.API
{
    public class MobileRunAPI
    {
        /// <summary>
        /// HTTP方法:
        /// POST
        /// 参数:
        /// stuId、pwd
        /// 返回值:
        /// json数据
        /// 备注:
        /// 参数中stuId为学号、pwd为身份证后六位；返回的数据为token和用户信息等数据。
        /// </summary>
        public const string Login = "http://enroll.lot.cat/sanzou/user/login";

        /// <summary>
        /// HTTP方法:
        /// POST
        /// 参数:
        /// stuId、timestamp、sign、data
        /// 返回值:
        /// success或者false或者overtime
        /// 备注:
        /// stuId为学号、timestamp为客户端上传的时间，以秒为单位、sign为先AES然后md5加密的验证数据、data为AES后的真实数据，具体参数说明参照当时发的图片。
        /// </summary>
        public const string Update = "http://enroll.lot.cat/sanzou/user/update";

        /// <summary>
        /// HTTP方法:
        /// POST
        /// 参数:
        /// stuId
        /// 返回值:
        /// json数据
        /// 备注:
        /// 返回值中包含个人排名和班级排名还有全校个人排名和全校班级排名。
        /// </summary>
        public const string Rank = "http://enroll.lot.cat/sanzou/user/rank";
    }
}
