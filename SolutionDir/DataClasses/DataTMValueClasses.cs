namespace TradeApplication.DataClasses
{
    /// <summary>
    /// TimeFrame data list tracks value distribution in TimeFrame, updated every time interval
    /// </summary>
    public class DataTMValueDistribution: DataTMListSum
    {

        /// <summary>
        /// TimeFrame Value Distribution constructor
        /// </summary>
        /// <seealso cref="DataTMListSum(DataTimeSeries, int, double, int)"/>
        public DataTMValueDistribution(DataTimeSeries ts, int timeframe, double mininterval, int allocn = 64)
            : base(ts, timeframe, mininterval, allocn)
        {
        }

        /// <summary>
        /// New data given key, value
        /// </summary>
        /// <param name="k">key</param>
        /// <param name="v">value</param>
        public override void NewData(double k, double v)
        {
            // check timeframe updates, row changed
            TimeUpdate();

            // check additional rows need to added
            NewKeyValue(k);

            // update value
            KeyValueAdd(k, v);
        }
    }

    /// <summary>
    /// TimeFrame data list tracks value total distribution in TimeFrame, updated every time interval
    /// </summary>
    public class DataTMValueTotalDistribution: DataTMListSum
    {
        private double value_total; // track value total in current time interval
        
        /// <summary>
        /// TimeFrame Value Total Distribution constructor
        /// </summary>
        /// <seealso cref="DataTMListSum(DataTimeSeries, int, double, int)"/>
        public DataTMValueTotalDistribution(DataTimeSeries ts, int timeframe, double mininterval, int allocn = 64)
            : base(ts, timeframe, mininterval, allocn)
        {
            value_total = 0; // init value_total
        }

        /// <summary>
        /// New data given key, value. Increment value_total by value until end of time interval
        /// then add to DataKey, DataList
        /// </summary>
        /// <param name="k">key</param>
        /// <param name="v">value</param>
        public override void NewData(double k, double v)
        {
            // add to DataKey on TSControl update
            // check timeframe updates, row changed
            TimeUpdate();

            // evaluate v, value_total insert is automatically managed on time update
            value_total += v;
        }

        /// <summary>
        /// Time update, add value_total to DataKey and increment corresponding DataList entry
        /// then perform base time update, clear time interval columns, adjust summation column
        /// </summary>
        public override void TimeUpdate()
        {
            // add to DataKey on TSControl update
            if (TSControl.RowsChanged > 0)
            {
                // add total value in given time interval as key and increment value++
                NewKeyValue(value_total);
                KeyValueAdd(value_total, 1);
                value_total = 0.0; // reset value_total
            }

            base.TimeUpdate();
        }
    }

    /// <summary>
    /// TimeFrame data list tracks value total range in TimeFrame, updated every time interval
    /// </summary>
    public class DataTMValueRangeDistribution: DataTMListSum
    {
        private double value_min, value_max; // track value_min, value_max in current time interval

        /// <summary>
        /// TimeFrame Value Range Distribution constructor
        /// </summary>
        /// <seealso cref="DataTMListSum(DataTimeSeries, int, double, int)"/>
        public DataTMValueRangeDistribution(DataTimeSeries ts, int timeframe, double mininterval, int allocn = 64)
            : base(ts, timeframe, mininterval, allocn)
        {
            value_min = value_max = double.NaN; // init value_min, value_max
        }

        /// <summary>
        /// New data given key, value. Track value_min, value_max until end of time interval
        /// then add to DataKey, DataList
        /// </summary>
        /// <param name="k">key</param>
        /// <param name="v">value</param>
        public override void NewData(double k, double v)
        {
            // add to DataKey on TSControl update
            // check timeframe updates, row changed
            TimeUpdate();

            // evalute v, range insert is automatically managed on time update
            if (double.IsNaN(value_min) || (v - value_min < -Core.DOUBLE_EPS))
                value_min = v;
            if (double.IsNaN(value_max) || (v - value_max > +Core.DOUBLE_EPS))
                value_max = v;
        }

        /// <summary>
        /// Time update, caculate value range in time interval, add to DataKey and increment 
        /// corresponding DataList entry then perform base time update, clear time interval 
        /// columns, adjust summation column
        /// </summary>
        public override void TimeUpdate()
        {
            // add to DataKey on TSControl update
            if (TSControl.RowsChanged > 0)
            {
                // add value range in given time interval as key and increment value++
                double k = value_max - value_min;
                if (!double.IsNaN(k))
                {
                    NewKeyValue(k);
                    KeyValueAdd(k, 1);
                    value_min = value_max = double.NaN; // reset value_min, value_max
                }
            }

            base.TimeUpdate();
        }
    }
}
