using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuft.Dataplane;
using Tuft.Dataplane.NOS;
using Tuft.Intilization;
using Tuft.Properties;

namespace Tuft.ControlPlane.DistributionWeights
{
    public class CaluclateWeights
    {
        private Sensor source { get; set; }
        private double TD { get; set; }
        private double PIR { get; set; }
        private int round { get; set; }
        private List<string> td { get; set; }
        private List<string> pirp { get; set; }
        private List<string> dir { get; set; }
        private List<string> energy { get; set; }
        List<List<string>> Parameters = new List<List<string>>();
        private static double halfCR = PublicParameters.CommunicationRangeRadius;

        public double TDWeight { get; set; }
        public double DirWeight { get; set; }
        public double PirpWeight { get; set; }
        public double EnergyWeight { get; set; }

        
        private void getArea()
        {
            int rounded = Convert.ToInt16(Math.Round((TD / (PublicParameters.CommunicationRangeRadius*2))));
            if (rounded <2)
            {
                round = 1;
                return;
            }
            else
            {
                round = rounded;
                return ;
            }
            
        }

        private void fillToArea()
        {
            switch (round)
            {
                case 1:
                    //Area One Energy Consmuption is needed the most
                    EnergyHighest();
                    break;
                case 2:
                    //Area two check the perpindicular distance here and give out
                    if (PIR >= halfCR)
                    {
                        PirpHighest();

                    }
                    else
                    {
                        DirHighest();
                    }
                    break;
                default:
                    //More than 2 then direction is the most needed here
                    DirHighest();
                    break;
            }
        }

        private void EnergyHighest()
        {
           /*td = new List<string>() { "3", "3" };
           pirp = new List<string>() { "3" };
           dir = new List<string>();
           energy = new List<string>() {"5","5","5","5"};
            * */
            TDWeight = 0.0546085858585859;
            PirpWeight = 0.125315656565657;
            DirWeight = 0.300820707070707;
            EnergyWeight = 0.519255050505051;

        }
        private void PirpHighest()
        {
            TDWeight = 0.115840042310631;
            PirpWeight = 0.304474936827878;
            DirWeight = 0.514265146618088;
            EnergyWeight = 0.0654198742434036;
            /*
            td = new List<string>() { "3"};
            pirp = new List<string>() {"5","5","5","5" };
            dir = new List<string>() { "3", "3" };
            energy = new List<string>();
             */
        }
        private void DirHighest()
        {
            TDWeight =0.096577380952381;
            PirpWeight = 0.226636904761905;
            DirWeight= 0.623958333333333;
            EnergyWeight = 0.052827380952381;
            /*
            td = new List<string>();
            pirp = new List<string>();
            dir = new List<string>();
            energy = new List<string>();
             */
        }
        public void getDynamicWeight(Packet pkt, Sensor sender)
        {
            if (pkt.PacketType == PacketType.Data)
            {
                PIR = Operations.GetPerpindicularDistance(pkt.Source.CenterLocation, pkt.Destination.CenterLocation, sender.CenterLocation);
                TD = Operations.DistanceBetweenTwoSensors(sender, pkt.Destination);
                getArea();
                fillToArea();
            }else if(pkt.PacketType == PacketType.QReq)
            {
                TDWeight = 0.150374511373458;
                DirWeight = 0.590994109471989;
                PirpWeight = 0.0435218310481653;
                EnergyWeight = 0.215109548106388;
            }
            else
            {
                TDWeight = Settings.Default.TDWeight;
                DirWeight = Settings.Default.DirWeight;
                PirpWeight = Settings.Default.PirpWeight;
                EnergyWeight = Settings.Default.EnergyWeight;
            }
         
        }
        

    }
}
