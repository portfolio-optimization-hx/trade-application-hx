using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeApplication.DataClasses;

namespace TradeApplication.ViewModels
{

    /// <summary>
    /// ViewModel for TimeSeries OHLC CandleStick Chart
    /// </summary>
    public class ViewModelCandleStickChart : ViewModelOxyChart
    {
        public DataTSOHLC DSOHLC { get; private set; } // data source
        public readonly ObservableCollection<DataPoint> CSOpenCloseUp; // green candles
        public readonly ObservableCollection<DataPoint> CSOpenCloseDown; // red candles
        public readonly ObservableCollection<DataPoint> CSHighLowUp; // green stick
        public readonly ObservableCollection<DataPoint> CSHighLowDown; // red stick
        public Func<double, string> XAxisLabelFormatter { get; private set; } // label formatter function

        public readonly int SDataCount; // number of data points in series / chart

        /// <summary>
        /// ViewModel constructor, requires data source and number of data point in series / chart
        /// </summary>
        /// <param name="ohlc">data source</param>
        /// <param name="nseries">number of data points</param>
        public ViewModelCandleStickChart(DataTSOHLC ohlc, int nseries)
        {
            // init properties
            DSOHLC = ohlc;
            SDataCount = nseries;
            CSOpenCloseDown = new ObservableCollection<DataPoint>();
            CSOpenCloseUp = new ObservableCollection<DataPoint>();
            CSHighLowDown = new ObservableCollection<DataPoint>();
            CSHighLowUp = new ObservableCollection<DataPoint>();

            XAxisLabelFormatter = (d0) => "";
            
            // pre-allocate data points
            for (int i0 = 0; i0 < SDataCount; ++i0)
                for (int i1 = 0; i1 < 3; ++i1)
                {
                    SetCollectionDataPoint(CSOpenCloseDown, -1, i0 + 1, double.NaN);
                    SetCollectionDataPoint(CSOpenCloseUp, -1, i0 + 1, double.NaN);
                    SetCollectionDataPoint(CSHighLowDown, -1, i0 + 1, double.NaN);
                    SetCollectionDataPoint(CSHighLowUp, -1, i0 + 1, double.NaN);
                }
        }

        /// <summary>
        /// Given data ObservableCollections, hide candlesticks by setting values to NaN
        /// </summary>
        /// <param name="openclose">OC candles</param>
        /// <param name="highlow">HL sticks</param>
        /// <param name="idx">index</param>
        private void HideCandleStick(ObservableCollection<DataPoint> openclose, ObservableCollection<DataPoint> highlow, int idx)
        {
            SetCollectionDataPointY(openclose, idx * 3 + 0, double.NaN);
            SetCollectionDataPointY(highlow, idx * 3 + 0, double.NaN);
            SetCollectionDataPointY(highlow, idx * 3 + 1, double.NaN);
            SetCollectionDataPointY(openclose, idx * 3 + 1, double.NaN);
        }

        /// <summary>
        /// Update data source and all data point
        /// </summary>
        /// <param name="ohlc">new data source</param>
        /// <param name="current">current print data array in DataCurrent.Print format</param>
        public void ChangeDataSource(DataTSOHLC ohlc, double[] currentprint)
        {
            DSOHLC = ohlc; // set to new data source
            NewData(currentprint, true); // update all data points, will cause GUI to redraw
        }

