using Cdy.Ant;
using InAntDevelopServer;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace InAntStuidoServer
{
    public class Program: IDatabaseManager
    {/// <summary>
     /// 
     /// </summary>
        static bool mIsExited = false;

        static object mLockObj = new object();

        /// <summary>
        /// 
        /// </summary>
        public class Config
        {

            #region ... Variables  ...

            /// <summary>
            /// 
            /// </summary>
            public static Config Instance = new Config();

            #endregion ...Variables...

            #region ... Events     ...

            #endregion ...Events...

            #region ... Constructor...


            #endregion ...Constructor...

            #region ... Properties ...

            /// <summary>
            /// 
            /// </summary>
            public bool IsWebApiEnable { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int WebApiPort { get; set; } = 9000;

            /// <summary>
            /// 
            /// </summary>
            public bool IsGrpcEnable { get; set; } = true;

            /// <summary>
            /// 
            /// </summary>
            public int GrpcPort { get; set; } = 15001;

            #endregion ...Properties...

            #region ... Methods    ...

            /// <summary>
            /// 
            /// </summary>
            public void Load()
            {
                try
                {
                    string spath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "InAntStuidoServer.cfg");

                    LoggerService.Service.Info("Config", "Start to load config");

                    if (System.IO.File.Exists(spath))
                    {
                        var xxx = XElement.Load(spath);
                        this.IsWebApiEnable = bool.Parse(xxx.Attribute("IsWebApiEnable")?.Value);
                        this.IsGrpcEnable = bool.Parse(xxx.Attribute("IsGrpcEnable")?.Value);
                        this.WebApiPort = int.Parse(xxx.Attribute("WebApiPort")?.Value);
                        this.GrpcPort = int.Parse(xxx.Attribute("GrpcPort")?.Value);
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Service.Erro("Config", ex.Message);
                }
            }

            #endregion ...Methods...

            #region ... Interfaces ...

            #endregion ...Interfaces...
        }


        static void Main(string[] args)
        {
            Console.Title = "AntDevelopServer";

            LogoHelper.Print();

            //注册日志
            ServiceLocator.Locator.Registor<ILog>(new ConsoleLogger());

            Program pg = new Program();
            Config.Instance.Load();
            ServiceLocator.Locator.Registor(typeof(IDatabaseManager), pg);

            if (!DBDevelopService.DbManager.Instance.IsLoaded)
                DBDevelopService.DbManager.Instance.PartLoad();

            int port = Config.Instance.GrpcPort;
            int webPort = Config.Instance.WebApiPort;

            bool isNeedMinMode = false;
            if (args.Length > 0)
            {
                if (args[0] == "/m")
                {
                    isNeedMinMode = true;
                }
                else
                {
                    port = int.Parse(args[0]);
                }
            }

            if (args.Length > 1)
            {
                webPort = int.Parse(args[1]);
            }

          

            WindowConsolHelper.DisbleQuickEditMode();

            if (isNeedMinMode)
            {
                WindowConsolHelper.MinWindow("AntDevelopServer");
            }

            if (!Console.IsInputRedirected)
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }


            Service.Instanse.Start(port, webPort, Config.Instance.IsGrpcEnable, Config.Instance.IsWebApiEnable);

            Thread.Sleep(100);

            if (!Console.IsInputRedirected)
                OutByLine("", Res.Get("HelpMsg"));

            while (!mIsExited)
            {
                OutInLine("", "");
                var vv = Console.In.ReadLine();

                if (vv != null)
                {
                    string[] cmd = vv.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (cmd.Length == 0) continue;

                    string cmsg = cmd[0].ToLower();

                    if (cmsg == "exit")
                    {
                        if (!Console.IsInputRedirected)
                        {
                            OutByLine("", Res.Get("AppExitHlp"));
                            cmd = Console.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (cmd.Length == 0) continue;
                            if (cmd[0].ToLower() == "y")
                                break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (cmsg == "db")
                    {
                        if (cmd.Length > 1)
                            ProcessDatabaseCreate(cmd[1]);
                    }
                    else if (cmsg == "list")
                    {
                        ListDatabase();
                    }
                    else if (cmsg == "h")
                    {
                        OutByLine("", GetHelpString());
                    }
                    else if (cmsg == "**")
                    {
                        LogoHelper.PrintAuthor();
                    }
                }
            }
            InAntDevelopServer.Service.Instanse.Stop();

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LoggerService.Service.Erro("GrpcDBService", e.ExceptionObject.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            mIsExited = true;
            e.Cancel = true;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Res.Get("AnyKeyToExit"));
            Console.ResetColor();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prechar"></param>
        /// <param name="msg"></param>
        private static void OutByLine(string prechar, string msg)
        {
            Console.WriteLine(prechar + ">" + msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prechar"></param>
        /// <param name="msg"></param>
        private static void OutInLine(string prechar, string msg)
        {
            Console.Write(prechar + ">" + msg);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ListDatabase()
        {
            //if (!DBDevelopService.DbManager.Instance.IsLoaded)
            //    DBDevelopService.DbManager.Instance.PartLoad();

            StringBuilder sb = new StringBuilder();
            foreach (var vdd in DBDevelopService.DbManager.Instance.ListDatabase())
            {
                sb.Append(vdd + ",");
            }
            sb.Length = sb.Length > 1 ? sb.Length - 1 : sb.Length;
            OutByLine("", sb.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="msg"></param>
        private static void ProcessDatabaseCommand(AlarmDatabase db, string msg)
        {
            try
            {
                string[] cmd = msg.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string cmsg = cmd[0].ToLower();
                if (cmsg == "save")
                {
                    new AlarmDatabaseSerise() { Database = db }.Save();
                    DBDevelopService.DbManager.Instance.ReLoad(db.Name);
                }
                else if (cmsg == "import")
                {
                    Import(db, cmd[1].ToLower());
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prechar"></param>
        /// <param name="description"></param>
        /// <param name="waitstring"></param>
        /// <returns></returns>
        private static bool WaitForInput(string prechar, string description, string waitstring)
        {
            OutByLine(prechar, description);
            var cmd = Console.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (cmd.Length > 0 && string.Compare(waitstring, cmd[0], true) == 0) return true;
            else return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private static void ProcessDatabaseCreate(string name)
        {
            AlarmDatabase db = DBDevelopService.DbManager.Instance.GetDatabase(name);

            if (db == null)
            {
                if (WaitForInput("", string.Format(Res.Get("NewDatabase"), name), "y"))
                {
                    db = DBDevelopService.DbManager.Instance.NewDB(name, name);
                }
                else
                {
                    return;
                }
            }
            else
            {
                DBDevelopService.DbManager.Instance.CheckAndContinueLoadDatabase(db);
            }

            OutByLine(name, Res.Get("HelpMsg"));
            while (true)
            {
                OutInLine(name, "");
                string[] cmd = Console.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (cmd.Length == 0) continue;
                string cmsg = cmd[0].ToLower();
                try
                {
                    if (cmsg == "save")
                    {
                        new AlarmDatabaseSerise() { Database = db }.Save();
                    }
                    else if (cmsg == "clear")
                    {
                        ClearTag(db);
                    }
                    else if (cmsg == "start")
                    {
                        if (!CheckStart(db.Name))
                        {
                            StartDb(db.Name);
                        }
                        else
                        {
                            Console.WriteLine(string.Format(Res.Get("databaseinrunningHlp"), db.Name));
                        }
                    }
                    //else if (cmsg == "rerun")
                    else if (cmsg == "restart")
                    {
                        if (!CheckStart(db.Name))
                        {
                            StartDb(db.Name);
                        }
                        else
                        {
                            ReLoadDatabase(db.Name);
                        }
                    }
                    else if (cmsg == "isstarted")
                    {
                        if (CheckStart(db.Name))
                        {
                            Console.WriteLine("database " + db.Name + " is start.");
                        }
                        else
                        {
                            Console.WriteLine("database " + db.Name + " is stop.");
                        }
                    }
                    else if (cmsg == "stop")
                    {
                        StopDatabase(db.Name+"_alm");
                    }
                    else if (cmsg == "import")
                    {
                        if (cmd.Length > 1)
                            Import(db, cmd[1].ToLower());
                        else
                        {
                            Import(db, name + ".csv");
                        }
                    }
                    else if (cmsg == "export")
                    {
                        if (cmd.Length > 1)
                            ExportToCSV(db, cmd[1].ToLower());
                        else
                        {
                            ExportToCSV(db, name + ".csv");
                        }
                    }
                    else if (cmsg == "list")
                    {
                        string ctype = cmd.Length > 1 ? cmd[1] : "";
                        ListDatabase(db, ctype);
                    }
                    else if (cmsg == "h")
                    {
                        if (cmd.Length == 1)
                        {
                            Console.WriteLine(GetDbManagerHelpString());
                        }
                    }
                    else if (cmsg == "exit")
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    OutByLine(name, Res.Get("ErroParameter") + " " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private static bool StartDb(string name)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var info = new ProcessStartInfo() { FileName = "InAntRun.exe" };
                    info.UseShellExecute = true;
                    info.Arguments = name;
                    info.WorkingDirectory = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    Process.Start(info).WaitForExit(1000);
                }
                else
                {
                    var info = new ProcessStartInfo() { FileName = "dotnet" };
                    info.UseShellExecute = true;
                    info.CreateNoWindow = false;
                    info.Arguments = "./InAntRun.dll " + name;
                    info.WorkingDirectory = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    Process.Start(info).WaitForExit(1000);
                }


                Console.WriteLine(string.Format(Res.Get("StartdatabaseSucessful"), name));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="headname"></param>
        ///// <param name="group"></param>
        ///// <param name="startId"></param>
        ///// <param name="realDatabase"></param>
        ///// <returns></returns>
        //private static string GetAvaiableName(string headname, string group, ref int startId, RealDatabase realDatabase)
        //{
        //    for (int i = startId; i < int.MaxValue; i++)
        //    {
        //        string sname = group + "." + headname + i;
        //        if (!realDatabase.NamedTags.ContainsKey(sname))
        //        {
        //            startId = i + 1;
        //            return headname + i;
        //        }
        //    }
        //    return string.Empty;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="db"></param>
        ///// <param name="paras"></param>
        //private static void Sp(Database db, int rtp, int ctp, int addressType, params string[] paras)
        //{
        //    Cdy.Tag.RealDatabase test = db.RealDatabase;

        //    Cdy.Tag.HisDatabase htest = db.HisDatabase;

        //    Cdy.Tag.RecordType rrtp = (RecordType)(rtp);

        //    int idstart = 0;

        //    string address = "";
        //    if (paras.Length > 0)
        //    {
        //        int dcount = int.Parse(paras[0]);
        //        for (int i = 0; i < dcount; i++)
        //        {
        //            if (addressType == 0)
        //            {
        //                address = "Spider:";
        //            }
        //            else
        //            {
        //                if (i % 3 == 0)
        //                {
        //                    address = "Sim:sin";
        //                }
        //                else if (i % 3 == 1)
        //                {
        //                    address = "Sim:cos";
        //                }
        //                else
        //                {
        //                    address = "Sim:step";
        //                }
        //            }
        //            var vtag = new Cdy.Tag.DoubleTag() { Name = GetAvaiableName("Double", "Double", ref idstart, test), Group = "Double", LinkAddress = address };
        //            test.Append(vtag);
        //            htest.AddHisTags(new Cdy.Tag.HisTag() { Id = vtag.Id, TagType = Cdy.Tag.TagType.Double, Circle = 1000, Type = rrtp, CompressType = ctp });
        //        }
        //    }

        //    if (paras.Length > 1)
        //    {
        //        int fcount = int.Parse(paras[1]);
        //        for (int i = 0; i < fcount; i++)
        //        {
        //            if (addressType == 0)
        //            {
        //                address = "Spider:";
        //            }
        //            else
        //            {
        //                if (i % 3 == 0)
        //                {
        //                    address = "Sim:sin";
        //                }
        //                else if (i % 3 == 1)
        //                {
        //                    address = "Sim:cos";
        //                }
        //                else
        //                {
        //                    address = "Sim:step";
        //                }
        //            }
        //            var vtag = new Cdy.Tag.FloatTag() { Name = GetAvaiableName("Float", "Float", ref idstart, test), Group = "Float", LinkAddress = address };
        //            test.Append(vtag);
        //            htest.AddHisTags(new Cdy.Tag.HisTag() { Id = vtag.Id, TagType = Cdy.Tag.TagType.Float, Circle = 1000, Type = rrtp, CompressType = ctp });
        //        }
        //    }

        //    if (paras.Length > 2)
        //    {
        //        int fcount = int.Parse(paras[2]);
        //        if (addressType == 0)
        //        {
        //            address = "Spider:";
        //        }
        //        else
        //        {
        //            address = "Sim:step";
        //        }
        //        for (int i = 0; i < fcount; i++)
        //        {
        //            var vtag = new Cdy.Tag.LongTag() { Name = GetAvaiableName("Long", "Long", ref idstart, test), Group = "Long", LinkAddress = address };
        //            test.Append(vtag);
        //            htest.AddHisTags(new Cdy.Tag.HisTag() { Id = vtag.Id, TagType = Cdy.Tag.TagType.Long, Circle = 1000, Type = rrtp, CompressType = ctp });
        //        }
        //    }

        //    if (paras.Length > 3)
        //    {
        //        if (addressType == 0)
        //        {
        //            address = "Spider:";
        //        }
        //        else
        //        {
        //            address = "Sim:step";
        //        }
        //        int fcount = int.Parse(paras[3]);
        //        for (int i = 0; i < fcount; i++)
        //        {
        //            var vtag = new Cdy.Tag.IntTag() { Name = GetAvaiableName("Int", "Int", ref idstart, test), Group = "Int", LinkAddress = address };
        //            test.Append(vtag);
        //            htest.AddHisTags(new Cdy.Tag.HisTag() { Id = vtag.Id, TagType = Cdy.Tag.TagType.Int, Circle = 1000, Type = rrtp, CompressType = ctp });
        //        }
        //    }

        //    if (paras.Length > 4)
        //    {
        //        if (addressType == 0)
        //        {
        //            address = "Spider:";
        //        }
        //        else
        //        {
        //            address = "Sim:square";
        //        }
        //        int fcount = int.Parse(paras[4]);
        //        for (int i = 0; i < fcount; i++)
        //        {
        //            var vtag = new Cdy.Tag.BoolTag() { Name = GetAvaiableName("Bool", "Bool", ref idstart, test), Group = "Bool", LinkAddress = address };
        //            test.Append(vtag);
        //            htest.AddHisTags(new Cdy.Tag.HisTag() { Id = vtag.Id, TagType = Cdy.Tag.TagType.Bool, Circle = 1000, Type = rrtp, CompressType = ctp });
        //        }
        //    }


        //    if (paras.Length > 5)
        //    {
        //        if (addressType == 0)
        //        {
        //            address = "Spider:";
        //        }
        //        else
        //        {
        //            address = "Sim:steppoint";
        //        }
        //        int fcount = int.Parse(paras[5]);
        //        for (int i = 0; i < fcount; i++)
        //        {
        //            var vtag = new Cdy.Tag.IntPointTag() { Name = GetAvaiableName("IntPoint", "IntPoint", ref idstart, test), Group = "IntPoint", LinkAddress = address };
        //            test.Append(vtag);
        //            htest.AddHisTags(new Cdy.Tag.HisTag() { Id = vtag.Id, TagType = Cdy.Tag.TagType.IntPoint, Circle = 1000, Type = rrtp, CompressType = ctp });
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetDbManagerHelpString()
        {
            string str = "{0,-10} {1,-50} {2}";
            StringBuilder re = new StringBuilder();
            re.AppendLine();
            re.AppendLine(string.Format(str, "save", "", "// " + Res.Get("SaveDatabaseHlp")));
            re.AppendLine(string.Format(str, "start", "", "// " + Res.Get("StartDatabaseHlp")));
            re.AppendLine(string.Format(str, "restart", "", "// " + Res.Get("ReStartDatabaseHlp")));
            re.AppendLine(string.Format(str, "stop", "", "// " + Res.Get("StopDatabaseHlp")));
            re.AppendLine(string.Format(str, "clear", "", "// " + Res.Get("ClearTagHlp")));
            //re.AppendLine(string.Format(str, "updatehis","[tagname] [propertyname] [propertyvalue]","// update value of a poperty in a tag's his config"));
            re.AppendLine(string.Format(str, "import", "[filename]", "// " + Res.Get("ImportHlp")));
            re.AppendLine(string.Format(str, "export", "[filename]", "// " + Res.Get("ExportHlp")));
            re.AppendLine(string.Format(str, "list", "[tagtype]", "// " + Res.Get("ListTagHlp")));
            re.AppendLine(string.Format(str, "exit", "", "// " + Res.Get("ExitDatabaseHlp")));
            re.AppendLine(string.Format(str, "sp", "[recordType] [compressType] [enable sim address][double tag number] [float tag number] [long tag number] [int tag number] [bool tag number] [intpoint tag number]", " //" + Res.Get("QuickGeneraterTagHlp")));


            return re.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="type"></param>
        private static void ListDatabase(AlarmDatabase database, string type = "")
        {
            if (!string.IsNullOrEmpty(type))
            {
                int count = database.Tags.Values.Where(e => e.Type == (TagType)Enum.Parse(typeof(TagType), type)).Count();
                OutByLine(database.Name, string.Format(Res.Get("TagMsg"), count, type));
            }
            else
            {
                foreach (TagType vv in Enum.GetValues(typeof(TagType)))
                {
                    int count = database.Tags.Values.Where(e => e.Type == vv).Count();
                    OutByLine(database.Name, string.Format(Res.Get("TagMsg"), count, vv.ToString()));
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="file"></param>
        private static void ExportToCSV(AlarmDatabase database, string file)
        {
            string sfile = file;

            if (!System.IO.Path.IsPathRooted(sfile))
            {
                sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), sfile);
            }

            var stream = new StreamWriter(File.Open(sfile, FileMode.OpenOrCreate, FileAccess.ReadWrite));
            foreach (var vv in database.Tags)
            {
                stream.WriteLine(SaveToCSVString(vv.Value));
            }
            stream.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mRealTagMode"></param>
        /// <param name="mHisTagMode"></param>
        /// <returns></returns>
        public static string SaveToCSVString(Tagbase mRealTagMode)
        {
            return mRealTagMode.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public static Tagbase LoadFromCSVString(string val)
        {
            string[] stmp = val.Split(new char[] { ',' });
            var re = TagManager.Manager.CreatTag((Cdy.Ant.TagType)Enum.Parse(typeof(Cdy.Ant.TagType), stmp[0]));
            re.LoadFromString(val);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="file"></param>
        private static void Import(AlarmDatabase database, string file)
        {
            string sfile = file;

            if (!System.IO.Path.IsPathRooted(sfile))
            {
                sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), sfile);
            }
            if (System.IO.File.Exists(sfile))
            {
                var reader = new System.IO.StreamReader(System.IO.File.Open(sfile, System.IO.FileMode.Open));
                while (reader.Peek() > 0)
                {
                    var cmd = reader.ReadLine();
                    if (sfile.EndsWith(".cmd"))
                    {
                        ProcessDatabaseCommand(database, cmd);
                    }
                    else if (sfile.EndsWith(".csv"))
                    {
                        var vres = LoadFromCSVString(cmd);
                        database.AddOrUpdate(vres);
                    }

                }
            }


        }





        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        private static void ClearTag(AlarmDatabase database)
        {
            database.Tags.Clear();
            database.MaxId = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tag"></param>
        private static void RemoveTag(AlarmDatabase database, string tag)
        {
            var vv = database.GetTagByName(tag);
            if (vv != null)
            {
                database.Remove(vv.Id);
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetHelpString()
        {
            string str = "{0,-10} {1,-16} {2}";
            StringBuilder re = new StringBuilder();
            re.AppendLine();
            re.AppendLine(string.Format(str, "db", "[databasename]", @"// " + Res.Get("GDMsgHlp")));
            re.AppendLine(string.Format(str, "list", "", "// " + Res.Get("ListDatabaseHlp")));
            re.AppendLine(string.Format(str, "exit", "", "// " + Res.Get("Exit")));
            re.AppendLine(string.Format(str, "h", "", "// " + Res.Get("HMsg")));
            return re.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public static bool StopDatabase(string name)
        {
            using (var client = new NamedPipeClientStream(".", "Ant" + name, PipeDirection.InOut))
            {
                try
                {
                    client.Connect(2000);
                    client.WriteByte(0);
                    client.FlushAsync();

                    if (OperatingSystem.IsWindows())
                    {
                        client.WaitForPipeDrain();
                    }

                    if (client.IsConnected)
                    {
                        var res = client.ReadByte();
                        int count = 0;
                        while (res == -1)
                        {
                            res = client.ReadByte();
                            count++;
                            if (count > 20) break;
                            Thread.Sleep(100);
                        }
                        if (res == 1)
                        {
                            Console.WriteLine(string.Format(Res.Get("StopdatabaseSucessful"), name));
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format(Res.Get("Stopdatabasefail"), name) + ex.Message);
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public static bool ReLoadDatabase(string name)
        {
            lock (mLockObj)
            {
                using (var client = new NamedPipeClientStream(".", "Ant" + name, PipeDirection.InOut))
                {
                    try
                    {
                        client.Connect(2000);
                        client.WriteByte(1);
                        client.FlushAsync();

                        var res = 0;
                        if (OperatingSystem.IsWindows())
                        {
                            client.WaitForPipeDrain();
                            res = client.ReadByte();
                        }
                        else
                        {
                            int count = 0;
                            res = client.ReadByte();
                            while (res == -1)
                            {
                                res = client.ReadByte();
                                count++;
                                if (count > 20) break;
                                Thread.Sleep(100);
                            }
                        }
                        if (res == 1)
                        {
                            Console.WriteLine(string.Format(Res.Get("RerundatabaseSucessful"), name));
                        }
                        return true;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format(Res.Get("Rerundatabasefail"), name));
                        //Console.WriteLine("ReRun database " + name + "  failed. " + ex.Message + "  " + ex.StackTrace);
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckStart(string name)
        {
            lock (mLockObj)
            {
                using (var client = new NamedPipeClientStream(".", "Ant"+ name, PipeDirection.InOut))
                {
                    try
                    {
                        client.Connect(1000);
                        client.Close();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine("CheckStart " + name + "  failed." + ex.Message + "  " + ex.StackTrace);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public bool Start(string name)
        {
            if (!CheckStart(name))
            {
                return StartDb(name);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Stop(string name)
        {
            //if (CheckStart(name))
            {
                return StopDatabase(name);
            }
            //return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Rerun(string name)
        {
            if (!CheckStart(name))
            {
                return StartDb(name);
            }
            else
            {
                return ReLoadDatabase(name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRunning(string name)
        {
            return CheckStart(name);
        }
    }
}
