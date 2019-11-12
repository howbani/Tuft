using Tuft.Constructor;
using Tuft.Dataplane;
using Tuft.Dataplane.NOS;
using Tuft.Dataplane.PacketRouter;
using Tuft.Intilization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tuft.Models.MobileModel;
using Tuft.Properties;
using Tuft.Forwarding;

namespace Tuft.Models.MobileSink
{
    public class MobileModel
    {
        private static int sinkDirection;
        private static double sinkInterval;
        private static DispatcherTimer timer_move = new DispatcherTimer();
        private static DispatcherTimer timer_changeInter = new DispatcherTimer();
        private static DispatcherTimer timer_checkSinkGoingOut = new DispatcherTimer();
        private static DispatcherTimer timer_getNewDirection = new DispatcherTimer();
        public Stack<int> PastDirections = new Stack<int>();
        private Sensor sink = PublicParameters.SinkNode;
        private static bool changedLocation = true;
        public static bool isSinkStatic = false;
        public static int rootTreeID { get; set; }

        private static Point oldLocation { get; set; }
        private static Point currentLocation { get; set; }
        private static bool isInsideCuster { get; set; }

        private static Line lineBetweenTwo = new Line();
        private static Canvas sensingField { get; set; }
        private static bool alreadySetLine = false;

        public static Sensor Agent { get; set; }

        private static CellGroup myNearCluster { get; set; }
        private static CellGroup AgentViewsCell { get; set; }
        public static Sensor rootClusterHeader { get; set; }
        private static Sensor myAgent { get; set; }
        private static bool isOutOfBound = false;


        


        private double nodeCommRange = PublicParameters.CommunicationRangeRadius;
        //Here we change the sink 
        //First we put the function that's going to be called every tick 


       

        private static void setLine()
        {
            CellGroup root = CellGroup.getClusterWithID(rootTreeID);
            lineBetweenTwo.Fill = Brushes.Black;
            lineBetweenTwo.Stroke = Brushes.Black;
            lineBetweenTwo.X1 = root.clusterActualCenter.X;
            lineBetweenTwo.Y1 = root.clusterActualCenter.Y;
            lineBetweenTwo.X2 = PublicParameters.SinkNode.CenterLocation.X;
            lineBetweenTwo.Y2 = PublicParameters.SinkNode.CenterLocation.Y;
            if (!alreadySetLine)
            {
                sensingField.Children.Add(lineBetweenTwo);
                alreadySetLine = true;
            }



        }


        private static void moveSink(Sensor sink, int direction)
        {
            double maxY = sensingField.ActualHeight;
            double maxX = sensingField.ActualWidth;

            double x = sink.Position.X;
            double y = sink.Position.Y;
            switch (direction)
            {
                case 0:
                    //Do nothing
                    break;
                case 1:
                    //Move to the right
                    x++;
                    break;
                case 2:
                    //Move to the left
                    x--;
                    break;
                case 3:
                    //Move up
                    y--;
                    break;
                case 4:
                    //Move down
                    y++;
                    break;
                case 5:
                    //up right
                    x++;
                    y--;
                    break;
                case 6:
                    //down left
                    x--;
                    y++;
                    break;
                case 7:
                    //left up
                    x--;
                    y--;
                    break;
                case 8:
                    //right down
                    x++;
                    y++;
                    break;
                default:
                    break;
            }
            if (!isSinkStatic)
            {
                Point p = new Point(x, y);
                PublicParameters.SinkNode.Position = p;
            }            
            sink = PublicParameters.SinkNode;
            currentLocation = sink.CenterLocation;

            sinkInOrOut();
            updateNeighborsTable();
            checkDistanceWithAgent();
            //setLine();
        }

        public static void setInitialParameters()
        {
            updateNeighborsTable();
            myNearCluster = CellGroup.getClusterWithID(Tree.rootClusterID);
            checkDistanceWithAgent();
        }

