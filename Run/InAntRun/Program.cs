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

            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            if (args.Length > 0)
            {
                mRunner = new AntRuntime.Runner() { Name = args[1] };
                mRunner.Name = args[0];
                mRunner.Init();
                mRunner.Start();

                Task.Run(() => {
                    StartMonitor(args[1]);
                });
            }

            Console.WriteLine(Res.Get("HelpMsg"));

            while (!mIsClosed)
            {
                Console.Write(">");
                while (!Console.KeyAvailable)
                {
                    if (mIsClosed)
                    {
                        break;
                    }
                    Thread.Sleep(100);
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
                        }
                        mIsClosed = true;

                        break;
                    case "stop":
                        if (mRunner != null && mRunner.IsStarted)
                        {
                            mRunner.Stop();
                        }
                        break;
                    case "start":
                        if (cmd.Length > 0)
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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }

        private static string GetHelpString()
        {
            StringBuilder re = new StringBuilder();
            re.AppendLine();
            re.AppendLine("exit                 // stop ant and exit application");
            re.AppendLine("start  project       // start ant project");
            re.AppendLine("stop                 // stop ant");
            re.AppendLine("h                    // print help message");
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
                                        Console.WriteLine("Start to restart database.......");
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
