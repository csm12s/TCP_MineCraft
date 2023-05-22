using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Collider m_Collider;
    Rigidbody m_Rigidbody;

    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        //if (other.collider.gameObject.layer 
        //    == LayerMask.NameToLayer("Cube"))
        if(other.collider.CompareTag("Cube"))
        {
            Destroy(other);
        }
        else 
        {
            Physics.IgnoreCollision(m_Collider, other.collider);
        }

    }

    private void Destroy(Collision other)
    {
        Vector3 pos = transform.position + m_Rigidbody.velocity.normalized * 0.2f;
        WorldModify modify = new WorldModify
        {
            x = Mathf.RoundToInt(pos.x),
            y = Mathf.RoundToInt(pos.y),
            z = Mathf.RoundToInt(pos.z),
            to = 0
        };

        UserClient.SendMessage(new Message("UpdateWorld",
            JsonConvert.SerializeObject(modify)));

        Debug.Log("bullet hit");
        Destroy(gameObject);
    }
}