        private static void checkDistanceWithAgent()
        {
            if (isOutOfBound)
            {
                outOfBound();
            }
            
                if (myAgent != null)
                {

                    double distance = Operations.DistanceBetweenTwoPoints(PublicParameters.SinkNode.CenterLocation, myAgent.CenterLocation);
                    double offset = (PublicParameters.SinkNode.ComunicationRangeRadius-5);
                    if (distance > offset)
                    {
                        chooseAgent();
                    }
                }
                else
                {
                    
                    chooseAgent();
                }
            


        }


        private static void updateNeighborsTable()
        {
            PublicParameters.SinkNode.NeighborsTable.Clear();
            double offset = PublicParameters.CommunicationRangeRadius;

            foreach (Sensor sen in PublicParameters.myNetwork)
            {
                if (sen.ID != PublicParameters.SinkNode.ID)
                {
                    double distance = Operations.DistanceBetweenTwoPoints(sen.CenterLocation, PublicParameters.SinkNode.CenterLocation);
                    int entered = 0;
                    if (distance <= offset)
                    {
                        entered++;
                        NeighborsTableEntry entry = new NeighborsTableEntry();
                        entry.NeiNode = sen;
                        PublicParameters.SinkNode.NeighborsTable.Add(entry);
                    }
                    if (entered > 10)
                    {
                        Console.WriteLine("Sink has lots of neighbors");
                    }
                }
            }
            if(PublicParameters.SinkNode.NeighborsTable.Count > 15)
            {
                Console.WriteLine("More than 15 neighbors");
            }

            if (PublicParameters.SinkNode.NeighborsTable.Count == 0)
            {
                isOutOfBound = true;
            }
        }



        private static void sendFollowUpToAgent(Sensor oldAgent, Sensor newAgent)
        {
            if (AgentViewsCell != null)
            {
                //Console.WriteLine("Changing Agent from {0} to {1}", oldAgent.ID, newAgent.ID);
                
                if (newAgent.ID == oldAgent.ID && (AgentViewsCell.getID() != Tree.rootClusterID))
                {
                    PublicParameters.SinkNode.GenerateAS(oldAgent, newAgent, CellGroup.getClusterWithID(Tree.rootClusterID).CellTable.CellHeader);
                    newAgent.AgentNode = new Agent(PublicParameters.SinkNode, CellGroup.getClusterWithID(Tree.rootClusterID).CellTable.CellHeader, oldAgent, newAgent);
                }
                else if (newAgent.ID != oldAgent.ID)
                {
                    PublicParameters.SinkNode.GenerateAS(oldAgent, newAgent, CellGroup.getClusterWithID(Tree.rootClusterID).CellTable.CellHeader);
                    PublicParameters.SinkNode.GenerateFM(oldAgent, newAgent);
                    newAgent.AgentNode = new Agent(PublicParameters.SinkNode, CellGroup.getClusterWithID(Tree.rootClusterID).CellTable.CellHeader, oldAgent, newAgent);
                    oldAgent.AgentNode.ChangeAgentFM(newAgent);
                }
            }
            else
            {
                PublicParameters.SinkNode.GenerateAS(oldAgent, newAgent, CellGroup.getClusterWithID(Tree.rootClusterID).CellTable.CellHeader);
                newAgent.AgentNode = new Agent(PublicParameters.SinkNode, CellGroup.getClusterWithID(Tree.rootClusterID).CellTable.CellHeader, oldAgent, newAgent);
            }

        }


