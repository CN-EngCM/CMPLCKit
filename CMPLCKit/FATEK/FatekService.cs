using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMPLCKit.FATEK
{

    public class FatekService
    {

        private SerialPort port;
        private string portName;
        private int baudRate;
        private object locker;
        private bool IsRunning;
        private Thread MainThread;

        public event EventHandler OnSerialPortStatusChanged;
        public event EventHandler OnReceivedMessage;
        public event EventHandler OnHandleWrite;
        public event EventHandler OnHandleRead;
        public event EventHandler OnOneLoopEnd;

        public long LoopSpan { get; set; }
        public List<DataInfo> RegisteredData { get; set; }

        public FatekService(string portName, int baudRate)
        {
            this.portName = portName;
            this.baudRate = baudRate;
            locker = new object();
            RegisteredData = new List<DataInfo>();
        }



        public void RegisterData(string Name, RegisterType RegisterType, int Index, DataType DataType, DataAccess DataAccess, int CycleTime)
        {
            if (IsRunning) throw new Exception("系统已启动,禁止注册新数据");
            RegisteredData.Add(new DataInfo
            {
                Name = Name,
                CycleTime = CycleTime,
                DataAccess = DataAccess,
                DataType = DataType,
                Index = Index,
                RegisterType = RegisterType
            });
        }

        public void OpenAsync()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                lock (locker)
                {
                    if (port != null) port.Dispose();
                    port = new SerialPort();
                    port.ReadTimeout = 200;
                    port.WriteTimeout = 200;
                    port.PortName = portName;
                    port.BaudRate = baudRate;
                    port.DataBits = 7;
                    port.Parity = Parity.Even;
                    port.StopBits = StopBits.One;
                    while (!port.IsOpen)
                    {
                        try
                        {
                            port.Open();
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    OnSerialPortStatusChanged?.Invoke(this, new StatusEventArgs { Status = Status.Opened });
                }
            });
        }

        public bool? IsOpen
        {
            get {
                return port == null ? null : (bool?)port.IsOpen;
            }
        }

        public string Send(string CmdString)
        {
            var res = Encoding.ASCII.GetBytes(CmdString.ToString());
            byte[] message = new byte[res.Length + 4];
            //STX
            message[0] = 0x02;
            Array.Copy(res, 0, message, 1, res.Length);
            //LRC
            var bytes = GetLRC(message);
            message[message.Length - 3] = bytes[0];
            message[message.Length - 2] = bytes[1];
            //ETX
            message[message.Length - 1] = 0x03;
            lock (locker)
            {
                //发送
                try
                {
                    port.Write(message, 0, message.Length);
                }
                catch
                {
                    OnSerialPortStatusChanged?.Invoke(this, new StatusEventArgs { Status = Status.Closed });
                    throw;
                }
                //接收
                List<byte> receivedBytes = new List<byte>();
                bool flg;
                int errorCount = 0;
                do
                {
                    int n = port.BytesToRead;
                    if (n > 0)
                    {
                        byte[] buf = new byte[n];
                        try
                        {
                            port.Read(buf, 0, n);
                        }
                        catch
                        {
                            OnSerialPortStatusChanged?.Invoke(this, new StatusEventArgs { Status = Status.Closed });
                            throw;
                        }
                        receivedBytes.AddRange(buf);
                    }
                    else
                    {
                        if (errorCount >= 500)
                        {
                            OnReceivedMessage?.Invoke(this, new StatusEventArgs { Status = Status.Timeout });
                            throw new Exception("接受数据超时");
                        }
                        errorCount += 1;
                        Thread.Sleep(1);
                    }
                    if (receivedBytes.Count == 0) flg = true;
                    else if (receivedBytes[receivedBytes.Count - 1] != 0x03) flg = true;
                    else flg = false;
                } while (flg);
                //处理
                byte[] receivedArray = receivedBytes.ToArray();
                byte[] lrcBytes = GetLRC(receivedArray);
                if (lrcBytes[0] == receivedArray[receivedArray.Length - 3] & lrcBytes[1] == receivedArray[receivedArray.Length - 2])
                {
                    if (receivedArray[5] == 0x30)
                    {
                        byte[] a = new byte[receivedArray.Length - 2];
                        Array.Copy(receivedArray, 1, a, 0, receivedArray.Length - 2);//a为去掉头尾之后的数组
                        OnReceivedMessage?.Invoke(this, new StatusEventArgs { Status = Status.Success });
                        return Encoding.ASCII.GetString(a);//将接收到的数组转为字符串
                    }
                    else
                    {
                        OnReceivedMessage?.Invoke(this, new StatusEventArgs { Status = Status.Fail });
                        throw new Exception("消息发送不正确,错误码" + receivedArray[5].ToString("X2"));
                    }

                }
                else
                {
                    OnReceivedMessage?.Invoke(this, new StatusEventArgs { Status = Status.Fail });
                    throw new Exception("接收到的校验码不正确");
                }

            }

        }

        public UInt16[] Read(string areaType, int startIndex, int length)
        {
            if (areaType != "R" & areaType != "D") throw new Exception("输入的缓存区名称不正确");
            StringBuilder str = new StringBuilder();
            str.Append("01");//站号
            str.Append("46");//命令码
            str.Append(length.ToString().PadLeft(2, '0'));//个数
            str.Append(areaType);//R区或者D区
            str.Append(startIndex.ToString().PadLeft(5, '0'));
            var res = Send(str.ToString());
            var returnData = new UInt16[length];
            for (int i = 0; i < length; i++)
            {
                string dataString = res.Substring(5 + i * 4, 4);
                returnData[i] = Convert.ToUInt16(dataString, 16);
            }
            return returnData;
        }

        public UInt32 UInt16ToUInt32(UInt16[] datas, int startIndex)
        {
            return (UInt32)(datas[startIndex + 1] * 65536) + datas[startIndex];
        }

        public decimal UInt16ToFloat(UInt16[] datas, int startIndex)
        {
            var high = datas[startIndex + 1];
            var low = datas[startIndex];
            byte[] a = new byte[4];
            a[0] = Convert.ToByte(low & 0xFF);//低8位
            a[1] = Convert.ToByte(low >> 8);//高8位
            a[2] = Convert.ToByte(high & 0xFF);//低8位
            a[3] = Convert.ToByte(high >> 8);//高8位
            return Math.Round((decimal)BitConverter.ToSingle(a, 0), 2);
        }

        private byte[] GetLRC(byte[] a)
        {
            byte[] LRC;
            byte sum = 0x00;
            for (int i = 0; i < a.Length - 3; i++)
            {
                sum += a[i];
            }
            LRC = BitConverter.GetBytes(sum);
            string strLRC = LRC[0].ToString("X2");
            byte[] byteLRC = Encoding.ASCII.GetBytes(strLRC);
            return byteLRC;
        }

        public void StartService()
        {
            if (MainThread == null) MainThread = new Thread(Loop);
            MainThread.IsBackground = true;
            MainThread.Start();
            IsRunning = true;
        }

        public void StopService()
        {
            MainThread.Abort();
        }

        private void Loop()
        {
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                stopwatch.Restart();
                try
                {
                    OnHandleWrite?.Invoke(this, null);
                    Thread.Sleep(5);
                    OnHandleRead?.Invoke(this, null);
                    OnOneLoopEnd?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException)
                    {
                        OpenAsync();
                    }
                    else
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                LoopSpan = stopwatch.ElapsedMilliseconds;
            }
        }



    }
}
