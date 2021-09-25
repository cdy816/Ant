using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public class TimerHelper
    {

        #region ... Variables  ...

        /// <summary>
        /// 
        /// </summary>
        public static TimerHelper Timer = new TimerHelper();

        private SortedDictionary<string, TimerCallBackItem> mCallBack = new SortedDictionary<string, TimerCallBackItem>();

        public int mTick = 1000;

        private System.Timers.Timer mtimer;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            mtimer = new System.Timers.Timer(mTick);
            mtimer.Elapsed += Mtimer_Elapsed;
            mtimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            mtimer.Stop();
            mtimer.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mtimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            mtimer.Elapsed -= Mtimer_Elapsed;
            foreach (var vv in mCallBack.ToArray())
            {
                vv.Value.IncTimer();
                if(vv.Value.Count >= vv.Value.Duration)
                {
                    vv.Value.CallBack(vv.Value.Value);
                }
                lock(mCallBack)
                mCallBack.Remove(vv.Key);
            }
            mtimer.Elapsed += Mtimer_Elapsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="callback"></param>
        public void Registor(string key,double duration,Action<object> callback,object value)
        {
            TimerCallBackItem tt = new TimerCallBackItem();
            int dd = (int)(duration / mTick);
            tt.Duration = dd;
            tt.CallBack = callback;
            tt.Value = value;

            lock (mCallBack)
            {
                if (mCallBack.ContainsKey(key))
                {
                    mCallBack[key] = tt;
                }
                else
                {
                    mCallBack.Add(key, tt);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void UnRegistor(string key)
        {
            lock(mCallBack)
            {
                if(mCallBack.ContainsKey(key))
                {
                    mCallBack.Remove(key);
                }
            }
        }


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    public class TimerCallBackItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Duration { get; set; }
        public object Value { get; set; }
        public Action<object> CallBack { get; set; }

        public int Count { get; set; }


        public void IncTimer()
        {
            Count++;
        }
    }

}