        private static Point getDirection(int direction)
        {
            double x = 0;
            double y = 0;
            switch (direction)
            {
                case 0:
                    //Do nothing
                    break;
                case 1:
                    //Move to the right
                    x++;
                    break;
                case 2:
                    //Move to the left
                    x--;
                    break;
                case 3:
                    //Move up
                    y--;
                    break;
                case 4:
                    //Move down
                    y++;
                    break;
                case 5:
                    //right up
                    x++;
                    y--;
                    break;
                case 6:
                    //right down
                    x--;
                    y++;
                    break;
                case 7:
                    //left up
                    x--;
                    y--;
                    break;
                case 8:
                    //left down
                    x++;
                    y++;
                    break;
                default:
                    break;
            }

            return new Point(x, y);

        }

       
        private static bool agentBuffering = false;
        private static void outOfBound()
        {
            if (!agentBuffering)
            {
                //Send a buffer message to agent

                agentBuffering = true;
            }
        }

  
        private static Sensor groupNeighbors()
        {
            //MobileModel model = new MobileModel();
            Sensor sink = PublicParameters.SinkNode;
            Point currentSinkLocation = sink.CenterLocation;
            double smallest = 0;
            Sensor cand = null;
            Point destination = getDirection(sinkDirection);
            double futureMoves = sinkInterval + 5;
            destination.X *= futureMoves;
            destination.Y *= futureMoves;
            Point futureSinkLocation = new Point(currentSinkLocation.X + destination.X, currentSinkLocation.Y + destination.Y); // If sink is going up the location will decrease 
            
            if (sink.NeighborsTable.Count == 0)
            {
                //Sink is out of bound send to the CurrentSinkAgent to buffer
                outOfBound();
                Console.WriteLine("Out of bound");
                cand = null;
                return cand;
            }
            else
            {
           
                   Point RootCellHeader = CellGroup.getClusterWithID(rootTreeID).CellTable.CellHeader.CenterLocation;
                    double dirSum =0;
                    double enerSum=0;
                    double eucSum=0;
                Sensor CellHeader = CellGroup.getClusterWithID(rootTreeID).CellTable.CellHeader;
                    foreach (NeighborsTableEntry neiEntry in sink.NeighborsTable){
                        if (neiEntry.ID != CellHeader.ID)
                        {
                            FlowTableEntry MiniEntry = new FlowTableEntry();
                            MiniEntry.NeighborEntry = neiEntry;
                        //    MiniEntry.NeighborEntry.EuclidieanDistance = Operations.DistanceBetweenTwoPoints(futureSinkLocation, MiniEntry.NeighborEntry.CenterLocation);
                            MiniEntry.NeighborEntry.EDNorm = Operations.GetTransmissionDistanceProbability(futureSinkLocation, MiniEntry.NeighborEntry.CenterLocation);
                            eucSum += MiniEntry.NeighborEntry.EDNorm;
                           // MiniEntry.NeighborEntry.Direction = Operations.DistanceBetweenTwoPoints(currentSinkLocation, MiniEntry.NeighborEntry.CenterLocation);
                            MiniEntry.NeighborEntry.DirNorm = Operations.GetTransmissionDistanceProbability(CellHeader.CenterLocation, MiniEntry.NeighborEntry.CenterLocation);
                            dirSum += MiniEntry.NeighborEntry.DirNorm;
                            MiniEntry.NeighborEntry.ResidualEnegNorm = Operations.GetResidualEnergyProbability(MiniEntry.NeighborEntry.NeiNode.ResidualEnergyPercentage);
                            enerSum += MiniEntry.NeighborEntry.ResidualEnegNorm;
                            sink.TuftFlowTable.Add(MiniEntry);
                        }
                       
                    }        
                    foreach(FlowTableEntry entry in sink.TuftFlowTable){
                        entry.NeighborEntry.EDNorm /= eucSum;
                        entry.NeighborEntry.DirNorm /= dirSum;
                        entry.NeighborEntry.ResidualEnegNorm /= enerSum;
                        entry.NeighborEntry.EuclidieanDistance = (entry.NeighborEntry.EDNorm + entry.NeighborEntry.DirNorm)*0.5 + entry.NeighborEntry.ResidualEnegNorm;
                        if(entry.NeighborEntry.EuclidieanDistance > smallest && !(entry.NeighborEntry.NeiNode.TuftNodeTable.CellHeaderTable.isHeader)){
                            smallest = entry.NeighborEntry.EuclidieanDistance;
                            cand = entry.NeighborEntry.NeiNode;
                        }
                    }
                
            }
            sink.TuftFlowTable.Clear();
            if (cand == null)
            {
                Console.WriteLine("cand is null");
            }
            return cand;
        }

