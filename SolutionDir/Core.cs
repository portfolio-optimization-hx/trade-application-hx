using System;

namespace TradeApplication
{
    /// <summary>
    /// Core, static variable and method available acrss the application
    /// </summary>
    public static class Core
    {
        public static int MINUTES_IN_DAY = 1440; // 1440 minutes in a day
        public static int[] ORIGIN_DATE = new int[2] { 00010101, 0000 }; // for date functionalities
        public static int PRINT_TYPE_TRADED = 2; // print type traded is 2
        public static double TICK_SIZE = 0.01; // minimum print price difference
        public static double DOUBLE_EPS = 0.00000001; // epsilon for float calculate, really only needs 2 digit accuracy
        
        /// <summary>
        /// Calculate the number of minutes d0, t0 is away from d1, t1
        /// </summary>
        /// <param name="d0">date 0</param>
        /// <param name="t0">time 0</param>
        /// <param name="d1">date 1</param>
        /// <param name="t1">time 1</param>
        /// <returns></returns>
        public static int MinutesDifference(int d0, int t0, int d1, int t1)
        {
            int min_diff; // time difference in minutes

            if (d0 != d1)
            {
                // if different day, add number of days x 1440 hours per day minus time 
                TimeSpan ts = DateTime.Parse(d0.ToString("0000-00-00")) - DateTime.Parse(d1.ToString("0000-00-00"));
                min_diff = (int)ts.TotalMinutes;
                min_diff = min_diff - 1440 + (1440 - t1) + t0;
            }
            else
            {
                min_diff = t0 - t1;
            }

            return min_diff;
        }

        /// <summary>
        /// Convert DateTime object to date int YYYYMMDD
        /// </summary>
        /// <param name="d0">DateTime object</param>
        /// <returns></returns>
        public static int DateToDateInt(DateTime d0)
        {
            return d0.Year * 10000 + d0.Month * 100 + d0.Day;
        }

        /// <summary>
        /// Print generic one dimension array
        /// </summary>
        /// <typeparam name="T">define array type</typeparam>
        /// <param name="arr0">array</param>
        public static void PrintArray<T>(T[] arr0)
        {
            for (int i0 = 0; i0 < arr0.Length; ++i0)
                Console.WriteLine(arr0[i0].ToString());
        }

        /// <summary>
        /// Print generic two dimension array
        /// </summary>
        /// <typeparam name="T">define array type</typeparam>
        /// <param name="arr0">array</param>
        public static void PrintArray<T>(T[,] arr0)
        {
            for (int ri0 = 0; ri0 < arr0.GetLength(0); ++ri0)
            {
                for (int ci0 = 0; ci0 < arr0.GetLength(1); ++ci0)
                {
                    Console.Write(arr0[ri0, ci0].ToString());
                    Console.Write(" ");
                }
                    
                Console.WriteLine("");
            }
        }
    }
}
