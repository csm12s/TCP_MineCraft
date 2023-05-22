using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// MainMenuUI
/// </summary>
public class Title : MonoBehaviour
{
    public static Title Ins;

    public Text Name, HostIP, Port, Warn;
    public GameObject TitlePage, PausePage;
    public static bool inTitle = true;
    
    private void Awake()
    {
        Ins = this;
        TitlePage.transform.localScale = Vector3.one;
        PausePage.transform.localScale = Vector3.zero;
        inTitle = true;
        GameManager.pause = true;
    }

    /// <summary>
    /// 尝试开始游戏
    /// </summary>
    public void TryLink()
    {
        Debug.Log("TryLink");
        if (Name.text.Length <= 0)
        {
            Warn.text = "你需要一个名字！";
            return;
        }
        GameManager.playerName = Name.text;
        
        try
        {
            UserClient.Port = int.Parse(Port.text);
        }
        catch
        {
            Warn.text = "端口不对！";
            return;
        }
        UserClient.HostIP = HostIP.text;

        //请求连接，监听结果
        GameManager.playerInfo = new UserInfo(GameManager.playerName, GameManager.BothPosition, 0);
        Warn.text = "正在连接...";
        try
        {
            UserClient.Connect();
            GameManager.ins.Player.Start();
            Listener.StartListening();
        }
        catch (System.Exception e)
        {
            Warn.text = e.Message;
            UserClient.socket.Close();
            return;
        }

        Warn.text = "连接成功，正在登录...";
        try
        {
            string str = UserClient.SendVitalMessage(new Message("Loin", JsonConvert.SerializeObject(GameManager.playerInfo)), "LoinResult");
            if (str == null)
            {
                throw new System.Exception("登录超时");
            }
            else if (str != "Success")
            {
                throw new System.Exception(str);
            }
        }
        catch (System.Exception e)
        {
            Warn.text = e.Message;
            UserClient.socket.Close();
            return;
        }

        Warn.text = "正在加载世界...";
        try
        {
            string str = UserClient.SendVitalMessage(new Message("QueryAllWorld", ""),
                "UpdateWorldList", 6000);
            if (str == null)
            {
                throw new System.Exception("加载世界失败！");
            }
            else
            {
                Invoke("StartGame", 0.5f);
            }
        }
        catch (System.Exception e)
        {
            Warn.text = e.Message;
            UserClient.socket.Close();
            return;
        }
    }


    /// <summary>
    /// 在标题界面显示错误信息
    /// </summary>
    public void ShowWarn(string tx)
    {
        Warn.text = tx;
    }

    /// <summary>
    /// 游戏开始
    /// </summary>
    public void StartGame()
    {
        Debug.Log("StartGame");
        Warn.text = "";
        GameManager.pause = false;
        TitlePage.transform.localScale = Vector3.zero;
        PausePage.transform.localScale = Vector3.zero;
        inTitle = false;
        GameManager.ins.Player.Start();
    }

    /// <summary>
    /// 从暂停界面中脱离
    /// </summary>
    public void ConTinue()
    {
        Debug.Log("ConTinue");
        GameManager.pause = false;
        PausePage.transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// 返回标题界面
    /// </summary>
    public void BackToTitle()
    {
        Debug.Log("BackToTitle");
        inTitle = true;
        TitlePage.transform.localScale = Vector3.one;
        PausePage.transform.localScale = Vector3.zero;
        GameManager.pause = true;
        UserClient.SendMessage(new Message("Logout", ""));
        UserClient.socket.Close();
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void Exit()
    {
        Debug.Log("Exit");
        UserClient.socket.Close();
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.pause = true;
            PausePage.transform.localScale = Vector3.one;
        }
        Cursor.lockState = GameManager.pause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = GameManager.pause;
    }
}
