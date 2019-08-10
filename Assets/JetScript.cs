using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetScript : MonoBehaviour
{
    GameObject target;
    Vector3 targetPos;
    Vector3 targetLead;
    Rigidbody targetRB;

    public GameObject bullet;

    public OriginController originController;
    Vector3 origin = Vector3.zero;
    Vector3 prevOrigin = Vector3.zero;
    Vector3 offset;

    Rigidbody rb;
    float mass = 20000;
    float thrust = 200000;
    float density = 1.2f;
    float dragCoefBase = 0.025f;
    float liftCoefBase = 0.25f;
    float area = 100;
    float chord = 10;
    float bulletSpeed = 2000;
    float lastShot = 0;

    bool first = true;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 37000, -100000);
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (first)
        {
            target = GameObject.FindGameObjectWithTag("Player");
            targetRB = target.GetComponent<Rigidbody>();
            first = false;
        }

        origin = originController.GetOrigin();
        if (prevOrigin != origin)
        {
            transform.position += (origin - prevOrigin);
            prevOrigin = origin;
        }
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.velocity;
        Vector3 velNorm = rb.velocity.normalized;

        targetPos = target.transform.position;
        targetLead = targetPos;

        float aoa = Vector3.Angle(rb.velocity.normalized, transform.rotation * Vector3.forward);
        float aoaDot = Vector3.Dot(velNorm, transform.rotation * Vector3.back); //angle of attack where 1 is parallel and 0 is perpendicular.
        float dynamPress = (density * vel.sqrMagnitude) / 2;

        float dragCoef = dragCoefBase + -Mathf.Cos(2 * Mathf.Deg2Rad * aoa) + 1;
        float drag = dragCoef * dynamPress * area;
        Vector3 dragDir = -velNorm;

        float liftCoef = 0.1f * aoa;
        if (liftCoef > 2)
        {
            liftCoef = 2;
        }
        float lift = liftCoef * dynamPress * area;
        Vector3 liftDir = Vector3.ProjectOnPlane(transform.rotation * Vector3.forward, velNorm);

        float momentCoef = 0.00004f * Mathf.Pow(aoa, 2);
        float moment = momentCoef * dynamPress * area * chord;
        Vector3 momentDir = Vector3.Cross(transform.forward, velNorm);

        rb.AddForce(drag * dragDir);
        rb.AddForce(lift * liftDir);
        rb.AddForce(transform.rotation * Vector3.forward * thrust);
        rb.AddForce(mass * 9.8f * Vector3.forward);
        rb.AddTorque(moment * momentDir - 20000 * rb.angularVelocity.normalized * rb.angularVelocity.sqrMagnitude); // Combination of natural aerodynamic stabilization and computer-aided-flight stabilization.

        Vector3 bulletSpawn = transform.position + transform.forward * 10;
        targetLead = target.transform.position + targetRB.velocity * (Vector3.Distance(bulletSpawn, target.transform.position) / bulletSpeed);
        for (int i = 0; i < 5; i++)
        {
            targetLead = target.transform.position + targetRB.velocity * (Vector3.Distance(transform.position, targetLead) / bulletSpeed);
        }
        Debug.DrawLine(targetPos, targetLead);
        float angleDiff = Vector3.Angle(transform.forward, targetLead - transform.position);
        float angleAdjust = (angleDiff < 10) ? (angleDiff+10)/20 : 1;
        Vector3 rotDir = Vector3.Cross(transform.forward, targetLead - transform.position);
        rb.AddTorque(rotDir.normalized * angleDiff/360 * 0.01f * dynamPress * area * chord);

        if (angleDiff < 15f && Time.time - lastShot > 0.5f)
        {
            Instantiate(bullet, transform.position + transform.forward * 10, Quaternion.LookRotation(targetLead-(transform.position + transform.forward * 10)));
            lastShot = Time.time;
        }
    }
}
