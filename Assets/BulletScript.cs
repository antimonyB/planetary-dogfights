using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    float speed = 2000;
    Vector3 prevPos;
    float startTime;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
        RaycastHit hit;
        Physics.Linecast(prevPos, transform.position, out hit);
        if (hit.collider.gameObject.tag == "Player")
        {
            print(hit.collider.gameObject.tag);
            //Destroy(gameObject,1);
        }
            Destroy(gameObject,5);
        prevPos = transform.position;
    }
}
