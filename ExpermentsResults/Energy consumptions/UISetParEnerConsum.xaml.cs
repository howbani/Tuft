using Tuft.Dataplane;
using Tuft.Properties;
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
using System.Windows.Shapes;
using Tuft.Models.MobileSink;
using Tuft.ControlPlane.DistributionWeights;

namespace Tuft.ExpermentsResults.Energy_consumptions
{
    /// <summary>
    /// Interaction logic for UISetParEnerConsum.xaml
    /// </summary>
    public partial class UISetParEnerConsum : Window
    {
        MainWindow _MainWindow;
        public UISetParEnerConsum(MainWindow __MainWindow_)
        {
            InitializeComponent();
            _MainWindow = __MainWindow_;

            try
            {
                com_queueTime.Items.Add("0.1");
                com_queueTime.Items.Add("0.2");
                com_queueTime.Items.Add("0.3");
                com_queueTime.Items.Add("0.4");
                com_queueTime.Items.Add("0.5");
                com_queueTime.Items.Add("0.6");
                com_queueTime.Items.Add("0.7");
                com_queueTime.Items.Add("0.8");
                com_queueTime.Items.Add("0.9");
                com_queueTime.Items.Add("1");
                com_queueTime.Items.Add("2");
                com_queueTime.Items.Add("3");
                com_queueTime.Items.Add("4");
                com_queueTime.Items.Add("5");


                for (int i = 5; i <= 50; i++)
                {
                    com_UpdateLossPercentage.Items.Add(i);
                }

                for (int j = 0; j <= 9; j++)
                {
                    string str = "0." + j;
                    double dc = Convert.ToDouble(str);
                    com_D.Items.Add(dc);
                    com_H.Items.Add(dc);
                    com_L.Items.Add(dc);
                    com_R.Items.Add(dc);
                    com_Dir.Items.Add(dc);
                }


                for (int j = 1; j <= 10; j++)
                {

                    com_D.Items.Add(j);
                    com_H.Items.Add(j);
                    com_L.Items.Add(j);
                    com_R.Items.Add(j);
                    com_Dir.Items.Add(j);
                }

                com_queueTime.Text = Settings.Default.QueueTime.ToString();
             
                com_L.Text = Settings.Default.ExpoEPCnt.ToString();
                com_R.Text = Settings.Default.ExpoPerpCnt.ToString();
                com_D.Text = Settings.Default.ExpoEucCnt.ToString();
                com_Dir.Text = Settings.Default.ExpoDirCnt.ToString();


                
            }
            catch
            {
                MessageBox.Show("Error!!!.");
            }

            com_UpdateLossPercentage.Text = Settings.Default.UpdateLossPercentage.ToString();
            Settings.Default.ShowRoutingPaths = false;
            //Settings.Default.SaveRoutingLog = false;
            Settings.Default.ShowAnimation = false;
            Settings.Default.ShowRadar = false;


            for (int i = 60; i <= 5000; i = i + 60)
            {
                comb_simuTime.Items.Add(i);
               
            }
            comb_simuTime.Text = "300";

            comb_packet_rate.Items.Add("0.1");
            comb_packet_rate.Items.Add("0.2");
            comb_packet_rate.Items.Add("0.3");
            comb_packet_rate.Items.Add("0.4");
            comb_packet_rate.Items.Add("0.5");
            comb_packet_rate.Items.Add("0.8");
            for (int i = 1; i <= 5; i++)
            {
                comb_packet_rate.Items.Add(i);
            }

            comb_packet_rate.Text = "1";

            for(int i=5;i<=15;i++)
            {
                comb_startup.Items.Add(i);
            }
            comb_startup.Text = "5";

            for(int i=1;i<=5;i++)
            {
                comb_active.Items.Add(i);
                comb_sleep.Items.Add(i);
            }
            comb_active.Text = "1";
            comb_sleep.Text = "2";


            for(double i=0.1; i <= 1; i +=0.1)
            {
                i = Math.Round(i, 1);
                com_InitialBattery.Items.Add(i);
            }
            com_InitialBattery.Text = "0.5";
                

             for (int i=0;i<=25;i++)
            {
                
                comb_sink_speed.Items.Add(i);

            }
             for (int i = 50; i <= 1000; i = i + 50)
             {

                 comb_simuPacket.Items.Add(i);
             }
             comb_simuPacket.Text = "500";
             comb_sink_speed.Text = "5";
            int conrange = 5;
            for (int i = 0; i <= conrange; i++)
            {
                if (i == conrange)
                {
                    double dc = Convert.ToDouble(i);
                   
                }
                else
                {
                    for (int j = 0; j <= 9; j++)
                    {
                        string str = i + "." + j;
                        double dc = Convert.ToDouble(str);
                       

                    }
                }
            }

        



        }


        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {

            Settings.Default.UpdateLossPercentage = Convert.ToInt16(com_UpdateLossPercentage.Text);
            Settings.Default.DrawPacketsLines = Convert.ToBoolean(chk_drawrouts.IsChecked);
            Settings.Default.KeepLogs= Convert.ToBoolean(chk_save_logs.IsChecked);
            Settings.Default.StopeWhenFirstNodeDeid = Convert.ToBoolean(chk_stope_when_first_node_deis.IsChecked);
            Settings.Default.QueueTime = Convert.ToDouble(com_queueTime.Text);
            Settings.Default.BatteryIntialEnergy = Convert.ToDouble(com_InitialBattery.Text);
            //PublicParameters.BatteryIntialEnergy = Settings.Default.BatteryIntialEnergy;


           if (TakePackets)
            {
                int stime = 100000000;
                _MainWindow.stopSimlationWhen = stime;
                double packetRate = Convert.ToDouble(comb_packet_rate.Text);
                int numberOfPacket = Convert.ToInt32(comb_simuPacket.Text);
                Settings.Default.StopByPacketNum = numberOfPacket;
                Settings.Default.StopByNumberOfPackets = true;
                _MainWindow.RandomDeplayment(0);
               // double numpackets = Convert.ToDouble(stime) / packetRate;
                _MainWindow.PacketRate = "1 packet per " + packetRate + " s";
                _MainWindow.SendPackectPerSecond(packetRate);
                //StartGenerating();

            }

            else if (Settings.Default.StopeWhenFirstNodeDeid == false)
            {
                int stime = Convert.ToInt16(comb_simuTime.Text);
                Settings.Default.StopByNumberOfPackets = false;
                double packetRate = Convert.ToDouble(comb_packet_rate.Text);
                _MainWindow.stopSimlationWhen = stime;
                _MainWindow.RandomDeplayment(0);
                double numpackets = Convert.ToDouble(stime) / packetRate;
                _MainWindow.SendPackectPerSecond(packetRate);
                _MainWindow.PacketRate = "1 packet per " + packetRate + " s";
            }
            else if (Settings.Default.StopeWhenFirstNodeDeid == true)
            {
                Settings.Default.StopByNumberOfPackets = false;
                int stime = 100000000;
                double packper = Convert.ToDouble(comb_packet_rate.Text);
                _MainWindow.stopSimlationWhen = stime;
                _MainWindow.RandomDeplayment(0);
                _MainWindow.SendPackectPerSecond(packper);
            }
            Close();
        }

