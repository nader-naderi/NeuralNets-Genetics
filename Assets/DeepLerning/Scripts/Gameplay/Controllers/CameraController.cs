using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    float speed;

    public Vector3 Offset;
    // change this value to get desired smoothness
    public float SmoothTime = 0.3f;

    // This value will change at the runtime depending on target movement. Initialize with zero vector.
    private Vector3 velocity = Vector3.zero;

    private void Update()
    {

        Vector3 targetPosition = target.position + Offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, SmoothTime);
        transform.LookAt(target);
        // if (Input.GetMouseButton(0))
        // {
        //     transform.LookAt(target);
        //     transform.RotateAround(target.position, Vector3.up, Input.GetAxis("Mouse X") * speed);
        // }
        //else
        // {
        //     transform.LookAt(target);
        // }
    }


    public void ResetEverything(Transform t)
    {
        Offset = transform.position - target.position;
        transform.position = t.position;
        transform.rotation = t.rotation;
    }
}
