/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;
using BUtility;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using ComApiBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;

namespace GLTFImporterPlugin
{
    class OperationManager
    {
        private OperationManager() { }
        private static OperationManager Instance = null;
        public static OperationManager Get()
        {
            if (Instance == null)
            {
                Instance = new OperationManager();
            }
            return Instance;
        }

        public bool IsThereAnActiveProcess()
        {
            return bThereIsAnActiveProcess;
        }
        private bool bThereIsAnActiveProcess = false;

        private string LastFBXFilePath;
        private string LastMetadataFilePath;
        private string LastNWDFilePath;
        private string LastNWCFilePath;

        public bool ConvertGLTFToFBXandBinary(Action<bool> _OnFormatConversionComplete, string _GLTFFilePath, Action<string> _LogMessageAction)
        {
            bThereIsAnActiveProcess = true;

            var FormatConverterExePath = AppDomain.CurrentDomain.BaseDirectory + "\\Plugins\\GLTFImporterPlugin\\FormatConverter.exe";

            var GLTFDirectory = Path.GetDirectoryName(_GLTFFilePath);
            var FileNameWithoutExtension = Path.GetFileNameWithoutExtension(_GLTFFilePath);
            LastFBXFilePath = GLTFDirectory + "\\" + FileNameWithoutExtension + ".fbx";
            LastMetadataFilePath = GLTFDirectory + "\\" + FileNameWithoutExtension + ".bk";
            LastNWDFilePath = GLTFDirectory + "\\" + FileNameWithoutExtension + ".nwd";
            LastNWCFilePath = GLTFDirectory + "\\" + FileNameWithoutExtension + ".nwc";

            if (!ProcessHelper.RunProcess((bool _bSuccess) =>
            {
                bThereIsAnActiveProcess = false;
                _OnFormatConversionComplete?.Invoke(_bSuccess);

            }, FormatConverterExePath, _GLTFFilePath, GLTFDirectory, null, _LogMessageAction))
            {
                bThereIsAnActiveProcess = false;
                return false;
            }
            return true;
        }

        public bool ImportFBXAndPopulateMetadataFromBinary_BackgroundThread(out BNode _RootBNode, Action<string> _LogMessageAction)
        {
            _RootBNode = null;

            if (!File.Exists(LastFBXFilePath) || !File.Exists(LastMetadataFilePath))
            {
                _LogMessageAction?.Invoke("Fatal error: Intermediate files do not exist!");
                return false;
            }

            if (!BHelper.DecompileFromFile(LastMetadataFilePath, out _RootBNode, _LogMessageAction))
            {
                _LogMessageAction?.Invoke("Fatal error: Build tree from metadata file operation has failed!");
                return false;
            }

            return true;
        }

        public bool ImportFBXAndPopulateMetadataFromBinary_MainThread(BNode _RootBNode, Action<string> _LogMessageAction)
        {
            Autodesk.Navisworks.Api.Application.MainDocument.Clear();

            try
            {
                Autodesk.Navisworks.Api.Application.MainDocument.OpenFile(LastFBXFilePath);
            }
            catch (Autodesk.Navisworks.Api.DocumentFileException e)
            {
                _LogMessageAction?.Invoke("Fatal error: Open intermediate file has failed with: " + e.Message);
                return false;
            }

            Autodesk.Navisworks.Api.ModelItem RootNNode = null;
            try
            {
                RootNNode = Autodesk.Navisworks.Api.Application.MainDocument.Models.First.RootItem;
            }
            catch (Exception e)
            {
                _LogMessageAction?.Invoke("Fatal error: Access to the root node in Navisworks has failed with: " + e.Message);
                return false;
            }

            try
            {
                PopulateMetadataInNavisworks(RootNNode, _RootBNode);
            }
            catch (Exception e)
            {
                _LogMessageAction?.Invoke("Fatal error: Populate metadata in Navisworks operation has failed with " + e.Message);
                return false;
            }

            try
            {
                Autodesk.Navisworks.Api.Application.MainDocument.SaveFile(LastNWDFilePath);
            }
            catch (Exception e)
            {
                _LogMessageAction?.Invoke("Fatal error: The result could be saved as NWD! Error: " + e.Message);
                return false;
            }

            try { File.Delete(LastFBXFilePath); } catch (Exception) { }
            try { File.Delete(LastMetadataFilePath); } catch (Exception) { }
            try { File.Delete(LastNWCFilePath); } catch (Exception) { }

            _LogMessageAction?.Invoke("Process has successfully been completed!");

            return true;
        }

        private void PopulateMetadataInNavisworks(Autodesk.Navisworks.Api.ModelItem _NNode, BNode _BNode)
        {
            ComApi.InwOpState10 ComState = ComApiBridge.State;

            if (_BNode.Metadata != null)
            {
                //convert .NET selection to COM
                var ComModelItemPath = ComApiBridge.ToInwOaPath(_NNode);

                //get property categories
                var ComPropertyCategories = (ComApi.InwGUIPropertyNode2)ComState.GetGUIPropertyNode(ComModelItemPath, true);

                //create a new property category
                var NewPropertyCategory = (ComApi.InwOaPropertyVec)ComState.ObjectFactory(ComApi.nwEObjectType.eObjectType_nwOaPropertyVec, null, null);

                foreach (var Metadata in _BNode.Metadata)
                {
                    //create a new property and add it to the category
                    ComApi.InwOaProperty NMetadata = (ComApi.InwOaProperty)ComState.ObjectFactory(
                    ComApi.nwEObjectType.eObjectType_nwOaProperty, null, null);
                    NMetadata.name = Metadata.Key;
                    NMetadata.UserName = Metadata.Key;
                    NMetadata.value = Metadata.Value.Length == 0 ? " " : Metadata.Value;
                    NewPropertyCategory.Properties().Add(NMetadata);
                }

                //add the new category to the object
                ComPropertyCategories.SetUserDefined(0, "GLTF", "GLTF", NewPropertyCategory);
            }

            if (_BNode.Children != null)
            {
                using (var ChildIterator = _NNode.Children.GetEnumerator())
                {
                    int i = 0;
                    while (ChildIterator.MoveNext())
                    {
                        PopulateMetadataInNavisworks(ChildIterator.Current, _BNode.Children[i++]);
                    }
                }
            }
        }
    }
}