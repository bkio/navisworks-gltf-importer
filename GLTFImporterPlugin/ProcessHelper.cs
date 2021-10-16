/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GLTFImporterPlugin
{
    public class ProcessHelper
    {
        private string Path;
        private string Args;
        private string WorkingDirectory;
        private Dictionary<string, string> EnvVars;
        private Action<string> LogMessageAction = null;

        private Process CreatedProcess;
        private Thread CreatedThread;

        private Action<bool> OnComplete;

        private bool bWasSuccessful = false;

        private ProcessHelper() { }

        public static bool RunProcess(Action<bool> _OnComplete, string _Path, string _Args, string _WorkingDirectory, Dictionary<string, string> _EnvVars = null, Action<string> _LogMessageAction = null)
        {
            var NewHelper = new ProcessHelper()
            {
                OnComplete = _OnComplete,
                Path = _Path,
                Args = _Args,
                WorkingDirectory = _WorkingDirectory,
                EnvVars = _EnvVars,
                LogMessageAction = _LogMessageAction
            };
            CreatedProcesses.Add(new WeakReference<ProcessHelper>(NewHelper));

            if (!NewHelper.Initialize())
            {
                NewHelper.KillProcess();
                return false;
            }
            return true;
        }

        private bool Initialize()
        {
            LogMessageAction?.Invoke($"Running [{Path}] - Args [{Args}] - Working Directory [{WorkingDirectory}]");
            CreatedProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = Args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = Path,
                    WorkingDirectory = WorkingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
            };

            try
            {
                if (EnvVars != null)
                {
                    foreach (var Var in EnvVars)
                    {
                        if (!CreatedProcess.StartInfo.EnvironmentVariables.ContainsKey(Var.Key))
                        {
                            CreatedProcess.StartInfo.EnvironmentVariables.Add(Var.Key, Var.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogMessageAction?.Invoke($"Environment variable setup of the process initialization has failed with " + e.Message);
                return false;
            }

            CreatedProcess.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
            CreatedProcess.ErrorDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);

            try
            {
                CreatedProcess.Start();
            }
            catch (Exception e)
            {
                LogMessageAction?.Invoke($"Start process has failed with " + e.Message);
                return false;
            }

            try
            {
                CreatedProcess.BeginOutputReadLine();
                CreatedProcess.BeginErrorReadLine();
            }
            catch (Exception e)
            {
                LogMessageAction?.Invoke($"Process output redirection setup has failed with " + e.Message);
                return false;
            }

            CreatedThread = new Thread(() => MainRunnable()) { IsBackground = true };
            try
            {
                CreatedThread.Start();
            }
            catch (Exception e)
            {
                LogMessageAction?.Invoke($"Process listener thread setup has failed with " + e.Message);
                return false;
            }

            return true;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            LogMessageAction?.Invoke(e.Data);
        }

        private void MainRunnable()
        {
            try
            {
                CreatedProcess.WaitForExit();
                bWasSuccessful = CreatedProcess.ExitCode == 0;
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                {
                    return;
                }

                LogMessageAction?.Invoke($"Process has failed during wait for exit with " + e.Message);
            }

            OnComplete?.Invoke(bWasSuccessful);
            KillProcess();
        }

        private void KillProcess()
        {
            try { CreatedProcess?.Kill(); } catch (Exception) { }
            try { CreatedProcess?.Dispose(); } catch (Exception) { }
            CreatedProcess = null;

            try { CreatedThread?.Abort(); } catch (Exception) { }
        }

        public static void KillCreatedProcesses()
        {
            foreach (var Proc in CreatedProcesses)
            {
                try
                {
                    if (Proc.TryGetTarget(out ProcessHelper PHelper))
                    {
                        PHelper.KillProcess();
                    }
                }
                catch (Exception) { }
            }
            CreatedProcesses.Clear();
        }
        private static readonly List<WeakReference<ProcessHelper>> CreatedProcesses = new List<WeakReference<ProcessHelper>>();
    }
}