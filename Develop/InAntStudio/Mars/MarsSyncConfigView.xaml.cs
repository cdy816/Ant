using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InAntStudio.View
{
    /// <summary>
    /// MarsSyncConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class MarsSyncConfigView : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public MarsSyncConfigView()
        {
            InitializeComponent();
            this.Loaded += MarsSyncConfigView_Loaded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MarsSyncConfigView_Loaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MarsSyncConfigViewModel).Load();
        }
    }
}
