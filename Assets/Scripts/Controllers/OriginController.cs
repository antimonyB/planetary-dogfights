using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginController : MonoBehaviour
{

    public GameObject[] cameras;
    GameObject playerCam;
    GameObject player;
    public GameObject[] objects;
    public Planet planet;
    int lastCam = 4;
    Vector3 origin = Vector3.zero;
    Vector3 playerCamPos;
    Vector3[] generationPos = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
    float originThreshold = 10000f;
    float[] generationThreshold = new float[] { 2000f, 10000f, 10000f };
    float[] generationAltitude = new float[] { 101000f, 110000f, 150000f };
    float altitude;

    public GameController gameController;

    public void UnifyPositions()
    {
        Vector3 outerDist = cameras[3].transform.position;
        for (int i = 0; i < lastCam-1; i++)
        {
            cameras[i].transform.position = ((cameras[lastCam - 1].transform.position - origin) / 10000) * Mathf.Pow(10, i);
            cameras[i].transform.rotation = cameras[lastCam - 1].transform.rotation ;
        }
    }
    public void CalculateOrigin()
    {
        origin -= cameras[lastCam - 1].transform.position;
        for (int i = 0; i < 3; i++)
        {
            generationPos[i] -= cameras[lastCam - 1].transform.position;
        }
        player.transform.position = Vector3.zero; // use (transform.root.position = -transform.position-transform.root.position) if the camera is a child
        //player.GetComponent<PlayerController>().SetOrigin(origin);
        objects[lastCam - 1].transform.position = origin;
        gameController.MoveCam();
    }
    public void ResetOrigin()
    {
        origin = Vector3.zero;
        objects[lastCam - 1].transform.position = Vector3.zero;
        player.transform.position = new Vector3(0, 37000, -101000);
    }

    private void Start()
    {
        playerCam = cameras[lastCam - 1];
        objects[0].GetComponent<Planet>().GeneratePlanet();
    }
    private void Update()
    {
        /*if (true)
        {
            playerCam.transform.rotation = Quaternion.LookRotation(cameras[lastCam-1].transform.position + origin, Vector3.up);
        }*/
        UnifyPositions();
        playerCamPos = playerCam.transform.position;
        if (playerCamPos.sqrMagnitude > Mathf.Pow(originThreshold, 2))
        {
            CalculateOrigin();
            //print("origin");
        }
        playerCamPos = playerCam.transform.position;
        altitude = Vector3.Distance(origin, playerCamPos);
        for (int i = 0; i < 3; i++)
        {
            if (altitude < generationAltitude[i] && Vector3.Distance(playerCamPos, generationPos[i]) > generationThreshold[i]) //if far enough from previous generationPos with low enough altitude
            {
                objects[lastCam  - 1 - i].GetComponent<Planet>().GenerateLOD();
                generationPos[i] = playerCamPos;
                //Debug.Log(generationThreshold[i]);
            }
        }
    }
    private void FixedUpdate()
    {
        gameController.MoveCam();
        //print(Vector3.Distance(playerCam.transform.position, origin));
        ShapeSettings settings = objects[0].GetComponent<Planet>().shapeSettings;
        ShapeGenerator temp = new ShapeGenerator();
        temp.UpdateSettings(settings);
        Vector3 clamp = Vector3.zero;
        /*if (Vector3.Distance(playerCam.transform.position, origin) < ((Quaternion.LookRotation(playerCam.transform.position - origin, Vector3.up) * new Vector3(0, 0, 100000 * 1.00005F)) * temp.GetScaledElevation(temp.CalculateUnscaledElevation((Quaternion.LookRotation(playerCam.transform.position - origin, Vector3.up) * new Vector3(0, 0, 1))))).magnitude-1)
        {
          clamp = origin + (Quaternion.LookRotation(playerCam.transform.position - origin, Vector3.up) * new Vector3(0, 0, 100000 * 1.00005F)) * temp.GetScaledElevation(temp.CalculateUnscaledElevation((Quaternion.LookRotation(playerCam.transform.position - origin, Vector3.up) * new Vector3(0, 0, 1))));
        }
        else
        {
            clamp = playerCam.transform.position;
        }
        playerCam.transform.position = clamp;// */
    }

    public void SetPlayer(GameObject newPlayer)
    {
        this.player = newPlayer;
    }

    public Vector3 GetOrigin()
    {
        return origin;
    }
}
