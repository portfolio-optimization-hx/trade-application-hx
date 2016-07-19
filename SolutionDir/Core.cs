using System;

namespace TradeApplication
{
    public static class Core
    {
        public static int MINUTES_IN_DAY = 1440;
        public static int[] ORIGIN_DATE = new int[2] { 00010101, 0000 }; // for date functionalities
        public static int PRINT_TYPE_TRADED = 2;
        public static double TICK_SIZE = 0.01;
        public static double DOUBLE_EPS = 0.00000001; // really only needs 2 digit accuracy
        
        public static int MinutesDifference(int d0, int t0, int d1, int t1)
        {
            int min_diff; // time difference in minutes

            if (d0 != d1)
            {
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

        public static int DateToDateInt(DateTime d0)
        {
            return d0.Year * 10000 + d0.Month * 100 + d0.Day;
        }

        public static void PrintArray<T>(T[] arr0)
        {
            for (int i0 = 0; i0 < arr0.Length; ++i0)
                Console.WriteLine(arr0[i0].ToString());
        }

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
