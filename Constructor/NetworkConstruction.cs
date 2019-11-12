using Tuft.Dataplane;
using Tuft.Dataplane.NOS;
using Tuft.Dataplane.PacketRouter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tuft.Models.Cell;

namespace Tuft.Constructor
{
    class NetworkConstruction
    {
        public NetworkConstruction(Canvas Canvase_SensingFeild, String method)
        {

            if (method == "center")
            {
                buildFromCenter(Canvase_SensingFeild);


            }
            else if (method == "zero")
            {
                buildFromZeroZero(Canvase_SensingFeild);
                ;
            }
        }
        private int assignID { get; set; }

        private void addClustersToWindow(Canvas Canvas_SensingFeild)
        {
            Console.WriteLine("The count for clusters {0}", PublicParameters.networkCells.Count());
            foreach (CellGroup cluster in PublicParameters.networkCells)
            {
                //   Console.WriteLine("Drawing cluster {0}", cluster.getID());
                cluster.getNodesCenter();
                //ClusterCenter center = new ClusterCenter(cluster.nodesCenter);
                cluster.setPositionOnWindow();
                Canvas_SensingFeild.Children.Add(cluster);
                Canvas_SensingFeild.Children.Add(cluster.centerOfCluster);

                // Canvas_SensingFeild.Children.Add(center);
            }
        }

        private void changeClusterPosition()
        {
            List<CellGroup> clusterList = PublicParameters.networkCells;
            double avg = CellGroup.getAverageSensors();

            for (int x = 0; x < clusterList.Count(); x++)
            {
                CellGroup oldCluster = PublicParameters.networkCells[x];
                Point oldClusterLocation = oldCluster.clusterLocMargin;

                int oldNodesCount = oldCluster.clusterNodes.Count;
                if (oldNodesCount < avg)
                {
                    //Consider changing the cluster
                    //Construct 8 clusters and compare between each of them
                    //Construct the 8 clusters by adding them to a temporary list in ReCheck
                    for (int i = 1; i <= 8; i++)
                    {
                        CellGroup tempCluster = new CellGroup(oldClusterLocation, oldCluster.getID());
                        //Console.WriteLine("Old Position X: {0} Y: {1}", tempCluster.clusterPoint.X, tempCluster.clusterPoint.Y);
                        tempCluster.incDecPos(i, (0.334));
                        // Console.WriteLine("New Position X: {0} Y: {1}", tempCluster.clusterPoint.X, tempCluster.clusterPoint.Y);
                        tempCluster.findNearestSensorRecheck(false);
                    }
                    CellGroup maxCluster = new CellGroup();
                    maxCluster = maxCluster.getMaxSensors();
                    maxCluster.findNearestSensorRecheck(true);
                }
            }
        }

        private static int AfterSmallAverage { get; set; }
        private static void getAfterAverage()
        {
            double avg = Math.Abs(CellGroup.getAverageSensors());
            avg-=2;
            double i = 0;
            double y = 0;
            foreach(CellGroup cell in PublicParameters.networkCells)
            {
                double count = cell.clusterNodes.Count;
                if(count > avg)
                {
                    i += count;
                    y++;
                }
            }
            AfterSmallAverage = Convert.ToInt32(i / y);
            //AfterSmallAverage = Math.Abs(i);

        }

        private void removeSmallCells()
        {
            List<CellGroup> clusterList = PublicParameters.networkCells;
            getAfterAverage();
            double avg =  AfterSmallAverage;// Math.Abs(Cluster.getAverageSensors());
            avg--;
            List<CellGroup> removeThese = new List<CellGroup>();
            for (int x = 0; x < clusterList.Count(); x++)
            {
                CellGroup oldCluster = PublicParameters.networkCells[x];
                int oldNodesCount = oldCluster.clusterNodes.Count;
                if (oldNodesCount < avg)
                {
                    //PublicParameters.networkClusters.Remove(oldCluster);
                    removeThese.Add(oldCluster);
                }
            }
            foreach(CellGroup clus in removeThese)
            {
                PublicParameters.networkCells.Remove(clus);
            }
        }

        //Assign cluster IDs here

