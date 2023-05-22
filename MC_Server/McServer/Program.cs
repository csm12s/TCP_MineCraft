using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading;
using MyWorld;
using System.IO;
using McServer.Dtos;

namespace MyServer
{
    class Program
    {
        const int PortIP = 8888;  
        const int MAX_CONNECT = 20;

        // world info
        static World world;        

        static List<Socket> userOnline = new List<Socket>();
        static List<UserInfo> allPlayer = new List<UserInfo>();
        static List<bool> available = new List<bool>();

        #region Main thread
        static void Main(string[] args)
        {
            Console.WriteLine("input map name to open or create:");
            string path = Console.ReadLine();

            // new world
            world = new World("map/" + path + ".txt");
            
            // sub thread
            Thread auto = new Thread(new ThreadStart(thread_autoWork));
            auto.Start();

            // wait for clients
            Thread waiter = new Thread(new ThreadStart(thread_WaitAllClients));
            waiter.Start();

            HandleTodos();
        }

        // handle in main thread
        private static void HandleTodos()
        {
            while (true)
            {
                if (_todoList.Count > 0)
                {
                    Socket from;
                    Message msg;
                    lock (_todoListLock)
                    {
                        from = _todoList.Peek().Key;
                        msg = _todoList.Dequeue().Value;
                    }

                    switch (msg.type)
                    {
                        case "Loin":
                            userLoin(from, msg.info);
                            break;
                            
                        case "Logout":
                            userLogout(from);
                            break;
                            
                            //update user position
                        case "UpdateUser":
                            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(msg.info);
                            int idx = allPlayer.FindIndex(delegate (UserInfo pl) { return pl.playername == userInfo.playername; });
                            if (idx != -1)
                            {
                                allPlayer[idx] = userInfo;
                                available[idx] = true;
                            }
                            break;
                            
                        case "UpdateWorld":
                            WorldModify wd = JsonConvert.DeserializeObject<WorldModify>(msg.info);
                            world.modify(wd);
                            sendToAllClient(msg);
                            break;
                        
                            //get world on login
                        case "QueryAllWorld":
                            sendToClient(from, new Message("UpdateWorldList", JsonConvert.SerializeObject(world.GetWorldModifyList())));
                            break;
                        
                        case "UpdateAllUser":
                            sendToAllClient(new Message("UpdateAllUser", JsonConvert.SerializeObject(allPlayer)));
                            break;
                        
                        case "ClearUser":
                            List<Socket> tmp = new List<Socket>();
                            for (int i = 0; i < available.Count; i++)
                            {
                                if (!available[i])
                                {
                                    string clientIP = ((IPEndPoint)userOnline[i].RemoteEndPoint).Address.ToString();
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(clientIP + " not connect for too long\n");
                                    tmp.Add(userOnline[i]);
                                }
                                available[i] = false;
                            }
                            foreach (Socket i in tmp)
                            {
                                userLogout(i);
                            }
                            break;
                    }
                }
            }
        }
        #endregion

