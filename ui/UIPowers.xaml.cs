﻿using Tuft.Dataplane;
using Tuft.Properties;
using System;
using System.Windows;

namespace Tuft.ui
{
    /// <summary>
    /// Interaction logic for UIPowers.xaml
    /// 
    /// </summary>
    public partial class UIPowers : Window
    {
        MainWindow __MainWindow;
        public UIPowers(MainWindow _MainWindow)
        {
            InitializeComponent();
            __MainWindow = _MainWindow;
            try
            {
                com_intail_energy.Items.Add("0.01");
                com_intail_energy.Items.Add("0.05");
                com_intail_energy.Items.Add("0.1");
                com_intail_energy.Items.Add("0.5");
                com_intail_energy.Items.Add("0.6");
                com_intail_energy.Items.Add("1");
                com_intail_energy.Items.Add("2");
                com_intail_energy.Items.Add("5");
                com_intail_energy.Items.Add("10");
                com_intail_energy.Items.Add("20");



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
                

                for (int i = 5; i <= 15; i++)
                {
                    comb_startup.Items.Add(i);
                }
                comb_startup.Text = "10";


                for (int i = 1; i <= 5; i++)
                {
                    comb_active.Items.Add(i);
                    comb_sleep.Items.Add(i);
                }
                comb_active.Text = "1";
                comb_sleep.Text = "2";


                for (int j = 0; j <= 9; j++)
                {
                    string str = "0." + j;
                    double dc = Convert.ToDouble(str);
                    com_Dir.Items.Add(dc);
                    com_Euc.Items.Add(dc);
                    com_EP.Items.Add(dc);
                    com_Perp.Items.Add(dc);
                    //com_Dir.Items.Add(dc);
                }


                for (int j = 1; j <=10; j++)
                {
                   
                    com_Dir.Items.Add(j);
                    com_Euc.Items.Add(j);
                    com_EP.Items.Add(j);
                    com_Perp.Items.Add(j);
                    com_Dir.Items.Add(j);
                }

                

                com_Dir.Text = Settings.Default.ExpoDirCnt.ToString();
                com_Euc.Text = Settings.Default.ExpoEucCnt.ToString();
                com_EP.Text = Settings.Default.ExpoEPCnt.ToString();
                com_Perp.Text = Settings.Default.ExpoPerpCnt.ToString();
               // com_Dir.Text = Settings.Default.ExpoECnt.ToString();

                com_UpdateLossPercentage.Text = Settings.Default.UpdateLossPercentage.ToString();

                com_queueTime.Text= Settings.Default.QueueTime.ToString();
                com_intail_energy.Text = Settings.Default.BatteryIntialEnergy.ToString();
                comb_active.Text = Settings.Default.ActivePeriod.ToString();
                comb_sleep.Text = Settings.Default.SleepPeriod.ToString();
                comb_startup.Text = Settings.Default.MacStartUp.ToString();

            }
            catch(Exception e)
            {
                MessageBox.Show("Error!!!." + e.Message);
            }
        }


        
        private void btn_set_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.Default.UpdateLossPercentage = Convert.ToInt16(com_UpdateLossPercentage.Text);
                Settings.Default.SleepPeriod = Convert.ToDouble(comb_sleep.Text);
                Settings.Default.ActivePeriod = Convert.ToDouble(comb_active.Text);
                Settings.Default.BatteryIntialEnergy = Convert.ToDouble(com_intail_energy.Text);
                Settings.Default.MacStartUp= Convert.ToInt16(comb_startup.Text);
                //Settings.Default.QueueTime= Convert.ToInt16(com_queueTime.Text);

             
           //     Settings.Default.ExpoEPCnt = Convert.ToDouble(com_EP.Text);
               // Settings.Default.ExpoDirCnt = Convert.ToDouble(com_Dir.Text);
             //   Settings.Default.ExpoEucCnt= Convert.ToDouble(com_Euc.Text);
                //Settings.Default.ExpoPerpCnt = Convert.ToDouble(com_Perp.Text);

                this.Close();

            }
            catch(Exception ex)
            {
                MessageBox.Show("Error. "+ex.Message);
            }


        }

       
    }
}
