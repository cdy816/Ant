using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Message
{
    /// <summary>
    /// 消息文件存储格式
    /// FileHead(64)+Block pointer(8)*24+[Message block]
    /// FileHead(文件头):datetime(8)+databasename(32)+other(24)
    /// Block pointer(Block 指针)(8)
    /// [Message block](消息数据块集合)
    /// </summary>
    public class MessageFileSerise
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 消息块存储格式
    /// blockheader(16)+alarmMessageBody+messageBody
    /// blockheader(块文件头):Datasize(8)(数据块大小)+messageBodyOffset(8)(一般报警消息偏移量)
    /// alarmMessageBody(报警消息数据)
    /// messageBody(一般消息数据)
    /// </summary>
    public class MessageBlockBuffer
    {
        /// <summary>
        /// 
        /// </summary>
        public AlarmMessageAreaBuffer AlarmArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CommonMessageAreaBuffer CommonArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="seek"></param>
        public void Load(System.IO.Stream stream)
        {
            
        }
    }

    /// <summary>
    /// 报警消息存储区域
    /// area header(8+8+8)+message data
    /// area header(24): count(消息个数)+restoreDataPoint(8)+ackDataPoint(8)
    /// message data(消息数据):[messageid(8)]+AlarmMessageBody(zip)+[RestoreTime(8)+RestoreValue(64)]+[AckTime(8)+AckMessage(64)+AckUser(32)]
    /// AlarmMessageBody(报警消息体,压缩内容):[source(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)+AlarmLevel(1)+AlarmValue(64)+AlarmCondition(64)+LinkTag(64)]
    /// </summary>
    public class AlarmMessageAreaBuffer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="seek"></param>
        public void Load(System.IO.Stream stream)
        {
            
        }
    }

    /// <summary>
    /// 一般消息存储区域
    /// area header+message data(zip)
    /// area header(8):count(消息个数)
    /// message data:messageid(8)+source(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)
    /// </summary>
    public class CommonMessageAreaBuffer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="seek"></param>
        public void Load(System.IO.Stream stream)
        {

        }
    }
}
