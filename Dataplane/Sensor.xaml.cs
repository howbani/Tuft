using Tuft.Intilization;
using Tuft.Energy;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tuft.ui;
using Tuft.Properties;
using System.Windows.Threading;
using System.Threading;
using Tuft.ControlPlane.NOS;
using Tuft.ui.conts;
using Tuft.ControlPlane.NOS.FlowEngin;
using Tuft.Forwarding;
using Tuft.Dataplane.PacketRouter;
using Tuft.Dataplane.NOS;
using Tuft.Models.MobileSink;
using Tuft.Constructor;
using System.Diagnostics;
using Tuft.Models.MobileModel;
using Tuft.ControlPlane.DistributionWeights;
using Tuft.Models.Energy;
using Tuft.Models.Cell;

namespace Tuft.Dataplane
{
    public enum SensorState { initalized, Active, Sleep } // defualt is not used. i 
    public enum EnergyConsumption { Transmit, Recive } // defualt is not used. i 


    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Sensor : UserControl
    {
        #region Common parameters.
        
        public Radar Myradar; 
        public List<Arrow> MyArrows = new List<Arrow>();
        public MainWindow MainWindow { get; set; } // the mian window where the sensor deployed.
        public static double SR { get; set; } // the radios of SENSING range.
        public double SensingRangeRadius { get { return SR; } }
        public static double CR { get; set; }  // the radios of COMUNICATION range. double OF SENSING RANGE
        public double ComunicationRangeRadius { get { return CR; } }
        public double BatteryIntialEnergy; // jouls // value will not be changed
        private double _ResidualEnergy; //// jouls this value will be changed according to useage of battery
        public List<int> DutyCycleString = new List<int>(); // return the first letter of each state.
        public BoXMAC Mac { get; set; } // the mac protocol for the node.
        public SensorState CurrentSensorState { get; set; } // state of node.
        public List<RoutingLog> Logs = new List<RoutingLog>();
        public List<NeighborsTableEntry> NeighborsTable = null; // neighboring table.
        public List<FlowTableEntry> TuftFlowTable = new List<FlowTableEntry>(); //flow table.
        private BatteryLevelThresh BT = new BatteryLevelThresh();
        public int NumberofPacketsGeneratedByMe = 0; // the number of packets sent by this packet.
        public FirstOrderRadioModel EnergyModel = new FirstOrderRadioModel(); // energy model.
        public int ID { get; set; } // the ID of sensor.
       
        public bool trun { get; set; }// this will be true if the node is already sent the beacon packet for discovering the number of hops to the sink.
        private DispatcherTimer SendPacketTimer = new DispatcherTimer();// 
        private DispatcherTimer QueuTimer = new DispatcherTimer();// to check the packets in the queue right now.
        public Queue<Packet> WaitingPacketsQueue = new Queue<Packet>(); // packets queue.
        public DispatcherTimer OldAgentTimer = new DispatcherTimer();
        public List<BatRange> BatRangesList = new List<Energy.BatRange>();

        public CaluclateWeights CW = new CaluclateWeights();

        public int inCell = -1;
        
        public CellNode TuftNodeTable = new CellNode();

        public Agent AgentNode = new Agent();
        public bool isSinkAgent = false;
        public Sensor SinkAdversary { get; set; }
        public Point SinkPosition { get; set; }
        public bool CanRecievePacket { get { return this.ResidualEnergy > 0; } }
        private Stopwatch QueryDelayStopwatch { get; set; }
        public int agentBufferCount { get {
            if (this.isSinkAgent)
            {
                return this.AgentNode.AgentBuffer.Count;
            }
            else
            {
                return 0;
            }
            } }
        public int cellHeaderBufferCount
        {
            get
            {
                if (this.inCell == -1)
                {
                    return 0;
                }
                else
                {
                    if (this.TuftNodeTable.myCellHeader.ID != this.ID)
                    {
                        return 0;
                    }
                    else
                    {

                        return this.TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count;
                     
                    }
                    
                }
                
            }
        }
        public double CellHeaderProbability { get; set; }

        public Stopwatch DelayStopWatch = new Stopwatch(); 
        /// <summary>
        /// CONFROM FROM NANO NO JOUL
        /// </summary>
        /// <param name="UsedEnergy_Nanojoule"></param>
        /// <returns></returns>
        public double ConvertToJoule(double UsedEnergy_Nanojoule) //the energy used for current operation
        {
            double _e9 = 1000000000; // 1*e^-9
            double _ONE = 1;
            double oNE_DIVIDE_e9 = _ONE / _e9;
            double re = UsedEnergy_Nanojoule * oNE_DIVIDE_e9;
            return re;
        }

        /// <summary>
        /// in JOULE
        /// </summary>
        public double ResidualEnergy // jouls this value will be changed according to useage of battery
        {
            get { return _ResidualEnergy; }
            set
            {
                _ResidualEnergy = value;
                Prog_batteryCapacityNotation.Value = _ResidualEnergy;
            }
        } //@unit(JOULS);


        /// <summary>
        /// 0%-100%
        /// </summary>
        public double ResidualEnergyPercentage
        {
            get { return (ResidualEnergy / BatteryIntialEnergy) * 100; }
        }
        /// <summary>
        /// visualized sensing range and comuinication range
        /// </summary>
        public double VisualizedRadius
        {
            get { return Ellipse_Sensing_range.Width / 2; }
            set
            {
                // sensing range:
                Ellipse_Sensing_range.Height = value * 2; // heigh= sen rad*2;
                Ellipse_Sensing_range.Width = value * 2; // Width= sen rad*2;
                SR = VisualizedRadius;
                CR = SR * 2; // comunication rad= sensing rad *2;

                // device:
                Device_Sensor.Width = value * 4; // device = sen rad*4;
                Device_Sensor.Height = value * 4;
                // communication range
                Ellipse_Communication_range.Height = value * 4; // com rang= sen rad *4;
                Ellipse_Communication_range.Width = value * 4;

                // battery:
                Prog_batteryCapacityNotation.Width = 8;
                Prog_batteryCapacityNotation.Height = 2;
            }
        }

        /// <summary>
        /// Real postion of object.
        /// </summary>
        public Point Position
        {
            get
            {
                double x = Device_Sensor.Margin.Left;
                double y = Device_Sensor.Margin.Top;
                Point p = new Point(x, y);
                return p;
            }
            set
            {
                Point p = value;
                Device_Sensor.Margin = new Thickness(p.X, p.Y, 0, 0);
            }
        }

        /// <summary>
        /// center location of node.
        /// </summary>
        public Point CenterLocation
        {
            get
            {
                double x = Device_Sensor.Margin.Left;
                double y = Device_Sensor.Margin.Top;
                Point p = new Point(x + CR, y + CR);
                return p;
            }
        }

        bool StartMove = false; // mouse start move.
        private void Device_Sensor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.Default.IsIntialized == false)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    System.Windows.Point P = e.GetPosition(MainWindow.Canvas_SensingFeild);
                    P.X = P.X - CR;
                    P.Y = P.Y - CR;
                    Position = P;
                    StartMove = true;
                }
            }
        }

        private void Device_Sensor_MouseMove(object sender, MouseEventArgs e)
        {
            if (Settings.Default.IsIntialized == false)
            {
                if (StartMove)
                {
                    System.Windows.Point P = e.GetPosition(MainWindow.Canvas_SensingFeild);
                    P.X = P.X - CR;
                    P.Y = P.Y - CR;
                    this.Position = P;
                }
            }
        }

        private void Device_Sensor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StartMove = false;
        }

        private void Prog_batteryCapacityNotation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            double val = ResidualEnergyPercentage;
            if (val <= 0)
            {
                MainWindow.RandomSelectSourceNodesTimer.Stop();
                
                // dead certificate:
                ExpermentsResults.Lifetime.DeadNodesRecord recod = new ExpermentsResults.Lifetime.DeadNodesRecord();
                recod.DeadAfterPackets = PublicParameters.NumberofGeneratedDataPackets;
                recod.DeadOrder = PublicParameters.DeadNodeList.Count + 1;
                recod.Rounds = PublicParameters.Rounds + 1;
                recod.DeadNodeID = ID;
                recod.NOS = PublicParameters.NOS;
                recod.NOP = PublicParameters.NOP;
                PublicParameters.DeadNodeList.Add(recod);

                Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col0));
                Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col0));


                if (Settings.Default.StopeWhenFirstNodeDeid)
                {
                    MainWindow.TimerCounter.Stop();
                    MainWindow.RandomSelectSourceNodesTimer.Stop();
                    MainWindow.stopSimlationWhen = PublicParameters.SimulationTime;
                    MainWindow.top_menu.IsEnabled = true;
                    MobileModel.StopSinkMovement();
                }
                Mac.SwichToSleep();
                Mac.SwichOnTimer.Stop();
                Mac.ActiveSleepTimer.Stop();
                if (this.ResidualEnergy <= 0)
                {
                    while (this.WaitingPacketsQueue.Count > 0)
                    {
                        //PublicParameters.NumberofDropedPackets += 1;
                        Packet pack = WaitingPacketsQueue.Dequeue();
                        pack.isDelivered = false;
                       // PublicParameters.FinishedRoutedPackets.Add(pack);
                        Console.WriteLine("PID:" + pack.PID + " has been droped.");
                        updateStates(pack);
                        MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Droped_Packet.Content = PublicParameters.NumberofDropedPackets, DispatcherPriority.Send);

                    }
                    this.QueuTimer.Stop();
                    foreach(Sensor sen in PublicParameters.myNetwork)
                    {
                        if(sen.WaitingPacketsQueue.Count > 0)
                        {
                            while (sen.WaitingPacketsQueue.Count > 0)
                            {
                                Packet pkt = sen.WaitingPacketsQueue.Dequeue();
                                pkt.isDelivered = false;
                                pkt.DroppedReason = "DeadNode";
                                updateStates(pkt);
                            }
                        }
                        if (sen.TuftNodeTable.CellHeaderTable.isHeader)
                        {
                            if (sen.TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0)
                            {
                                while(sen.TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0)
                                {
                                    Packet pkt = sen.TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Dequeue();
                                    pkt.isDelivered = false;
                                    pkt.DroppedReason = "DeadNode";
                                    updateStates(pkt);
                                }
                            }
                        }else if(sen.agentBufferCount > 0)
                        {
                            if (sen.AgentNode.AgentBuffer.Count > 0)
                            {
                                while (sen.AgentNode.AgentBuffer.Count > 0)
                                {
                                    Packet pkt = sen.AgentNode.AgentBuffer.Dequeue();
                                    pkt.isDelivered = false;
                                    pkt.DroppedReason = "DeadNode";
                                    updateStates(pkt);
                                }
                            }
                        }
                    }
                    if (Settings.Default.ShowRadar) Myradar.StopRadio();
                    QueuTimer.Stop();
                    Console.WriteLine("NID:" + this.ID + ". Queu Timer is stoped.");
                    MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.Transparent);
                    MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Hidden);

                    return;
                }
                return;


            }
            if (val >= 1 && val <= 9)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col1_9)));
               Dispatcher.Invoke(()=> Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col1_9)));
            }

            if (val >= 10 && val <= 19)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col10_19)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col10_19)));
            }

            if (val >= 20 && val <= 29)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col20_29)));
                Dispatcher.Invoke(() => Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col20_29))));
            }

            // full:
            if (val >= 30 && val <= 39)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col30_39)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col30_39)));
            }
            // full:
            if (val >= 40 && val <= 49)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col40_49)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col40_49)));
            }
            // full:
            if (val >= 50 && val <= 59)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col50_59)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col50_59)));
            }
            // full:
            if (val >= 60 && val <= 69)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col60_69)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col60_69)));
            }
            // full:
            if (val >= 70 && val <= 79)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col70_79)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col70_79)));
            }
            // full:
            if (val >= 80 && val <= 89)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col80_89)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col80_89)));
            }
            // full:
            if (val >= 90 && val <= 100)
            {
                Dispatcher.Invoke(() => Prog_batteryCapacityNotation.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col90_100)));
                Dispatcher.Invoke(() => Ellipse_battryIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BatteryLevelColoring.col90_100)));
            }


            /*
            // update the battery distrubtion.
            int battper = Convert.ToInt16(val);
            if (battper > PublicParamerters.UpdateLossPercentage)
            {
                int rangeIndex = battper / PublicParamerters.UpdateLossPercentage;
                if (rangeIndex >= 1)
                {
                    if (BatRangesList.Count > 0)
                    {
                        BatRange range = BatRangesList[rangeIndex - 1];
                        if (battper >= range.Rang[0] && battper <= range.Rang[1])
                        {
                            if (range.isUpdated == false)
                            {
                                range.isUpdated = true;
                                // update the uplink.
                                UplinkRouting.UpdateUplinkFlowEnery(this,);

                            }
                        }
                    }
                }
            }*/
        }


        /// <summary>
        /// show or hide the arrow in seperated thread.
        /// </summary>
        /// <param name="id"></param>
        public void ShowOrHideArrow(int id) 
        {
            Thread thread = new Thread(() =>
            
            {
                lock (MyArrows)
                {
                    Arrow ar = GetArrow(id);
                    if (ar != null)
                    {
                        lock (ar)
                        {
                            if (ar.Visibility == Visibility.Visible)
                            {
                                Action action = () => ar.Visibility = Visibility.Hidden;
                                Dispatcher.Invoke(action);
                            }
                            else
                            {
                                Action action = () => ar.Visibility = Visibility.Visible;
                                Dispatcher.Invoke(action);
                            }
                        }
                    }
                }
            }
            );
            thread.Name = "Arrow for " + id;
            thread.Start();
        }


        // get arrow by ID.
        private Arrow GetArrow(int EndPointID)
        {
            foreach (Arrow arr in MyArrows) { if (arr.To.ID == EndPointID) return arr; }
            return null;
        }



       

        #endregion



       
       

        /// <summary>
        /// 
        /// </summary>
        public void SwichToActive()
        {
            Mac.SwichToActive();

        }

        /// <summary>
        /// 
        /// </summary>
        private void SwichToSleep()
        {
            Mac.SwichToSleep();
        }

       
        public Sensor(int nodeID)
        {
            InitializeComponent();
            //: sink is diffrent:
            if (nodeID == 0) BatteryIntialEnergy = PublicParameters.BatteryIntialEnergyForSink; // the value will not be change
            else
                BatteryIntialEnergy = PublicParameters.BatteryIntialEnergy;
           
            
            ResidualEnergy = BatteryIntialEnergy;// joules. intializing.
            Prog_batteryCapacityNotation.Value = BatteryIntialEnergy;
            Prog_batteryCapacityNotation.Maximum = BatteryIntialEnergy;
            lbl_Sensing_ID.Content = nodeID;
            ID = nodeID;
            QueuTimer.Interval = PublicParameters.QueueTime;
            QueuTimer.Tick += DeliveerPacketsInQueuTimer_Tick;
            OldAgentTimer.Interval = TimeSpan.FromSeconds(3);
            OldAgentTimer.Tick += RemoveOldAgentTimer_Tick;
            //:

            SendPacketTimer.Interval = TimeSpan.FromSeconds(1);
           

        }


        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            

        }

        /// <summary>
        /// hide all arrows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            /*
            Vertex ver = MainWindow.MyGraph[ID];
            foreach(Vertex v in ver.Candidates)
            {
                MainWindow.myNetWork[v.ID].lbl_Sensing_ID.Background = Brushes.Black;
            }*/
         
        }

        

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
        }

       

        public int ComputeMaxHopsUplink
        {
            get
            {
                double  DIS= Operations.DistanceBetweenTwoSensors(PublicParameters.SinkNode, this);
                return Convert.ToInt16(Math.Ceiling((Math.Sqrt(PublicParameters.Density) * (DIS / ComunicationRangeRadius))));
            }
        }

        public int ComputeMaxHopsDownlink(Sensor endNode)
        {
            double DIS = Operations.DistanceBetweenTwoSensors(PublicParameters.SinkNode, endNode);
            return Convert.ToInt16(Math.Ceiling((Math.Sqrt(PublicParameters.Density) * (DIS / ComunicationRangeRadius))));
        }

        #region Old Sending Data ///
      
        /// <summary>
        ///  data or control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reciver"></param>
        /// <param name="packt"></param>
        
       
        
        public void IdentifySourceNode(Sensor source)
        {
            if (Settings.Default.ShowAnimation && source.ID != PublicParameters.SinkNode.ID)
            {
                Action actionx = () => source.Ellipse_indicator.Visibility = Visibility.Visible;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => source.Ellipse_indicator.Fill = Brushes.Yellow;
                Dispatcher.Invoke(actionxx);
            }
        }

        public void UnIdentifySourceNode(Sensor source)
        {
            if (Settings.Default.ShowAnimation && source.ID != PublicParameters.SinkNode.ID)
            {
                Action actionx = () => source.Ellipse_indicator.Visibility = Visibility.Hidden;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => source.Ellipse_indicator.Fill = Brushes.Transparent;
                Dispatcher.Invoke(actionxx);
            }
        }

        public void GenerateDataPacket()
        {
            if (Settings.Default.IsIntialized && this.ResidualEnergy > 0)
            {
                this.DissemenateData();

            }
        }

        public void GenerateMultipleDataPackets(int numOfPackets)
        {
            for (int i = 0; i < numOfPackets; i++)
            {
                GenerateDataPacket();
                //  Thread.Sleep(50);
            }
        }

        public void GenerateControlPacket(Sensor endNode)
        {
            if (Settings.Default.IsIntialized && this.ResidualEnergy > 0)
            {

                

            }
        }
        /// <summary>
        /// to the same endnode.
        /// </summary>
        /// <param name="numOfPackets"></param>
        /// <param name="endone"></param>
        public void GenerateMultipleControlPackets(int numOfPackets, Sensor endone)
        {
            for (int i = 0; i < numOfPackets; i++)
            {
                GenerateControlPacket(endone);
            }
        }

        public void IdentifyEndNode(Sensor endNode)
        {
            if (Settings.Default.ShowAnimation && endNode.ID != PublicParameters.SinkNode.ID)
            {
                Action actionx = () => endNode.Ellipse_indicator.Visibility = Visibility.Visible;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => endNode.Ellipse_indicator.Fill = Brushes.DarkOrange;
                Dispatcher.Invoke(actionxx);
            }
        }

        public void UnIdentifyEndNode(Sensor endNode)
        {
            if (Settings.Default.ShowAnimation && endNode.ID != PublicParameters.SinkNode.ID)
            {
                Action actionx = () => endNode.Ellipse_indicator.Visibility = Visibility.Hidden;
                Dispatcher.Invoke(actionx);

                Action actionxx = () => endNode.Ellipse_indicator.Fill = Brushes.Transparent;
                Dispatcher.Invoke(actionxx);
            }
        }

        public void btn_send_packet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lbl_title = sender as Label;
            switch (lbl_title.Name)
            {
                case "btn_send_1_packet":
                    {
                        if (this.ID != PublicParameters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(1);
                        }
                        else
                        {
                            RandomSelectEndNodes(1);
                        }

                        break;
                    }
                case "btn_send_10_packet":
                    {
                        if (this.ID != PublicParameters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(10);
                        }
                        else
                        {
                            RandomSelectEndNodes(10);
                        }
                        break;
                    }

                case "btn_send_100_packet":
                    {
                        if (this.ID != PublicParameters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(100);
                        }
                        else
                        {
                            RandomSelectEndNodes(100);
                        }
                        break;
                    }

                case "btn_send_300_packet":
                    {
                        if (this.ID != PublicParameters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(300);
                        }
                        else
                        {
                            RandomSelectEndNodes(300);
                        }
                        break;
                    }

                case "btn_send_1000_packet":
                    {
                        if (this.ID != PublicParameters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(1000);
                        }
                        else
                        {
                            RandomSelectEndNodes(1000);
                        }
                        break;
                    }

                case "btn_send_5000_packet":
                    {
                        if (this.ID != PublicParameters.SinkNode.ID)
                        {
                            // uplink:
                            GenerateMultipleDataPackets(5000);
                        }
                        else
                        {
                            // DOWN
                            RandomSelectEndNodes(5000);
                        }
                        break;
                    }
            }
        }

        private void OpenChanel(int reciverID, long PID)
        {
            Thread thread = new Thread(() =>
            {
                lock (MyArrows)
                {
                    
                    Arrow ar = GetArrow(reciverID);
                    if (ar != null)
                    {
                        lock (ar)
                        {
                            if (ar.Visibility == Visibility.Hidden)
                            {
                                if (Settings.Default.ShowAnimation)
                                {
                                    Action actionx = () => ar.BeginAnimation(PID);
                                    Dispatcher.Invoke(actionx);
                                    Action action1 = () => ar.Visibility = Visibility.Visible;
                                    Dispatcher.Invoke(action1);
                                }
                                else
                                {
                                    Action action1 = () => ar.Visibility = Visibility.Visible;
                                    Dispatcher.Invoke(action1);
                                    Dispatcher.Invoke(() => ar.Stroke = new SolidColorBrush(Colors.Black));
                                    Dispatcher.Invoke(() => ar.StrokeThickness = 1);
                                    Dispatcher.Invoke(() => ar.HeadHeight = 1);
                                    Dispatcher.Invoke(() => ar.HeadWidth = 1);
                                }
                            }
                            else
                            {
                                if (Settings.Default.ShowAnimation)
                                {
                                    int cid = Convert.ToInt16(PID % PublicParameters.RandomColors.Count);
                                    Action actionx = () => ar.BeginAnimation(PID);
                                    Dispatcher.Invoke(actionx);
                                    Dispatcher.Invoke(() => ar.HeadHeight = 1);
                                    Dispatcher.Invoke(() => ar.HeadWidth = 1);
                                }
                                else
                                {
                                    Dispatcher.Invoke(() => ar.Stroke = new SolidColorBrush(Colors.Black));
                                    Dispatcher.Invoke(() => ar.StrokeThickness = 1);
                                    Dispatcher.Invoke(() => ar.HeadHeight = 1);
                                    Dispatcher.Invoke(() => ar.HeadWidth = 1);
                                }
                            }
                        }
                    }
                }
            }
           );
            thread.Name = "OpenChannel thread " + reciverID + "PID:" + PID;
            thread.Start();
            thread.Priority = ThreadPriority.Highest;
        }

        #endregion


        #region send data: /////////////////////////////////////////////////////////////////////////////

       
        public bool isQreqGoingIn(Packet QReq)
        {
            double distance = Operations.DistanceBetweenTwoPoints(this.CenterLocation, QReq.DestinationAddress);
            if (distance <= PublicParameters.cellRadius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    
        public int maxHopsForDestination(Sensor destination)
        {
            if (destination != null)
            {
                try
                {
                    double DIS = Operations.DistanceBetweenTwoPoints(destination.CenterLocation, this.CenterLocation) *1.5;
                    return PublicParameters.HopsErrorRange + Convert.ToInt16(Math.Ceiling(((PublicParameters.Density / 2) * (DIS / ComunicationRangeRadius))));
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e.Message + " destination node in max hops is null");
                    return 0;
                }

            }
            else { return 0; }
           
        }

        public int maxHopsForQuery(Sensor sourceNode)
        {
            if (sourceNode.inCell != -1)
            {
                double DIS;

                if(sourceNode.TuftNodeTable.myCellHeader.ID != sourceNode.ID){
                    double twoNodes = Operations.DistanceBetweenTwoSensors(sourceNode, sourceNode.TuftNodeTable.myCellHeader) ;
                    DIS = ((PublicParameters.networkCells.Count ) * PublicParameters.cellRadius);
                    DIS += twoNodes;

                }else{
                    DIS = sourceNode.TuftNodeTable.CellHeaderTable.DistanceFromRoot * 2;         
                    
                 }

                return Convert.ToInt16(Math.Ceiling(((PublicParameters.Density / 2) * (DIS / ComunicationRangeRadius)))) + PublicParameters.HopsErrorRange;
            }
            else
            {

                double smallDistance = Operations.DistanceBetweenTwoPoints(sourceNode.CenterLocation, sourceNode.TuftNodeTable.NearestCellCenter);
                double DIS = (PublicParameters.cellRadius * PublicParameters.networkCells.Count) /3;
                DIS += smallDistance;
                return Convert.ToInt16(Math.Ceiling(((PublicParameters.Density / 2) * (DIS / ComunicationRangeRadius))));
            }
        
        }

        //**************Generating Packets and Data Dissemenation

        public void DissemenateData()
        {
            //MessageBox.Show("I am here");
            PublicParameters.NumberOfNodesDissemenating += 1;
           
            if (this.isSinkAgent)
            {
                //Directly send to the sink
                this.GenerateDataToSink(PublicParameters.SinkNode);
            }
            else if (this.inCell != -1)
            {
                if (TuftNodeTable.CellHeaderTable.isHeader)
                {
                    if (TuftNodeTable.CellHeaderTable.isRootHeader)
                    {
                        if (TuftNodeTable.CellHeaderTable.hasSinkPosition)
                        {
                            GenerateDataToSink(TuftNodeTable.CellHeaderTable.SinkAgent);
                        }
                    }
                    else
                    {
                        GenerateQueryRequest();
                    }
                }
                else
                {
                    GenerateQueryRequest();
                }
            }
            else
            {
                GenerateQueryRequest();
            }
 
        }

        public void GenerateDataToSink(Sensor SinkAgent)
        {
           
            Packet packet = new Packet();
            PublicParameters.NumberofGeneratedDataPackets += 1;
            
            packet.Source = this;
            packet.PacketLength = PublicParameters.RoutingDataLength;
            packet.PacketType = PacketType.Data;
            packet.PID = PublicParameters.OverallGeneratedPackets;
            packet.Path = "" + this.ID;
            packet.Destination = SinkAgent;
            packet.TimeToLive = this.maxHopsForDestination(SinkAgent);
            IdentifySourceNode(this);
            MainWindow.Dispatcher.Invoke(() => PublicParameters.SinkNode.MainWindow.lbl_num_of_gen_packets.Content = PublicParameters.NumberofGeneratedDataPackets, DispatcherPriority.Normal);
         
            this.sendDataPack(packet);
        }
        
        public void GenerateQueryRequest()
        {
            if (Settings.Default.IsIntialized && this.ResidualEnergy > 0)
            {
                PublicParameters.NumberofGeneratedQueryPackets += 1;
                Packet QReq = new Packet();
                QReq.Path = "" + this.ID;
                QReq.TimeToLive = maxHopsForQuery(this);
                QReq.Source = this;
                QReq.PacketLength = PublicParameters.ControlDataLength;
                QReq.PacketType = PacketType.QReq;
                QReq.PID = PublicParameters.OverallGeneratedPackets;
                if (this.inCell == -1)
                {
                    QReq.DestinationAddress = this.TuftNodeTable.NearestCellCenter;
                    QReq.isQreqInsideCell = false ;
                }
                else
                {  
                    if(this.TuftNodeTable.CellHeaderTable.isHeader){
                        QReq.DestinationAddress = this.TuftNodeTable.CellHeaderTable.ParentCellCenter;
                        QReq.isQreqInsideCell = false;
                    }else{
                        QReq.Destination = this.TuftNodeTable.myCellHeader;
                        QReq.isQreqInsideCell = true;
                    }
                }
                IdentifySourceNode(this);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_num_of_gen_query.Content = PublicParameters.NumberofGeneratedQueryPackets, DispatcherPriority.Normal);
                SendQReq(QReq);
            }
          
        }

        public void GenerateQueryResponse(Sensor destination)
        {
            Packet QResp = new Packet();
            PublicParameters.NumberofGeneratedQueryPackets += 1;
            QResp.Source = this;
            QResp.Path = "" + this.ID;
            QResp.PacketLength = PublicParameters.ControlDataLength;
            QResp.PacketType = PacketType.QResp;
            try
            {
               if (!destination.Equals(null))
                    {                             
                        QResp.Destination = destination;
                        QResp.TimeToLive = this.maxHopsForDestination(QResp.Destination);               
                        QResp.PID = PublicParameters.OverallGeneratedPackets;
                    if (TuftNodeTable.CellHeaderTable.SinkAgent != null)
                    {
                        QResp.SinkAgent = this.TuftNodeTable.CellHeaderTable.SinkAgent;
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                        
                        IdentifySourceNode(this);
                        MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_num_of_gen_query.Content = PublicParameters.NumberofGeneratedQueryPackets, DispatcherPriority.Normal);
                        //:  
                        this.SendQResponse(QResp);
                    }
                else
                {
                    QResp.isDelivered = false;
                    QResp.DroppedReason = "Empty Destination in QResp";
                    updateStates(QResp);
                }
            }catch(NullReferenceException e){
                QResp.isDelivered = false;
                QResp.DroppedReason = "Empty Destination in QResp";
                updateStates(QResp);
                Console.WriteLine(e.Message);
            }
            
           
               
          
        }

        public void GenerateTreeChange(Sensor OldRoot)
        {
            Packet control = new Packet();
            PublicParameters.NumberofGeneratedFollowUpPackets += 1;
            control.Source = this;
            control.Root = this;
            control.PacketLength = PublicParameters.ControlDataLength;
            control.PacketType = PacketType.Control;
            control.PID = PublicParameters.OverallGeneratedPackets;
            control.Path = "" + this.ID;
            control.Destination = OldRoot;
            control.TimeToLive = this.maxHopsForDestination(OldRoot);
            IdentifySourceNode(this);
            MainWindow.Dispatcher.Invoke(() => PublicParameters.SinkNode.MainWindow.lbl_num_of_gen_followup.Content = PublicParameters.NumberofGeneratedFollowUpPackets, DispatcherPriority.Normal);
            this.SendTreeChange(control);

        }
        public void GenerateAS(Sensor oldAgent,Sensor newAgent,Sensor rootClusterHeader)
        {
            Packet ASNewAgent = new Packet();
            Packet ASOldAgent = new Packet();
            PublicParameters.NumberofGeneratedFollowUpPackets += 1;
            ASNewAgent.Source = this;
            ASNewAgent.Root = rootClusterHeader;
            ASNewAgent.OldAgent = oldAgent;
            ASNewAgent.PacketLength = PublicParameters.ControlDataLength;
            ASNewAgent.PacketType = PacketType.AS;
            ASNewAgent.PID = PublicParameters.OverallGeneratedPackets;
            ASNewAgent.Path = "" + this.ID;
            ASNewAgent.Destination = newAgent;
            ASNewAgent.TimeToLive = this.maxHopsForDestination(rootClusterHeader);
         /*   if (oldAgent.ID != newAgent.ID)
            {
                ASOldAgent = ASNewAgent;
                PublicParameters.NumberofGeneratedFollowUpPackets += 1;
                ASOldAgent.Destination = oldAgent;
                ASOldAgent.PID = PublicParameters.NumberofGeneratedDataPackets;
                this.SendAS(ASOldAgent);
            }*/
           
            IdentifySourceNode(this);
            MainWindow.Dispatcher.Invoke(() => PublicParameters.SinkNode.MainWindow.lbl_num_of_gen_followup.Content = PublicParameters.NumberofGeneratedFollowUpPackets, DispatcherPriority.Normal);
            this.SendAS(ASNewAgent);
         
            

        }

        public void GenerateFM(Sensor OldAgent,Sensor newAgent)
        {
            //..WriteLine("Sending From {0} to old agent {1}", this.ID, OldAgent.ID);
            PublicParameters.NumberofGeneratedFollowUpPackets++;
            Packet FM = new Packet();
            FM.Source = this;
            try
            {
                FM.Destination = OldAgent;
                FM.PacketLength = PublicParameters.ControlDataLength;
                FM.PID = PublicParameters.OverallGeneratedPackets;
                FM.PacketType = PacketType.FM;
                FM.Path = "" + this.ID;               
                FM.TimeToLive = this.maxHopsForDestination(OldAgent);               
                FM.SinkAgent = newAgent;
                IdentifySourceNode(this);
                MainWindow.Dispatcher.Invoke(() => PublicParameters.SinkNode.MainWindow.lbl_num_of_gen_followup.Content = PublicParameters.NumberofGeneratedFollowUpPackets, DispatcherPriority.Normal);
                this.sendFM(FM);
            }
            catch(NullReferenceException e)
            {
                Console.WriteLine(e.Message + " from generate FM agent null");
            }
            
           
        }
        public void GenerateFSA(Sensor CellLeader)
        {
             try
            {
                                  
                Packet FSA = new Packet();
                PublicParameters.NumberofGeneratedFollowUpPackets += 1;           
                FSA.Destination = CellLeader;
                FSA.PacketLength = PublicParameters.ControlDataLength;
                FSA.PID = PublicParameters.OverallGeneratedPackets;
                FSA.PacketType = PacketType.FSA;
                FSA.Path = "" + this.ID;
                FSA.Source = this;
                FSA.TimeToLive = this.maxHopsForDestination(CellLeader);
                IdentifySourceNode(this);   
                MainWindow.Dispatcher.Invoke(() => PublicParameters.SinkNode.MainWindow.lbl_num_of_gen_followup.Content = PublicParameters.NumberofGeneratedFollowUpPackets, DispatcherPriority.Normal);
                this.sendFSA(FSA);
             }
            catch(NullReferenceException e)
            {
                Console.WriteLine("FSA returning a null reference "+e.Message);
            }
            
        }
        
        //********************Sending

        public void sendDataPack(Packet packet)
        {
           
            lock (TuftFlowTable)
            {
                Sensor Reciver;

                if (isSinkAgent && packet.Destination.ID == PublicParameters.SinkNode.ID)
                {
                    Reciver = PublicParameters.SinkNode;
                    ComputeOverhead(packet, EnergyConsumption.Transmit, Reciver);
                    //Console.WriteLine("bn:" + ID + "->" + Reciver.ID + ". PID: " + packet.PID);
                    Reciver.RecieveDataPack(packet);
                    return;

                }
                else if (Operations.isInMyComunicationRange(this, packet.Destination))
                {
                    Reciver = packet.Destination;
                    if (Reciver.CanRecievePacket && Reciver.CurrentSensorState == SensorState.Active)
                    {
                        ComputeOverhead(packet, EnergyConsumption.Transmit, Reciver);
                        Reciver.RecieveDataPack(packet);
                        return;
                    }
                    else
                    {
                        WaitingPacketsQueue.Enqueue(packet);
                        if (!QueuTimer.IsEnabled)
                        {
                            QueuTimer.Start();
                        }
                        
                        if (Settings.Default.ShowRadar) Myradar.StartRadio();
                        PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                        PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                        return;
                    }

                }
                else {
                    LinkRouting.GetD_Distribution(this, packet);
                    if (this.TuftFlowTable.Count == 0)
                    {
                        packet.isDelivered = false;
                        packet.DroppedReason = "No Flows match";
                        updateStates(packet);
                        return;
                    }
                    else
                    {
                        FlowTableEntry FlowEntry = MatchFlow(packet);
                        if (FlowEntry != null)
                        {
                            Reciver = FlowEntry.NeighborEntry.NeiNode;

                            ComputeOverhead(packet, EnergyConsumption.Transmit, Reciver);
                            //Console.WriteLine("sucess:" + ID + "->" + Reciver.ID + ". PID: " + packet.PID);
                            FlowEntry.DownLinkStatistics += 1;
                            Reciver.RecieveDataPack(packet);
                        }
                        else
                        {
                            // no available node right now.
                            // add the packt to the wait list.
                            //  Console.WriteLine("NID:" + this.ID + " Faild to sent PID:" + packet.PID);
                            WaitingPacketsQueue.Enqueue(packet);
                            if (!QueuTimer.IsEnabled)
                            {
                                QueuTimer.Start();
                            }
                            // Console.WriteLine("NID:" + this.ID + ". Queu Timer is started.");

                            if (Settings.Default.ShowRadar) Myradar.StartRadio();
                            PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                            PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                            return;
                        }
                    }
              
                }
                    
            }
        }

      
        public void SendQReq(Packet QReq)
        {
            Sensor Reciver;
            if (this.inCell != -1)
            {
                if (this.TuftNodeTable.CellHeaderTable.isRootHeader && this.TuftNodeTable.CellHeaderTable.isHeader)
                {
                    if (TuftNodeTable.CellHeaderTable.hasSinkPosition)
                    {
                        QReq.isDelivered = true;
                        updateStates(QReq);
                        GenerateQueryResponse(QReq.Source);
                        return;
                    }
                    else
                    {
                        TuftNodeTable.CellHeaderTable.StoreInCellHeaderBuffer(QReq);
                        return;
                    }

                }
                else if (this.TuftNodeTable.CellHeaderTable.isHeader)
                {
                    if (QReq.Destination != null)
                    {
                        
                        QReq.DestinationAddress = TuftNodeTable.CellHeaderTable.ParentCellCenter;
                        QReq.isQreqInsideCell = false;
                        QReq.Destination = null;
                        SendQReq(QReq);
                        return;
                    }
                }
                else
                {
                    if (QReq.isQreqInsideCell)
                    {
                        if (Operations.isInMyComunicationRange(this, TuftNodeTable.myCellHeader) && !TuftNodeTable.CellHeaderTable.isHeader)
                        {
                            Reciver = TuftNodeTable.myCellHeader;
                            if (Reciver.ID == ID)
                            {
                                Console.WriteLine("Same id");
                            }
                            if (Reciver.CanRecievePacket && Reciver.CurrentSensorState == SensorState.Active)
                            {
                                ComputeOverhead(QReq, EnergyConsumption.Transmit, Reciver);
                                Reciver.RecvQReq(QReq);
                            }
                            else
                            {
                                WaitingPacketsQueue.Enqueue(QReq);
                                if (!QueuTimer.IsEnabled)
                                {
                                    QueuTimer.Start();
                                }
                                if (Settings.Default.ShowRadar) Myradar.StartRadio();
                                PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                                PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                            }

                            return;
                        }  
                    }
                    
                }
            } 
            LinkRouting.GetD_Distribution(this, QReq);
            if (TuftFlowTable.Count == 0)
            {
                QReq.isDelivered = false;
                QReq.DroppedReason = "No Flows Matched";
                updateStates(QReq);
                return;

            }
            else
            {
                FlowTableEntry FlowEntry = MatchFlow(QReq);
                if (FlowEntry != null)
                {
                    Reciver = FlowEntry.NeighborEntry.NeiNode;
                    if (Reciver.ID == ID)
                    {
                        Console.WriteLine("Same id");
                    }
                    ComputeOverhead(QReq, EnergyConsumption.Transmit, Reciver);
                    Reciver.RecvQReq(QReq);
                }
                else
                {
                    // no available node right now.
                    // add the packt to the wait list.
                    WaitingPacketsQueue.Enqueue(QReq);
                    if (!QueuTimer.IsEnabled)
                    {
                        QueuTimer.Start();
                    }
                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }
            }
            
        }
         public void SendQResponse(Packet QResp)
        {
            lock (TuftFlowTable)
            {
                Sensor Reciver;
               if (Operations.isInMyComunicationRange(this, QResp.Destination))
                {
                    Reciver = QResp.Destination;
                    if (Reciver.CanRecievePacket && Reciver.CurrentSensorState == SensorState.Active)
                    {
                        ComputeOverhead(QResp, EnergyConsumption.Transmit, Reciver);
                        Reciver.RecvQueryResponse(QResp);
                    }
                    else
                    {
                        WaitingPacketsQueue.Enqueue(QResp);
                        QueuTimer.Start();
                       
                        if (Settings.Default.ShowRadar) Myradar.StartRadio();
                        PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                        PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                    }

                }
                else
                {
                    LinkRouting.GetD_Distribution(this, QResp);
                    FlowTableEntry FlowEntry = MatchFlow(QResp);
                    if (FlowEntry != null)
                    {
                        Reciver = FlowEntry.NeighborEntry.NeiNode;
                        ComputeOverhead(QResp, EnergyConsumption.Transmit, Reciver);
                        FlowEntry.DownLinkStatistics += 1;
                        Reciver.RecvQueryResponse(QResp);
                    }
                    else
                    {
                        // no available node right now.
                        // add the packt to the wait list.
                        WaitingPacketsQueue.Enqueue(QResp);
                        if (!QueuTimer.IsEnabled)
                        {
                            QueuTimer.Start();
                        }

                        if (Settings.Default.ShowRadar) Myradar.StartRadio();
                        PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                        PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                    }
               }
               
            }
        }

        public void SendAS(Packet AS)
        {
            lock (TuftFlowTable) {

                Sensor Reciver = AS.Destination;
                if (Reciver.CanRecievePacket && Reciver.CurrentSensorState == SensorState.Active)
                {
                    ComputeOverhead(AS, EnergyConsumption.Transmit, Reciver);
                   // Console.WriteLine("sucess:" + ID + "->" + Reciver.ID + ". PID: " + AS.PID);
                    Reciver.RecieveAS(AS);
                }
                else
                {
                   // Console.WriteLine("NID:" + this.ID + " Faild to sent PID:" + AS.PID);
                    WaitingPacketsQueue.Enqueue(AS);
                    QueuTimer.Start();
                   // Console.WriteLine("NID:" + this.ID + ". Queu Timer is started.");
                   
                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }
            }
           
        }

        public void sendFM(Packet FM)
        {
            Sensor Reciver;
            if (Operations.isInMyComunicationRange(this, FM.Destination))
            {
                Reciver = FM.Destination;
                if (Reciver.CanRecievePacket && Reciver.CurrentSensorState == SensorState.Active)
                {
                    ComputeOverhead(FM, EnergyConsumption.Transmit, Reciver);
                    Reciver.RecieveFM(FM);
                }
                else
                {
                    WaitingPacketsQueue.Enqueue(FM);
                    QueuTimer.Start();
                   
                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }

            }
            else
            {
                LinkRouting.GetD_Distribution(this, FM);
                FlowTableEntry FlowEntry = MatchFlow(FM);
                if (FlowEntry != null)
                {
                    Reciver = FlowEntry.NeighborEntry.NeiNode;
                    ComputeOverhead(FM, EnergyConsumption.Transmit, Reciver);
                    FlowEntry.DownLinkStatistics += 1;
                    Reciver.RecieveFM(FM);

                }
                else
                {
                    // Console.WriteLine("NID:" + this.ID + " Faild to sent PID:" + FM.PID);
                    WaitingPacketsQueue.Enqueue(FM);
                    QueuTimer.Start();
                    //  Console.WriteLine("NID:" + this.ID + ". Queu Timer is started.");

                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }
            }
           
        }

        public void sendFSA(Packet FSA)
        {
            Sensor Reciver;
            if (Operations.isInMyComunicationRange(this, FSA.Destination))
            {
                Reciver = FSA.Destination;
                if (Reciver.CanRecievePacket && Reciver.CurrentSensorState == SensorState.Active)
                {
                    ComputeOverhead(FSA, EnergyConsumption.Transmit, Reciver);
                    Reciver.RecieveFSA(FSA);
                }
                else
                {
                    WaitingPacketsQueue.Enqueue(FSA);
                    QueuTimer.Start();
                   
                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }

            }
            else
            {
                LinkRouting.GetD_Distribution(this, FSA);
                FlowTableEntry FlowEntry = MatchFlow(FSA);
                if (FlowEntry != null)
                {
                    Reciver = FlowEntry.NeighborEntry.NeiNode;
                    ComputeOverhead(FSA, EnergyConsumption.Transmit, Reciver);
                    FlowEntry.DownLinkStatistics += 1;
                    Reciver.RecieveFSA(FSA);

                }
                else
                {
                    //  Console.WriteLine("NID:" + this.ID + " Faild to sent PID:" + FSA.PID);
                    WaitingPacketsQueue.Enqueue(FSA);
                    QueuTimer.Start();
                    ///Console.WriteLine("NID:" + this.ID + ". Queu Timer is started.");

                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }
            }
           
        }

        //*******************Recieving 

        #region //Recieving a follow Up
        //Inform Old Agent
        public void RecieveFM(Packet FM)
        {
            FM.ReTransmissionTry = 0;
            if (this.CanRecievePacket)
            {
                if (this.ID == FM.Destination.ID )
                {
                        if (this.AgentNode.hasStoredPackets)
                        {
                            this.AgentDelieverStoredPackets();
                        }
                        FM.isDelivered = true;
                        updateStates(FM);
                }
                else
                {
                    if (FM.Hops > FM.TimeToLive)
                    {
                        FM.isDelivered = false;
                        FM.DroppedReason = " Hops > Time To Live ";
                        updateStates(FM);
                    }
                    else
                    {
                        sendFM(FM);
                    }
                }
            }
            else
            {
                FM.isDelivered = false;
                FM.DroppedReason = "Node " + this.ID + " can't recieve packet";
                updateStates(FM);
            }
        }

        //Inform Root
        public void RecieveFSA(Packet FSA)
        {
            FSA.ReTransmissionTry = 0;
            if (this.CanRecievePacket)
            {
                FSA.Path += ">" + this.ID;
                if (FSA.Destination.ID == this.ID)
                {
                   this.TuftNodeTable.CellHeaderTable.SinkAgent = FSA.Source;
                   this.TuftNodeTable.CellHeaderTable.hasSinkPosition = true;
                   FSA.isDelivered = true;
                   updateStates(FSA);
                   if (this.TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0)
                   {
                      this.ClearCellHeaderBuffer();                               
                    }    
                }
                else if (FSA.Hops>FSA.TimeToLive)
                {
                    FSA.isDelivered = false;
                    FSA.DroppedReason = "Hops > Time to Live ";
                    updateStates(FSA);
                    return;
                }
                else
                {
                    this.sendFSA(FSA);
                }
            }
            else
            {
                FSA.isDelivered = false;
                FSA.DroppedReason = "Node " + this.ID + " can't recieve packet";
                updateStates(FSA);
            }
           
        }

        //Agent Selection
        public void RecieveAS(Packet AS)
        {
            AS.ReTransmissionTry = 0;
            if (this.CanRecievePacket)
            {
                AS.Path += ">" + this.ID;
                if (this.ID == AS.Destination.ID) {
                        //recieve by new agent 
                     AS.isDelivered = true;
                     updateStates(AS);                   
                     this.GenerateFSA(AS.Root);                   
                        if (this.AgentNode.hasStoredPackets)
                        {
                            this.AgentDelieverStoredPackets();
                        }
                }
                else
                {
                    if (AS.Hops > AS.TimeToLive)
                    {
                        // drop the paket.
                        AS.isDelivered = false;
                        AS.DroppedReason = "Hops > Time to live ";
                        updateStates(AS);
                    }
                    else
                    {
                        SendAS(AS);
                    }
                }
                
            }
            else
            {
                AS.isDelivered = false;
                AS.DroppedReason = "Node " + this.ID + " can't recieve packet";
                updateStates(AS);
            }
           
        }
        #endregion
  

        #region //Recieving Data Packet
        public void RecieveDataPack(Packet packet)
        {
            if (packet.Destination == null)
            {
                packet.isDelivered = false;
                packet.DroppedReason = "Empty Agent";
                updateStates(packet);
                return;
            }
            packet.ReTransmissionTry = 0;
            if (Operations.PacketPathToIDS(packet.Path).Contains(PublicParameters.SinkNode.ID))
            {
                Console.WriteLine("Sink contained in packet {0}",packet.PID);
            }

            if (!this.CanRecievePacket)
            {
                packet.isDelivered = false;
                packet.DroppedReason = "Node "+this.ID+" can't recieve packet";
                updateStates(packet);
                return;
            }
            packet.Path += ">" + this.ID;
            if (this.ID == PublicParameters.SinkNode.ID)
            {
     
                packet.isDelivered = true;
                updateStates(packet);
                return;
            }
            else if (packet.Destination.ID == this.ID)
            {
            
                if (this.isSinkAgent)
                {
                    if (this.AgentNode.isSinkInRange())
                    {
                        packet.Destination = PublicParameters.SinkNode;
                        this.sendDataPack(packet);
                        return;
                    }
                    else
                    {
                        this.AgentNode.AgentStorePacket(packet);
                    }
                   
                }
                else
                {
                    //Old Agent Follow Up Mechanisim          
                        if (this.AgentNode.NewAgent != null)
                        {
                            packet.Destination = this.AgentNode.NewAgent;
                            packet.TimeToLive += maxHopsForDestination(packet.Destination);
                            this.sendDataPack(packet);
                        }
                        else
                        {
                            packet.isDelivered = false;
                            packet.DroppedReason = "Old Agent is already restored";
                            updateStates(packet);
                        }
                      
                    }
            }
            else
            {
                if (packet.Hops > packet.TimeToLive)
                {
                    // drop the paket.
                    packet.isDelivered = false;
                    packet.DroppedReason = "Hops > Time to live";
                    updateStates(packet);
                }
                else
                {
                    // forward the packet.
                    this.sendDataPack(packet);
                }
            }
        }
        #endregion

        public void SendTreeChange(Packet packet)
        {
            Sensor Reciver;
            if (Operations.isInMyComunicationRange(this, packet.Destination))
            {
                Reciver = packet.Destination;
                if (Reciver.CanRecievePacket && Reciver.CurrentSensorState == SensorState.Active)
                {
                    ComputeOverhead(packet, EnergyConsumption.Transmit, Reciver);
                    Reciver.RecvTreeChange(packet);
                }
                else
                {
                    WaitingPacketsQueue.Enqueue(packet);
                    if (!QueuTimer.IsEnabled)
                    {
                        QueuTimer.Start();
                    }
                 
                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }

            }
            else
            {
                LinkRouting.GetD_Distribution(this, packet);
                FlowTableEntry FlowEntry = MatchFlow(packet);
                if (FlowEntry != null)
                {
                    Reciver = FlowEntry.NeighborEntry.NeiNode;
                    ComputeOverhead(packet, EnergyConsumption.Transmit, Reciver);
                    Reciver.RecvTreeChange(packet);

                }
                else
                {
                    //  Console.WriteLine("NID:" + this.ID + " Faild to sent PID:" + FSA.PID);
                    WaitingPacketsQueue.Enqueue(packet);
                    if (!QueuTimer.IsEnabled)
                    {
                        QueuTimer.Start();
                    }
                    if (Settings.Default.ShowRadar) Myradar.StartRadio();
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.DeepSkyBlue);
                    PublicParameters.MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Visible);
                }
            }
        }
        public void RecvTreeChange(Packet packet)
        {
            packet.ReTransmissionTry = 0;
            if (this.CanRecievePacket)
            {
                packet.Path += ">" + this.ID;
                if (packet.Destination.ID == this.ID)
                {
                    packet.isDelivered = true;
                    updateStates(packet);
                    return;
                }
                else
                {
                    if (packet.Hops > packet.TimeToLive)
                    {
                        // drop the paket.
                        packet.DroppedReason = "Hops > Time to live";
                        packet.isDelivered = false;
                        updateStates(packet);
                        return;
                    }
                    else
                    {
                        // forward the packet.
                        this.SendTreeChange(packet);
                    }
                }
            }
            else
            {
                packet.isDelivered = false;
                packet.DroppedReason = "Node " + this.ID + " can't recieve packet";
                updateStates(packet);
            }
           
        }

        #region//Recieving Query
        public void  RecvQueryResponse(Packet QResp)
        {
            QResp.ReTransmissionTry = 0;
            if (this.CanRecievePacket)
            {
                QResp.Path += ">" + this.ID;
                List<int> paths = Operations.PacketPathToIDS(QResp.Path);
                
                if (QResp.Destination.ID == this.ID)
                {
                    if(QResp.SinkAgent != null){
                        QResp.isDelivered = true;
                        updateStates(QResp);
                        this.GenerateDataToSink(QResp.SinkAgent);
                        return;
                    }
                    else
                    {
                        QResp.isDelivered = false;
                        QResp.DroppedReason = "Sink Agent is null";
                        updateStates(QResp);
                      
                        return;
                    }
                }
                else
                {
                    if (QResp.Hops > QResp.TimeToLive)
                    {
                        // drop the paket.
                        QResp.DroppedReason = "Hops > Time to live";
                        QResp.isDelivered = false;
                        updateStates(QResp);
                    }
                    else
                    {
                        // forward the packet.
                        this.SendQResponse(QResp);
                    }
                }
            }
            else
            {
                QResp.isDelivered = false;
                QResp.DroppedReason = "Node " + this.ID + " can't recieve packet";
                updateStates(QResp);
            }
           
        }

     
        public static bool needtoCheck = false;
     

        public void CellHeaderRecvQReq(Packet QReq)
        {
            if (TuftNodeTable.CellHeaderTable.isRootHeader)
            {
                if (TuftNodeTable.CellHeaderTable.hasSinkPosition)
                {
                    QReq.isDelivered = true;
                    updateStates(QReq);
                    GenerateQueryResponse(QReq.Source);
                }
                else
                {
                    TuftNodeTable.CellHeaderTable.StoreInCellHeaderBuffer(QReq);
                }
                
            }
            else
            {
                if(TuftNodeTable.CellHeaderTable.ParentCellCenter == null)
                {
                    Console.WriteLine("from, Receve Cell HeaderQReq");
                }
                QReq.TimeToLive = maxHopsForQuery(this);
                 QReq.DestinationAddress = TuftNodeTable.CellHeaderTable.ParentCellCenter;
                 QReq.Destination = null; 
                 QReq.isQreqInsideCell = false;
                if (QReq.Hops > QReq.TimeToLive)
                {
                    QReq.isDelivered = false;
                    QReq.DroppedReason = "TTL>Hops";
                    updateStates(QReq);
                    return;
                }
                else
                {
                    SendQReq(QReq);
                }
            }
           if(BT.threshReached(this.ResidualEnergyPercentage)){
               CellFunctions.ChangeCellHeader(this);
           }

        }
        public void RecvQReq(Packet QReq)
        {
            QReq.ReTransmissionTry = 0;
            QReq.Path += ">" + ID;
            if (!this.CanRecievePacket)
            {
                QReq.isDelivered = false;
                QReq.DroppedReason = "Can't recieve packet";
                updateStates(QReq);
                return;
            }
           
            
            //Recieved by a non encapsuated node
             if (this.inCell == -1)
            {
                 if (QReq.Hops > QReq.TimeToLive)
                {
                    QReq.isDelivered = false;
                    QReq.DroppedReason = "TTL>Hops";
                    updateStates(QReq);
                    return;
                }
                else
                {
                    SendQReq(QReq);
                    return;
                }
              
            }
                //Recieved by a cell node
            else
            {       
                //Cell Node is a CellHeader
                if (TuftNodeTable.CellHeaderTable.isHeader)
                {
                    CellHeaderRecvQReq(QReq);
                    return;
                }
                //Cell node is a regular node
                else
                {
                    if (!QReq.isQreqInsideCell)
                    {
                        if (isQreqGoingIn(QReq))
                        {
                            if(QReq.DestinationAddress == TuftNodeTable.myCellHeader.CenterLocation)
                            {
                                Console.WriteLine("From, isQreq Going In");
                            }
                            QReq.TimeToLive = maxHopsForQuery(this);
                            QReq.DestinationAddress = new Point(0,0);
                            QReq.Destination = TuftNodeTable.myCellHeader;
                            QReq.isQreqInsideCell = true;
                        }

                        if (QReq.Hops > QReq.TimeToLive)
                        {
                            QReq.isDelivered = false;
                            QReq.DroppedReason = "TTL>Hops";
                            updateStates(QReq);
                            return;
                        }
                        else
                        {
                            SendQReq(QReq);
                            return;
                        }
                    }
                    else
                    {
                            QReq.DestinationAddress = new Point(0, 0);
                            QReq.Destination = TuftNodeTable.myCellHeader;
                            QReq.isQreqInsideCell = true;
                        if (QReq.Hops > QReq.TimeToLive)
                        {
                            QReq.isDelivered = false;
                            QReq.DroppedReason = "TTL>Hops";
                            updateStates(QReq);
                            return;
                        }
                        else
                        {
                            SendQReq(QReq);
                            return;
                        }

                    }
                }
            }
        }

        #endregion

        public void AgentDelieverStoredPackets()
        {
            do
            {
                Console.WriteLine("Agent Deliever stored packets");
                Packet packet = this.AgentNode.AgentBuffer.Dequeue();
                if (this.isSinkAgent)
                {
                    if (this.AgentNode.isSinkInRange())
                    {
                      //  Console.WriteLine("Sending to the sink directly");
                        packet.Destination = PublicParameters.SinkNode;
                        packet.TimeToLive += maxHopsForDestination(packet.Destination);
                        sendDataPack(packet);
                    }
                }
                else if (this.AgentNode.NewAgent != null)
                {
                   // Console.WriteLine("Sending to the new agent, packet {0}", packet.PID);
                    packet.Destination = this.AgentNode.NewAgent;
                    packet.TimeToLive += maxHopsForDestination(packet.Destination);
                    PIDE = packet.PID;
                    sendDataPack(packet);
                }
                else
                {
                    packet.isDelivered = false;
                    packet.DroppedReason = "Old Agent unkown destination";
                    updateStates(packet);
                }
            } while (this.AgentNode.AgentBuffer.Count > 0);
            
        }


        public static long PIDE = -1;


        public void updateStates(Packet packet)
        {
            if (packet.isDelivered)
            {
               
                if (packet.PacketType == PacketType.FM || packet.PacketType == PacketType.FSA || packet.PacketType == PacketType.AS || packet.PacketType==PacketType.Control)
                {
                    PublicParameters.NumberofDelieveredFollowUpPackets += 1;
                }
                else if (packet.PacketType == PacketType.QReq || packet.PacketType == PacketType.QResp)
                {
                    PublicParameters.NumberOfDelieveredQueryPackets += 1;
                    packet.ComputeDelay();
                    PublicParameters.QueryDelay += packet.Delay;
                }
                else
                {
                    PublicParameters.NumberOfDelieveredDataPackets += 1;
                    packet.ComputeDelay();
                    PublicParameters.DataDelay += packet.Delay;
                }
                
                

                PublicParameters.NumberofDeliveredPackets += 1;
               // Console.WriteLine("{2} Packet: {0} with Path: {1} delievered",packet.PID,packet.Path,packet.PacketType);
                PublicParameters.FinishedRoutedPackets.Add(packet);
                ComputeOverhead(packet, EnergyConsumption.Recive, null);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_total_consumed_energy.Content = PublicParameters.TotalEnergyConsumptionJoule + " (JOULS)", DispatcherPriority.Send);

                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Delivered_QPacket.Content = PublicParameters.NumberOfDelieveredQueryPackets, DispatcherPriority.Send);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Delivered_CPacket.Content = PublicParameters.NumberofDelieveredFollowUpPackets, DispatcherPriority.Send);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Delivered_Packet.Content = PublicParameters.NumberOfDelieveredDataPackets, DispatcherPriority.Send);

                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_sucess_ratio.Content = PublicParameters.DeliveredRatio, DispatcherPriority.Send);
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_nymber_inQueu.Content = PublicParameters.InQueuePackets.ToString());
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_num_of_disseminatingNodes.Content = PublicParameters.NumberOfNodesDissemenating.ToString());
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Average_QDelay.Content = PublicParameters.AverageQueryDelay.ToString());
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Total_Delay.Content = PublicParameters.AverageTotalDelay.ToString());

                UnIdentifySourceNode(packet.Source);
                // Console.WriteLine("PID:" + packet.PID + " has been delivered.");
            }
            else
            {
               
                Console.WriteLine("Failed {2} PID: {0} Reason: {1}", packet.PID, packet.DroppedReason,packet.PacketType);
                PublicParameters.NumberofDropedPackets += 1;
                PublicParameters.FinishedRoutedPackets.Add(packet);
                //  Console.WriteLine("PID:" + packet.PID + " has been droped.");


                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Droped_Packet.Content = PublicParameters.NumberofDropedPackets, DispatcherPriority.Send);
            }
        }

        private void DeliveerPacketsInQueuTimer_Tick(object sender, EventArgs e)
        {
            if(WaitingPacketsQueue.Count > 0)
            {
                Packet toppacket = WaitingPacketsQueue.Dequeue();
                toppacket.WaitingTimes += 1;
                toppacket.ReTransmissionTry += 1;
                PublicParameters.TotalWaitingTime += 1; // total;
                if (toppacket.ReTransmissionTry < 7)
                {
                    if (toppacket.PacketType == PacketType.QResp)
                    {
                        SendQResponse(toppacket);
                    }
                    else if (toppacket.PacketType == PacketType.QReq)
                    {
                        SendQReq(toppacket);
                    }
                    else if (toppacket.PacketType == PacketType.FSA)
                    {
                        sendFSA(toppacket);
                    }
                    else if (toppacket.PacketType == PacketType.AS)
                    {
                        SendAS(toppacket);
                    }
                    else if (toppacket.PacketType == PacketType.Data)
                    {
                        sendDataPack(toppacket);
                    }
                    else if (toppacket.PacketType == PacketType.FM)
                    {
                        sendFM(toppacket);
                    }
                    else if (toppacket.PacketType == PacketType.Control)
                    {
                        SendTreeChange(toppacket);
                    }
                    else
                    {
                        MessageBox.Show("Unknown");
                    }
                }
                else
                {
                    // PublicParameters.NumberofDropedPackets += 1;
                    toppacket.isDelivered = false;
                    toppacket.DroppedReason = "Waiting times > 7";
                    updateStates(toppacket);
                    //  Console.WriteLine("Waiting times more for packet {0}", toppacket.PID);
                    // PublicParameters.FinishedRoutedPackets.Add(toppacket);
                    //    MessageBox.Show("PID:" + toppacket.PID + " has been droped. Packet Type = "+toppacket.PacketType);
                    MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Number_of_Droped_Packet.Content = PublicParameters.NumberofDropedPackets, DispatcherPriority.Send);
                }
                if (WaitingPacketsQueue.Count == 0)
                {
                    if (Settings.Default.ShowRadar) Myradar.StopRadio();
                    QueuTimer.Stop();
                    // Console.WriteLine("NID:" + this.ID + ". Queu Timer is stoped.");
                    MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.Transparent);
                    MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Hidden);
                }
                MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_nymber_inQueu.Content = PublicParameters.InQueuePackets.ToString());
            }
            else
            {
                if (Settings.Default.ShowRadar) Myradar.StopRadio();
                QueuTimer.Stop();
                // Console.WriteLine("NID:" + this.ID + ". Queu Timer is stoped.");
                MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Fill = Brushes.Transparent);
                MainWindow.Dispatcher.Invoke(() => Ellipse_indicator.Visibility = Visibility.Hidden);
            }
            
            
        }


        private void RemoveOldAgentTimer_Tick(object sender, EventArgs e) {
            OldAgentTimer.Stop();
            this.AgentNode = new Agent();
        }

               
        public static int CountRedun =0;
        public void RedundantTransmisionCost(Packet pacekt, Sensor reciverNode)
        {
            // logs.
            PublicParameters.TotalReduntantTransmission += 1;       
            double UsedEnergy_Nanojoule = EnergyModel.Receive(PublicParameters.PreamblePacketLength); // preamble packet length.
            double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);
            reciverNode.ResidualEnergy = reciverNode.ResidualEnergy - UsedEnergy_joule;
            pacekt.UsedEnergy_Joule += UsedEnergy_joule;
            PublicParameters.TotalEnergyConsumptionJoule += UsedEnergy_joule;
            PublicParameters.TotalWastedEnergyJoule += UsedEnergy_joule;
            MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Redundant_packets.Content = PublicParameters.TotalReduntantTransmission);
            MainWindow.Dispatcher.Invoke(() => MainWindow.lbl_Wasted_Energy_percentage.Content = PublicParameters.WastedEnergyPercentage);
        }

        /// <summary>
        /// the node which is active will send preample packet and will be selected.
        /// match the packet.
        /// </summary>
        public FlowTableEntry MatchFlow(Packet pacekt)
        {

            FlowTableEntry ret = null;
            try
            {
               
                if (TuftFlowTable.Count > 0)
                {
                  
                    foreach (FlowTableEntry selectedflow in TuftFlowTable)
                    {
                        if (selectedflow.NID != PublicParameters.SinkNode.ID)
                        {
                            if (selectedflow.SensorState == SensorState.Active && selectedflow.DownLinkAction == FlowAction.Forward && selectedflow.SensorBufferHasSpace)
                            {
                                if (ret == null)
                                {
                                    ret = selectedflow;
                                }
                                else
                                {
                                    RedundantTransmisionCost(pacekt, selectedflow.NeighborEntry.NeiNode);
                                }
                            }
                        }
                        
                    }
                }
                else
                {
                    MessageBox.Show("No Flow!!!. muach flow!");
                    return null;
                }
            }
            catch
            {
                ret = null;
                MessageBox.Show(" Null Match.!");
            }

            return ret;
        }

        // When the sensor open the channel to transmit the data.
  
        
        




        public void ComputeOverhead(Packet packt, EnergyConsumption enCon, Sensor Reciver)
        {
            if (enCon == EnergyConsumption.Transmit)
            {
                if (ID != PublicParameters.SinkNode.ID)
                {
                    // calculate the energy 
                    double Distance_M = Operations.DistanceBetweenTwoSensors(this, Reciver);
                    double UsedEnergy_Nanojoule = EnergyModel.Transmit(packt.PacketLength, Distance_M);
                    double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);
                    ResidualEnergy = this.ResidualEnergy - UsedEnergy_joule;
                    PublicParameters.TotalEnergyConsumptionJoule += UsedEnergy_joule;
                    packt.UsedEnergy_Joule += UsedEnergy_joule;
                    packt.RoutingDistance += Distance_M;
                    packt.Hops += 1;
                    double delay = DelayModel.DelayModel.Delay(this, Reciver);
                    packt.Delay += delay;
                    PublicParameters.TotalDelayMs += delay;
                    

                    // for control packet.
                    if (packt.isAdvirtismentPacket())
                    {
                        // just to remember how much energy is consumed here.
                        PublicParameters.EnergyComsumedForControlPackets += UsedEnergy_joule;
                    }
                }

                if (Settings.Default.ShowRoutingPaths)
                {
                    OpenChanel(Reciver.ID, packt.PID);
                }

            }
            else if (enCon == EnergyConsumption.Recive)
            {

                double UsedEnergy_Nanojoule = EnergyModel.Receive(packt.PacketLength);
                double UsedEnergy_joule = ConvertToJoule(UsedEnergy_Nanojoule);
                ResidualEnergy = ResidualEnergy - UsedEnergy_joule;
                packt.UsedEnergy_Joule += UsedEnergy_joule;
                PublicParameters.TotalEnergyConsumptionJoule += UsedEnergy_joule;


                if (packt.isAdvirtismentPacket())
                {
                    // just to remember how much energy is consumed here.
                    PublicParameters.EnergyComsumedForControlPackets += UsedEnergy_joule;
                }


            }

        }

     
        #endregion

        #region Buffer



        public void ReRoutePacketsInCellHeaderBuffer()
        {
            if (TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0)
            {
                do
                {

                    Packet pkt = TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Dequeue();
                    if (!TuftNodeTable.CellHeaderTable.isHeader)
                    {
                        pkt.Destination = TuftNodeTable.myCellHeader;
                        pkt.isQreqRouted = true;
                        pkt.ReRouteSource = this;
                        pkt.TimeToLive += maxHopsForQuery(this);
                        this.SendQReq(pkt);
                    }
                    else
                    {
                        Console.WriteLine("FromRe-route");
                    }
                } while (TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0);
            }
        }


        public void ClearCellHeaderBuffer()
        {
            if (TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0)
            {
                Console.WriteLine("Clearing cell header");
                //Regular Cell Node
                if (!TuftNodeTable.CellHeaderTable.isHeader)
                {
                    do
                    {
                      Packet pkt = TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Dequeue();
                      if (TuftNodeTable.myCellHeader.ID != this.ID)
                        {
                            pkt.DestinationAddress = new Point(0,0);
                            pkt.isQreqRouted = true;
                            pkt.ReRouteSource = this;
                            pkt.isQreqInsideCell = true;
                            pkt.Destination = TuftNodeTable.myCellHeader;
                        }
                        else
                        {
                            Console.WriteLine("From Buffer");
                        }
                      pkt.TimeToLive += maxHopsForQuery(this);
                        this.SendQReq(pkt);
                    } while (TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0);
                }
                    //Regular CellHeader
                else if (!TuftNodeTable.CellHeaderTable.isRootHeader)
                {
                    do
                    {
                        Packet pkt = TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Dequeue();
                        if (TuftNodeTable.CellHeaderTable.ParentCellCenter != null)
                        {
                            pkt.DestinationAddress = TuftNodeTable.CellHeaderTable.ParentCellCenter;
                            pkt.Destination = null;
                            pkt.isQreqRouted = true;
                            pkt.ReRouteSource = this;
                            pkt.isQreqInsideCell = false;
                        }
                        else
                        {
                            Console.WriteLine("From Buffer 2");
                        }
                        pkt.TimeToLive += maxHopsForQuery(this);
                        this.SendQReq(pkt);
                    } while (TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0);

                }
                else if (TuftNodeTable.CellHeaderTable.isRootHeader)
                {
                    do
                        {
                            Packet pkt = TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Dequeue();
                             if (TuftNodeTable.CellHeaderTable.hasSinkPosition)
                            {
                                pkt.isDelivered = true;
                                updateStates(pkt);
                                this.GenerateQueryResponse(pkt.Source);
                            } 
                            else
                            {
                                pkt.isDelivered = false;
                                pkt.DroppedReason = "Max # tries dnt havesink pos";
                                updateStates(pkt);
                            }
                        

                        } while (TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0);
                }
                else
                {
                    do
                    {
                        Packet pkt = TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Dequeue();
                        pkt.isDelivered = false;
                        pkt.DroppedReason = "Couldntfind destination";
                        updateStates(pkt);
                    } while (TuftNodeTable.CellHeaderTable.CellHeaderBuffer.Count > 0);
                }
                   
                   
                }
                

            }

       
        #endregion






        private void lbl_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTip = new Label() { Content = "("+ID + ") [ " + ResidualEnergyPercentage + "% ] [ " + ResidualEnergy + " J ]" };
        }

        private void btn_show_routing_log_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(Logs.Count>0)
            {
                UiShowRelativityForAnode re = new ui.UiShowRelativityForAnode();
                re.dg_relative_shortlist.ItemsSource = Logs;
                re.Show();
            }
        }

        private void btn_draw_random_numbers_MouseDown(object sender, MouseButtonEventArgs e)
        {
            List<KeyValuePair<int, double>> rands = new List<KeyValuePair<int, double>>();
            int index = 0;
            foreach (RoutingLog log in Logs )
            {
                if(log.IsSend)
                {
                    index++;
                    rands.Add(new KeyValuePair<int, double>(index, log.ForwardingRandomNumber));
                }
            }
            UiRandomNumberGeneration wndsow = new ui.UiRandomNumberGeneration();
            wndsow.chart_x.DataContext = rands;
            wndsow.Show();
        }

        private void Ellipse_center_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void btn_show_my_duytcycling_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void btn_draw_paths_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NetworkVisualization.UpLinksDrawPaths(this);
        }

       
         
        private void btn_show_my_flows_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            ListControl ConMini = new ui.conts.ListControl();
            ConMini.lbl_title.Content = "Mini-Flow-Table";
            ConMini.dg_date.ItemsSource = TuftFlowTable;


            ListControl ConNei = new ui.conts.ListControl();
            ConNei.lbl_title.Content = "Neighbors-Table";
            ConNei.dg_date.ItemsSource = NeighborsTable;

            UiShowLists win = new UiShowLists();
            win.stack_items.Children.Add(ConMini);
            win.stack_items.Children.Add(ConNei);
            win.Title = "Tables of Node " + ID;
            win.Show();
            win.WindowState = WindowState.Maximized;
        }

        private void btn_send_1_p_each1sec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendPacketTimer.Start();
            SendPacketTimer.Tick += SendPacketTimer_Random; // redfine th trigger.
        }



        public void RandomSelectEndNodes(int numOFpACKETS)
        {
            if (PublicParameters.SimulationTime > PublicParameters.MacStartUp)
            {
                int index = 1 + Convert.ToInt16(UnformRandomNumberGenerator.GetUniform(PublicParameters.NumberofNodes - 2));
                if (index != PublicParameters.SinkNode.ID)
                {
                    Sensor endNode = MainWindow.myNetWork[index];
                    GenerateMultipleControlPackets(numOFpACKETS, endNode);
                }
            }
        }

        private void SendPacketTimer_Random(object sender, EventArgs e)
        {
            if (ID != PublicParameters.SinkNode.ID)
            {
                // uplink:
                GenerateMultipleDataPackets(1);
            }
            else
            { //
                RandomSelectEndNodes(1);
            }
        }

        /// <summary>
        /// i am slected as end node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_select_me_as_end_node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lbl_title = sender as Label;
            switch (lbl_title.Name)
            {
                case "Btn_select_me_as_end_node_1":
                    {
                       PublicParameters.SinkNode.GenerateMultipleControlPackets(1, this);

                        break;
                    }
                case "Btn_select_me_as_end_node_10":
                    {
                        PublicParameters.SinkNode.GenerateMultipleControlPackets(10, this);
                        break;
                    }
                //Btn_select_me_as_end_node_1_5sec

                case "Btn_select_me_as_end_node_1_5sec":
                    {
                        PublicParameters.SinkNode.SendPacketTimer.Start();
                        PublicParameters.SinkNode.SendPacketTimer.Tick += SelectMeAsEndNodeAndSendonepacketPer5s_Tick;
                        break;
                    }
            }
        }

        
        
        public void SelectMeAsEndNodeAndSendonepacketPer5s_Tick(object sender, EventArgs e)
        {
            PublicParameters.SinkNode.GenerateMultipleControlPackets(1, this);
        }





        /*** Vistualize****/

        public void ShowID(bool isVis )
        {
            if (isVis) { lbl_Sensing_ID.Visibility = Visibility.Visible; lbl_hops_to_sink.Visibility = Visibility.Visible; }
            else { lbl_Sensing_ID.Visibility = Visibility.Hidden; lbl_hops_to_sink.Visibility = Visibility.Hidden; }
        }

        public void ShowSensingRange(bool isVis)
        {
            if (isVis) Ellipse_Sensing_range.Visibility = Visibility.Visible;
            else Ellipse_Sensing_range.Visibility = Visibility.Hidden;
        }

        public void ShowComunicationRange(bool isVis)
        {
            if (isVis) Ellipse_Communication_range.Visibility = Visibility.Visible;
            else Ellipse_Communication_range.Visibility = Visibility.Hidden;
        }

        public void ShowBattery(bool isVis) 
        {
            if (isVis) Prog_batteryCapacityNotation.Visibility = Visibility.Visible;
            else Prog_batteryCapacityNotation.Visibility = Visibility.Hidden;
        }

        private void btn_update_mini_flow_MouseDown(object sender, MouseButtonEventArgs e)
        {
          
        }
    }
}
