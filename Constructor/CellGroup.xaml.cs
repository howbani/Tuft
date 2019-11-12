using Tuft.Dataplane;
using Tuft.Dataplane.PacketRouter;
using Tuft.Intilization;
using Tuft.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tuft.Models.Cell;



namespace Tuft.Constructor
{
    /// <summary>
    /// Interaction logic for Cluster.xaml
    /// </summary>
    public partial class CellGroup : UserControl
    {

        private double clusterHeight = PublicParameters.cellRadius;
        private double clusterWidth = PublicParameters.cellRadius;
        public List<Sensor> clusterNodes = new List<Sensor>();
        //Location variables for the center and the actual location of cluster
        public Point clusterLocMargin { get; set; }
        public Point clusterCenterComputed { get; set; }
        public Point clusterCenterMargin { get; set; }
        public Point clusterActualCenter { get; set; }
      
        public CellCenter centerOfCluster { get; set; }

        public int clusterDepth { get; set; }

        public int buildClustersunderTop { get; set; }
        public Link clusterLinks = new Link();


        private List<int> neighborClusters = new List<int>();
        List<Sensor> myNetwork = PublicParameters.myNetwork;

        private static int assignID { set; get; }
        public static Point ptrail = new Point();
        public static List<CellGroup> changePosClus = new List<CellGroup>();

        //Tree heirarchry variables
        public CellGroup parentCluster;
        public List<CellGroup> childrenClusters = new List<CellGroup>();
        public bool isLeafNode = false;
        public bool isVisited = false;
        public int clusterLevel { get; set; }
        public Point treeParentNodePos { get; set; }
        public Point treeNodePos { get; set; }
        public int xValue { get; set; }


        public CellTable CellTable = new CellTable();
        //Cluster header
       // public ClusterHeaderTable clusterHeader = new ClusterHeaderTable();

        public CellGroup()
        {

        }
        public CellGroup(Point locatio, int id)
        {
            InitializeComponent();

            this.id = id;
            clusterLocMargin = locatio;


        }

        public void setPositionOnWindow()
        {
            //Setting the height and widths of each cluster and its container
            ell_clust.Height = clusterHeight;
            ell_clust.Width = clusterWidth;
            canv_cluster.Height = clusterHeight;
            canv_cluster.Width = clusterWidth;

            //Giving a margin for each cluster container
            Thickness clusterMargin = canv_cluster.Margin;
            clusterMargin.Top = this.clusterLocMargin.Y;
            clusterMargin.Left = this.clusterLocMargin.X;
            canv_cluster.Margin = clusterMargin;
            //Giving a margin for each cluster center (Margin inside the container)





        }


