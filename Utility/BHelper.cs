/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

namespace BUtility
{
    partial class BHelper
    {
        public static bool CompileToFile(string _FilePath, BNode _Root, Action<string> _ErrorAction)
        {
            try
            {
                using (var FStream = File.OpenWrite(_FilePath))
                {
                    using (var Compressor = new DeflateStream(FStream, CompressionMode.Compress))
                    {
                        var Serializer = new XmlSerializer(typeof(BNode));
                        Serializer.Serialize(XmlWriter.Create(Compressor), _Root);
                    }
                }
            }
            catch (Exception e)
            {
                _ErrorAction?.Invoke("CompileToFile failed with: " + e.Message);
                return false;
            }
            return true;
        }

        public static bool DecompileFromFile(string _FilePath, out BNode _RootNode, Action<string> _ErrorAction)
        {
            try
            {
                using (FileStream FStream = File.Open(_FilePath, FileMode.Open))
                {
                    using (var Decompressor = new DeflateStream(FStream, CompressionMode.Decompress))
                    {
                        var Deserializer = new XmlSerializer(typeof(BNode));
                        _RootNode = (BNode)Deserializer.Deserialize(XmlReader.Create(Decompressor));
                    }
                }
            }
            catch (Exception e)
            {
                _RootNode = null;
                _ErrorAction?.Invoke("DecompileFromFile failed with: " + e.Message);
                return false;
            }
            return true;
        }
    }
}