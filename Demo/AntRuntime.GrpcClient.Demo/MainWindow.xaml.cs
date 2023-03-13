using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace AntRuntime.GrpcClient.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private AntRuntime.GrpcApi.Client client;

        /// <summary>
        /// 
        /// </summary>
        enum GetMessageType { All,Alarm,Info};

        /// <summary>
        /// 
        /// </summary>
        private GetMessageType GetType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private DateTime mLastTime;

        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            mLastTime = DateTime.Now;
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void conn_Click(object sender, RoutedEventArgs e)
        {
            client = new AntRuntime.GrpcApi.Client();
            client.Login(ipa.Text,user.Text,pass.Text);
            conn.IsEnabled = false;

            client.MessageNotifyCallBack = new GrpcApi.Client.MessageNotifyCallBackDelegate((mm,iscancel) => {
                if (!iscancel)
                {
                    this.Dispatcher.Invoke(() => {
                        msgShow.AppendText(mm.FormateToString() + "\r\n");
                        msgShow.ScrollToEnd();
                    });
                }
                else
                {
                    Task.Run(() => {
                        client.RegistorMessageNotify();
                    });
                }
            });
            client.RegistorMessageNotify();

            //Task.Run(() => { 
            
            //    while(true)
            //    {

            //        IEnumerable<Cdy.Ant.Message> msgs=null;

            //        switch (GetType)
            //        {
            //            case GetMessageType.All:

            //                msgs = client.QueryRecentMessage(mLastTime);
            //                mLastTime = DateTime.Now;
            //                break;
            //            case GetMessageType.Alarm:
            //                msgs = client.QueryRecentAlarmMessage(mLastTime);
            //                mLastTime = DateTime.Now;
            //                break;
            //            case GetMessageType.Info:
            //                msgs = client.QueryRecentInfoMessage(mLastTime);
            //                mLastTime = DateTime.Now;
            //                break;
            //        }

            //        if(msgs!=null)
            //        {
            //            foreach (Cdy.Ant.Message msg in msgs)
            //            {
            //                this.Dispatcher.Invoke(() => { 
                            
            //                    msgShow.AppendText(msg.FormateToString()+"\r\n");
            //                    msgShow.ScrollToEnd();
            //                });
            //            }
            //        }


            //        Thread.Sleep(1000);
            //    }
            //});

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allmsg_Checked(object sender, RoutedEventArgs e)
        {
            GetType = GetMessageType.All;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void alarmmsg_Checked(object sender, RoutedEventArgs e)
        {
            GetType=GetMessageType.Alarm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void infomsg_Checked(object sender, RoutedEventArgs e)
        {
            GetType = GetMessageType.Info;
        }
    }
}
