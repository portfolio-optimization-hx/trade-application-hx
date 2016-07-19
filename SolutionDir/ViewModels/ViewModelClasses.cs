using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TradeApplication.DataClasses;

namespace TradeApplication.ViewModels
{
    public abstract class ViewModelOxyChart
    {
        protected const double eps = 0.0005;

        protected virtual void SetCollectionDataPoint(ObservableCollection<DataPoint> cdp, int idx, double x, double y)
        {
            if (idx == -1)
            {
                cdp.Add(new DataPoint(x, y));
            }
            else
            {
                if ((x != cdp[idx].X) || (y != cdp[idx].Y))
                    cdp[idx] = new DataPoint(x, y);
            }
        }

        protected virtual void SetCollectionDataPointX(ObservableCollection<DataPoint> cdp, int idx, double x)
        {
            if (x != cdp[idx].X)
                cdp[idx] = new DataPoint(x, cdp[idx].Y);
        }

        protected virtual void SetCollectionDataPointY(ObservableCollection<DataPoint> cdp, int idx, double y)
        {
            if (y != cdp[idx].Y)
                cdp[idx] = new DataPoint(cdp[idx].X, y);
        }
    }

    public class ViewModelButtonsSelect : ViewModelOxyChart
    {
        public readonly Collection<ObservableCollection<ButtonBindings>> BtnBind;
        public struct ButtonBindings
        {
            public Tuple<string, int> BtnParameters { get; set; }
            public string BtnText { get; set; }
            public string BtnToolTip { get; set; }
        }

        public ViewModelButtonsSelect()
        {
            BtnBind = new Collection<ObservableCollection<ButtonBindings>>();
        }

        public void AddSrcString(string srcname, List<string> btnstr, string tooltip_prepend = "")
        {
            ObservableCollection<ButtonBindings> cbtn = new ObservableCollection<ButtonBindings>();
            for (int i0 = 0; i0 < btnstr.Count; ++i0)
            {
                cbtn.Add(new ButtonBindings
                {
                    BtnParameters = new Tuple<string, int>(srcname, i0),
                    BtnText = btnstr[i0],
                    BtnToolTip = tooltip_prepend + ""
                });
            }
            BtnBind.Add(cbtn);
        }

        public void AddSrcDS(string srcname, Collection<DataTSOHLC> datasource, string tooltip_prepend = "")
        {
            ObservableCollection<ButtonBindings> cbtn = new ObservableCollection<ButtonBindings>();
            for (int i0 = 0; i0 < datasource.Count; ++i0)
            {
                cbtn.Add(new ButtonBindings
                {
                    BtnParameters = new Tuple<string, int>(srcname, i0),
                    BtnText = datasource[i0].TSControl.TimeInterval.ToString("0m"),
                    BtnToolTip = tooltip_prepend + datasource[i0].TSControl.TimeInterval.ToString("0m")
                });
            }
            BtnBind.Add(cbtn);
        }

        public void AddSrcDS<T>(string srcname, Collection<T> datasource, string tooltip_prepend = "")
            where T : DataTMListSum
        {
            ObservableCollection<ButtonBindings> cbtn = new ObservableCollection<ButtonBindings>();
            for (int i0 = 0; i0 < datasource.Count; ++i0)
            {
                cbtn.Add(new ButtonBindings
                {
                    BtnParameters = new Tuple<string, int>(srcname, i0),
                    BtnText = datasource[i0].TimeFrame.ToString("0m × ") +
                        datasource[i0].TSControl.TimeInterval.ToString("0m"),
                    BtnToolTip = tooltip_prepend + datasource[i0].TimeFrame.ToString("0m updated every ") +
                        datasource[i0].TSControl.TimeInterval.ToString("0m | Value Interval: ") +
                        datasource[i0].MinValueInterval.ToString()
                });
            }
            BtnBind.Add(cbtn);
        }

    }
}
