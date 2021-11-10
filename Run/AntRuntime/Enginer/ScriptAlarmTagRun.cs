using Cdy.Ant;
using Cdy.Ant.Tag;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public class ScriptAlarmTagRun : TagRunBase
    {

        #region ... Variables  ...
        static ScriptOptions sop;
        ScriptTag mDTag;
        private Script<object> mScript;
        private bool mIsNeedCallAlways = false;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        static ScriptAlarmTagRun()
        {
            InitGlobel();
        }

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public override Tagbase LinkedTag { get => base.LinkedTag; set { base.LinkedTag = value; mDTag = value as Cdy.Ant.ScriptTag; } }

        /// <summary>
        /// 
        /// </summary>
        public TagScriptImp Tag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MessageScriptImp Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override TagType SupportTag => TagType.Script;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        private static void InitGlobel()
        {
            sop = ScriptOptions.Default;
            try
            {
                if (ScriptExtend.extend.ExtendDlls.Count > 0)
                {
                    sop = sop.AddReferences(ScriptExtend.extend.ExtendDlls.Select(e => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(e)));
                }
                sop = sop.AddReferences(typeof(System.Collections.Generic.ReferenceEqualityComparer).Assembly).AddReferences(typeof(ScriptExtend).Assembly).AddReferences(typeof(ScriptAlarmTagRun).Assembly).WithImports("AntRuntime.Enginer", "Cdy.Ant.Tag", "System", "System.Collections.Generic");
            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("ScriptAlarmTagRun", ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            var vsp = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create(mDTag.Expresse, sop, typeof(ScriptAlarmTagRun));
            try
            {
                var cp = vsp.Compile();
                if (cp != null && cp.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var vvp in cp)
                    {
                        sb.Append(vvp.ToString());
                    }
                    LoggerService.Service.Warn("ScriptAlarmTagRun", mDTag.FullName + " " + sb.ToString());
                }
                mScript = vsp;

                //如果没有操作任何变量，则直接开个线程让其执行
                if(ListLinkTag().Count==0)
                {
                    mIsNeedCallAlways = true;
                }

            }
            catch (Exception ex)
            {
                LoggerService.Service.Erro("ScriptAlarmTagRun", ex.Message);
            }
            base.Init();
        }

        

        /// <summary>
        /// 
        /// </summary>
        public override void LinkExecute()
        {
            try
            {
                mScript?.RunAsync(this, (exp) =>
                {
                    LoggerService.Service.Erro("ScriptAlarmTagRun", this.mDTag.FullName + " : " + exp.Message);
                    return true;
                });
                if (mIsNeedCallAlways) mNeedCal = true;
            }
            catch
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> ListLinkTag()
        {
            var re = AnalysizeTags(mDTag.Expresse);
            var rr = base.ListLinkTag();
            rr.AddRange(re);
            return rr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private List<string> AnalysizeTags(string exp)
        {
             Regex regex = new Regex(@"\bTag((\.\w*)(?!\())*\b",
             RegexOptions.IgnoreCase | RegexOptions.Multiline |
             RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            List<string> ltmp = new List<string>();

            var vvs = regex.Matches(exp);
            if (vvs.Count > 0)
            {
                foreach (var vv in vvs)
                {
                    ltmp.Add(vv.ToString());
                }
            }

            return ltmp;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }

    /// <summary>
    /// 
    /// </summary>
    public class TagScriptImp
    {
        public ScriptAlarmTagRun Owner { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MessageScriptImp
    {
        public ScriptAlarmTagRun Owner { get; set; }
    }


}
