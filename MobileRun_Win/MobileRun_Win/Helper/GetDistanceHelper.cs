using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileRun_Win.Helper
{
    public class GetDistanceHelper
    {
        public static double GetDistance(double la1, double lon1, double la2, double lon2) //获得两坐标点之间的距离，单位：m
        {
            la1 = 90 - la1;
            la2 = 90 - la2;
            double temp_c = Math.Sin(la1) * Math.Sin(la2) * Math.Cos(lon1 - lon2) + Math.Cos(la1) * Math.Cos(la2);
            return (6371.004 * Math.Acos(temp_c) * Math.PI / 180 * 1000);
        }
    }
}
