using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdy.Ant.Tag
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRuntimeTagService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="propertys"></param>
        /// <returns></returns>
        bool ModifyTag(string tagname, Dictionary<string, string> propertys);
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagname"></param>
        /// <returns></returns>
        Dictionary<string, string> ListTagDefines(string tagname);


    }
}
