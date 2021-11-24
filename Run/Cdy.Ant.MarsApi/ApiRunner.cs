using Cdy.Tag;
using Cheetah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cdy.Ant.MarsApi
{
    /// <summary>
    /// Mars Tag 简化定义
    /// 用于缓存采集到的值
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    public class ApiRunner : Cdy.Ant.IDataTagService
    {

        #region ... Variables  ...
        
        private Dictionary<string, ApiTag> mTags = new Dictionary<string, ApiTag>();
        private Dictionary<int, ApiTag> mIdMapTags = new Dictionary<int, ApiTag>();

        private List<Action<string, object>> mChangedCallBack = new List<Action<string, object>>();

        private DBRunTime.ServiceApi.ApiClient client;
        private Thread mScanThread;

        private List<string> mNeedInitTags = new List<string>();

        private List<int> mAvaiableIds = new List<int>();

        private List<ApiTag> mChangedApi;

        private bool mIsClosed = false;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 配置数据
        /// </summary>
        public MarsApiData Data { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            mIsClosed = false;

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
            mIsClosed = true;
            client.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ScanThread()
        {
            int lcount = 20;
            while(!mIsClosed)
            {
                if(client.IsConnected)
                {
                    if(!client.IsLogin)
                    {
                        client.Login(Data.UserName, Data.Password);

                        if (client.IsLogin)
                        {
                            LoggerService.Service.Info("Mars Api", "Login " + client.ServerIp + " " + client.Port + " Successful!");
                        }
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        ScanData();
                        NotifyValueChanged();
                        Thread.Sleep(Data.ScanCircle);
                    }
                    if(!client.IsLogin)
                    {
                        lcount++;
                        if (lcount > 20)
                        {
                            lcount = 0;
                            LoggerService.Service.Warn("Mars Api", "Login " + client.ServerIp + " " + client.Port + " failed!");
                        }
                    }
                }
                else
                {
                    if (!client.IsConnected)
                    {
                        lcount++;
                        if (lcount > 20)
                        {
                            lcount = 0;
                            LoggerService.Service.Warn("Mars Api", "Connect " + client.ServerIp + " " + client.Port + " failed!");
                           
                        }
                    }
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ScanData()
        {
            if (mNeedInitTags.Count > 0)
            {
                if (client.IsConnected)
                {
                    List<string> ll;
                    lock (mNeedInitTags)
                        ll = mNeedInitTags.ToList();

                    var ids = client.GetTagIds(mNeedInitTags.ToList());
                    if (ids != null)
                    {
                        lock (mNeedInitTags)
                            mNeedInitTags.Clear();

                        foreach (var vv in ids)
                        {
                            if (vv.Value == -1) continue;
                            mAvaiableIds.Add(vv.Value);

                            var vtag = mTags[vv.Key];
                            vtag.Id = vv.Value;

                            mIdMapTags.Add(vv.Value, vtag);
                        }
                    }
                    mChangedApi = new List<ApiTag>(mIdMapTags.Count);
                }
            }
            else
            {
                var vals = client.GetRealData(mAvaiableIds);
                if (vals != null)
                {
                    ProcessRealDataResult(vals);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        private void NotifyValueChanged(IEnumerable<ApiTag> tags)
        {
            if (mChangedCallBack != null)
            {
                foreach (var vv in tags.Where(e => e.IsValueChanged))
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

        /// <summary>
        /// 
        /// </summary>
        private void NotifyValueChanged()
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datas"></param>
        private void ProcessRealDataResult(Cheetah.ByteBuffer datas)
        {
            mChangedApi.Clear();
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
                    mChangedApi.Add(vtag);
                }
            }
            NotifyValueChanged(mChangedApi);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public object GetTagValue(string tagName)
        {
            if(mTags.ContainsKey(tagName))
            {
                return mTags[tagName].Value;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public int GetTagValueQuality(string tagName)
        {
            if(mTags.ContainsKey(tagName))
            {
                return mTags[tagName].Quality;
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetTagValue(string tagname, object value)
        {
            if(client.IsLogin)
            {
                var vtag = mTags[tagname];

                return client.SetTagValue(vtag.Id, vtag.ValueType,value);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private unsafe HisQueryResult<T> ProcessHisResultByMemory<T>(ByteBuffer data)
        {

            int count = data.ReadInt();
            HisQueryResult<T> re = new HisQueryResult<T>(count);

            data.CopyTo(re.Address, data.ReadIndex, 0, data.WriteIndex - data.ReadIndex);

            //Marshal.Copy(data.Array, data.ArrayOffset + data.ReaderIndex, re.Address, data.ReadableBytes);
            re.Count = count;
            data.UnlockAndReturn();
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Dictionary<DateTime,object> ProcessHisResult(string tagName,ByteBuffer value)
        {
            Dictionary<DateTime, object> re = new Dictionary<DateTime, object>();
            var vtag = mTags[tagName];
            switch (vtag.ValueType)
            {
                case (byte)Cdy.Tag.TagType.Bool:
                    var rr = ProcessHisResultByMemory<bool>(value);
                    for (int i = 0; i < rr.Count; i++)
                    {
                        var val = rr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Byte:
                    var brr = ProcessHisResultByMemory<byte>(value);
                    for (int i = 0; i < brr.Count; i++)
                    {
                        var val = brr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Short:
                    var srr = ProcessHisResultByMemory<short>(value);
                    for (int i = 0; i < srr.Count; i++)
                    {
                        var val = srr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UShort:
                    var usrr = ProcessHisResultByMemory<ushort>(value);
                    for (int i = 0; i < usrr.Count; i++)
                    {
                        var val = usrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Int:
                    var irr = ProcessHisResultByMemory<int>(value);
                    for (int i = 0; i < irr.Count; i++)
                    {
                        var val = irr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UInt:
                    var uirr = ProcessHisResultByMemory<uint>(value);
                    for (int i = 0; i < uirr.Count; i++)
                    {
                        var val = uirr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Long:
                    var lrr = ProcessHisResultByMemory<long>(value);
                    for (int i = 0; i < lrr.Count; i++)
                    {
                        var val = lrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.ULong:
                    var ulrr = ProcessHisResultByMemory<ulong>(value);
                    for (int i = 0; i < ulrr.Count; i++)
                    {
                        var val = ulrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Double:
                    var drr = ProcessHisResultByMemory<double>(value);
                    for (int i = 0; i < drr.Count; i++)
                    {
                        var val = drr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Float:
                    var frr = ProcessHisResultByMemory<float>(value);
                    for (int i = 0; i < frr.Count; i++)
                    {
                        var val = frr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.DateTime:
                    var dtrr = ProcessHisResultByMemory<DateTime>(value);
                    for (int i = 0; i < dtrr.Count; i++)
                    {
                        var val = dtrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.String:
                    var strr = ProcessHisResultByMemory<string>(value);
                    for (int i = 0; i < strr.Count; i++)
                    {
                        var val = strr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.IntPoint:
                    var iptrr = ProcessHisResultByMemory<IntPointData>(value);
                    for (int i = 0; i < iptrr.Count; i++)
                    {
                        var val = iptrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.IntPoint3:
                    var ip3trr = ProcessHisResultByMemory<IntPoint3Data>(value);
                    for (int i = 0; i < ip3trr.Count; i++)
                    {
                        var val = ip3trr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UIntPoint:
                    var uiptrr = ProcessHisResultByMemory<UIntPointData>(value);
                    for (int i = 0; i < uiptrr.Count; i++)
                    {
                        var val = uiptrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UIntPoint3:
                    var uip3trr = ProcessHisResultByMemory<UIntPoint3Data>(value);
                    for (int i = 0; i < uip3trr.Count; i++)
                    {
                        var val = uip3trr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.LongPoint:
                    var liptrr = ProcessHisResultByMemory<LongPointData>(value);
                    for (int i = 0; i < liptrr.Count; i++)
                    {
                        var val = liptrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.LongPoint3:
                    var lip3trr = ProcessHisResultByMemory<LongPoint3Data>(value);
                    for (int i = 0; i < lip3trr.Count; i++)
                    {
                        var val = lip3trr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.ULongPoint:
                    var uliptrr = ProcessHisResultByMemory<ULongPointData>(value);
                    for (int i = 0; i < uliptrr.Count; i++)
                    {
                        var val = uliptrr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;
                case (byte)Cdy.Tag.TagType.ULongPoint3:
                    var ulip3trr = ProcessHisResultByMemory<ULongPoint3Data>(value);
                    for (int i = 0; i < ulip3trr.Count; i++)
                    {
                        var val = ulip3trr.GetValue(i, out DateTime time, out byte qu);
                        if (qu == 0)
                        {
                            re.Add(time, val);
                        }
                    }
                    break;


            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Dictionary<DateTime,Tuple<object,byte>> ProcessHisResult2(string tagName, ByteBuffer value)
        {
            Dictionary<DateTime, Tuple<object, byte>> re = new Dictionary<DateTime, Tuple<object, byte>>();
            var vtag = mTags[tagName];
            switch (vtag.ValueType)
            {
                case (byte)Cdy.Tag.TagType.Bool:
                    var rr = ProcessHisResultByMemory<bool>(value);
                    for (int i = 0; i < rr.Count; i++)
                    {
                        var val = rr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val,qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Byte:
                    var brr = ProcessHisResultByMemory<byte>(value);
                    for (int i = 0; i < brr.Count; i++)
                    {
                        var val = brr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Short:
                    var srr = ProcessHisResultByMemory<short>(value);
                    for (int i = 0; i < srr.Count; i++)
                    {
                        var val = srr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UShort:
                    var usrr = ProcessHisResultByMemory<ushort>(value);
                    for (int i = 0; i < usrr.Count; i++)
                    {
                        var val = usrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Int:
                    var irr = ProcessHisResultByMemory<int>(value);
                    for (int i = 0; i < irr.Count; i++)
                    {
                        var val = irr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UInt:
                    var uirr = ProcessHisResultByMemory<uint>(value);
                    for (int i = 0; i < uirr.Count; i++)
                    {
                        var val = uirr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Long:
                    var lrr = ProcessHisResultByMemory<long>(value);
                    for (int i = 0; i < lrr.Count; i++)
                    {
                        var val = lrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.ULong:
                    var ulrr = ProcessHisResultByMemory<ulong>(value);
                    for (int i = 0; i < ulrr.Count; i++)
                    {
                        var val = ulrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Double:
                    var drr = ProcessHisResultByMemory<double>(value);
                    for (int i = 0; i < drr.Count; i++)
                    {
                        var val = drr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.Float:
                    var frr = ProcessHisResultByMemory<float>(value);
                    for (int i = 0; i < frr.Count; i++)
                    {
                        var val = frr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.DateTime:
                    var dtrr = ProcessHisResultByMemory<DateTime>(value);
                    for (int i = 0; i < dtrr.Count; i++)
                    {
                        var val = dtrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.String:
                    var strr = ProcessHisResultByMemory<string>(value);
                    for (int i = 0; i < strr.Count; i++)
                    {
                        var val = strr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.IntPoint:
                    var iptrr = ProcessHisResultByMemory<IntPointData>(value);
                    for (int i = 0; i < iptrr.Count; i++)
                    {
                        var val = iptrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.IntPoint3:
                    var ip3trr = ProcessHisResultByMemory<IntPoint3Data>(value);
                    for (int i = 0; i < ip3trr.Count; i++)
                    {
                        var val = ip3trr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UIntPoint:
                    var uiptrr = ProcessHisResultByMemory<UIntPointData>(value);
                    for (int i = 0; i < uiptrr.Count; i++)
                    {
                        var val = uiptrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.UIntPoint3:
                    var uip3trr = ProcessHisResultByMemory<UIntPoint3Data>(value);
                    for (int i = 0; i < uip3trr.Count; i++)
                    {
                        var val = uip3trr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.LongPoint:
                    var liptrr = ProcessHisResultByMemory<LongPointData>(value);
                    for (int i = 0; i < liptrr.Count; i++)
                    {
                        var val = liptrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.LongPoint3:
                    var lip3trr = ProcessHisResultByMemory<LongPoint3Data>(value);
                    for (int i = 0; i < lip3trr.Count; i++)
                    {
                        var val = lip3trr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.ULongPoint:
                    var uliptrr = ProcessHisResultByMemory<ULongPointData>(value);
                    for (int i = 0; i < uliptrr.Count; i++)
                    {
                        var val = uliptrr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;
                case (byte)Cdy.Tag.TagType.ULongPoint3:
                    var ulip3trr = ProcessHisResultByMemory<ULongPoint3Data>(value);
                    for (int i = 0; i < ulip3trr.Count; i++)
                    {
                        var val = ulip3trr.GetValue(i, out DateTime time, out byte qu);
                        re.Add(time, new Tuple<object, byte>(val, qu));
                    }
                    break;


            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public Dictionary<DateTime, object> QueryGoodHisValue(string tagName, DateTime startTime, DateTime endTime,TimeSpan span)
        {
            Dictionary<DateTime, object> re = new Dictionary<DateTime, object>();
            if (client.IsLogin)
            {
                var vtag = mTags[tagName];
                if(vtag!=null)
                {
                    var  res = client.QueryHisValueForTimeSpan(vtag.Id, startTime, endTime, span, Cdy.Tag.QueryValueMatchType.Linear);
                    if(res!=null)
                    {
                        return ProcessHisResult(tagName, res);
                    }
                }
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public Dictionary<DateTime, object> QueryAllGoodHisValue(string tagName, DateTime startTime, DateTime endTime)
        {
            Dictionary<DateTime, object> re = new Dictionary<DateTime, object>();
            if (client.IsLogin)
            {
                var vtag = mTags[tagName];
                if (vtag != null)
                {
                    var res = client.QueryAllHisValue(vtag.Id, startTime, endTime);
                    if (res != null)
                    {
                        return ProcessHisResult(tagName, res);
                    }
                }
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public Dictionary<DateTime, object> QueryGoodHisValue(string tagName, List<DateTime> times)
        {
            Dictionary<DateTime, object> re = new Dictionary<DateTime, object>();
            if (client.IsLogin)
            {
                var vtag = mTags[tagName];
                if (vtag != null)
                {
                    var res = client.QueryHisValueAtTimes(vtag.Id, times,QueryValueMatchType.Linear);
                    if (res != null)
                    {
                        return ProcessHisResult(tagName, res);
                    }
                }
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public Dictionary<DateTime, Tuple<object, byte>> QueryHisValue(string tagName, DateTime startTime, DateTime endTime, TimeSpan span)
        {
            Dictionary<DateTime, Tuple<object,byte>> re = new Dictionary<DateTime, Tuple<object, byte>>();
            if (client.IsLogin)
            {
                var vtag = mTags[tagName];
                if (vtag != null)
                {
                    var res = client.QueryAllHisValue(vtag.Id, startTime, endTime);
                    if (res != null)
                    {
                        return ProcessHisResult2(tagName, res);
                    }
                }
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public Dictionary<DateTime, Tuple<object, byte>> QueryAllHisValue(string tagName, DateTime startTime, DateTime endTime)
        {
            Dictionary<DateTime, Tuple<object, byte>> re = new Dictionary<DateTime, Tuple<object, byte>>();
            if (client.IsLogin)
            {
                var vtag = mTags[tagName];
                if (vtag != null)
                {
                    var res = client.QueryAllHisValue(vtag.Id, startTime, endTime);
                    if (res != null)
                    {
                        return ProcessHisResult2(tagName, res);
                    }
                }
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public Dictionary<DateTime, Tuple<object, byte>> QueryHisValue(string tagName, List<DateTime> times)
        {
            Dictionary<DateTime, Tuple<object, byte>> re = new Dictionary<DateTime, Tuple<object, byte>>();
            if (client.IsLogin)
            {
                var vtag = mTags[tagName];
                if (vtag != null)
                {
                    var res = client.QueryHisValueAtTimes(vtag.Id, times, QueryValueMatchType.Linear);
                    if (res != null)
                    {
                        return ProcessHisResult2(tagName, res);
                    }
                }
            }
            return re;
        }


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }
}
