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
