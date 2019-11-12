using Tuft.Dataplane;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Tuft.Intilization
{
    public class Operations
    {
        public static int ConvertAngleToDirection(int angle) {
            int part = getAnglePart(angle);
            int direction = convertToDirection(getNearestAngle(angle, part));
            return direction;
        }
        private static int getAnglePart(int angle)
        {
            int part = 0;
            if (angle <= 90 && angle >= 0)
            {
                //1
                part = 1;
            }
            else if (angle > 90 && angle <= 180)
            {
                //2
                part = 2;
            }
            else if (angle > 180 && angle <= 270)
            {
                //3
                part = 3;
            }
            else if (angle > 270 && angle <= 360)
            {
                //4
                part = 4;
            }

            return part;

        }
        private static int getNearestAngle(double angle, int part)
        {
            int direction = 0;
            int to = 90 * part;
            int from = to - 90;
            int middle = (to + from) / 2;
            int middleUp = (middle + to) / 2;
            int middleDown = (middle + from) / 2;

            if (angle > middle)
            {
                //between to and middle 
                if (angle > middleUp)
                {
                    //Return to
                    direction = to;
                }
                else
                {
                    //return middle 
                    direction = middle;
                }
            }
            else
            {
                if (angle > middleDown)
                {
                    //return middle 
                    direction = middle;
                }
                else
                {
                    direction = from;
                }
            }
            return direction;
        }
        private static int convertToDirection(int dir)
        {
            switch (dir)
            {
                case 0:
                    return 1;

                case 360:
                    return 1;
                case 180:
                    return 2;
                case 90:
                    return 3;
                case 270:
                    return 4;
                case 45:
                    return 5;
                case 225:
                    return 6;
                case 135:
                    return 7;
                case 315:
                    return 8;
                default:
                    return 0;

            }
       

        }

        public static List<int> PacketPathToIDS(String path)
        {
            String[] strIDS = path.Split('>');
            List<int> ids = new List<int>();

            foreach (String id in strIDS)
            {
                int x = Int16.Parse(id);
                ids.Add(x);
            }
            return ids;

        }
        public static double kmphToTimerInterval(double speed)
        {
            if (speed <= 0)
            {
                return 0;
            }
            double disInMeter = speed * 1000;
            double timeInSec = 3600;
            double interval = timeInSec / disInMeter;
            return interval;

        }
        public static double factorial(double n)
        {
            double results = 1;
            while (n != 1)
            {
                results *= n;
                n -= 1;
            }
            return results;
        }
        public static double factorial(double n, double r, double dif)
        {
            double untill = 0;
            double divide = 0;
            if (r > dif)
            {
                untill = (n - r);
                divide = dif;
            }
            else
            {

                untill = dif;
                divide = r;
            }
            double result = 1;
            divide = factorial(divide);
            while (n != untill)
            {
                result = result * n;
                n = n - 1;
            }
            return result / divide;
        }

        public static double nChooseR(double n, double r)
        {
            if (n == r)
            {
                return 1;
            }
            double dif = n - r;
            double solution = factorial(n, r, dif);

            return solution;


        }


        public static double DistanceBetweenTwoSensors(Sensor sensor1, Sensor sensor2)
        {
            try
            {
                double dx = (sensor1.CenterLocation.X - sensor2.CenterLocation.X);
                dx *= dx;
                double dy = (sensor1.CenterLocation.Y - sensor2.CenterLocation.Y);
                dy *= dy;
                return Math.Sqrt(dx + dy);
            }
            catch (Exception e)
            {
                Console.WriteLine("Distance between sensors returned an exception:");
                Console.WriteLine(e.Message);
                return 3 * PublicParameters.cellRadius;
            }

        }

        public static double DistanceBetweenTwoPoints(Point p1, Point p2)
        {
            double dx = (p1.X - p2.X);
            dx *= dx;
            double dy = (p1.Y - p2.Y);
            dy *= dy;
            return Math.Sqrt(dx + dy);
        }

        /// <summary>
        /// the communication range is overlapped.
        /// 
        /// </summary>
        /// <param name="sensor1"></param>
        /// <param name="sensor2"></param>
        /// <returns></returns>
        public static bool isOverlapped(Sensor sensor1, Sensor sensor2)
        {
            bool re = false;
            double disttance = DistanceBetweenTwoSensors(sensor1, sensor2);
            if (disttance < (sensor1.ComunicationRangeRadius + sensor2.ComunicationRangeRadius))
            {
                re = true;
            }
            return re;
        }

        /// <summary>
        /// check if j is within the range of i.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static bool isInMySensingRange(Sensor i, Sensor j)
        {
            bool re = false;
            double disttance = DistanceBetweenTwoSensors(i, j);
            if (disttance <= (i.VisualizedRadius))
            {
                re = true;
            }
            return re;
        }

        /// <summary>
        /// Returns the perpendicular distance between a line (between two points) and a point
        /// 
        /// </summary>
        /// <param name="src">Source Node Center Location</param>
        /// <param name="dest">Destination Node Center Location</param>
        /// <param name="candi">Candidate Node (Outside Point)</param>
        /// <returns>The value of the distance</returns>

        public static double GetPerpindicularDistance(Point psource, Point pj, Point pdestination)
        {
            if (psource == pdestination)
            {
                return 1;
            }
            double past = Math.Abs(((pdestination.Y - psource.Y) * pj.X) - ((pdestination.X - psource.X) * pj.Y) + (pdestination.X * psource.Y) - (pdestination.Y * psource.X));
            double sbDis = DistanceBetweenTwoPoints(psource, pdestination);
            double perDis = past / sbDis;
            return perDis;
        }

        public static double GetDirectionAngle(Point i, Point j, Point d)
        {
            /* double angle = 0;
             double srcForwarder = DistanceBetweenTwoPoints(source, forwarder);
             double srcDest = DistanceBetweenTwoPoints(source, destination);
             double forwarderDest = DistanceBetweenTwoPoints(destination, forwarder);
             double sum = (srcDest * srcDest) + (srcForwarder * srcForwarder)-(forwarderDest * forwarderDest);
             sum /= (2 * srcDest * srcForwarder);
             angle = Math.Acos(sum);
             if (srcDest == 0 || srcForwarder == 0)
             {
                 Console.WriteLine();
             }
             return angle;
             * */
            
            double axb = (j.X - i.X) * (d.X - i.X) + (j.Y - i.Y) * (d.Y - i.Y);
            double disMul = DistanceBetweenTwoPoints(i, d) * DistanceBetweenTwoPoints(i, j);
            double angale = Math.Acos(axb / disMul);
            double norAngle = angale / Math.PI;
            return norAngle;
        }



        public static double GetAngleDotProduction(Point i, Point j, Point d)
        {
            double axb = (j.X - i.X) * (d.X - i.X) + (j.Y - i.Y) * (d.Y - i.Y);
            double disMul = DistanceBetweenTwoPoints(i, d) * DistanceBetweenTwoPoints(i, j);
            double div;
            //Round div to the nearest 4 Math.Round(div,4);
            if (j == d || i==j ||i==d)
            {
                div = 1;
            }
            else
            {
                div = axb / disMul;
                div = Math.Round(div, 4);
            }
            double angale = Math.Acos(div);
            if (Double.IsNaN(angale))
            {
                Console.WriteLine();
            }
            double norAngle = angale / Math.PI;
            if (norAngle <= 0.5)
                return (Math.Pow(((1 - (norAngle * Math.Exp(norAngle))) / (1 + (norAngle * Math.Exp(norAngle)))), 1)); // heigher pri
            else
                return (Math.Pow(((1 - (norAngle * Math.Exp(norAngle))) / (1 + (norAngle * Math.Exp(norAngle)))), 3)); // smaller pri
        }


        public static double GetPerpendicularProbability(Point psource, Point pj, Point pdestination)
        {
            double past = Math.Abs(((pdestination.Y - psource.Y) * pj.X) - ((pdestination.X - psource.X) * pj.Y) + (pdestination.X * psource.Y) - (pdestination.Y * psource.X));
            double sbDis = DistanceBetweenTwoPoints(psource, pdestination);
            double perDis = past / sbDis;
            // dist: if there is a mistake, then we should consider the normalization.
           // double perNorm = 1 + (perDis / PublicParameters.CommunicationRangeRadius + perDis);
            double perNorm = perDis / PublicParameters.CommunicationRangeRadius;
            double pr = Math.Exp(-perNorm);
            return pr;
        }

        public static double GetResidualEnergyProbability(double ResidualEnergyPer)
        {
            double norm = 1+ (ResidualEnergyPer/0.5);
            double prob = Math.Exp(norm);
            if(Double.IsInfinity(prob)){
                Console.WriteLine();
            }
            return prob;
        }
        public static double GetTransmissionDistanceProbability(Point pj, Point pdestination)
        {
            double distance = DistanceBetweenTwoPoints(pj, pdestination);
            double norm = 1 + (distance / PublicParameters.CommunicationRangeRadius);
            double prob = Math.Exp(-1 * norm);
            return prob;
        }


        /// <summary>
        /// commnication=sensing rang*2
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static bool isInMyComunicationRange(Sensor i, Sensor j)
        {
            bool re = false;
            double disttance = DistanceBetweenTwoSensors(i, j);
            if (disttance <= (i.ComunicationRangeRadius))
            {
                re = true;
            }
            return re;
        }

        public static double FindNodeArea(double com_raduos)
        {
            return Math.PI * Math.Pow(com_raduos, 2);
        }

        /// <summary>
        /// n!
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double Factorial(int n)
        {
            long i, fact;
            fact = n;
            for (i = n - 1; i >= 1; i--)
            {
                fact = fact * i;
            }
            return fact;
        }

        /// <summary>
        /// combination 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double Combination(int n, int k)
        {
            if (k == 0 || n == k) return 1;
            if (k == 1) return n;
            int dif = n - k;
            int max = Max(dif, k);
            int min = Min(dif, k);

            long i, bast;
            bast = n;
            for (i = n - 1; i > max; i--)
            {
                bast = bast * i;
            }
            double mack = Factorial(min);
            double x = bast / mack;
            return x;
        }


        private static int Max(int n1,int n2) { if (n1 > n2) return n1; else return n2; }
        private static int Min(int n1, int n2) { if (n1 < n2) return n1; else return n2; } 
    }
}
