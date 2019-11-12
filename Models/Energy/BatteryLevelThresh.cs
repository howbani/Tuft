using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuft.Dataplane;

namespace Tuft.Models.Energy
{
    public class BatteryLevelThresh
    {
        private Sensor node { get; set; }
        private List<int> Levels = new List<int>();
        private int currentLevel = 10;
        private int oldLevel = 10;
        private double currentBattery { get; set; }


        private void updateLevel(double currentResidual)
        {
            currentBattery = currentResidual;
            int rounded = Convert.ToInt16(Math.Round(currentBattery / 10));
            currentLevel = rounded;
        }

        public bool threshReached(double currentResidual)
        {
            updateLevel(currentResidual);
            if (oldLevel != currentLevel)
            {
                oldLevel = currentLevel;
                return true;
            }
            else
            {
                return false;
            }
        }


       
    }
}
