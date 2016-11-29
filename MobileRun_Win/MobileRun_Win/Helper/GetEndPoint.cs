using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MobileRun_Win.Helper
{
    public class GetEndPoint
    {
        public Point GetPoint(Point p1, Point p2, double distance)
        {
            double third = Math.Sqrt((Math.Pow((p1.X - p2.X), 2) + Math.Pow((p1.Y - p2.Y), 2)));
            double cos_angle = (p2.Y - p1.Y) / third;
            double x = distance * cos_angle;
            cos_angle = Math.Cos(90 - Math.Acos(cos_angle));
            double y = distance * cos_angle;
            return new Point(x, y);
        }
    }
}
