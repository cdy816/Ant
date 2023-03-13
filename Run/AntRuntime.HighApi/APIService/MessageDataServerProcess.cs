//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/5/14 11:00:38.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Cdy.Ant;
using Cdy.Ant.Tag;
using Cheetah;
//using DotNetty.Buffers;
using Microsoft.VisualBasic;

namespace AntRuntime.HighApi
{
    public class MessageDataServerProcess : ServerProcessBase
    {

        #region ... Variables  ...

        /// <summary>
        /// 获取消息
        /// </summary>
        public const byte GetMessage = 0;


        /// <summary>
        /// 确认消息
        /// </summary>
        public const byte AckMessage = 3;

        /// <summary>
        /// 删除消息
        /// </summary>
        public const byte DeleteMessage = 4;


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
        public const byte MessageNotify= 30;


        private Dictionary<string, MessageFilter> mCallBackRegistorIds = new Dictionary<string, MessageFilter>();

        private Dictionary<string, bool> mBlockCallBackRegistorIds = new Dictionary<string, bool>();


        private Thread mScanThread;

        private ManualResetEvent resetEvent;

        private bool mIsClosed = false;
       
        private Dictionary<string, ByteBuffer> buffers = new Dictionary<string, ByteBuffer>();

        private Dictionary<string, int> mDataCounts = new Dictionary<string, int>();

        private IMessageQuery mMessageServce;

        private System.Collections.Generic.Queue<Message> mDataBuffers = new System.Collections.Generic.Queue<Message>();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        public MessageDataServerProcess()
        {
            mMessageServce =ServiceLocator.Locator.Resolve<IMessageQuery>();
            mMessageServce.NewMessage += MMessageServce_NewMessage;
        }



        #endregion ...Constructor...

        #region ... Properties ...