        #region Sub Threads
        static void thread_WaitAllClients()
        {
            Socket serverSK = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSK.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), PortIP));

            //开启监听（注意这只是一个开启，不是阻塞方法，参数为最大连接的客户端数量）
            serverSK.Listen(MAX_CONNECT);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nServer is working..");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(DateTime.Now.ToString() + '\n');
            
            while (true)
            {
                Socket clientSK = serverSK.Accept();
                // create a new sub thread
                Thread subThread = new Thread(new ParameterizedThreadStart(ListenClient));
                subThread.Start(clientSK);
            }
        }

        static void ListenClient(object obj)
        {
            Socket clientSK = (Socket)obj;

            byte[] readBuffs = new byte[1024];
            while (isAvailable(clientSK))
            {
                try
                {
                    // hold
                    int count = clientSK.Receive(readBuffs);// Socket.Receive是阻塞方法，等待直到客户端发送数据
                    
                    string readStrList = Encoding.UTF8.GetString(readBuffs, 0, count);
                    foreach (string readStr in readStrList.Split('&'))
                    {
                        if (readStr.Length > 0)
                        {
                            Message recMsg = JsonConvert.DeserializeObject<Message>(readStr);
                            // main thread to handle client req
                            lock (_todoListLock)
                            {
                                _todoList.Enqueue(new KeyValuePair<Socket, Message>(clientSK, recMsg));
                            }
                        }
                    }
                }
                catch
                {
                    // 直接socket.close()来终止这个子线程
                    break;
                }
            }
        }
        #endregion

        #region funcs

        /// <summary>
        /// 这个Socket是否可用
        /// </summary>
        static bool isAvailable(Socket x) { return x != null && x.Connected; }

        /// <summary>
        /// 主动向客户端发送消息
        /// </summary>
        static void sendToClient(Socket sclient, Message msg)
        {
            try
            {
                byte[] bytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(msg) + '&');
                sclient.Send(bytes);
            }
            catch (SocketException)
            {
                string clientIP = ((IPEndPoint)sclient.RemoteEndPoint).Address.ToString();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(clientIP + " 失去连接\n");
                userLogout(sclient);
                return;
            }
        }

        /// <summary>
        /// 向所有客户端广播消息
        /// </summary>
        static void sendToAllClient(Message msg)
        {
            for (int i = 0; i < userOnline.Count; i++)
            {
                if (isAvailable(userOnline[i]))
                    sendToClient(userOnline[i], msg);
            }
        }

        /// <summary>
        /// 处理用户登录请求，再向其发送允许登录或失败消息
        /// </summary>
        static void userLoin(Socket sclient, string info)
        {
            //Loin是每个用户登录时发送的第一个请求，根据Loin结果为其建立存储信息
            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(info);
            string clientIP = ((IPEndPoint)sclient.RemoteEndPoint).Address.ToString();
            int clientPort = ((IPEndPoint)sclient.RemoteEndPoint).Port;

            int idx = allPlayer.FindIndex(delegate (UserInfo cl) { return cl.playername == userInfo.playername; });
            if (idx != -1)
            {
                sendToClient(sclient, new Message("LoinResult", userInfo.playername + " has already in this game!"));
                sclient.Close();
            }
            else
            {
                sendToClient(sclient, new Message("LoinResult", "Success"));
                userOnline.Add(sclient);
                allPlayer.Add(userInfo);
                available.Add(true);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("Name:{0} Loined.\nIP:{1} Port:{2}", userInfo.playername, clientIP, clientPort));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(DateTime.Now.ToString() + '\n');
            }
        }

        /// <summary>
        /// 处理用户登出
        /// </summary>
        static void userLogout(Socket sclient)
        {
            string clientIP = ((IPEndPoint)sclient.RemoteEndPoint).Address.ToString();
            int clientPort = ((IPEndPoint)sclient.RemoteEndPoint).Port;
            int idx = userOnline.FindIndex(delegate (Socket cl) { return cl == sclient; });
            if (idx == -1) return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Format("Name:{0} Logout.\nIP:{1} Port:{2}", allPlayer[idx].playername, clientIP, clientPort));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(DateTime.Now.ToString() + '\n');

            sendToAllClient(new Message("Logout", allPlayer[idx].playername));//广播此条登出信息

            userOnline.RemoveAt(idx);
            allPlayer.RemoveAt(idx);
            available.RemoveAt(idx);

            sclient.Close();
        }
        #endregion

        #region thread auto task
        /// <summary>
        /// 子线程通过toDoList将请求转交给主线程完成，避免异步冲突
        /// </summary>
        static Queue<KeyValuePair<Socket, Message>> _todoList = new Queue<KeyValuePair<Socket, Message>>();
        private static readonly object _todoListLock = new object();

        /// <summary>
        /// 定期执行一些事务的子线程
        /// </summary>
        static void thread_autoWork()
        {
            // 0.01s, 100 fps
            // 0.04
            var updateInterval = 40;
            // 5s
            var autoSaveInterval = 5000;

            int time = 0;
            while (true)
            {
                Thread.Sleep(updateInterval);
                time = (time + updateInterval) % 100000;

                lock (_todoListLock)
                {
                    if (time % updateInterval == 0)
                    {
                        _todoList.Enqueue(new KeyValuePair<Socket, Message>
                            (null, new Message("UpdateAllUser", "")));
                    }

                    if (time % autoSaveInterval == 0)
                    {
                        world.SaveWorld();
                        _todoList.Enqueue(new KeyValuePair<Socket, Message>
                            (null, new Message("ClearUser", "")));
                    }
                }
            }
        }
        #endregion



    }
}