        private void addIdsToSensorA(CellGroup cluster)
        {

            foreach (Sensor sensor in cluster.getClusterNodes())
            {
                sensor.inCell = cluster.getID();
                //Console.WriteLine("Sensor {0} is in {1}", sensor.ID, cluster.getID());
            }

        }
        private static void addIdsToSensorFinal()
        {
            foreach (Sensor sen in PublicParameters.myNetwork)
            {
                sen.inCell = -1;

            }
            foreach (CellGroup cluster in PublicParameters.networkCells)
            { 
                foreach (Sensor sensor in cluster.getClusterNodes())
                {

                    sensor.inCell = cluster.getID();
                    sensor.TuftNodeTable.isEncapsulated = true;
                    sensor.TuftNodeTable.myCellHeader = cluster.CellTable.CellHeader;
                    sensor.TuftNodeTable.CellNumber = cluster.getID();

                }
            }
            CellFunctions.FillOutsideSensnors();
        }

        private static void populateClusterTables()
        {
            foreach (CellGroup cluster in PublicParameters.networkCells)
            {

                foreach (Sensor sensor in cluster.getClusterNodes())
                {
                    sensor.inCell = cluster.getID();
                    sensor.TuftNodeTable.isEncapsulated = true;
                    sensor.TuftNodeTable.myCellHeader = cluster.CellTable.CellHeader; ;

                }
            }
            CellFunctions.FillOutsideSensnors();
        }

        private void buildFromZeroZero(Canvas Canvase_SensingFeild)
        {
            double canvasHeight = Canvase_SensingFeild.ActualHeight;
            double canvasWidth = Canvase_SensingFeild.ActualWidth;
            Point startFrom = new Point(0, 0);
            double radius = PublicParameters.cellRadius;
            double offset = (radius + (radius / 2));
            double xAxesCount = Math.Floor(canvasWidth / offset);
            double yAxesCount = Math.Floor(canvasHeight / (radius + (radius / 2)));

            assignID = 1;

            for (int rightCount = 0; rightCount <= xAxesCount; rightCount++)
            {
                buildFromDirection("down", startFrom, yAxesCount);
                startFrom.X += offset;

            }
            changeClusterPosition();
            addIdsToSensorFinal();
            addClustersToWindow(Canvase_SensingFeild); ;
            Tree tree = new Tree(Canvase_SensingFeild);
        }

        private void buildFromCenter(Canvas Canvase_SensingFeild)
        {
            double canvasHeight = Canvase_SensingFeild.ActualHeight;
            double canvasWidth = Canvase_SensingFeild.ActualWidth;
          
            CellGroup.getCenterOfNetwork();
            
            double radius = PublicParameters.cellRadius;
           
            Point networkCenter = PublicParameters.networkCenter;
            networkCenter.X -= radius / 2;
            networkCenter.Y -= radius / 2;
            double xAxesCount = Math.Floor(canvasWidth / (radius + (radius / 2)));
            double yAxesCount = Math.Floor(canvasHeight / (radius + (radius / 2)));
            double pyth = Math.Sqrt((Math.Pow(canvasHeight, 2) + (Math.Pow(canvasWidth, 2))));
            pyth = Math.Floor(pyth / (radius + (radius / 2)));
            assignID = 1;
           
            buildFromDirection("center", networkCenter, 1);
            buildFromDirection("right", networkCenter, xAxesCount / 2);
            buildFromDirection("left", networkCenter, xAxesCount / 2);
            buildFromDirection("up", networkCenter, yAxesCount / 2);
            buildFromDirection("down", networkCenter, yAxesCount / 2);
            buildFromDirection("upright", networkCenter, pyth / 2);
            buildFromDirection("upleft", networkCenter, pyth / 2);
            buildFromDirection("downright", networkCenter, pyth / 2);
            buildFromDirection("downleft", networkCenter, pyth / 2);
            
            buildUnderTop();
           // changeClusterPosition();
            removeSmallCells();

            addIdsToSensorFinal();

            addClustersToWindow(Canvase_SensingFeild);

            Tree tree = new Tree(Canvase_SensingFeild);
            tree.displayTree();

            initAssignHead();
            populateClusterTables();
        }


        private void initAssignHead()
        {
            foreach (CellGroup cluster in PublicParameters.networkCells)
            {
                CellFunctions.assignClusterHead(cluster);
               
            }
        }