        public static void chooseAgent()
        {
            if (myAgent != null)
            {
                //Changing the agent from to 
                Sensor newAgent = groupNeighbors();
                if (newAgent != null)
                {
                    sendFollowUpToAgent(myAgent, newAgent);
                    AgentViewsCell = CellGroup.getClusterWithID(Tree.rootClusterID);
                    if (AgentViewsCell == null)
                    {
                        Console.WriteLine("View Cell is null");
                    }
                    myAgent = newAgent;
                }
                else {
                    Console.WriteLine("Agent Not Found");
                    return;
                }
             

            }
            else
            {
                //First time choice
                myAgent = groupNeighbors();
                sendFollowUpToAgent(myAgent, myAgent);
                AgentViewsCell = CellGroup.getClusterWithID(Tree.rootClusterID);
            }



        }


        public static void passField(Canvas sensingFiel)
        {
            sensingField = sensingFiel;
        }

        private static void sinkInOrOut()
        {
            CellGroup root = CellGroup.getClusterWithID(rootTreeID);
            double distanceWithRoot = Operations.DistanceBetweenTwoPoints(currentLocation, root.clusterActualCenter);
            double radius = PublicParameters.cellRadius;
            if (distanceWithRoot < (radius / 2))
            {
                isInsideCuster = true;
            }
            else
            {
                isInsideCuster = false;
            }

        }



        //start moving the sink here 

        //Changes the interval between each one pixel move of the sink


         public static void StopSinkMovement()
        {
            timer_move.Stop();
            timer_changeInter.Stop();
            timer_checkSinkGoingOut.Stop();
        }


        public void startMoving()
        {           
            RandomeNumberGenerator.SetSeedFromSystemTime();
            myNearCluster = getNearestCluster();
            sinkDirection = GetUniformDirection();
            PastDirections.Push(sinkDirection);
            if (Settings.Default.SinkSpeed > 0)
            {
                timer_changeInter.Interval = TimeSpan.FromSeconds(3);
                timer_changeInter.Start();
                timer_changeInter.Tick += timer_tick_speed;
                //Moves the sink according to its speed and direction
                timer_move.Interval = TimeSpan.FromSeconds(0.5);
                timer_move.Start();
                timer_move.Tick += timer_tick_move;
                //Changes the direction of the sink
                timer_checkSinkGoingOut.Interval = TimeSpan.FromSeconds(1);
                timer_checkSinkGoingOut.Tick += timer_tick_direction;
                timer_checkSinkGoingOut.Start();
                timer_getNewDirection.Interval = TimeSpan.FromSeconds(getDirectionInterval());
                timer_getNewDirection.Tick += timer_getNewDirection_Tick;
                timer_getNewDirection.Start();

            }
            
        }

        void timer_getNewDirection_Tick(object sender, EventArgs e)
        {
            changeDirection();   
        }

        private static double getDirectionInterval()
        {
            double speed = Settings.Default.SinkSpeed;
            double constant = 25;
            double avg = constant / speed;
            return (avg*5 );
        }
        private CellGroup getNearestCluster()
        {

            CellGroup oldRoot = CellGroup.getClusterWithID(rootTreeID);
            double distanceWithRoot = Operations.DistanceBetweenTwoPoints(currentLocation, oldRoot.clusterActualCenter);
            double offset = distanceWithRoot;
            int nearestCluster = rootTreeID;
           
            foreach (CellGroup child in oldRoot.childrenClusters)
            {
                double distance = Operations.DistanceBetweenTwoPoints(currentLocation, child.clusterActualCenter);
                if (distance < offset)
                {
                    offset = distance;
                    nearestCluster = child.getID();
                 
                }
            }
            // Console.WriteLine("In nearest cluster is {0}", nearestCluster);
            
            return CellGroup.getClusterWithID(nearestCluster);

        }


