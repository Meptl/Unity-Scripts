/**
 * Orbits the camera around a gameObject. Camera does not go through
 * objects in layer 8.
 */
using UnityEngine;
using System.Collections;

public class CameraOrbit : MonoBehaviour {
    public Vector2 sensitivityXY = new Vector2(2f, 2f);
    public float minDegreesY = -30f;
    public float maxDegreesY = 80f;
    public float minZoom = 1f;
    public float maxZoom = 5f;
    public float offsetWall = 0.5f; // Distance to stay away from a wall
	public Transform target;

    private float cameraSpeed = 10f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private float zoom;
    private float distance;
    private int layerMask;

    void Start ()
    {
        // Camera will only hit detect with layer 8
        layerMask = 1 << 8;
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
        zoom = maxZoom;
        distance = maxZoom;

    }

    void LateUpdate ()
    {
        if (target != null) {
            moveCamera();
        }
    }

    private void moveCamera()
    {
        // Desired zoom based on scrollwheel
        zoom = Mathf.Clamp (zoom - Input.GetAxis("Mouse ScrollWheel") * 5, minZoom, maxZoom);

        // Generate a desired rotation from current rotation
        rotationX += Input.GetAxisRaw("Mouse X") * sensitivityXY.x;
        rotationY -= Input.GetAxisRaw("Mouse Y") * sensitivityXY.y;
        rotationY = ClampAngle(rotationY, minDegreesY, maxDegreesY);

        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);

        // Direction * magnitude + location
        Vector3 posWant = rotation * new Vector3(0f, 0f, -zoom) + target.position;

        /* Some raycast hit detection on layer 8 to prevent camera from going into walls
         * distance = desired zoom
         */
        Vector3 hitDiff = new Vector3(0f, 0f, 0f);
        RaycastHit hit;
        if (Physics.Linecast(target.position, posWant, out hit, layerMask))
            distance = Mathf.Lerp(distance, hit.distance - offsetWall, 0.25f);
        else
            distance = Mathf.Lerp(distance, zoom, 0.1f);

        // Slerp to desired rotation and position
        posWant = rotation * new Vector3(0f, 0f, -(distance - offsetWall)) + target.position - hitDiff;
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, cameraSpeed);
        transform.position = Vector3.Lerp(transform.position, posWant, cameraSpeed);
    }

    /* Angle % 360;
    */
    private static float ClampAngle(float angle, float min, float max)
    {
        while(angle < -360f || angle > 360f){
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
        }
        return Mathf.Clamp(angle, min, max);
    }
}
