using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Tuft.Dataplane;

namespace Tuft.Coverage
{
    public class ZizageCoverage
    {
       public ZizageCoverage() { }

         public void  coverage(Canvas canvas, List<Sensor> SensorsList,int _2xr)  
         {
             double ZigzagLineLength = _2xr * 0.751;// vertical. top re //_2xr*0.8
             double DistanceBtweenZigzagsCorners = (_2xr / 2) * 0.881; // horizontal left re.//(_2xr/2)*0.8;
             double worksideWidth = canvas.Width;
             double worksideHeight = canvas.Height;
             int Nodescount = SensorsList.Count;
             int currentNode = 0;
             int Patterns = Convert.ToInt16(worksideHeight / ZigzagLineLength/2);  //2 line levels.
             int corners = Convert.ToInt16(worksideWidth / DistanceBtweenZigzagsCorners);

             for (int pattern = 0; pattern <Patterns; pattern++)
             {
                 for (int corner = 0; corner <corners; corner++)
                 {
                     double x = corner * DistanceBtweenZigzagsCorners;
                     int level = (2 * pattern) + 1;
                     if (currentNode < Nodescount)
                     {
                         if (corner % 2 != 0)// first level of zaigza
                         {
                             double y = ZigzagLineLength * (level - 1);
                             Point p = new Point(x, y);
                             SensorsList[currentNode].Position = p;
                             currentNode++;
                         }
                         else // second level of zigzag
                         {
                             double y = ZigzagLineLength * (level);
                             Point p = new Point(x, y);
                             SensorsList[currentNode].Position = p;
                             currentNode++;
                         }
                     }
                     else break;
                 }
             }
         }


        /// <summary>
        /// ZigzagLineLengthRatio must be : 0.1~ 0.99
        /// DistanceBtweenZigzagsCornersRaio must be: 0.1~ 0.99
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="SensorsList"></param>
        /// <param name="_2xr"></param>
        /// <param name="C"></param>
        /// <param name="V"></param>
         public void coverage(Canvas canvas, List<Sensor> SensorsList, int _2xr, double C, double V)  
         {
             double Li = _2xr * C;// vertical. top re //_2xr*0.8
             double D = (_2xr / 2) * V; // horizontal left re.//(_2xr/2)*0.8;
             double W = canvas.Width;
             double H = canvas.Height;
             int Nodescount = SensorsList.Count;
             int currentNode = 0;
             int Zigzags = Convert.ToInt16(H / Li/2); 
             int Corners = Convert.ToInt16(W / D*0.9);

             for (int zigzag = 0; zigzag <Zigzags; zigzag++)
             {
                 for (int corner = 0; corner <Corners; corner++)
                 {
                     double x = corner * D;// horizontal
                     int level = (2 * zigzag) + 1;
                     if (currentNode < Nodescount)
                     {
                         if (corner % 2 != 0)// first level of zaigza
                         {
                             double y = Li * (level - 1);
                             Point p = new Point(x, y);
                             SensorsList[currentNode].Position = p;
                             currentNode++;
                         }
                         else // second level of zigzag
                         {
                             double y = Li * (level);
                             Point p = new Point(x, y);
                             SensorsList[currentNode].Position = p;
                             currentNode++;
                         }
                     }
                     else break;
                 }
             }
         }


    }
}