        private void movedLocation()
        {
            PublicParameters.SinkNode.Ellipse_MAC.Fill = Brushes.OrangeRed;
            double halfRad = (PublicParameters.cellRadius / 2);
            if (changedLocation)
            {
                oldLocation = currentLocation;
                changedLocation = false;
            }

            double offset = 0;
            CellGroup nearestRoot = CellGroup.getClusterWithID(rootTreeID);

            if (isInsideCuster)
            {
                offset = halfRad;
            }
            else if (!isInsideCuster)
            {
                offset = (halfRad / 2);
            }

            double distanceCrossed = Operations.DistanceBetweenTwoPoints(currentLocation, oldLocation);

            if (distanceCrossed > offset)
            {
                
                myNearCluster = getNearestCluster();
                //myNearCluster = Cluster.getClusterWithID(nearestCluster);
                changedLocation = true;
                if (myNearCluster.getID() != Tree.rootClusterID)
                {
                    Tree.keepChanging(myNearCluster.getID());
                    chooseAgent();
                }
            }

        }



        

        private void getSinkInterval()
        {
            int MaxSinkSpeed = Settings.Default.SinkSpeed;
            double speedInKmph = RandomeNumberGenerator.uniformMaxSpeed(MaxSinkSpeed);
            sinkInterval = Operations.kmphToTimerInterval(speedInKmph);
            if (sinkInterval > 0)
            {
                timer_move.Interval = TimeSpan.FromSeconds(sinkInterval);
            }
            
        }

       
        private static int directionMean = 270;
        private static int sinkAngle;
        private static int oldSinkDirection { get; set; }
       // private static int oldDirectionMean { get; set; }
       // private static bool firstInitialize = false;



        private static bool isAllowedToChange{get;set;}

        private void checkIfSinkIsGoingOut()
        {
            /*For Border Node 
             * [0] Smallest Y
             * [1] Smallest X
             * [2] Biggest Y
             * [3] Biggest X
             * */
                Point sinkPos = PublicParameters.SinkNode.CenterLocation;
                if ((PublicParameters.BorderNodes[3].Position.X - sinkPos.X) < 40)
                {
                    directionMean = 180;
                    sinkDirection = 2;
                    isAllowedToChange = false;
              //  changeDirection();
                }
                else if (((PublicParameters.BorderNodes[2].Position.Y - sinkPos.Y) < 40))
                {
                sinkDirection = 3;
                //changeDirection();
                directionMean = 90;
               
                 isAllowedToChange = false;
            }
                else if ((sinkPos.Y - PublicParameters.BorderNodes[0].Position.Y) <40)
                {
                sinkDirection = 4;
                //changeDirection();
                 directionMean = 270;
                 
                 isAllowedToChange = false;
            }
                else if (((sinkPos.X - PublicParameters.BorderNodes[1].Position.X) <40))
                {
                sinkDirection = 1;
               // changeDirection();
                 directionMean = 360;
                  
                  isAllowedToChange = false;
            }
                if(PastDirections.Count >= 1)
            {
                int oldDir = PastDirections.Pop();
                if (oldDir != sinkDirection)
                {
                    PastDirections.Push(oldDir);
                    PastDirections.Push(sinkDirection);
                }
                else
                {
                    PastDirections.Push(sinkDirection);

                }
            }
           
        }


