using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void FixedUpdate()
    {
        var desiredPos = target.position + offset;
        var smoothPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        transform.position = smoothPos;

        transform.LookAt(target);
    }


    // Use this for initialization
 //   void Start ()
 //   {
 //       target = GameObject.FindGameObjectWithTag("Player").transform;
 //       offset = new Vector3(-3 , 3 , 0);
 //   }
	
	//// Update is called once per frame
	//void Update () {
	//    transform.position = target.position + offset;
 //   }
}
