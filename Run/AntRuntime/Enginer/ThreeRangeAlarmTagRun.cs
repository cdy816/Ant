using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    public class ThreeRangeAlarmTagRun : AlarmTagRunBase
    {

        #region ... Variables  ...
        ThreeRangeAlarmTag mTag;
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
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mTag = value as ThreeRangeAlarmTag; } }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> GetSupportModifyProperty()
        {
            var re = base.GetSupportModifyProperty();
            re.Add("AlarmLevel", ((byte)mTag.AlarmLevel).ToString());
            re.Add("AlarmDatas", mTag.AlarmDatasToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void OnPropertyChangedForRuntime(string name, string value)
        {
            if (name == "alarmdatas")
            {
                mTag.AlarmDatasFromSting(value);
            }
            else if (name == "alarmlevel")
            {
                mTag.AlarmLevel = (AlarmLevel)(int.Parse(value));
            }
            base.OnPropertyChangedForRuntime(name, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            string[] sval = Value.ToString().Split(new char[] { ',' });
            if (sval.Length <= 1) return;

            double dval1 = Convert.ToDouble(sval[0]);
            double dval2 = Convert.ToDouble(sval[1]);
            double dval3 = Convert.ToDouble(sval[2]);
            if (IsInRange(dval1,dval2,dval3))
            {
                if (mTag.IsInRangeAlarm)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mTag.AlarmLevel, mTag.Desc, Value.ToString(), "in range");
                    }
                }
                else
                {
                    if(CurrentStatue != AlarmStatue.None)
                    {
                        Restore(Value.ToString());
                    }
                }
            }
            else
            {
                if (!mTag.IsInRangeAlarm)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mTag.AlarmLevel, mTag.Desc, Value.ToString(),"out of range");
                    }
                }
                else
                {
                    if (CurrentStatue != AlarmStatue.None)
                    {
                        Restore(Value.ToString());
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
        private bool IsInRange(double dval1,double dval2,double dval3)
        {
            //todo here
            return false;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
