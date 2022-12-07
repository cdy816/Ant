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
        /// 模拟量范围报警
        /// </summary>
        AnalogRangeAlarm,
        /// <summary>
        /// 数字量
        /// </summary>
        DigitalAlarm,
        /// <summary>
        /// 延迟类型数字变量
        /// </summary>
        DelayDigitalAlarm,
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

    /// <summary>
    /// 
    /// </summary>
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
            switch (typ)
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
                default:
                    break;
            }
            return null;
        }
    }

    /// <summary>
    /// 0:提示信息,1:预警,2:一般,3:重要,4:紧急,5:非常紧急
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

        private string mCustomContent1 = "";

        private string mCustomContent2 = "";

        private string mCustomContent3 = "";

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
                    mFullName = mName;
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
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public string CustomContent1
        {
            get
            {
                return mCustomContent1;
            }
            set
            {
                if (mCustomContent1 != value)
                {
                    mCustomContent1 = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CustomContent2
        {
            get
            {
                return mCustomContent2;
            }
            set
            {
                if (mCustomContent2 != value)
                {
                    mCustomContent2 = value;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string CustomContent3
        {
            get
            {
                return mCustomContent3;
            }
            set
            {
                if (mCustomContent3 != value)
                {
                    mCustomContent3 = value;
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Type+","+Id+","+Name+","+Desc+","+Group + "," +IsEnable+","+CustomContent1 + "," + CustomContent2 + "," + CustomContent3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void LoadFromString(string val)
        {
            var ss = val.Split(new char[] { ',' });
            LoadFrom(ss.AsSpan<string>(1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        protected virtual Span<string> LoadFrom(Span<string> vals)
        {
            Id = int.Parse(vals[0]);
            Name = vals[1];
            Desc = vals[2];
            Group = vals[3];
            IsEnable = bool.Parse(vals[4]);
            CustomContent1 = vals[5];
            CustomContent2 = vals[6];
            CustomContent2 = vals[7];
            return vals.Slice(8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Tagbase Clone()
        {
            var re = SaveTo();
            var tag = TagManager.Manager.CreatTag(this.Type);
            tag.LoadFrom(re);
            return tag;
        }

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
            xe.SetAttributeValue("CustomContent1", CustomContent1);
            xe.SetAttributeValue("CustomContent2", CustomContent2);
            xe.SetAttributeValue("CustomContent3", CustomContent3);
            return xe;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public virtual void LoadFrom(XElement xe)
        {
            this.Id = int.Parse(xe.Attribute("Id").Value);
            this.Name = xe.Attribute("Name").Value;
            Group = xe.Attribute("Group").Value;
            Desc = xe.Attribute("Desc").Value;
            IsEnable = bool.Parse(xe.Attribute("IsEnable").Value);
            CustomContent1 = xe.Attribute("CustomContent1").Value;
            CustomContent2 = xe.Attribute("CustomContent2").Value;
            CustomContent3 = xe.Attribute("CustomContent3").Value;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+","+LinkTag.Replace(",","|");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            LinkTag = re[0].Replace("|", ",");
            return re.Length > 1 ? re.Slice(1) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("LinkTag", LinkTag);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            LinkTag = xe.Attribute("LinkTag") != null ? xe.Attribute("LinkTag").Value : string.Empty;
        }


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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("AlarmLevel", (int)AlarmLevel);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            if(xe.Attribute("AlarmLevel") !=null)
            {
                AlarmLevel = (AlarmLevel)int.Parse(xe.Attribute("AlarmLevel").Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+","+(byte)AlarmLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            AlarmLevel = (AlarmLevel)(int.Parse(re[0]));
            return re.Slice(1);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class AnalogRangeAlarmItem
    {
        /// <summary>
        /// 最小值
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// 报警级别
        /// </summary>
        public AlarmLevel AlarmLevel { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public double DeadArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement SaveTo()
        {
            XElement re = new XElement("AnalogRangeAlarmItem");
            re.SetAttributeValue("MinValue", MinValue);
            re.SetAttributeValue("MaxValue", MaxValue);
            re.SetAttributeValue("DeadArea", DeadArea);
            re.SetAttributeValue("AlarmLevel", (int)AlarmLevel);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public AnalogRangeAlarmItem LoadFrom(XElement xe)
        {
            if (xe.Attribute("AlarmLevel") != null)
            {
                AlarmLevel = (AlarmLevel)int.Parse(xe.Attribute("AlarmLevel").Value);
            }
            if (xe.Attribute("MinValue") != null)
            {
                MinValue = double.Parse(xe.Attribute("MinValue").Value);
            }
            if (xe.Attribute("MaxValue") != null)
            {
                MaxValue = double.Parse(xe.Attribute("MaxValue").Value);
            }
            if (xe.Attribute("DeadArea") != null)
            {
                DeadArea = double.Parse(xe.Attribute("DeadArea").Value);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (byte)AlarmLevel+","+MinValue+","+MaxValue+","+DeadArea;
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sval"></param>
        /// <returns></returns>
        public AnalogRangeAlarmItem LoadFromString(string sval)
        {
            string[] ss = sval.Split(new char[] { ',' });
            AlarmLevel = (AlarmLevel)(int.Parse(ss[0]));
            MinValue = double.Parse(ss[1]);
            MaxValue = double.Parse(ss[2]);
            DeadArea = double.Parse(ss[3]);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public AnalogRangeAlarmItem LoadFromSpan(Span<string> ss)
        {
            AlarmLevel = (AlarmLevel)(int.Parse(ss[0]));
            MinValue = double.Parse(ss[1]);
            MaxValue = double.Parse(ss[2]);
            DeadArea = double.Parse(ss[3]);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AnalogRangeAlarmItem Clone()
        {
            AnalogRangeAlarmItem re =new AnalogRangeAlarmItem();
            re.AlarmLevel = this.AlarmLevel;
            re.MinValue= this.MinValue;
            re.MaxValue= this.MaxValue;
            re.DeadArea= this.DeadArea;
            return re;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class AnalogRangeAlarmTag : AlarmTag
    {
        /// <summary>
        /// 
        /// </summary>
        public List<AnalogRangeAlarmItem> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override TagType Type => TagType.AnalogRangeAlarm;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            if(Items!=null)
            {
                foreach(var vv in Items)
                {
                    re.Add(vv.SaveTo());
                }
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            Items = new List<AnalogRangeAlarmItem>();
            base.LoadFrom(xe);
            foreach(var vv in xe.Elements("AnalogRangeAlarmItem"))
            {
                Items.Add(new AnalogRangeAlarmItem().LoadFrom(vv));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var vv in Items)
            {
                sb.Append("," + vv.ToString());
            }
            return base.ToString()+sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            while (re.Length > 0)
            {
                this.Items.Add(new AnalogRangeAlarmItem().LoadFromSpan(re));
                if (re.Length > 4)
                {
                    re = re.Slice(4);
                }
                else
                {
                    break;
                }
            }
            return re;
        }

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

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();

            XElement xe;
            if (HighHighValue != null)
            {
                xe = new XElement("HighHighValue");
                xe.Add(HighHighValue.SaveTo());
                re.Add(xe);
            }
            if (HighValue != null)
            {
                xe = new XElement("HighValue");
                xe.Add(HighValue.SaveTo());
                re.Add(xe);
            }
            if (LowValue != null)
            {
                xe = new XElement("LowValue");
                xe.Add(LowValue.SaveTo());
                re.Add(xe);
            }
            if (LowLowValue != null)
            {
                xe = new XElement("LowLowValue");
                xe.Add(LowLowValue.SaveTo());
                re.Add(xe);
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            if(xe.Element("HighHighValue") !=null)
            {
                HighHighValue = new AnalogAlarmItem().LoadFrom(xe.Element("HighHighValue").FirstNode as XElement);
            }

            if (xe.Element("HighValue") != null)
            {
                HighValue = new AnalogAlarmItem().LoadFrom(xe.Element("HighValue").FirstNode as XElement);
            }

            if (xe.Element("LowValue") != null)
            {
                LowValue = new AnalogAlarmItem().LoadFrom(xe.Element("LowValue").FirstNode as XElement);
            }

            if (xe.Element("LowLowValue") != null)
            {
                LowLowValue = new AnalogAlarmItem().LoadFrom(xe.Element("LowLowValue").FirstNode as XElement);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var re = base.ToString();
            re += "," + (HighHighValue!=null? HighHighValue.ToString():AnalogAlarmItem.Empty.ToString());
            re += "," + (HighValue != null ? HighValue.ToString():AnalogAlarmItem.Empty.ToString());
            re += "," + (LowValue != null ? LowValue.ToString():AnalogAlarmItem.Empty.ToString());
            re += "," + (LowLowValue != null ? LowLowValue.ToString():AnalogAlarmItem.Empty.ToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals); 
            HighHighValue = new AnalogAlarmItem().LoadFrom(re);
            if(HighHighValue.IsEmpty())
            {
                HighHighValue = null;
            }
            re = re.Slice(3);
            HighValue = new AnalogAlarmItem().LoadFrom(re);
            if (HighValue.IsEmpty())
            {
                HighValue = null;
            }
            re = re.Slice(3);
            LowValue = new AnalogAlarmItem().LoadFrom(re);
            if (LowValue.IsEmpty())
            {
                LowValue = null;
            }
            re = re.Slice(3);
            LowLowValue = new AnalogAlarmItem().LoadFrom(re);
            if (LowLowValue.IsEmpty())
            {
                LowLowValue = null;
            }
            return re;

        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 
    /// </summary>
    public class AnalogAlarmItem
    {
        /// <summary>
        /// 
        /// </summary>
        public static AnalogAlarmItem Empty = new AnalogAlarmItem() { Value = 0.0000000001, DeadArea = 0, AlarmLevel = AlarmLevel.Info };
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Value == 0.0000000001 && DeadArea == 0.0000000001 && AlarmLevel == AlarmLevel.Info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement SaveTo()
        {
            XElement re = new XElement("AnalogRangeAlarmItem");
            re.SetAttributeValue("Value", Value);
            re.SetAttributeValue("DeadArea", DeadArea);
            re.SetAttributeValue("AlarmLevel", (int)AlarmLevel);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public AnalogAlarmItem LoadFrom(XElement xe)
        {
            if (xe.Attribute("AlarmLevel") != null)
            {
                AlarmLevel = (AlarmLevel)int.Parse(xe.Attribute("AlarmLevel").Value);
            }
            if (xe.Attribute("Value") != null)
            {
                Value = double.Parse(xe.Attribute("Value").Value);
            }
            if (xe.Attribute("DeadArea") != null)
            {
                DeadArea = double.Parse(xe.Attribute("DeadArea").Value);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (int)AlarmLevel+","+Value+","+DeadArea;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public AnalogAlarmItem LoadFromString(string val)
        {
            string[] ss = val.Split(new char[] { ',' });
            AlarmLevel = (AlarmLevel)(int.Parse(ss[0]));
            Value = double.Parse(ss[1]);
            DeadArea = double.Parse(ss[2]);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        public AnalogAlarmItem LoadFrom(Span<string> ss)
        {
            AlarmLevel = (AlarmLevel)(int.Parse(ss[0]));
            Value = double.Parse(ss[1]);
            DeadArea = double.Parse(ss[2]);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AnalogAlarmItem Clone()
        {
            AnalogAlarmItem re = new AnalogAlarmItem();
            re.AlarmLevel = AlarmLevel;
            re.Value= Value;
            re.DeadArea= DeadArea;
            return re;
        }
    }

    /// <summary>
    /// 延时数字量报警
    /// </summary>
    public class DelayDigitalAlarmTag : SimpleAlarmTag
    {

        #region ... Variables  ...
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
        public override TagType Type => TagType.DelayDigitalAlarm;

        /// <summary>
        /// 值
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// 恢复延时
        /// </summary>
        public double Delay { get { return mDelay; } set { mDelay = value; } }

        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            if (xe.Attribute("Value") != null)
            {
                Value = bool.Parse(xe.Attribute("Value").Value);
            }
            if (xe.Attribute("Delay") != null)
            {
                Delay = double.Parse(xe.Attribute("Delay").Value);
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("IsReverse", Value);
            re.SetAttributeValue("Delay", Delay);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + "," + Delay + "," + Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            Delay = double.Parse(re[0]);
            Value = bool.Parse(re[1]);
            return re;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

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
        /// 报警值
        /// </summary>
        public bool Value { get; set; }


        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            if(xe.Attribute("Value") !=null)
            {
                Value = bool.Parse(xe.Attribute("Value").Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("Value", Value);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+","+Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            Value = bool.Parse(re[0]);
            return re;
        }

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


        public override TagType Type => TagType.Pulse;

        /// <summary>
        /// 跳变类型
        /// </summary>
        public PulseAlarmType PulseType { get; set; }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("PulseType", (int)PulseType);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            if(xe.Attribute("PulseType") !=null)
            {
                PulseType = (PulseAlarmType)int.Parse(xe.Attribute("PulseType").Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+","+(int)PulseType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re= base.LoadFrom(vals);
            PulseType = (PulseAlarmType)(int.Parse(re[0]));
            return re;
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            Value = xe.Attribute("Value") != null ? xe.Attribute("Value").Value : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("Value", Value);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+","+Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            Value = re[0];
            return re;
        }

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


        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("MinValue", MinValue);
            re.SetAttributeValue("MaxValue", MaxValue);
            re.SetAttributeValue("IsInRangeAlarm", IsInRangeAlarm);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            MaxValue = xe.Attribute("MaxValue") != null ? double.Parse(xe.Attribute("MaxValue").Value) : 0;
            MinValue = xe.Attribute("MinValue") != null ? double.Parse(xe.Attribute("MinValue").Value) : 0;
            IsInRangeAlarm = xe.Attribute("IsInRangeAlarm") != null ? bool.Parse(xe.Attribute("IsInRangeAlarm").Value) : true;
        }

        public override string ToString()
        {
            return base.ToString()+","+IsInRangeAlarm + ","+MaxValue + ","+MinValue;
        }

        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            IsInRangeAlarm = bool.Parse(re[0]);
            MaxValue = double.Parse(re[1]);
            MinValue = double.Parse(re[2]);
            return re;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 二维区域报警
    /// </summary>
    public class TwoRangeAlarmTag : SimpleAlarmTag
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

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("IsInRangeAlarm", IsInRangeAlarm);
            StringBuilder sb = new StringBuilder();
            foreach (var vv in AlarmDatas)
            {
                sb.Append(vv.X + "," + vv.Y + ";");
            }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            re.Add(sb.ToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            if (xe.Attribute("IsInRangeAlarm") != null)
            {
                IsInRangeAlarm = bool.Parse(xe.Attribute("IsInRangeAlarm").Value);
            }
            AlarmDatas = new List<Point>();
            string[] ss = xe.Value.Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { ',' });
                    double val1 = double.Parse(s1[0]);
                    double val2 = double.Parse(s1[1]);
                    AlarmDatas.Add(new Point() { X = val1, Y = val2 });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var vv in AlarmDatas)
            {
                sb.Append(vv.X + "|" + vv.Y + ";");
            }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            return base.ToString() + "," + IsInRangeAlarm + "," + sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string AlarmDatasToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var vv in AlarmDatas)
            {
                sb.Append(vv.X + "|" + vv.Y + ";");
            }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void AlarmDatasFromSting(string val)
        {
            AlarmDatas = new List<Point>();
            string[] ss = val.Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { '|' });
                    AlarmDatas.Add(new Point() { X = double.Parse(s1[0]), Y = double.Parse(s1[1]) });
                }
            }
        }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="vals"></param>
    /// <returns></returns>
    protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            IsInRangeAlarm = bool.Parse(re[0]);
            AlarmDatas = new List<Point>();
            string[] ss = re[1].Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { '|' });
                    AlarmDatas.Add(new Point() { X = double.Parse(s1[0]), Y = double.Parse(s1[1]) });
                }
            }
            return re;
        }

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

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string AlarmDatasToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var vv in AlarmDatas)
            {
                sb.Append(vv.X + "|" + vv.Y + "|" + vv.Z + ";");
            }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void AlarmDatasFromSting(string val)
        {
            AlarmDatas = new List<Point3D>();
            string[] ss = val.Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { '|' });
                    AlarmDatas.Add(new Point3D() { X = double.Parse(s1[0]), Y = double.Parse(s1[1]), Z = double.Parse(s1[2]) });
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.SetAttributeValue("IsInRangeAlarm", IsInRangeAlarm);
            StringBuilder sb = new StringBuilder();
            foreach (var vv in AlarmDatas)
            {
                sb.Append(vv.X + ","+vv.Y+"," + vv.Z + ";");
            }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            re.Add(sb.ToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            if (xe.Attribute("IsInRangeAlarm") != null)
            {
                IsInRangeAlarm = bool.Parse(xe.Attribute("IsInRangeAlarm").Value);
            }
            AlarmDatas = new List<Point3D>();
            string[] ss = xe.Value.Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { ',' });
                    AlarmDatas.Add(new Point3D() { X = double.Parse(s1[0]), Y = double.Parse(s1[1]),Z= double.Parse(s1[2]) });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var vv in AlarmDatas)
            {
                sb.Append(vv.X + "|" + vv.Y + "|" + vv.Z + ";");
            }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            return base.ToString()+","+IsInRangeAlarm+","+sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            IsInRangeAlarm = bool.Parse(re[0]);
            AlarmDatas = new List<Point3D>();
            string[] ss = re[1].Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { '|' });
                    AlarmDatas.Add(new Point3D() { X = double.Parse(s1[0]), Y = double.Parse(s1[1]), Z = double.Parse(s1[2]) });
                }
            }
            return re;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScriptTag : AlarmTag
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
        /// 表达式
        /// </summary>
        public string Expresse { get; set; }

        /// <summary>
        /// 启动触发
        /// </summary>
        public TriggerBase StartTrigger { get; set; } = new TagChangedTrigger();

        /// <summary>
        /// 执行模式
        /// </summary>
        public ExecuteMode Mode { get; set; } = ExecuteMode.Once;

        /// <summary>
        /// 间隔时间
        /// 单位:毫秒
        /// </summary>
        public int Duration { get; set; } = 1000;

        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveTo()
        {
            var re = base.SaveTo();
            re.Add(new XCData(Expresse));
            re.SetAttributeValue("Mode", (int)Mode);
            re.SetAttributeValue("Duration", Duration);
            re.SetAttributeValue("StartTrigger", StartTrigger.ToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFrom(XElement xe)
        {
            base.LoadFrom(xe);
            this.Expresse = xe.Value;
            if(xe.Attribute("Mode") !=null)
            {
                Mode = (ExecuteMode)(int.Parse(xe.Attribute("Mode").Value));
            }

            if(xe.Attribute("Duration") !=null)
            {
                Duration = int.Parse(xe.Attribute("Duration").Value);
            }

            if (xe.Attribute("StartTrigger") != null)
            {
                StartTrigger = LoadTriggerFromString(xe.Attribute("StartTrigger").Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TriggerBase LoadTriggerFromString(string value)
        {
            var ss = value.Split("$");
            if(ss[0]== "Start")
            {
                return new StartTrigger();
            }
            else if(ss[0]== "TagChanged")
            {
                return new TagChangedTrigger();
            }
            else if(ss[0]== "Timer")
            {
               return new TimerTrigger().LoadFromString(ss[1]);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+"," + StartTrigger.ToString()+","+Mode+","+Duration+"," +Expresse.Replace(",", "&#44").Replace("\r", "&#13").Replace("\n", "&#10");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        protected override Span<string> LoadFrom(Span<string> vals)
        {
            var re = base.LoadFrom(vals);
            StartTrigger = LoadTriggerFromString(re[0]);
            Mode = (ExecuteMode)Enum.Parse(typeof(ExecuteMode),re[1]);
            Duration = int.Parse(re[2]);
            Expresse = re[3].Replace("&#44", ",").Replace("&#13", "\r").Replace("&#10", "\n");
            return re;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 执行模式
    /// </summary>
    public enum ExecuteMode
    {
        /// <summary>
        /// 执行一次
        /// </summary>
        Once,
        /// <summary>
        /// 重复执行
        /// </summary>
        Repeat
    }

    /// <summary>
    /// 触发条件
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        /// 变量改变
        /// </summary>
        TagChanged,
        /// <summary>
        /// 定时
        /// </summary>
        Timer,
        /// <summary>
        /// 程序启动
        /// </summary>
        Start
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class TriggerBase
    {
        /// <summary>
        /// 
        /// </summary>
       public abstract TriggerType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Type.ToString();
        }

        public virtual TriggerBase Clone()
        {
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TagChangedTrigger:TriggerBase
    {
        /// <summary>
        /// 
        /// </summary>
        public override TriggerType Type => TriggerType.TagChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TriggerBase Clone()
        {
            return new TagChangedTrigger();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StartTrigger:TriggerBase
    {
        /// <summary>
        /// 
        /// </summary>
        public override TriggerType Type => TriggerType.Start;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TriggerBase Clone()
        {
            return new StartTrigger();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class TimerTrigger:TriggerBase
    {
        /// <summary>
        /// 
        /// </summary>
        public override TriggerType Type => TriggerType.Timer;

        /// <summary>
        /// 
        /// </summary>
        public string Timer { get; set; } = "--|9::";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString()+"$"+Timer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public TimerTrigger LoadFromString(string val)
        {
            Timer = val;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override TriggerBase Clone()
        {
            return new TimerTrigger() { Timer = this.Timer };
        }

    }


}
