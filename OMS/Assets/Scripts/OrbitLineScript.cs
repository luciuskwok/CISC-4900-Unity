using UnityEngine;
using System.Collections;


public class OrbitLineScript : MonoBehaviour
{
	//public LineRenderer lineRenderer;

	void Start()
	{
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


		//Vector3.Distance()
	}
}
