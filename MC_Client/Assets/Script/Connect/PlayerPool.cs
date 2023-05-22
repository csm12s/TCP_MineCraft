using System.Collections.Generic;
using UnityEngine;

public class PlayerPool : MonoBehaviour
{
    //已经存在的玩家模型
    static Dictionary<string, GameObject> playerModels = new Dictionary<string, GameObject>();

    public static void Remove(string playername)
    {
        if (playername == GameManager.playerName && !Title.inTitle)
        {
            Title.Ins.ShowWarn("你网不好");
            Title.Ins.BackToTitle();
        }
        else if (playerModels.ContainsKey(playername))
        {
            Destroy(playerModels[playername]);
            playerModels.Remove(playername);
        }
    }

    public static void UpdatePlayers(List<UserInfo> allPlayer)
    {
        foreach (UserInfo player in allPlayer)
        {
            // self
            if (player.playername == GameManager.playerName)
            {
                GameManager.playerInfo = new UserInfo(GameManager.playerName,
                    new Vector3(player.px, player.py, player.pz), 
                    0);// todo
            }
            // others
            else
            {
                // create new
                if (!playerModels.ContainsKey(player.playername))
                {
                    playerModels[player.playername] = Instantiate(GameManager.ins.playerModel);

                    var nameTag = playerModels[player.playername].transform.Find("Name");
                    nameTag.GetComponent<TextMesh>().text = player.playername;

                    //nameTag.LookAt(Global.playerInfo.GetPosition(), Vector3.up);    
                }

                // update position
                var playerObj = playerModels[player.playername];
                playerObj.transform.position = new Vector3(player.px, player.py, player.pz);
                playerObj.transform.eulerAngles = new Vector3(0, player.rh, 0);
            }
        }
    }
}
