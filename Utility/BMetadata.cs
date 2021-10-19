/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;

namespace BUtility
{
    public class BMetadata
    {
        public string Key;
        public string Value;

        public BMetadata(string _Key, string _Value)
        {
            Key = _Key;
            Value = _Value;
        }
        private BMetadata() {}
    }
}