        private void checkTheNewDirection()
        {
            bool didChange = false;
            int dir;
            int counter = 0;
            bool CheckTwoDirections = (PastDirections.Count > 1);
            int currentDirection = 0;
            if(PastDirections.Count > 1)
            {
                currentDirection = PastDirections.Pop();
            }
            else
            {
                currentDirection = sinkDirection;
            }
            int oldDirection = 0;
            if (CheckTwoDirections)
            {
                oldDirection = PastDirections.Pop();
            }
            do
            {
                counter++;
                sinkAngle = RandomeNumberGenerator.uniformMaxDirection(directionMean);
                //sinkDirection = Operations.ConvertAngleToDirection(sinkAngle);
                dir = Operations.ConvertAngleToDirection(sinkAngle);
               
               
                if (dir % 2 == 0)
                {
                    if ((dir - 1) != sinkDirection)
                    {
                        if (CheckTwoDirections)
                        {
                            if ((dir - 1) != oldDirection)
                            {
                                sinkDirection = dir;
                                didChange = true;
                            }
                        }
                        else
                        {
                            sinkDirection = dir;
                            didChange = true;
                        }

                    }
                        
                }
                else if ((dir + 1) != sinkDirection)
                {
                    if (CheckTwoDirections)
                    {
                        if((dir+1) != oldSinkDirection)
                        {
                            didChange = true;
                            sinkDirection = dir;
                        }
                    }
                    else
                    {
                        didChange = true;
                        sinkDirection = dir;
                    }
                    
                }
            } while ((!didChange) && counter <3);
            PastDirections.Push(currentDirection);
            if (didChange)
            {
                
                  //  int hold = PastDirections[(PastDirections.Count-1)];
                    PastDirections.Clear();
                    PastDirections.Push(currentDirection);
                    PastDirections.Push(sinkDirection);
                
                switch (dir)
                {
                    case 1:
                        directionMean = 360;
                        break;
                    case 2:
                        directionMean = 180;
                        break;
                    case 3:
                        directionMean = 90;
                        break;
                    case 4:
                        directionMean = 270;
                        break;
                    case 5:
                        directionMean = 45;
                        break;
                    case 6:
                        directionMean = 225;
                        break;
                    case 7:
                        directionMean = 135;
                        break;
                    case 8:
                        directionMean = 315;
                        break;
                    case 0:
                        directionMean = 0;
                        break;
                    default:
                        break;
                }
            }
        }


        private int GetUniformDirection()
        {
            int nd = Convert.ToInt16(Math.Abs(UnformRandomNumberGenerator.GetUniform(8)));
            //checkTheNewDirection(nd);
            return nd;
        }
     
        private void changeDirection()
        {
            if (isAllowedToChange)
            {     
                    checkTheNewDirection();
   
                isAllowedToChange = false;
            }
            else
            {
               // sinkAngle = RandomeNumberGenerator.uniformMaxDirection(directionMean);
                //sinkDirection = Operations.ConvertAngleToDirection(sinkAngle);
                isAllowedToChange = true;
                /*if(PastDirections.Count >= 1)
                {
                    int oldDir = PastDirections.Pop();
                    if (oldDir != sinkDirection)
                    {
                        PastDirections.Push(oldDir);
                        PastDirections.Push(sinkDirection);
                    }
                    else
                    {
                        PastDirections.Push(sinkDirection);

                    }
                }*/
                
            }
                 
        }


        


        private void moveSink()
        {
            MobileModel.moveSink(PublicParameters.SinkNode, sinkDirection);
            if (Settings.Default.SinkSpeed == 0)
            {
                isSinkStatic = true;
            }
            movedLocation();
        }

    
        private void timer_tick_move(Object sender, EventArgs e)
        {

            //this.Dispatcher.Invoke(() => getSinkDirection());

            moveSink();

        }
        private void timer_tick_speed(Object sender, EventArgs e)
        {
            // this.Dispatcher.Invoke(() => getSinkInterval());
            getSinkInterval();
        }
        private void timer_tick_direction(Object sender, EventArgs e)
        {
            checkIfSinkIsGoingOut();
        }
    }
}









