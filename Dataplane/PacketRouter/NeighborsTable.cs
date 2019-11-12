using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tuft.Dataplane.PacketRouter
{
    /// <summary>
    /// TABLE 2: NEIGHBORING NODES INFORMATION TABLE (NEIGHBORS-TABLE)
    /// </summary>
    public class NeighborsTableEntry
    {
        public int ID { get { return NeiNode.ID; } } // id of candidate.
                                                     // Elementry values:      
        public double RP { get; set; } // RSSI.

        public double RE { get; set; }
        public double ResidualEnegNorm { get; set; }
        public double ResEnergyProb { get; set; } // battry level.
   

      
        // closer to the sender and closer to the sink.
  
        
        // rssi:
     
        public double EuclidieanDistance { get; set; } //  IDRECTION TO THE SINK
        public double EDNorm { get; set; } // // NORMLIZED
        public double TransDisProb { get; set; } // ECLIDIAN DISTANCE

      

        //
        public double Direction { get; set; } // 
        public double DirNorm { get; set; } // D normalized 
        public double DirProb { get; set; } // distance from the me Candidate to target node.


        public double pirDis { get; set; }
        public double pirDisNorm { get; set; }
        public double pirDisProb { get; set; }


        public System.Windows.Point CenterLocation { get { return NeiNode.CenterLocation; } }
        //: The neighbor Node
        public Sensor NeiNode { get; set; }
    }

}
