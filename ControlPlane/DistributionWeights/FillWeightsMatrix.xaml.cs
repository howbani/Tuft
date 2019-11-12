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
using Tuft.Properties;

namespace Tuft.ControlPlane.DistributionWeights
{
    public partial class AHP
    {
        public int row { get; set; }
        public string Value { get; set; }
        public double finalValue { get; set; }
        public List<double> rowValues { get; set; }
        public List<double> colValues { get; set; }
        public List<double> averageColValues { get; set; }
        public double rowSum
        {
            get
            {
                double sum = 0;
                foreach (double x in rowValues)
                {
                    sum += x;
                }
                return sum;
            }
        }
        public double colSum
        {
            get
            {
                double sum = 0;
                foreach (double x in colValues)
                {
                    sum += x;
                }
                return sum;
            }
        }
        public double averageSum
        {
            get
            {
                double sum = 0;
                foreach (double x in averageColValues)
                {
                    sum += x;
                }
                return sum;
            }
        }

        public void average()
        {
            foreach (double x in colValues)
            {
                averageColValues.Add(x / colSum);
            }
        }

        public AHP()
        {
            rowValues = new List<double>();
            colValues = new List<double>();
            averageColValues = new List<double>();

        }
    }
    /// <summary>
    /// Interaction logic for FillWeightsMatrix.xaml
    /// </summary>
    public partial class FillWeightsMatrix : Window
    {
        #region variables
        private static List<ComboBox[]> Children = new List<ComboBox[]>();
        private static List<AHP> Matrix = new List<AHP>();
        private static List<string> distributionValues = new List<string>();

        private static string inChangingProcess { get; set; }
        private static bool initialisingPhase = true;
        private static bool isChange = true;
        #endregion

        public FillWeightsMatrix()
        {
            InitializeComponent();
            populateWindow();
            populateInformation();
            initialisingPhase = false;
        }

        private void addDistributionValues()
        {
            foreach (AHP row in Matrix)
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                Label label = new Label();
                label.Content = row.Value;
                Label l = new Label();
                l.Content = row.finalValue;
                panel.Children.Add(label);
                panel.Children.Add(l);
                dist_val.Children.Add(panel);
            }
        }

        private ComboBox getOppositeComboBox(ComboBox cb)
        {
            //var index = Array.FindIndex(myArray, row => row.Author == "xyz");
            int rowNumber = 0;

            int coloumn = 0;
            int counter = 0;
            foreach (ComboBox[] row in Children)
            {
                if (row.Contains(cb))
                {
                    var index = Array.FindIndex(row, c => c.Name == cb.Name);
                    rowNumber = counter;
                    coloumn = (int)index;

                }
                counter++;
            }
            ComboBox opp = Children[coloumn][rowNumber];
            return opp;

        }

        private void populateWindow()
        {
            StackPanel names = new StackPanel();
            names.Orientation = Orientation.Horizontal;
            Label name = new Label();
            name.Content = "(i,j)";
            name.Width = 50;
            names.Children.Add(name);
            foreach (string val in PublicParameters.WeightParameters)
            {
                Label a = new Label();
                a.Content = val;
                a.Width = 50;
                names.Children.Add(a);
            }
            Boxes_Container.Children.Add(names);
            int row = 1;

            foreach (string val in PublicParameters.WeightParameters)
            {
                int col = 1;
                StackPanel stk = new StackPanel();
                stk.Orientation = Orientation.Horizontal;
                Label l = new Label();
                l.Content = val;
                l.Width = 50;
                stk.Children.Add(l);
                for (int i = 0; i < PublicParameters.WeightParameters.Count; i++)
                {

                    ComboBox cb = new ComboBox();
                    cb.Width = 50;
                    cb.Name = "a" + row + col;
                    if (row == col)
                    {
                        cb.SelectedIndex = 0;
                        cb.IsEnabled = false;

                    }
                    //ComboBox Width="50" Name="a11" SelectedIndex="0"/>
                    col++;
                    stk.Children.Add(cb);
                }
                row++;
                Boxes_Container.Children.Add(stk);
            }
        }
      
