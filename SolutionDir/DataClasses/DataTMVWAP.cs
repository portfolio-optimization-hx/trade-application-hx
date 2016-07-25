using System;
using System.Linq;

namespace TradeApplication.DataClasses
{
    /// <summary>
    /// TimeFrame data list tracks VWAP in TimeFrame, updated every time interval
    /// </summary>
    /// <remarks>
    /// DataTMVWAP extends DataTMListSum further than other TimeFrame classes. 
    /// The data class is create to efficiently calculate and track multiple VWAPs 
    /// that are all updated on the same time interval.
    /// VWAP is used to store VWAP information in row format 
    ///     [timeframe, number of interval in timeframe, volume total, (volume x price) total, VWAP]
    /// DataTMListSum is used to track volume in list[0], and volume x price in list[1]
    /// across time intervals. In this case the lists should not expand since there are 
    /// no new keys. All other update, clearing, summation column function as usual and 
    /// are used to update the VWAP array.
    /// </remarks>
    public class DataTMVWAP: DataTMListSum
    {
        public readonly double[,] VWAPArray;

        /// <summary>
        /// TimeFrame VWAP contructor, DataKey, DataList fixed size of 2.
        /// </summary>
        /// <param name="ts">TimeSeries control</param>
        /// <param name="timeframe">VWAP timeframes</param>
        public DataTMVWAP(DataTimeSeries ts, int[] timeframe)
            : base(ts, timeframe.Max(), 1, 2)
        {
            NewKeyValue(0.0); // volume
            NewKeyValue(1.0); // volume x price
            DataList[0][0] = 1;
            DataList[1][0] = 1;

            VWAPArray = new double[timeframe.GetLength(0),5];
            for (int i0 = 0; i0 < timeframe.GetLength(0); ++i0)
            {
                VWAPArray[i0,0] = timeframe[i0];
                VWAPArray[i0,1] = timeframe[i0] / UpdateInterval;
            }            
        }

        /// <summary>
        /// New data given key, value. Calculate, add new values to volume, 
        /// volumexprice. Calculate VWAPs.
        /// </summary>
        /// <param name="k">key: price</param>
        /// <param name="v">value: volume</param>
        public override void NewData(double k, double v)
        {
            // remove previous time interval value on TSControl update            
            // check timeframe updates, row changed
            TimeUpdate();

            // update value
            KeyValueAdd(0, v);
            KeyValueAdd(1, v * k);
            
            // cumulate volume, and volumexprice (volume x price)
            for (int vwapidx = 0; vwapidx < VWAPArray.GetLength(0); ++vwapidx)
            {
                VWAPArray[vwapidx, 2] += v;
                VWAPArray[vwapidx, 3] += v * k;
            }
            VWAPArrayCalc(); // Calculate VWAP
        }

        /// <summary>
        /// Time update, update VWAP array similar to summation column, then perform
        /// base update, clear time interval columns, adjust summation column
        /// </summary>
        public override void TimeUpdate()
        {
            // remove previous time interval value on TSControl update 
            if (TSControl.RowsChanged > 0)
            {
                int vwapidx, cidx;

                for (vwapidx = 0; vwapidx < VWAPArray.GetLength(0); ++vwapidx)
                {
                    // all columns cleared, zero data
                    if (TSControl.RowsChanged >= VWAPArray[vwapidx, 1])
                    {
                        for (cidx = 2; cidx < 5; cidx++)
                            VWAPArray[vwapidx, cidx] = 0;
                        continue;
                    }

                    // clear, zero select data, subtract zeroed value from VWAP array
                    cidx = ColIdx - (int)VWAPArray[vwapidx, 1] + 1;
                    if (cidx < 1)
                        cidx = cidx + ColCount - 1;
                    for (int i0 = 0; i0 < TSControl.RowsChanged; ++i0)
                    {
                        VWAPArray[vwapidx, 2] -= DataList[0][cidx]; // remove past time interval volume
                        VWAPArray[vwapidx, 3] -= DataList[1][cidx]; // remove past time interval volume x price
                        cidx = (cidx == ColCount - 1) ? 1 : cidx + 1;
                    }
                }
            }

            base.TimeUpdate();
        }

        /// <summary>
        /// Calculating VWAP from (price x volume) / volume
        /// </summary>
        private void VWAPArrayCalc()
        {
            // loop through all vwap data
            for (int ridx = 0; ridx < VWAPArray.GetLength(0); ++ridx)
                if (VWAPArray[ridx, 2] != 0.0)
                    VWAPArray[ridx, 4] = VWAPArray[ridx, 3] / VWAPArray[ridx, 2]; // VWAP = volumexprice / volume
        }

        /// <summary>
        /// Print VWAP array data
        /// </summary>
        public void PrintVWAP()
        {
            Console.WriteLine("");
            for (int ridx = 0; ridx < VWAPArray.GetLength(0); ++ridx)
            {
                for (int cidx = 0; cidx < VWAPArray.GetLength(1); ++cidx)
                    Console.Write(VWAPArray[ridx, cidx].ToString(" 0.00"));
                Console.WriteLine("");
            }
            Console.WriteLine("");
        }
    }
}
