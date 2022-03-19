using AntRuntime;
using Microsoft.Win32;
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

namespace MessageServiceTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string datebaseFolder = "";
        private List<long> msgIdCach = new List<long>();

        public MainWindow()
        {
            InitializeComponent();
            starttime.SelectedDate = DateTime.Now.AddDays(-1);
            endtime.SelectedDate = DateTime.Now;
        }

        private void openb_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Multiselect=false};
            ofd.Filter = "alarm database|*.adb";
            if(ofd.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(ofd.FileName))
                {
                    datebaseFolder = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(ofd.FileName)).Name;
                    HisMessageService.Service.DatabaseName = datebaseFolder;
                    MessageService.Service.DatabaseName = datebaseFolder;
                    Cdy.Ant.PathHelper.helper.DataPath = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(ofd.FileName)).Parent.FullName;
                }
                databasename.Text = datebaseFolder;
            }
        }

        private void queryb_Click(object sender, RoutedEventArgs e)
        {
            DateTime stime = starttime.SelectedDate.Value.Date.AddHours(starttimeh.SelectedIndex);
            DateTime etime = this.endtime.SelectedDate.Value.Date.AddHours(endtimeh.SelectedIndex);
            msgIdCach.Clear();
            this.msg.Clear();
            var msg = MessageService.Service.Query(stime,etime);
            if (msg != null)
            {
                int i = 0;
                foreach (var item in msg)
                {
                    this.msg.AppendText( i+" > "+ item.ToFormateString()+Environment.NewLine);
                    i++;
                    msgIdCach.Add(item.Id);
                }
            }

        }

        private void alarmB_Click(object sender, RoutedEventArgs e)
        {
            for(int i=0;i<100;i++)
            {
                DateTime dt = DateTime.Now.AddSeconds(i);
                Cdy.Ant.Tag.AlarmMessage msg = new Cdy.Ant.Tag.AlarmMessage();
                msg.CreateTime = dt;
                msg.Server = "Server";
                msg.SourceTag = "Tag"+i;
                msg.LinkTag = "Tag"+i;
                msg.MessageBody = $"Tag{i} 高报警 ";
                msg.AlarmLevel = Cdy.Ant.AlarmLevel.Critical;
                msg.AlarmValue = "1";
                msg.Id = MessageService.Service.GetId(dt.Ticks);
                msg.AlarmCondition = ">1";

                MessageService.Service.RaiseMessage(msg);
            }
        }

        private void infob_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                DateTime dt = DateTime.Now.AddSeconds(i);
                Cdy.Ant.Tag.InfoMessage msg = new Cdy.Ant.Tag.InfoMessage();
                msg.CreateTime = dt;
                msg.Server = "Server";
                msg.SourceTag = "Tag" + i;
                msg.MessageBody =  $"Tag {i} 日志消息";
                msg.Id = MessageService.Service.GetId(dt.Ticks);

                MessageService.Service.RaiseMessage(msg);
            }
        }

        private void freshDisk_Click(object sender, RoutedEventArgs e)
        {
            MessageService.Service.FlushDirtyToDiskAll();
        }

        private void alarmack_Click(object sender, RoutedEventArgs e)
        {
            foreach (var vv in msgIdCach)
                MessageService.Service.AckMessage(vv, qrbz.Text, qrr.Text);
        }

        private void msgdelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (var vv in msgIdCach)
                MessageService.Service.DeleteMessage(vv, scbz.Text, scr.Text);
        }

        private void restoreb_Click(object sender, RoutedEventArgs e)
        {
            foreach (var vv in msgIdCach)
                MessageService.Service.RestoreMessage(vv, "报警恢复值");
        }

        private void querydelb_Click(object sender, RoutedEventArgs e)
        {
            DateTime stime = starttime.SelectedDate.Value.Date.AddHours(starttimeh.SelectedIndex);
            DateTime etime = this.endtime.SelectedDate.Value.Date.AddHours(endtimeh.SelectedIndex);
            msgIdCach.Clear();
            this.msg.Clear();
            var msg = MessageService.Service.QueryDelete(stime, etime);
            if (msg != null)
            {
                int i = 0;
                foreach (var item in msg)
                {
                    this.msg.AppendText(i + " > " + item.ToFormateString() + Environment.NewLine);
                    i++;
                    msgIdCach.Add(item.Id);
                }
            }
        }
    }
}