        private void comb_startup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_startup.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            Settings.Default.MacStartUp = va;
        }

        private void comb_active_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_active.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            Settings.Default.ActivePeriod = va;
        }

        private void comb_sleep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval = comb_sleep.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            Settings.Default.SleepPeriod = va;
        }

        private void chk_stope_when_first_node_deis_Checked(object sender, RoutedEventArgs e)
        {
            comb_simuTime.IsEnabled = false;
        }

        private void chk_stope_when_first_node_deis_Unchecked(object sender, RoutedEventArgs e)
        {
            comb_simuTime.IsEnabled = true;
        }

      

        private void chk_drawrouts_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRoutingPaths = true;
        }

        private void chk_drawrouts_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRoutingPaths = false;
        }

        private void chk_save_logs_Checked(object sender, RoutedEventArgs e)
        {
           // Settings.Default.SaveRoutingLog = true;
        }

        private void chk_save_logs_Unchecked(object sender, RoutedEventArgs e)
        {
           // Settings.Default.SaveRoutingLog = false;
        }

        private void chek_show_radar_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRadar = true;
        }

        private void chek_show_radar_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowRadar = false;
        }

        private void chek_animation_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowAnimation = true;
        }

        private void chek_animation_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowAnimation = false;
        }

        private void comb_sink_speed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object objval =  comb_sink_speed.SelectedItem as object;
            int va = Convert.ToInt16(objval);
            if (va == 0)
            {
                MobileModel.isSinkStatic = true;
            }
            Settings.Default.SinkSpeed = va;
        }

        private void btn_changeWeights_Click(object sender, RoutedEventArgs e)
        {
            FillWeightsMatrix mat = new FillWeightsMatrix();
            mat.Show();

        }

        private static bool TakePackets = true;
        private void chk_take_packets_Check(object sender, RoutedEventArgs e)
        {
            CheckBox a = sender as CheckBox;
            if (!a.IsChecked.Equals(true))
            {
                comb_simuTime.IsEnabled = true;
                comb_simuPacket.IsEnabled = false;
                chk_stope_when_first_node_deis.IsEnabled = true;
                TakePackets = false;
            }
            else if (!TakePackets)
            {
                comb_simuTime.IsEnabled = false;
                comb_simuPacket.IsEnabled = true;
                chk_stope_when_first_node_deis.IsEnabled = false;
                TakePackets = true;
            }

        }
    }
}
