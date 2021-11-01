using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace TaskKeepAlive
{
    class Program
    {

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow([In] IntPtr hWnd, [In] int nCmdShow);

        static void Main(string[] args)
        {
            WriteLine("TaskKeepAlive by pftq ~ www.pftq.com ~ Oct. 2015");
            WriteLine("Pass arguments to TaskKeepAlive via shortcut/command prompt to pass them to tasks it restarts.");
            WriteLine("Running from: " + Process.GetCurrentProcess().MainModule.FileName);
            Thread.Sleep(1000);

            // from https://stackoverflow.com/questions/44675085/minimize-console-at-app-startup-c-sharp
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(handle, 6);

            Dictionary<string, DateTime> unresponsive = new Dictionary<string, DateTime>();
            while (true)
            {
                Stack<string> nextDir = new Stack<string>();
                nextDir.Push(Path.GetFullPath(Directory.GetCurrentDirectory()));
                while(nextDir.Any())
                {
                    
                    string dir = nextDir.Pop();
                    foreach (string d in Directory.GetDirectories(dir))
                    {
                        nextDir.Push(d);
                    }

                    WriteLine("Checking folder " + dir, false);
                    foreach(string f in Directory.GetFiles(dir, "*.exe"))
                    {
                        if (f == Process.GetCurrentProcess().MainModule.FileName) continue;
                        string file = Path.GetFileName(f);
                        string process = Path.GetFileNameWithoutExtension(f);
                        WriteLine(" - Checking file " + file, false);
                        try
                        {
                            bool processExists = Process.GetProcesses().Where(p => p.ProcessName == process && p.MainModule.FileName == f).Any();
                            if (!Process.GetProcesses().Where(p => p.ProcessName==process && p.MainModule.FileName ==f && p.Responding).Any())
                            {
                                if (processExists)
                                {
                                    if (!unresponsive.ContainsKey(f))
                                    {
                                        unresponsive[f] = DateTime.Now;
                                    }
                                    if ((DateTime.Now - unresponsive[f]).TotalMinutes < 1)
                                    {
                                        WriteLine(" - Unresponsive but waiting first: " + f + " (Froze at " + unresponsive[f] + ")");
                                        continue;
                                    }
                                }
                                WriteLine("Restarting: " + f + (args.Length!=0? " "+string.Join(" ", args):""));
                                Process.GetProcesses().Where(p => p.ProcessName == process && p.MainModule.FileName == f).ToList().ForEach(p => p.Kill());
                                ProcessStartInfo proc = new ProcessStartInfo();

                                proc.WorkingDirectory = dir;
                                proc.FileName = f;
                                proc.Arguments = (args.Length != 0 ? " " + string.Join(" ", args) : "");
                                Process.Start(proc);
                                Thread.Sleep(2000);
                            }
                            if (unresponsive.ContainsKey(f))
                            {
                                WriteLine(" - No longer unresponsive: " + f + " (Froze at " + unresponsive[f] + ")");
                                unresponsive.Remove(f);
                            }
                        }
                        catch (Exception e)
                        {
                            WriteLine("Error for "+f+":\n" + e);
                        }
                    }
                }
                Thread.Sleep(10000);
            }
        }
        static void WriteLine(string s, bool log=true)
        {
            s = DateTime.Now + ": " + s;
            Console.WriteLine(s);
            if(log) File.AppendAllLines("TaskKeepAlive_log.txt", new string[]{s});
        }
    }
}
