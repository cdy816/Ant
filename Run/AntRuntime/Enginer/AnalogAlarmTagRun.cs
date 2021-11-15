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
    public class AnalogRangeAlarmTagRun : AlarmTagRunBase
    {

        #region ... Variables  ...
        Cdy.Ant.AnalogRangeAlarmTag mATag;
       protected  Cdy.Ant.AnalogRangeAlarmItem mLastAlarmItem;
        protected SortedList<double, AnalogRangeAlarmItem> mItems = new SortedList<double, AnalogRangeAlarmItem>();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mATag = value as AnalogRangeAlarmTag; } }

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag => TagType.AnalogRangeAlarm;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> GetSupportModifyProperty()
        {
            var re = base.GetSupportModifyProperty();
            if (mATag != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var vv in mATag.Items)
                {
                    sb.Append(vv.ToString() + ";");
                }
                sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
                re.Add("Items", sb.ToString());
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void OnPropertyChangedForRuntime(string name, string value)
        {
            if(name=="items")
            {
                mATag.Items.Clear();
                string[] ss = value.Split(new char[] { ';' });
                foreach(var vv in ss)
                {
                    mATag.Items.Add(new AnalogRangeAlarmItem().LoadFromString(vv));
                }
                Init();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            lock (mItems)
            {
                mItems.Clear();
                foreach (var vv in mATag.Items)
                {
                    mItems.Add(vv.MaxValue + vv.MinValue, vv);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            double dval = Convert.ToDouble(Value);

            if(CurrentStatue!= AlarmStatue.None)
            {
                var maxval = mLastAlarmItem.MaxValue + mLastAlarmItem.DeadArea;
                var minval = mLastAlarmItem.MinValue - mLastAlarmItem.DeadArea;
                if (dval >= minval && dval <= maxval) return;
            }
            AnalogRangeAlarmItem item = null;
            lock(mItems)
            foreach (var vv in mItems)
            {
                if(dval>=vv.Value.MinValue&&dval<vv.Value.MaxValue)
                {
                    item = vv.Value;
                    break;
                }
            }

            if (item != mLastAlarmItem)
            {
                if (mLastAlarmItem != null)
                {
                    Restore(dval.ToString());
                    CurrentStatue = AlarmStatue.None;
                }

                if (item != null)
                {
                    Alarm(Source, item.AlarmLevel, mATag.Desc, dval.ToString(),"["+item.MinValue+","+item.MaxValue+"]");
                    CurrentStatue = (AlarmStatue)((byte)item.AlarmLevel);
                }

                mLastAlarmItem = item;
            }
        }



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnalogAlarmTagRun : AnalogRangeAlarmTagRun
    {

        #region ... Variables  ...
        private AnalogAlarmTag mAtag;

        protected SortedList<double, AnalogRangeAlarmItem> mLowItems = new SortedList<double, AnalogRangeAlarmItem>();

        private bool mIsDirty = false;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mAtag = value as AnalogAlarmTag; } }

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag =>  TagType.AnalogAlarm;


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> GetSupportModifyProperty()
        {
            var re = base.GetSupportModifyProperty();
            if (mAtag.LowLowValue != null)
            {
                re.Add("LowLowValue", mAtag.LowLowValue.ToString());
            }

            if (mAtag.LowValue != null)
            {
                re.Add("LowValue", mAtag.LowValue.ToString());
            }

            if (mAtag.HighHighValue != null)
            {
                re.Add("HighHighValue", mAtag.HighHighValue.ToString());
            }

            if (mAtag.HighValue != null)
            {
                re.Add("HighValue", mAtag.HighValue.ToString());
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void OnPropertyChangedForRuntime(string name, string value)
        {
            switch (name)
            {
                case "lowlowvalue":
                    mAtag.LowLowValue = new AnalogAlarmItem().LoadFromString(value);
                    mIsDirty = true;
                    break;
                case "lowvalue":
                    mAtag.LowValue = new AnalogAlarmItem().LoadFromString(value);
                    mIsDirty = true;
                    break;
                case "highhighvalue":
                    mAtag.HighHighValue = new AnalogAlarmItem().LoadFromString(value);
                    mIsDirty = true;
                    break;
                case "highvalue":
                    mAtag.HighValue = new AnalogAlarmItem().LoadFromString(value);
                    mIsDirty = true;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPropertyChangedFinish()
        {
            if(mIsDirty)
            {
                Init();
                mIsDirty = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            AnalogRangeAlarmItem lastitem = null;
            lock (mLowItems)
            {
                mLowItems.Clear();
                mItems.Clear();
                if (mAtag.LowLowValue != null)
                {
                    var ll = mAtag.LowLowValue;
                    var vv = new AnalogRangeAlarmItem() { AlarmLevel = ll.AlarmLevel, MinValue = lastitem != null ? lastitem.MaxValue : double.MinValue, MaxValue = ll.Value, DeadArea = ll.DeadArea };
                    mLowItems.Add(vv.MaxValue + vv.MinValue, vv);
                    lastitem = vv;
                }

                if (mAtag.LowValue != null)
                {
                    var ll = mAtag.LowValue;
                    var vv = new AnalogRangeAlarmItem() { AlarmLevel = ll.AlarmLevel, MinValue = lastitem != null ? lastitem.MaxValue : double.MinValue, MaxValue = ll.Value, DeadArea = ll.DeadArea };
                    mLowItems.Add(vv.MaxValue + vv.MinValue, vv);
                    lastitem = vv;
                }

                lastitem = null;
                if (mAtag.HighHighValue != null)
                {
                    var ll = mAtag.HighHighValue;
                    var vv = new AnalogRangeAlarmItem() { AlarmLevel = ll.AlarmLevel, MaxValue = lastitem != null ? lastitem.MinValue : double.MaxValue, MinValue = ll.Value, DeadArea = ll.DeadArea };
                    mItems.Add(vv.MaxValue + vv.MinValue, vv);
                    lastitem = vv;
                }


                if (mAtag.HighValue != null)
                {
                    var ll = mAtag.HighValue;
                    var vv = new AnalogRangeAlarmItem() { AlarmLevel = ll.AlarmLevel, MaxValue = lastitem != null ? lastitem.MinValue : double.MaxValue, MinValue = ll.Value, DeadArea = ll.DeadArea };
                    mItems.Add(vv.MaxValue + vv.MinValue, vv);
                    lastitem = vv;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            double dval = Convert.ToDouble(Value);

            if (CurrentStatue != AlarmStatue.None && mLastAlarmItem!=null)
            {
                var maxval = mLastAlarmItem.MaxValue + mLastAlarmItem.DeadArea;
                var minval = mLastAlarmItem.MinValue - mLastAlarmItem.DeadArea;
                if (dval >= minval && dval <= maxval) return;
            }
            AnalogRangeAlarmItem item = null;

            lock(mLowItems)
            foreach (var vv in mLowItems)
            {
                if (dval > vv.Value.MinValue && dval <= vv.Value.MaxValue)
                {
                    item = vv.Value;
                    break;
                }
            }
            lock (mLowItems)
            foreach (var vv in mItems)
            {
                if (dval >= vv.Value.MinValue && dval < vv.Value.MaxValue)
                {
                    item = vv.Value;
                    break;
                }
            }

            if (item != mLastAlarmItem)
            {
                if (mLastAlarmItem != null)
                {
                    Restore(dval.ToString());
                    CurrentStatue = AlarmStatue.None;
                }

                if (item != null)
                {
                    Alarm(Source, item.AlarmLevel, mAtag.Desc, dval.ToString(), "[" + (item.MinValue==double.MinValue?"-∞":item.MinValue.ToString()) + "," + (item.MaxValue==double.MaxValue?"∞":item.MaxValue.ToString()) + "]");
                    CurrentStatue = (AlarmStatue)((byte)item.AlarmLevel);
                }

                mLastAlarmItem = item;
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

}
