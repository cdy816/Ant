using Cdy.Ant;
using Cdy.Ant.Tag;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private bool mIsNeedCallAlways = false;

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
        public override TagType SupportTag => TagType.Script;

        private object mLockObj = new object();

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
            }
            base.OnPropertyChangedForRuntime(name, value);
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
                }
                sop = sop.AddReferences(typeof(System.Collections.Generic.ReferenceEqualityComparer).Assembly).AddReferences(typeof(ScriptExtend).Assembly).AddReferences(typeof(ScriptAlarmTagRun).Assembly).WithImports("AntRuntime.Enginer", "Cdy.Ant.Tag", "System", "System.Collections.Generic");
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
                Message = new MessageScriptImp() { Owner = this };
                Tag = new TagScriptImp() { Owner = this };

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

                    //如果没有操作任何变量，则直接开个线程让其执行
                    if (ListLinkTag().Count == 0)
                    {
                        mIsNeedCallAlways = true;
                    }

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
        /// 
        /// </summary>
        public override void LinkExecute()
        {
            try
            {
                lock (mLockObj)
                {
                    mScript?.RunAsync(this, (exp) =>
                    {
                        LoggerService.Service.Erro("ScriptAlarmTagRun", this.mDTag.FullName + " : " + exp.Message);
                        return true;
                    });
                        if (mIsNeedCallAlways) mNeedCal = true;
                }
            }
            catch
            {

            }
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


}
