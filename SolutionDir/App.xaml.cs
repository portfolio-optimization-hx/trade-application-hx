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
    public partial class App : Application
    {
        private DataFeedSimulator DataFeedSim { get; set; }
        private DataBuilder DBuilder { get; set; }
        private MainWindow MWindow { get; set; }

        public App()
        {
            // initialize DataFeedSimulator and DataBuilder objects
            // change simulator to datafeed api to handle real-time data
            DataFeedSim = new DataFeedSimulator();
            DBuilder = new DataBuilder();

            // build TimeSeries OHLC and Volume with different time settings
            DBuilder.NewTimeSeriesOHLCVolume(1);
            DBuilder.NewTimeSeriesOHLCVolume(5);
            DBuilder.NewTimeSeriesOHLCVolume(10);
            DBuilder.NewTimeSeriesOHLCVolume(17);
            DBuilder.NewTimeSeriesOHLCVolume(30);
            DBuilder.NewTimeSeriesOHLCVolume(60);

            // create TimeFrame with default time and value settings.
            // it would greatly enhance the application to handle dynamic
            // data object construction from user input
            DBuilder.NewTimeFrameAnalytics();

            // create GUI window
            MWindow = new MainWindow(new MainWindowViewModel(DBuilder));

            // add event listeners
            DataFeedSim.PrintUpdate += DBuilder.HandlePrintUpdate; // DataBuilder listen to new prints event from DataFeed Simulator
            DBuilder.DataUpdate += MWindow.VM.HandleDataUpdate; // on data update, update GUI
            MWindow.VM.SimControlEvent += HandleSimControlClick; // on SimControl button click event, DataFeed responding accordingly
            MWindow.WindowClosed += HandleWindowClosed; // add window close, app shutdown to main window closed event
            DataFeedSim.EndofDataFile += EndofDataFileDialog; // end of data file dialog on DataFeedSimulator end of data file event

            // file paths
            string data_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\");
            string zip_path = Path.Combine(data_dir, @"trade_data.zip");
            string csv_path = Path.Combine(data_dir, @"trade_data.csv");

            if (!File.Exists(csv_path))
            {
                // if data csv file does not exist, check if data zip file exist and extract
                if (File.Exists(zip_path))
                    ZipFile.ExtractToDirectory(zip_path, data_dir);
                else
                    MessageBox.Show(string.Format("Please ensure the following directory contains either trade_data.csv or trade_data.zip. A sample trade_data.zip is found in `SolutionDir\\Data`\n     {0}\n\nApplication will shutdown after message box close.", data_dir),
                        "Can Not Find Data File");
            }
            
            // start simulation if data csv file exist, otherwise shutdown
            if (File.Exists(csv_path))
            {
                MWindow.Show();
                DataFeedSim.SetDataFile(csv_path);
                DataFeedSim.StartSimulation();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Handle Simulation User Interface Button Click Event
        /// </summary>
        /// <param name="mode">0: pause, 1: continue, 2: foward</param>
        /// <param name="nprints">forward only, number prints to simulate forward</param>
        public void HandleSimControlClick(int mode, int nprints)
        {
            if (DataFeedSim.DataFileHasEnded)
            {
                MessageBox.Show("Simulation at end of data file. To restart simulation, please close and restart the application.", "End of Data File");
                return;
            }

            switch (mode)
            {
                case 0:
                    // pause
                    DataFeedSim.StopSimulation();
                    DBuilder.DataUpdate -= MWindow.VM.HandleDataUpdate;
                    break;
                case 1:
                    // continue
                    DataFeedSim.StartSimulation();
                    DBuilder.DataUpdate += MWindow.VM.HandleDataUpdate;
                    break;
                case 2:
                    // foward
                    if (!DataFeedSim.SimExternalContinue)
                    {
                        DataFeedSim.StartSimulation(nprints, 0, false); // run simulation synchronously nprints with no pause in between
                        if (!DataFeedSim.DataFileHasEnded)
                            MWindow.VM.NewData(true); // update graphical user interface
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle window close, shutdown simulation then application
        /// </summary>
        public void HandleWindowClosed(object o, EventArgs e)
        {
            // end simulation, shutdown app on main window close
            DataFeedSim.StopSimulation();
            Application.Current.Shutdown();
        }
        
        /// <summary>
        /// Handle DataFeedSimulation end of data file
        /// </summary>
        public void EndofDataFileDialog(object o, EventArgs e)
        {
            MWindow.VM.NewData(true);
            MessageBox.Show("Simulation at end of data file. To restart simulation, please close and restart the application.", "End of Data File");
        }
    }
}