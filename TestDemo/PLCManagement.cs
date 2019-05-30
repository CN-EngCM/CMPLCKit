using CMPLCKit.FATEK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMPLCKit;

namespace TestDemo
{
    public static class PLCManagement
    {
        private static bool isRunning;
        private static FatekService plc;
        private static Thread MainThread;
        private static decimal R0;
        private static decimal R2;
        private static decimal R4;
        private static decimal R6;
        private static decimal R8;
        private static decimal R10;
        private static decimal R12;
        private static decimal R14;
        private static decimal R16;
        private static decimal R18;
        private static decimal R20;
        private static decimal R22;
        private static decimal R24;
        private static decimal R26;
        private static decimal R28;
        private static decimal R30;
        private static decimal R32;

        public static event EventHandler OnSerialPortStatusChanged;
        public static event EventHandler OnReceivedMessage;


        public static void Initialize()
        {
            plc = new FatekService("COM3", 115200);
            plc.OnSerialPortStatusChanged += Plc_OnSerialPortStatusChanged;
            plc.OnReceivedMessage += Plc_OnReceivedMessage;
            plc.OnHandleRead += Plc_OnHandleRead;
            plc.OpenAsync();
        }

        private static void Plc_OnHandleRead(object sender, EventArgs e)
        {
            var data = plc.Read("R", 0, 34);
            var newR0 = plc.UInt16ToUInt32(data, 0);
            if (R0 != newR0)
            {
                R0 = plc.UInt16ToUInt32(data, 0);
                R2 = plc.UInt16ToFloat(data, 2);
                R4 = plc.UInt16ToUInt32(data, 4);
                R6 = plc.UInt16ToUInt32(data, 6);
                R8 = plc.UInt16ToUInt32(data, 8);
                R10 = plc.UInt16ToUInt32(data, 10);
                R12 = plc.UInt16ToUInt32(data, 12);
                R14 = plc.UInt16ToUInt32(data, 14);
                R32 = plc.UInt16ToUInt32(data, 32);
                Console.WriteLine(R0+"\r\n"+"一轮耗时:"+plc.LoopSpan);
            } 
        }

        private static void Plc_OnReceivedMessage(object sender, EventArgs e)
        {
            OnReceivedMessage?.Invoke(sender, e);
        }

        private static void Plc_OnSerialPortStatusChanged(object sender, EventArgs e)
        {
            var args = (StatusEventArgs)e;
            if (args.Status == Status.Opened & !isRunning) Run();
            OnSerialPortStatusChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public static void Run()
        {
            plc.StartService();
            isRunning = true;
        }

       


    }
}
