using System;

namespace TradeApplication.DataClasses
{
    /// <summary>
    /// DEPRECATED, NOT USED AT THE MOMENT
    /// </summary>
    class DataPrint
    {
        private int row_idx;
        private int data_size;
        private double[,] data;

        public DataPrint(int init_size = 10000)
        {
            data_size   = init_size;
            row_idx     = data_size;
            data        = new double[data_size, 5];
        }

        /// <summary>
        /// Process new print string, insert into data array
        /// </summary>
        /// <param name="s">print string in format "DATE,TIME,BID-ASK-TRADED,PRICE,VOLUME"</param>
        public void ProcessNewPrintStr(string s)
        {
            ++row_idx;
            if (row_idx >= data_size) { row_idx = 0; }
            Buffer.BlockCopy(Array.ConvertAll(s.Split(','), double.Parse), 0, data,row_idx*20, 20);
            data[row_idx, 2] = ((double) Math.Floor(data[row_idx, 2] / 100) * 60) +
                (data[row_idx,2] - (double) Math.Floor(data[row_idx, 2] / 100));
        }

        public double[] GetCurrentPrint()
        {
            double[] data_row = new double[5];
            Buffer.BlockCopy(data, row_idx * 20, data_row, 0, 20);
            return data_row;
        }

        public void Print()
        {
            for (int ri=0; ri < row_idx; ++ri)
            {
                for (int ci=0; ci < 5; ++ci)
                {
                    Console.Write(data[ri, ci].ToString("      0.00"));
                }
                Console.WriteLine("");
            }
        }
    }
}