        /// <summary>
        /// New data. If data source rows have not changed, only update last data point
        /// else or changed data source update all data points.
        /// </summary>
        /// <remarks>
        /// Method requires current print because DataTSOHLC data source does not set
        /// OHLC Close price until timeseries has passed
        /// </remarks>
        /// <param name="current">current print data array in DataCurrent.Print format</param>
        /// <param name="changeds">change data source flag</param>
        public void NewData(double[] currentprint, bool changeds = false)
        {
            double price = currentprint[3];
            int dataidx = DSOHLC.RowIdx;
            int csidx = SDataCount - 1;

            if ((DSOHLC.RowsChanged == 0) && (!changeds))
            {
                // no row changes only check and update last data point
                if (currentprint[2] != Core.PRINT_TYPE_TRADED) // only update for shifted price
                    return;

                if (price <= DSOHLC.DataArray[dataidx, 0])
                {
                    SetCollectionDataPointY(CSOpenCloseDown, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 0] + eps);
                    SetCollectionDataPointY(CSHighLowDown, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 1]);
                    SetCollectionDataPointY(CSHighLowDown, csidx * 3 + 1, DSOHLC.DataArray[dataidx, 2]);
                    SetCollectionDataPointY(CSOpenCloseDown, csidx * 3 + 1, price - eps);
                    HideCandleStick(CSOpenCloseUp, CSHighLowUp, csidx);
                }
                else
                {
                    SetCollectionDataPointY(CSOpenCloseUp, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 0] - eps);
                    SetCollectionDataPointY(CSHighLowUp, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 1]);
                    SetCollectionDataPointY(CSHighLowUp, csidx * 3 + 1, DSOHLC.DataArray[dataidx, 2]);
                    SetCollectionDataPointY(CSOpenCloseUp, csidx * 3 + 1, price + eps);
                    HideCandleStick(CSOpenCloseDown, CSHighLowDown, csidx);
                }
            }
            else
            {
                // rows changed, shift all datapoint and match data source
                for (csidx = SDataCount - 1; csidx >= 0; --csidx)
                {
                    if (DSOHLC.DataArray[dataidx, 3] <= DSOHLC.DataArray[dataidx, 0])
                    {
                        SetCollectionDataPointY(CSOpenCloseDown, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 0] + eps);
                        SetCollectionDataPointY(CSHighLowDown, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 1]);
                        SetCollectionDataPointY(CSHighLowDown, csidx * 3 + 1, DSOHLC.DataArray[dataidx, 2]);
                        SetCollectionDataPointY(CSOpenCloseDown, csidx * 3 + 1, DSOHLC.DataArray[dataidx, 3] - eps);
                        HideCandleStick(CSOpenCloseUp, CSHighLowUp, csidx);
                    }
                    else
                    {
                        SetCollectionDataPointY(CSOpenCloseUp, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 0] - eps);
                        SetCollectionDataPointY(CSHighLowUp, csidx * 3 + 0, DSOHLC.DataArray[dataidx, 1]);
                        SetCollectionDataPointY(CSHighLowUp, csidx * 3 + 1, DSOHLC.DataArray[dataidx, 2]);
                        SetCollectionDataPointY(CSOpenCloseUp, csidx * 3 + 1, DSOHLC.DataArray[dataidx, 3] + eps);
                        HideCandleStick(CSOpenCloseDown, CSHighLowDown, csidx);
                    }

                    dataidx = (dataidx == 0) ? DSOHLC.RowCount - 1 : dataidx - 1;
                }

                // change current candlestick if print type is traded
                if (currentprint[2] == Core.PRINT_TYPE_TRADED)
                {
                    dataidx = DSOHLC.RowIdx;
                    csidx = SDataCount - 1;
                    if (price <= DSOHLC.DataArray[dataidx, 0])
                        SetCollectionDataPointY(CSOpenCloseDown, csidx * 3 + 1, price + eps);
                    else
                        SetCollectionDataPointY(CSOpenCloseUp, csidx * 3 + 1, price - eps);
                }

                // change labels
                XAxisLabelFormatter = CreateLabels(DSOHLC.TSControl);
            }
        }

        /// <summary>
        /// Create new label formatter function
        /// </summary>
        /// <param name="ts">data source TimeSeries TSControl</param>
        /// <returns>label formatter function</returns>
        private Func<double, string> CreateLabels(DataClasses.DataTimeSeries ts)
        {
            Collection<string> labels = new Collection<string>();
            int i0;
            int ridx = ts.RowIdx;

            for (i0 = 0; i0 < SDataCount + 1; ++i0)
                labels.Add("");

            labels[0] = ts.DataArray[ridx, 0].ToString("0000-00-00");
            for (i0 = SDataCount; i0 > 2; --i0)
            {
                labels[i0] = (ts.DataArray[ridx, 1] / 60).ToString("00:") +
                    (ts.DataArray[ridx, 1] - (ts.DataArray[ridx, 1] / 60 * 60)).ToString("00");
                ridx = (ridx == 0) ? ts.RowCount - 1 : ridx - 1;
            }

            return (d0) => labels[(int)d0];
        }

        /// <summary>
        /// Override base SetCollectionDataPoint, convert y zero values to NaN
        /// </summary>
        /// <seealso cref="ViewModelOxyChart.SetCollectionDataPoint(ObservableCollection{DataPoint}, int, double, double)"/>
        protected override sealed void SetCollectionDataPoint(ObservableCollection<DataPoint> cdp, int idx, double x, double y)
        {
            if (Math.Abs(y) < eps * 2) y = double.NaN;
            base.SetCollectionDataPoint(cdp, idx, x, y);
        }

        /// <summary>
        /// Override base SetCollectionDataPointY, convert y zero values to NaN
        /// </summary>
        /// <seealso cref="ViewModelOxyChart.SetCollectionDataPointY(ObservableCollection{DataPoint}, int, double)"/>
        protected override void SetCollectionDataPointY(ObservableCollection<DataPoint> cdp, int idx, double y)
        {
            if (Math.Abs(y) < eps * 2) y = double.NaN;
            base.SetCollectionDataPointY(cdp, idx, y);
        }
    }


    /// <summary>
    /// ViewModel for TimeSeries Volume Chart(s)
    /// </summary>
    public class ViewModelVolumeChart : ViewModelOxyChart
    {
        public DataTSVolume DSVolume { get; private set; } // data source
        public readonly ObservableCollection<DataPoint> VBid;
        public readonly ObservableCollection<DataPoint> VAsk;
        public readonly ObservableCollection<DataPoint> VTraded;

        public readonly int SDataCount; // number of data points in chart / series

        /// <summary>
        /// ViewModel constructor, requires data source and number of data point in series / chart
        /// </summary>
        /// <param name="volume">data source</param>
        /// <param name="nseries">number of data points</param>
        public ViewModelVolumeChart(DataTSVolume volume, int nseries)
        {
            // init properties
            DSVolume = volume;
            SDataCount = nseries;
            VBid = new ObservableCollection<DataPoint>();
            VAsk = new ObservableCollection<DataPoint>();
            VTraded = new ObservableCollection<DataPoint>();

            // pre-allocate data points
            for (int i0 = 0; i0 < SDataCount; ++i0)
            {
                SetCollectionDataPoint(VBid, -1, i0 + 1, 0);
                SetCollectionDataPoint(VAsk, -1, i0 + 1, 0);
                SetCollectionDataPoint(VTraded, -1, i0 + 1, double.NaN);
                SetCollectionDataPoint(VTraded, -1, i0 + 1, double.NaN);
                SetCollectionDataPoint(VTraded, -1, i0 + 1, double.NaN);
            }
        }
        
        /// <summary>
        /// Update data source and all data point
        /// </summary>
        /// <param name="volume">new data source</param>
        public void ChangeDataSource(DataTSVolume volume)
        {
            DSVolume = volume; // set to new data source
            NewData(true);  // update all data points, will cause GUI to redraw
        }

        /// <summary>
        /// Update data source and all data point
        /// </summary>
        /// <param name="volume">new data source</param>

        /// <summary>
        /// New data. If data source rows have not changed, only update last data point
        /// else or changed data source update all data points.
        /// </summary>
        /// <param name="changeds">change data source flag</param>
        public void NewData(bool changeds = false)
        {
            int dataidx = DSVolume.RowIdx;
            int vidx = SDataCount - 1;

            if ((DSVolume.RowsChanged == 0) && (!changeds))
            {
                // no row change, only update last data point
                SetCollectionDataPointY(VBid, vidx, DSVolume.DataArray[dataidx, 0]);
                SetCollectionDataPointY(VAsk, vidx, DSVolume.DataArray[dataidx, 1]);
                SetCollectionDataPointYBar(VTraded, vidx, DSVolume.DataArray[dataidx, 2]);
            }
            else
            {
                // rows changed, update all data points, shift accordingly
                for (vidx = SDataCount - 1; vidx > 0; --vidx)
                {
                    SetCollectionDataPointY(VBid, vidx, DSVolume.DataArray[dataidx, 0]);
                    SetCollectionDataPointY(VAsk, vidx, DSVolume.DataArray[dataidx, 1]);
                    SetCollectionDataPointYBar(VTraded, vidx, DSVolume.DataArray[dataidx, 2]);
                    dataidx = (dataidx == 0) ? DSVolume.RowCount - 1 : dataidx - 1;
                }
            }
        }

        /// <summary>
        /// Set collection data point cdp, index idx bar size to y.
        /// </summary>
        /// <param name="cdp">collection data point</param>
        /// <param name="idx">index</param>
        /// <param name="y">bar size</param>
        protected void SetCollectionDataPointYBar(ObservableCollection<DataPoint> cdp, int idx, double y)
        {
            // if bar size < eps * 2, hide
            if (y < eps * 2)
            {
                SetCollectionDataPointY(cdp, idx * 3 + 0, double.NaN);
                SetCollectionDataPointY(cdp, idx * 3 + 1, double.NaN);
                return;
            }

            SetCollectionDataPointY(cdp, idx * 3 + 0, 0);
            SetCollectionDataPointY(cdp, idx * 3 + 1, y);
        }        
    }

}
