using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication
{
    /// <summary>
    /// Grouping object and controller for different collections of TimeFrame data / analytics objects
    /// </summary>
    public class DataTimeFrameAnalytics
    {
        public readonly Collection<DataTMValueDistribution> BidVolumeDist;
        public readonly Collection<DataTMValueDistribution> AskVolumeDist;
        public readonly Collection<DataTMValueDistribution> TradedVolumeDist;

        public readonly Collection<DataTMValueRangeDistribution> PriceRangeDist;
        public readonly Collection<DataTMValueTotalDistribution> VolumeTotalDist;

        public readonly Collection<DataTMVWAP> VWAP;
        
        /// <summary>
        /// DataTimeFrameAnalytics constructor, object requires a TimeSeries consturctor
        /// to initialize TimeFrame analytics objects
        /// </summary>
        /// <param name="tsc">TimeSeries data collection</param>
        public DataTimeFrameAnalytics(Collection<DataTimeSeries> tsc)
        {
            BidVolumeDist = new Collection<DataTMValueDistribution>();
            AskVolumeDist = new Collection<DataTMValueDistribution>();
            TradedVolumeDist = new Collection<DataTMValueDistribution>();
            PriceRangeDist = new Collection<DataTMValueRangeDistribution>();
            VolumeTotalDist = new Collection<DataTMValueTotalDistribution>();
            VWAP = new Collection<DataTMVWAP>();

            double[,] pmtarray; // parameter array in row format [TimeFrame, UpdateInterval, MinValueInterval]
            
            // common parameter array for Bid, Ask, Traded volume distributions
            pmtarray = new double[3,3]{
              {  60,  1, Core.TICK_SIZE},
              { 240, 10, Core.TICK_SIZE},
              {1440, 60, Core.TICK_SIZE}
            };
            ClctnDataTMAdd(BidVolumeDist,tsc,pmtarray);
            ClctnDataTMAdd(AskVolumeDist,tsc,pmtarray);
            ClctnDataTMAdd(TradedVolumeDist,tsc,pmtarray);

            // parameter array for price range distribution
            pmtarray = new double[2, 3]{
              {  60,  1, Core.TICK_SIZE},
              { 240,  5, Core.TICK_SIZE}
            };
            ClctnDataTMAdd(PriceRangeDist,tsc,pmtarray);

            // parameter array for volume total distribution
            pmtarray = new double[2, 3]{
              {  240,  1,  25},
              {  480,  5, 100}
            };
            ClctnDataTMAdd(VolumeTotalDist,tsc,pmtarray);

            // add vwap analytics
            ClctnDataTMVWAPAdd(VWAP, tsc, new int[] {    1,    5,   10,   15 }, 01);
            ClctnDataTMVWAPAdd(VWAP, tsc, new int[] {   30,   60,  120,  240 }, 30);
            ClctnDataTMVWAPAdd(VWAP, tsc, new int[] {  360,  720, 1080, 1440 }, 30);
        }

        /// <summary>
        /// Process new data for all analytics data collections
        /// </summary>
        /// <param name="newprint">new print data array in DataCurrent.Print format</param>
        public void NewData(double[] newprint)
        {
            if (newprint.Length < 5) // minimal data verification
                return;

            // update based on print type
            switch ((int)newprint[2])
            {
                case 0:
                    ClctnDataTMNewData(BidVolumeDist, newprint[3], newprint[4]);
                    
                    // no new data but check timeupdate for data classes alignment
                    ClctnDataTMTimeUpdate(AskVolumeDist);
                    ClctnDataTMTimeUpdate(TradedVolumeDist);
                    ClctnDataTMTimeUpdate(VolumeTotalDist);
                    ClctnDataTMTimeUpdate(PriceRangeDist);
                    ClctnDataTMTimeUpdate(VWAP);
                    break;
                case 1:
                    ClctnDataTMNewData(AskVolumeDist, newprint[3], newprint[4]);

                    // no new data but check timeupdate for data classes alignment
                    ClctnDataTMTimeUpdate(BidVolumeDist);
                    ClctnDataTMTimeUpdate(TradedVolumeDist);
                    ClctnDataTMTimeUpdate(VolumeTotalDist);
                    ClctnDataTMTimeUpdate(PriceRangeDist);
                    ClctnDataTMTimeUpdate(VWAP);
                    break;
                case 2:
                    ClctnDataTMNewData(TradedVolumeDist, newprint[3], newprint[4]);

                    // more analytics when print is PRINT_TYPE_TRADED
                    ClctnDataTMNewData(VolumeTotalDist, -1, newprint[4]);
                    ClctnDataTMNewData(PriceRangeDist, -1, newprint[3]);
                    ClctnDataTMNewData(VWAP, newprint[3], newprint[4]);

                    // no new data but check timeupdate for data classes alignment
                    ClctnDataTMTimeUpdate(BidVolumeDist);
                    ClctnDataTMTimeUpdate(AskVolumeDist);
                    break;
            }
        }

        /// <summary>
        /// Create DataTM object(s) from parameter array settings and add to Collection
        /// overloaded method because generic method require workaround for constructor
        /// </summary>
        /// <param name="clctn">TimeFrame Value Distribution data collection</param>
        /// <param name="tsc">TimeSeries data collection</param>
        /// <param name="parameter_array">parameter array in row format [TimeFrame, UpdateInterval, MinValueInterval]</param>
        private void ClctnDataTMAdd(Collection<DataTMValueDistribution> clctn, Collection<DataTimeSeries> tsc, double[,] parameter_array)
        {
            if (parameter_array.GetLength(1) != 3)
                return;

            int tscidx;
            for (int i0 = 0; i0 < parameter_array.GetLength(0); ++i0)
            {
                tscidx = TimeIntervalIdx(tsc, (int)parameter_array[i0, 1]);
                if (tscidx != -1)
                    clctn.Add(new DataTMValueDistribution(tsc[tscidx], (int)parameter_array[i0, 0], parameter_array[i0, 2]));
            }
        }

        /// <summary>
        /// Create DataTM object(s) from parameter array settings and add to Collection
        /// overloaded method because generic method require workaround for constructor
        /// </summary>
        /// <param name="clctn">TimeFrame Value Range Distribution data collection</param>
        /// <param name="tsc">TimeSeries data collection</param>
        /// <param name="parameter_array">parameter array in row format [TimeFrame, UpdateInterval, MinValueInterval]</param>
        private void ClctnDataTMAdd(Collection<DataTMValueRangeDistribution> clctn, Collection<DataTimeSeries> tsc, double[,] parameter_array)
        {
            if (parameter_array.GetLength(1) != 3)
                return;

            int tscidx;
            for (int i0 = 0; i0 < parameter_array.GetLength(0); ++i0)
            {
                tscidx = TimeIntervalIdx(tsc, (int)parameter_array[i0, 1]);
                if (tscidx != -1)
                    clctn.Add(new DataTMValueRangeDistribution(tsc[tscidx], (int)parameter_array[i0, 0], parameter_array[i0, 2]));
            }
        }

        /// <summary>
        /// Create DataTM object(s) from parameter array settings and add to Collection
        /// overloaded method because generic method require workaround for constructor
        /// </summary>
        /// <param name="clctn">TimeFrame Value Total Distribution data collection</param>
        /// <param name="tsc">TimeSeries data collection</param>
        /// <param name="parameter_array">parameter array in row format [TimeFrame, UpdateInterval, MinValueInterval]</param>
        private void ClctnDataTMAdd(Collection<DataTMValueTotalDistribution> clctn, Collection<DataTimeSeries> tsc, double[,] parameter_array)
        {
            if (parameter_array.GetLength(1) != 3)
                return;

            int tscidx;
            for (int i0 = 0; i0 < parameter_array.GetLength(0); ++i0)
            {
                tscidx = TimeIntervalIdx(tsc, (int)parameter_array[i0, 1]);
                if (tscidx != -1)
                    clctn.Add(new DataTMValueTotalDistribution(tsc[tscidx], (int)parameter_array[i0, 0], parameter_array[i0, 2]));
            }
        }

        /// <summary>
        /// Create single DataTMVWAP object and add to colleciton
        /// </summary>
        /// <param name="clctn">TimeFrame VWAP data collection</param>
        /// <param name="tsc">TimeSeries data collection</param>
        /// <param name="timeframes">parameter array in row format [TimeFrame, UpdateInterval, MinValueInterval]</param>
        /// <param name="updateinterval">parameter array in row format [TimeFrame, UpdateInterval, MinValueInterval]</param>
        private void ClctnDataTMVWAPAdd(Collection<DataTMVWAP> clctn, Collection<DataTimeSeries> tsc, int[] timeframes, int updateinterval)
        {
            int tscidx = TimeIntervalIdx(tsc, updateinterval);
            if (tscidx != -1) // if timeseries exist, create DataTMVWAP and add to collection
                VWAP.Add(new DataTMVWAP(tsc[tscidx], timeframes));
        }
        
        /// <summary>
        /// Private Collection Method: call TimeUpdate() in TimeFrame data collection
        /// </summary>
        /// <typeparam name="T">DataTM subclasses of DataTMListSum</typeparam>
        /// <param name="clctn">TimeFrame data Collection</param>
        private void ClctnDataTMTimeUpdate<T>(Collection<T> clctn)
            where T : DataTMListSum
        {
            for (int i0 = 0; i0 < clctn.Count; ++i0)
                clctn[i0].TimeUpdate();
        }

        /// <summary>
        /// Private Collection Method: call NewData() in TimeFrame data collection
        /// </summary>
        /// <typeparam name="T">DataTM subclasses of DataTMListSum</typeparam>
        /// <param name="clctn">TimeFrame data Collection</param>
        /// <param name="k">new data key</param>
        /// <param name="v">new data value</param>
        private void ClctnDataTMNewData<T>(Collection<T> clctn, double k, double v)
            where T : DataTMListSum
        {
            for (int i0 = 0; i0 < clctn.Count; ++i0)
                clctn[i0].NewData(k, v);
        }
        
        /// <summary>
        /// Search TimeSeries data collection for timeinterval, return Idx of timeinterval and -1 timeinterval
        /// does not exist in TimeSeries collection
        /// </summary>
        /// <param name="tsc">TimeSeries data collection</param>
        /// <param name="timeinterval">time interval in (int) minutes</param>
        /// <returns></returns>
        private int TimeIntervalIdx(Collection<DataTimeSeries> tsc, int timeinterval)
        {
            int idx = -1;
            for (int i0 = 0; i0 < tsc.Count; ++i0)
            {
                if (tsc[i0].TimeInterval == timeinterval)
                {
                    idx = i0;
                    break;
                }
            }

            return idx;
        }

    }
}
