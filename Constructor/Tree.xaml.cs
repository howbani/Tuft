using Tuft.Dataplane;
using Tuft.Dataplane.PacketRouter;
using Tuft.Intilization;
using Tuft.Models.MobileSink;
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
using System.Windows.Threading;
using Tuft.Models.Cell;

namespace Tuft.Constructor
{
    /// <summary>
    /// Interaction logic for Tree.xaml
    /// </summary>
    /// 
    public partial class Level
    {
        public List<CellGroup> nodes = new List<CellGroup>();
        public int nodesCount { get; set; }
        public int levelID { get; set; }
        Point currentPosition { get; set; }

    }
    public partial class Link
    {
        public List<CellGroup> hasLinkwith = new List<CellGroup>();

    }

    public partial class Tree : UserControl
    {
        public Tree()
        {

        }
        public Tree(Canvas canv)
        {
            InitializeComponent();
            sensingField = canv;
            buildTree();

        }


        private static CellGroup rootCluster { get; set; }
        private List<CellGroup> clusterGroup = PublicParameters.networkCells;
        private static Queue<CellGroup> BFSvisited = new Queue<CellGroup>();
        private static Canvas sensingField;
        private static List<CellGroup> tempClusterGroup = new List<CellGroup>();
        //Variables that will contain manipulating the tree
        public List<CellGroup> clusterTree = new List<CellGroup>();
        public static int rootClusterID { get; set; }

        public static Canvas stack_panel = new Canvas();
        //Showing the tree variables
        private static List<Level> clusterLevels = new List<Level>();
        private Point nodePosition { get; set; }
        private static List<Tree> treeNodes = new List<Tree>();
        private static Queue<CellGroup> nodesQueue = new Queue<CellGroup>();

        private int nodeID { get; set; }

        public static int treeDepth { get; set; }

        private static DispatcherTimer timer = new DispatcherTimer();

        public Tree(Point pos, int id)
        {
            InitializeComponent();
            nodePosition = pos;
            nodeID = id;
        }

        public static int getRootClusterID()
        {
            return rootClusterID;
        }


        public void setLevel(CellGroup node)
        {
            if (node.childrenClusters.Count > 0)
            {
                foreach (CellGroup child in node.childrenClusters)
                {
                    child.clusterLevel = node.clusterLevel + 1;
                    setLevel(child);
                }
            }



        }
        private void saveLevels()
        {
            for (int i = 1; i <= PublicParameters.networkCells.Count(); i++)
            {
                Level level = new Level();
                level.levelID = i;
                foreach (CellGroup node in PublicParameters.networkCells)
                {
                    if (node.clusterLevel == level.levelID)
                    {
                        level.nodes.Add(node);
                    }

                }
                level.nodesCount = level.nodes.Count();
                if (level.nodesCount > 0)
                {
                    clusterLevels.Add(level);
                }

            }



        }

        public static void printLevels()
        {
            Console.WriteLine("Printiing");
            foreach (Level level in clusterLevels)
            {
                Console.WriteLine("** Level {0} has {1}", level.levelID, level.nodesCount);
                foreach (CellGroup node in level.nodes)
                {
                    Console.Write("   {0}", node.getID());
                    if (node.getID() != rootCluster.getID())
                    {
                        Console.Write("  with parent {0}", node.parentCluster.getID());
                    }
                }
                Console.WriteLine();
            }
        }
 
       
        public void displayTree()
        {
            CellGroup root = CellGroup.getClusterWithID(rootClusterID);
        
            root.clusterLevel = 1;
            setLevel(root);
            saveLevels();
            //printLevels();
           

        }


        /*
         *  1- Start from cluster center or randomly select a number with the median as the mean
         *  2- Get the children and add their parents var, then add them to the cluster root's children list
         *  3- add the cluster to the tempClusterGroup 
         *  4- add the rootCluster to the visited list
         *  5- take variables from the visited list and search the children variable 
         *  6- if the clusters have children add them to BFS visited 
         *  7- else set them as leafNode untill BFSVisited is empty
         */


