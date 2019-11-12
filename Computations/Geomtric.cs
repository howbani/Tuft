using System.Windows;
namespace Tuft.Intilization
{

   

    /// <summary>
    /// 
    /// </summary>
    public class Triangle
    {
        public Point A { get; set; }
        public Point B { get; set; }
        public Point C { get; set; }  
    }

    public class Parallelogram
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; }
        public Point P3 { get; set; }
        public Point P4 { get; set; } 
    }
    public class Vector
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; } 
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class Geomtric
    {
        /// <summary>
        /// This method is called walking-around-the-edge. Pretend you are walking around the edges of the triangle. Let's call our 3 corners A, B, and C. Let's call the point you're trying to figure out if it's inside or outside of the triangle P.
        /// Start at A.
        //  Walk to B.
        //  Then Walk to C.
        //  Then walk back to A.
        //   If P was to your left the entire time, then it must have been inside the triangle and you must have been walking counter-clockwise.
        //   If P was to your right the entire time, then it also must have been inside the triangle and you must have been walking clockwise.
        //  If P switched from your left to right or vice versa, then it couldn't have been inside the triangle.
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool PointTestInTriangle(Triangle T, Point P) 
        {
            //AP × AB
            Vector AP = new Intilization.Vector() { P1= T.A, P2= P };
            Vector AB = new Intilization.Vector() { P1 = T.A, P2 = T.B };
            double APAB = CrossProduct(AP, AB);
            // BP × BC
            Vector BP = new Intilization.Vector() { P1 = T.B, P2 = P };
            Vector BC = new Intilization.Vector() { P1 = T.B, P2 = T.C };
            double BPBC = CrossProduct(BP, BC);
            //CP × CA
            Vector CP = new Intilization.Vector() { P1 = T.C, P2 = P };
            Vector CA = new Intilization.Vector() { P1 = T.C, P2 = T.A };
            double CPCA = CrossProduct(CP, CA);

            if (APAB >= 0 && BPBC >= 0 && CPCA >= 0) return true;
            else if (APAB < 0 && BPBC < 0 && CPCA < 0) return true;
            else return false;
        }





        public bool PointTestParallelogram(Parallelogram Para, Point P)
        {
            Triangle T1 = new Intilization.Triangle() { A = Para.P1, B = Para.P2, C = Para.P3 };
            Triangle T2 = new Intilization.Triangle() { A = Para.P2, B = Para.P3, C = Para.P4 };

            if (PointTestInTriangle(T1, P)) return true;
            else if (PointTestInTriangle(T2, P)) return true;
            else return false;
        }

        /// <summary>
        /// Vector QR = (rx - qx), (ry - qy)
        /// Vector ST = (tx - sx), (ty - sy)
        /// QR × ST = (rx - qx) * (ty - sy) - (tx - sx) * (ry - qy)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public double CrossProduct(Vector V1, Vector V2)
        {
            return ((V1.P1.X - V1.P2.X) * (V2.P1.Y - V2.P2.Y)) - ((V2.P1.X - V2.P2.X) * (V1.P1.Y - V1.P2.Y));
        }

       


    
    }
}
