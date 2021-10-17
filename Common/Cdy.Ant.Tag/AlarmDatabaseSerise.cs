//==============================================================
//  Copyright (C) 2021  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2021/8/18 21:45:02.
//  Version 1.0
//  种道洋
//==============================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public class AlarmDatabaseSerise
    {

        #region ... Variables  ...

        /// <summary>
        /// 
        /// </summary>
        public static AlarmDatabaseSerise Manager = new AlarmDatabaseSerise();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public AlarmDatabase Database { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public AlarmDatabase Load()
        {
            return Load(PathHelper.helper.GetDataPath("local", "local.adb"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public AlarmDatabase LoadByName(string name)
        {
           return  Load(PathHelper.helper.GetDataPath(name, name+ ".adb"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AlarmDatabase QuickLoadByName(string name)
        {
            return LoadQuickly(PathHelper.helper.GetDataPath(name, name + ".adb"));
        }

        /// <summary>
        /// 
        /// </summary>
        public void ContinueLoad()
        {
            string name = Database.Name;
            ContinueLoad(PathHelper.helper.GetDataPath(name, name + ".adb"));
        }

        /// <summary>
        /// 加载差异部分
        /// </summary>
        /// <param name="name"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public AlarmDatabase LoadDifferenceByName(string name, AlarmDatabase target)
        {
            return LoadDifference(PathHelper.helper.GetDataPath(name, name + ".adb"),target);
        }

        /// <summary>
        /// 加载差异部分
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public AlarmDatabase LoadDifference(string path,AlarmDatabase target)
        {
            AlarmDatabase db = new AlarmDatabase();
            if (System.IO.File.Exists(path))
            {
                db.UpdateTime = new System.IO.FileInfo(path).LastWriteTimeUtc.ToString();

                XElement xe = XElement.Load(path);

                db.Name = xe.Attribute("Name").Value;
                db.Version = xe.Attribute("Version").Value;
                                
                if (xe.Element("Tags") != null)
                {
                    foreach (var vv in xe.Element("Tags").Elements())
                    {
                        var vtp = (TagType)int.Parse(vv.Attribute("Type").Value);
                        var tag = TagManager.Manager.CreatTag(vtp);
                        tag.LoadFrom(vv);
                        //var tag = vv.LoadTagFromXML();

                        if (!target.Tags.ContainsKey(tag.Id) || !tag.Equals(target.Tags[tag.Id]))
                        {
                            db.Tags.Add(tag.Id, tag);
                        }
                    }
                    db.BuildNameMap();
                }
                if(db.Tags.Count>0)
                db.MaxId = db.Tags.Keys.Max();
            }
            db.IsDirty = false;
            this.Database = db;
            return db;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public AlarmDatabase LoadQuickly(string path)
        {
            AlarmDatabase db = new AlarmDatabase();
            if (System.IO.File.Exists(path))
            {
                db.UpdateTime = new System.IO.FileInfo(path).LastWriteTimeUtc.ToString();

                XElement xe = XElement.Load(path);

                db.Name = xe.Attribute("Name").Value;
                db.Version = xe.Attribute("Version").Value;
                db.Tags = null;

                if (System.IO.File.Exists(path + "s"))
                {
                    XElement xx = XElement.Load(path + "s");
                    db.Setting = new Setting();
                    if (xx.Attribute("WebServerPort") != null)
                    {
                        db.Setting.WebServerPort = int.Parse(xx.Attribute("WebServerPort").Value);
                    }
                }
                db.IsDirty = false;
                this.Database = db;
            }
            else
            {
                db = null;
            }
         
            
            return db;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void ContinueLoad(string path)
        {
            AlarmDatabase db = Database;
            if (System.IO.File.Exists(path))
            {
                XElement xe = XElement.Load(path);
                Dictionary<string, TagGroup> groups = new Dictionary<string, TagGroup>();
                Dictionary<TagGroup, string> parents = new Dictionary<TagGroup, string>();
                if (xe.Element("Groups") != null)
                {
                    foreach (var vv in xe.Element("Groups").Elements())
                    {
                        TagGroup group = new TagGroup();
                        group.Name = vv.Attribute("Name").Value;
                        string parent = vv.Attribute("Parent") != null ? vv.Attribute("Parent").Value : "";

                        string fullName = vv.Attribute("FullName").Value;

                        if (vv.Attribute("Description") != null)
                        {
                            group.Description = vv.Attribute("Description").Value;
                        }

                        if (!groups.ContainsKey(fullName))
                        {
                            groups.Add(fullName, group);
                        }

                        parents.Add(group, parent);
                    }
                }
                db.Groups = groups;

                foreach (var vv in parents)
                {
                    if (!string.IsNullOrEmpty(vv.Value) && db.Groups.ContainsKey(vv.Value))
                    {
                        vv.Key.Parent = db.Groups[vv.Value];
                    }
                }

                if (xe.Element("Tags") != null)
                {
                    foreach (var vv in xe.Element("Tags").Elements())
                    {
                        var vtp = (TagType)int.Parse(vv.Attribute("Type").Value);
                        var tag = TagManager.Manager.CreatTag(vtp);
                        tag.LoadFrom(vv);
                        db.Tags.Add(tag.Id, tag);
                    }

                    db.BuildNameMap();
                    db.BuildGroupMap();

                }

                if (xe.Attribute("MaxId") != null)
                {
                    db.MaxId = int.Parse(xe.Attribute("MaxId").Value);
                }
                else
                {
                    if (db.Tags.Count > 0)
                        db.MaxId = db.Tags.Keys.Max();
                }

                db.MinId = db.Tags.Count > 0 ? db.Tags.Keys.Min() : 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public AlarmDatabase Load(string path)
        {
            AlarmDatabase db = new AlarmDatabase();
            if (System.IO.File.Exists(path))
            {
                db.UpdateTime = new System.IO.FileInfo(path).LastWriteTimeUtc.ToString();

                XElement xe = XElement.Load(path);

                db.Name = xe.Attribute("Name").Value;
                db.Version = xe.Attribute("Version").Value;

                Dictionary<string, TagGroup> groups = new Dictionary<string, TagGroup>();
                Dictionary<TagGroup, string> parents = new Dictionary<TagGroup, string>(); 
                if(xe.Element("Groups")!=null)
                {
                    foreach (var vv in xe.Element("Groups").Elements())
                    {
                        TagGroup group = new TagGroup();
                        group.Name = vv.Attribute("Name").Value;
                        string parent = vv.Attribute("Parent") != null ? vv.Attribute("Parent").Value : "";

                        string fullName = vv.Attribute("FullName").Value;

                        if(vv.Attribute("Description") !=null)
                        {
                            group.Description = vv.Attribute("Description").Value;
                        }

                        if(!groups.ContainsKey(fullName))
                        {
                            groups.Add(fullName, group);
                        }

                        parents.Add(group, parent);
                    }
                }
                db.Groups = groups;

                foreach(var vv in parents)
                {
                    if(!string.IsNullOrEmpty(vv.Value) && db.Groups.ContainsKey(vv.Value))
                    {
                        vv.Key.Parent = db.Groups[vv.Value];
                    }
                }

                if (xe.Element("Tags") != null)
                {
                    foreach (var vv in xe.Element("Tags").Elements())
                    {
                        var vtp = (TagType) int.Parse(vv.Attribute("Type").Value);
                        var tag = TagManager.Manager.CreatTag(vtp);
                        tag.LoadFrom(vv);
                        db.Tags.Add(tag.Id, tag);
                    }

                    db.BuildNameMap();
                    db.BuildGroupMap();
                    
                }

                if (xe.Attribute("MaxId") != null)
                {
                    db.MaxId = int.Parse(xe.Attribute("MaxId").Value);
                }
                else
                {
                    if(db.Tags.Count>0)
                    db.MaxId = db.Tags.Keys.Max();
                }

                db.MinId = db.Tags.Count>0?db.Tags.Keys.Min():0;

            }

            if (System.IO.File.Exists(path + "s"))
            {
                XElement xx = XElement.Load(path + "s");
                db.Setting = new Setting();
                if (xx.Attribute("WebServerPort") != null)
                {
                    db.Setting.WebServerPort = int.Parse(xx.Attribute("WebServerPort").Value);
                }
            }
            db.IsDirty = false;
            this.Database = db;
            return db;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void SaveAs(string name)
        {
            Save(PathHelper.helper.GetDataPath(name , name + ".adb"));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            Save(PathHelper.helper.GetDataPath(this.Database.Name,this.Database.Name + ".adb"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sfile"></param>
        public void Save(string sfile)
        {
            XElement doc = new XElement("AlarmDatabase");
            doc.SetAttributeValue("Name", Database.Name);
            doc.SetAttributeValue("Version", Database.Version);
            doc.SetAttributeValue("Auther", "cdy");
            doc.SetAttributeValue("MaxId", Database.MaxId);
            doc.SetAttributeValue("TagCount", Database.Tags.Count);
            XElement xe = new XElement("Tags");
            foreach(var vv in Database.Tags.Values)
            {
                xe.Add(vv.SaveTo());
            }
            doc.Add(xe);

            

            xe = new XElement("Groups");
            foreach(var vv in Database.Groups.Values)
            {
                xe.Add(vv.SaveToXML());
            }
            doc.Add(xe);

            string sd = System.IO.Path.GetDirectoryName(sfile);
            if(!System.IO.Directory.Exists(sd))
            {
                System.IO.Directory.CreateDirectory(sd);
            }
            doc.Save(sfile);

            XElement xx = new XElement("AlarmDatabaseSetting");
            xx.SetAttributeValue("WebServerPort", Database.Setting.WebServerPort);
            xx.Save(sfile + ".s");
            Database.IsDirty = false;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="stream"></param>
        //public void Save(System.IO.Stream stream)
        //{
        //    XElement doc = new XElement("AlarmDatabase");
        //    doc.SetAttributeValue("Name", Database.Name);
        //    doc.SetAttributeValue("Version", Database.Version);
        //    doc.SetAttributeValue("Auther", "cdy");
        //    doc.SetAttributeValue("MaxId", Database.MaxId);
        //    doc.SetAttributeValue("TagCount", Database.Tags.Count);
        //    XElement xe = new XElement("Tags");
        //    foreach (var vv in Database.Tags.Values)
        //    {
        //        xe.Add(vv.SaveTo());
        //    }
        //    doc.Add(xe);
        //    xe = new XElement("Groups");
        //    foreach (var vv in Database.Groups.Values)
        //    {
        //        xe.Add(vv.SaveToXML());
        //    }
        //    doc.Add(xe);
        //    doc.Save(stream);
        //}

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
