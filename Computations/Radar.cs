using Tuft.Dataplane;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Tuft.Intilization
{
    public class Radar
    {
        private Line line;
        private int angle = 0;
        private Sensor sens;
        private DispatcherTimer _RotationTimer;

        public Radar(Sensor sen)
        {
            sens = sen;
            DispatcherTimer RotationTimer = new DispatcherTimer();
            _RotationTimer = RotationTimer;
            RotationTimer.Interval = TimeSpan.FromMilliseconds(0.5);
            RotationTimer.Tick += RotationTimer_Tick; ;
            Line ra = new Line();
            ra.Stroke = Brushes.Black;
            ra.Visibility = Visibility.Hidden;
            ra.StrokeThickness = sen.ComunicationRangeRadius * 0.11;
            ra.X1 = sen.CenterLocation.X;
            ra.X2 = sen.CenterLocation.X;
            ra.Y1 = sen.CenterLocation.Y + sen.ComunicationRangeRadius;
            ra.Y2 = sen.CenterLocation.Y;
            PublicParameters.MainWindow.Canvas_SensingFeild.Children.Add(ra);
            line = ra;
        }

        public void StartRadio()
        {
            _RotationTimer.Start();
            sens.Ellipse_Communication_range.Visibility = Visibility.Visible;
            line.Visibility = Visibility.Visible;
        }

        private void RotationTimer_Tick(object sender, EventArgs e)
        {
            angle++;
            angle = angle % 360;
            double x = sens.CenterLocation.X;
            double y = sens.CenterLocation.Y;
            RotateTransform cc = new RotateTransform(angle, x, y);
            line.Dispatcher.Invoke(() => line.RenderTransform = cc);
        }
        public void StopRadio()
        {
            _RotationTimer.Stop();
            line.Visibility = Visibility.Hidden;
            sens.Ellipse_Communication_range.Visibility = Visibility.Hidden;
        }
    }
}