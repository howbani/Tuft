using Tuft.Dataplane;
using Tuft.Dataplane.NOS;
using Tuft.Dataplane.PacketRouter;
using Tuft.Intilization;
using Tuft.Properties;
using Tuft.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tuft.ControlPlane.NOS.FlowEngin
{
    public class MiniFlowTableSorterDownLinkPriority : IComparer<FlowTableEntry>
    {

        public int Compare(FlowTableEntry y, FlowTableEntry x)
        {
            return x.DownLinkPriority.CompareTo(y.DownLinkPriority);
        }
    }


    public class LinkFlowEnery
    {
        public Sensor Current { get; set; }
        public Sensor Next { get; set; }
        public Sensor Target { get; set; }

        // Elementry values:
        public double D { get; set; } // direction value tworads the end node
        public double DN { get; set; } // R NORMALIZEE value of To. 
        public double DP { get; set; } // defual.

        public double L { get; set; } // remian energy
        public double LN { get; set; } // L normalized
        public double LP { get; set; } // L value of To.

        public double R { get; set; } // riss
        public double RN { get; set; } // R NORMALIZEE value of To. 
        public double RP { get; set; } // R NORMALIZEE value of To. 

        //Perpendicular Distance
        public double pirDis { get; set; }
        public double pirDisNorm { get; set; }



        //
        public double Pr
        {
            get;
            set;
        }

        // return:
        public double Mul
        {
            get
            {
                return LP * DP * RP;
            }
        }

        public int IindexInMiniFlow { get; set; }
        public FlowTableEntry FlowTableEntry { get; set; }
    }



    public class LinkRouting
    {
        public static double srcPerDis { get; set; }

        public static FlowTableEntry getBiggest(List<FlowTableEntry> table)
        {
            double offset = -10;
            FlowTableEntry biggest = null;
            foreach (FlowTableEntry entry in table)
            {                
                if (entry.DownLinkPriority > offset)
                {
                    offset = entry.DownLinkPriority;
                    biggest = entry;
                }

            }
            return biggest;
        }
        public static void sortTable(Sensor sender)
        {

            List<FlowTableEntry> beforeSort = sender.TuftFlowTable;
            List<FlowTableEntry> afterSort = new List<FlowTableEntry>();
            do
            {
                FlowTableEntry big = getBiggest(beforeSort);
                if (big != null)
                {
                    afterSort.Add(big);
                    beforeSort.Remove(big);
                }
                else
                {                
                    beforeSort.Clear();
                    return;
                }
                if (afterSort.Count > 20)
                {
                    beforeSort.Clear();
                    return;
                }

            } while (beforeSort.Count > 0);
            sender.TuftFlowTable.Clear();
            sender.TuftFlowTable = afterSort;

        }
        /// <summary>
        /// This will be change per sender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="endNode"></param>


        public static void GetD_Distribution(Sensor sender, Packet packet)
        {
            List<int> path = Operations.PacketPathToIDS(packet.Path);
            sender.TuftFlowTable.Clear();
            List<int> PacketPath = Operations.PacketPathToIDS(packet.Path);

            Sensor sourceNode;// = packet.Source;
            Point endNodePosition;
            if (packet.PacketType == PacketType.QReq)
            {
                if (packet.isQreqRouted)
                {
                    sourceNode = packet.ReRouteSource;
                }
                else
                {
                    sourceNode = packet.Source;
                }
            }
            else
            {
                sourceNode = packet.Source;
            }
            if (packet.PacketType == PacketType.Data || packet.PacketType == PacketType.Control)
            {
                endNodePosition = packet.Destination.CenterLocation;
            }
            
            else if (packet.PacketType == PacketType.QReq)
            {
                if (packet.isQreqInsideCell)
                {
                    endNodePosition = packet.Destination.CenterLocation;
                }
                else
                {
                    endNodePosition = packet.DestinationAddress;
                }
              
            }
            else
            {
                if (packet.Destination != null)
                {
                    endNodePosition = packet.Destination.CenterLocation;
                }
                else
                {
                    return;
                }
                                         
            }

            double distSrcToEnd = Operations.DistanceBetweenTwoPoints(sender.CenterLocation, endNodePosition);
            double ENDifference = distSrcToEnd - sender.ComunicationRangeRadius;

            double n = Convert.ToDouble(sender.NeighborsTable.Count) + 1;

            foreach (NeighborsTableEntry neiEntry in sender.NeighborsTable)
            {
                if (neiEntry.NeiNode.ResidualEnergyPercentage > 0)
                {
                    if (neiEntry.ID != PublicParameters.SinkNode.ID)
                    {
                        FlowTableEntry MiniEntry = new FlowTableEntry();
                        MiniEntry.SID = sender.ID;
                        MiniEntry.NeighborEntry = neiEntry;

                       // MiniEntry.NeighborEntry.Direction = Operations.GetDirectionAngle(sender.CenterLocation, MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                       
                        MiniEntry.NeighborEntry.DirProb = Operations.GetAngleDotProduction(sender.CenterLocation, MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                        if (MiniEntry.NeighborEntry.DirProb < 0)
                        {
                            MiniEntry.NeighborEntry.DirProb = 0;
                        }
                        MiniEntry.NeighborEntry.pirDisProb = Operations.GetPerpendicularProbability(sourceNode.CenterLocation, MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                        MiniEntry.NeighborEntry.TransDisProb = Operations.GetTransmissionDistanceProbability(MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                        MiniEntry.NeighborEntry.ResEnergyProb = Operations.GetResidualEnergyProbability(MiniEntry.NeighborEntry.NeiNode.ResidualEnergy);

                        MiniEntry.NeighborEntry.pirDis = Operations.GetPerpindicularDistance(sourceNode.CenterLocation, MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                       
                        MiniEntry.NeighborEntry.pirDisNorm =  (MiniEntry.NeighborEntry.pirDis / PublicParameters.CommunicationRangeRadius);
                        MiniEntry.NeighborEntry.RE = MiniEntry.NeighborEntry.ResEnergyProb;
                        MiniEntry.NeighborEntry.EDNorm = MiniEntry.NeighborEntry.TransDisProb;
                        MiniEntry.NeighborEntry.Direction = MiniEntry.NeighborEntry.DirProb;
                       
                        sender.TuftFlowTable.Add(MiniEntry);
                    }                   
                }
            }

            double RESum = 0;
            double TDSum = 0;
            double DirSum = 0;
            double PirSum = 0;
         
            sender.CW.getDynamicWeight(packet, sender);
            foreach (FlowTableEntry MiniEntry in sender.TuftFlowTable)
            {
                MiniEntry.NeighborEntry.TransDisProb *= sender.CW.TDWeight;
                if (MiniEntry.NeighborEntry.DirProb < 0)
                {
                    MiniEntry.NeighborEntry.DirProb = 0;
                }
                else
                {
                    MiniEntry.NeighborEntry.DirProb *= sender.CW.DirWeight;
                }
              
                
                MiniEntry.NeighborEntry.pirDisProb *= sender.CW.PirpWeight;
                MiniEntry.NeighborEntry.ResEnergyProb *= sender.CW.EnergyWeight;
            }

         
            foreach (FlowTableEntry MiniEntry in sender.TuftFlowTable)
            {
                if (MiniEntry.NID != PublicParameters.SinkNode.ID)
                {
                    RESum += MiniEntry.NeighborEntry.ResEnergyProb;
                    TDSum += MiniEntry.NeighborEntry.TransDisProb;
                    DirSum += MiniEntry.NeighborEntry.DirProb;
                    PirSum += MiniEntry.NeighborEntry.pirDisProb;
                }
               
            }
           
            double downLinkSum =0;
            foreach (FlowTableEntry MiniEntry in sender.TuftFlowTable)
            {
                if (MiniEntry.NID != PublicParameters.SinkNode.ID)
                {
                    MiniEntry.NeighborEntry.TransDisProb /=TDSum;
                    if (MiniEntry.NeighborEntry.DirProb < 0)
                    {
                        MiniEntry.NeighborEntry.DirProb = 0;
                    }
                    else if(DirSum !=0)
                    {
                        MiniEntry.NeighborEntry.DirProb /= DirSum;
                    }
                 
                    MiniEntry.NeighborEntry.ResEnergyProb /= RESum;
                    MiniEntry.NeighborEntry.pirDisProb /=PirSum;
                    /*  if (packet.PacketType == PacketType.QReq || packet.PacketType == PacketType.QResp)
                      {
                          MiniEntry.NeighborEntry.pirDisProb = 0;
                      }
                      */
                    if (endNodePosition != packet.Source.CenterLocation)
                    {
                        MiniEntry.DownLinkPriority = (MiniEntry.NeighborEntry.TransDisProb + MiniEntry.NeighborEntry.DirProb + MiniEntry.NeighborEntry.ResEnergyProb + MiniEntry.NeighborEntry.pirDisProb) / 4;
                    }
                    else
                    {
                        MiniEntry.DownLinkPriority = (MiniEntry.NeighborEntry.TransDisProb + MiniEntry.NeighborEntry.DirProb + MiniEntry.NeighborEntry.ResEnergyProb) / 3;
                    }
                    downLinkSum += MiniEntry.DownLinkPriority;

                }                             
            }


         
            sortTable(sender);

            //int a = packet.Hops;
            //a ++;  
           
                double average = 1 / Convert.ToDouble(sender.TuftFlowTable.Count);
                int Ftheashoeld = Convert.ToInt16(Math.Ceiling(Math.Sqrt(Math.Sqrt(n)))); // theshold.
                int forwardersCount = 0;
                int minus;
                if (path.Count < 2)
                {
                    minus = 1;
                }
                else
                {
                    minus = 2;
                }
                int lastForwarder = path[path.Count - minus];


            foreach (FlowTableEntry MiniEntry in sender.TuftFlowTable)
            {

                double dir = Operations.GetDirectionAngle(sender.CenterLocation, MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                double dist = Operations.DistanceBetweenTwoPoints(sender.CenterLocation, endNodePosition);
                double distCand = Operations.DistanceBetweenTwoPoints(MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                if (MiniEntry.DownLinkPriority >= average && forwardersCount <= Ftheashoeld)// && MiniEntry.NID != lastForwarder)

                {
                    if (!path.Contains(MiniEntry.NID))
                    {
                        MiniEntry.DownLinkAction = FlowAction.Forward;
                        forwardersCount++;
                    }
                    else if (dir < 0.2 && (distCand < dist))
                    {
                        MiniEntry.DownLinkAction = FlowAction.Forward;
                        forwardersCount++;
                    }
                    else
                    {
                        MiniEntry.DownLinkAction = FlowAction.Drop;
                    }


                }
                else
                {
                    MiniEntry.DownLinkAction = FlowAction.Drop;
                }


            }

            if (forwardersCount == 0)
            {
                foreach (FlowTableEntry MiniEntry in sender.TuftFlowTable)
                {
                    double srcEnd = Operations.DistanceBetweenTwoPoints(sender.CenterLocation, endNodePosition);
                    double candEnd = Operations.DistanceBetweenTwoPoints(MiniEntry.NeighborEntry.CenterLocation, endNodePosition);
                    if (MiniEntry.DownLinkPriority >= average && forwardersCount <= Ftheashoeld)
                    {
                        MiniEntry.DownLinkAction = FlowAction.Forward;
                        forwardersCount++;
                    }
                    else
                    {
                        MiniEntry.DownLinkAction = FlowAction.Drop;
                    }

                }
            }





        }

         
    }
}
