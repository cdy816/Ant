using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// ID编号
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MessageBody
        { get; set; }


        /// <summary>
        /// 消息处置内容
        /// </summary>
        public List<DisposalItem> DisposalMessages { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class AlarmMessage : Message
    {

        /// <summary>
        /// 报警级别
        /// </summary>
        public AlarmLevel AlarmLevel { get; set; }

        /// <summary>
        /// 关联变量
        /// </summary>
        public string LinkTag { get; set; }

        /// <summary>
        /// 恢复时间
        /// </summary>
        public DateTime RestoreTime { get; set; }

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
        /// 处置时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }


}
