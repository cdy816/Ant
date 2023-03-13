using Cdy.Ant;
using Cdy.Ant.Tag;
using Grpc.Core;

namespace AntRuntime.GrpcApi
{
    public class MessageHelper
    {

        #region ... Variables  ...
        /// <summary>
        /// 
        /// </summary>
        public static MessageHelper Helper = new MessageHelper();

        private IMessageQuery mMessageServce;

        private Queue<Cdy.Ant.Tag.Message> mCachMessage = new Queue<Cdy.Ant.Tag.Message>();

        private Dictionary<string, IServerStreamWriter<GetMessageResponse>> mWriters = new Dictionary<string, IServerStreamWriter<GetMessageResponse>>();

        private ManualResetEvent mWaitevent=new ManualResetEvent(false);

        private bool mIsClosed = false;

        private Thread mProThread;

        private Dictionary<string,Action> mCancelAction= new Dictionary<string, Action>();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...



        /// <summary>
        /// 
        /// </summary>
        public MessageHelper()
        {
            mMessageServce = ServiceLocator.Locator.Resolve<IMessageQuery>();
            mMessageServce.NewMessage += MMessageServce_NewMessage1;
        }

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            mProThread = new Thread(MessageSendPro);
            mProThread.IsBackground = true;
            mProThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            mIsClosed = false;
            mWaitevent.Set();
        }

        private void MessageSendPro()
        {
            List<Cdy.Ant.Tag.Message> msgs=new List<Cdy.Ant.Tag.Message>();
            while (!mIsClosed)
            {
                mWaitevent.WaitOne();
                while(mCachMessage.Count > 0 )
                {
                    
                    lock(mCachMessage)
                    {
                        msgs.Add(mCachMessage.Dequeue());
                    }
                    Send(msgs);
                }
            }
        }

        private async void Send(IEnumerable<Cdy.Ant.Tag.Message> msg)
        {
            var res = new GetMessageResponse();
            res.Messages.AddRange(msg.Select(e => e.ToString()));
            List<KeyValuePair<string, IServerStreamWriter<GetMessageResponse>>> mmm = null;
            lock (mWriters)
            {
                mmm = mWriters.ToList();
            }
            foreach (var vv in mmm)
            {
                try
                {
                    await vv.Value.WriteAsync(res);
                }
                catch
                {
                    if (mCancelAction.ContainsKey(vv.Key))
                    {
                        mCancelAction[vv.Key]();
                        mCancelAction.Remove(vv.Key);

                        if (mWriters.ContainsKey(vv.Key))
                            mWriters.Remove(vv.Key);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void MMessageServce_NewMessage1(Cdy.Ant.Tag.Message msg)
        {
            lock(mCachMessage)
            {
                if(mWriters.Count> 0)
                {
                    mCachMessage.Enqueue(msg);
                    mWaitevent.Set();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void RegistorMessageSend(string key, IServerStreamWriter<GetMessageResponse> writer,Action action)
        {
            lock(mWriters)
            {
                if(mCancelAction.ContainsKey(key))
                {
                    mCancelAction[key]();
                    mCancelAction[key] = action;
                }
                else
                {
                    mCancelAction.Add(key, action);
                }

                if (mWriters.ContainsKey(key))
                {
                    mWriters[key] = writer;
                }
                else
                {
                    mWriters.Add(key, writer);
                }
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
