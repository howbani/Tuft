using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tuft.Dataplane;

namespace Tuft.Models.Cell
{
    public class CellNode
    {
        public bool isEncapsulated = false;
        public int CellNumber { get; set; }
        public Point CellCenter { get; set; }
        public Sensor myCellHeader { get; set; }

        public CellHeader CellHeaderTable = new CellHeader();


        //Regular Nodes
        public Point NearestCellCenter { get; set; }


    }
}
