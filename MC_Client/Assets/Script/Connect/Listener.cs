using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Listener : MonoBehaviour
{
    public Text Warn;

    /// <summary>
    /// 一个侦听表，在侦听到特定message时，将消息内容填入
    /// </summary>
    public static Dictionary<string, string> waiting = new Dictionary<string, string>();

    /// <summary>
    /// 启动本地对服务器的监听
    /// </summary>
    public static void StartListening()
    {
        Thread Listening = new Thread(new ThreadStart(Listen));
        Listening.Start();
    }

    static byte[] readBuff = new byte[1024000];

    //监听，子线程
    static void Listen()
    {
        while (UserClient.isAvailable())
        {
            try
            {
                int count = UserClient.socket.Receive(readBuff);
                string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
                
                foreach (string s in str.Split('&'))
                {
                    if (s.Length > 0)
                    {
                        Message recv = JsonConvert.DeserializeObject<Message>(s);
                        //转到主线程中执行监听到的消息
                        _todolist.Enqueue(recv);
                        if (waiting.ContainsKey(recv.type))
                            waiting[recv.type] = recv.info;
                    }
                }
            }
            catch (SocketException)
            {
                break;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        Thread.Sleep(1000);
        if (!Title.inTitle)
        {
            _todolist.Enqueue(new Message("ExitGame", "Server down"));
        }
    }

    /// <summary>
    /// Listener将待处理的Message通过此队列转交给主进程执行
    /// </summary>
    static Queue<Message> _todolist = new Queue<Message>();
    private void Update()
    {
        if (_todolist.Count > 0)
        {
            Message recv = _todolist.Dequeue();
            switch (recv.type)
            {
                case "Logout":
                    PlayerPool.Remove(recv.info);
                    break;

                    //世界被修改
                case "UpdateWorld":
                    WorldModify wd = JsonConvert.DeserializeObject<WorldModify>(recv.info);
                    World.Ins.modify(wd);
                    break;
                
                case "UpdateAllUser":
                    var listinfo = JsonConvert.DeserializeObject<List<UserInfo>>(recv.info);
                    PlayerPool.UpdatePlayers(listinfo);
                    break;
                
                case "UpdateWorldList":
                    World.Ins.clear();
                    List<WorldModify> allmodi = JsonConvert.DeserializeObject<List<WorldModify>>(recv.info);
                    foreach (var i in allmodi)
                    {
                        World.Ins.modify(i);
                    }
                    break;
                
                case "ExitGame":
                    Title.Ins.ShowWarn(recv.info);
                    Title.Ins.BackToTitle();
                    break;
            }
        }
    }
}