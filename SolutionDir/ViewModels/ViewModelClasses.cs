using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication.ViewModels
{
    /// <summary>
    /// ViewModel abstract base class with ObservableCollections of data binding 
    /// for OxyPlot WPF chart objects. ObservableCollection does not update / redraw
    /// if update values are the same.
    /// </summary>
    public abstract class ViewModelOxyChart
    {
        protected const double eps = 0.0005; // epsilon for minimal double differential
        
        /// <summary>
        /// Set collection data point cdp, index idx to new DataPoint(x,y)
        /// index of -1 adds new data point
        /// </summary>
        /// <param name="cdp">collection data point</param>
        /// <param name="idx">index</param>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        protected virtual void SetCollectionDataPoint(ObservableCollection<DataPoint> cdp, int idx, double x, double y)
        {
            if (idx == -1)
                cdp.Add(new DataPoint(x, y));
            else if ((idx >= 0) && (idx < cdp.Count)) 
                if ((x != cdp[idx].X) || (y != cdp[idx].Y))
                    cdp[idx] = new DataPoint(x, y);
        }

        /// <summary>
        /// Set collection data point cdp, index idx to new DataPoint(x,existing y)
        /// </summary>
        /// <param name="cdp">collection data point</param>
        /// <param name="idx">index</param>
        /// <param name="x">x value</param>
        protected virtual void SetCollectionDataPointX(ObservableCollection<DataPoint> cdp, int idx, double x)
        {
            if ((idx >= 0) && (idx < cdp.Count))
                if (x != cdp[idx].X)
                    cdp[idx] = new DataPoint(x, cdp[idx].Y);
        }

        /// <summary>
        /// Set collection data point cdp, index idx to new DataPoint(existing x, new y)
        /// </summary>
        /// <param name="cdp">collection data point</param>
        /// <param name="idx">index</param>
        /// <param name="y">y value</param>
        protected virtual void SetCollectionDataPointY(ObservableCollection<DataPoint> cdp, int idx, double y)
        {
            if ((idx >= 0) && (idx < cdp.Count))
                if (y != cdp[idx].Y)
                    cdp[idx] = new DataPoint(cdp[idx].X, y);
        }
    }

    /// <summary>
    /// ViewModel for chart select data source with multiple Collections of 
    /// ObservableCollection button bindings
    /// </summary>
    public class ViewModelButtonsSelect : ViewModelOxyChart
    {
        public readonly Collection<ObservableCollection<ButtonBindings>> BtnBind;
        public struct ButtonBindings
        {
            public Tuple<string, int> BtnParameters { get; set; } // button parameter for callback
            public string BtnText { get; set; } // button text string
            public string BtnToolTip { get; set; } // button tooltip string
        }

        /// <summary>
        /// ViewModel Buttons Select constructor
        /// </summary>
        public ViewModelButtonsSelect()
        {
            BtnBind = new Collection<ObservableCollection<ButtonBindings>>();
        }

        /// <summary>
        /// Create button binding from DataTSOHLC data source
        /// </summary>
        /// <param name="srcname">name</param>
        /// <param name="datasource">data source object</param>
        /// <param name="tooltip_prepend">tooltip prepend string</param>
        public void AddSrcDS(string srcname, Collection<DataTSOHLC> datasource, string tooltip_prepend = "")
        {
            ObservableCollection<ButtonBindings> cbtn = new ObservableCollection<ButtonBindings>();
            for (int i0 = 0; i0 < datasource.Count; ++i0)
            {
                cbtn.Add(new ButtonBindings
                {
                    BtnParameters = new Tuple<string, int>(srcname, i0),
                    BtnText = datasource[i0].TSControl.TimeInterval.ToString("0m"),
                    BtnToolTip = tooltip_prepend + datasource[i0].TSControl.TimeInterval.ToString("0m")
                });
            }
            BtnBind.Add(cbtn);
        }

        /// <summary>
        /// Create button binding from DataTMListSum subclasses data source
        /// </summary>
        /// <typeparam name="T">define DataTMListSum subclass</typeparam>
        /// <param name="srcname">name</param>
        /// <param name="datasource">data source object</param>
        /// <param name="tooltip_prepend">tooltip prepend string</param>
        public void AddSrcDS<T>(string srcname, Collection<T> datasource, string tooltip_prepend = "")
            where T : DataTMListSum
        {
            ObservableCollection<ButtonBindings> cbtn = new ObservableCollection<ButtonBindings>();
            for (int i0 = 0; i0 < datasource.Count; ++i0)
            {
                cbtn.Add(new ButtonBindings
                {
                    BtnParameters = new Tuple<string, int>(srcname, i0),
                    BtnText = datasource[i0].TimeFrame.ToString("0m × ") +
                        datasource[i0].TSControl.TimeInterval.ToString("0m"),
                    BtnToolTip = tooltip_prepend + datasource[i0].TimeFrame.ToString("0m updated every ") +
                        datasource[i0].TSControl.TimeInterval.ToString("0m | Value Interval: ") +
                        datasource[i0].MinValueInterval.ToString()
                });
            }
            BtnBind.Add(cbtn);
        }


        /// <summary>
        /// Create button binding from list of button text strings
        /// </summary>
        /// <param name="srcname">name</param>
        /// <param name="btnstr">button text string</param>
        /// <param name="tooltip_prepend">tooltip prepend string</param>
        public void AddSrcString(string srcname, List<string> btnstr, string tooltip_prepend = "")
        {
            ObservableCollection<ButtonBindings> cbtn = new ObservableCollection<ButtonBindings>();
            for (int i0 = 0; i0 < btnstr.Count; ++i0)
            {
                cbtn.Add(new ButtonBindings
                {
                    BtnParameters = new Tuple<string, int>(srcname, i0),
                    BtnText = btnstr[i0],
                    BtnToolTip = tooltip_prepend + ""
                });
            }
            BtnBind.Add(cbtn);
        }

    }
}
