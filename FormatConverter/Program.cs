/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;
using Assimp;
using Assimp.Configs;
using Newtonsoft.Json.Linq;
using BUtility;

namespace FormatConverter
{
    class Program
    {
        static int Main(string[] _Args)
        {
            if (_Args == null || _Args.Length < 1)
            {
                Console.WriteLine("Fatal error: One argument must be provided. (GLTF file path)");
                return 1;
            }

            var GLTFPath = _Args[0];

            if (!File.Exists(GLTFPath))
            {
                Console.WriteLine("Fatal error: File does not exist at " + GLTFPath);
                return 1;
            }

            try
            {
                return ConvertGLTFToFBXPath(GLTFPath, Console.WriteLine) ? 0 : 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal error: File conversion has failed with: " + e.Message);
                return 1;
            }
        }

        private static bool ConvertGLTFToFBXPath(string _GLTFPath, Action<string> _LogAction)
        {
            var TmpPathPrefix = Path.GetDirectoryName(_GLTFPath) + "\\" + Path.GetFileNameWithoutExtension(_GLTFPath);
            var FBXPath = TmpPathPrefix + ".fbx";
            var MetadataBinaryPath = TmpPathPrefix + ".bk";

            using (AssimpContext ImporterExporter = new AssimpContext())
            {
                //This is how we add a logging callback 
                using (LogStream LStream = new LogStream(delegate (string _Msg, string _UserData)
                {
                    _LogAction?.Invoke(_Msg);
                }))
                {
                    LStream.Attach();

                    _LogAction?.Invoke("Importing GLTF geometry...");

                    var Model = ImporterExporter.ImportFile(_GLTFPath);

                    _LogAction?.Invoke("Import GLTF geometry operation has been completed.");
                    _LogAction?.Invoke("Exporting geometry to FBX...");

                    if (!ImporterExporter.ExportFile(Model, FBXPath, "fbxa"))
                    {
                        try
                        {
                            File.Delete(FBXPath);
                        }
                        catch (Exception) { }

                        _LogAction?.Invoke("Fatal error: Export geometry to FBX has failed!");

                        return false;
                    }
                }
            }

            BNode RootNode = null;
            {
                JArray NodesJArray;
                {
                    _LogAction?.Invoke("Importing GLTF metadata...");
                    var GLTFText = File.ReadAllText(_GLTFPath);

                    _LogAction?.Invoke("Parsing GLTF metadata as JSON...");
                    var Parsed = JObject.Parse(GLTFText);
                    NodesJArray = (JArray)Parsed["nodes"];
                }

                _LogAction?.Invoke("Building tree for metadata...");
                RootNode = BHelper.BuildTreeFromGLTF(NodesJArray);
            }

            _LogAction?.Invoke("Building metadata object from the tree for metadata and writing it to a file...");
            if (!BHelper.CompileToFile(MetadataBinaryPath, RootNode, _LogAction))
            {
                _LogAction?.Invoke("Fatal error: Cannot compile/save metadata object!");
                return false;
            }

            _LogAction?.Invoke("GLTF to intermediate files operation has successfully been completed!");

            return true;
        }
    }
}