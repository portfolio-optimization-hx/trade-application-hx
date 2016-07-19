# trade-application-hx
Trade Application / Dashboard in C#

![gif demo](/SolutionDir/media/demo.gif)

__trade-application-hx__ is created to showcase desktop development rather than quantitative modeling - the analytics in this project is very basic. The code is currently in alpha and some areas can be further polished.

&nbsp;

# general_info

__trade-application-hx__ is developed with .NET.Framework V4.0.30319 using OxyPlot as the WPF charting library. A release build `TradeApplication.exe` is available in [/SolutionDir/bin/Release](/SolutionDir/bin/Release).

The application takes a `trade_data.csv` in the `Data\` directory relative to the executable root as DataFeedSimulator source. A sample zipped csv file of real trading data is provided. The prices are uniformly altered and the prints are in the format `[date, time, type, price, volume]`

Using the data feed, the application will asynchronously new print events and create the follwing:

* __OHLC__ - standard Open-High-Low-Close timeseries data for Traded prints only

* __Volume__ - aggregate Bid<sup>1</sup>, Ask<sup>1</sup>, Traded volume.

* __Quotes__ - Price and Volume on the current / last Bid, Ask, Traded prints

* __VWAP__ - Volume Weighted Average Price in given timeframe (in minutes)

* __BATV Dist__ - Bid<sup>1</sup>, Ask<sup>1</sup>, Traded volume distribution for a given price in x timeframe, updated every y minutes, value in z interval<sup>2</sup>

* __PriceRange Dist__ - price range distribution in x timeframe, updated every y minutes, value in z interval<sup>2</sup>

* __Total Volume Dist__ - total Traded volume distribution in x timeframe, updated every y minutes, value in z interval

<sup>1</sup>_volume on the Bid and Ask prints are updates to the volume, so the sum is not very meanful and used for demo purposes_

<sup>2</sup>_eg. 60 minute timeframe updated everyminute, 0.01 value interval for price, 100 value interval for volume_

*__Note:__ all time in minutes, data classes can handle non traditional time parameters like 17 mins.*
