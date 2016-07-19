using System;

namespace TradeApplication.DataClasses
{
    /// <summary>
    /// Data class models after timeseries price form {open,high,low,close}
    /// bid and ask prices are generally not included OHLC format, therefore
    /// class currently will only handle traded price
    /// </summary>
    public class DataTSOHLC : DataTSData<double>
    {
        private double price_last { get; set; }
        private bool row_hastraded { get; set; }
        
        public DataTSOHLC(DataTimeSeries ts) : base(ts, 4)
        {
            RowIdx = ts.RowIdx;

            price_last = 0;
            row_hastraded = false;
        }

        public override void NewData(double[] newprint)
        {
            RowsChanged = 0;

            if (newprint.Length < 5) // minimal data verification
                return;

            int cidx0;
            double price = newprint[3];

            // timeseries has new data / row
            // if the newest price is bid or ask, zero row
            TimeUpdate(price_last);
            if (TSControl.RowsChanged > 0)
                row_hastraded = false;
            
            if (((int)newprint[2]) != Core.PRINT_TYPE_TRADED)
                return;

            if (!row_hastraded)
                for (cidx0 = 0; cidx0 < ColCount; ++cidx0)
                    DataArray[RowIdx, cidx0] = price;
            row_hastraded = true;

            if (price == price_last)
                return;

            // previous in new row price is a bid or ask, replace zero with first traded price
            if (DataArray[RowIdx,0] == 0)
                for (cidx0 = 0; cidx0 < ColCount; ++cidx0)
                    DataArray[RowIdx, cidx0] = price;

            // check and update high and low
            if (price > DataArray[RowIdx, 1]) { DataArray[RowIdx, 1] = price; }
            if (price < DataArray[RowIdx, 2]) { DataArray[RowIdx, 2] = price; }

            price_last = price;
        }

        public override void TimeUpdate(double value_fill)
        {
            if (TSControl.RowsChanged > 0)
                DataArray[RowIdx, 3] = value_fill; // set closing price
            base.TimeUpdate(value_fill);
        }

        public void Print(int nrows = 100)
        {
            int ridx0 = TSControl.RowIdx;
            for (int i0 = 0; i0 < Math.Min(nrows,TSControl.RowCount); ++i0)
            {
                Console.Write("{0} {1} {2} - ",
                    ridx0,
                    TSControl.DataArray[ridx0, 0].ToString(),
                    TSControl.DataArray[ridx0, 1].ToString("0000")
                    );

                Console.Write("{0} {1} {2} {3} ",
                    DataArray[ridx0, 0].ToString("00.00"),
                    DataArray[ridx0, 1].ToString("00.00"),
                    DataArray[ridx0, 2].ToString("00.00"),
                    DataArray[ridx0, 3].ToString("00.00")
                    );

                Console.WriteLine();

                ridx0 = (ridx0 == 0) ? TSControl.RowCount - 1 : --ridx0;
            }
        }
    }
}