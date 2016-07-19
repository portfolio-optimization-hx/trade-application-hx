using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication
{
    public class DataTimeFrameAnalytics
    {
        public readonly Collection<DataTMValueDistribution> BidVolumeDist;
        public readonly Collection<DataTMValueDistribution> AskVolumeDist;
        public readonly Collection<DataTMValueDistribution> TradedVolumeDist;

        public readonly Collection<DataTMValueRangeDistribution> PriceRangeDist;
        public readonly Collection<DataTMValueTotalDistribution> VolumeTotalDist;

        public readonly Collection<DataTMVWAP> VWAP; // VWAP TimeInterval 01 minute
        
        public DataTimeFrameAnalytics(Collection<DataTimeSeries> tsc)
        {
            BidVolumeDist = new Collection<DataTMValueDistribution>();
            AskVolumeDist = new Collection<DataTMValueDistribution>();
            TradedVolumeDist = new Collection<DataTMValueDistribution>();
            PriceRangeDist = new Collection<DataTMValueRangeDistribution>();
            VolumeTotalDist = new Collection<DataTMValueTotalDistribution>();
            VWAP = new Collection<DataTMVWAP>();

            double[,] pmtarray; // parameter array

            pmtarray = new double[3,3]{
              {  60,  1, Core.TICK_SIZE},
              { 240, 10, Core.TICK_SIZE},
              {1440, 60, Core.TICK_SIZE}
            };
            ClctnDataTMAdd(BidVolumeDist,tsc,pmtarray);
            ClctnDataTMAdd(AskVolumeDist,tsc,pmtarray);
            ClctnDataTMAdd(TradedVolumeDist,tsc,pmtarray);

            pmtarray = new double[2, 3]{
              {  60,  1, Core.TICK_SIZE},
              { 240,  5, Core.TICK_SIZE}
            };
            ClctnDataTMAdd(PriceRangeDist,tsc,pmtarray);


            pmtarray = new double[2, 3]{
              {  240,  1,  25},
              {  480,  5, 100}
            };
            ClctnDataTMAdd(VolumeTotalDist,tsc,pmtarray);


            int tscidx;            
            tscidx = TimeIntervalIdx(tsc, 01);
            if (tscidx != -1)
                VWAP.Add(new DataTMVWAP(tsc[tscidx], new int[] { 1, 5, 10, 15 }));

            tscidx = TimeIntervalIdx(tsc, 30);
            if (tscidx != -1)
                VWAP.Add(new DataTMVWAP(tsc[tscidx], new int[] { 30, 60, 120, 240 }));

            tscidx = TimeIntervalIdx(tsc, 60);
            if (tscidx != -1)
                VWAP.Add(new DataTMVWAP(tsc[tscidx], new int[] { 360, 720, 1080, 1440 }));
        }

        public void NewData(double[] newprint)
        {
            switch ((int)newprint[2])
            {
                case 0:
                    ClctnDataTMNewData(BidVolumeDist, newprint[3], newprint[4]);
                    
                    // no new data but check timeupdate
                    ClctnDataTMTimeUpdate(AskVolumeDist);
                    ClctnDataTMTimeUpdate(TradedVolumeDist);
                    ClctnDataTMTimeUpdate(VolumeTotalDist);
                    ClctnDataTMTimeUpdate(PriceRangeDist);
                    ClctnDataTMTimeUpdate(VWAP);
                    break;
                case 1:
                    ClctnDataTMNewData(AskVolumeDist, newprint[3], newprint[4]);

                    // no new data but check timeupdate
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

                    // no new data but check timeupdate
                    ClctnDataTMTimeUpdate(BidVolumeDist);
                    ClctnDataTMTimeUpdate(AskVolumeDist);
                    break;
            }
        }
        
        private int TimeIntervalIdx(Collection<DataTimeSeries> tsc,int timeinterval)
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

        private DataTimeSeries GetTSControl(Collection<DataTimeSeries> tsc, int timeinterval)
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

            if (idx == -1)
                return null;
            return tsc[idx];
        }

        #region collection private functions
        private void ClctnDataTMTimeUpdate<T>(Collection<T> clctn)
            where T:DataTMListSum
        {
            for (int i0 = 0; i0 < clctn.Count; ++i0)
                clctn[i0].TimeUpdate();
        }

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

        //private void ClctnDataTMAdd<T>(Collection<T> clctn, Collection<DataTimeSeries> tsc, double[,] parameter_array)
        //    where T:DataTMList, new()
        //{
        //    if (parameter_array.GetLength(1) != 3)
        //        return;

        //    int tscidx;
        //    for (int i0 = 0; i0 < parameter_array.GetLength(0); ++i0)
        //    {
        //        tscidx = TimeIntervalIdx(tsc, (int)parameter_array[i0, 1]);
        //        if (tscidx != -1)
        //        clctn.Add((T)Activator.CreateInstance(typeof(T), tsc[tscidx], (int)parameter_array[i0, 0], parameter_array[i0, 2]));
        //    }
        //}

        private void ClctnDataTMNewData<T>(Collection<T> tmc, double k, double v)
            where T : DataTMListSum
        {
            for (int i0 = 0; i0 < tmc.Count; ++i0)
                tmc[i0].NewData(k, v);
        }
        #endregion
    }
}
