/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.Windows.Forms;
using Autodesk.Navisworks.Api.Plugins;
using BUtility;

namespace GLTFImporterPlugin
{
    [PluginAttribute("GLTFImporter", "BurakKara", DisplayName = "Import GLTF")]
    public class MainClass : AddInPlugin
    {
        public MainClass()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            ProcessHelper.KillCreatedProcesses();
        }
        
        public override int Execute(params string[] parameters)
        {
            if (OperationManager.Get().IsThereAnActiveProcess())
            {
                var DialogResult = MessageBox.Show("There is an active process ongoing. Do you wish to cancel it?", "Cancel active conversion", MessageBoxButtons.YesNo);
                if (DialogResult == DialogResult.Yes)
                {
                    ProcessHelper.KillCreatedProcesses();
                    CloseLogWindow();
                }
                else
                {
                    ShowLogWindow();
                }
                return 1;
            }

            if (!ShowFileDialog(out string GLTFFilePath))
            {
                MessageBox.Show("Please select a GLTF file.");
                return 1;
            }

            ShowLogWindow();

            if (!OperationManager.Get().ConvertGLTFToFBXandBinary(
            (bool _bSuccess) =>
            {
                if (!_bSuccess)
                {
                    MessageBox.Show("Conversion process has failed.");
                }
                else
                {
                    try
                    {
                        if (!OperationManager.Get().ImportFBXAndPopulateMetadataFromBinary_BackgroundThread(out BNode RootBNode, LogAction))
                        {
                            MessageBox.Show("Metadata parse stage has failed.");
                        }
                        else
                        {
                            LogAction?.Invoke("Clearing the Navisworks scene...");
                            LogAction?.Invoke("Importing intermediate file to Navisworks...");
                            LogAction?.Invoke("Iterating through the hierarchy and populating the metadata...");

                            LogWindow.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (!OperationManager.Get().ImportFBXAndPopulateMetadataFromBinary_MainThread(RootBNode, LogAction))
                                {
                                    MessageBox.Show("Import to Navisworks stage has failed.");
                                }
                                else
                                {
                                    MessageBox.Show("File has successfully been imported.");
                                    CloseLogWindow();
                                }
                            }));
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to continue with the Navisworks import stage.");
                    }
                }

            }, GLTFFilePath, LogAction))
            {
                MessageBox.Show("Convert to intermediate file formats stage has failed.");
                return 1;
            }

            return 0;
        }

        private bool ShowFileDialog(out string _GLTFFilePath)
        {
            var Dialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse GLTF Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "gltf",
                Filter = "gltf files (*.gltf)|*.gltf",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (Dialog.ShowDialog() != DialogResult.OK)
            {
                _GLTFFilePath = null;
                return false;
            }

            _GLTFFilePath = Dialog.FileName;

            return true;
        }

        private void ShowLogWindow()
        {
            if (LogWindow == null)
            {
                LogWindow = new LogViewer();
            }
            LogAction = (string _Message) =>
            {
                LogWindow.AddEntry(new LogEntry(_Message));
            };
            LogWindow.Show();
        }
        private void CloseLogWindow()
        {
            if (LogWindow != null)
            {
                LogWindow.Hide();
                LogWindow.ClearEntries();
            }
        }

        private LogViewer LogWindow;
        private Action<string> LogAction;
    }
}