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

    Rigidbody rb;
    float mass = 20000;
    float thrust = 20000 * 15;
    float density = 1.2f;
    float dragCoefBase = 0.025f;
    float liftCoefBase = 0.25f;
    float area = 100;
    float chord = 10;

    // Start is called before the first frame update
    void Start()
    {
        player = gameObject;
        string tempString = "RPC Sent";
        photonView.RPC("TestRPC", RpcTarget.All, tempString);

        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.velocity;
        Vector3 velNorm = rb.velocity.normalized;

        float aoa = Vector3.Angle(rb.velocity.normalized, transform.rotation * Vector3.forward);
        float aoaDot = Vector3.Dot(velNorm, transform.rotation * Vector3.back); //angle of attack where 1 is parallel and 0 is perpendicular.
        float dynamPress = (density * vel.sqrMagnitude) / 2;

        float dragCoef = dragCoefBase + -Mathf.Cos(2*Mathf.Deg2Rad*aoa) + 1;
        float drag = dragCoef * dynamPress * area;
        Vector3 dragDir = -velNorm;

        float liftCoef = 0.1f*aoa;
        if (liftCoef > 2)
        {
            liftCoef = 2;
        }
        float lift = liftCoef * dynamPress * area;
        Vector3 liftDir = Vector3.ProjectOnPlane(transform.rotation * Vector3.forward, velNorm);

        float momentCoef = 0.00004f * Mathf.Pow(aoa,2);
        float moment = momentCoef * dynamPress * area * chord;
        Vector3 momentDir = Vector3.Cross(transform.forward, velNorm);

        rb.AddForce(drag * dragDir);
        rb.AddForce(lift * liftDir);
        rb.AddForce(transform.rotation * Vector3.forward * thrust);
        rb.AddForce(mass * 9.8f * Vector3.forward);
        rb.AddTorque(moment * momentDir -20000*rb.angularVelocity.normalized*rb.angularVelocity.sqrMagnitude); // Combination of natural aerodynamic stabilization and computer-aided-flight stabilization.

        print(vel.magnitude);

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
                //rb.AddForce(Vector3.up * speed);
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
                //player.transform.Rotate(Vector3.right);
                rb.AddTorque(transform.right * 0.01f * dynamPress * area * chord);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                //player.transform.Rotate(Vector3.left);
                rb.AddTorque(-transform.right * 0.01f * dynamPress * area * chord);
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
