using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    public class OneRangeAlarmTagRun : AlarmTagRunBase
    {

        #region ... Variables  ...
        OneRangeAlarmTag mTag;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag =>  TagType.OneRange;

        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mTag = value as OneRangeAlarmTag; } }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            double dval = Convert.ToDouble(Value);
            if(IsInRange(dval))
            {
                if (mTag.IsInRangeAlarm)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mTag.AlarmLevel, mTag.Desc, dval.ToString());
                    }
                }
                else
                {
                    if(CurrentStatue != AlarmStatue.None)
                    {
                        Restore(dval.ToString());
                    }
                }
            }
            else
            {
                if (!mTag.IsInRangeAlarm)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mTag.AlarmLevel, mTag.Desc, dval.ToString());
                    }
                }
                else
                {
                    if (CurrentStatue != AlarmStatue.None)
                    {
                        Restore(dval.ToString());
                    }
                }
            }
            base.CheckTagValueAlarm();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dval"></param>
        /// <returns></returns>
        private bool IsInRange(double dval)
        {
            return dval >= mTag.MinValue && dval <= mTag.MaxValue;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
