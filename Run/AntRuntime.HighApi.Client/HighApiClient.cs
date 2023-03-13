using Cdy.Ant;
using Cheetah;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace AntRuntime.HighApi
{
    /// <summary>
    /// 
    /// </summary>
    public class HighApiClient : SocketClient2
    {

        #region ... Variables  ...
        public const byte ListTagsFun = 2;

        public const byte ModifyTagFun = 3;

        public const byte QueryTagPropertyFun = 4;

        public const byte LoginFun = 1;

        public const byte HeartFun = 5;

        /// <summary>
        /// 获取消息
        /// </summary>
        public const byte GetMessage = 0;


        /// <summary>
        /// 确认消息
        /// </summary>
        public const byte AckMessageFun = 3;

        /// <summary>
        /// 删除消息
        /// </summary>
        public const byte DeleteMessageFun = 4;


        /// <summary>
        /// 订购消息通知
        /// </summary>
        public const byte RegistorMessageChangedNotify = 20;

        /// <summary>
        /// 取消消息订购
        /// </summary>
        public const byte UnRegistorMessageChangedNotify = 21;


        /// <summary>
        /// 消息通知推送
        /// </summary>
        public const byte MessageNotify = 30;



        /// <summary>
        /// 
        /// </summary>
        public const byte TagInfoRequest = 1;

        /// <summary>
        /// 
        /// </summary>
        public const byte MessageDataRequestFun = 10;

        private object mTagInfoObj = new object();

        private object mMessageObj = new object();

        private ManualResetEvent infoRequreEvent = new ManualResetEvent(false);

        private ByteBuffer mInfoRequreData;


        private ManualResetEvent MessageRequreEvent = new ManualResetEvent(false);

        private ByteBuffer mMessageRequreData;

        private string mUser;
        private string mPass;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public delegate void NewMessageCallBackDelegate(IEnumerable<Message> obj);

        /// <summary>
        /// 
        /// </summary>
        public event NewMessageCallBackDelegate NewMessageCallBack;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 是否已经登录
        /// </summary>
        public bool IsLogin { get { return LoginId > 0; } }

        /// <summary>
        /// 登录ID
        /// </summary>
        public long LoginId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string User
        {
            get
            {
                return mUser;
            }
            set
            {
                if (mUser != value)
                {
                    mUser = value;
                    OnPropertyChanged("User");
                }
            }
        }


        /// <summary>
            /// 
            /// </summary>
        public string Pass
        {
            get
            {
                return mPass;
            }
            set
            {
                if (mPass != value)
                {
                    mPass = value;
                    OnPropertyChanged("Pass");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void ProcessMessageNotify(ByteBuffer data)
        {
            if (NewMessageCallBack == null)
            {
                data.UnlockAndReturn();
                return;
            }

            var datas = data.ReadBytes((int)data.WriteIndex);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(datas))
            {
                using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms,System.IO.Compression.CompressionMode.Decompress,true))
                {
                    using (System.IO.MemoryStream tms = new System.IO.MemoryStream())
                    {
                        gs.CopyTo(tms);
                        var sdata = Encoding.Unicode.GetString(tms.GetBuffer(), 0, (int)tms.Position);
                        if (sdata.Length > 0)
                        {
                            List<Message> msgs= new List<Message>();
                            string[] svals = sdata.Split(@"\r\n");
                            foreach(var vv in svals)
                            {
                                msgs.Add(Cdy.Ant.Message.LoadFromString(vv));
                            }
                            NewMessageCallBack?.Invoke(msgs);
                        }
                    }
                }
            }
            data.UnlockAndReturn();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="datas"></param>
        protected override void ProcessData(byte fun, ByteBuffer datas)
        {
            if (fun == MessageNotify)
            {
                ProcessMessageNotify(datas);
            }
            else
            {
                switch (fun)
                {
                    case TagInfoRequest:
                        mInfoRequreData?.UnlockAndReturn();
                        mInfoRequreData = datas;
                        infoRequreEvent.Set();
                        break;
                    case MessageDataRequestFun:
                        mMessageRequreData?.UnlockAndReturn();
                        mMessageRequreData = datas;
                        this.MessageRequreEvent.Set();
                        break;
                }
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public bool Login(string username, string password, int timeount = 30000)
        {

            lock (mTagInfoObj)
            {
                if (IsLogin) return true;
                try
                {
                    mUser = username;
                    mPass = password;
                    var apphash = Md5Helper.CalSha1();
                    int size = username.Length + password.Length + apphash.Length + 9;
                    var mb = GetBuffer(TagInfoRequest, size);
                    mb.Write(LoginFun);
                    mb.Write(username);
                    mb.Write(password);
                    mb.Write(apphash);

                    infoRequreEvent.Reset();
                    SendData(mb);
                    if (infoRequreEvent.WaitOne(timeount))
                    {
                        try
                        {
                            if (mInfoRequreData != null && (mInfoRequreData.WriteIndex - mInfoRequreData.ReadIndex) > 4)
                            {
                                LoginId = mInfoRequreData.ReadLong();
                                return IsLogin;
                            }
                        }
                        finally
                        {
                            mInfoRequreData?.UnlockAndReturn();
                        }
                    }
                    //mInfoRequreData?.Release();
                    LoginId = -1;
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                return IsLogin;
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public bool Login(int timeout)
        {
            return Login(mUser, mPass, timeout);
        }


        /// <summary>
        /// 心跳
        /// </summary>
        public void Heart()
        {
            lock (mTagInfoObj)
            {
                try
                {
                    var mb = GetBuffer(TagInfoRequest, 1 + 9);
                    mb.Write(HeartFun);
                    mb.Write(LoginId);
                    SendData(mb);
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
            }
        }

        private void PrintErroMessage(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        private void CheckLogin()
        {
            if (LoginId <= 0)
            {
                Login(mUser, mPass);
            }
        }

        #region Tags
        /// <summary>
        /// 枚举所有变量名称
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ListTags(int timeout)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return null;

                try
                {
                    var mb = GetBuffer(TagInfoRequest, 30);
                    mb.Write(ListTagsFun);
                    mb.Write(this.LoginId);

                    infoRequreEvent.Reset();
                    SendData(mb);

                    if (infoRequreEvent.WaitOne(timeout))
                    {
                        var cmd = mInfoRequreData.ReadByte();
                        if (cmd == ListTagsFun)
                        {
                            var sdata = UnZip(mInfoRequreData);
                            return sdata.Split(",");
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mInfoRequreData?.UnlockAndReturn();
                    mInfoRequreData = null;
                }
                return null;
            }
        }

        /// <summary>
        /// 修改变量的属性
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public bool ModifyTags(string tag,Dictionary<string,string> props,int timeout)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return false;

                try
                {

                    StringBuilder sb = new StringBuilder();
                    foreach(var vv in props)
                    {
                        sb.Append(vv.Key + "," + vv.Value + ";");
                    }
                    sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;

                    var mb = GetBuffer(TagInfoRequest, 30+tag.Length+sb.Length);
                    mb.Write(ModifyTagFun);
                    mb.Write(this.LoginId);
                    mb.Write(tag);
                    mb.Write(sb.ToString());

                    infoRequreEvent.Reset();
                    SendData(mb);

                    if (infoRequreEvent.WaitOne(timeout))
                    {
                        return mInfoRequreData.ReadIndex > 0;
                        
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mInfoRequreData?.UnlockAndReturn();
                    mInfoRequreData = null;
                }
            }
            return false;
        }


        /// <summary>
        /// 查询变量的属性值
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,string> QueryTagProps(string tag, int timeout)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return null;

                try
                {
                    var mb = GetBuffer(TagInfoRequest, 30+tag.Length);
                    mb.Write(QueryTagPropertyFun);
                    mb.Write(this.LoginId);
                    mb.Write(tag);

                    infoRequreEvent.Reset();
                    SendData(mb);

                    if (infoRequreEvent.WaitOne(timeout))
                    {
                        var cmd = mInfoRequreData.ReadByte();
                        if (cmd == QueryTagPropertyFun)
                        {
                            var sdata = UnZip(mInfoRequreData);
                            Dictionary<string, string> re = new Dictionary<string, string>();
                            if (string.IsNullOrEmpty(sdata))
                            {
                                var vl = sdata.Split(";");
                                foreach (var vv in vl)
                                {
                                    var ss = vv.Split(",");
                                    if (ss.Length > 1)
                                        re.Add(ss[0], ss[1]);
                                }
                            }
                            return re;
                        }
                       
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mInfoRequreData?.UnlockAndReturn();
                    mInfoRequreData = null;
                }
                return null;
            }
        }

        #endregion


        #region Message
        /// <summary>
        /// 查询消息
        /// </summary>
        /// <param name="type">0:所有类型，1:报警，2:消息</param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IEnumerable<Message> QueryMessage(byte type, DateTime starttime, DateTime endtime, int timeout)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return null;

                try
                {
                    var mb = GetBuffer(MessageDataRequestFun, 30);
                    mb.Write(GetMessage);
                    mb.Write(this.LoginId);
                    mb.Write(type);
                    mb.Write(starttime);
                    mb.Write(endtime);

                    MessageRequreEvent.Reset();
                    SendData(mb);

                    if (MessageRequreEvent.WaitOne(timeout))
                    {
                        var cmd = mMessageRequreData.ReadByte();
                        if (cmd == GetMessage)
                        {
                            var sdata = UnZip(mMessageRequreData);
                            List<Message> msgs = new List<Message>();
                            string[] svals = sdata.Split(@"\r\n");
                            foreach (var vv in svals)
                            {
                                msgs.Add(Cdy.Ant.Message.LoadFromString(vv));
                            }
                            return msgs;
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mMessageRequreData?.UnlockAndReturn();
                    mMessageRequreData = null;
                }
                return null;
            }
        }

        /// <summary>
        /// 确认消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool AckMessage(long id, string msg, string user, int timeout)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return false;

                try
                {
                    var mb = GetBuffer(MessageDataRequestFun, 24 + msg.Length + user.Length);
                    mb.Write(AckMessageFun);
                    mb.Write(this.LoginId);
                    mb.Write(id);
                    mb.Write(msg);
                    mb.Write(user);

                    MessageRequreEvent.Reset();
                    SendData(mb);

                    if (MessageRequreEvent.WaitOne(timeout))
                    {
                        return mMessageRequreData.ReadableCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mMessageRequreData?.UnlockAndReturn();
                    mMessageRequreData = null;
                }
                return false;
            }
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool DeleteMessage(long id, string msg, string user, int timeout)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return false;

                try
                {
                    var mb = GetBuffer(MessageDataRequestFun, 24 + msg.Length + user.Length);
                    mb.Write(DeleteMessageFun);
                    mb.Write(this.LoginId);
                    mb.Write(id);
                    mb.Write(msg);
                    mb.Write(user);

                    MessageRequreEvent.Reset();
                    SendData(mb);

                    if (MessageRequreEvent.WaitOne(timeout))
                    {
                        return mMessageRequreData.ReadableCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mMessageRequreData?.UnlockAndReturn();
                    mMessageRequreData = null;
                }
                return false;
            }
        }

        /// <summary>
        /// 订购消息主动推送
        /// </summary>
        /// <param name="type">报警类型，0:所有，1：报警，2：消息</param>
        /// <param name="level">报警级别</param>
        /// <returns></returns>
        public bool RegistorMessageNotify(byte type, int timeout, byte level = 0)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return false;

                try
                {
                    var mb = GetBuffer(MessageDataRequestFun, 24);
                    mb.Write(RegistorMessageChangedNotify);
                    mb.Write(this.LoginId);
                    mb.Write(type);
                    mb.Write(level);

                    MessageRequreEvent.Reset();
                    SendData(mb);

                    if (MessageRequreEvent.WaitOne(timeout))
                    {
                        return mMessageRequreData.ReadableCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mMessageRequreData?.UnlockAndReturn();
                    mMessageRequreData = null;
                }
                return false;
            }
        }

        /// <summary>
        /// 取消订购
        /// </summary>
        /// <returns></returns>
        public bool UnRegistorMessageNotify(int timeout)
        {
            lock (mMessageObj)
            {
                CheckLogin();
                if (!IsLogin) return false;

                try
                {
                    var mb = GetBuffer(MessageDataRequestFun, 24);
                    mb.Write(UnRegistorMessageChangedNotify);
                    mb.Write(this.LoginId);

                    MessageRequreEvent.Reset();
                    SendData(mb);

                    if (MessageRequreEvent.WaitOne(timeout))
                    {
                        return mMessageRequreData.ReadableCount > 0;
                    }
                }
                catch (Exception ex)
                {
                    PrintErroMessage(ex);
                }
                finally
                {
                    mMessageRequreData?.UnlockAndReturn();
                    mMessageRequreData = null;
                }
                return false;
            }
        }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string UnZip(ByteBuffer data)
        {
            try
            {
                int len = data.ReadInt();
                var datas = data.ReadBytes(len);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(datas))
                {
                    using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress, true))
                    {
                        using (System.IO.MemoryStream tms = new System.IO.MemoryStream())
                        {
                            gs.CopyTo(tms);
                            var sdata = Encoding.Unicode.GetString(tms.GetBuffer(), 0, (int)tms.Position);
                            return sdata;
                        }
                    }
                }
            }
            catch
            {

            }
            return string.Empty;
        }

       

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
