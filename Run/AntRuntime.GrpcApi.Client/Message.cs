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
        public string Server { get; set; } = "";

        /// <summary>
        /// 生产者变量
        /// </summary>
        public string SourceTag { get; set; } = "";

        /// <summary>
        /// 消息产生时间
        /// </summary>
        public DateTime CreateTime { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public string MessageBody
        { get; set; } = "";

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
        public string AppendContent1 { get; set; } = "";

        /// <summary>
        /// 附加字段2
        /// </summary>
        public string AppendContent2 { get; set; } = "";


        /// <summary>
        /// 附加字段3
        /// </summary>
        public string AppendContent3 { get; set; } = "";


        /// <summary>
        /// 删除备注
        /// </summary>
        public string DeleteNote { get; set; } = "";

        /// <summary>
        /// 删除人
        /// </summary>
        public string DeleteUser { get; set; } = "";

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime DeleteTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder re=new StringBuilder();

            re.Append((int)Type).Append("^").Append(Id).Append("^").Append(Server).Append("^").Append(SourceTag).Append("^").Append(CreateTime.Ticks).Append("^").Append(MessageBody).Append("^");
            re.Append(AppendContent1).Append("^").Append(AppendContent2).Append("^").Append(AppendContent3).Append("^").Append(DeleteNote).Append("^").Append(DeleteUser).Append("^").Append(DeleteTime.Ticks);
            if(DisposalMessages!=null)
            {
                re.Append("^");
                foreach(DisposalItem item in DisposalMessages)
                {
                    re.Append(item.ToString()).Append("|");
                }
                re.Append("");
            }
            else
            {
                re.Append("^");
            }
            return re.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string FormateToString()
        {
            StringBuilder re = new StringBuilder();

            re.Append((int)Type).Append("^").Append(Id).Append("^").Append(Server).Append("^").Append(SourceTag).Append("^").Append(CreateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("^").Append(MessageBody).Append("^");
            re.Append(AppendContent1).Append("^").Append(AppendContent2).Append("^").Append(AppendContent3).Append("^").Append(DeleteNote).Append("^").Append(DeleteUser).Append("^").Append(DeleteTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            if (DisposalMessages != null)
            {
                re.Append("^");
                foreach (DisposalItem item in DisposalMessages)
                {
                    re.Append(item.ToString()).Append("|");
                }
                re.Append("");
            }
            else
            {
                re.Append("^");
            }
            return re.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public virtual Message LoadFromString(string[] val)
        {
            Id = long.Parse(val[1]);
            Server = val[2];
            SourceTag = val[3];
            CreateTime = DateTime.FromBinary(long.Parse(val[4]));
            MessageBody = val[5];
            AppendContent1 = val[6];
            AppendContent2 = val[7];
            AppendContent3 = val[8];
            DeleteNote = val[9];
            DeleteUser = val[10];
            DeleteTime = DateTime.FromBinary(long.Parse(val[11]));
            if(!string.IsNullOrEmpty(val[12]))
            {
                DisposalMessages = new List<DisposalItem>();
                var vss = val[12].Split("|");
                foreach(var vss2 in vss)
                {
                    DisposalMessages.Add(DisposalItem.LoadFromString(vss2));
                }
            }
            return this;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Message LoadFromString(string val)
        {
            string[] sval = val.Split("^");
            if(sval[0]=="0")
            {
                return new AlarmMessage().LoadFromString(sval);
            }
            else
            {
                return new InfoMessage().LoadFromString(sval);
            }
        }

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
    /// 0:提示信息,1:预警,2:一般,3:重要,4:紧急,5:非常紧急
    /// </summary>
    public enum AlarmLevel
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
        public string AlarmValue { get; set; } = "";

        /// <summary>
        /// 报警时的报警条件
        /// </summary>
        public string AlarmCondition { get; set; } = "";

        /// <summary>
        /// 关联变量
        /// </summary>
        public string LinkTag { get; set; } = "";

        /// <summary>
        /// 恢复时间
        /// </summary>
        public DateTime RestoreTime { get; set; }

        /// <summary>
        /// 恢复值
        /// </summary>
        public string RestoreValue { get; set; } = "";

        /// <summary>
        /// 确认时间
        /// </summary>
        public DateTime AckTime { get; set; }

        /// <summary>
        /// 确认内容,允许确认者备注内容
        /// </summary>
        public string AckMessage { get; set; } = "";

        /// <summary>
        /// 确认人
        /// </summary>
        public string AckUser { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var re = base.ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append((int)AlarmLevel).Append("^").Append(AlarmValue).Append("^").Append(AlarmCondition).Append("^").Append(LinkTag).Append("^").Append(RestoreTime.Ticks).Append("^").Append(RestoreValue).Append("^").Append(AckTime.Ticks).Append("^").Append(AckMessage).Append("^").Append(AckUser);
            return re + "^"+sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string FormateToString()
        {
            var re = base.FormateToString();
            StringBuilder sb = new StringBuilder();
            sb.Append((int)AlarmLevel).Append("^").Append(AlarmValue).Append("^").Append(AlarmCondition).Append("^").Append(LinkTag).Append("^").Append(RestoreTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("^").Append(RestoreValue).Append("^").Append(AckTime.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("^").Append(AckMessage).Append("^").Append(AckUser);
            return re + "^" + sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public override Message LoadFromString(string[] val)
        {
            base.LoadFromString(val);
            AlarmLevel = (AlarmLevel)(int.Parse(val[13]));
            AlarmValue =val[14];
            AlarmCondition = val[15];
            LinkTag = val[16];
            RestoreTime = DateTime.FromBinary((long.Parse(val[17])));
            RestoreValue = val[18];
            AckTime = DateTime.FromBinary((long.Parse(val[19])));
            AckMessage = val[20];
            AckUser = val[21];
            return this;
        }

        

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
        public string User { get; set; } = "";
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return MessageId+","+Time.Ticks.ToString()+","+User+""+Message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sval"></param>
        /// <returns></returns>
        public static DisposalItem LoadFromString(string sval)
        {
            DisposalItem re = new DisposalItem();
            var vals = sval.Split(",");
            re.MessageId = Convert.ToInt64(vals[0]);
            re.Time = DateTime.FromBinary(Convert.ToInt64(vals[1]));
            re.User = vals[2];
            re.Message = vals[3];
            return re;
        }

    }


}
