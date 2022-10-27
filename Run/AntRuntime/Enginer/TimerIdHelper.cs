using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    public static class TimerIdHelper
    {
        public static DateTime ConstTime = new DateTime(2000, 1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTicks(DateTime time)
        {
            return (time - ConstTime).Ticks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DateTime TicksToTimer(long id)
        {
            return DateTime.FromBinary((id + ConstTime.Ticks));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetID(DateTime time)
        {
            return GetTicks(time)*10;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DateTime IDToTimer(long id)
        {
            return DateTime.FromBinary((id/10 + ConstTime.Ticks));
        }
    }
}
