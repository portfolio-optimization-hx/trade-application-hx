using System;
using System.Collections.Generic;

namespace TradeApplication.DataClasses
{

    public class DataLoopedArray<T>
    {
        public readonly T[,] DataArray;
        public readonly int ColCount;
        public readonly int RowCount;
        public int RowIdx { get; protected set; }
        public int RowsChanged { get; protected set; }

        public DataLoopedArray(int rown, int coln)
        {
            RowCount = rown;
            ColCount = coln;
            DataArray = new T[RowCount, ColCount];
        }

        /// <summary>
        /// Get last n rows from RowIdx
        /// </summary>
        /// <param name="nrows">number of rows</param>
        /// <returns>n rows of data from DataArray</returns>
        public T[,] GetLastNRows(int nrows)
        {
            T[,] arr0 = new T[nrows, ColCount];

            int rbsize = ColCount * System.Runtime.InteropServices.Marshal.SizeOf(DataArray[0, 0]); // row byte size
            
            if (nrows-1 <= RowIdx)
            {
                Buffer.BlockCopy(
                    DataArray, (RowIdx - nrows + 1) * rbsize,
                    arr0, 0,
                    nrows * rbsize);
            }
            else
            {
                Buffer.BlockCopy(
                    DataArray, 0,
                    arr0, (nrows - RowIdx - 1) * rbsize,
                    (RowIdx + 1) * rbsize);
                nrows = Math.Min(nrows - RowIdx,RowCount - RowIdx) - 1;
                Buffer.BlockCopy(
                    DataArray, (RowCount - nrows) * rbsize,
                    arr0, (arr0.GetLength(0) - RowIdx - nrows - 1) * rbsize,
                    nrows * rbsize);
            }

            return arr0;
        }
    }

    public abstract class DataTSData<T> : DataLoopedArray<T>
    {
        public readonly DataTimeSeries TSControl;

        public DataTSData(DataTimeSeries ts, int coln) 
            : base(ts.RowCount,coln)
        {
            TSControl = ts;
        }
        
        abstract public void NewData(double[] newprint);

        public virtual void TimeUpdate(T value_fill)
        {
            if (TSControl.RowsChanged > 0)
            {
                RowIdx = TSControl.RowIdx;
                RowsChanged = TSControl.RowsChanged;

                int cidx0, ridx0 = RowIdx;
                for (int i0 = 0; i0 < TSControl.RowsChanged; ++i0)
                {
                    for (cidx0 = 0; cidx0 < ColCount; ++cidx0)
                        DataArray[ridx0, cidx0] = value_fill;
                    ridx0 = (ridx0 == 0) ? RowCount - 1 : ridx0 - 1;
                }
            }
        }
    }
    
    public abstract class DataTMListSum
    {
        public readonly List<double> DataKey;
        public readonly List<double[]> DataList;
        public readonly double MinValueInterval;

        public readonly DataTimeSeries TSControl;
        public readonly int UpdateInterval;
        public readonly int TimeFrame;
        public readonly int ColCount;

        public double KeyMin { get; private set; }
        public double KeyMax { get; private set; }
        public double KeyMinCV { get; private set; } // KeyMin in contains value range
        public double KeyMaxCV { get; private set; } // KeyMax in contains value range
        public int ColIdx { get; protected set; }

        public double KeyLastUpdated { get; protected set; }
        
        /// <summary>
        /// Data structure maintains time frames so it can efficiently update the summation column on TimeSeries controller changes.
        /// The summation column is the first column in List<double array>
        /// </summary>
        /// <param name="dts"></param>
        /// <param name="timeframe"></param>
        /// <param name="mininterval"></param>
        /// <param name="allocn"></param>
        public DataTMListSum(DataTimeSeries dts, int timeframe, double mininterval, int allocn = 64)
        {
            int coladd = 1; // column additional to the number required to maintain frames

            TSControl = dts;
            UpdateInterval = dts.TimeInterval;
            TimeFrame = timeframe;
            ColCount = coladd + (int)Math.Ceiling((double)TimeFrame / UpdateInterval);
            ColIdx = coladd;
            MinValueInterval = mininterval;

            DataKey = new List<double>(allocn);
            DataList = new List<double[]>(allocn);
        }

        public int KeyToIdx(double k)
        {
            int idx;

            if ((DataKey.Count == 0) ||
                (k - KeyMin < -Core.DOUBLE_EPS) ||
                (k - KeyMax > +Core.DOUBLE_EPS))
                idx = -1;
            else
                idx = (int)((k - KeyMin) / MinValueInterval + Core.DOUBLE_EPS);

            return idx;
        }

