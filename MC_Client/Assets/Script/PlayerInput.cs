using Newtonsoft.Json;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    public GameObject Wood;

    void DestroyCube()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rhit;
        if (Physics.Raycast(ray, out rhit, 7, LayerMask.GetMask("Cube")))
        {
            Vector3 pos = rhit.point + ray.direction.normalized * 0.1f;
            WorldModify wd = new WorldModify();
            wd.x = Mathf.RoundToInt(pos.x);
            wd.y = Mathf.RoundToInt(pos.y);
            wd.z = Mathf.RoundToInt(pos.z);
            wd.to = 0;
            //World.ins.modify(wd);
            UserClient.SendMessage(new Message("UpdateWorld",
                JsonConvert.SerializeObject(wd)));
        }
    }

    void PutCube(int cubeId)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rhit;
        if (Physics.Raycast(ray, out rhit, 7, LayerMask.GetMask("Cube")))
        {
            Vector3 pos = rhit.point - ray.direction.normalized * 0.1f;
            WorldModify wd = new WorldModify
            {
                x = Mathf.RoundToInt(pos.x),
                y = Mathf.RoundToInt(pos.y),
                z = Mathf.RoundToInt(pos.z),
                to = cubeId
            };
            if (wd.inside())
            {
                //World.ins.modify(wd);
                UserClient.SendMessage(new Message("UpdateWorld", JsonConvert.SerializeObject(wd)));
            }
        }
    }

    void Update()
    {
        if (GameManager.pause) return;
        if (Input.GetMouseButtonDown(0))
        {
            DestroyCube();
        }
        if (Input.GetMouseButtonDown(1))
        {
            PutCube(3);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            PutCube(2);
            //Shoot();
        }

    }
}
