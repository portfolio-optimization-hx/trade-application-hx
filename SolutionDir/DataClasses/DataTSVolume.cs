namespace TradeApplication.DataClasses
{
    /// <summary>
    /// Volume data class, manage, aggregate, assemble volume.
    /// Timestamp is controlled and aligned to corresponding time interval TimeSeries TSControl data object
    /// </summary>
    /// <remarks>
    /// Volume is stored as a column looped array
    /// column [0] is the sum of Bid volumes in the current volume timeseries
    /// column [1] is the sum of Ask volumes in the current volume timeseries
    /// column [2] is the sum of Traded volumes in the current volume timeseries
    /// Bid, Ask volumes are updates to the Bid, Ask prints. Thus the sum of Bid, 
    /// Ask volumes is not as meanful, they're summed for demo purpose only.
    /// </remarks>
    public class DataTSVolume : DataTSData<int>
    {
        /// <summary>
        ///  Volume data class constructor, require corresponding time interval TimeSeries TSControl
        /// </summary>
        /// <param name="ts">TimeSeries control</param>
        public DataTSVolume(DataTimeSeries ts) : base(ts, 3)
        {
            RowIdx = ts.RowIdx; // match TimeSeries RowIdx
        }

        public override void NewData(double[] newprint)
        {
            RowsChanged = 0;

            if (newprint.Length < 5) // minimal data verification
                return;

            TimeUpdate(0); // check new row, volume timeseries
            DataArray[RowIdx, (int)newprint[2]] += (int)newprint[4]; // add volume to corresponding print type column
        }

    }
}
