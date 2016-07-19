namespace TradeApplication.DataClasses
{
    public class DataTMValueDistribution: DataTMListSum
    {

        public DataTMValueDistribution(DataTimeSeries dts, int timeframe, double mininterval, int allocn = 64)
            : base(dts, timeframe, mininterval, allocn)
        {
        }

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

    public class DataTMValueTotalDistribution: DataTMListSum
    {
        private double value_total;

        public DataTMValueTotalDistribution(DataTimeSeries dts, int timeframe, double mininterval, int allocn = 64)
            : base(dts, timeframe, mininterval, allocn)
        {
            value_total = 0;
        }

        public override void NewData(double k, double v)
        {
            // add to DataKey on TSControl update
            // check timeframe updates, row changed
            TimeUpdate();

            // evalute v, price, key is automatically managed on time update
            value_total += v;
        }

        public override void TimeUpdate()
        {
            // add to DataKey on TSControl update
            if (TSControl.RowsChanged > 0)
            {
                // add price range in given timeinterval as key and increment value
                NewKeyValue(value_total);
                KeyValueAdd(value_total, 1);
                value_total = 0.0;
            }

            base.TimeUpdate();
        }
    }

    public class DataTMValueRangeDistribution: DataTMListSum
    {
        private double value_min, value_max;

        public DataTMValueRangeDistribution(DataTimeSeries dts, int timeframe, double mininterval, int allocn = 64)
            : base(dts, timeframe, mininterval, allocn)
        {
            value_min = value_max = double.NaN;
        }

        public override void NewData(double k, double v)
        {
            // add to DataKey on TSControl update
            // check timeframe updates, row changed
            TimeUpdate();

            // evalute v, price, key is automatically managed on time update
            if (double.IsNaN(value_min) || (v - value_min < -Core.DOUBLE_EPS))
                value_min = v;
            if (double.IsNaN(value_max) || (v - value_max > +Core.DOUBLE_EPS))
                value_max = v;
        }

        public override void TimeUpdate()
        {
            // add to DataKey on TSControl update
            if (TSControl.RowsChanged > 0)
            {
                // add price range in given timeinterval as key and increment value
                double k = value_max - value_min;
                if (!double.IsNaN(k))
                {
                    NewKeyValue(k);
                    KeyValueAdd(k, 1);
                    value_min = value_max = double.NaN;
                }
            }

            base.TimeUpdate();
        }
    }
}