        private void startFromCluster(CellGroup parent, bool isRoot)
        {

            double radius = PublicParameters.cellRadius;
            parent.isVisited = true;
            double offset = Math.Ceiling((radius + (radius / 2)));
            offset = Math.Sqrt(Math.Pow(offset, 2) + Math.Pow(offset, 2));
            if (isRoot)
            {
                parent.parentCluster = null;
                rootCluster = parent;
                rootClusterID = parent.getID();
                MobileModel.rootTreeID = rootClusterID;

            }

            PublicParameters.currentNetworkTree.Add(parent);

            foreach (CellGroup child in PublicParameters.networkCells)
            {

                if (!child.isVisited)
                {

                    double distance = Operations.DistanceBetweenTwoPoints(parent.clusterActualCenter, child.clusterActualCenter);
                    // Console.WriteLine("Distance between {0} and {1} is {2}",  parent.getID(), child.getID(),distance);
                    //Think about the lines where x difference might be 0 if no change and less than half the radius if it changed its place
                    //Also for Y
                    if (distance <= offset)
                    {

                        //That means this cluster is a child so we add it and edit it's parent variable
                        child.parentCluster = parent;
                        child.isVisited = true;
                        parent.childrenClusters.Add(child);


                    }
                }

            }
            if (parent.childrenClusters.Count() > 0)
            {
                parent.isLeafNode = false;
            }
            else
            {
                parent.isLeafNode = true;
            }

            BFSvisited.Enqueue(parent);
        }

        private void buildTree()
        {
            getClusterLinks();

            int clusterCount = PublicParameters.networkCells.Count();

            startFromCluster(CellGroup.getClusterWithID(1), true);
            CellGroup.getClusterWithID(1).CellTable.isRootCell = true;
            for (int i = 0; (BFSvisited.Count == 0) || (i < clusterCount - 1); i++)
            {
                if (BFSvisited.Count() > 0)
                {
                    CellGroup parent = BFSvisited.Dequeue();
                    Queue<CellGroup> childrenQue = new Queue<CellGroup>();// parent.childrenClusters;
                    foreach (CellGroup child in parent.childrenClusters)
                    {
                        childrenQue.Enqueue(child);
                    }
                    int childrenCount = childrenQue.Count();
                    for (int q = 0; q < childrenCount; q++)
                    {
                        startFromCluster(childrenQue.Dequeue(), false);
                    }


                }
                else
                {
                    break;
                }

            }


        }

        private void reBuildTree(int rootID)
        {
            int clusterCount = PublicParameters.networkCells.Count();

            startFromCluster(CellGroup.getClusterWithID(rootID), true);
            for (int i = 0; (BFSvisited.Count == 0) || (i < clusterCount - 1); i++)
            {
                if (BFSvisited.Count() > 0)
                {
                    CellGroup parent = BFSvisited.Dequeue();
                    Queue<CellGroup> childrenQue = new Queue<CellGroup>();// parent.childrenClusters;
                    foreach (CellGroup child in parent.childrenClusters)
                    {
                        childrenQue.Enqueue(child);
                    }
                    int childrenCount = childrenQue.Count();
                    for (int q = 0; q < childrenCount; q++)
                    {
                        startFromCluster(childrenQue.Dequeue(), false);
                    }


                }
                else
                {
                    break;
                }

            }




        }

