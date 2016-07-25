using System;
using System.Collections.Generic;

namespace TradeApplication.DataClasses
{

    /// <summary>
    /// Two dimensional array, subclass designed to loops around and replace
    /// the oldest value if array size exceeded.
    /// </summary>
    /// <remarks>
    /// Class for now not defined as abstact to allow object to be created as 
    /// most basic looped array. However may change class to be abstract in the 
    /// future depending on usage
    /// </remarks>
    /// <typeparam name="T">define array type, designed for numeric types</typeparam>
    public class DataLoopedArray<T>
    {
        public readonly T[,] DataArray;
        public readonly int ColCount; // number of columns
        public readonly int RowCount; // number of rows
        public int RowIdx { get; protected set; } // current row index
        public int RowsChanged { get; protected set; } // number of rows changed

        /// <summary>
        /// Create array of size [rown, coln]
        /// </summary>
        /// <param name="rown">number of rows</param>
        /// <param name="coln">number of columns</param>
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

    /// <summary>
    /// Abstract looped array TimeSeriesData data class.
    /// Data class is controlled by / matches a DataTimeSeries object.
    /// </summary>
    /// <typeparam name="T">define array type</typeparam>
    public abstract class DataTSData<T> : DataLoopedArray<T>
    {
        public readonly DataTimeSeries TSControl; // TimeSeries control for data class

        /// <summary>
        /// Create looped array, same number of rows as TSControl, and given columns
        /// </summary>
        /// <param name="ts">TimeSeries control</param>
        /// <param name="coln">number of columns</param>
        public DataTSData(DataTimeSeries ts, int coln) 
            : base(ts.RowCount,coln)
        {
            TSControl = ts;
        }
        
        /// <summary>
        /// Abstract method handles new data
        /// </summary>
        /// <param name="newprint"></param>
        abstract public void NewData(double[] newprint);


        /// <summary>
        /// On time update, match TSControl row change, replace inbetween, no-data rows with value_fill
        /// </summary>
        /// <param name="value_fill">value to replace in between, no-data values</param>
        public virtual void TimeUpdate(T value_fill)
        {
            if (TSControl.RowsChanged > 0)
            {
                // match settings
                RowIdx = TSControl.RowIdx;
                RowsChanged = TSControl.RowsChanged;

                // replace in between, no-data values
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

    /// <summary>
    /// Abstract TimeFrame data class using a list of double array with the first index of the array as the sum of the array
    /// Data class is controlled both by a DataTimeSeries object and new values
    /// </summary>
    /// <remarks>
    /// TimeFrame aggregates across time and specfic consecutive keys in DataList 
    /// property. DataList can be viewed two dimensionally with keys as 
    /// rows and time as column. The amount of time each object holds is fixed 
    /// whereas key can expand therefore the data structure is arrays combined
    /// into a list. 
    /// Index zero of each array is a summation entry, the rest of the rows are
    /// the values in each time interval. As time progresses, column / time interval
    /// are cleared and values subtracted from summation index.
    /// DataKey is a corresponding key to each DataList. The data class
    /// does not need DataKey, it is included for other classes and methods to 
    /// reference DataList keys rather than recreating DataKey every time
    /// </remarks>
    public abstract class DataTMListSum
    {
        public readonly List<double> DataKey;
        public readonly List<double[]> DataList;
        public readonly double MinValueInterval; // minimum value interval between keys

        public readonly DataTimeSeries TSControl; // TimeSeries control
        public readonly int TimeFrame; // time frame of data object
        public readonly int UpdateInterval; // data class update every interval minutes
        public readonly int ColCount; // number of columns

        public double KeyMin { get; private set; } // DataKey minimum bound
        public double KeyMax { get; private set; } // DataKey maximum bound
        public double KeyMinCV { get; private set; } // KeyMin in contains value range
        public double KeyMaxCV { get; private set; } // KeyMax in contains value range
        public int ColIdx { get; protected set; } // current column index

        public double KeyLastUpdated { get; protected set; } // last key updated
        
        /// <summary>
        /// Data class maintains time intervals so it can efficiently update the summation column on TimeSeries controller changes.
        /// The summation column is the first column in List<double array>
        /// </summary>
        /// <param name="ts">TimeSeries control</param>
        /// <param name="timeframe">total time frame in object</param>
        /// <param name="mininterval">minimum value between keys</param>
        /// <param name="allocn">pre-allocate list size</param>
        public DataTMListSum(DataTimeSeries ts, int timeframe, double mininterval, int allocn = 64)
        {
            int coladd = 1; // column additional to the number required to maintain frames

            TSControl = ts;
            UpdateInterval = ts.TimeInterval;
            TimeFrame = timeframe;
            ColCount = coladd + (int)Math.Ceiling((double)TimeFrame / UpdateInterval);
            ColIdx = coladd;
            MinValueInterval = mininterval;

            DataKey = new List<double>(allocn);
            DataList = new List<double[]>(allocn);
        }

        /// <summary>
        /// Convert key to list index of DataKey, DataList
        /// </summary>
        /// <param name="k">key</param>
        /// <returns></returns>
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

        /// <summary>
        /// Add new key to DataKey if key is not within current min max bounds
        /// </summary>
        /// <param name="k">key</param>
        public void NewKeyValue(double k)
        {
            if (DataKey.Count == 0)
            {
                // empty DataKey, add new key
                DataKey.Add(k);
                DataList.Add(new double[ColCount]);
                KeyMin = KeyMax = k;
                KeyMinCV = KeyMaxCV = k;
            }
            else
            {
                while (k - KeyMin < -Core.DOUBLE_EPS)
                {
                    // if key is less than lower bound
                    KeyMin -= MinValueInterval;
                    DataKey.Insert(0, KeyMin);
                    DataList.Insert(0, new double[ColCount]);
                    KeyMinCV = KeyMin;
                }
                while (k - KeyMax > +Core.DOUBLE_EPS)
                {
                    // if key is greater than upper bound
                    KeyMax += MinValueInterval;
                    DataKey.Add(KeyMax);
                    DataList.Add(new double[ColCount]);
                    KeyMaxCV = KeyMax;
                }
            }
        }

        /// <summary>
        /// Check and update KeyMinCV, KeyMaxCV, check every index in array
        /// </summary>
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

        /// <summary>
        /// Check and update KeyMinCV, KeyMaxCV, check summation index only
        /// </summary>
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

        /// <summary>
        /// New data given key, value
        /// </summary>
        /// <param name="k">key</param>
        /// <param name="v">value</param>
        abstract public void NewData(double k, double v);

        /// <summary>
        /// Add value to DataList[key]
        /// </summary>
        /// <param name="k">key</param>
        /// <param name="v">value</param>
        public virtual void KeyValueAdd(double k, double v)
        {
            int lidx = KeyToIdx(k);
            if (ColIdx != 0)
                DataList[lidx][0] += v; // add to summation column [0] 
            DataList[lidx][ColIdx] += v; // add to current time interval column

            if (k - KeyMinCV < -Core.DOUBLE_EPS)
                KeyMinCV = k;
            if (k - KeyMaxCV > -Core.DOUBLE_EPS)
                KeyMaxCV = k;
            KeyLastUpdated = k;
        }

        /// <summary>
        /// Time update, subtract passed value from summation column,
        /// clear passed time interval column
        /// </summary>
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

            // check, update KeyMinCV, KeyMaxCV
            KeyContainValueMinMaxUpdateQ();
        }

        /// <summary>
        /// Print data
        /// </summary>
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
