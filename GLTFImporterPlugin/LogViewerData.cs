/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace GLTFImporterPlugin
{
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }));
        }
    }

    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }

        public LogEntry(string _Message)
        {
            Message = _Message;
            DateTime = DateTime.Now;
            lock (InternalIndex_Lock)
            {
                Index = InternalIndex++;
            }
        }

        private static int InternalIndex = 0;
        private static object InternalIndex_Lock = new object();
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public CollapsibleLogEntry(string _Message) : base(_Message) { }

        public List<LogEntry> Contents { get; set; }
    }

}
