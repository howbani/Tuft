using Tuft.Dataplane;
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

namespace Tuft.Constructor
{
    /// <summary>
    /// Interaction logic for ClusterCenter.xaml
    /// </summary>
    public partial class CellCenter : UserControl
    {
        public CellCenter(Point p, int clusterID)
        {

            InitializeComponent();
            setPosition(p);
            center_label_text.Text = "";
            center_label_text.Text = clusterID.ToString();
        }
        public void setPosition(Point p)
        {
            try
            {
                double height = center_label.Height / 2;
                double width = center_label.Width / 2;
                Thickness margin = center_label.Margin;
                margin.Top = p.Y - height;
                margin.Left = p.X - width;
                center_label.Margin = margin;



            }
            catch
            {
                Console.WriteLine("adding the center has failed");
            }
        }

    }
}
