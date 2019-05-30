using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient.Base
{
    public class MyFilter : TerminatorReceiveFilter<MessagePackage>
    {
        public MyFilter(): base(Encoding.UTF8.GetBytes("\r\n"))
        {

        }

        public override MessagePackage ResolvePackage(IBufferStream bufferStream)
        {
            var buf = bufferStream.Take(Convert.ToInt32(bufferStream.Length-2));
            string received = Encoding.UTF8.GetString(buf);
            int index=received.IndexOf(' ');
            MessagePackage messagePackage = new MessagePackage();
            messagePackage.Command = received.Substring(0, index);
            messagePackage.JsonParas = received.Substring(index+1, received.Length- index - 1);
            return messagePackage;
        }
    }

    public class MessagePackage : IPackageInfo
    {
        public string Command { get; set; }
        public string JsonParas { get; set; }


    }
}
