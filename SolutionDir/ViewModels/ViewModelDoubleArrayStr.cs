using System.Collections.Generic;
using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication.ViewModels
{
    public abstract class ViewModelDoubleArrayStr
    {
        public double[,] TextData { get; protected set; }
        public ObservableCollection<string> TextStr { get; protected set; }
        
    }

    public class ViewModelQuotesText : ViewModelDoubleArrayStr
    {
        private List<string> LabelStr = new List<string>{ "BID", "ASK", "TRADED" };

        public ViewModelQuotesText()
        {
            TextData = new double[3, 2];
            TextStr = new ObservableCollection<string>();
            for (int i0 = 0; i0 < TextData.GetLength(0); ++i0)
            {
                TextData[i0, 0] = 0;
                TextData[i0, 1] = 0;
                TextStr.Add("");
            }
        }

        public void NewData(double[] quotes, int[] volumes)
        {
            for (int i0 = 0; i0 < quotes.GetLength(0); ++i0)
            {
                if ((TextData[i0, 0] - quotes[i0] < -Core.DOUBLE_EPS) ||
                (TextData[i0, 0] - quotes[i0] > +Core.DOUBLE_EPS) ||
                (TextData[i0, 1] - volumes[i0] > -Core.DOUBLE_EPS) ||
                (TextData[i0, 1] - volumes[i0] > -Core.DOUBLE_EPS))
                {
                    TextData[i0, 0] = quotes[i0];
                    TextData[i0, 1] = volumes[i0];
                    TextStr[i0] = (LabelStr[i0] + ": ").PadRight(8) + TextData[i0, 0].ToString("0.00  × ") + TextData[i0, 1].ToString().PadLeft(4);
                }
            }
        }
    }
    
    public class ViewModelVWAPText
    {
        public Collection<double[]> TextData { get; protected set; }
        public Collection<ObservableCollection<string>> TextStr { get; private set; }
        public Collection<DataTMVWAP> DSVWAP { get; private set; }

        private readonly List<List<string>> LabelStr;

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