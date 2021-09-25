using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    public class TwoRangeAlarmTagRun : AlarmTagRunBase
    {

        #region ... Variables  ...
        TwoRangeAlarmTag mTag;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag =>  TagType.OneRange;

        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mTag = value as TwoRangeAlarmTag; } }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public override void CheckTagValueAlarm()
        {
            string[] sval = Value.ToString().Split(new char[] { ',' });
            if (sval.Length <= 1) return;

            double dval1 = Convert.ToDouble(sval[0]);
            double dval2 = Convert.ToDouble(sval[1]);
            if(IsInRange(dval1,dval2))
            {
                if (mTag.IsInRangeAlarm)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mTag.AlarmLevel, mTag.Desc, Value.ToString());
                    }
                }
                else
                {
                    if(CurrentStatue != AlarmStatue.None)
                    {
                        Restore(Value.ToString());
                    }
                }
            }
            else
            {
                if (!mTag.IsInRangeAlarm)
                {
                    if (CurrentStatue == AlarmStatue.None)
                    {
                        Alarm(Source, mTag.AlarmLevel, mTag.Desc, Value.ToString());
                    }
                }
                else
                {
                    if (CurrentStatue != AlarmStatue.None)
                    {
                        Restore(Value.ToString());
                    }
                }
            }
            base.CheckTagValueAlarm();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dval"></param>
        /// <returns></returns>
        private bool IsInRange(double dval1,double dval2)
        {
            //mTag.AlarmDatas;
            return IsInPolygon(new Point(dval1,dval2),mTag.AlarmDatas);
        }

        /// <summary>
        /// 判断点是否在多边形内.
        /// ----------原理----------
        /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，
        /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。
        /// </summary>
        /// <param name="checkPoint">要判断的点</param>
        /// <param name="polygonPoints">多边形的顶点</param>
        /// <returns></returns>
        public static bool IsInPolygon(Point checkPoint, List<Point> polygonPoints)
        {
            bool inside = false;
            int pointCount = polygonPoints.Count;
            Point p1, p2;
            for (int i = 0, j = pointCount - 1; i < pointCount; j = i, i++)//第一个点和最后一个点作为第一条线，之后是第一个点和第二个点作为第二条线，之后是第二个点与第三个点，第三个点与第四个点...
            {
                p1 = polygonPoints[i];
                p2 = polygonPoints[j];
                if (checkPoint.Y < p2.Y)
                {//p2在射线之上
                    if (p1.Y <= checkPoint.Y)
                    {//p1正好在射线中或者射线下方
                        if ((checkPoint.Y - p1.Y) * (p2.X - p1.X) > (checkPoint.X - p1.X) * (p2.Y - p1.Y))//斜率判断,在P1和P2之间且在P1P2右侧
                        {
                            //射线与多边形交点为奇数时则在多边形之内，若为偶数个交点时则在多边形之外。
                            //由于inside初始值为false，即交点数为零。所以当有第一个交点时，则必为奇数，则在内部，此时为inside=(!inside)
                            //所以当有第二个交点时，则必为偶数，则在外部，此时为inside=(!inside)
                            inside = (!inside);
                        }
                    }
                }
                else if (checkPoint.Y < p1.Y)
                {
                    //p2正好在射线中或者在射线下方，p1在射线上
                    if ((checkPoint.Y - p1.Y) * (p2.X - p1.X) < (checkPoint.X - p1.X) * (p2.Y - p1.Y))//斜率判断,在P1和P2之间且在P1P2右侧
                    {
                        inside = (!inside);
                    }
                }
            }
            return inside;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
