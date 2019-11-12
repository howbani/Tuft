using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Tuft.Dataplane;

namespace Tuft.Coverage
{

    public class GridCoverage
    {
        public GridCoverage() { }
        public  void GridCoverage1(Canvas canvas, List<Sensor> SensorsList, int _2xr)
        {
            int w = Convert.ToInt16(canvas.Width);
            int h = Convert.ToInt16(canvas.Height);
            int count = SensorsList.Count;
            int s = 0;
            for (int x = 1; x < w - (_2xr / 2); x += _2xr)
            {
                for (int y = 1; y < h - (_2xr / 2); y += _2xr)
                {
                    if (s < count)
                    {
                        Point p = new Point(x, y); 
                        SensorsList[s].Position = p;
                        s++;
                    }
                    else break;
                }
            }

        }

        /// <summary>
        /// 2r*0.7
        /// the intersection is 0.3
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="SensorsList"></param>
        /// <param name="_2xr"></param>
        public void GridCoverage2(Canvas canvas, List<Sensor> SensorsList, int _2xr)
        {
            // 
            // double 
            int w = Convert.ToInt16(canvas.Width);
            int h = Convert.ToInt16(canvas.Height);
            int count = SensorsList.Count;
            int s = 0;

            // step 1:
            for (int x = 1; x < w - (_2xr / 2); x += _2xr)
            {
                for (int y = 1; y < h - (_2xr / 2); y += _2xr)
                {
                    if (s < count)
                    {
                        Point p = new Point(x, y);
                        SensorsList[s].Position = p;
                        s++;
                    }
                    else
                        break;
                }
            }

            //step 2:

            for (int xx = _2xr; xx < w - (_2xr / 2); xx += _2xr)
            {
                for (int yy = _2xr; yy < h - (_2xr / 2); yy += _2xr)
                {
                    if (s < count)
                    {
                        Point p = new Point(xx - _2xr / 2, yy - _2xr / 2);
                        SensorsList[s].Position = p;
                        s++;
                    }
                    else
                        break;
                }
            }

        }

    } // class
} // namespace
