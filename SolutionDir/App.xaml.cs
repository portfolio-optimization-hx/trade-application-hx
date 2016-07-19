using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;

namespace TradeApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App : Application
    {
        private DataFeedSimulator DataFeedSim { get; set; }
        private DataBuilder DBuilder { get; set; }
        private MainWindow MWindow { get; set; }

        public App()
        {
            //try
            //{
            DataFeedSim = new DataFeedSimulator();
            DBuilder = new DataBuilder();

            DBuilder.NewTimeSeriesOHLCVolume(1);
            DBuilder.NewTimeSeriesOHLCVolume(5);
            DBuilder.NewTimeSeriesOHLCVolume(10);
            DBuilder.NewTimeSeriesOHLCVolume(17);
            DBuilder.NewTimeSeriesOHLCVolume(30);
            DBuilder.NewTimeSeriesOHLCVolume(60);
            DBuilder.NewTimeFrameAnalytics();

            MWindow = new MainWindow(new MainWindowViewModel(DBuilder));
            DataFeedSim.PrintUpdate += DBuilder.HandlePrintUpdate;
            DBuilder.DataUpdate += MWindow.VM.HandleDataUpdate;
            MWindow.WindowClosed += HandleWindowClosed;

            string data_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\");
            string zip_path = Path.Combine(data_dir, @"trade_data.zip");
            string csv_path = Path.Combine(data_dir, @"trade_data.csv");

            if (!File.Exists(csv_path))
            {
                if (File.Exists(zip_path))
                    ZipFile.ExtractToDirectory(zip_path, data_dir);
                else
                    MessageBox.Show(string.Format("Please ensure the following directory contains either trade_data.csv or trade_data.zip.\n\n     {0}\n\nApplication will shutdown after message box close.", data_dir),
                        "Can Not Find Data File");
            }

            if (File.Exists(csv_path))
            {
                MWindow.Show();
                DataFeedSim.StartSimulation(csv_path);
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        public void HandleWindowClosed(object o, EventArgs e)
        {
            DataFeedSim.AbortSimulation();
            Application.Current.Shutdown();
        }
    }
}
