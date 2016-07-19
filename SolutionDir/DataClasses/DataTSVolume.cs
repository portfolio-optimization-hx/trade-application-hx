namespace TradeApplication.DataClasses
{
    public class DataTSVolume : DataTSData<int>
    {
        public DataTSVolume(DataTimeSeries ts) : base(ts, 3)
        {
            RowIdx = ts.RowIdx;
        }

        public override void NewData(double[] newprint)
        {
            RowsChanged = 0;

            if (newprint.Length < 5) // minimal data verification
                return;

            TimeUpdate(0);
            DataArray[RowIdx, (int)newprint[2]] += (int)newprint[4];
        }

    }
}
