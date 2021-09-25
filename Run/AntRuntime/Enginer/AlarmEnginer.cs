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

        private Dictionary<string,List<TagRunBase>> mRunTags = new Dictionary<string, List<TagRunBase>>();
        private IDataTagService mDataTagService;

        private Thread mScanThread;
        bool mIsClosed = false;
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
                    Init();
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
            LoadTag();
            mDataTagService = ServiceLocator.Locator.Resolve<IDataTagApi>().TagService;
            if(mDataTagService!=null)
            {
                mDataTagService.RegistorTagChangeCallBack(DataTagValueChanged);
                mDataTagService.RegistorMonitorTag(mRunTags.Keys);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        private void DataTagValueChanged(string tagname, object value)
        {
            lock (mRunTags)
                if (mRunTags.ContainsKey(tagname))
                {

                    mRunTags[tagname].ForEach(e => e.NeedCal = true);
                }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadTag()
        {
            mRunTags.Clear();
            foreach(var vv in mDatabase.Tags.Values)
            {
                switch(vv.Type)
                {
                    case TagType.AnalogAlarm:
                        string[] skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach(var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            AnalogAlarmTagRun at = new AnalogAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv,TagName = vvv,Id=vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.AnalogRangeAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            AnalogRangeAlarmTagRun at = new AnalogRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.DelayDigitalAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            DelayDigitalAlarmTagRun at = new DelayDigitalAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.DigitalAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            DigitalAlarmTagRun at = new DigitalAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.OneRange:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            OneRangeAlarmTagRun at = new OneRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.TwoRange:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            TwoRangeAlarmTagRun at = new TwoRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.StringAlarm:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            StringAlarmTagRun at = new StringAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.ThreeRange:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            ThreeRangeAlarmTagRun at = new ThreeRangeAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.Pulse:
                        skeys = (vv as AlarmTag).LinkTag.Split(new char[] { ',' });
                        foreach (var vvv in skeys)
                        {
                            string skey = vvv + vv.Id;
                            PulseAlarmTagRun at = new PulseAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv, TagName = vvv, Id = vv.Id };
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(at);
                            }
                            else
                            {
                                mRunTags.Add(skey, new List<TagRunBase>() { at });
                            }
                        }
                        break;
                    case TagType.Script:
                         var  skey1 = vv.Id.ToString();
                        ScriptAlarmTagRun stag = new ScriptAlarmTagRun() { Source = mDatabase.Name, LinkedTag = vv,Id=vv.Id };
                        foreach(var vvv in stag.ListLinkTag())
                        {
                            if (mRunTags.ContainsKey(vvv))
                            {
                                mRunTags[vvv].Add(stag);
                            }
                            else
                            {
                                mRunTags.Add(vvv, new List<TagRunBase>() { stag });
                            }
                        }
                        break;
                }
            }
        }

        private void ThreadScan()
        {
            while(!mIsClosed)
            {
                foreach(var vv in mRunTags.Values)
                {
                    foreach(var vvv in vv)
                    {
                        if(vvv.NeedCal)
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
        /// 
        /// </summary>
        public void Start()
        {
            mScanThread = new Thread(ThreadScan);
            mScanThread.IsBackground = true;
            mScanThread.Start();
        }

        /// <summary>
        /// 
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
