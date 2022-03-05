using Cdy.Spider.CalculateExpressEditor;
using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Immutable;
using InAntStudio.ViewModel;
using Cdy.Ant.Tag;
using System.Windows.Input;

namespace InAntStudio
{
    /// <summary>
    /// ExpressionEditView.xaml 的交互逻辑
    /// </summary>
    public partial class ExpressionEditView : UserControl
    {

        private RoslynHost mHost;
        /// <summary>
        /// 
        /// </summary>
        public ExpressionEditView()
        {
            InitializeComponent();
            this.Loaded += ExpressionEditView_Loaded;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExpressionEditView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ExpressionEditView_Loaded;
            (this.DataContext as ExpressionEditViewModel).ExpressEditor = rc;
            (this.DataContext as ExpressionEditViewModel).ScriptView = sc;
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            List<Assembly> ass = new List<Assembly>();
            ass.Add(typeof(Cdy.Ant.Tag.ScriptExtend).Assembly);
            ass.Add(typeof(Cdy.Spider.CalculateExpressEditor.AvalonEditExtensions).Assembly);
            ass.Add(this.GetType().Assembly);

            if (ScriptExtend.extend.ExtendDlls.Count > 0)
            {
                var vfiles = new List<Assembly>();
                try
                {
                    foreach(var vv in ScriptExtend.extend.ExtendDlls)
                    {
                        if(System.IO.File.Exists(vv))
                        {
                            vfiles.Add(Assembly.LoadFile(vv));
                        }
                    }
                }
                catch
                {

                }
                ass.AddRange(vfiles);
            }

            mHost = new RoslynHost(ass.ToArray(), RoslynHostReferences.NamespaceDefault.With(new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Text.RegularExpressions.Regex).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            }),new string[] { "Cdy.Ant.Tag","Cdy.Ant" }, ass.Select(e=>e.Location).ToArray());


            var colors = new ClassificationHighlightColors();
            colors.DefaultBrush.Foreground = new  ICSharpCode.AvalonEdit.Highlighting.SimpleHighlightingBrush(Colors.White);
            colors.KeywordBrush.Foreground = new ICSharpCode.AvalonEdit.Highlighting.SimpleHighlightingBrush(Colors.LightBlue);
            colors.StringBrush.Foreground = new ICSharpCode.AvalonEdit.Highlighting.SimpleHighlightingBrush(Colors.OrangeRed);

            string sval = (this.DataContext as ExpressionEditViewModel).Expresse;

            rc.Initialize(mHost, colors, AppDomain.CurrentDomain.BaseDirectory, sval==null?"":sval);
        }

        private void ss_Click(object sender, RoutedEventArgs e)
        {
            if(bd.Height==74)
            {
                var tg = new TransformGroup();
                tg.Children.Add(new RotateTransform() { Angle = -90 });
                tg.Children.Add(new TranslateTransform() { Y = 10 });
                bd.Height = 14;
                tb.RenderTransform = tg;
            }
            else
            {
                var tg = new TransformGroup();
                tg.Children.Add(new RotateTransform() { Angle = 90 });
                tg.Children.Add(new TranslateTransform() { Y = 2,X=14 });
              
                bd.Height = 74;
                tb.RenderTransform = tg;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private Point mMouseDownPoint;
        private bool mIsMouseDown = false;
        private bool mIsDrag = false;

        private void mb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mMouseDownPoint = e.GetPosition(sender as FrameworkElement);
            mIsMouseDown = true;
            e.Handled= true;
        }

        private void mb_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mIsMouseDown && !mIsDrag)
            {
                var vp = e.GetPosition((FrameworkElement)sender);
                if((vp - mMouseDownPoint).Length>10)
                {
                    mIsDrag = true;
                    ProcessDrag(((sender as FrameworkElement).DataContext as ScriptItem).Body);
                }
                e.Handled =(true);
            }
        }

        private void mb_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mIsDrag = false;
            mIsMouseDown = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private void ProcessDrag(string text)
        {
            DragDrop.DoDragDrop(this, text, DragDropEffects.Copy|DragDropEffects.All);
        }

        private void mb_MouseLeave(object sender, MouseEventArgs e)
        {
            mIsDrag = false;
            mIsMouseDown = false;
        }
    }
}
