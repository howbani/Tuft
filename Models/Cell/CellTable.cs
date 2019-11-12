using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tuft.Dataplane;

namespace Tuft.Models.Cell
{
    public class CellTable
    {
        public Sensor CellHeader { get; set; }

        public Point HeaderLocation { get; set; }

        public bool isRootCell = false;


        public CellTable()
        {

        }
        public CellTable(Sensor header, Point x)
        {
            CellHeader = header;
            HeaderLocation = x;

        }

    }
}
