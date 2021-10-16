/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;

namespace BUtility
{
    class BMetadata
    {
        public string Key;
        public string Value;

        public BMetadata(string _Key, string _Value)
        {
            Key = _Key;
            Value = _Value;
        }
        private BMetadata() {}

        public void Serialize(Stream _Dest)
        {
            {
                var BArray = BitConverter.GetBytes(Key.Length);
                _Dest.Write(BArray, 0, BArray.Length);
            }
            {
                var BArray = System.Text.Encoding.UTF8.GetBytes(Key);
                _Dest.Write(BArray, 0, BArray.Length);
            }

            {
                var BArray = BitConverter.GetBytes(Value.Length);
                _Dest.Write(BArray, 0, BArray.Length);
            }
            {
                var BArray = System.Text.Encoding.UTF8.GetBytes(Value);
                _Dest.Write(BArray, 0, BArray.Length);
            }
        }

        public static BMetadata Deserialize(Stream _Src)
        {
            var Result = new BMetadata();

            int StringSize;

            {
                var BArray = new byte[4];
                _Src.Read(BArray, 0, 4);
                StringSize = BitConverter.ToInt32(BArray, 0);
            }
            {
                var BArray = new byte[StringSize];
                _Src.Read(BArray, 0, StringSize);
                Result.Key = System.Text.Encoding.UTF8.GetString(BArray);
            }

            {
                var BArray = new byte[4];
                _Src.Read(BArray, 0, 4);
                StringSize = BitConverter.ToInt32(BArray, 0);
            }
            {
                var BArray = new byte[StringSize];
                _Src.Read(BArray, 0, StringSize);
                Result.Value = System.Text.Encoding.UTF8.GetString(BArray);
            }

            return Result;
        }
    }
}