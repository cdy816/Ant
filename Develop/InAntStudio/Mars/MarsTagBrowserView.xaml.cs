﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InAntStudio
{
    /// <summary>
    /// TagBrowserView.xaml 的交互逻辑
    /// </summary>
    public partial class MarsTagBrowserView : UserControl
    {
        public MarsTagBrowserView()
        {
            InitializeComponent();
            this.Loaded += TagBrowserView_Loaded;
        }

        private void TagBrowserView_Loaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MarsTagBrowserViewModel).Grid = dg;
            if ((this.DataContext as MarsTagBrowserViewModel).IsLocalServer)
            {
                (this.DataContext as MarsTagBrowserViewModel).LoadFromLocal();
            }
            else
            {
                (this.DataContext as MarsTagBrowserViewModel).ConnectCommand.Execute(null);
            }
        }

        private void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            (this.DataContext as MarsTagBrowserViewModel).CurrentGroup = (tv.SelectedItem as MarsTagGroupViewModel);
        }

        private void GetVisualParent(DependencyObject target, List<string> ll)
        {
            var parent = LogicalTreeHelper.GetParent(target);
            if (parent != null)
            {
                ll.Add(parent.ToString());
                GetVisualParent(parent, ll);
            }
        }

        private void dg_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            List<string> ll = new List<string>();
            GetVisualParent(e.OriginalSource as DependencyObject, ll);
            if (ll.Contains("System.Windows.Controls.Primitives.Popup")) return;

            var dscoll = (e.ExtentHeight - e.ViewportHeight);

            if (dscoll > 0 && (dscoll - e.VerticalOffset) / dscoll < 0.25)
            {
                if (!(this.DataContext as MarsTagBrowserViewModel).IsLocalServer)
                    (this.DataContext as MarsTagBrowserViewModel).ContinueLoadData();
            }
        }

        private void kwinput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as FrameworkElement).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void dg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount>1)
            {
                var cmd = (this.DataContext as MarsTagBrowserViewModel).OKCommand;
                if(cmd.CanExecute(null))
                {
                    cmd.Execute(null);
                }
            }
        }

        private void tv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource is Grid)
            {
                //(sender as TreeView).SelectedValue
            }
        }
    }
}
