using System;

namespace TradeApplication.DataClasses
{
    public class DataCurrent
    {
        public double[] Print { get; private set; }
        public double[] PrintLast { get; private set; }
        public double[] Quotes { get; private set; }
        public int[] QuotesVolume { get; private set; }
        public bool NewPrint { get; private set; }

        public DataCurrent()
        {
            Print = new double[5];
            PrintLast = new double[5];
            Quotes = new double[3];
            QuotesVolume = new int[3];
            NewPrint = false;
        }

        /// <summary>
        /// Process new print string, insert into data array
        /// </summary>
        /// <param name="s">print string in format "DATE,TIME,BID-ASK-TRADED,PRICE,VOLUME"</param>
        public void ProcessPrintStr(string s)
        {
            double[] print_uv = Array.ConvertAll(s.Split(','), double.Parse);

            // minimal data verfication
            NewPrint = print_uv.Length == 5;
            if (NewPrint)
            {
                // convert time to minute count
                print_uv[1] = (Math.Floor(print_uv[1] / 100) * 60) +
                    (print_uv[1] - Math.Floor(print_uv[1] / 100)*100);
                if (print_uv[2] > 2.0) print_uv[2] = 2.0; // change settlement prints 6, to traded 2
                //print_uv[2] = 2.0;
                // update 
                PrintLast = Print;
                Print = print_uv;
                Quotes[(int) Print[2]] = Print[3];
                QuotesVolume[(int)Print[2]] = (int)Print[4];
            }
        }
    }
}
