using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public class TagGroup
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TagGroup Parent { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FullName { get { return Parent == null ? Name : Parent.FullName + "." + Name; } }

        /// <summary>
        /// 
        /// </summary>
        public List<Tagbase> Tags { get; set; } = new List<Tagbase>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement SaveToXML()
        {
            XElement xe = new XElement("TagGroup");
            xe.SetAttributeValue("Name", Name);
            if (!string.IsNullOrEmpty(Description))
            {
                xe.SetAttributeValue("Description", Description);
            }
            if (Parent != null)
                xe.SetAttributeValue("Parent", Parent.FullName);
            xe.SetAttributeValue("FullName", FullName);
            return xe;
        }

    }

}