        private int id { set; get; }

        
        public bool isNotEmpty()
        {
            if (this.getClusterNodes().Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int getID()
        {
            return this.id;
        }

        public List<Sensor> getClusterNodes()
        {
            return clusterNodes;
        }

        public bool isNear(Point p1)
        {
            double offset = PublicParameters.cellRadius / 2;
            Point p2 = new Point(this.clusterLocMargin.X + offset, this.clusterLocMargin.Y + offset);
            double x = Operations.DistanceBetweenTwoPoints(p1, p2);
           
            if (x <= offset)
            {
                
                return true;
            }
            else
            {
                return false;
            }
        }



        //This function find the nearest sensor to the point needed 
        public void findNearestSensor(bool isReCheck)
        {
            double radius = PublicParameters.cellRadius;
            List<Sensor> nearestSen = new List<Sensor>();
            bool clusterDouble = false;
            foreach (Sensor sensor in myNetwork)
            {
                Point p = new Point(sensor.CenterLocation.X, sensor.CenterLocation.Y);
                
                if (sensor.ID != PublicParameters.SinkNode.ID)
                {
                    if (isNear(p))
                    {
                        if (sensor.inCell != -1)
                        {
                            clusterDouble = true;
                            break;
                        }
                        
                        nearestSen.Add(sensor);
                    }
                }
            }
            this.clusterNodes = nearestSen;
            if (nearestSen.Count > 0 && !isReCheck && !clusterDouble)
            {
                
                PublicParameters.networkCells.Add(this);
            }
            else if (isReCheck)
            {
            }
        }

        public void findNearestSensorRecheck(bool isFinal)
        {
            double radius = PublicParameters.cellRadius;
            List<Sensor> nearestSen = new List<Sensor>();
            nearestSen.Clear();
            foreach (Sensor sensor in myNetwork)
            {
                Point p = new Point(sensor.CenterLocation.X, sensor.CenterLocation.Y);
                if (sensor.ID != PublicParameters.SinkNode.ID)
                {
                    if (this.isNear(p))
                    {
                        nearestSen.Add(sensor);
                    }
                }
            }
            this.clusterNodes = nearestSen;
            if (nearestSen.Count > 0 && !isFinal)
            {
                changePosClus.Add(this);

            }
            if (nearestSen.Count > 0 && isFinal)
            {

                for (int i = 0; i < PublicParameters.networkCells.Count(); i++)
                {
                    if (PublicParameters.networkCells[i].getID() == this.getID())
                    {
                        PublicParameters.networkCells.Remove(PublicParameters.networkCells[i]);
                        PublicParameters.networkCells.Add(this);
                    }
                }


            }




        }

        public static void getCenterOfNetwork()
        {
            double sumX = 0;
            double sumY = 0;
            double count = 0;
            foreach (Sensor sensor in PublicParameters.myNetwork)
            {
                sumX += sensor.CenterLocation.X;
                sumY += sensor.CenterLocation.Y;
                count++;
            }
            sumX /= count;
            sumY /= count;
            PublicParameters.networkCenter = new Point(sumX, sumY);
           


        }

        public void getNodesCenter()
        {
            double sensorX = 0;
            double sensorY = 0;
            double halfRad = PublicParameters.cellRadius / 2;
            double clusterX = this.clusterLocMargin.X + halfRad;
            double clusterY = this.clusterLocMargin.Y + halfRad;
            clusterActualCenter = new Point(clusterX, clusterY);


            double sumX = 0;
            double sumY = 0;
            double n = clusterNodes.Count;

            foreach (Sensor sensor in this.clusterNodes)
            {
                sensorX += sensor.CenterLocation.X;
                sensorY += sensor.CenterLocation.Y;

            }


            sumX = (double)clusterX + (sensorX / n);
            sumY = (double)clusterY + (sensorY / n);

            sumX = sumX / 2;
            sumY = sumY / 2;

            //double marginTop = Math.Floor(clusterY - this.clusterLocMargin.Y) - label_clustercenter.Height/2;
            // double marginLeft =Math.Floor( clusterX - this.clusterLocMargin.X) - label_clustercenter.Width/2;


            clusterCenterMargin = new Point(sumX, sumY);
            CellCenter center = new CellCenter(clusterCenterMargin, this.getID());
            this.centerOfCluster = center;

            clusterCenterComputed = new Point(sumX, sumY);


        }

        public static double getAverageSensors()
        {
            double sum = 0;
            double clusterCount = PublicParameters.networkCells.Count();
            foreach (CellGroup cluster in PublicParameters.networkCells)
            {
                sum += cluster.clusterNodes.Count();
            }
            Console.WriteLine("AVG {0}", (sum / clusterCount));
            return Math.Floor(sum / clusterCount);
        }


        public void incDecPos(int direction, double multiply)
        {
            double radius = PublicParameters.cellRadius;
            double distance = (radius + (radius / 2));
            Point moveTo = new Point(this.clusterLocMargin.X, this.clusterLocMargin.Y);
            switch (direction)
            {
                case 1:
                    moveTo.X += distance * multiply;
                    break;
                case 2:
                    moveTo.X -= distance * multiply;
                    break;
                case 3:
                    moveTo.Y -= distance * multiply;
                    break;
                case 4:
                    moveTo.Y += distance * multiply;
                    break;
                case 5:
                    moveTo.X += distance * multiply;
                    moveTo.Y -= distance * multiply;
                    break;
                case 6:
                    moveTo.X -= distance * multiply;
                    moveTo.Y += distance * multiply;
                    break;
                case 7:
                    moveTo.X -= distance * multiply;
                    moveTo.Y -= distance * multiply;
                    break;
                case 8:
                    moveTo.X += distance * multiply;
                    moveTo.Y += distance * multiply;
                    break;
            }
            this.clusterLocMargin = moveTo;

        }

        public CellGroup getMaxSensors()
        {
            List<int> sensorsCount = new List<int>();
            foreach (CellGroup tempClus in changePosClus)
            {
                sensorsCount.Add(tempClus.clusterNodes.Count());

            }
            int maxCount = 0;
            foreach (int i in sensorsCount)
            {
                if (maxCount < i)
                {
                    maxCount = i;
                }
            }
            CellGroup foundMax = new CellGroup();
            foreach (CellGroup tempClus in changePosClus)
            {
                if (tempClus.clusterNodes.Count == maxCount)
                {
                    foundMax = tempClus;
                }
            }
            changePosClus.Clear();
            return foundMax;
        }

        public void checkNearClusters()
        {

        }

        public static CellGroup getClusterWithID(int findID)
        {
            CellGroup getCluster = new CellGroup();


            foreach (CellGroup findCluster in PublicParameters.networkCells)
            {
                if (findCluster.getID() == findID)
                {

                    // Console.WriteLine("Searching in {0}", findCluster.getID());
                    getCluster = findCluster;
                    //  Console.WriteLine("Returning");
                    return findCluster;

                }
            }
            //   Console.WriteLine("Returned");
            // if (!foundCluster)
            //{
            //     throw new System.ArgumentException("Cluster not found", "original");
            //  }
            //  else
            // {
            return getCluster;
            // }




        }

        /// <summary>
        /// Used to set or change the cluster head
        /// </summary>
        /// <param name="isRechange">isReachange = false if it's used to set for the first time</param>
        



    }





}
