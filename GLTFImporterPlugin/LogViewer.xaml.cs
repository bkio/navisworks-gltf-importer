/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace GLTFImporterPlugin
{
    public partial class LogViewer : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var Hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(Hwnd, GWL_STYLE, GetWindowLong(Hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
        }

        public ObservableCollection<LogEntry> LogEntries { get; set; }
        public void ClearEntries()
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(() => LogEntries.Clear()));
            }
            catch (Exception) { }
        }

        public LogViewer()
        {
            InitializeComponent();

            DataContext = LogEntries = new ObservableCollection<LogEntry>();
        }

        public void AddEntry(LogEntry _Entry)
        {
            try
            {
                if (_Entry == null || _Entry.Message == null ||_Entry.Message.Length == 0) return;
                Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(_Entry)));
            }
            catch (Exception) { }
        }

        private bool AutoScroll = true;
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if ((e.Source as ScrollViewer).VerticalOffset == (e.Source as ScrollViewer).ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                (e.Source as ScrollViewer).ScrollToVerticalOffset((e.Source as ScrollViewer).ExtentHeight);
            }
        }
    }
}
