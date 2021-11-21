using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdy.Ant
{
    /// <summary>
    /// 消息类型，0:报警,1:通知消息
    /// </summary>
    public enum MessgeType
    {
        /// <summary>
        /// 报警
        /// </summary>
        Alarm,
        /// <summary>
        /// 通知消息
        /// </summary>
        InfoMessage
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// ID编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 生产者变量
        /// </summary>
        public string SourceTag { get; set; }

        /// <summary>
        /// 消息产生时间
        /// </summary>
        public DateTime CreateTime { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public string MessageBody
        { get; set; }

        /// <summary>
        /// 消息类型，0:报警,1:通知消息
        /// </summary>
        public abstract MessgeType Type { get; }


        /// <summary>
        /// 消息处置内容
        /// </summary>
        public List<DisposalItem> DisposalMessages { get; set; }


        /// <summary>
        /// 附加字段1
        /// </summary>
        public string AppendContent1 { get; set; }

        /// <summary>
        /// 附加字段2
        /// </summary>
        public string AppendContent2 { get; set; }


        /// <summary>
        /// 附加字段3
        /// </summary>
        public string AppendContent3 { get; set; }


        /// <summary>
        /// 删除备注
        /// </summary>
        public string DeleteNote { get; set; }

        /// <summary>
        /// 删除人
        /// </summary>
        public string DeleteUser { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime DeleteTime { get; set; }

    }

    /// <summary>
    /// 提示消息
    /// </summary>
    public class InfoMessage : Message
    {
        /// <summary>
        /// 消息类型，0:报警,1:通知消息
        /// </summary>
        public override MessgeType Type => MessgeType.InfoMessage;
    }

    /// <summary>
    /// 报警消息
    /// </summary>
    public class AlarmMessage : Message
    {
        /// <summary>
        /// 消息类型，0:报警,1:通知消息
        /// </summary>
        public override MessgeType Type => MessgeType.Alarm;

        /// <summary>
        /// 报警级别,0:提示信息,1:预警,2:一般,3:重要,4:紧急,5:非常紧急
        /// </summary>
        public AlarmLevel AlarmLevel { get; set; }

        /// <summary>
        /// 报警值
        /// </summary>
        public string AlarmValue { get; set; }

        /// <summary>
        /// 报警时的报警条件
        /// </summary>
        public string AlarmCondition { get; set; }

        /// <summary>
        /// 关联变量
        /// </summary>
        public string LinkTag { get; set; }

        /// <summary>
        /// 恢复时间
        /// </summary>
        public DateTime RestoreTime { get; set; }

        /// <summary>
        /// 恢复值
        /// </summary>
        public string RestoreValue { get; set; }

        /// <summary>
        /// 确认时间
        /// </summary>
        public DateTime AckTime { get; set; }

        /// <summary>
        /// 确认内容,允许确认者备注内容
        /// </summary>
        public string AckMessage { get; set; }

        /// <summary>
        /// 确认人
        /// </summary>
        public string AckUser { get; set; }
    }

    /// <summary>
    /// 处置内容
    /// </summary>
    public class DisposalItem
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// 处置时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
    }


}