        /* private static void drawLines()
         {

             int i = 0;
             Queue<Cluster> visited = new Queue<Cluster>();
             Point start;
             Point finish;

             start = rootCluster.clusterCenterComputed;
             foreach (Cluster child in rootCluster.childrenClusters)
             {
                 finish = child.clusterCenterComputed;
                 Line connection = new Line();
                 connection.Stroke = Brushes.Black;
                 connection.Fill = Brushes.Black;

                 connection.X1 = start.X;
                 connection.Y1 = start.Y;
                 connection.X2 = finish.X;
                 connection.Y2 = finish.Y;
                 sensingField.Children.Add(connection);
                 visited.Enqueue(child);

             }

             do
             {
                 i++;
                 Cluster parent = visited.Dequeue();
                 start = parent.clusterCenterComputed;
                 foreach (Cluster child in parent.childrenClusters)
                 {
                     finish = child.clusterCenterComputed;
                     Line connection = new Line();
                     connection.Stroke = Brushes.Black;
                     connection.Fill = Brushes.Black;

                     connection.X1 = start.X;
                     connection.Y1 = start.Y;
                     connection.X2 = finish.X;
                     connection.Y2 = finish.Y;
                     sensingField.Children.Add(connection);
                     visited.Enqueue(child);
                 }
             } while (visited.Count > 0);



         }

         private static void updateTree()
         {



         }

         public static void showTree()
         {
             Queue<Cluster> visitedNodes = new Queue<Cluster>();
             visitedNodes.Enqueue(Cluster.getClusterWithID(rootClusterID));
             Queue<Cluster> childNodes = new Queue<Cluster>();
             int level = 1;
             Cluster node;
             int previousParent = 1;
             for (int i = 0; i < PublicParamerters.networkClusters.Count(); i++)
             {
                 if (visitedNodes.Count == 0)
                 {
                     break;
                 }
                 else
                 {
                  
                     node = visitedNodes.Dequeue();
                     Tree tree = new Tree(node.getID(), level, node.childrenClusters.Count());
                     foreach (Cluster child in node.childrenClusters)
                     {
                             childNodes.Enqueue(child);
                     }
                    
                     if (previousParent != node.parentCluster.getID())
                     {
                         level++;
                     }
                    
                     if (childNodes.Count > 0)
                     {
                         visitedNodes.Enqueue(childNodes.Dequeue());
                     }
                     else
                     {
                         break;
                     }
                     if (previousParent != node.parentCluster.getID())
                     {
                         previousParent = node.getID();
                     }
                    
                        
                    


                 }
             }
            

         }
        
         }
         */

        private void drawLines()
        {
            foreach (Level level in clusterLevels)
            {
                if (level.nodesCount > 0)
                {
                    foreach (CellGroup node in level.nodes)
                    {
                        if (node.getID() != rootClusterID)
                        {
                            CellGroup parent = node.parentCluster;
                            Point parentPos = getNode(parent).treeNodePos;
                            node.treeParentNodePos = parentPos;
                        }


                    }
                }

            }
            foreach (Level level in clusterLevels)
            {
                if (level.nodesCount > 0)
                {
                    foreach (CellGroup node in level.nodes)
                    {
                        if (node.getID() != rootClusterID)
                        {

                            Point parentPos = node.treeParentNodePos;
                            Point nodePos = node.treeNodePos;
                            parentPos.X += 25 / 2;
                            parentPos.Y += 25;
                            nodePos.X += 25 / 2;

                            Line connection = new Line();
                            connection.Stroke = Brushes.Black;
                            connection.Fill = Brushes.Black;
                            connection.X1 = parentPos.X;
                            connection.Y1 = parentPos.Y;
                            connection.X2 = nodePos.X;
                            connection.Y2 = nodePos.Y;
                            stack_panel.Children.Add(connection);

                        }


                    }
                }




            }

        }

        private void getClusterLinks()
        {

            double radius = PublicParameters.cellRadius;
            double offset = Math.Ceiling((radius + (radius / 2)));
            offset = Math.Sqrt(Math.Pow(offset, 2) + Math.Pow(offset, 2));

            foreach (CellGroup node in PublicParameters.networkCells)
            {
                foreach (CellGroup otherNode in PublicParameters.networkCells)
                {
                    if (node.getID() != otherNode.getID())
                    {
                        double distance = Operations.DistanceBetweenTwoPoints(node.clusterActualCenter, otherNode.clusterActualCenter);
                        if (distance <= offset)
                        {
                            node.clusterLinks.hasLinkwith.Add(otherNode);

                        }
                    }
                }
            }
        }

        public static void keepChanging(int nearest)
        {
            Tree tree = new Tree();
            changeTree(nearest);
            clusterLevels.Clear();
            treeNodes.Clear();
          //  stack_panel.Children.RemoveRange(0, stack_panel.Children.Count);
            //tree.displayTree();
        }

        public void startChanging(Canvas stackPanel)
        {
            stack_panel = stackPanel;

            // timer.Tick += timer_tick_change;
            //  keepChanging(nearest);

        }

