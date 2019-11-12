using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Tuft.Dataplane;
using Tuft.Dataplane.NOS;

namespace Tuft.Models.Cell
{
    public class CellHeader
    {
        //Cell Header Main Variables
        public bool isHeader = false;
        public Point ParentCellCenter { get; set; }        
        public bool hasSinkPosition = false;
        public Sensor SinkAgent { get; set; }
        public int atTreeDepth { get; set; }
        public double DistanceFromRoot { get { return PublicParameters.cellRadius * atTreeDepth; } }
        public bool isRootHeader = false;

        private Sensor me { get; set; }
        public bool isNewHeaderAvail = false;
        private DispatcherTimer OldHeaderTimer;

        //Cell Header Buffer
        public Queue<Packet> CellHeaderBuffer = new Queue<Packet>();
        public void StoreInCellHeaderBuffer(Packet packet)
        {
            CellHeaderBuffer.Enqueue(packet);
        }

        public void DidChangeHeader(Sensor m)
        {
            isNewHeaderAvail = true;
           // OldHeaderTimer = new DispatcherTimer();
           // OldHeaderTimer.Interval = TimeSpan.FromSeconds(3);
          //  OldHeaderTimer.Start();
            me = m;
           // OldHeaderTimer.Tick += OldHeaderTimer_Tick;
            hasSinkPosition = false;
            if (CellHeaderBuffer.Count > 0)
            {
                me.ReRoutePacketsInCellHeaderBuffer();
            }
            ClearData();
            
        }

        void OldHeaderTimer_Tick(object sender, EventArgs e)
        {
            Sensor.needtoCheck = true;
            if (CellHeaderBuffer.Count > 0)
            {
                me.ReRoutePacketsInCellHeaderBuffer();
            }
            isNewHeaderAvail = false;
            ClearData();
            OldHeaderTimer.Stop();
            OldHeaderTimer = null;

        }
       private void ClearData(){
             isHeader = false;     
             hasSinkPosition = false;
             SinkAgent =null;
             isRootHeader = false;
        }
        



    }
}
