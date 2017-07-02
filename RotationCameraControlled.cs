/**
 * Controls Y rotation of an object based on a camera.
 * Forces the X and Z rotation of an object to be 0, but this can easily be prevented.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCameraControlled : MonoBehaviour {
    public float turnSpeed = 23f; // Smaller is more responsive.
	public float turnClamp = 0.5f; // Angle at which turn snaps to camera, otherwise lerp
    public Camera targetCam;

	void FixedUpdate()
	{
		if (targetCam != null) {
			Quaternion rotWant = Quaternion.Euler(0, targetCam.transform.eulerAngles.y, 0);

			if (Quaternion.Angle(transform.rotation, rotWant) > turnClamp) {
				transform.rotation = Quaternion.Slerp(transform.rotation, rotWant, Time.deltaTime * turnSpeed);
			} else {
				transform.rotation = rotWant;
			}

			/*
			A more static implementation. Polls for camera position then lerps there,
			then polls again.

			if (isRotating) {
				float timeSinceStart = Time.time - startTime;
				float percentComplete = timeSinceStart / turnResponsiveness;
				if (percentComplete > 1.0) {
					isRotating = false;
				}

				transform.rotation = Quaternion.Slerp(startRot, endRot, percentComplete);
			} else {
				startRot = transform.rotation;
				startTime = Time.time;

				isRotating = true;
			}
			*/
		}
	}
}
