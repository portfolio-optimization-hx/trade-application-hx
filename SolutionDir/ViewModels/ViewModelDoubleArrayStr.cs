using System.Collections.Generic;
using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication.ViewModels
{
    /// <summary>
    /// ViewModel base class with ObservableCollection strings for text bindings
    /// </summary>
    public abstract class ViewModelDoubleArrayStr
    {
        public double[,] TextData { get; protected set; }
        public ObservableCollection<string> TextStr { get; protected set; }        
    }

    /// <summary>
    /// ViewModel for Quotes, Volume text display binding.
    /// </summary>
    public class ViewModelQuotesText : ViewModelDoubleArrayStr
    {
        public DataCurrent DSCurrent { get; private set; }

        private List<string> LabelStr = new List<string>{ "BID", "ASK", "TRADED" };

        /// <summary>
        /// ViewModel constructor.
        /// </summary>
        /// <param name="current">data source</param>
        public ViewModelQuotesText(DataCurrent current)
        {
            // init propreties
            DSCurrent = current;
            TextData = new double[3, 2];
            TextStr = new ObservableCollection<string>();

            // pre-allocate
            for (int i0 = 0; i0 < TextData.GetLength(0); ++i0)
            {
                TextData[i0, 0] = 0;
                TextData[i0, 1] = 0;
                TextStr.Add("");
            }
        }

        /// <summary>
        /// New data, if data changed, update ObservableCollection.
        /// </summary>
        public void NewData()
        {
            for (int i0 = 0; i0 < DSCurrent.Quotes.GetLength(0); ++i0)
            {
                if ((TextData[i0, 0] - DSCurrent.Quotes[i0] < -Core.DOUBLE_EPS) ||
                (TextData[i0, 0] - DSCurrent.Quotes[i0] > +Core.DOUBLE_EPS) ||
                (TextData[i0, 1] - DSCurrent.QuotesVolume[i0] > -Core.DOUBLE_EPS) ||
                (TextData[i0, 1] - DSCurrent.QuotesVolume[i0] > -Core.DOUBLE_EPS))
                {
                    TextData[i0, 0] = DSCurrent.Quotes[i0];
                    TextData[i0, 1] = DSCurrent.QuotesVolume[i0];
                    TextStr[i0] = (LabelStr[i0] + ": ").PadRight(8) + TextData[i0, 0].ToString("0.00  × ") + TextData[i0, 1].ToString().PadLeft(4);
                }
            }
        }
    }
    
    /// <summary>
    /// ViewModel for VWAP text display bindings.
    /// </summary>
    public class ViewModelVWAPText
    {
        public Collection<double[]> TextData { get; protected set; }
        public Collection<ObservableCollection<string>> TextStr { get; private set; }
        public Collection<DataTMVWAP> DSVWAP { get; private set; }

        private readonly List<List<string>> LabelStr;

        /// <summary>
        /// ViewModel constructor.
        /// </summary>
        /// <param name="vwap">data source</param>
        public ViewModelVWAPText(Collection<DataTMVWAP> vwap)
        {
            TextData = new Collection<double[]>();
            TextStr = new Collection<ObservableCollection<string>>();
            LabelStr = new List<List<string>>();
            
            DSVWAP = vwap;
            ObservableCollection<string> cstr;
            List<string> lstr;
            for (int lidx = 0; lidx < DSVWAP.Count; ++lidx)
            {
                TextData.Add(new double[DSVWAP[lidx].VWAPArray.GetLength(0)]);
                cstr = new ObservableCollection<string>();
                lstr = new List<string>();
                for (int ridx = 0; ridx < DSVWAP[lidx].VWAPArray.GetLength(0); ++ridx)
                {
                    lstr.Add(DSVWAP[lidx].VWAPArray[ridx, 0].ToString().PadRight(4) + "m: ");
                    cstr.Add(lstr[ridx]);
                }
                TextStr.Add(cstr);
                LabelStr.Add(lstr);
            }
        }

        /// <summary>
        /// New data, if data changed, update ObservableCollection.
        /// </summary>
        public void NewData()
        {
            for (int lidx = 0; lidx < DSVWAP.Count; ++lidx)
                for (int ridx = 0; ridx < DSVWAP[lidx].VWAPArray.GetLength(0); ++ridx)
                    if ((DSVWAP[lidx].VWAPArray[ridx, 4] > +0.001) &&
                       ((DSVWAP[lidx].VWAPArray[ridx, 4] - TextData[lidx][ridx] < -0.001) ||
                        (DSVWAP[lidx].VWAPArray[ridx, 4] - TextData[lidx][ridx] > +0.001)))
                    {
                        TextData[lidx][ridx] = DSVWAP[lidx].VWAPArray[ridx, 4];
                        TextStr[lidx][ridx] = LabelStr[lidx][ridx] + TextData[lidx][ridx].ToString("0.00").PadLeft(8);
                    }
        }
    }
}