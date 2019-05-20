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
using System.Threading;
using System.Diagnostics;
using CMPLCKit;
using SuperSocket.SocketBase;

namespace TestDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PLCManagement.OnReceivedMessage += PLCManagement_OnReceivedMessage;
            PLCManagement.OnSerialPortStatusChanged += PLCManagement_OnSerialPortStatusChanged;
        }

        private void PLCManagement_OnSerialPortStatusChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var args = (StatusEventArgs)e;
                var status = args.Status;
                if (status == Status.Opened) txt_SerialPortStatus.Text = "串口已打开";
                if (status == Status.Closed) txt_SerialPortStatus.Text = "串口已关闭";
            }));
        }

        private void PLCManagement_OnReceivedMessage(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var args = (StatusEventArgs)e;
                var status = args.Status;
                if (status == Status.Success) txt_MessageStatus.Text = "通讯正常";
                else if (status == Status.Timeout) txt_MessageStatus.Text = "消息超时";
                else txt_MessageStatus.Text = "通讯失败";
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PLCManagement.Initialize();
            //PLCManagement.Run();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var appServer = new AppServer();
            appServer.Setup("127.0.0.1",12345);
            appServer.Start(); 

        }
    }
}
