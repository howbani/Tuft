using Tuft.Dataplane;
using Tuft.Dataplane.NOS;
using Tuft.Properties;
using Tuft.ui;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Tuft.ExpermentsResults.Energy_consumptions
{
    class ResultsObject
    {
        public double AverageEnergyConsumption { get; set; }
        public double AverageHops { get; set; }
        public double AverageWaitingTime { get; set; }
        public double AverageRedundantTransmissions { get; set; }
        public double AverageRoutingDistance { get; set; }
        public double AverageTransmissionDistance { get; set; }
        public double AverageEnergyConsumedForControl { get; set; }
    }

    public class ValParPair
    {
        public string Par { get; set; }
        public string Val { get; set; }
    }

    /// <summary>
    /// Interaction logic for ExpReport.xaml
    /// </summary>
    public partial class ExpReport : Window
    {

        public ExpReport(MainWindow _mianWind)
        {
            InitializeComponent();

            List<ValParPair> List = new List<ValParPair>();
            ResultsObject res = new ResultsObject();
            

            double hopsCoun = 0;
            double routingDisEf = 0;
            double avergTransDist = 0;
            foreach (Packet pk in PublicParameters.FinishedRoutedPackets)
            {
                hopsCoun += pk.Hops;
                routingDisEf += pk.RoutingDistanceEfficiency;
                avergTransDist += pk.AverageTransDistrancePerHop;
            }

            double NumberOfGenPackets = PublicParameters.OverallGeneratedPackets;// Convert.ToDouble(PublicParameters.NumberofGeneratedDataPackets) + Convert.ToDouble(PublicParameters.NumberofGeneratedQueryPackets);
            double NumberofDeliveredPacket = PublicParameters.OverallDelieverdPackets;// Convert.ToDouble(PublicParameters.NumberofDeliveredPackets);
            double succesRatio = PublicParameters.DeliveredRatio;

            res.AverageEnergyConsumption = PublicParameters.TotalEnergyConsumptionJoule;

            double averageWaitingTime = Convert.ToDouble(PublicParameters.TotalWaitingTime) / PublicParameters.OverallDelieverdPackets;
            res.AverageWaitingTime = averageWaitingTime;
            double avergaeRedundan = Convert.ToDouble(PublicParameters.TotalReduntantTransmission) / PublicParameters.OverallDelieverdPackets;
            res.AverageRedundantTransmissions = avergaeRedundan;
            res.AverageHops = hopsCoun / PublicParameters.OverallDelieverdPackets;
            res.AverageRoutingDistance = routingDisEf / PublicParameters.OverallDelieverdPackets;
            res.AverageTransmissionDistance = avergTransDist / PublicParameters.OverallDelieverdPackets;
            res.AverageEnergyConsumedForControl = PublicParameters.EnergyComsumedForControlPackets / (PublicParameters.NumberofGeneratedControlPackets);
            List.Add(new ValParPair() {Par="Number of Nodes", Val= _mianWind.myNetWork.Count.ToString() } );
            List.Add(new ValParPair() { Par = "Communication Range Radius", Val = PublicParameters.CommunicationRangeRadius.ToString()+" m"});
            List.Add(new ValParPair() { Par = "Density", Val = PublicParameters.Density.ToString()});
            List.Add(new ValParPair() { Par = "Packet Rate", Val = _mianWind.PacketRate });
            List.Add(new ValParPair() { Par = "Simulation Time", Val = PublicParameters.SimulationTime.ToString() + " s" });
            List.Add(new ValParPair() { Par = "Start up time", Val = PublicParameters.MacStartUp.ToString() + " s" });
            List.Add(new ValParPair() { Par = "Active Time", Val = PublicParameters.Periods.ActivePeriod.ToString() + " s" });
            List.Add(new ValParPair() { Par = "Sleep Time", Val = PublicParameters.Periods.SleepPeriod.ToString() + " s" });
            List.Add(new ValParPair() { Par = "Initial Energy (J)", Val = PublicParameters.BatteryIntialEnergy.ToString() });
            List.Add(new ValParPair() { Par = "Queue Time", Val = PublicParameters.QueueTime.Seconds.ToString() });
            List.Add(new ValParPair() { Par = "Mean Value of Sink speed", Val = Settings.Default.SinkSpeed.ToString() });
            List.Add(new ValParPair() { Par = "Update Energy", Val = PublicParameters.UpdateLossPercentage.ToString() });
            List.Add(new ValParPair() { Par = "Number of Nodes Dissemnating Data", Val = PublicParameters.NumberOfNodesDissemenating.ToString() });
            List.Add(new ValParPair() { Par = "# gen pck", Val = PublicParameters.OverallGeneratedPackets.ToString() });
            List.Add(new ValParPair() { Par = "# Query & Control pck", Val = (PublicParameters.NumberofGeneratedControlPackets).ToString() });
            List.Add(new ValParPair() { Par = "# del pck", Val = PublicParameters.OverallDelieverdPackets.ToString() });
            List.Add(new ValParPair() { Par = "# droped pck", Val = PublicParameters.NumberofDropedPackets.ToString() });
            List.Add(new ValParPair() { Par = "Success %", Val = succesRatio.ToString() });
            List.Add(new ValParPair() { Par = "Droped %", Val = (100 - succesRatio).ToString() });
            List.Add(new ValParPair() { Par = "Number of Delieverd Data packets", Val = PublicParameters.NumberOfDelieveredDataPackets.ToString() });
            List.Add(new ValParPair() { Par = "Average Transmission Distance/Hop", Val = res.AverageTransmissionDistance.ToString() });
            List.Add(new ValParPair() { Par = "Average Waiting Time/path", Val = res.AverageWaitingTime.ToString() });


            List.Add(new ValParPair() { Par = "Total Energy Consumption (J)", Val = res.AverageEnergyConsumption.ToString() });
           







            List.Add(new ValParPair() { Par = "Average Query & Control Energy Consumption (J)", Val = res.AverageEnergyConsumedForControl.ToString() });
            List.Add(new ValParPair() { Par = "Query & Control Energy Consumption Percentage(%)", Val = PublicParameters.ControlPacketsEnergyConsmPercentage.ToString() });
            List.Add(new ValParPair() { Par = "Total Query & Control Energy Consumption (J)", Val = PublicParameters.EnergyComsumedForControlPackets.ToString() });

            //List.Add(new ValParPair() { Par = "cont pck %", Val = PublicParamerters.ControlPacketsPercentage.ToString() });
            List.Add(new ValParPair() { Par = "Average Query Delay in (s)", Val = PublicParameters.AverageQueryDelay.ToString() }); ;
            List.Add(new ValParPair() { Par = "Average Overall Delay in (s)", Val = PublicParameters.AverageTotalDelay.ToString() });



            // PublicParameters.NetworkName
            List.Add(new ValParPair() { Par = "Protocol", Val = "TBP" });
            List.Add(new ValParPair() { Par = "ATopology", Val = PublicParameters.NetworkName.ToString() });
            dg_data.ItemsSource = List;
        }
    }
}
