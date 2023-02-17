using Cdy.Ant;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InAntRun
{
    class Program
    {
        static bool mIsClosed = false;
        static AntRuntime.Runner mRunner;
        static void Main(string[] args)
        {
            LogoHelper.Print();
            Console.WriteLine(Res.Get("WelcomeMsg"));

            if (!Console.IsInputRedirected)
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args.Length > 0)
            {
                try
                {
                    mRunner = new AntRuntime.Runner() { Name = args[0] };
                    //mRunner.Name = args[0];
                    mRunner.Init();
                    mRunner.Start();
                    if (!Console.IsInputRedirected)
                        Console.Title = " InAntRun " + mRunner.Name;
                    Task.Run(() =>
                    {
                        StartMonitor(args[0]);
                    });
                }
                catch(Exception ex)
                {
                    Console.WriteLine(args[0]+" --> "+ ex.Message);
                }
            }

            Console.WriteLine(Res.Get("HelpMsg"));

            while (!mIsClosed)
            {
                Console.Write(">");

                if (!Console.IsInputRedirected)
                {
                    while (!Console.KeyAvailable)
                    {
                        if (mIsClosed)
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }
                if (mIsClosed)
                {
                    break;
                }

                string smd = Console.ReadLine();
                string[] cmd = smd.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (cmd.Length == 0) continue;
                string scmd = cmd[0].ToLower();
                switch (scmd)
                {
                    case "exit":
                        if (mRunner != null && mRunner.IsStarted)
                        {
                            mRunner.Stop();
                            if (!Console.IsInputRedirected)
                                Console.Title = " InAntRun";
                        }
                        mIsClosed = true;

                        break;
                    case "stop":
                        if (mRunner != null && mRunner.IsStarted)
                        {
                            mRunner.Stop();
                            if (!Console.IsInputRedirected)
                                Console.Title = " InAntRun";
                        }
                        break;
                    case "list":
                        ListDatabase();
                        break;
                    case "start":
                        if (cmd.Length > 1)
                        {
                            if (AntRuntime.Runner.CheckNameExit(cmd[1]))
                            {
                                if (mRunner == null)
                                    mRunner = new AntRuntime.Runner() { Name = cmd[1] };
                                if (!mRunner.IsStarted)
                                {
                                    mRunner.Init();
                                    mRunner.Start();
                                }
                                if (!Console.IsInputRedirected)
                                    Console.Title = " InAntRun " + mRunner.Name;
                                Task.Run(() => {
                                    StartMonitor(cmd[1]);
                                });
                            }
                            else
                            {
                                Console.WriteLine(cmd[1] + " is not exist!");
                            }
                        }
                        break;
                    case "h":
                        Console.WriteLine(GetHelpString());
                        break;
                    case "**":
                        LogoHelper.PrintAuthor();
                        break;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private static void ListDatabase()
        {
            string spath = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);
            spath = System.IO.Path.Combine(spath, "Data");
            StringBuilder sb = new StringBuilder();
            string stemp = "{0} {1}";
            foreach (var vv in System.IO.Directory.EnumerateDirectories(spath))
            {
                var vvn = new System.IO.DirectoryInfo(vv).Name;
                string sdb = System.IO.Path.Combine(vv, vvn + ".adb");
                if(System.IO.File.Exists(sdb))
                sb.AppendLine(string.Format(stemp, vvn, System.IO.File.GetLastWriteTime(sdb)));
            }
            Console.WriteLine(sb.ToString());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }

        private static string GetHelpString()
        {
            string str = "{0,-10} {1,-16} {2}";
            StringBuilder re = new StringBuilder();
            re.AppendLine();
            //re.AppendLine("start  project        // " + Res.Get("StartMsg"));
            //re.AppendLine("stop                    // " + Res.Get("StopMsg"));
            //re.AppendLine("restart                 // " + Res.Get("RestartMsg"));
            re.AppendLine(string.Format(str, "start", "project", "// " + Res.Get("StartMsg")));
            re.AppendLine(string.Format(str, "stop", "", "// " + Res.Get("StopMsg")));
            re.AppendLine(string.Format(str, "list", "", "// " + Res.Get("ListMsg")));
            re.AppendLine(string.Format(str, "exit", "", "// " + Res.Get("ExitMsg")));
            re.AppendLine(string.Format(str, "h", "", "// " + Res.Get("HMsg")));
            //re.AppendLine("list                    // " + Res.Get("ListMsg"));
            //re.AppendLine("exit                 // "+ Res.Get("ExitMsg"));
            //re.AppendLine("h                       // " + Res.Get("HMsg"));
            return re.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (mRunner != null && mRunner.IsStarted)
            {
                mRunner.Stop();
            }
            mIsClosed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (mRunner != null && mRunner.IsStarted)
            {
                mRunner.Stop();
            }
            mIsClosed = true;
            e.Cancel = true;
        }

        private static void StartMonitor(string name)
        {
            try
            {
                while (!mIsClosed)
                {
                    try
                    {
                        using (var server = new NamedPipeServerStream("Ant"+name, PipeDirection.InOut))
                        {
                            server.WaitForConnection();
                            while (!mIsClosed)
                            {
                                try
                                {
                                    if (!server.IsConnected) break;
                                    var cmd = server.ReadByte();
                                    if (cmd == 0)
                                    {
                                        if (mRunner.IsStarted)
                                        {
                                            mRunner.Stop();
                                        }
                                        mIsClosed = true;
                                        server.WriteByte(1);
                                        server.FlushAsync();
                                        //server.WaitForPipeDrain();
                                        Console.WriteLine(Res.Get("AnyKeyToExit") + ".....");
                                        break;
                                        //退出系统
                                    }
                                    else if (cmd == 1)
                                    {
                                        Console.WriteLine("Ready to restart .......");
                                        Task.Run(() =>
                                        {
                                            mRunner.ReStartDatabase();
                                        });
                                        server.WriteByte(1);
                                        server.FlushAsync();
                                        // server.WaitForPipeDrain();
                                    }
                                    else
                                    {

                                    }
                                }
                                catch (Exception eex)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }

            }
            catch (Exception ex)
            {
                LoggerService.Service.Info("Programe", ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }
    }
}
