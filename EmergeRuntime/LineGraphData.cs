using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace EmergeRuntime
{
    public class LineGraphData : IPointDataSource
    {
        private RingArray<DataPoint> data;
        private EnumerableDataSource<DataPoint> ds;

        public LineGraphData(int size, string description)
        {
            // Array for data
            data = new RingArray<DataPoint>(size);

            // Convert to an enumerable data source for line graph
            ds = new EnumerableDataSource<DataPoint>(data);

            // and set mappings
            ds.SetXMapping(x => x.X);
            ds.SetYMapping(y => y.Y);

            ds.AddMapping(ShapeElementPointMarker.ToolTipTextProperty, p => string.Format("{0}, {1}, " + description, p.X, p.Y));
        }

        public void AddDataPoint(double x, double y)
        {
            data.Add(new DataPoint() { X = x, Y = y });
        }
 
        #region IPointDataSource Members

        public event EventHandler DataChanged;

        public void RaiseDataChanged()
        {
            if (DataChanged != null)
            {
                DataChanged(this, EventArgs.Empty);
            }
        }

        public IPointEnumerator GetEnumerator(System.Windows.DependencyObject context)
        {
            return ds.GetEnumerator(context);
        }

        #endregion

        private class DataPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }
    }
}
