using System;

namespace TradeApplication.DataClasses
{
    /// <summary>
    /// OHLC data class, manage, aggregate, assemble price into OHLC format.
    /// Timestamp is controlled and aligned to corresponding time interval TimeSeries TSControl data object
    /// </summary>
    /// <remarks>
    /// Data class models after timeseries price form {open,high,low,close}
    /// bid and ask prices are generally not included OHLC format, therefore
    /// class currently will only handle traded price
    /// </remarks>
    /// <seealso cref="DataTSData{T}"/>
    public class DataTSOHLC : DataTSData<double>
    {
        private double price_last { get; set; }
        private bool row_hastraded { get; set; } // OHLC has traded print
        
        /// <summary>
        ///  OHLC data class constructor, require corresponding time interval TimeSeries TSControl
        /// </summary>
        /// <param name="ts">TimeSeries control</param>
        public DataTSOHLC(DataTimeSeries ts) : base(ts, 4)
        {
            RowIdx = ts.RowIdx; // match TimeSeries RowIdx

            price_last = 0;
            row_hastraded = false;
        }

        /// <summary>
        /// Get traded price from new print data, assemble OHLC 
        /// </summary>
        /// <remarks>
        /// Set first traded print in TimeSeries as Open. if print type is not traded
        /// allocate new row with last traded print, maintain row_hastraded at false.
        /// On print type traded, if row_hastraded is set at false, replace Open price
        /// and set row_hastraded to true. 
        /// If traded price is greater than OHLC High, replace new traded price as High
        /// If traded price is less than OHLC Low, replace new traded price as Low
        /// OHLC does not set Close price, until a new OHLC series starts in TimeUpdate().
        /// This may affect displays and other data classes that depends on OHLC
        /// </remarks>
        /// <seealso cref="TimeUpdate(double)"/>
        /// <param name="newprint">new print data array in DataCurrent.Print format</param>
        public override void NewData(double[] newprint)
        {
            RowsChanged = 0;

            if (newprint.Length < 5) // minimal data verification
                return;

            int cidx0;
            double price = newprint[3];

            // check timeseries has new data / row
            // if the newest price is bid or ask, zero row with price_last
            TimeUpdate(price_last);
            if (TSControl.RowsChanged > 0)
                row_hastraded = false;
            
            if (((int)newprint[2]) != Core.PRINT_TYPE_TRADED)
                return;

            // OHLC series does not contain a traded price yet, update
            if (!row_hastraded)
                for (cidx0 = 0; cidx0 < ColCount; ++cidx0)
                    DataArray[RowIdx, cidx0] = price;
            row_hastraded = true;
            
            //// double check, previous in new row price is a bid or ask, replace zero with first traded price
            //if (DataArray[RowIdx, 0] == 0)
            //    for (cidx0 = 0; cidx0 < ColCount; ++cidx0)
            //        DataArray[RowIdx, cidx0] = price;

            if (price == price_last)
                return;

            // check and update high and low
            if (price > DataArray[RowIdx, 1]) { DataArray[RowIdx, 1] = price; }
            if (price < DataArray[RowIdx, 2]) { DataArray[RowIdx, 2] = price; }

            price_last = price;
        }

        /// <summary>
        /// Extend DataTSData.TimeUpdate(), on new RowsChange finalize last price
        /// then do TimeUpdate row change with price_last as value_fill
        /// </summary>
        /// <param name="value_fill">fill new rows with last price</param>
        public override void TimeUpdate(double value_fill)
        {
            if (TSControl.RowsChanged > 0)
                DataArray[RowIdx, 3] = value_fill; // set closing price
            base.TimeUpdate(value_fill);
        }

        /// <summary>
        /// Print timestamp, OHLC data
        /// </summary>
        /// <param name="nrows"></param>
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