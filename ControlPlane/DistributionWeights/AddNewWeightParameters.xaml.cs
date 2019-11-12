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
using Tuft.Dataplane;

namespace Tuft.ControlPlane.DistributionWeights
{
    /// <summary>
    /// Interaction logic for AddNewWeightParameters.xaml
    /// </summary>
    public partial class AddNewWeightParameters : Window
    {
        private static List<string> addedWeightsHolder = new List<string>();
        public AddNewWeightParameters()
        {
            InitializeComponent();
        }

        #region AddtheDistributions
        private void btn_click_add(object sender, RoutedEventArgs e)
        {
            string str = txt_input.Text;
            if (str.Count() < 1)
            {
                MessageBox.Show("Input Parameter name in the text box");
            }
            else
            {
                addedWeightsHolder.Add(str);
            }
            txt_input.Text = "";
        }
        private void btn_click_done(object sender, RoutedEventArgs e)
        {
            if (addedWeightsHolder.Count > 0)
            {
                PublicParameters.WeightParameters.Clear();
                PublicParameters.WeightParameters = addedWeightsHolder;
            }
            AddNewWeightParameters adn = new AddNewWeightParameters();
            Close();
            adn.Show();
        }

        #endregion
    }
}
