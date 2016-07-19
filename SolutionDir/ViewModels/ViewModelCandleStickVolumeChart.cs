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

    public class ViewModelCandleStickChart : ViewModelOxyChart
    {
        public DataTSOHLC DSOHLC { get; private set; } // data source
        public readonly ObservableCollection<DataPoint> CSOpenCloseUp; // green candles
        public readonly ObservableCollection<DataPoint> CSOpenCloseDown; // red candles
        public readonly ObservableCollection<DataPoint> CSHighLowUp; // green stick
        public readonly ObservableCollection<DataPoint> CSHighLowDown; // red stick
        public Func<double, string> XAxisLabelFormatter { get; private set; }

        public readonly int SDataCount;

        public ViewModelCandleStickChart(DataTSOHLC ohlc, int nseries)
        {
            DSOHLC = ohlc;
            SDataCount = nseries;
            CSOpenCloseDown = new ObservableCollection<DataPoint>();
            CSOpenCloseUp = new ObservableCollection<DataPoint>();
            CSHighLowDown = new ObservableCollection<DataPoint>();
            CSHighLowUp = new ObservableCollection<DataPoint>();

            XAxisLabelFormatter = (d0) => "";

            for (int i0 = 0; i0 < SDataCount; ++i0)
                for (int i1 = 0; i1 < 3; ++i1)
                {
                    SetCollectionDataPoint(CSOpenCloseDown, -1, i0 + 1, double.NaN);
                    SetCollectionDataPoint(CSOpenCloseUp, -1, i0 + 1, double.NaN);
                    SetCollectionDataPoint(CSHighLowDown, -1, i0 + 1, double.NaN);
                    SetCollectionDataPoint(CSHighLowUp, -1, i0 + 1, double.NaN);
                }
        }

        private void HideCandleStick(ObservableCollection<DataPoint> openclose, ObservableCollection<DataPoint> highlow, int idx)
        {
            SetCollectionDataPointY(openclose, idx * 3 + 0, double.NaN);
            SetCollectionDataPointY(highlow, idx * 3 + 0, double.NaN);
            SetCollectionDataPointY(highlow, idx * 3 + 1, double.NaN);
            SetCollectionDataPointY(openclose, idx * 3 + 1, double.NaN);
        }

        public void ChangeDataSource(DataTSOHLC ohlc, double[] currentprint)
        {
            DSOHLC = ohlc;
            NewData(currentprint, true);
        }

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

                if (currentprint[2] == Core.PRINT_TYPE_TRADED)
                {
                    dataidx = DSOHLC.RowIdx;
                    csidx = SDataCount - 1;
                    if (price <= DSOHLC.DataArray[dataidx, 0])
                        SetCollectionDataPointY(CSOpenCloseDown, csidx * 3 + 1, price + eps);
                    else
                        SetCollectionDataPointY(CSOpenCloseUp, csidx * 3 + 1, price - eps);
                }

                XAxisLabelFormatter = CreateLabels(DSOHLC.TSControl);
            }
        }

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

        protected override sealed void SetCollectionDataPoint(ObservableCollection<DataPoint> cdp, int idx, double x, double y)
        {
            if (Math.Abs(y) < eps * 2) y = double.NaN;
            base.SetCollectionDataPoint(cdp, idx, x, y);
        }

        protected override void SetCollectionDataPointY(ObservableCollection<DataPoint> cdp, int idx, double y)
        {
            if (Math.Abs(y) < eps * 2) y = double.NaN;
            base.SetCollectionDataPointY(cdp, idx, y);
        }
    }


    public class ViewModelVolumeChart : ViewModelOxyChart
    {
        public DataTSVolume DSVolume { get; private set; } // data source
        public readonly ObservableCollection<DataPoint> VBid;
        public readonly ObservableCollection<DataPoint> VAsk;
        public readonly ObservableCollection<DataPoint> VTraded;

        public readonly int SDataCount;

        public ViewModelVolumeChart(DataTSVolume volume, int nseries)
        {
            DSVolume = volume;
            SDataCount = nseries;
            VBid = new ObservableCollection<DataPoint>();
            VAsk = new ObservableCollection<DataPoint>();
            VTraded = new ObservableCollection<DataPoint>();

            for (int i0 = 0; i0 < SDataCount; ++i0)
            {
                SetCollectionDataPoint(VBid, -1, i0 + 1, 0);
                SetCollectionDataPoint(VAsk, -1, i0 + 1, 0);
                SetCollectionDataPoint(VTraded, -1, i0 + 1, double.NaN);
                SetCollectionDataPoint(VTraded, -1, i0 + 1, double.NaN);
                SetCollectionDataPoint(VTraded, -1, i0 + 1, double.NaN);
            }
        }

        public void ChangeDataSource(DataTSVolume volume)
        {
            DSVolume = volume;
            NewData(true);
        }


        public void NewData(bool changeds = false)
        {
            int dataidx = DSVolume.RowIdx;
            int vidx = SDataCount - 1;

            if ((DSVolume.RowsChanged == 0) && (!changeds))
            {
                SetCollectionDataPointY(VBid, vidx, DSVolume.DataArray[dataidx, 0]);
                SetCollectionDataPointY(VAsk, vidx, DSVolume.DataArray[dataidx, 1]);
                SetCollectionDataPointYBar(VTraded, vidx, DSVolume.DataArray[dataidx, 2]);
            }
            else
            {
                for (vidx = SDataCount - 1; vidx > 0; --vidx)
                {
                    SetCollectionDataPointY(VBid, vidx, DSVolume.DataArray[dataidx, 0]);
                    SetCollectionDataPointY(VAsk, vidx, DSVolume.DataArray[dataidx, 1]);
                    SetCollectionDataPointYBar(VTraded, vidx, DSVolume.DataArray[dataidx, 2]);
                    dataidx = (dataidx == 0) ? DSVolume.RowCount - 1 : dataidx - 1;
                }
            }
        }

        protected void SetCollectionDataPointYBar(ObservableCollection<DataPoint> cdp, int idx, double y)
        {
            if (y < eps * 2)
            {
                SetCollectionDataPointY(cdp, idx * 3 + 0, double.NaN);
                SetCollectionDataPointY(cdp, idx * 3 + 1, double.NaN);
                return;
            }

            SetCollectionDataPointY(cdp, idx * 3 + 0, 0);
            SetCollectionDataPointY(cdp, idx * 3 + 1, y);
        }
        
        protected override sealed void SetCollectionDataPoint(ObservableCollection<DataPoint> cdp, int idx, double x, double y)
        {
            base.SetCollectionDataPoint(cdp, idx, x, y);
        }
    }

}
