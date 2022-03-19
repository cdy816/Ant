﻿using Cdy.Spider.CalculateExpressEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace InAntStudio.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ExpressionEditViewModel: WindowViewModelBase
    {

        #region ... Variables  ...
        
        private string mExpresse = "";

        private RoslynCodeEditor mExpressEditor;

        private System.Collections.ObjectModel.ObservableCollection<CommandGroup> mCommands = new System.Collections.ObjectModel.ObservableCollection<CommandGroup>();

        private System.Collections.ObjectModel.ObservableCollection<ScriptItem> mScripts = new System.Collections.ObjectModel.ObservableCollection<ScriptItem>();

        private CommandItem mCurrentCommand;

        private ICommand mInsertCharCommand;

        private CommandGroup mCurrentCommandGroup;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        public ExpressionEditViewModel()
        {
            Title = Res.Get("Expresse");
            DefaultWidth = 1020;
            DefaultHeight = 760;
            this.IsEnableMax = true;
            Init();
        }
        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public ICommand InsertCharCommand
        {
            get
            {
                if(mInsertCharCommand==null)
                {
                    mInsertCharCommand = new RelayCommand<object>((obj) => {
                        string str = obj.ToString();
                        if(str=="Try")
                        {
                            InsertTextDelegate("try\r\n{\r\n}\r\ncatch (Exception e)\r\n{\r\n}");
                        }
                        else if (str == "If")
                        {
                            InsertTextDelegate("if(true)\r\n{\r\n}");
                        }
                        else if(str=="Enter")
                        {
                            InsertTextDelegate("\r\n");
                        }
                        else if (str == "Space")
                        {
                            InsertTextDelegate(" ");
                        }
                        else if(str=="Return")
                        {
                            InsertTextDelegate("return");
                        }
                        else if(str=="Clear")
                        {
                            ExpressEditor.Text = string.Empty;
                            ExpressEditor.CaretOffset = 0;
                            ExpressEditor.Focus();
                        }
                        else if(str== "Backspace")
                        {
                            var cf = ExpressEditor.SelectionStart;
                            if (ExpressEditor.SelectionLength == 0)
                            {
                                if (ExpressEditor.SelectionStart > 0)
                                {
                                    ExpressEditor.SelectionStart = ExpressEditor.SelectionStart - 1;
                                    ExpressEditor.SelectionLength = 1;
                                    ExpressEditor.SelectedText = string.Empty;
                                    cf -= 1;
                                }
                            }
                            else
                            {
                                ExpressEditor.SelectedText = string.Empty;
                            }
                            ExpressEditor.CaretOffset = cf;
                            ExpressEditor.Focus();
                        }
                        else
                        {
                            InsertTextDelegate(str);
                        }
                    });
                }
                return mInsertCharCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<CommandGroup> Commands
        {
            get
            {
                return mCommands;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<ScriptItem> Scripts
        {
            get
            {
                return mScripts;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public RoslynCodeEditor ExpressEditor
        {
            get
            {
                return mExpressEditor;
            }
            set
            {
                if (mExpressEditor != value)
                {
                    mExpressEditor = value;
                    //mExpressEditor.Text = mExpresse;
                    OnPropertyChanged("ExpressEditor");
                }
            }
        }

        private FrameworkElement mScriptView;
        /// <summary>
        /// 
        /// </summary>
        public FrameworkElement ScriptView
        {
            get
            {
                return mScriptView;
            }
            set
            {
                if (mScriptView != null)
                {
                    mScriptView.PreviewDragEnter -= MScriptView_DragEnter;
                    mScriptView.PreviewDrop -= MScriptView_Drop;
                }
                mScriptView = value;
                if (mScriptView != null)
                {
                    mScriptView.PreviewDragEnter += MScriptView_DragEnter;
                    mScriptView.PreviewDrop += MScriptView_Drop;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Expresse
        {
            get
            {
                return mExpresse;
            }
            set
            {
                if (mExpresse != value)
                {
                    mExpresse = value;
                    //if(mExpressEditor!=null)
                    //mExpressEditor.Text = value;
                    OnPropertyChanged("Expresse");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public CommandItem CurrentCommand
        {
            get
            {
                return mCurrentCommand;
            }
            set
            {
                if (mCurrentCommand != value)
                {
                    if (mCurrentCommand != null) mCurrentCommand.IsSelected = false;
                    mCurrentCommand = value;
                    OnPropertyChanged("CurrentCommand");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public CommandGroup CurrentCommandGroup
        {
            get
            {
                return mCurrentCommandGroup;
            }
            set
            {
                if (mCurrentCommandGroup != value)
                {
                    if (mCurrentCommandGroup != null) mCurrentCommandGroup.IsSelected = false;
                    mCurrentCommandGroup = value;
                    OnPropertyChanged("CurrentCommandGroup");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MScriptView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string txt = (string)e.Data.GetData(DataFormats.Text);
                if (txt != null)
                {
                    if ((e.OriginalSource as FrameworkElement).DataContext is ScriptItem)
                    {
                        UpdateScript(((e.OriginalSource as FrameworkElement).DataContext as ScriptItem), txt);
                    }
                    else
                    {
                        AppendScript(txt);
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string sfile = (string)e.Data.GetData(DataFormats.FileDrop);
                if (sfile != null)
                {
                    if(System.IO.File.Exists(sfile)&&(sfile.EndsWith(".txt")||sfile.EndsWith(".cs")))
                    {
                        if ((e.OriginalSource as FrameworkElement).DataContext is ScriptItem)
                        {
                            UpdateScript(((e.OriginalSource as FrameworkElement).DataContext as ScriptItem), System.IO.File.ReadAllText(sfile));
                        }
                        else
                        {
                            AppendScript(System.IO.File.ReadAllText(sfile));
                        }
                      
                    }
                }
            }
        }

        /// <summary>
        /// 追加脚本
        /// </summary>
        /// <param name="txt"></param>
        private void AppendScript(string txt)
        {
            ScriptNewViewModel mm = new ScriptNewViewModel();
            if(mm.ShowDialog().Value)
            {
                mScripts.Add(new ScriptItem() { Name = mm.Name, Desc = mm.Description, Body = txt ,Parent=this});
                SaveScript();
            }
        }

        private void UpdateScript(ScriptItem item,string txt)
        {
            item.Body = txt;
            SaveScript();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MScriptView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        public void InsertTextDelegate(string exp)
        {
            ExpressEditor.SelectedText = exp;
            ExpressEditor.SelectionLength = 0;
            ExpressEditor.SelectionStart += (exp.Length);
            ExpressEditor.Focus();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            LoadFunction();
            LoadScript();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadFunction()
        {
            mCommands.Clear();
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "AntCommand.xml");
            if(System.IO.File.Exists(sfile))
            {
                XDocument xe = XDocument.Load(sfile);
                foreach(var vv in xe.Element("CommandDocument").Elements())
                {
                    var cg = new CommandGroup() { Parent = this }.LoadFromXML(vv);
                    mCommands.Add(cg);
                    cg.PropertyChanged += Cg_PropertyChanged;
                }
            }
            if(mCommands.Count>0)
            mCommands.Last().IsSelected = true; ;

        }

        private void Cg_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           if(e.PropertyName=="IsSelected")
            {
                if((sender as CommandGroup).IsSelected)
                {
                    CurrentCommandGroup = sender as CommandGroup;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadScript()
        {
            mScripts.Clear();
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "AntScript.xml");
            if (System.IO.File.Exists(sfile))
            {
                XDocument xe = XDocument.Load(sfile);
                foreach (var vv in (xe.FirstNode as XElement).Elements())
                {
                    mScripts.Add(new ScriptItem() { Parent = this }.LoadFromXML(vv));
                }
               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveScript()
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "AntScript.xml");
            XElement xe = new XElement("ScriptDocument");
            xe.SetAttributeValue("Version", "0.1");
            foreach(var vv in mScripts)
            {
                xe.Add(vv.SaveToXML());
            }
            xe.Save(sfile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void RemoveScript(ScriptItem item)
        {
            if(mScripts.Contains(item))
            mScripts.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetExpressResult()
        {
            return mExpressEditor.Text;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class CommandGroup:ViewModelBase
    {
        private bool mIsSelected = false;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<CommandItem> Commands { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ExpressionEditViewModel Parent { get; set; }

        /// <summary>
            /// 
            /// </summary>
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public CommandGroup LoadFromXML(XElement xe)
        {
            Commands = new ObservableCollection<CommandItem>();

            this.Name = xe.Attribute("Name") != null ? xe.Attribute("Name").Value : string.Empty;
            foreach(var vv in xe.Elements())
            Commands.Add(new CommandItem() { Parent = this.Parent }.LoadFromXML(vv));
            return this;
        }

    }

    /// <summary>
    /// 脚本片段
    /// </summary>
    public class ScriptItem:ViewModelBase
    {
        private ICommand mInsertCommand;

        private ICommand mModifyNameCommand;

        private ICommand mModifyDescCommand;

        private ICommand mRemoveCommand;

        private string mName;

        private string mDesc;

        private bool mIsNameModify = false;
        private bool mIsDescModify = false;


        /// <summary>
        /// 
        /// </summary>
        public string Name { get { return mName; } set { mName = value;OnPropertyChanged("Name"); IsNameModify = false; Update(); } }


        /// <summary>
        /// 
        /// </summary>
        public string Desc { get { return mDesc; } set { mDesc = value; OnPropertyChanged("Desc"); IsDescModify = false; Update(); } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNameModify { get { return mIsNameModify; } set { mIsNameModify = value; OnPropertyChanged("IsNameModify"); } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDescModify { get { return mIsDescModify; } set { mIsDescModify = value; OnPropertyChanged("IsDescModify"); } }

        /// <summary>
        /// 
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ExpressionEditViewModel Parent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand InsertCommand
        {
            get
            {
                if(mInsertCommand==null)
                {
                    mInsertCommand = new RelayCommand(() => { 
                        if(Parent!=null)
                        {
                            Parent.InsertTextDelegate(this.Body);
                        }
                    });
                }
                return mInsertCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand RemoveCommand
        {
            get
            {
                if(mRemoveCommand==null)
                {
                    mRemoveCommand = new RelayCommand(() =>
                    {
                        Parent.RemoveScript(this);
                    });
                }
                return mRemoveCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ModifyNameCommand
        {
            get
            {
                if (mModifyNameCommand == null)
                {
                    mModifyNameCommand = new RelayCommand(() => {
                        IsNameModify = true;
                    });
                }
                return mModifyNameCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ModifyDescCommand
        {
            get
            {
                if (mModifyDescCommand == null)
                {
                    mModifyDescCommand = new RelayCommand(() => {
                        IsDescModify = true;
                    });
                }
                return mModifyDescCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public ScriptItem LoadFromXML(XElement xe)
        {
            if (xe.Attribute("Name") != null)
            {
                this.mName = xe.Attribute("Name").Value;
            }

            if (xe.Attribute("Desc") != null)
            {
                this.mDesc = xe.Attribute("Desc").Value;
            }

            this.Body = xe.Value;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            Parent?.SaveScript();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement SaveToXML()
        {
            XElement xe = new XElement("Script");
            xe.SetAttributeValue("Name", this.Name);
            xe.SetAttributeValue("Desc",this.Desc);
            XCData xd = new XCData(Body);
            xe.Add(xd);
            return xe;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class CommandItem:ViewModelBase
    {
        private ICommand mInsertCommand;
        private ICommand mSelectCommand;
        private bool mIsSelected = false;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 函数定义
        /// </summary>
        public Function Function { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Parameter> Parameters
        {
            get
            {
                return Function.Parameters;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand InsertCommand
        {
            get
            {
                if (mInsertCommand == null)
                {
                    mInsertCommand = new RelayCommand(() => {
                        ParametersViewModel mm = new ParametersViewModel();
                        mm.Init(this.Parameters.Select(e => e.Clone()).ToList());
                        if(mm.ShowDialog().Value)
                        {
                            Parent?.InsertTextDelegate(GetInsertBody(mm.GetResult()));
                        }
                    });
                }
                return mInsertCommand;
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public ICommand SelectCommand
        {
            get
            {
                if(mSelectCommand==null)
                {
                    mSelectCommand = new RelayCommand(() => {
                        IsSelected = true;
                        Parent.CurrentCommand = this;
                    });
                }
                return mSelectCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ExpressionEditViewModel Parent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public CommandItem LoadFromXML(XElement xe)
        {
            if(xe.Attribute("Name")!=null)
            {
                this.Name = xe.Attribute("Name").Value;
            }

            if (xe.Attribute("Desc") != null)
            {
                this.Description = xe.Attribute("Desc").Value;
            }

            Function ff = new Function();
            ff.LoadFromXML(xe.FirstNode as XElement);
            this.Function = ff;

            return this;
        }

        private string GetInsertBody(List<Parameter> vals)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Function.Body);
            sb.Append("(");
            foreach(var vv in vals)
            {
                if(vv.Type.Equals("string",StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append("\"");
                    sb.Append(vv.Value.ToString());
                    sb.Append("\"");
                }
                else
                {
                    if(vv.Value!=null)
                    {
                        sb.Append(vv.Value.ToString());
                    }
                }
                sb.Append(",");
            }
            if (vals.Count > 0) sb.Length = sb.Length - 1;
            sb.Append(")");
            return sb.ToString();
        }
    }

    /// <summary>
    /// 函数
    /// </summary>
    public class Function
    {
        /// <summary>
        /// 函数体
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public List<Parameter> Parameters { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public void LoadFromXML(XElement xe)
        {
            this.Body = xe.Attribute("Body") != null ? xe.Attribute("Body").Value : "";
            
            this.ReturnType = xe.Attribute("ReturnType") != null ? xe.Attribute("ReturnType").Value : "";

            this.Parameters = new List<Parameter>();
            foreach(var vv in xe.Elements())
            {
                Parameter pp = new Parameter();
                pp.LoadFromXML(vv);
                this.Parameters.Add(pp);
            }
        }

    }
    /// <summary>
    /// 参数
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DesignType { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 配置的值
        /// </summary>
        public string Value { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public void LoadFromXML(XElement xe)
        {
            Name = xe.Attribute("Name") != null ? xe.Attribute("Name").Value : string.Empty;
            Type = xe.Attribute("Type") != null ? xe.Attribute("Type").Value : string.Empty;
            Desc = xe.Attribute("Desc") != null ? xe.Attribute("Desc").Value : string.Empty;
            Value = xe.Attribute("Value") != null ? xe.Attribute("Value").Value : string.Empty;
            DesignType = xe.Attribute("DesignType") != null ? xe.Attribute("DesignType").Value : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Parameter Clone()
        {
            return new Parameter() { Name = this.Name, Type = this.Type, Desc = this.Desc, Value = this.Value,DesignType=this.DesignType };
        }

    }


}
