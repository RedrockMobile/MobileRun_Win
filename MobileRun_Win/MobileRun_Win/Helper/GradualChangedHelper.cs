using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace MobileRun_Win.Helper
{
    public class GradualChangedHelper
    {
        private const double _distance = 500; //颜色渐变的范围距离，单位m
        private static readonly Color _color1 = Color.FromArgb(255, 246, 100, 135);
        private static readonly Color _color2 = Color.FromArgb(255, 241, 99, 133);
        private static readonly Color _color3 = Color.FromArgb(255, 148, 93, 179);
        private static readonly Color _color4 = Color.FromArgb(255, 135, 81, 168);
        private static readonly Color[] colors = new Color[] { _color1, _color2, _color3, _color4 };

        public static LinearGradientBrush GradualChanged(double total_distance, double delta_distance)
        {
            if (total_distance == double.NaN)
                total_distance = 0;
            if (delta_distance == double.NaN)
                delta_distance = 0;
            double start_distance = total_distance; //该段路径的起始点在总距离中的位置
            double end_distance = total_distance + delta_distance; //总距离加上该段路经后的结束点的位置
            double part_distance = _distance / (colors.Length - 1); //将总范围分段，每一段的长度，单位m
            GradientStopCollection list = new GradientStopCollection(); //返回的颜色值集合
            int p = 0; //差值
            while (true)
            {
                if (start_distance > end_distance)
                    start_distance = end_distance;
                if ((start_distance / _distance) % 2 == 1)
                    p = -(colors.Length - 1);
                else
                    p = 0;
                list.Add(new GradientStop { Color = colors[Math.Abs(((int)((start_distance % _distance) / part_distance)) - p)] });
                if (start_distance == end_distance)
                    break;
                start_distance += part_distance;
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list.Count == 1)
                    list[i].Offset = 1;
                else
                    list[i].Offset = (double)i / (double)(list.Count - 1);
            }
            return new LinearGradientBrush(list, 0);
        }
    }
}
