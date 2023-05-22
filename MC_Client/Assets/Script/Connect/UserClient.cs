using Newtonsoft.Json;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// �ͷ����ͨ�ŵ���Ϣ�࣬�洢�������ͺ;�����Ϣ
/// </summary>
public class Message
{
    public string type;
    public string info;
    public Message(string a, string b)
    {
        type = a;
        info = b;
    }
}

public class UserClient : MonoBehaviour
{
    public static string HostIP;
    public static int Port;
    public static Socket socket;

    //�Ƿ�ͨ
    public static bool isAvailable() { return socket != null && socket.Connected; }

    /// <summary>
    /// ���ӷ�����
    /// </summary>
    public static void Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(HostIP, Port);
    }

    /// <summary>
    /// �����˷���һ����Ϣ
    /// </summary>
    public static void SendMessage(Message msg)
    {
        string info = JsonConvert.SerializeObject(msg) + "&";
        byte[] bytes = System.Text.Encoding.Default.GetBytes(info);
        if (isAvailable()) 
            socket.Send(bytes);
    }

    /// <summary>
    /// ������Ҫ��Ϣ��Ҫ��һ��ʱ���ڱ���õ�ĳ�ֻظ�������������
    /// </summary>
    /// <param name="waitType">�ȴ���Ϣ��type</param>
    /// <param name="maxWait">���ȴ�ʱ��</param>
    /// <returns>�ظ�����</returns>
    public static string SendVitalMessage(Message msg, string waitType, int maxWait = 3000)
    {
        string info = JsonConvert.SerializeObject(msg) + "&";
        byte[] bytes = System.Text.Encoding.Default.GetBytes(info);

        Listener.waiting[waitType] = "";
        if (isAvailable()) 
            socket.Send(bytes);

        while (maxWait > 0)
        {
            Thread.Sleep(50);
            maxWait -= 50;
            if (Listener.waiting[waitType] != "")
            {
                string res = Listener.waiting[waitType];
                Listener.waiting.Remove(waitType);
                return res;
            }
        }
        return null;
    }
}
