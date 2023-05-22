using System;
using UnityEngine;

/// <summary>
/// 存储玩家的位置和方向信息
/// </summary>
public class UserInfo
{
    public string playername;
    public float px, py, pz;
    public float rh;
    public UserInfo(string name, Vector3 pos, float ang)
    {
        playername = name;
        px = pos.x;
        py = pos.y;
        pz = pos.z;
        rh = ang;
    }



    internal Vector3 GetPosition()
    {
        return  new Vector3(px, py, pz);
    }
}

public class GameManager : MonoBehaviour
{
    public GameObject CubeBox; // world root
    public static string playerName = "test";
    public Player1 Player;
    public static UserInfo playerInfo;
    public static int maxX = 45, maxY = 15, maxZ = 45;
    public static Vector3 BothPosition = new Vector3(20, 20, 20);

    public GameObject playerModel;//玩家模型
    public GameObject[] cubePrefab = new GameObject[5];

    public static bool pause = true;

    public static GameManager ins;
    private void Awake()
    {
        ins = this;
    }
}
