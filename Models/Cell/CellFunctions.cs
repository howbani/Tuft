using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Tuft.Constructor;
using Tuft.Dataplane;
using Tuft.Intilization;

namespace Tuft.Models.Cell
{
    class CellFunctions
    {

        public static void FillOutsideSensnors()
        {  
            foreach (Sensor sen in PublicParameters.myNetwork)
            {
                if (sen.inCell == -1)
                {
                    //Check the nearest cluster for it
                    double offset = 120;
                    int nearestID = 0;
                    foreach (CellGroup cluster in PublicParameters.networkCells)
                    {
                        double distance = Operations.DistanceBetweenTwoPoints(sen.CenterLocation, cluster.clusterActualCenter);
                        if (distance < offset)
                        {
                            nearestID = cluster.getID();
                            offset = distance;
                        }
                    }
                    sen.TuftNodeTable.NearestCellCenter = CellGroup.getClusterWithID(nearestID).clusterActualCenter;

                }
            }
        
        }

        public static void ChangeCellHeader(Sensor currentHeader)
        {
            CellGroup Cell = CellGroup.getClusterWithID(currentHeader.TuftNodeTable.CellNumber);
            Sensor newHeader = ReassignCellHeader(Cell);
            if (newHeader.ID != currentHeader.ID)
            {
                Cell.CellTable.CellHeader = newHeader;
                PopulateHeaderInformation(Cell);
                ClearOldCellHeader(currentHeader, newHeader);
                PublicParameters.SinkNode.MainWindow.Dispatcher.Invoke(() => currentHeader.Ellipse_HeaderAgent_Mark.Visibility = Visibility.Hidden);
                newHeader.Ellipse_HeaderAgent_Mark.Stroke = new SolidColorBrush(Colors.Red);
                PublicParameters.SinkNode.MainWindow.Dispatcher.Invoke(() => newHeader.Ellipse_HeaderAgent_Mark.Visibility = Visibility.Visible);
                currentHeader.ReRoutePacketsInCellHeaderBuffer();
                currentHeader.TuftNodeTable.CellHeaderTable.DidChangeHeader(currentHeader);
            }
           
           
        }

        public static void ChangeTreeLevels()
        {
            foreach (CellGroup Cell in PublicParameters.networkCells)
            {

                if (Cell.getID() == Tree.rootClusterID)
                {

                    Cell.CellTable.isRootCell = true;
                    Cell.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.isRootHeader = true;

                }
                else
                {
                    Cell.CellTable.isRootCell = false;
                    Cell.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.isRootHeader = false;
                    Cell.CellTable.CellHeader.TuftNodeTable.CellHeaderTable.ParentCellCenter = Cell.parentCluster.clusterActualCenter;
                }
               

            }
        }

        public static void PopulateHeaderInformation(CellGroup Cell)
        {
            Sensor header = Cell.CellTable.CellHeader;
            foreach (Sensor cellNode in Cell.clusterNodes)
            {
                if (header.ID != cellNode.ID)
                {
                    cellNode.TuftNodeTable.myCellHeader = header;
                    cellNode.TuftNodeTable.CellHeaderTable.isHeader = false;
                }
                else
                {
                    cellNode.TuftNodeTable.myCellHeader = header;
                    cellNode.TuftNodeTable.CellHeaderTable.isHeader = true;
                }
                
            }
            
            CellHeader ch = new CellHeader();
            ch.atTreeDepth = Cell.clusterLevel;
            ch.isHeader = true;

            if (Cell.getID() == Tree.rootClusterID)
            {
                Cell.CellTable.isRootCell = true;
                ch.isRootHeader = true;

            }
            else
            {
                ch.ParentCellCenter = Cell.parentCluster.clusterActualCenter;
            }


            header.TuftNodeTable.CellHeaderTable = ch;
        }

        private static void ClearOldCellHeader(Sensor oldHeader,Sensor newHeader)
        {
            newHeader.TuftNodeTable.CellHeaderTable.hasSinkPosition = oldHeader.TuftNodeTable.CellHeaderTable.hasSinkPosition;
            newHeader.TuftNodeTable.CellHeaderTable.SinkAgent = oldHeader.TuftNodeTable.CellHeaderTable.SinkAgent;

    

        }

        

        public static Sensor ReassignCellHeader(CellGroup Cell){

                Sensor holder = null;
                // check according to remaining enery and distance
                double ENorm = 0;
                double DNorm = 0;
                double max = 0;

                foreach (Sensor sen in Cell.clusterNodes)
                {
                    ENorm = sen.ResidualEnergyPercentage;
                    DNorm = (Operations.DistanceBetweenTwoPoints(sen.CenterLocation, Cell.clusterCenterComputed));

                    sen.CellHeaderProbability = (2 * ENorm);// +DNorm;

                    if (sen.CellHeaderProbability > max)
                    {
                        max = sen.CellHeaderProbability;
                        holder = sen;
                    }
                }

                try
                {
                    return holder;
                }
                catch
                {
                    holder = null;
                    return null;
                }
                /*if (holder.ID != Cell.CellTable.CellHeader.ID)
                {
                    Sensor oldheader = Cell.CellTable.CellHeader;
                    Cell.CellTable = new CellTable(holder, holder.CenterLocation);
                    PublicParameters.SinkNode.MainWindow.Dispatcher.Invoke(() => holder.Ellipse_HeaderAgent_Mark.Visibility = Visibility.Hidden);
                    RePopulateHeaderInformation(Cell, oldheader);
                }*/

          
        }


        public static void assignClusterHead(CellGroup Cell)
        {
            double offset = PublicParameters.cellRadius;
            Sensor holder = null;

            foreach (Sensor sen in Cell.clusterNodes)
                {
                    double distance = Operations.DistanceBetweenTwoPoints(Cell.clusterCenterComputed, sen.CenterLocation);
                    if (distance < offset)
                    {
                        offset = distance;
                        holder = sen;
                    }
            }
                   
            try
            {
                Cell.CellTable = new CellTable(holder, holder.CenterLocation);
            }
            catch
            {
                holder = null;
                MessageBox.Show("Error in assiging Cluster Header");
                return;
            }
            holder.Ellipse_HeaderAgent_Mark.Stroke = new SolidColorBrush(Colors.Red);
            PublicParameters.SinkNode.MainWindow.Dispatcher.Invoke(() => holder.Ellipse_HeaderAgent_Mark.Visibility = Visibility.Visible);
            PopulateHeaderInformation(Cell);
        }
    }
}