        private void populateInformation()
        {
            StackPanel labels = Boxes_Container.Children[0] as StackPanel;

            List<StackPanel> children = new List<StackPanel>();

            for (int i = 1; i < labels.Children.Count; i++)
            {
                Label l = labels.Children[i] as Label;
                string str = l.Content as string;
                distributionValues.Add(str);
            }



            for (int x = 1; x < Boxes_Container.Children.Count; x++)
            {
                StackPanel child = Boxes_Container.Children[x] as StackPanel;
                ComboBox[] ax = new ComboBox[(child.Children.Count - 1)];
                int counter = 0;
                for (int i = 1; i < child.Children.Count; i++)
                {
                    counter = i;
                    counter--;
                    ax[counter] = child.Children[i] as ComboBox;

                }
                Children.Add(ax);
            }



            foreach (ComboBox[] row in Children)
            {
                foreach (ComboBox col in row)
                {
                    col.SelectionChanged += Combo_Box_SelectionChanged;
                    string name = col.Name;
                    if (name[1] == name[2])
                    {
                        col.IsEnabled = false;
                        col.SelectedIndex = 0;
                    }

                    // MessageBox.Show(col.Text);
                    for (int i = 1; i <= 10; i++)
                    {
                        col.Items.Add(i);
                    }

                }

            }
        }

        private void Combo_Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialisingPhase && isChange)
            {
                //e.addeditems produces the new thingy
                ComboBox selected = sender as ComboBox;
                ComboBox opposite = getOppositeComboBox(selected);
                if (selected.Name != inChangingProcess)
                {
                    inChangingProcess = opposite.Name;
                    string name = selected.Name;
                    if (selected.SelectedIndex == 0 && !opposite.IsEnabled)
                    {
                        if (opposite.SelectedIndex == 10)
                        {
                            opposite.Items.RemoveAt(10);
                        }
                        opposite.SelectedIndex = 0;
                        opposite.IsEnabled = true;
                    }
                    else if (selected.SelectedIndex == 0 && opposite.IsEnabled)
                    {
                        opposite.SelectedIndex = 0;
                    }
                    else
                    {

                        if (opposite.SelectedIndex == 10)
                        {
                            isChange = false;
                            opposite.SelectedIndex = 0;
                            opposite.Items.RemoveAt(10);

                        }
                        int value = selected.SelectedIndex;
                        value++;
                        double newVal = (double)1 / (double)value;
                        if (value != 1)
                        {
                            opposite.Items.Add(newVal);
                            opposite.SelectedIndex = 10;
                            opposite.IsEnabled = false;

                        }

                    }
                }
                inChangingProcess = "";
                isChange = true;
            }

        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            //if selectedindex == -1 then has no value
            //to get the value combobox.SelectedValue
            int i = 1;
            foreach (ComboBox[] row in Children)
            {
                AHP MatrixRow = new AHP();
                int rowIndex = i;
                MatrixRow.row = rowIndex;
                int distVal = rowIndex - 1;
                MatrixRow.Value = distributionValues[distVal];
                for (int y = 0; y < row.Count(); y++)
                {
                    double v = 1;
                    if (row[y].SelectedIndex != -1)
                    {
                        v = Convert.ToDouble(row[y].Text);
                    }
                    //int v = (int)row[y].SelectedValue;
                    MatrixRow.rowValues.Add(v);
                    if ((i - 1) == y)
                    {
                        foreach (ComboBox[] rowForCol in Children)
                        {
                            double holder = 1;
                            if (rowForCol[y].SelectedIndex != -1)
                            {
                                holder = Convert.ToDouble(rowForCol[y].Text);
                            }
                            MatrixRow.colValues.Add(holder);
                        }
                    }
                }
                MatrixRow.average();
                Matrix.Add(MatrixRow);
                i++;
            }

            for (int pointer = 0; pointer < Matrix.Count; pointer++)
            {
                double sum = 0;
                foreach (AHP item in Matrix)
                {
                    sum += item.averageColValues[pointer];
                }
                Matrix[pointer].finalValue = sum / Matrix.Count;

            }
            addDistributionValues();
        }

        private void btn_done_Click(object sender, RoutedEventArgs e)
        {
            double td = Matrix[0].finalValue;
            double Dir = Matrix[1].finalValue;
            double pirp = Matrix[2].finalValue;
            double energy = Matrix[3].finalValue;
            Settings.Default.TDWeight = td;
            Settings.Default.DirWeight = Dir;
            Settings.Default.PirpWeight = pirp;
            Settings.Default.EnergyWeight = energy;
            Close();
        }

        
    }
}
