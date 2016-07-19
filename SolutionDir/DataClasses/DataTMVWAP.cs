using System;
using System.Linq;

namespace TradeApplication.DataClasses
{
    public class DataTMVWAP: DataTMListSum
    {
        public readonly double[,] VWAPArray;

        public DataTMVWAP(DataTimeSeries dts, int[] timeframe, int allocn = 64)
            : base(dts, timeframe.Max(), 1, allocn)
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
        
        public override void NewData(double k, double v)
        {
            // remove previous time frame value on TSControl update            
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

        public override void TimeUpdate()
        {
            // remove previous time frame value on TSControl update 
            if (TSControl.RowsChanged > 0)
            {
                int vwapidx, cidx;

                for (vwapidx = 0; vwapidx < VWAPArray.GetLength(0); ++vwapidx)
                {
                    if (TSControl.RowsChanged >= VWAPArray[vwapidx, 1])
                    {
                        for (cidx = 2; cidx < 5; cidx++)
                            VWAPArray[vwapidx, cidx] = 0;
                        continue;
                    }

                    cidx = ColIdx - (int)VWAPArray[vwapidx, 1] + 1;
                    if (cidx < 1)
                        cidx = cidx + ColCount - 1;
                    for (int i0 = 0; i0 < TSControl.RowsChanged; ++i0)
                    {
                        VWAPArray[vwapidx, 2] -= DataList[0][cidx]; // remove past time frame volume
                        VWAPArray[vwapidx, 3] -= DataList[1][cidx]; // remove past time frame volume x price
                        cidx = (cidx == ColCount - 1) ? 1 : cidx + 1;
                    }
                }
            }

            base.TimeUpdate();
        }

        private void VWAPArrayCalc()
        {
            for (int ridx = 0; ridx < VWAPArray.GetLength(0); ++ridx)
                if (VWAPArray[ridx, 2] != 0.0)
                    VWAPArray[ridx, 4] = VWAPArray[ridx, 3] / VWAPArray[ridx, 2]; // VWAP = volumexprice / volume
        }

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
