using System;
using System.IO;
using System.Threading;

namespace TradeApplication
{

    class DataFeedSimulator
    {
        //public event EventHandler NewPrint;
        public delegate void EventStrHandler(object source, EventArgs e, string s);
        public event EventStrHandler PrintUpdate;
        private Thread SimThread { get; set; }

        public DataFeedSimulator()
        {
        }
        
        /// <summary>
        /// Start data feed simulation from csv file
        /// </summary>
        /// <param name="file_path">path csv file</param>
        /// <param name="prints_max">max number of prints to simulate, default 0 for entire file</param>
        /// <param name="async">simulate asynchronously in seperate thread, default true</param>
        public void StartSimulation(string file_path, int prints_max = 0, bool async = true)
        {
            if (!File.Exists(file_path))
                throw new ArgumentException("File does not exist.");

            if (async)
            {
                if (SimThread != null)
                    if (SimThread.IsAlive) // cannot start async simulation when another one is running, abort first
                        return;

                SimThread = new Thread(delegate () { SimulationMethod(file_path, prints_max); });
                SimThread.Start();
            }
            else
            {
                SimulationMethod(file_path, prints_max);
            }
        }

        public void AbortSimulation()
        {
            if (SimThread != null)
                if (SimThread.IsAlive)
                    SimThread.Abort();
        }

        public bool SimulationIsRunning()
        {
            if (SimThread == null)
                return false; // checking synchronously therefore false
            return SimThread.IsAlive;
        }

        private void SimulationMethod(string file_path, int prints_max)
        {
            StreamReader file = new StreamReader(file_path);
            string nline;
            int prints_count = 0;

            while (((prints_max == 0) || (prints_count < prints_max)) && ((nline = file.ReadLine()) != null))
            {
                PrintUpdate?.Invoke(this, EventArgs.Empty, nline);
                ++prints_count;
                Thread.Sleep(1);
                //if (nline[14] == '2')
                //    Thread.Sleep(2);
            }
            file.Close();
        }
    }
}
