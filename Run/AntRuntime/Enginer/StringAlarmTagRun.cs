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
    public class StringAlarmTagRun : AlarmTagRunBase
    {

        #region ... Variables  ...
        StringAlarmTag mDTag;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mDTag = value as Cdy.Ant.StringAlarmTag; } }

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag => TagType.StringAlarm;
        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            string sval = Convert.ToString(Value);
            if(sval == mDTag.Value)
            {
                if(CurrentStatue == AlarmStatue.None)
                {
                    Alarm(Source, mDTag.AlarmLevel, mDTag.Desc, sval);
                    CurrentStatue = (AlarmStatue)((byte)mDTag.AlarmLevel);
                }
            }
            else
            {
                if(CurrentStatue != AlarmStatue.None)
                {
                    Restore(sval);
                    CurrentStatue = AlarmStatue.None;
                }    
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
