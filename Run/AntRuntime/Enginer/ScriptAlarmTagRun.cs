using Cdy.Ant;
using Cdy.Ant.Tag;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public class ScriptAlarmTagRun : TagRunBase
    {

        #region ... Variables  ...
        static ScriptOptions sop;
        ScriptTag mDTag;
        private Script<object> mScript;
        //private bool mIsNeedCallAlways = false;

        private long mCurrentMessageId = -1;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        static ScriptAlarmTagRun()
        {
            InitGlobel();
        }



        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
       public IDataTagService TagService { get { return ServiceLocator.Locator.Resolve<IDataTagApi>().TagService; } }

        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mDTag = value as Cdy.Ant.ScriptTag; } }

        /// <summary>
        /// 
        /// </summary>
        public TagScriptImp Tag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MessageScriptImp Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LoggerImp Logger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag => TagType.Script;

        private object mLockObj = new object();

        private Timer mTimer;

        private Task<ScriptState<object>> mResult;

        DateTime mLastExecuteTime;

        DateTime mNextExecuteTime;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> GetSupportModifyProperty()
        {
            var re = base.GetSupportModifyProperty();
            re.Add("Expresse", "");
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void OnPropertyChangedForRuntime(string name, string value)
        {
            if (name == "expresse")
            {
                mDTag.Expresse = value;
                Init();
                ExecuteExpress();
            }
            base.OnPropertyChangedForRuntime(name, value);
        }

        public static void LoadRefernceDll(string sfile)
        {
            //using (var dependencyFileStream = System.IO.File.OpenRead(sfile))
            //{
            //    using (DependencyContextJsonReader dependencyContextJsonReader = new DependencyContextJsonReader())
            //    {
            //        //得到对应的实体文件
            //        var dependencyContext = dependencyContextJsonReader.Read(dependencyFileStream);
            //        //定义的运行环境,没有,则为全平台运行.
            //        string currentRuntimeIdentifier = dependencyContext.Target.Runtime;
            //        //运行时所需要的dll文件
            //        var assemblyNames = dependencyContext.RuntimeLibraries;
            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        private static void InitGlobel()
        {
            sop = ScriptOptions.Default;
            try
            {
                if (ScriptExtend.extend.ExtendDlls.Count > 0)
                {
                    sop = sop.AddReferences(ScriptExtend.extend.ExtendDlls.Select(e => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(e)));

                    foreach(var vv in ScriptExtend.extend.ExtendDlls)
                    {
                        try
                        {
                            var ass = Assembly.LoadFrom(vv);
                            var reass = ass.GetReferencedAssemblies();

                            //foreach(var vva in reass)
                            //{
                            //    Assembly.Load(vva);
                            //}

                            //string sfile = System.IO.Path.GetFileNameWithoutExtension(vv) + ".deps.json";
                            //LoadRefernceDll(sfile);

                        }
                        catch(Exception eex)
                        {
                            LoggerService.Service.Erro("ScriptAlarmTagRun", eex.Message);
                        }
                    }

                }
                sop = sop.AddReferences(typeof(System.Collections.Generic.ReferenceEqualityComparer).Assembly).AddReferences(typeof(ScriptExtend).Assembly).AddReferences(typeof(ScriptAlarmTagRun).Assembly).WithImports("AntRuntime.Enginer", "Cdy.Ant.Tag", "Cdy.Ant", "System", "System.Collections.Generic", "System.Linq", "System.Text");
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("ScriptAlarmTagRun", ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            lock (mLockObj)
            {

                if (mTimer != null)
                {
                    mTimer.Dispose();
                    mTimer = null;
                }

                Message = new MessageScriptImp() { Owner = this };
                Tag = new TagScriptImp() { Owner = this };
                Logger = new LoggerImp();

                var vsp = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create(mDTag.Expresse, sop, typeof(ScriptAlarmTagRun));
                try
                {
                    var cp = vsp.Compile();
                    if (cp != null && cp.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var vvp in cp)
                        {
                            sb.Append(vvp.ToString());
                        }
                        LoggerService.Service.Warn("ScriptAlarmTagRun", mDTag.FullName + " " + sb.ToString());
                    }
                    mScript = vsp;

                }
                catch (Exception ex)
                {
                    LoggerService.Service.Erro("ScriptAlarmTagRun", ex.Message);
                }
                base.Init();
                mNeedCal = true;
            }
        }

        /// <summary>
        /// 对于类型为Trigger 为 Start类型的
        /// </summary>
        public override void PreRun()
        {
            if (mDTag.StartTrigger.Type == TriggerType.Start)
            {
                ExecuteExpress();
                if(mDTag.Mode == ExecuteMode.Repeat)
                {
                    mTimer =  new Timer((obj)=> {
                        ExecuteExpress();
                    },null,0,mDTag.Duration);     
                }
            }
            base.PreRun();
        }



        /// <summary>
        /// 
        /// </summary>
        public void ExecuteExpress()
        {
            //如果上次没有执行完，则直接返回
            if (mResult != null && !mResult.IsCompleted) return;
            mResult = mScript?.RunAsync(this, (exp) =>
            {
                LoggerService.Service.Erro("ScriptAlarmTagRun", this.mDTag.FullName + " : " + exp.Message);
                return true;
            });
            mLastExecuteTime = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void LinkExecute()
        {
            try
            {
                lock (mLockObj)
                {
                    if (mDTag.StartTrigger.Type == TriggerType.TagChanged)
                    {
                        ExecuteExpress();
                        //mScript?.RunAsync(this, (exp) =>
                        //{
                        //    LoggerService.Service.Erro("ScriptAlarmTagRun", this.mDTag.FullName + " : " + exp.Message);
                        //    return true;
                        //});
                        //if (mIsNeedCallAlways) mNeedCal = true;
                    }
                    else if (mDTag.StartTrigger.Type == TriggerType.Timer)
                    {
                        var dnow = DateTime.Now;
                        if (CheckTimeCondition())
                        {
                            if (mDTag.Mode == ExecuteMode.Repeat && ((dnow - mLastExecuteTime).TotalMilliseconds > mDTag.Duration))
                            {
                                ExecuteExpress();
                            }
                            else if ((dnow > mNextExecuteTime || mLastExecuteTime == DateTime.MinValue) && mDTag.Mode == ExecuteMode.Once)
                            {
                                ExecuteExpress();
                            }
                        }
                        mNeedCal = true;
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 计算定时条件是否满足
        /// </summary>
        /// <returns></returns>
        private bool CheckTimeCondition()
        {

            DateTime dnow = DateTime.Now;

            DateTime nexttime = dnow;

            string stime = (mDTag.StartTrigger as TimerTrigger).Timer;
            //时间格式为 yyyy-mm-dd|HH:dd:ss

            int? year = null;
            int? month = null;
            int? day = null;
            int? hour = null;
            int? min = null;
            int? sec = null;

            string[] ss = stime.Split("|");
            foreach (var vv in ss)
            {
                if (vv.Contains("-"))
                {
                    string[] dd = vv.Split("-");
                    dd = dd.Reverse().ToArray();
                    if (int.TryParse(dd[0], out int imp))
                    {
                        day = imp;
                    }

                    if (dd.Length > 1)
                    {
                        if (int.TryParse(dd[1], out imp))
                        {
                            month = imp;
                        }
                    }
                    if (dd.Length > 2)
                    {
                        if (int.TryParse(dd[2], out imp))
                        {
                            year = imp;
                        }
                    }
                }
                else
                {
                    string[] dd = vv.Split(":");
                    dd = dd.Reverse().ToArray();
                    if (int.TryParse(dd[0], out int imp))
                    {
                        sec = imp;
                    }
                    if (dd.Length > 1)
                    {
                        if (int.TryParse(dd[1], out imp))
                        {
                            min = imp;
                        }
                    }
                    if (dd.Length > 2)
                    {
                        if (int.TryParse(dd[2], out imp))
                        {
                            hour = imp;
                        }
                    }
                }
            }

            bool isAdded = false;
            bool re = true;
            if (year != null)
            {
                re &= (dnow.Year == year.Value);
                if (re && !isAdded) { nexttime = nexttime.AddYears(1000); isAdded = true; }
            }

            if (month != null)
            {
                re &= (dnow.Month == month.Value);
                if (re && !isAdded) { nexttime = nexttime.AddYears(1); isAdded = true; }
            }

            if (day != null)
            {
                re &= (dnow.Day == day.Value);
                if (re && !isAdded) { nexttime = nexttime.AddMonths(1); isAdded = true; }
            }

            if (hour != null)
            {
                re &= (dnow.Hour == hour.Value);
                if (re && !isAdded) { nexttime = nexttime.AddDays(1); isAdded = true; }
            }
            if (min != null)
            {
                re &= (dnow.Minute == min.Value);
                if (re && !isAdded) { nexttime = nexttime.AddHours(1); isAdded = true; }
            }

            if (sec != null)
            {
                re &= (dnow.Second == sec.Value);
                if (re && !isAdded)
                {
                    nexttime = nexttime.AddMinutes(1); isAdded = true;
                }
            }
            if (re)
                mNextExecuteTime = nexttime;
            else
            {
                //如果优于程序执行慢，导致错过了执行条件，也要补上让其执行
                if (dnow > mNextExecuteTime && mLastExecuteTime!=DateTime.MinValue)
                {
                    re = true;
                }
            }

            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> ListLinkTag()
        {
            var re = AnalysizeTags(mDTag.Expresse);
            var rr = base.ListLinkTag();
            rr.AddRange(re);
            return rr;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private List<string> AnalysizeTags(string exp)
        {
             Regex regex = new Regex(@"\bTag((\.\w*)(?!\())*\b",
             RegexOptions.IgnoreCase | RegexOptions.Multiline |
             RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            List<string> ltmp = new List<string>();

            var vvs = regex.Matches(exp);
            if (vvs.Count > 0)
            {
                foreach (var vv in vvs)
                {
                    ltmp.Add(vv.ToString());
                }
            }

            return ltmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="messageBody"></param>
        /// <param name="value"></param>
        /// <param name="alarmCondition"></param>
        public void Alarm(string source, Cdy.Ant.AlarmLevel level, string messageBody, string value, string alarmCondition)
        {
            DateTime dt = DateTime.Now;
            Cdy.Ant.AlarmMessage msg = new Cdy.Ant.AlarmMessage();
            msg.CreateTime = dt;
            msg.Server = source;
            msg.SourceTag = TagName;
            if ((LinkedTag is AlarmTag) && (!string.IsNullOrEmpty((LinkedTag as AlarmTag).LinkTag)))
            {
                msg.LinkTag = (LinkedTag as AlarmTag).LinkTag;
            }
            else
            {
                msg.LinkTag = "";
            }
            msg.MessageBody = messageBody;
            msg.AlarmLevel = level;
            msg.AlarmValue = value;
            msg.Id = MessageService.Service.GetId(dt.Ticks);
            msg.AppendContent1 = LinkedTag.CustomContent1;
            msg.AppendContent2 = LinkedTag.CustomContent2;
            msg.AppendContent3 = LinkedTag.CustomContent3;
            msg.AlarmCondition = alarmCondition;
            mCurrentMessageId = msg.Id;

            MessageService.Service.RaiseMessage(msg);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="messageBody"></param>
        /// <param name="value"></param>
        /// <param name="alarmCondition"></param>
        /// <param name="linktag"></param>
        public void Alarm(string source, Cdy.Ant.AlarmLevel level, string messageBody, string value, string alarmCondition,string linktag)
        {
            DateTime dt = DateTime.Now;
            Cdy.Ant.AlarmMessage msg = new Cdy.Ant.AlarmMessage();
            msg.CreateTime = dt;
            msg.Server = source;
            msg.SourceTag = TagName;
            msg.LinkTag = linktag;
            msg.MessageBody = messageBody;
            msg.AlarmLevel = level;
            msg.AlarmValue = value;
            msg.Id = MessageService.Service.GetId(dt.Ticks);
            msg.AppendContent1 = LinkedTag.CustomContent1;
            msg.AppendContent2 = LinkedTag.CustomContent2;
            msg.AppendContent3 = LinkedTag.CustomContent3;
            msg.AlarmCondition = alarmCondition;
            mCurrentMessageId = msg.Id;

            MessageService.Service.RaiseMessage(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            DateTime dt = DateTime.Now;
            Cdy.Ant.InfoMessage msg = new InfoMessage();
            msg.CreateTime = dt;
            msg.Server = this.Source;
            msg.SourceTag = TagName;
            msg.MessageBody = message;
            msg.Id = MessageService.Service.GetId(dt.Ticks);
            
            msg.AppendContent1 = LinkedTag.CustomContent1;
            msg.AppendContent2 = LinkedTag.CustomContent2;
            msg.AppendContent3 = LinkedTag.CustomContent3;

            MessageService.Service.RaiseMessage(msg);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Restore(string value)
        {
            MessageService.Service.RestoreMessage(mCurrentMessageId, value);
        }


        public override void Dispose()
        {
            if (mTimer != null)
            {
                mTimer.Dispose();
                mTimer = null;
            }
            base.Dispose();
        }


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 
    /// </summary>
    public class TagScriptImp
    {
        /// <summary>
        /// 
        /// </summary>
        public ScriptAlarmTagRun Owner { get; set; }

        /// <summary>
        /// 用于缓存中间数据的对象
        /// </summary>
        public Dictionary<string, object> GlobalObject { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 查询变量的历史值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="span">时间间隔</param>
        /// <returns></returns>
        public Dictionary<DateTime, Tuple<object, byte>> QueryHisValue(string tagName, DateTime startTime, DateTime endTime, TimeSpan span)
        {
            return Owner.TagService?.QueryHisValue(tagName, startTime, endTime, span);
        }

        /// <summary>
        /// 查询某个时间段记录的所有的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public Dictionary<DateTime, Tuple<object, byte>> QueryAllHisValue(string tagName, DateTime startTime, DateTime endTime)
        {
            return Owner.TagService?.QueryAllHisValue(tagName, startTime, endTime);
        }

        /// <summary>
        /// 查询执行时刻点的的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="times">时间点集合</param>
        /// <returns></returns>
        public Dictionary<DateTime, Tuple<object, byte>> QueryHisValue(string tagName, List<DateTime> times)
        {
            return Owner.TagService?.QueryHisValue(tagName, times);
        }

        /// <summary>
        /// 查询变量好的历史值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="span">时间间隔</param>
        /// <returns></returns>
        public Dictionary<DateTime, object> QueryGoodHisValue(string tagName, DateTime startTime, DateTime endTime, TimeSpan span)
        {
            return Owner.TagService?.QueryGoodHisValue(tagName,startTime,endTime,span);
        }

        /// <summary>
        /// 查询某个时间段记录的所有好的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public Dictionary<DateTime, object> QueryAllGoodHisValue(string tagName, DateTime startTime, DateTime endTime)
        {
            return Owner.TagService?.QueryAllGoodHisValue(tagName,startTime,endTime);
        }

        /// <summary>
        /// 查询执行时刻点的好的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="times">时间点集合</param>
        /// <returns></returns>
        public Dictionary<DateTime, object> QueryGoodHisValue(string tagName, List<DateTime> times)
        {
            return Owner.TagService?.QueryGoodHisValue(tagName,times);
        }

        /// <summary>
        /// 获取变量的值
        /// </summary>
        /// <param name="tag">格式:"Tag.设备名.点名"</param>
        /// <returns></returns>
        public object GetTagValue(string tag)
        {
            return Owner.TagService?.GetTagValue(tag);
        }

        /// <summary>
        /// 设置变量的值
        /// </summary>
        /// <param name="tag">格式:"Tag.设备名.点名"</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetTagValue(string tag, object value)
        {
            Owner.TagService?.SetTagValue(tag, value);
            return true;
        }

        /// <summary>
        /// 变量的质量戳是否为有效值
        /// </summary>
        /// <param name="tag">格式:"Tag.设备名.点名"</param>
        /// <returns></returns>
        public bool IsTagQualityGood(string tag)
        {
            return Owner.TagService?.GetTagValueQuality(tag) == 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public double TagValueSum(params string[] tags)
        {
            try
            {
                double[] dtmps = new double[tags.Length];
                for (int i = 0; i < tags.Length; i++)
                {
                    dtmps[i] = Convert.ToDouble(GetTagValue(tags[i]));
                }
                return dtmps.Sum();
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("Calculate", ex.StackTrace);
            }
            return 0;
        }

        /// <summary>
        /// 对变量的值求平局
        /// </summary>
        /// <param name="tags">变量名</param>
        /// <returns></returns>
        public double TagValueAvg(params string[] tags)
        {
            try
            {
                double[] dtmps = new double[tags.Length];
                for (int i = 0; i < tags.Length; i++)
                {
                    dtmps[i] = Convert.ToDouble(GetTagValue(tags[i]));
                }
                return dtmps.Average();
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("Calculate", ex.StackTrace);
            }
            return 0;
        }

        /// <summary>
        /// 对变量的值取最大
        /// </summary>
        /// <param name="tags">变量名</param>
        /// <returns></returns>
        public double TagValueMax(params string[] tags)
        {
            try
            {
                double[] dtmps = new double[tags.Length];
                for (int i = 0; i < tags.Length; i++)
                {
                    dtmps[i] = Convert.ToDouble(GetTagValue(tags[i]));
                }
                return dtmps.Max();
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("Calculate", ex.StackTrace);
            }
            return 0;
        }

        /// <summary>
        /// 对变量的值取最小
        /// </summary>
        /// <param name="tags">变量名</param>
        /// <returns></returns>
        public double TagValueMin(params string[] tags)
        {
            try
            {
                double[] dtmps = new double[tags.Length];
                for (int i = 0; i < tags.Length; i++)
                {
                    dtmps[i] = Convert.ToDouble(GetTagValue(tags[i]));
                }
                return dtmps.Min();
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("Calculate", ex.StackTrace);
            }
            return 0;
        }

        /// <summary>
        /// 对数值进行请平均值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public double Avg(params object[] values)
        {
            try
            {
                double[] dtmps = new double[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dtmps[i] = Convert.ToDouble(values[i]);
                }
                return dtmps.Average();
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("Calculate", ex.StackTrace);
            }
            return 0;
        }

        /// <summary>
        /// 对数值进行取最大值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public double Max(params object[] values)
        {
            try
            {
                double[] dtmps = new double[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dtmps[i] = Convert.ToDouble(values[i]);
                }
                return dtmps.Max();
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("Calculate", ex.StackTrace);
            }
            return 0;
        }

        /// <summary>
        /// 对数值进行取最小值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public double Min(params object[] values)
        {
            try
            {
                double[] dtmps = new double[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dtmps[i] = Convert.ToDouble(values[i]);
                }
                return dtmps.Min();
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("Calculate", ex.StackTrace);
            }
            return 0;
        }

        /// <summary>
        /// 对值进行取位
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="index">要取位的序号，从0开始</param>
        /// <returns></returns>
        public byte Bit(object value, byte index)
        {
            var val = Convert.ToInt64(value);
            return (byte)(val >> index & 0x01);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MessageScriptImp
    {
        /// <summary>
        /// 
        /// </summary>
        public ScriptAlarmTagRun Owner { get; set; }

        /// <summary>
        /// 查询消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public  IEnumerable<Cdy.Ant.Message> QueryMessage(DateTime startTime, DateTime endTime)
        {
            return MessageService.Service.Query(startTime,endTime);
        }

        /// <summary>
        /// 查询消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="filters">过滤条件</param>
        /// <returns></returns>
        public  IEnumerable<Cdy.Ant.Message> QueryMessage(DateTime startTime, DateTime endTime,params string[] filters)
        {
            return MessageService.Service.Query(startTime, endTime,filters.GetFiltersFromString());
        }

        /// <summary>
        /// 查询报警消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public  IEnumerable<Cdy.Ant.AlarmMessage> QueryAlarmMessage(DateTime startTime, DateTime endTime)
        {
            return MessageService.Service.Query(startTime, endTime).Where(e=>e is Cdy.Ant.AlarmMessage).Select(e=>e as Cdy.Ant.AlarmMessage);
        }

        /// <summary>
        /// 查询报警消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="filters">过滤条件</param>
        /// <returns></returns>
        public  IEnumerable<Cdy.Ant.AlarmMessage> QueryAlarmMessage(DateTime startTime, DateTime endTime, params string[] filters)
        {
            return MessageService.Service.Query(startTime, endTime,filters.GetFiltersFromString()).Where(e => e is Cdy.Ant.AlarmMessage).Select(e => e as Cdy.Ant.AlarmMessage);
        }


        /// <summary>
        /// 产生报警
        /// </summary>
        /// <param name="messageBody">消息体</param>
        /// <param name="value">报警值</param>
        /// <param name="level">报警级别</param>
        public void Alarm(string messageBody, string value, Cdy.Ant.AlarmLevel level)
        {
            Owner?.Alarm(Owner.Source, level, messageBody, value, "");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageBody"></param>
        /// <param name="value"></param>
        /// <param name="level"></param>
        /// <param name="linkTag"></param>
        public void Alarm(string messageBody, string value, Cdy.Ant.AlarmLevel level,string linkTag)
        {
            Owner?.Alarm(Owner.Source, level, messageBody, value, "",linkTag);
        }

        /// <summary>
        /// 产生报警
        /// </summary>
        /// <param name="value">报警值</param>
        /// <param name="level">报警级别</param>
        public void Alarm(string value, Cdy.Ant.AlarmLevel level)
        {
            Owner?.Alarm(Owner.Source, level, Owner.LinkedTag.Desc, value, "");
        }

        /// <summary>
        /// 产生一条消息
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            Owner?.Info(message);
        }

        /// <summary>
        /// 恢复报警
        /// </summary>
        /// <param name="value">恢复值</param>
        public void Restore(string value)
        {
            Owner?.Restore(value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LoggerImp
    {
        /// <summary>
        /// 输出提示信息
        /// </summary>
        /// <param name="source">触发者</param>
        /// <param name="msg">消息内容</param>
        public void Info(string source, string msg)
        {
            LoggerService.Service.Info(source, msg);
        }

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="source">触发者</param>
        /// <param name="msg">消息内容</param>
        public void Warn(string source, string msg)
        {
            LoggerService.Service.Warn(source, msg);
        }

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="source">触发者</param>
        /// <param name="msg">消息内容</param>
        public void Erro(string source, string msg)
        {
            LoggerService.Service.Erro(source, msg);
        }
    }


}
