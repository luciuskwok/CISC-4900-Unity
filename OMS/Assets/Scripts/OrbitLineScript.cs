using UnityEngine;
using System.Collections;


public class OrbitLineScript : MonoBehaviour
{
	//public LineRenderer lineRenderer;
	private Camera mainCamera;
	private float lineFactor = 500.0f;

	void Start()
	{
		mainCamera = Camera.main;

		// Create an ellipse
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		int count = 64;
		lineRenderer.positionCount = count;

		var points = new Vector3[count];
		for (int i = 0; i < count; i++)
		{
			float a = (float)i / (float)count * 2.0f * Mathf.PI;
			points[i] = new Vector3(Mathf.Sin(a), 0.0f, Mathf.Cos(a));
		}

		lineRenderer.SetPositions(points);
	}

	void Update()
	{
		// Change the line width based on the distance from the camera to the closest point on the line.
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		float min = float.MaxValue;

		var points = new Vector3[lineRenderer.positionCount];
		int count = lineRenderer.GetPositions(points);
		Vector3 cameraPosition = mainCamera.transform.position;

		for (int i = 0; i < count; i++)
		{
			Vector3 pt = points[i];
			float distance = Vector3.Distance(pt, cameraPosition);
			min = (min <= distance) ? min : distance;
		}
		lineRenderer.startWidth = min / lineFactor;
		lineRenderer.endWidth = lineRenderer.startWidth;

		if (min < 0.1f)
		{
			Debug.Log("Distance:" + min + ", line width:" + lineRenderer.startWidth);

		}
	}
}
