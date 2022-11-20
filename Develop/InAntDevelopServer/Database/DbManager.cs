using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBDevelopService
{
    public class DbManager
    {

        #region ... Variables  ...
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Cdy.Ant.AlarmDatabase> mDatabase = new Dictionary<string, Cdy.Ant.AlarmDatabase>();

        public static DbManager Instance = new DbManager();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public bool IsLoaded { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...



        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            string databasePath = PathHelper.helper.DataPath;
            
            if (System.IO.Directory.Exists(databasePath))
            {
                foreach (var vv in System.IO.Directory.EnumerateDirectories(databasePath))
                {
                    string sname = new System.IO.DirectoryInfo(vv).Name;

                    Cdy.Ant.AlarmDatabase db = new Cdy.Ant.AlarmDatabaseSerise().LoadByName(sname);
                    if(!mDatabase.ContainsKey(db.Name))
                    mDatabase.Add(db.Name, db);
                    else
                    {
                        mDatabase[db.Name] = db;
                    }
                }
            }
            IsLoaded = true;
        }

        /// <summary>
        /// 局部加载
        /// </summary>
        public void PartLoad()
        {
            string databasePath = PathHelper.helper.DataPath;

            if (System.IO.Directory.Exists(databasePath))
            {
                foreach (var vv in System.IO.Directory.EnumerateDirectories(databasePath))
                {
                    string sname = new System.IO.DirectoryInfo(vv).Name;

                    Cdy.Ant.AlarmDatabase db = new Cdy.Ant.AlarmDatabaseSerise().QuickLoadByName(sname);
                    if (db != null)
                    {
                        if (!mDatabase.ContainsKey(db.Name))
                            mDatabase.Add(db.Name, db);
                        else
                        {
                            mDatabase[db.Name] = db;
                        }
                    }
                }
            }
            IsLoaded = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        public void CheckAndContinueLoadDatabase(AlarmDatabase db)
        {
            lock (db)
            {
                if (db.Tags == null || db.Tags.Count==0)
                {
                   new AlarmDatabaseSerise() { Database = db }.ContinueLoad();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void QuickReload()
        {
            string databasePath = PathHelper.helper.DataPath;

            if (System.IO.Directory.Exists(databasePath))
            {
                foreach (var vv in System.IO.Directory.EnumerateDirectories(databasePath))
                {
                    string sname = new System.IO.DirectoryInfo(vv).Name;
                    if (!mDatabase.ContainsKey(sname))
                    {
                        AlarmDatabase db = new AlarmDatabaseSerise().LoadByName(sname);
                        mDatabase.Add(db.Name, db);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> ListMarsDatabase()
        {
            List<string> ll = new List<string>();
            string databasePath = PathHelper.helper.DataPath;
            if (System.IO.Directory.Exists(databasePath))
            {
                foreach (var vv in System.IO.Directory.EnumerateDirectories(databasePath))
                {
                    string sname = new System.IO.DirectoryInfo(vv).Name;
                    ll.Add(sname);
                }
            }
            return ll;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        public void ReLoad(string database)
        {
            string databasePath = PathHelper.helper.DataPath;

            if (System.IO.Directory.Exists(databasePath))
            {
                foreach (var vv in System.IO.Directory.EnumerateDirectories(databasePath))
                {
                    string sname = new System.IO.DirectoryInfo(vv).Name;

                    if (sname == database)
                    {
                        AlarmDatabase db = new AlarmDatabaseSerise().LoadByName(sname);
                        if (!mDatabase.ContainsKey(db.Name))
                            mDatabase.Add(db.Name, db);
                        else
                        {
                            mDatabase[db.Name] = db;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="db"></param>
        public void AddDatabase(string name, AlarmDatabase db)
        {
            if(!mDatabase.ContainsKey(name))
            {
                mDatabase[name] = db;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reload()
        {
            mDatabase.Clear();
            Load();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        public void Reload(string database)
        {
            if(mDatabase.ContainsKey(database))
            {
                mDatabase.Remove(database);
            }

            Cdy.Ant.AlarmDatabase db = new Cdy.Ant.AlarmDatabaseSerise().LoadByName(database);
            if (!mDatabase.ContainsKey(db.Name))
                mDatabase.Add(db.Name, db);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public AlarmDatabase NewDB(string name,string desc)
        {
            if (mDatabase.ContainsKey(name))
            {
                mDatabase[name] = new AlarmDatabase() { Name = name};
            }
            else
            {
                mDatabase.Add(name, new AlarmDatabase() { Name = name});
            }
            return mDatabase[name];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AlarmDatabase GetDatabase(string name)
        {
            if (mDatabase.ContainsKey(name))
                return mDatabase[name];
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool Save(AlarmDatabase database)
        {
            try
            {
                new AlarmDatabaseSerise() { Database = database }.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] ListDatabase()
        {
            return mDatabase.Keys.ToArray();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
