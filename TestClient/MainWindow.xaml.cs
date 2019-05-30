using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using TestClient.Base;

namespace TestClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        EasyClient client;
        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            string ip = "127.0.0.1";
            int port = 12345;
            client = new EasyClient();
            client.Initialize(new MyFilter(), (request) =>
            {
                Console.WriteLine(request.JsonParas);
                Console.WriteLine("接收端接受长度:" + request.JsonParas.Length);
            });
            var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
            // 收到服务器数据事件
            //client.DataReceived += client_DataReceived;
        }

        void client_DataReceived(object sender, DataEventArgs e)
        {
            string msg = Encoding.Default.GetString(e.Data);
            Dispatcher.Invoke(new Action(() =>
            {
                listbox1.Items.Add(Encoding.ASCII.GetString(e.Data));
            }));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            client.Send(Encoding.UTF8.GetBytes(textbox.Text + "\r\n"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }


}