        private void buildUnderTop()
        {
            List<CellGroup> buildTopClusters = new List<CellGroup>();

            foreach (CellGroup cluster in PublicParameters.networkCells)
            {
                if (cluster.buildClustersunderTop > 0)
                {
                    buildTopClusters.Add(cluster);
                }
            }
            foreach (CellGroup fromCluster in buildTopClusters)
            {
                buildFromDirection("down", fromCluster.clusterLocMargin, fromCluster.buildClustersunderTop);
                buildFromDirection("up", fromCluster.clusterLocMargin, fromCluster.buildClustersunderTop);
            }

        }

        private void buildFromDirection(String direction, Point startFrom, double count)
        {

            switch (direction)
            {
                case "center":

                    CellGroup cluster = new CellGroup(startFrom, assignID);
                    cluster.findNearestSensor(false);
                    if (cluster.isNotEmpty())
                    {
                        addIdsToSensorA(cluster);
                        assignID++;
                    }

                    break;
                case "right":
                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster0 = new CellGroup(startFrom, assignID);
                        cluster0.incDecPos(1, i + 1);
                        cluster0.findNearestSensor(true);
                        if (cluster0.isNotEmpty())
                        {
                            //ui.MainWindow.net
                            cluster0.findNearestSensor(false);
                            addIdsToSensorA(cluster0);
                            assignID++;

                        }
                    }
                    break;
                case "left":

                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster1 = new CellGroup(startFrom, assignID);
                        cluster1.incDecPos(2, i + 1);
                        cluster1.findNearestSensor(true);
                        if (cluster1.isNotEmpty())
                        {

                            cluster1.findNearestSensor(false);
                            addIdsToSensorA(cluster1);
                            assignID++;
                        }
                    }
                    break;
                case "up":

                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster2 = new CellGroup(startFrom, assignID);
                        cluster2.incDecPos(3, i + 1);
                        cluster2.findNearestSensor(true);
                        if (cluster2.isNotEmpty())
                        {

                            cluster2.findNearestSensor(false);
                            addIdsToSensorA(cluster2);
                            assignID++;

                        }
                    }
                    break;
                case "down":

                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster3 = new CellGroup(startFrom, assignID);
                        cluster3.incDecPos(4, i + 1);
                        cluster3.findNearestSensor(true);
                        if (cluster3.isNotEmpty())
                        {

                            cluster3.findNearestSensor(false);
                            addIdsToSensorA(cluster3);
                            assignID++;

                        }
                    }
                    break;
                case "upright":

                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster4 = new CellGroup(startFrom, assignID);
                        cluster4.incDecPos(5, i + 1);
                        cluster4.findNearestSensor(true);
                        if (cluster4.isNotEmpty())
                        {
                            cluster4.buildClustersunderTop = i + 1;
                            cluster4.findNearestSensor(false);
                            addIdsToSensorA(cluster4);
                            assignID++;


                        }
                    }
                    break;
                case "downleft":

                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster5 = new CellGroup(startFrom, assignID);
                        cluster5.incDecPos(6, i + 1);
                        cluster5.findNearestSensor(true);
                        if (cluster5.isNotEmpty())
                        {
                            cluster5.buildClustersunderTop = i + 1;
                            cluster5.findNearestSensor(false);
                            addIdsToSensorA(cluster5);
                            assignID++;

                        }

                    }

                    break;
                case "upleft":

                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster6 = new CellGroup(startFrom, assignID);
                        cluster6.incDecPos(7, i + 1);
                        cluster6.findNearestSensor(true);
                        if (cluster6.isNotEmpty())
                        {
                            cluster6.buildClustersunderTop = i + 1;
                            cluster6.findNearestSensor(false);
                            addIdsToSensorA(cluster6);
                            assignID++;

                        }
                    }
                    break;
                case "downright":

                    for (int i = 0; i < count; i++)
                    {
                        CellGroup cluster7 = new CellGroup(startFrom, assignID);
                        cluster7.incDecPos(8, i + 1);
                        cluster7.findNearestSensor(true);
                        if (cluster7.isNotEmpty())
                        {
                            cluster7.buildClustersunderTop = i + 1;
                            cluster7.findNearestSensor(false);
                            addIdsToSensorA(cluster7);
                            assignID++;

                        }

                    }
                    break;

            }
        }


        public static void sendTrial(int count)
        {           
         
        }
    }
}
