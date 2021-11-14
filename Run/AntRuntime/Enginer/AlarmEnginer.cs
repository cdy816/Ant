using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public class AlarmEnginer
    {

        #region ... Variables  ...

        public static AlarmEnginer Enginer = new AlarmEnginer();

        private AlarmDatabase mDatabase;

        //private Dictionary<string,List<TagRunBase>> mRunTags = new Dictionary<string, List<TagRunBase>>();
        private Dictionary<string, TagRunBase> mRunTags = new Dictionary<string, TagRunBase>();

        private IDataTagService mDataTagService;

        //private Thread mScanThread;
        //bool mIsClosed = false;

        private List<AlarmEnginerExecuter> mExecuter = new List<AlarmEnginerExecuter>();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public AlarmDatabase Database
        {
            get
            {
                return mDatabase;
            }
            set
            {
                if (mDatabase != value)
                {
                    mDatabase = value;
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            for(int i= 0;i<4;i++)
            {
                mExecuter.Add(new AlarmEnginerExecuter());
            }
            LoadTag();
            mDataTagService = ServiceLocator.Locator.Resolve<IDataTagApi>().TagService;
            if(mDataTagService!=null)
            {
                mDataTagService.RegistorTagChangeCallBack(DataTagValueChanged);
                foreach(var vv in mExecuter)
                mDataTagService.RegistorMonitorTag(vv.ListLinkTags());
            }

            foreach(var vv in mRunTags)
            {
                vv.Value.Init();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        private void DataTagValueChanged(string tagname, object value)
        {
            foreach(var vv in mExecuter)
            {
                vv.NotifyTagToExecute(tagname);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadTag()
        {
            //mRunTags.Clear();
            int i = 0;
            AlarmEnginerExecuter exe;
            int num = mExecuter.Count;
            foreach(var vv in mDatabase.Tags.Values)
            {
                exe = mExecuter[i];
                i = i >= num ? 0 : i;

                switch(vv.Type)
                {
                    case TagType.AnalogAlarm:
                        string[] skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach(var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            AnalogAlarmTagRun at = new AnalogAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv,TagName = vvv,Id=vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.AnalogRangeAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            AnalogRangeAlarmTagRun at = new AnalogRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.DelayDigitalAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            DelayDigitalAlarmTagRun at = new DelayDigitalAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.DigitalAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            DigitalAlarmTagRun at = new DigitalAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.OneRange:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            OneRangeAlarmTagRun at = new OneRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.TwoRange:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            TwoRangeAlarmTagRun at = new TwoRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.StringAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            StringAlarmTagRun at = new StringAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.ThreeRange:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            ThreeRangeAlarmTagRun at = new ThreeRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.Pulse:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            PulseAlarmTagRun at = new PulseAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(at);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { at });
                            }
                            mRunTags.Add(skey, at);
                        }
                        break;
                    case TagType.Script:
                         var  skey1 = vv.Id.ToString();
                        ScriptAlarmTagRun stag = new ScriptAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv,Id=vv.Id };
                        bool ishas = false;
                        foreach(var vvv in stag.ListLinkTag())
                        {
                            string skey = vvv + vv.Id;
                            if (exe.RunTags.ContainsKey(vvv))
                            {
                                exe.RunTags[vvv].Add(stag);
                            }
                            else
                            {
                                exe.RunTags.Add(vvv, new List<TagRunBase>() { stag });
                            }
                            ishas = true;
                        }
                        if(!ishas)
                        {
                            string skey = "notags";
                            if (exe.RunTags.ContainsKey(skey))
                            {
                                exe.RunTags[skey].Add(stag);
                            }
                            else
                            {
                                exe.RunTags.Add(skey, new List<TagRunBase>() { stag });
                            }
                        }
                        mRunTags.Add(skey1, stag);
                        break;
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //private void ThreadScan()
        //{
        //    var vpp = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
        //    while (!mIsClosed)
        //    {
        //        Parallel.ForEach(mRunTags.Values, vpp, (vv) =>
        //        {
        //            foreach (var vvv in vv)
        //            {
        //                if (vvv.NeedCal)
        //                {
        //                    vvv.NeedCal = false;
        //                    vvv.LinkExecute();
        //                }
        //            }
        //        });
        //        //foreach (var vv in mRunTags.Values)
        //        //{
        //        //    foreach (var vvv in vv)
        //        //    {
        //        //        if (vvv.NeedCal)
        //        //        {
        //        //            vvv.NeedCal = false;
        //        //            vvv.LinkExecute();
        //        //        }
        //        //    }
        //        //}
        //        Thread.Sleep(100);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
           foreach(var vv in mExecuter)
            {
                vv.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            foreach (var vv in mExecuter)
            {
                vv.Start();
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 一组报警变量执行者
    /// </summary>
    public class AlarmEnginerExecuter
    {


        #region ... Variables  ...
        private Thread mScanThread;
        bool mIsClosed = false;
        private Dictionary<string, List<TagRunBase>> mRunTags = new Dictionary<string, List<TagRunBase>>();
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, List<TagRunBase>> RunTags
        {
            get
            {
                return mRunTags;
            }
        }
        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 枚举出关联的数据库变量
        /// </summary>
        /// <returns></returns>
        public List<string> ListLinkTags()
        {
            List<string> re = new List<string>();
            foreach(var vv in this.mRunTags)
            {
                foreach(var vvv in vv.Value)
                {
                    re.AddRange(vvv.ListLinkTag());
                }
            }
            re.Distinct();
            return re;
        }

        /// <summary>
        /// 通知和数据库管理的报警变量可以执行报警逻辑
        /// </summary>
        /// <param name="tag">数据库变量</param>
        public void NotifyTagToExecute(string tagname)
        {
            lock (mRunTags)
                if (mRunTags.ContainsKey(tagname))
                {

                    mRunTags[tagname].ForEach(e => e.NeedCal = true);
                }
        }

        /// <summary>
        /// 扫描线程
        /// </summary>
        private void ThreadScan()
        {
            var vpp = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
            while (!mIsClosed)
            {
                foreach (var vv in mRunTags.Values)
                {
                    foreach (var vvv in vv)
                    {
                        if (vvv.NeedCal)
                        {
                            vvv.NeedCal = false;
                            vvv.LinkExecute();
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            mScanThread = new Thread(ThreadScan);
            mScanThread.IsBackground = true;
            mScanThread.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            mIsClosed = true;
            mScanThread = null;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
