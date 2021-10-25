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
    public class PulseAlarmTagRun:AlarmTagRunBase
    {

        #region ... Variables  ...

        Cdy.Ant.PulseAlarmTag mDTag;

        private bool mOValue = false;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mDTag = value as Cdy.Ant.PulseAlarmTag; } }

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag => TagType.Pulse;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 每次发生变化，则会产生报警，同时不会产生回复信息
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            bool bval = Convert.ToBoolean(Value);
            if (bval!=mOValue)
            {
                if (mDTag.PulseType == PulseAlarmType.All || mDTag.PulseType == PulseAlarmType.Rise)
                {
                    if (bval)
                    {
                        Alarm(Source, mDTag.AlarmLevel, mDTag.Desc, bval.ToString(),mDTag.PulseType.ToString());
                        CurrentStatue = (AlarmStatue)((byte)mDTag.AlarmLevel);
                    }
                }
                else if(mDTag.PulseType == PulseAlarmType.All || mDTag.PulseType == PulseAlarmType.Fall)
                {
                    if (!bval)
                    {
                        Alarm(Source, mDTag.AlarmLevel, mDTag.Desc, bval.ToString(),mDTag.PulseType.ToString());
                        CurrentStatue = (AlarmStatue)((byte)mDTag.AlarmLevel);
                    }
                }
                mOValue = bval;
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }
}
