using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public class DigitalAlarmTagRun:AlarmTagRunBase
    {

        #region ... Variables  ...

        Cdy.Ant.DigitalAlarmTag mDTag;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mDTag = value as Cdy.Ant.DigitalAlarmTag; } }

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag => TagType.DigitalAlarm;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> GetSupportModifyProperty()
        {
            var re = base.GetSupportModifyProperty();
            re.Add("AlarmLevel", ((byte)mDTag.AlarmLevel).ToString());
            re.Add("Value", mDTag.Value.ToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void OnPropertyChangedForRuntime(string name, string value)
        {
            if(name=="value")
            {
                mDTag.Value = bool.Parse(value);
            }
            else if(name== "alarmlevel")
            {
                mDTag.AlarmLevel = (AlarmLevel)(int.Parse(value));
            }
            base.OnPropertyChangedForRuntime(name, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            bool bval = Convert.ToBoolean(Value);
            if (mDTag.Value)
            {
                if (bval)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mDTag.AlarmLevel, mDTag.Desc, bval.ToString(),"=="+mDTag.Value);
                        CurrentStatue = (AlarmStatue)((byte)mDTag.AlarmLevel);
                    }
                }
                else if(CurrentStatue != AlarmStatue.None)
                {
                    Restore(bval.ToString());
                    CurrentStatue = AlarmStatue.None;
                }
            }
            else
            {
                if (!bval)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mDTag.AlarmLevel, mDTag.Desc, bval.ToString(),"==" + mDTag.Value);
                        CurrentStatue = (AlarmStatue)((byte)mDTag.AlarmLevel);
                    }
                }
                else if (CurrentStatue != AlarmStatue.None)
                {
                    Restore(bval.ToString());
                    CurrentStatue = AlarmStatue.None;
                }
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 
    /// </summary>
    public class DelayDigitalAlarmTagRun : AlarmTagRunBase
    {

        #region ... Variables  ...
        DelayDigitalAlarmTag mDTag;
        bool IsInPreAlarm = false;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag => TagType.DelayDigitalAlarm;

        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value;  mDTag = value as DelayDigitalAlarmTag; } }



        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> GetSupportModifyProperty()
        {
            var re = base.GetSupportModifyProperty();
            re.Add("AlarmLevel", ((byte)mDTag.AlarmLevel).ToString());
            re.Add("Value", mDTag.Value.ToString());
            re.Add("Delay", mDTag.Delay.ToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void OnPropertyChangedForRuntime(string name, string value)
        {
            if (name == "value")
            {
                mDTag.Value = bool.Parse(value);
            }
            else if (name == "alarmlevel")
            {
                mDTag.AlarmLevel = (AlarmLevel)(int.Parse(value));
            }
            else if (name == "delay")
            {
                mDTag.Delay = (double.Parse(value));
            }
            base.OnPropertyChangedForRuntime(name, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            base.Init();
        }

        private void AlarmTimeDelay(object value)
        {
            Alarm(Source, mDTag.AlarmLevel, mDTag.Desc, value.ToString(),"==" + mDTag.Value);
            CurrentStatue = (AlarmStatue)((byte)mDTag.AlarmLevel);
        }


        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            bool bval = Convert.ToBoolean(Value);
            if (mDTag.Value)
            {
                if (bval)
                {
                    if (CurrentStatue == AlarmStatue.None && !IsInPreAlarm)
                    {
                        IsInPreAlarm = true;
                        TimerHelper.Timer.Registor(mDTag.FullName, mDTag.Delay, AlarmTimeDelay, bval);
                    }
                }
                else if (CurrentStatue != AlarmStatue.None)
                {
                    Restore(bval.ToString());
                    CurrentStatue = AlarmStatue.None;
                }
                else if (IsInPreAlarm)
                {
                    TimerHelper.Timer.UnRegistor(mDTag.FullName);
                }
            }
            else
            {
                if (!bval)
                {
                    if (CurrentStatue == AlarmStatue.None && !IsInPreAlarm)
                    {
                        Alarm(Source, mDTag.AlarmLevel, mDTag.Desc, bval.ToString(), "==" + mDTag.Value);
                        CurrentStatue = (AlarmStatue)((byte)mDTag.AlarmLevel);
                    }
                }
                else if (CurrentStatue != AlarmStatue.None)
                {
                    Restore(bval.ToString());
                    CurrentStatue = AlarmStatue.None;
                }
                else if(IsInPreAlarm)
                {
                    TimerHelper.Timer.UnRegistor(mDTag.FullName);
                }
            }
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
