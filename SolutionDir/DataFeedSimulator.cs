using System;
using System.IO;
using System.Threading;

namespace TradeApplication
{

    /// <summary>
    /// Datafeed Simulator, use a csv file as data source and simulate data api new print events
    /// </summary>
    class DataFeedSimulator
    {
        public bool SimExternalContinue { get; private set; } // bool for stopping simulation asynchronously
        public bool DataFileHasEnded { get; private set; }
        public delegate void EventStrHandler(object source, EventArgs e, string s);
        public event EventStrHandler PrintUpdate;
        public event EventHandler EndofDataFile;

        private Thread SimThread { get; set; }
        private StreamReader FileRef;

        public DataFeedSimulator()
        {
            SimExternalContinue = false;
        }

        /// <summary>
        /// Set simulation data file, if previous file still open, close file
        /// </summary>
        /// <param name="file_path">csv file path</param>
        public void SetDataFile(string file_path)
        {
            if (!File.Exists(file_path))
                return;

            // if previous file open close file
            if ((FileRef is StreamReader) && (FileRef.BaseStream != null))
                FileRef.Close();
            
            FileRef = new StreamReader(new FileStream(file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        /// <summary>
        /// Start data feed simulation from csv file
        /// </summary>
        /// <param name="file_path">path csv file</param>
        /// <param name="prints_max">max number of prints to simulate, default 0 for entire file</param>
        /// <param name="sleep_ms">thread sleep in milliseconds</param>
        /// <param name="async">simulate asynchronously in seperate thread, default true</param>
        public void StartSimulation(int prints_max = 0, int sleep_ms = 10, bool async = true)
        {
            if (!(FileRef is StreamReader) || (FileRef.BaseStream == null))
                return;

            if (sleep_ms > 0) // if thread is sleeping, must be asynchronous
                async = true;

            SimExternalContinue = true;
            if (async)
            {
                if (SimThread != null)
                    if (SimThread.IsAlive) // cannot start async simulation when another one is running, abort first
                        return;

                SimThread = new Thread(delegate () { SimulationMethod(prints_max, sleep_ms); });
                SimThread.Start();
            }
            else
            {
                SimulationMethod(prints_max,0);
            }
        }
        
        /// <summary>
        /// Start simulation
        /// </summary>
        /// <param name="file_path">csv data source path</param>
        /// <param name="prints_max">simulate n prints</param>
        /// <param name="sleep_ms">thread sleep in milliseconds</param>
        private void SimulationMethod(int prints_max, int sleep_ms)
        {
            string nline;
            int prints_count = 0;

            if (sleep_ms > 0)
            {
                // loop through file till end or max, call PrintUpdate event
                while (SimExternalContinue && ((prints_max == 0) || (prints_count < prints_max)) && ((nline = FileRef.ReadLine()) != null))
                {
                    PrintUpdate?.Invoke(this, EventArgs.Empty, nline);
                    ++prints_count;
                    Thread.Sleep(sleep_ms);
                    //if (nline[14] == '2') // only sleep on traded prints
                    //    Thread.Sleep(2);
                }
            }
            else
            {
                // loop through file till end or max, call PrintUpdate event
                while (SimExternalContinue && ((prints_max == 0) || (prints_count < prints_max)) && ((nline = FileRef.ReadLine()) != null))
                {
                    PrintUpdate?.Invoke(this, EventArgs.Empty, nline);
                    ++prints_count;
                }
            }
            SimExternalContinue = false;

            // if end of data file, close data file and invoke EndofDataFile event listener
            if (FileRef.EndOfStream)
            {
                FileRef.Close();
                DataFileHasEnded = true;
                EndofDataFile?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Abort async datafeed simulation, should only be used on application close
        /// </summary>
        public void StopSimulation()
        {
            SimExternalContinue = false;
        }

        /// <summary>
        /// Check async datafeed simulation is running
        /// </summary>
        /// <returns></returns>
        public bool SimulationIsRunning()
        {
            if (SimThread == null)
                return false; // checking synchronously therefore false
            return SimThread.IsAlive;
        }
    }
}
