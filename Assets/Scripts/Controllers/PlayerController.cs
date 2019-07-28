using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    Camera playerCam;
    GameObject player;
    OriginController originController;
    Vector3 sharedPos = new Vector3(0, 37000, -101000);
    Quaternion sharedRot = Quaternion.identity;
    int shareCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = gameObject;
        string tempString = "RPC Sent";
        photonView.RPC("TestRPC", RpcTarget.All, tempString);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            float speed = 1f;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = 10;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                speed = 100;
            }

            if (Input.GetKey(KeyCode.W))
            {
                player.transform.Translate(Vector3.forward * speed);
            }
            if (Input.GetKey(KeyCode.S))
            {
                player.transform.Translate(Vector3.back * speed);
            }
            if (Input.GetKey(KeyCode.A))
            {
                player.transform.Translate(Vector3.left * speed);
            }
            if (Input.GetKey(KeyCode.D))
            {
                player.transform.Translate(Vector3.right * speed);
            }
            if (Input.GetKey(KeyCode.Space))
            {
                player.transform.Translate(Vector3.up * speed);
            }
            if (Input.GetKey(KeyCode.C))
            {
                player.transform.Translate(Vector3.down * speed);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                player.transform.Rotate(Vector3.forward);
            }
            if (Input.GetKey(KeyCode.E))
            {
                player.transform.Rotate(Vector3.back);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                player.transform.Rotate(Vector3.right);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                player.transform.Rotate(Vector3.left);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                player.transform.Rotate(Vector3.down);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                player.transform.Rotate(Vector3.up);
            }
            if (shareCounter > 4)
            {
                shareCounter = 0;
            }
            if (shareCounter == 0)
            {
                photonView.RPC("SharePosition", RpcTarget.All, transform.position - originController.GetOrigin(), transform.rotation);
            }
        }
        else // photonView is not mine, this is not the local player.
        {
            print("origin: " + originController.GetOrigin());
            transform.position = originController.GetOrigin() + sharedPos;
            transform.rotation = sharedRot;
        }

    }

    [PunRPC]
    void TestRPC(string tempString)
    {
        if (this.photonView.IsMine)
        {
            Debug.Log(tempString);
        }
    }

    [PunRPC]
    void SharePosition(Vector3 pos, Quaternion rot)
    {
        this.sharedPos = pos;
        this.sharedRot = rot;
    }

    [PunRPC]
    void OnMySpawn() //Called when I spawn as a remote player.
    {
        //Find the local originController and assign it to myself.
        originController = GameObject.FindGameObjectWithTag("OriginController").GetComponent<OriginController>();
    }


    public void SetOriginController(OriginController o)
    {
        this.originController = o;
    }
}