        private static CellGroup getNode(CellGroup node)
        {
            CellGroup found = null;
            foreach (Level level in clusterLevels)
            {
                if (level.nodesCount > 0)
                {
                    foreach (CellGroup compareNode in level.nodes)
                    {
                        if (compareNode.getID() == node.getID())
                        {
                            found = compareNode;
                        }
                    }
                }
                else if (level.nodes[0].getID() == node.getID())
                {
                    found = level.nodes[0];
                }
            }
            return found;
        }

        public static void changeTree(int nearClusterID)
        {
            if (nearClusterID != rootClusterID)
            {
                // PublicParamerters.currentNetworkTree.clusterTree.Clear();
                // The near cluster will be come the new root 
                CellGroup oldRoot = CellGroup.getClusterWithID(rootClusterID);
                CellGroup newRoot = CellGroup.getClusterWithID(nearClusterID);
               // oldRoot.clusterHeader.headerSensor.ClusterHeader.SinkAgent = null;
                //Edit the old root cluster's children & parent
                oldRoot.parentCluster = newRoot;
                oldRoot.childrenClusters.Remove(newRoot);
                //Edit the new root cluster's children & parent
                newRoot.parentCluster = null;
                newRoot.childrenClusters.Add(oldRoot);
                rootCluster = newRoot;
                rootClusterID = newRoot.getID();
                MobileModel.rootTreeID = rootClusterID;
                changeRootChildren();
                //Here we need to send to all the new headers the new parametrs in it
               // oldRoot.clusterHeader.headerSensor.CellHeader.hasSinkPosition = false;
               // oldRoot.clusterHeader.headerSensor.CellHeader.isRootHeader = false;
               // oldRoot.clusterHeader.headerSensor.CellHeader.ClearBuffer();

                oldRoot.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.hasSinkPosition = false;
                oldRoot.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.isRootHeader = false;
                oldRoot.CellTable.CellHeader.ClearCellHeaderBuffer();

                newRoot.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.hasSinkPosition = false;
                newRoot.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.SinkAgent = null;
                newRoot.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.isRootHeader = true;
               // newRoot.CellTable.CellHeader.GenerateTreeChange(oldRoot.CellTable.CellHeader);
                
                CellFunctions.ChangeTreeLevels();
              
            }
        }

        private static void changeRootChildren()
        {
            CellGroup root = CellGroup.getClusterWithID(rootClusterID);

            foreach (CellGroup linked in root.clusterLinks.hasLinkwith)
            {
                if (linked.parentCluster.getID() != rootClusterID)
                {
                    double offset = PublicParameters.cellRadius + PublicParameters.cellRadius/2;
                    double distance = Operations.DistanceBetweenTwoPoints(linked.clusterActualCenter, root.clusterActualCenter);
                   // if (distance <= offset)
                    //{
                        CellGroup oldParent = CellGroup.getClusterWithID(linked.parentCluster.getID());
                        CellGroup child = CellGroup.getClusterWithID(linked.getID());
                        oldParent.childrenClusters.Remove(linked);
                        child.parentCluster = root;
                        root.childrenClusters.Add(linked);
                    //}  
                }
            }
        }

        private void _btn_cluster_change(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Grid parent = btn.Parent as Grid;
            Label text = parent.Children[1] as Label;
            String x = text.Content.ToString();
            int nearest;
            Int32.TryParse(x, out nearest);
            keepChanging(nearest);
        }

        private Queue<CellGroup> TreeQueueReingfold = new Queue<CellGroup>();

        private void setInitialXVal(CellGroup parent)
        {
            int numberofChildren = parent.childrenClusters.Count;
            int startX = 0;
            if (numberofChildren > 0)
            {
                foreach (CellGroup child in parent.childrenClusters)
                {
                    child.xValue = startX;
                    startX++;
                    TreeQueueReingfold.Enqueue(child);
                }
            }
           
            if (TreeQueueReingfold.Count > 0)
            {
                setInitialXVal(TreeQueueReingfold.Dequeue());
            }

        }

        private void Reingfold_Tree_Drawing()
        {
            setInitialXVal(rootCluster);
        }



    }
}
