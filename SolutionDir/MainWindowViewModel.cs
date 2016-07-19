using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using OxyPlot;
using TradeApplication.DataClasses;
using TradeApplication.ViewModels;

namespace TradeApplication
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public readonly DataBuilder DBuilder;

        public ViewModelCandleStickChart CSChartVM { get; private set; }
        public ObservableCollection<DataPoint> CSOpenCloseUp { get; private set; } // green candles
        public ObservableCollection<DataPoint> CSOpenCloseDown { get; private set; } // red candles
        public ObservableCollection<DataPoint> CSHighLowUp { get; private set; } // green stick
        public ObservableCollection<DataPoint> CSHighLowDown { get; private set; } // red stick
        public int CSXAxisMax { get; private set; }
        public Func<double, string> CSLabelFormatter { get; private set; }

        public ViewModelVolumeChart VolumeChartVM { get; private set; }
        public ObservableCollection<DataPoint> BidVolume { get; private set; }
        public ObservableCollection<DataPoint> AskVolume { get; private set; }
        public ObservableCollection<DataPoint> TradedVolume { get; private set; }

        public ViewModelValueDistChart<DataTMValueDistribution> BidVDChartVM { get; private set; }
        public ViewModelValueDistChart<DataTMValueDistribution> AskVDChartVM { get; private set; }
        public ViewModelValueDistChart<DataTMValueDistribution> TradedVDChartVM { get; private set; }

        public ObservableCollection<DataPoint> BidVDist { get; private set; }
        public ObservableCollection<DataPoint> AskVDist { get; private set; }
        public ObservableCollection<DataPoint> TradedVDist { get; private set; }
        public double BATVDistYAxisMin { get; private set; }
        public double BATVDistYAxisMax { get; private set; }
        
        public ViewModelValueDistChart<DataTMValueRangeDistribution> PriceRDChartVM { get; private set; }
        public ViewModelValueDistChart<DataTMValueTotalDistribution> VolumeTDChartVM { get; private set; }
        public ObservableCollection<DataPoint> PriceRDist { get; private set; }
        public ObservableCollection<DataPoint> VolumeTDist { get; private set; }
        
        public ViewModelQuotesText QuotesTextVM { get; private set; }
        public ObservableCollection<string> QuotesTextStr { get; private set; }

        public ViewModelVWAPText VWAPTextVM { get; private set; }
        public Collection<ObservableCollection<string>> VWAPTextStr { get; private set; }

        public ViewModelButtonsSelect BtnSlctCSVVM { get; private set; }
        public Collection<ObservableCollection<ViewModelButtonsSelect.ButtonBindings>> BtnSelectBind { get; private set; }
        
        public ICommand SelectSourceClick { get; set; }

        public Func<double, string> LabelFormatterEmpty { get; private set; }
        public Func<double, string> LabelFormatterAbs { get; private set; }
        
        private delegate void LambdaZeroInput();

        public MainWindowViewModel(DataBuilder dbuilder)
        {
            DBuilder = dbuilder;
            int nseries = 30;

            CSChartVM = new ViewModelCandleStickChart(DBuilder.OHLC[0], nseries);
            CSOpenCloseUp = CSChartVM.CSOpenCloseUp;
            CSOpenCloseDown = CSChartVM.CSOpenCloseDown;
            CSHighLowUp = CSChartVM.CSHighLowUp;
            CSHighLowDown = CSChartVM.CSHighLowDown;
            CSXAxisMax = CSChartVM.SDataCount + 1;
            CSLabelFormatter = CSChartVM.XAxisLabelFormatter;

            VolumeChartVM = new ViewModelVolumeChart(DBuilder.Volume[0], nseries);
            BidVolume = VolumeChartVM.VBid;
            AskVolume = VolumeChartVM.VAsk;
            TradedVolume = VolumeChartVM.VTraded;

            BidVDChartVM = new ViewModelValueDistChart<DataTMValueDistribution>(DBuilder.TFAnalytics.BidVolumeDist[0],"vertical", "negative");
            AskVDChartVM = new ViewModelValueDistChart<DataTMValueDistribution>(DBuilder.TFAnalytics.AskVolumeDist[0], "vertical");
            TradedVDChartVM = new ViewModelValueDistChart<DataTMValueDistribution>(DBuilder.TFAnalytics.TradedVolumeDist[0], "vertical","negative");
            BidVDist = BidVDChartVM.ValueDist;
            AskVDist = AskVDChartVM.ValueDist;
            TradedVDist = TradedVDChartVM.ValueDist;
            BATVDistYAxisMin = BATVDistYAxisMax = 0;

            PriceRDChartVM = new ViewModelValueDistChart<DataTMValueRangeDistribution>(DBuilder.TFAnalytics.PriceRangeDist[0]);
            VolumeTDChartVM = new ViewModelValueDistChart<DataTMValueTotalDistribution>(DBuilder.TFAnalytics.VolumeTotalDist[0]);
            PriceRDist = PriceRDChartVM.ValueDist;
            VolumeTDist = VolumeTDChartVM.ValueDist;

            QuotesTextVM = new ViewModelQuotesText();
            QuotesTextStr = QuotesTextVM.TextStr;

            VWAPTextVM = new ViewModelVWAPText(DBuilder.TFAnalytics.VWAP);
            VWAPTextStr = VWAPTextVM.TextStr;

            BtnSlctCSVVM = new ViewModelButtonsSelect();
            BtnSlctCSVVM.AddSrcDS("OHLCVolume", DBuilder.OHLC,"View OHLC Volume: ");
            BtnSlctCSVVM.AddSrcDS("BATVDist", DBuilder.TFAnalytics.BidVolumeDist);
            BtnSlctCSVVM.AddSrcDS("PriceRDist", DBuilder.TFAnalytics.PriceRangeDist);
            BtnSlctCSVVM.AddSrcDS("VolumeTDist", DBuilder.TFAnalytics.VolumeTotalDist);
            BtnSelectBind = BtnSlctCSVVM.BtnBind;

            LabelFormatterEmpty = (d0) => "";
            LabelFormatterAbs = (d0) => Math.Abs(d0).ToString();

            Tuple<string, int> test = new Tuple<string, int> ( "test", 1 );
            object o = (object)test;
            Tuple<string, int> back = (Tuple<string, int>)o;
            
            SelectSourceClick = new RelayCommand(SelectSourceCommand);
        }  

        public void SelectSourceCommand(object o)
        {
            Tuple<string,int> btnbinding = (Tuple<string, int>)o;
            int dsidx = btnbinding.Item2;
            switch (btnbinding.Item1)
            {
                case "OHLCVolume":
                    if (DBuilder.OHLC[dsidx].TSControl.TimeInterval != CSChartVM.DSOHLC.TSControl.TimeInterval)
                    {
                        CSChartVM.ChangeDataSource(DBuilder.OHLC[dsidx], DBuilder.Current.Print);
                        VolumeChartVM.ChangeDataSource(DBuilder.Volume[dsidx]);
                    }
                    break;
                case "BATVDist":
                    if (DBuilder.TFAnalytics.BidVolumeDist[dsidx].TSControl.TimeInterval != 
                        BidVDChartVM.DSValueDist.TSControl.TimeInterval)
                    {
                        BidVDChartVM.ChangeDataSource(DBuilder.TFAnalytics.BidVolumeDist[dsidx], DBuilder.Current.Print);
                        AskVDChartVM.ChangeDataSource(DBuilder.TFAnalytics.AskVolumeDist[dsidx], DBuilder.Current.Print);
                        TradedVDChartVM.ChangeDataSource(DBuilder.TFAnalytics.TradedVolumeDist[dsidx], DBuilder.Current.Print);

                        //BidVDist = BidVDChartVM.ValueDist;
                        //AskVDist = AskVDChartVM.ValueDist;
                        //TradedVDist = TradedVDChartVM.ValueDist;
                        //BATVDistYAxisMin = BATVDistYAxisMax = 0;
                    }
                    break;
                case "PriceRDist":
                    if (DBuilder.TFAnalytics.PriceRangeDist[dsidx].TSControl.TimeInterval !=
                        PriceRDChartVM.DSValueDist.TSControl.TimeInterval)
                    {
                        PriceRDChartVM.ChangeDataSource(DBuilder.TFAnalytics.PriceRangeDist[dsidx], DBuilder.Current.Print);
                        //PriceRDist = PriceRDChartVM.ValueDist;
                    }
                    break;
                case "VolumeTDist":
                    if (DBuilder.TFAnalytics.VolumeTotalDist[dsidx].TSControl.TimeInterval !=
                        VolumeTDChartVM.DSValueDist.TSControl.TimeInterval)
                    {
                        VolumeTDChartVM.ChangeDataSource(DBuilder.TFAnalytics.VolumeTotalDist[dsidx], DBuilder.Current.Print);
                        //VolumeTDist = VolumeTDChartVM.ValueDist;
                    }
                    break;
            }
        }

        public void HandleDataUpdate(object o, EventArgs e)
        {
            App.Current?.Dispatcher.Invoke((Action)delegate
            {
                double[] currentprint = DBuilder.Current.Print;
                CSChartVM.NewData(currentprint);
                if (DBuilder.OHLC[0].RowsChanged == 0)
                {
                    CSLabelFormatter = CSChartVM.XAxisLabelFormatter;
                    NotifyPropertyChanged("CSLabelFormatter");
                }

                VolumeChartVM.NewData();
                BidVDChartVM.NewData(currentprint);
                AskVDChartVM.NewData(currentprint);
                TradedVDChartVM.NewData(currentprint);
                PriceRDChartVM.NewData(currentprint);
                VolumeTDChartVM.NewData(currentprint);
                BATVDistYAxisUpdate();

                QuotesTextVM.NewData(DBuilder.Current.Quotes,DBuilder.Current.QuotesVolume);
                VWAPTextVM.NewData();
            });
        }
        
        private void BATVDistYAxisUpdate()
        {
            // init quick
            if (BATVDistYAxisMin < Core.DOUBLE_EPS)
            {
                BATVDistYAxisMin = BATVDistYAxisMin = DBuilder.Current.Print[3];
                NotifyPropertyChanged("BATVDistYAxisMin");
                NotifyPropertyChanged("BATVDistYAxisMax");
                return;
            }

            // regular update
            double vmin, vmax;
            double mininterval = BidVDChartVM.DSValueDist.MinValueInterval;
            vmin = vmax = DBuilder.Current.Print[3];

            if (vmin > BidVDChartVM.DSValueDist.KeyMinCV)
                vmin = BidVDChartVM.DSValueDist.KeyMinCV;
            if (vmin > AskVDChartVM.DSValueDist.KeyMinCV)
                vmin = AskVDChartVM.DSValueDist.KeyMinCV;
            if (vmin > TradedVDChartVM.DSValueDist.KeyMinCV)
                vmin = TradedVDChartVM.DSValueDist.KeyMinCV;

            if (vmax < BidVDChartVM.DSValueDist.KeyMaxCV)
                vmax = BidVDChartVM.DSValueDist.KeyMaxCV;
            if (vmax < AskVDChartVM.DSValueDist.KeyMaxCV)
                vmax = AskVDChartVM.DSValueDist.KeyMaxCV;
            if (vmax < TradedVDChartVM.DSValueDist.KeyMaxCV)
                vmax = TradedVDChartVM.DSValueDist.KeyMaxCV;
            
            vmin -= mininterval * 2;
            vmax += mininterval * 2;

            // update notify
            if ((vmin - BATVDistYAxisMin < -Core.DOUBLE_EPS) ||
                (vmin - BATVDistYAxisMin > +mininterval * 5))
            {
                BATVDistYAxisMin = vmin;
                NotifyPropertyChanged("BATVDistYAxisMin");
            }

            if ((vmax - BATVDistYAxisMax < -mininterval * 5) ||
                (vmax - BATVDistYAxisMax > +Core.DOUBLE_EPS))
            {
                BATVDistYAxisMax = vmax;
                NotifyPropertyChanged("BATVDistYAxisMax");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string caller)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }
            
    public class RelayCommand : ICommand
    {
        #region Fields 
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        #endregion // Fields 
        
        #region ICommand Members [DebuggerStepThrough] 
        public bool CanExecute(object parameter) { return _canExecute == null ? true : _canExecute(parameter); }
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
        public void Execute(object parameter) { _execute(parameter); }
        #endregion // ICommand Members }

        #region Constructors 
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion // Constructors
    }
}
