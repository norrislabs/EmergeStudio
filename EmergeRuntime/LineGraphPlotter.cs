using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace EmergeRuntime
{
    public class LineGraphPlotter
    {
        private ChartPlotter m_Plotter;
        private SolidColorBrush[] m_Colors = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Violet, Brushes.Orange, Brushes.Lime };
        private int m_CurrentColor = 0;

        public LineGraphPlotter(ChartPlotter plotter)
        {
            m_Plotter = plotter;
        }

        public void AddLineGraph(LineGraphData ds, string description)
        {
            if (m_CurrentColor < m_Colors.Count())
            {
                m_Plotter.AddLineGraph(ds, new Pen(m_Colors[m_CurrentColor], 2), new PlotMarker() { Size = 4, Fill = m_Colors[m_CurrentColor] }, new PenDescription(description));
                m_CurrentColor++;
            }
            else
                m_Plotter.AddLineGraph(ds, 3, description);
        }

        public void RemoveLineGraph(string description)
        {
            LineGraph lg = m_Plotter.Children.OfType<LineGraph>().Where(x => x.Description.ToString() == description).Single();
            m_Plotter.Children.Remove(lg);
        }

        public void ClearLineGraphs()
        {
            List<LineGraph> lgs = new List<LineGraph>();
            foreach (LineGraph lg in m_Plotter.Children.OfType<LineGraph>())
                lgs.Add(lg);

            foreach (LineGraph lg in lgs)
                m_Plotter.Children.Remove(lg);

            List<ElementMarkerPointsGraph> lms = new List<ElementMarkerPointsGraph>();
            foreach (ElementMarkerPointsGraph lm in m_Plotter.Children.OfType<ElementMarkerPointsGraph>())
                lms.Add(lm);

            foreach (ElementMarkerPointsGraph lm in lms)
                m_Plotter.Children.Remove(lm);

            m_CurrentColor = 0;
        }

        public bool LineGraphExists(string description)
        {
            foreach (LineGraph lg in m_Plotter.Children.OfType<LineGraph>())
            {
                if (lg.Description.ToString() == description)
                    return true;
            }
            return false;
        }

        public void AddDataPoint(string description, double x, double y)
        {
            LineGraph lg = m_Plotter.Children.OfType<LineGraph>().Where(l => l.Description.ToString() == description).Single();
            LineGraphData lgd = lg.DataSource as LineGraphData;
            lgd.AddDataPoint(x, y);
        }

        public void Refresh(string description)
        {
            LineGraph lg = m_Plotter.Children.OfType<LineGraph>().Where(l => l.Description.ToString() == description).Single();
            LineGraphData lgd = lg.DataSource as LineGraphData;
            lgd.RaiseDataChanged();
        }

        public void Refresh()
        {
            foreach (LineGraph lg in m_Plotter.Children.OfType<LineGraph>())
            {
                LineGraphData lgd = lg.DataSource as LineGraphData;
                lgd.RaiseDataChanged();
            }
        }

        public void SetMainTitle(string title)
        {
            Header hdr;
            if (m_Plotter.Children.OfType<Header>().Count() == 1)
            {
                hdr = m_Plotter.Children.OfType<Header>().Single();
                m_Plotter.Children.Remove(hdr);
            }

            hdr = new Header();
            hdr.Content = title;
            m_Plotter.Children.Add(hdr);
        }

        public void SetVerticalTitle(string title)
        {
            VerticalAxisTitle vt;
            if (m_Plotter.Children.OfType<VerticalAxisTitle>().Count() == 1)
            {
                vt = m_Plotter.Children.OfType<VerticalAxisTitle>().Single();
                m_Plotter.Children.Remove(vt);
            }

            vt = new VerticalAxisTitle();
            vt.Content = title;
            m_Plotter.Children.Add(vt);
        }

        public void SetHorizontalTitle(string title)
        {
            HorizontalAxisTitle ht;
            if (m_Plotter.Children.OfType<HorizontalAxisTitle>().Count() == 1)
            {
                ht = m_Plotter.Children.OfType<HorizontalAxisTitle>().Single();
                m_Plotter.Children.Remove(ht);
            }

            ht = new HorizontalAxisTitle();
            ht.Content = title;
            m_Plotter.Children.Add(ht);
        }

        public ChartPlotter Plotter
        {
            get { return m_Plotter; }
        }
    }

    public class PlotMarker : ShapeElementPointMarker
    {

        public override UIElement CreateMarker()
        {
            Ellipse result = new Ellipse();
            result.Width = Size;
            result.Height = Size;
            result.Stroke = Fill;
            result.Fill = Fill;
            if (!String.IsNullOrEmpty(ToolTipText))
            {
                ToolTip tt = new ToolTip();
                tt.Content = ToolTipText;
                result.ToolTip = tt;
            }
            return result;
        }

        public override void SetPosition(UIElement marker, Point screenPoint)
        {
            Canvas.SetLeft(marker, screenPoint.X - Size / 2);
            Canvas.SetTop(marker, screenPoint.Y - Size / 2);
        }
    }

}