        public void NewKeyValue(double k)
        {
            if (DataKey.Count == 0)
            {
                DataKey.Add(k);
                DataList.Add(new double[ColCount]);
                KeyMin = KeyMax = k;
                KeyMinCV = KeyMaxCV = k;
            }
            else
            {
                while (k - KeyMin < -Core.DOUBLE_EPS)
                {
                    KeyMin -= MinValueInterval;
                    DataKey.Insert(0, KeyMin);
                    DataList.Insert(0, new double[ColCount]);
                    KeyMinCV = KeyMin;
                }
                while (k - KeyMax > +Core.DOUBLE_EPS)
                {
                    KeyMax += MinValueInterval;
                    DataKey.Add(KeyMax);
                    DataList.Add(new double[ColCount]);
                    KeyMaxCV = KeyMax;
                }
            }
        }

        public virtual void KeyContainValueMinMaxUpdate()
        {

            int lidx,ridx;
            bool isnonzero;

            isnonzero = false;
            for (lidx = KeyToIdx(KeyMinCV); lidx < DataKey.Count-1; ++lidx)
            {
                for (ridx = 0; ridx < ColCount; ++ridx)
                {
                    isnonzero = (DataList[lidx][ridx] < -Core.DOUBLE_EPS) || (DataList[lidx][ridx] > +Core.DOUBLE_EPS);
                    if (isnonzero)
                        break;
                }
                if (isnonzero)
                    break;

            }
            KeyMinCV = DataKey[lidx];

            isnonzero = false;
            for (lidx = KeyToIdx(KeyMaxCV); lidx > 0; --lidx)
            {
                for (ridx = 0; ridx < ColCount; ++ridx)
                {
                    isnonzero = (DataList[lidx][ridx] < -Core.DOUBLE_EPS) || (DataList[lidx][ridx] > +Core.DOUBLE_EPS);
                    if (isnonzero)
                        break;
                }
                if (isnonzero)
                    break;

            }
            KeyMaxCV = DataKey[lidx];
        }

        public virtual void KeyContainValueMinMaxUpdateQ() // quick
        {
            if (DataKey.Count == 0)
                return;

            int lidx;
            
            for (lidx = KeyToIdx(KeyMinCV); lidx < DataKey.Count - 1; ++lidx)
                if ((DataList[lidx][0] < -Core.DOUBLE_EPS) || 
                    (DataList[lidx][0] > +Core.DOUBLE_EPS))
                    break;
            KeyMinCV = DataKey[lidx];
            
            for (lidx = KeyToIdx(KeyMaxCV); lidx > 0; --lidx)
                if ((DataList[lidx][0] < -Core.DOUBLE_EPS) ||
                    (DataList[lidx][0] > +Core.DOUBLE_EPS))
                    break;
            KeyMaxCV = DataKey[lidx];
        }

        abstract public void NewData(double k, double v);

        public virtual void KeyValueAdd(double k, double v)
        {
            int lidx = KeyToIdx(k);
            if (ColIdx != 0)
                DataList[lidx][0] += v; // add to summation column [0] 
            DataList[lidx][ColIdx] += v; // add to current time frame column

            if (k - KeyMinCV < -Core.DOUBLE_EPS)
                KeyMinCV = k;
            if (k - KeyMaxCV > -Core.DOUBLE_EPS)
                KeyMaxCV = k;
            KeyLastUpdated = k;
        }

        public virtual void TimeUpdate()
        {
            if ((DataList.Count == 0) || //no data to update
                (TSControl.RowsChanged == 0))
                return;

            if (TSControl.RowsChanged > ColCount - 1)
            {
                // update more columns than data list contains, essentially clears all data
                for (int lidx = KeyToIdx(KeyMinCV); lidx <= KeyToIdx(KeyMaxCV); ++lidx)
                    DataList[lidx] = new double[ColCount];
            }
            else
            {
                for (int i0 = 0; i0 < TSControl.RowsChanged; ++i0)
                {
                    ColIdx = (ColIdx == ColCount - 1) ? 1 : ColIdx + 1;
                    for (int lidx = KeyToIdx(KeyMinCV); lidx <= KeyToIdx(KeyMaxCV); ++lidx)
                    {
                        if (DataList[lidx][ColIdx] != 0.0)
                        {
                            // clear and subtract from sum
                            DataList[lidx][0] -= DataList[lidx][ColIdx];
                            DataList[lidx][ColIdx] = 0.0;
                        }
                    }
                }
            }
            KeyContainValueMinMaxUpdateQ();
        }

        public void PrintData()
        {
            Console.WriteLine();
            for (int lidx = 0; lidx < DataKey.Count; ++lidx)
            {
                Console.Write(lidx.ToString("0000 ") + DataKey[lidx].ToString("0.00 "));
                for (int cidx = 0; cidx < ColCount; ++cidx)
                    Console.Write(DataList[lidx][cidx].ToString(" 0.00"));
                Console.WriteLine();
            }
            Console.WriteLine();
        }

    }
}
