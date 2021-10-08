using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cdy.Ant.MarsApi
{

    public class ApiTag
    {
        private object mValue;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        public bool IsValueChanged { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte ValueType { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public object Value { get { return mValue; } set { if (mValue != value) { mValue = value; IsValueChanged = true; } } }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 质量戳
        /// </summary>
        public byte Quality { get; set; }
    }

    public class ApiRunner : Cdy.Ant.IDataTagService
    {

        #region ... Variables  ...
        
        private Dictionary<string, ApiTag> mTags = new Dictionary<string, ApiTag>();
        private Dictionary<int, ApiTag> mIdMapTags = new Dictionary<int, ApiTag>();


        //private Dictionary<string, object> mValues = new Dictionary<string, object>();
        
        //private Dictionary<string, int> mNameIdMaps = new Dictionary<string, int>();
        //private Dictionary<int, string> mIdNameMaps = new Dictionary<int, string>();

        private List<Action<string, object>> mChangedCallBack = new List<Action<string, object>>();

        private DBRunTime.ServiceApi.ApiClient client;
        private Thread mScanThread;

        private List<string> mNeedInitTags = new List<string>();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public MarsApiData Data { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            client = new DBRunTime.ServiceApi.ApiClient();
            client.ServerIp = Data.ServerIp;
            client.Port = Data.Port;
            client.Open();
            mScanThread = new Thread(ScanThread);
            mScanThread.IsBackground = true;
            mScanThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            client.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ScanThread()
        {
            while(true)
            {
                if(client.IsConnected)
                {
                    if(!client.IsLogin)
                    {
                        client.Login(Data.UserName, Data.Password);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        ScanData();
                        NotifyValueChanged();
                        Thread.Sleep(Data.ScanCircle);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ScanData()
        {
            if(mNeedInitTags.Count>0)
            {
               if(client.IsConnected)
                {
                    List<string> ll;
                    lock(mNeedInitTags)
                    ll = mNeedInitTags.ToList();

                    var ids = client.GetTagIds(mNeedInitTags.ToList());
                    if(ids!=null)
                    {
                        lock(mNeedInitTags)
                        mNeedInitTags.Clear();

                        foreach(var vv in ids)
                        {
                            var vtag = mTags[vv.Key];
                            vtag.Id = vv.Value;

                            mIdMapTags.Add(vv.Value, vtag);                           
                        }
                    }
                }
            }
            else
            {
                var vals = client.GetRealData(mIdMapTags.Keys.ToList());
                if(vals!=null)
                {
                    ProcessRealDataResult(vals);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void NotifyValueChanged()
        {
            //Dictionary<string, object> mchangedvals = new Dictionary<string, object>();
            
            if(mChangedCallBack!=null)
            {
                foreach (var vv in mTags.Values.Where(e => e.IsValueChanged))
                {
                    //mchangedvals.Add(vv.Name, vv.Value);
                    foreach (var vvc in mChangedCallBack)
                    {
                        vvc(vv.Name, vv.Value);
                    }
                    vv.IsValueChanged = false;
                }
            }
        }

        private void ProcessRealDataResult(Cheetah.ByteBuffer datas)
        {
            int count = datas.ReadInt();
            for(int i=0;i<count;i++)
            {
                var id = datas.ReadInt();
                var typ = datas.ReadByte();
                object re=null;
                switch (typ)
                {
                    case (byte)Cdy.Tag.TagType.Bool:
                        re = datas.ReadByte();
                        break;
                    case (byte)Cdy.Tag.TagType.Byte:
                        re = datas.ReadByte();
                        break;
                    case (byte)Cdy.Tag.TagType.Short:
                        re = datas.ReadShort();
                        break;
                    case (byte)Cdy.Tag.TagType.UShort:
                        re = datas.ReadUShort();
                        break;
                    case (byte)Cdy.Tag.TagType.Int:
                        re = datas.ReadInt();
                        break;
                    case (byte)Cdy.Tag.TagType.UInt:
                        re = datas.ReadUInt();
                        break;
                    case (byte)Cdy.Tag.TagType.Long:
                    case (byte)Cdy.Tag.TagType.ULong:
                        re = datas.ReadLong();
                        break;
                    case (byte)Cdy.Tag.TagType.Float:
                        re = datas.ReadFloat();
                        break;
                    case (byte)Cdy.Tag.TagType.Double:
                        re = datas.ReadDouble();
                        break;
                    case (byte)Cdy.Tag.TagType.String:
                        re = datas.ReadString();
                        break;
                    case (byte)Cdy.Tag.TagType.DateTime:
                        re = DateTime.FromBinary(datas.ReadLong());
                        break;
                    case (byte)Cdy.Tag.TagType.IntPoint:
                        var i1 = datas.ReadInt();
                        var i2 = datas.ReadInt();
                        re = new Point() { X = i1, Y = i2 };
                        
                        break;
                    case (byte)Cdy.Tag.TagType.UIntPoint:
                        var ui1 = datas.ReadUInt();
                        var ui2 = datas.ReadUInt();
                        re = new Point() { X = ui1, Y = ui2 };
                        break;
                    case (byte)Cdy.Tag.TagType.IntPoint3:
                        i1 = datas.ReadInt();
                        i2 = datas.ReadInt();
                        var i3 = datas.ReadInt();
                        re = new Point3D() { X = i1, Y = i2,Z=i3 };
                        break;
                    case (byte)Cdy.Tag.TagType.UIntPoint3:
                        ui1 = datas.ReadUInt();
                        ui2 = datas.ReadUInt();
                       var ui3 = datas.ReadUInt();
                        re = new Point3D() { X = ui1, Y = ui2 ,Z=ui3};
                        break;
                    case (byte)Cdy.Tag.TagType.ULongPoint:
                    case (byte)Cdy.Tag.TagType.LongPoint:
                        var li1 = datas.ReadLong();
                        var li2 = datas.ReadLong();
                        re = new Point() { X = li1, Y = li2 };
                        break;
                    case (byte)Cdy.Tag.TagType.ULongPoint3:
                        li1 = datas.ReadLong();
                        li2 = datas.ReadLong();
                        var li3 = datas.ReadLong();
                        re = new Point3D() { X = (ulong)li1, Y = (ulong)li2, Z = (ulong)li3 };
                        break;
                    case (byte)Cdy.Tag.TagType.LongPoint3:
                        li1 = datas.ReadLong();
                        li2 = datas.ReadLong();
                        li3 = datas.ReadLong();
                        re = new Point3D() { X = li1, Y = li2, Z = li3 };
                        break;
                }
                var time = DateTime.FromBinary(datas.ReadLong());
                var qu = datas.ReadByte();

                if(mIdMapTags.ContainsKey(id))
                {
                    var vtag = mIdMapTags[id];
                    vtag.Value = re;
                    vtag.Time = time;
                    vtag.Quality = qu;
                    vtag.ValueType = typ;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public void RegistorMonitorTag(IEnumerable<string> tag)
        {
            foreach (var vv in tag)
            {
                lock (mTags)
                {
                    if (!mTags.ContainsKey(vv))
                    {
                        mTags.Add(vv, new ApiTag() { Name = vv });
                    }
                    lock(mNeedInitTags)
                    {
                        mNeedInitTags.Add(vv);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public void RegistorTagChangeCallBack(Action<string, object> callback)
        {
            mChangedCallBack.Add(callback);
        }


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }
}
