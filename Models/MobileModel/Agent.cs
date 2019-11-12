using Tuft.Dataplane;
using Tuft.Dataplane.NOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Tuft.Intilization;
using System.Windows;
using Tuft.Dataplane.PacketRouter;
using System.Windows.Media;
using Tuft.ui;

namespace Tuft.Models.MobileModel
{
    public class Agent
    {
        public Sensor sinkNode { get; set; }
        public Sensor OldAgent { get; set; }
        public Sensor RootNode { get; set; }
        public Sensor NewAgent { get; set; }
        public Sensor Node { get; set; }
        public Queue<Packet> AgentBuffer { get; set; }
        public DispatcherTimer OldAgentTimer;
        public DispatcherTimer SinkOutOfRangeTimer = new DispatcherTimer();
        public bool hasStoredPackets
        {
            get
            {
                if (AgentBuffer != null)
                {
                    return (AgentBuffer.Count > 0);
                }
                else
                {
                    return false;
                }
            }
        }


        public Agent()
        {
            sinkNode = null;
            OldAgent = null;
            RootNode = null;
            NewAgent = null;
        }


        public Agent(Sensor sink, Sensor root, Sensor oldagent, Sensor self)
        {
            try { 
            sinkNode = sink;
            OldAgent = oldagent;
            NewAgent = null;
            RootNode = root;
            Node = self;
            if (AgentBuffer == null)
            {
                AgentBuffer = new Queue<Packet>();
            }
            if (SinkOutOfRangeTimer.IsEnabled)
            {
                SinkOutOfRangeTimer.Stop();
            }
            self.isSinkAgent = true;
            NeighborsTableEntry sinkEntry = new NeighborsTableEntry();
            sinkEntry.NeiNode = PublicParameters.SinkNode;
            int index = self.NeighborsTable.FindIndex(item => item.ID == PublicParameters.SinkNode.ID);
            if (!(index >= 0))
            {
                self.NeighborsTable.Add(sinkEntry);
                self.Ellipse_HeaderAgent_Mark.Stroke = new SolidColorBrush(Colors.Black);
                self.MainWindow.Dispatcher.Invoke(() => self.Ellipse_HeaderAgent_Mark.Visibility = Visibility.Visible);
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void ChangeAgentFM(Sensor newAgent)
        {
            try
            {
                Node.AgentNode.sinkNode = null;
                Node.AgentNode.OldAgent = null;
                Node.isSinkAgent = false;
                Node.MainWindow.Dispatcher.Invoke(() => Node.Ellipse_HeaderAgent_Mark.Visibility = Visibility.Hidden, DispatcherPriority.Send);
                Node.NeighborsTable.RemoveAll(sinkItem => sinkItem.NeiNode.ID == PublicParameters.SinkNode.ID);
                Node.AgentNode.NewAgent = newAgent;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
           
        }



        private void initiateSinkOutOfRangeTimer()
        {
            if (!SinkOutOfRangeTimer.IsEnabled)
            {
                SinkOutOfRangeTimer.Interval = TimeSpan.FromSeconds(1);
                SinkOutOfRangeTimer.Start();
                SinkOutOfRangeTimer.Tick += OutOfRangeTimer_Tick;
                waitingTimesForSink = 0;
            }

        }


        private int waitingTimesForSink { get; set; }


        private void OutOfRangeTimer_Tick(object sender, EventArgs e)
        {
            if (waitingTimesForSink > 7)
            {
                Node.isSinkAgent = false;
                PublicParameters.SinkNode.MainWindow.Dispatcher.Invoke(() => Node.Ellipse_HeaderAgent_Mark.Visibility = Visibility.Hidden);
                Node.NeighborsTable.RemoveAll(sinkItem => sinkItem.NeiNode.ID == PublicParameters.SinkNode.ID);
                SinkOutOfRangeTimer.Stop();
                if (AgentBuffer.Count > 0)
                {
                    Console.WriteLine("Emptying Agent Buffer");
                    do
                    {
                        Packet packet = AgentBuffer.Dequeue();
                        packet.isDelivered = false;
                        Node.updateStates(packet);
                    } while (AgentBuffer.Count > 0);
                }

            }
            else if (AgentBuffer.Count > 0)
            {
                if (isSinkInRange())
                {
                    SinkOutOfRangeTimer.Stop();
                    Console.WriteLine("Emptying Agent Buffer 2");
                    do
                    {
                        Packet packet = AgentBuffer.Dequeue();
                        packet.Destination = PublicParameters.SinkNode;
                        packet.TimeToLive += PublicParameters.HopsErrorRange;
                        Node.sendDataPack(packet);
                    } while (AgentBuffer.Count > 0);
                }
                else
                {
                    if (NewAgent != null)
                    {
                        Console.WriteLine("Emptying Agent Buffer 3");
                        do
                            {
                                Packet packet = AgentBuffer.Dequeue();
                                packet.Destination = NewAgent;
                                packet.TimeToLive += Node.maxHopsForDestination(NewAgent);
                                Node.sendDataPack(packet);
                            } while (AgentBuffer.Count > 0);

                    }
                    else
                    {
                        Console.WriteLine("Emptying Agent Buffer 4");
                        do
                        {
                            Packet packet = AgentBuffer.Dequeue();
                            packet.isDelivered = false;
                            packet.DroppedReason = "Agent Buffer";
                            PublicParameters.SinkNode.updateStates(packet);
                        } while (AgentBuffer.Count > 0);
                    
                    }
                   

                }
            }
            else if (isSinkInRange() && AgentBuffer.Count == 0)
            {
                SinkOutOfRangeTimer.Stop();
            }

            waitingTimesForSink++;
        }



        public void AgentStorePacket(Packet packet)
        {
            initiateSinkOutOfRangeTimer();
            AgentBuffer.Enqueue(packet);

        }

        public bool isSinkInRange()
        {
            double distance = Operations.DistanceBetweenTwoSensors(PublicParameters.SinkNode, Node);
            double offset = PublicParameters.CommunicationRangeRadius;
            if (distance >= offset)
            {
                return false;
            }
            else
            {
                return true;
            }
        }






    }
}
