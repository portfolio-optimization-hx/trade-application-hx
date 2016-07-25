using System;
using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication
{
    /// <summary>
    /// DataBuilder, create, aggregate, control data objects
    /// </summary>
    public class DataBuilder
    {
        public readonly DataCurrent Current;

        public Collection<DataTimeSeries> TimeSeries { get; private set; }
        public Collection<DataTSOHLC> OHLC { get; private set; }
        public Collection<DataTSVolume> Volume { get; private set; }
        public DataTimeFrameAnalytics TFAnalytics { get; private set; }
        
        public event EventHandler DataUpdate;

        public DataBuilder()
        {
            Current = new DataCurrent(); // print processor
            TimeSeries = new Collection<DataTimeSeries>(); // timeseries used as controll for other data classes
            OHLC = new Collection<DataTSOHLC>(); // open high low close
            Volume = new Collection<DataTSVolume>(); // bid, ask, traded volume
        }

        public void HandlePrintUpdate(object src, EventArgs e, string s)
        {
            // process print string
            Current.ProcessPrintStr(s);

            if (!Current.NewPrint)
                return;

            int i0;

            // update timeseries data objects
            for (i0 = 0; i0 < TimeSeries.Count; ++i0)
                TimeSeries[i0].NewData(Current.Print);
            for (i0 = 0; i0 < OHLC.Count; ++i0)
                OHLC[i0].NewData(Current.Print);
            for (i0 = 0; i0 < Volume.Count; ++i0)
                Volume[i0].NewData(Current.Print);

            // update timeframe analytics
            TFAnalytics.NewData(Current.Print);

            // call data update event
            DataUpdate?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Create DataTimeSeries, DataTSOHLC, DataTSVolume objects add to DataBuilder collection
        /// </summary>
        /// <param name="tinterval">time interval of timeseries (in minutes)</param>
        /// /// <param name="nrows">number of rows to allocate</param>
        public void NewTimeSeriesOHLCVolume(int tinterval, int nrows = 2880)
        {
            // check if time interval already exist
            foreach (DataTimeSeries ts0 in TimeSeries)
                if (ts0.TimeInterval == tinterval)
                    return;

            DataTimeSeries ts = new DataTimeSeries(tinterval, nrows);
            TimeSeries.Add(ts);
            OHLC.Add(new DataTSOHLC(ts));
            Volume.Add(new DataTSVolume(ts));
        }

        /// <summary>
        /// Create time frame analytics, should refactor similar to timeseries to allow for custom
        /// timeframe analytics data objects rather than preset values
        /// </summary>
        public void NewTimeFrameAnalytics()
        {
            // DataTimeFrameAnalytics cannot be create if TimeSeries collection is empty
            if (TimeSeries != null)
                TFAnalytics = new DataTimeFrameAnalytics(TimeSeries);
        }
    }
}
