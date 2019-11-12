using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Tuft.Intilization
{

    public class RandomColorsGenerator
    {
        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeColorBrightness(Color color, double  correctionFactor)
        {
            double red = (float)color.R;
            double green = (float)color.G;
            double blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        public static List<Color> RandomColor(int numberofcolors)
        {
            
            List<Color> list = new List<Color>(numberofcolors);
            for (int i = 1; i <= numberofcolors; i++)
            {
                Random rnd = new Random();
                double a = (rnd.Next(1000) * DateTime.Now.Ticks * DateTime.Now.Millisecond * i) % 255;
                if (a < 0) a *= -1;
                byte abyte = Convert.ToByte(a);

                double r = (rnd.Next(2000)* DateTime.Now.Ticks * DateTime.Now.Millisecond * i) % 255;
                if (r < 0) r *= -1;
                byte rbyte = Convert.ToByte(r);

                double g = (rnd.Next(3000) * abyte * rbyte * DateTime.Now.Ticks * DateTime.Now.Millisecond * i) % 255;
                if (g < 0) g *= -1;
                byte gbyte = Convert.ToByte(g);

                double b = (rnd.Next(4000) * abyte * rbyte * DateTime.Now.Ticks * DateTime.Now.Millisecond * i) % 255;
                if (b < 0) b *= -1;
                byte bbyte = Convert.ToByte(b);
                Color randomColor = Color.FromArgb(abyte, rbyte, gbyte, bbyte);
                Color xr = ChangeColorBrightness(randomColor, -0.7);
                list.Add(xr);
            }
            return list;
        } 
    }

    public class BatteryLevelColoring
    {
        public static string
               col90_100 = "#FF61F01F",
               col80_89 = "#FF1FF0D3",
               col70_79 = "#FF741FF0",
               col60_69 = "#FF1F9AF0",
               col50_59 = "#FFFF0197",
               col40_49 = "#FFB67DCB",
               col30_39 = "#FFF0E50E",
               col20_29 = "#FFF0A80E",
               col10_19 = "#FFF0740E",
               col1_9 = "#FFF02C0E",
               col0 = "#FF1D1C1C";
    }
     
    /// <summary>
    /// colors 
    /// </summary>
    public class NodeStateColoring
    {
        public static SolidColorBrush ActiveColor = Brushes.Coral;
        public static SolidColorBrush SleepColor = Brushes.SeaGreen;
        public static SolidColorBrush IntializeColor = Brushes.LightBlue; 
    }

   
}
