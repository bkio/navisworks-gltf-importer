/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;

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
                    _Root.Serialize(FStream);
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
                    _RootNode = BNode.Deserialize(FStream);
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