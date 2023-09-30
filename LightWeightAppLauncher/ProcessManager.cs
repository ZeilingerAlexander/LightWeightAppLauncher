using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace LightWeightAppLauncher
{
    /// <summary>
    /// class for starting processes
    /// </summary>
    internal class ProcessManager
    {
        public static List<Thread> ActiveThreads = new List<Thread>();
        public static async void DispenseAllThreads()
        {
            MainWindow.abort = true;
        }
        /// <summary>
        /// Starts a given process
        /// </summary>
        public static void StartProccess(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source + "\n" + ex.Data);
            }
        }
    }
}
