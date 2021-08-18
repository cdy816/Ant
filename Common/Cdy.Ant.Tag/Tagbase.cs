//==============================================================
//  Copyright (C) 2021  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2021/8/15 16:58:50.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cdy.Ant
{

    /// <summary>
    /// 
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// 模拟量
        /// </summary>
        AnalogAlarm,
        /// <summary>
        /// 数字量
        /// </summary>
        DigitalAlarm,
        /// <summary>
        /// 字符串
        /// </summary>
        StringAlarm,
        /// <summary>
        /// 脚本
        /// </summary>
        Script,
        /// <summary>
        /// 脉冲
        /// </summary>
        Pulse,
        /// <summary>
        /// 一维区域报警
        /// </summary>
        OneRange,
        /// <summary>
        /// 二维区域报警
        /// </summary>
        TwoRange,
        /// <summary>
        /// 三维区域报警
        /// </summary>
        ThreeRange
    }

    public class TagManager
    {
        /// <summary>
        /// 
        /// </summary>
        public static TagManager Manager = new TagManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        public Tagbase CreatTag(TagType typ)
        {
            switch(typ)
            {
                case TagType.AnalogAlarm:
                    return new AnalogAlarmTag();
                case TagType.DigitalAlarm:
                    return new DigitalAlarmTag();
                case TagType.OneRange:
                    return new OneRangeAlarmTag();
                case TagType.Pulse:
                    return new PulseAlarmTag();
                case TagType.Script:
                    return new ScriptTag();
                case TagType.StringAlarm:
                    return new StringAlarmTag();
                case TagType.ThreeRange:
                    return new ThreeRangeAlarmTag();
                case TagType.TwoRange:
                    return new TwoRangeAlarmTag();
            }
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AlarmLevel
    {
        /// <summary>
        /// 提示信息
        /// </summary>
        Info=0,
        /// <summary>
        /// 预警
        /// </summary>
        EarlyWarning=1,
        /// <summary>
        /// 一般
        /// </summary>
        Normal =2,
        /// <summary>
        /// 重要
        /// </summary>
        Critical=3,
        /// <summary>
        /// 紧急
        /// </summary>
        Urgency=4,

        /// <summary>
        /// 非常紧急
        /// </summary>
        VeryUrgency = 5,

    }


    /// <summary>
    /// 
    /// </summary>
    public abstract class Tagbase
    {

        #region ... Variables  ...
        
        private string mName = "";

        private string mDesc = "";

        private string mFullName;

        private string mGroup;

        private int mId = 0;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get
            {
                return mId;
            }
            set
            {
                if (mId != value)
                {
                    mId = value;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                if (mName != value)
                {
                    mName = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Desc
        {
            get
            {
                return mDesc;
            }
            set
            {
                if (mDesc != value)
                {
                    mDesc = value;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public abstract TagType Type
        {
            get;
        }


        /// <summary>
        /// 
        /// </summary>
        public string FullName
        {
            get { return mFullName; }
            private set { mFullName = value; }
        }

        /// <summary>
        /// 组
        /// </summary>
        public string Group { get { return mGroup; } set { mGroup = value; UpdateFullName(); } }

        /// <summary>
        /// 使能报警
        /// </summary>
        public bool IsEnable { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual XElement SaveTo()
        {
            XElement xe = new XElement(Type.ToString());
            xe.SetAttributeValue("Id", Id);
            xe.SetAttributeValue("Name", Name);
            xe.SetAttributeValue("Type", (int)Type);
            xe.SetAttributeValue("Group", Group);
            xe.SetAttributeValue("Desc", Desc);
            xe.SetAttributeValue("IsEnable", IsEnable);
            return xe;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public void LoadFrom(XElement xe)
        {
            this.Id = int.Parse(xe.Attribute("Id").Value);
            this.Name = xe.Attribute("Name").Value;
            Group = xe.Attribute("Group").Value;
            Desc = xe.Attribute("Desc").Value;
            IsEnable = bool.Parse(xe.Attribute("IsEnable").Value);
        }

        /// <summary>
        /// 
        /// </summary>
        internal void UpdateFullName()
        {
            if (string.IsNullOrEmpty(Group))
            {
                mFullName = Name;
            }
            else
            {
                mFullName = Group + "." + Name;
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class AlarmTag : Tagbase
    {

        #region ... Variables  ...
        private string mLinkTag = "";
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public string LinkTag
        {
            get
            {
                return mLinkTag;
            }
            set
            {
                if (mLinkTag != value)
                {
                    mLinkTag = value;
                }
            }
        }

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class SimpleAlarmTag:AlarmTag
    {
        /// <summary>
        /// 报警级别
        /// </summary>
        public AlarmLevel AlarmLevel { get; set; }
    }


    /// <summary>
    /// 模拟量报警
    /// </summary>
    public class AnalogAlarmTag : AlarmTag
    {

        #region ... Variables  ...
        private AnalogAlarmItem mHighHighValue;
        private AnalogAlarmItem mHighValue;
        private AnalogAlarmItem mLowValue;
        private AnalogAlarmItem mLowLowValue;
        private double mDelay = 0;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.AnalogAlarm;

        /// <summary>
        /// 
        /// </summary>
        public AnalogAlarmItem HighHighValue
        {
            get
            {
                return mHighHighValue;
            }
            set
            {
                mHighHighValue = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public AnalogAlarmItem HighValue
        {
            get
            {
                return mHighValue;
            }
            set
            {
                mHighValue = value;
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public AnalogAlarmItem LowValue
        {
            get
            {
                return mLowValue;
            }
            set
            {
                mLowValue = value;
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public AnalogAlarmItem LowLowValue
        {
            get
            {
                return mLowLowValue;
            }
            set
            {
                mLowLowValue = value;
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public double Delay
        {
            get
            {
                return mDelay;
            }
            set
            {
                if (mDelay != value)
                {
                    mDelay = value;
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 
    /// </summary>
    public struct AnalogAlarmItem
    {
        /// <summary>
        /// 值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 死区
        /// </summary>
        public double DeadArea { get; set; }

        /// <summary>
        /// 报警级别
        /// </summary>
        public AlarmLevel AlarmLevel { get; set; }
    }

    /// <summary>
    /// 数字量报警
    /// </summary>
    public class DigitalAlarmTag : SimpleAlarmTag
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.DigitalAlarm;

        /// <summary>
        /// 是否取反
        /// </summary>
        public bool IsReverse { get; set; }


        /// <summary>
        /// 延时
        /// </summary>
        public double Delay
        {
            get;
            set;
        }

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 
    /// </summary>
    public enum PulseAlarmType
    {
        /// <summary>
        /// 上升沿
        /// </summary>
        Rise,
        /// <summary>
        /// 下降沿
        /// </summary>
        Fall,
        /// <summary>
        /// 所有
        /// </summary>
        All
    }


    /// <summary>
    /// 跳变报警
    /// </summary>
    public class PulseAlarmTag : SimpleAlarmTag
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...


        public override TagType Type => TagType.Script;

        /// <summary>
        /// 跳变类型
        /// </summary>
        public PulseAlarmType PulseType { get; set; }


        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class StringAlarmTag : SimpleAlarmTag
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.StringAlarm;

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }


        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 一维区域报警
    /// </summary>
    public class OneRangeAlarmTag : SimpleAlarmTag
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.OneRange;

        /// <summary>
        /// 最小值
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// 在范围内报警
        /// </summary>
        public bool IsInRangeAlarm { get; set; }

        /// <summary>
        /// 延时
        /// </summary>
        public double Delay
        {
            get;
            set;
        }

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 二维区域报警
    /// </summary>
    public class TwoRangeAlarmTag:SimpleAlarmTag
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.TwoRange;

        /// <summary>
        /// 
        /// </summary>
        public List<Point> AlarmDatas { get; set; }


        /// <summary>
        /// 在范围内报警
        /// </summary>
        public bool IsInRangeAlarm { get; set; }

        /// <summary>
        /// 延时
        /// </summary>
        public double Delay
        {
            get;
            set;
        }

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 三维区域报警
    /// </summary>
    public class ThreeRangeAlarmTag : SimpleAlarmTag
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.ThreeRange;

        /// <summary>
        /// 
        /// </summary>
        public List<Point3D> AlarmDatas { get; set; }


        /// <summary>
        /// 在范围内报警
        /// </summary>
        public bool IsInRangeAlarm { get; set; }

        /// <summary>
        /// 延时
        /// </summary>
        public double Delay
        {
            get;
            set;
        }

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScriptTag : Tagbase
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        
        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.Script;

        /// <summary>
        /// 
        /// </summary>
        public string Expresse { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }


}
