using OxyPlot;
using System;
using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication.ViewModels
{
    /// <summary>
    /// ViewModel for distribution bar charts. ViewModel supports ObservableCollection
    /// data point bindings for horizontal-vertical, negative-positive (left-right) bar charts.
    /// </summary>
    /// <typeparam name="T">define data source DataTMListSum subclass</typeparam>
    public class ViewModelValueDistChart<T> : ViewModelOxyChart
        where T : DataTMListSum
    {
        public T DSValueDist { get; private set; }
        public ObservableCollection<DataPoint> ValueDist { get; private set; }
        public readonly bool IsHorizontal; // horizontal or vertical
        public readonly bool IsNegative; // positive or negative - reversed, left side bar chart
        private double KeyMin;
        private double KeyMax;

        /// <summary>
        /// ViewModel Constructor, require data source, and optional setting for 
        /// horizontal-vertical, negative-positive
        /// </summary>
        /// <param name="valuedist">data source</param>
        /// <param name="horv">horizontal-vertical flag</param>
        /// <param name="norp">positive-negative flag</param>
        public ViewModelValueDistChart(T valuedist, string horv = "horizontal", string norp = "positive")
        {
            // init properties
            DSValueDist = valuedist;
            IsHorizontal = string.Equals(horv, "horizontal", StringComparison.OrdinalIgnoreCase);
            IsNegative = string.Equals(norp, "negative", StringComparison.OrdinalIgnoreCase);

            ValueDist = new ObservableCollection<DataPoint>();
            KeyMin = KeyMax = double.NaN;
        }

        /// <summary>
        /// Update data source and all data point
        /// </summary>
        /// <param name="datasource">new data source</param>
        public void ChangeDataSource(T datasource)
        {
            DSValueDist = datasource; // change data source

            // reset data points, min, max, 
            ValueDist.Clear();
            KeyMin = KeyMax = double.NaN;
            NewData(true);
        }

        /// <summary>
        /// New data. If data source rows have not changed, only update last changed data point
        /// else or changed data source update all data points.
        /// </summary>
        /// <param name="changeds">change data source flag</param>
        public void NewData(bool changeds = false)
        {
            if (DSValueDist.DataKey.Count == 0)
                return;

            // init
            int lidx;
            if (double.IsNaN(KeyMin))
            {
                KeyMin = KeyMax = DSValueDist.KeyMin;
                lidx = DSValueDist.KeyToIdx(KeyMin);
                NewBarKeyValue(ValueDist, 00, DSValueDist.DataKey[lidx], DSValueDist.DataList[lidx][0]);
            }

            // match DataPoint with DataKey, DataList
            for (lidx = DSValueDist.KeyToIdx(KeyMin); lidx > 0; --lidx)
                NewBarKeyValue(ValueDist, 00, DSValueDist.DataKey[lidx - 1], DSValueDist.DataList[lidx - 1][0]);

            for (lidx = DSValueDist.KeyToIdx(KeyMax); lidx < DSValueDist.KeyToIdx(DSValueDist.KeyMax); ++lidx)
                NewBarKeyValue(ValueDist, -1, DSValueDist.DataKey[lidx + 1], DSValueDist.DataList[lidx + 1][0]);

            KeyMin = DSValueDist.KeyMin;
            KeyMax = DSValueDist.KeyMax;

            if ((DSValueDist.TSControl.RowsChanged == 0) && (!changeds))
            {
                // change given list index
                lidx = DSValueDist.KeyToIdx(DSValueDist.KeyLastUpdated);
                SetBarValue(ValueDist, lidx, DSValueDist.DataList[lidx][0]);
            }
            else
            {
                // frame change update all
                for (lidx = 0; lidx < DSValueDist.DataList.Count; ++lidx)
                    SetBarValue(ValueDist, lidx, DSValueDist.DataList[lidx][0]);
            }
        }

        /// <summary>
        /// Insert new triple data point bars to collection bar data points cdp at index idx
        /// index of -1 adds new bars data point to the end
        /// </summary>
        /// <param name="cdp">collection data point</param>
        /// <param name="idx">idx == -1 add, else insert at idx</param>
        /// <param name="k">key</param>
        /// <param name="v1">bar size</param>
        protected void NewBarKeyValue(ObservableCollection<DataPoint> cdp, int idx, double k, double v1)
        {
            double v0 = 0;
            if (IsNegative)
                v1 = -v1;
            if ((v1 > -eps) && (v1 < +eps))
                v0 = v1 = double.NaN;

            if (idx == -1)
            {
                // new data point
                if (IsHorizontal)
                {
                    cdp.Add(new DataPoint(k, v0));
                    cdp.Add(new DataPoint(k, v1));
                    cdp.Add(new DataPoint(k, double.NaN));
                }
                else
                {
                    cdp.Add(new DataPoint(v0, k));
                    cdp.Add(new DataPoint(v1, k));
                    cdp.Add(new DataPoint(double.NaN, k));
                }
            }
            else
            {
                if (IsHorizontal)
                {
                    cdp.Insert(idx, new DataPoint(k, double.NaN));
                    cdp.Insert(idx, new DataPoint(k, v1));
                    cdp.Insert(idx, new DataPoint(k, v0));
                }
                else
                {
                    cdp.Insert(idx, new DataPoint(double.NaN, k));
                    cdp.Insert(idx, new DataPoint(v1, k));
                    cdp.Insert(idx, new DataPoint(v0, k));
                }
            }
        }

        /// <summary>
        /// Set collection data point bar size to v at index idx
        /// </summary>
        /// <param name="cdp">collection data point</param>
        /// <param name="idx">index</param>
        /// <param name="v">bar size</param>
        protected void SetBarValue(ObservableCollection<DataPoint> cdp, int idx, double v)
        {
            if (IsNegative)
                v = -v;

            if (IsHorizontal)
            {
                if ((v > -eps * 2) && (v < +eps * 2))
                {
                    SetCollectionDataPointY(cdp, idx * 3 + 0, double.NaN);
                    SetCollectionDataPointY(cdp, idx * 3 + 1, double.NaN);
                }
                else
                {
                    SetCollectionDataPointY(cdp, idx * 3 + 0, 0);
                    SetCollectionDataPointY(cdp, idx * 3 + 1, v);
                }
            }
            else
            {
                if ((v > -eps * 2) && (v < +eps * 2))
                {
                    SetCollectionDataPointX(cdp, idx * 3 + 0, double.NaN);
                    SetCollectionDataPointX(cdp, idx * 3 + 1, double.NaN);
                }
                else
                {
                    SetCollectionDataPointX(cdp, idx * 3 + 0, 0);
                    SetCollectionDataPointX(cdp, idx * 3 + 1, v);
                }
            }
        }
    }
}
