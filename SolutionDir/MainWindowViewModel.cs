using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using OxyPlot;
using TradeApplication.DataClasses;
using TradeApplication.ViewModels;

namespace TradeApplication
{
    /// <summary>
    /// MainWindow ViewModel, create Chart ViewModels and public ObservableCollections for View binding.
    /// Object also manages and propagates new data, updates, axis control, and user interaction
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public readonly DataBuilder DBuilder;
        
        // candlestick chart properties
        public ViewModelCandleStickChart CSChartVM { get; private set; }
        public ObservableCollection<DataPoint> CSOpenCloseUp { get; private set; } // green candles
        public ObservableCollection<DataPoint> CSOpenCloseDown { get; private set; } // red candles
        public ObservableCollection<DataPoint> CSHighLowUp { get; private set; } // green stick
        public ObservableCollection<DataPoint> CSHighLowDown { get; private set; } // red stick

        // volume chart properties
        public ViewModelVolumeChart VolumeChartVM { get; private set; }
        public ObservableCollection<DataPoint> BidVolume { get; private set; }
        public ObservableCollection<DataPoint> AskVolume { get; private set; }
        public ObservableCollection<DataPoint> TradedVolume { get; private set; }

        // candlestick and volume chart axis properties
        public int CSXAxisMax { get; private set; }
        public Func<double, string> CSLabelFormatter { get; private set; }

        // bid ask traded value distribution chart properties
        public ViewModelValueDistChart<DataTMValueDistribution> BidVDChartVM { get; private set; }
        public ViewModelValueDistChart<DataTMValueDistribution> AskVDChartVM { get; private set; }
        public ViewModelValueDistChart<DataTMValueDistribution> TradedVDChartVM { get; private set; }
        public ObservableCollection<DataPoint> BidVDist { get; private set; }
        public ObservableCollection<DataPoint> AskVDist { get; private set; }
        public ObservableCollection<DataPoint> TradedVDist { get; private set; }

        // bid ask traded value distribution chart axis properties
        public double BATVDistYAxisMin { get; private set; }
        public double BATVDistYAxisMax { get; private set; }
        public Func<double, string> LabelFormatterEmpty { get; private set; }
        public Func<double, string> LabelFormatterAbs { get; private set; }

        // price range, volume total value distribution chart properties
        public ViewModelValueDistChart<DataTMValueRangeDistribution> PriceRDChartVM { get; private set; }
        public ViewModelValueDistChart<DataTMValueTotalDistribution> VolumeTDChartVM { get; private set; }
        public ObservableCollection<DataPoint> PriceRDist { get; private set; }
        public ObservableCollection<DataPoint> VolumeTDist { get; private set; }
        
        // quotes text properties
        public ViewModelQuotesText QuotesTextVM { get; private set; }
        public ObservableCollection<string> QuotesTextStr { get; private set; }

        // vwap text properties
        public ViewModelVWAPText VWAPTextVM { get; private set; }
        public Collection<ObservableCollection<string>> VWAPTextStr { get; private set; }

        // data source selection button properties
        public ViewModelButtonsSelect BtnSlctCSVVM { get; private set; }
        public Collection<ObservableCollection<ViewModelButtonsSelect.ButtonBindings>> BtnSelectBind { get; private set; }

        public ICommand SelectSourceClick { get; set; }
                
        /// <summary>
        /// MainWindow constructor, create chart ViewModels setup ObservableCollections for View binding
        /// </summary>
        /// <param name="dbuilder">DataBuilder object, data source for charts</param>
        public MainWindowViewModel(DataBuilder dbuilder)
        {
            DBuilder = dbuilder;

            // candlestick and volume charts
            int nseries = 30; // number of data points in candlestick and volume

            CSChartVM = new ViewModelCandleStickChart(DBuilder.OHLC[0], nseries);
            CSOpenCloseUp = CSChartVM.CSOpenCloseUp;
            CSOpenCloseDown = CSChartVM.CSOpenCloseDown;
            CSHighLowUp = CSChartVM.CSHighLowUp;
            CSHighLowDown = CSChartVM.CSHighLowDown;

            VolumeChartVM = new ViewModelVolumeChart(DBuilder.Volume[0], nseries);
            BidVolume = VolumeChartVM.VBid;
            AskVolume = VolumeChartVM.VAsk;
            TradedVolume = VolumeChartVM.VTraded;
            
            CSXAxisMax = CSChartVM.SDataCount + 1; // axis
            CSLabelFormatter = CSChartVM.XAxisLabelFormatter; // label formatter

            // bid ask trade value distribution charts
            BidVDChartVM = new ViewModelValueDistChart<DataTMValueDistribution>(DBuilder.TFAnalytics.BidVolumeDist[0],"vertical", "negative");
            AskVDChartVM = new ViewModelValueDistChart<DataTMValueDistribution>(DBuilder.TFAnalytics.AskVolumeDist[0], "vertical");
            TradedVDChartVM = new ViewModelValueDistChart<DataTMValueDistribution>(DBuilder.TFAnalytics.TradedVolumeDist[0], "vertical","negative");
            BidVDist = BidVDChartVM.ValueDist;
            AskVDist = AskVDChartVM.ValueDist;
            TradedVDist = TradedVDChartVM.ValueDist;

            BATVDistYAxisMin = BATVDistYAxisMax = 0; // axis
            LabelFormatterEmpty = (d0) => ""; // label formatter empty
            LabelFormatterAbs = (d0) => Math.Abs(d0).ToString(); // label formmatter absolute value

            // price range, volume total value distribution charts
            PriceRDChartVM = new ViewModelValueDistChart<DataTMValueRangeDistribution>(DBuilder.TFAnalytics.PriceRangeDist[0]);
            VolumeTDChartVM = new ViewModelValueDistChart<DataTMValueTotalDistribution>(DBuilder.TFAnalytics.VolumeTotalDist[0]);
            PriceRDist = PriceRDChartVM.ValueDist;
            VolumeTDist = VolumeTDChartVM.ValueDist;

            // quotes text display
            QuotesTextVM = new ViewModelQuotesText(DBuilder.Current);
            QuotesTextStr = QuotesTextVM.TextStr;

            // vwap text display
            VWAPTextVM = new ViewModelVWAPText(DBuilder.TFAnalytics.VWAP);
            VWAPTextStr = VWAPTextVM.TextStr;

            // add data source selection buttons each set of charts
            BtnSlctCSVVM = new ViewModelButtonsSelect();
            BtnSlctCSVVM.AddSrcDS("OHLCVolume", DBuilder.OHLC,"View OHLC Volume: ");
            BtnSlctCSVVM.AddSrcDS("BATVDist", DBuilder.TFAnalytics.BidVolumeDist);
            BtnSlctCSVVM.AddSrcDS("PriceRDist", DBuilder.TFAnalytics.PriceRangeDist);
            BtnSlctCSVVM.AddSrcDS("VolumeTDist", DBuilder.TFAnalytics.VolumeTotalDist);
            BtnSelectBind = BtnSlctCSVVM.BtnBind;
            
            // selection button click command handler
            SelectSourceClick = new RelayCommand(SelectSourceCommand);
        }  