        public override byte FunId => ApiFunConst.MessageDataRequestFun;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MMessageServce_NewMessage(Message msg)
        {
            lock(mDataBuffers)
            {
                mDataBuffers.Enqueue(msg);
                resetEvent.Set();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  void Manager_ValueUpdateEvent(object sender, EventArgs e)
        {
            resetEvent?.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        protected  override void ProcessSingleData(string client, ByteBuffer data)
        {
            if(data.RefCount==0)
            {
                Debug.Print("invailed data buffer in RealDataServerProcess");
                return;
            }
            byte cmd = data.ReadByte();

            if (Parent != null)
            {
                long loginId = data.ReadLong();
                if (ServiceLocator.Locator.Resolve<IRuntimeSecurity>().CheckLogin(loginId))
                {
                    switch(cmd)
                    {
                        case GetMessage:
                            ProcessRequestMessage(client, data);
                            break;
                        case AckMessage:
                            ProcessAckMessage(client, data);
                            break;
                        case DeleteMessage:
                            ProcessDeleteMessage(client, data);
                            break;
                        case RegistorMessageChangedNotify:
                            ProcessRegistor(client, data);
                            break;
                        case UnRegistorMessageChangedNotify:
                            ProcessUnRegistor(client, data);
                            break;
                    }
                }
                else
                {
                    Parent.AsyncCallback(client, FunId, new byte[1], 0);
                }
            }
            base.ProcessSingleData(client, data);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="data"></param>
        private unsafe void ProcessRequestMessage(string clientId, ByteBuffer data)
        {
            int type = data.ReadByte();
            DateTime sTime = new DateTime(data.ReadLong());
            DateTime eTime = new DateTime(data.ReadLong());

            var service = ServiceLocator.Locator.Resolve<IMessageQuery>();
            IEnumerable<Message> msgs = service.Query(sTime, eTime);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal);

            if (type == 0)
            {
                foreach(var vv in msgs)
                {
                    gs.Write(Encoding.UTF8.GetBytes(vv.ToFormateString()+"\r\n"));
                }
            }
            else if(type== 1)
            {
                foreach (var vv in msgs.Where(e=>e is AlarmMessage))
                {
                    gs.Write(Encoding.UTF8.GetBytes(vv.ToFormateString() + "\r\n"));
                }
            }
            else
            {
                foreach (var vv in msgs.Where(e => e is InfoMessage))
                {
                    gs.Write(Encoding.UTF8.GetBytes(vv.ToFormateString() + "\r\n"));
                }
            }
            gs.Flush();

            var re = Parent.Allocate(ApiFunConst.TagInfoRequest, (int)ms.Position + 5);
            re.Write((byte)GetMessage);
            re.Write((int)ms.Position);
            re.Write(ms.GetBuffer(), 0, (int)ms.Position);

            gs.Close();
            ms.Close();

            Parent.AsyncCallback(clientId, re);
        }

        private unsafe void ProcessAckMessage(string clientId, ByteBuffer data)
        {
            long id = data.ReadLong();
            string content =data.ReadString();
            string user = data.ReadString();

            var service = ServiceLocator.Locator.Resolve<IMessageQuery>();
            service.AckMessage(id,content, user);
            var re = Parent.Allocate(ApiFunConst.TagInfoRequest, 3);
            re.Write((byte)AckMessage);
            re.Write((byte)0);

            Parent.AsyncCallback(clientId, re);
        }

        private unsafe void ProcessDeleteMessage(string clientId, ByteBuffer data)
        {
            long id = data.ReadLong();
            string content = data.ReadString();
            string user = data.ReadString();

            var service = ServiceLocator.Locator.Resolve<IMessageQuery>();
            service.DeleteMessage(id, content, user);
            var re = Parent.Allocate(ApiFunConst.TagInfoRequest, 3);
            re.Write((byte)DeleteMessage);
            re.Write((byte)0);

            Parent.AsyncCallback(clientId, re);
        }

        private unsafe void ProcessRegistor(string clientId, ByteBuffer data)
        {
            byte type = data.ReadByte();
            byte alarmlevel =data.ReadByte();
            lock(mCallBackRegistorIds)
            {
                if(!mCallBackRegistorIds.ContainsKey(clientId))
                {
                    mCallBackRegistorIds.Add(clientId,new MessageFilter() { Type= type,AlarmLevel=alarmlevel });
                }
            }

            var re = Parent.Allocate(ApiFunConst.TagInfoRequest, 3);
            re.Write((byte)RegistorMessageChangedNotify);
            re.Write((byte)0);

            Parent.AsyncCallback(clientId, re);
        }


        private unsafe void ProcessUnRegistor(string clientId, ByteBuffer data)
        {
            lock (mCallBackRegistorIds)
            {
                if (mCallBackRegistorIds.ContainsKey(clientId))
                {
                    mCallBackRegistorIds.Remove(clientId);
                }
            }

            var re = Parent.Allocate(ApiFunConst.TagInfoRequest, 3);
            re.Write((byte)UnRegistorMessageChangedNotify);
            re.Write((byte)0);

            Parent.AsyncCallback(clientId, re);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="filer"></param>
        /// <returns></returns>
        private bool IsFit(Message msg,MessageFilter filer)
        {
            if(filer.Type == 2 && msg is AlarmMessage)
            {
                return false;
            }
            else if(filer.Type==1 && msg is InfoMessage)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SendThreadPro()
        {
            while (!mIsClosed)
            {
                resetEvent.WaitOne();
                List<Message> msgs = new List<Message>();
                msgs.AddRange(mDataBuffers);
                mDataBuffers.Clear();

                KeyValuePair<string, MessageFilter>[] clients;
                lock (mCallBackRegistorIds)
                    clients = mCallBackRegistorIds.ToArray();

                foreach (var vv in clients)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal))
                        {
                            foreach (var vvv in msgs)
                            {
                                if (IsFit(vvv, vv.Value))
                                {
                                    gs.Write(Encoding.UTF8.GetBytes(vvv.ToFormateString()+"\r\n"));
                                }
                            }

                            gs.Flush();

                            var re = Parent.Allocate(ApiFunConst.RealDataPushFun, (int)ms.Position + 5);
                            re.Write((int)ms.Position);
                            re.Write(ms.GetBuffer(), 0, (int)ms.Position);
                            Parent.SendDataToClient(vv.Key, re);
                        }
                    }

                }

               
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public override void Start()
        {
           
            base.Start();
            resetEvent = new ManualResetEvent(false);
            mScanThread = new Thread(SendThreadPro);
            mScanThread.IsBackground = true;
            mScanThread.Start();
           
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {
            mIsClosed = true;
            resetEvent.Set();
            resetEvent.Close();
            resetEvent = null;
            base.Stop();
        }

        public override void OnClientDisconnected(string id)
        {
            if(mBlockCallBackRegistorIds.ContainsKey(id))
            {
                mBlockCallBackRegistorIds.Remove(id);
            }
            if(mCallBackRegistorIds.ContainsKey(id))
            {
                //ProcessResetValueChangedNotify(id, null);
                mCallBackRegistorIds.Remove(id);
            }
            base.OnClientDisconnected(id);
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    public struct MessageFilter
    {
        /// <summary>
        /// 0：全部，1：报警，2：消息
        /// </summary>
        public byte Type;
        /// <summary>
        /// 报警级别
        /// </summary>
        public byte AlarmLevel; 
    }
}
