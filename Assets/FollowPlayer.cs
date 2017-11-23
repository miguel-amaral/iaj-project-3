using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    public Transform Target;

    public float SmoothSpeed = 1f;
    public float Offset;
    public Vector3 RelativePosition;

    void FixedUpdate()
    {
        var desiredPos = Target.position + -Target.transform.forward * Offset + RelativePosition;
        var smoothPos = Vector3.Lerp(transform.position, desiredPos, SmoothSpeed);
        transform.position = smoothPos;

        transform.LookAt(Target);
    }

}
