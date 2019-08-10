using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpawner : MonoBehaviour
{
    public OriginController originController;
    public GameObject[] buildingPrefabs;
    public int size;
    Vector3 spawnPos;
    Vector3 origin;
    Vector3 offset;
    GameObject[] buildings;
    // Start is called before the first frame update
    void Start()
    {
        buildings = new GameObject[size*size];

        transform.LookAt(originController.GetOrigin());
        transform.position = Quaternion.LookRotation(transform.forward) * new Vector3(0, 0, -10);
        transform.Rotate(new Vector3(-90, 0, 0),Space.Self);
        for (int i = 0; i < size*size; i++)
        {
            buildings[i] = GameObject.Instantiate(buildingPrefabs[Random.Range(0,buildingPrefabs.Length)], transform.position, transform.rotation);
            buildings[i].transform.localScale = new Vector3(Random.Range(10,20), Random.Range(5, 10), Random.Range(10, 20));
        }
    }

    // Update is called once per frame
    void Update()
    {
        origin = originController.GetOrigin();
        Vector3 clamp;
        ShapeSettings settings = originController.objects[0].GetComponent<Planet>().shapeSettings;
        ShapeGenerator temp = new ShapeGenerator();
        temp.UpdateSettings(settings);

        for (int i = 0; i < 25; i++) {
            Vector3 offset = Quaternion.LookRotation(-transform.position, transform.up) * new Vector3((i % size)*40, Mathf.Floor(i/size)*40, 0);

            buildings[i].transform.position = origin + transform.position * 10000 + offset;
            clamp = origin + (Quaternion.LookRotation(origin - buildings[i].transform.position) * new Vector3(0, 0, -100010)) * temp.GetScaledElevation(temp.CalculateUnscaledElevation(Quaternion.LookRotation(origin - buildings[i].transform.position) * new Vector3(0, 0, -1)));
            buildings[i].transform.position = clamp;
        }
    }
}