        /// <summary>
        /// Selection button click command handler
        /// </summary>
        /// <param name="o">button event object</param>
        public void SelectSourceCommand(object o)
        {
            string srcname;
            int dsidx;

            // event object should be a tuple created by ViewModelButtonsSelect, see ViewModelButtonsSelect.ButtonBindings.BtnParameters
            // try to convert event object to <string,int> tuple and assign srcname, and dsidx, else return
            try
            {
                Tuple<string, int> btnbinding = (Tuple<string, int>)o; // convert object back to tuple
                srcname = btnbinding.Item1;
                dsidx = btnbinding.Item2;
            }
            catch
            {
                return;
            }


            // if not already assigned to dsidx, switch chart data source to dsidx using ViewModel ChangeDataSource()
            switch (srcname)
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
                        BidVDChartVM.ChangeDataSource(DBuilder.TFAnalytics.BidVolumeDist[dsidx]);
                        AskVDChartVM.ChangeDataSource(DBuilder.TFAnalytics.AskVolumeDist[dsidx]);
                        TradedVDChartVM.ChangeDataSource(DBuilder.TFAnalytics.TradedVolumeDist[dsidx]);
                    }
                    break;
                case "PriceRDist":
                    if (DBuilder.TFAnalytics.PriceRangeDist[dsidx].TSControl.TimeInterval !=
                        PriceRDChartVM.DSValueDist.TSControl.TimeInterval)
                    {
                        PriceRDChartVM.ChangeDataSource(DBuilder.TFAnalytics.PriceRangeDist[dsidx]);
                    }
                    break;
                case "VolumeTDist":
                    if (DBuilder.TFAnalytics.VolumeTotalDist[dsidx].TSControl.TimeInterval !=
                        VolumeTDChartVM.DSValueDist.TSControl.TimeInterval)
                    {
                        VolumeTDChartVM.ChangeDataSource(DBuilder.TFAnalytics.VolumeTotalDist[dsidx]);
                    }
                    break;
            }
        }

        /// <summary>
        /// Event handler for DataBuilder.DataUpdate event, call corresponding chart ViewModel new data methods
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void HandleDataUpdate(object o, EventArgs e)
        {
            App.Current?.Dispatcher.Invoke((Action)delegate // invoke in GUI thread
            {
                double[] currentprint = DBuilder.Current.Print;
                CSChartVM.NewData(currentprint);
                if (DBuilder.OHLC[0].RowsChanged == 0)
                {
                    // notify label formatter change
                    CSLabelFormatter = CSChartVM.XAxisLabelFormatter;
                    NotifyPropertyChanged("CSLabelFormatter");
                }

                VolumeChartVM.NewData();
                BidVDChartVM.NewData();
                AskVDChartVM.NewData();
                TradedVDChartVM.NewData();
                PriceRDChartVM.NewData();
                VolumeTDChartVM.NewData();
                BATVDistYAxisUpdate();

                QuotesTextVM.NewData();
                VWAPTextVM.NewData();
            });
        }
        
        /// <summary>
        /// Update, align Bid, Ask, Traded distribution chart axis
        /// </summary>
        private void BATVDistYAxisUpdate()
        {
            // init quick update
            if (BATVDistYAxisMin < Core.DOUBLE_EPS)
            {
                BATVDistYAxisMin = BATVDistYAxisMin = DBuilder.Current.Print[3];
                NotifyPropertyChanged("BATVDistYAxisMin");
                NotifyPropertyChanged("BATVDistYAxisMax");
                return;
            }

            // regular update, find min max of Bid, Ask, Traded values
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

            // update and notify
            // axis min max increase as value bounds increase, delayed decrease as value bounds decreate
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

        // INotifyPropertyChanged class PropertyChanged property and method
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string caller)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }
            
    /// <summary>
    /// RelayCommand extend ICommand interface
    /// </summary>
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        
        public bool CanExecute(object parameter) { return _canExecute == null ? true : _canExecute(parameter); }
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
        public void Execute(object parameter) { _execute(parameter); }
        
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }
    }
}
