using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServer.Dtos
{
    /// <summary>
    /// 和服务端通信的消息类，存储请求类型和具体信息
    /// </summary>
    public class Message
    {
        public string type;
        public string info;
        public Message(string type, string info)
        {
            this.type = type;
            this.info = info;
        }
    }

}
