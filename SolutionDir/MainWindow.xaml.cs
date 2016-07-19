using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace TradeApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    partial class MainWindow : Window
    {
        public MainWindowViewModel VM { get; private set; }
        public event EventHandler WindowClosed;

        public MainWindow(MainWindowViewModel vm)
        {
            NewViewModel(vm);
            BindingErrorListener.Listen(m => MessageBox.Show(m));
            InitializeComponent();
        }

        private void NewViewModel(MainWindowViewModel vm)
        {
            VM = vm;
            DataContext = vm;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    public class BindingErrorListener : TraceListener
    {
        private Action<string> logAction;
        public static void Listen(Action<string> logAction)
        {
            PresentationTraceSources.DataBindingSource.Listeners
                .Add(new BindingErrorListener() { logAction = logAction });
        }
        public override void Write(string message) { }
        public override void WriteLine(string message)
        {
            logAction(message);
        }
    }

}
