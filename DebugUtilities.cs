using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugUtilities {
	private static float arrowSize = 0.1f;
	private static GameObject sphObj, vecObj;
	public static void debugSphere(Vector3 position, float radius) {
		if (sphObj == null) {
			sphObj = createMeshPrimitive(PrimitiveType.Sphere);
		}

		sphObj.transform.position = position;
		sphObj.transform.localScale = new Vector3(radius, radius, radius);
	}

	public static void debugVector(Vector3 origin, Vector3 vec) {
		if (vecObj == null) {
			vecObj = createMeshPrimitive(PrimitiveType.Cube);
		}

		vecObj.transform.position = origin;
		vecObj.transform.localScale = new Vector3(arrowSize, arrowSize, vec.magnitude);

		vecObj.transform.LookAt(origin + vec);
	}

	/**
	 * Creates a primitive with only a mesh (no collider).
	 */
	private static GameObject createMeshPrimitive(PrimitiveType type)
	{
		GameObject gameObject = new GameObject(type.ToString());
		Mesh mesh = getPrimitiveMesh(type);

		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = mesh;
		gameObject.AddComponent<MeshRenderer>();

		return gameObject;
	}

	/**
	 * Grabs the mesh from the primitive type.
	 */
	private static Mesh getPrimitiveMesh(PrimitiveType type)
	{
		// Grab the mesh from a sphere object
		GameObject tmpObj = GameObject.CreatePrimitive(type);
		Mesh mesh = tmpObj.GetComponent<MeshFilter>().sharedMesh;
		GameObject.Destroy(tmpObj);

		return mesh;
	}
}
