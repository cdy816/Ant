﻿using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 报警状态
    /// </summary>
    public enum AlarmStatue
    {
        /// <summary>
        /// 提示信息
        /// </summary>
        Info = 0,
        /// <summary>
        /// 预警
        /// </summary>
        EarlyWarning = 1,
        /// <summary>
        /// 一般
        /// </summary>
        Normal = 2,
        /// <summary>
        /// 重要
        /// </summary>
        Critical = 3,
        /// <summary>
        /// 紧急
        /// </summary>
        Urgency = 4,

        /// <summary>
        /// 非常紧急
        /// </summary>
        VeryUrgency = 5,

        /// <summary>
        /// 无报警
        /// </summary>
        None=6
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class AlarmTagRunBase:TagRunBase
    {

        #region ... Variables  ...
        
        protected Cdy.Ant.AlarmTag mTagModel;

        private long mCurrentMessageId = 0;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        public AlarmTagRunBase()
        {
            CurrentStatue = AlarmStatue.None;
        }
        #endregion ...Constructor...

        #region ... Properties ...


        /// <summary>
        /// 当前报警状态
        /// </summary>
        public AlarmStatue CurrentStatue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override object Value { get => base.Value; set { base.Value = value; CheckTagValueAlarm(); } }

        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mTagModel = value as Cdy.Ant.AlarmTag; } }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveRuntimeStatue(string keyName)
        {
            var re = base.SaveRuntimeStatue(keyName);
            re.SetAttributeValue("CurrentStatue", (int)CurrentStatue);
            re.SetAttributeValue("CurrentMessageId", mCurrentMessageId);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadRuntimeStatue(XElement xe)
        {
            base.LoadRuntimeStatue(xe);
            CurrentStatue = xe.Attribute("CurrentStatue") != null ? (AlarmStatue)int.Parse(xe.Attribute("CurrentStatue").Value) : AlarmStatue.None;
            mCurrentMessageId = xe.Attribute("CurrentMessageId") != null ? long.Parse(xe.Attribute("CurrentMessageId").Value) : 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> ListLinkTag()
        {
            return new List<string>() { mTagModel.LinkTag };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="messageBody"></param>
        /// <param name="value"></param>
        /// <param name="alarmCondition"></param>
        public void Alarm(string source,Cdy.Ant.AlarmLevel level,string messageBody,string value,string alarmCondition)
        {
            DateTime dt = DateTime.Now;
            Cdy.Ant.Tag.AlarmMessage msg = new Cdy.Ant.Tag.AlarmMessage();
            msg.CreateTime = dt;
            msg.Server = source;
            msg.SourceTag = TagName;
            if ((LinkedTag is AlarmTag)&&(!string.IsNullOrEmpty((LinkedTag as AlarmTag).LinkTag)))
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
            msg.Id = MessageService.Service.GetId(TimerIdHelper.GetTicks(dt));
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
        /// <param name="value"></param>
        public void Restore(string value)
        {
            MessageService.Service.RestoreMessage(mCurrentMessageId, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void LinkExecute()
        {
            var tapi = ServiceLocator.Locator.Resolve<IDataTagApi>();
            if (tapi != null && tapi.TagService != null && !string.IsNullOrEmpty(mTagModel.LinkTag))
                this.Value = tapi.TagService.GetTagValue(mTagModel.LinkTag);
        }

        /// <summary>
        /// 检查变量的值
        /// </summary>
        public virtual void CheckTagValueAlarm()
        {

        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
