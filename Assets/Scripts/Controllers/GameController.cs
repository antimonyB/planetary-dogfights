using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;


public class GameController : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    GameObject myPlayer;
    public GameObject playerCam; //Camera that sees the closest and defines the other cameras positions.

    public OriginController originController;
    PlayerController playerController;

    bool isOCSet = false;

    // Start is called before the first frame update
    void Start()
    {
        Spawn();
        playerController = myPlayer.GetComponent<PlayerController>();
        originController.SetPlayer(myPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        MoveCam();
    }

    private void FixedUpdate()
    {
        if (!isOCSet)
        {   //Assign all player objects the local origin controller.
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                p.GetComponent<PlayerController>().SetOriginController(originController);
            }
            isOCSet = true;
        }

    }

    void Spawn()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'GameController'", this);
        }
        else
        {
            Debug.LogFormat("We are Instantiating a local player");
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            myPlayer = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 37000, -101000), Quaternion.identity, 0);
            //myPlayer = GameObject.Instantiate(playerPrefab, new Vector3(0, 37000, -101000), Quaternion.identity);
            myPlayer.GetPhotonView().RPC("OnMySpawn", RpcTarget.Others);
        }

    }

    public void MoveCam()
    {
        playerCam.transform.rotation = myPlayer.transform.rotation;
        playerCam.transform.position = myPlayer.transform.position + myPlayer.transform.rotation * (Vector3.back * 5);
    }

}
