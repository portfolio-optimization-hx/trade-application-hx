# trade-application-hx
Trade Application / Dashboard in C#

Download `Demo.zip` and run `TradeApplication.exe` for a compiled application or check [SolutionDir/bin/Release/](SolutionDir/bin/Release/) for the release build.

![gif demo](/SolutionDir/media/demo.gif)

__trade-application-hx__ is created to showcase desktop development rather than quantitative modeling - the analytics in this project is very basic. The code is currently in alpha and some areas can be further polished.

&nbsp;

# general_info

__trade-application-hx__ is developed with .NET.Framework V4.0.30319 using OxyPlot as the WPF charting library.

The application takes a `trade_data.csv` in the `Data\` directory relative to the executable root as DataFeedSimulator source. A sample zipped csv file of real trading data is provided. The prices are uniformly altered and the prints are in the format `[date, time, type, price, volume]`

Using the data feed, the application will asynchronously new print events and create the follwing:

* __OHLC__ - standard Open-High-Low-Close timeseries data for Traded prints only

* __Volume__ - aggregate Bid<sup>1</sup>, Ask<sup>1</sup>, Traded volume.

* __Quotes__ - Price and Volume on the current / last Bid, Ask, Traded prints

* __VWAP__ - Volume Weighted Average Price in given timeframe (in minutes)

* __BATV Dist__ - Bid<sup>1</sup>, Ask<sup>1</sup>, Traded volume distribution for a given price in x timeframe, updated every y minutes, value in z interval<sup>2</sup>

* __PriceRange Dist__ - price range distribution in x timeframe, updated every y minutes, value in z interval<sup>2</sup>

* __Total Volume Dist__ - total Traded volume distribution in x timeframe, updated every y minutes, value in z interval

&nbsp;

<sup>1</sup> *volume on the Bid and Ask prints are updates to the volume, so the sum is not very meanful and used for demo purposes*

<sup>2</sup> *eg. 60 minute timeframe updated everyminute, 0.01 value interval for price, 100 value interval for volume*

*__Note:__ all time in minutes, data classes can handle non traditional time parameters like 17 mins.*

&nbsp;

# performance

Trade Application is developed with performance in mind. Without the graphical user interface<sup>1</sup>, the program processes 4 million+ prints in the sample data file in around 20 seconds. Once the graphical user interface is added, the simulation will take hours. Depending on `Thread.Sleep` settings in `SolutionDir\DataSimulator.cs` method `SimulationMethod`, the display source buttons may be less responsive. To address this the user interface can be change to only update on Traded prints or other filtering methods.

&nbsp;

<sup>1</sup> *To do so, comment out line*

  `DBuilder.DataUpdate += MWindow.VM.HandleDataUpdate;` in `SolutionDir\App.xaml.cs`

  `Thread.Sleep(1);` in `SolutionDir\DataFeedSimulator.cs` 

&nbsp;

# task_list

Some potential future work for this project, in no particular order:

* improve documentation and comments
* add test cases
* unify chart viewmodels to inherit from common parent class, common parameter data source
* add simulation control methods and user interface
  * start, stop, pause, continue simulation
  * run x prints in background
  * start simulation at x prints
* add functionality to allow user defined inputs, layout
* multi-panelled user interface
