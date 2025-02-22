using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;

public class CameraController : MonoBehaviour
{
	public float distanceMax = 88000.0f;	// * 10^6 km
	public float panSpeed = 20.0f;			// degrees per second
	public float scrollSpeed = 20.0f;		// distance per scroll wheel unit depends on distance from object

	private Vector3 lastMousePosition;
	private GameObject[] targets;
	private int targetIndex = 0;
 
	void Start()
	{
		targets = GameObject.FindGameObjectsWithTag("Targetable");
		if (targets.Length == 0)
		{
			Debug.Log("No GameObjects with tag 'Targetable' found.");
		}
	}

	private GameObject CurrentViewTarget()
	{
		if (targets.Length == 0) return null;
		if (targetIndex < 0 || targetIndex >= targets.Length) return null;
		return targets[targetIndex];
	}

	private void PanTiltCamera(float deltaX, float deltaY)
	{
		GameObject viewTarget = CurrentViewTarget();

		if (viewTarget)
		{
			Vector3 targetPosition = viewTarget.transform.position;
			Vector3 cameraPosition = transform.position;
			float distance = Vector3.Distance(targetPosition, cameraPosition);
			Vector3 direction = (cameraPosition - targetPosition).normalized;

			float latDeg = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
			float lonDeg = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

			// Normalize latDeg to range from -180 to +180 degrees
			if (latDeg > 180.0f) latDeg -= 360.0f;

			latDeg += deltaY;
			lonDeg += deltaX;

			// Debug.Log("Lat:" + latDeg + " Lon:" + lonDeg + " X:" + cameraPosition.x + " Y:" + cameraPosition.y + " Z:" + cameraPosition.z);

			// Clamp lat values to min and max
			latDeg = (latDeg > 89.9f) ? 89.9f : latDeg;
			latDeg = (latDeg < -89.9f) ? -89.9f : latDeg;

			// Convert to radians
			float latRad = latDeg * Mathf.Deg2Rad;
			float lonRad = lonDeg * Mathf.Deg2Rad;

			direction.y = Mathf.Sin(latRad);
			direction.z = Mathf.Cos(latRad) * Mathf.Sin(lonRad);
			direction.x = Mathf.Cos(latRad) * Mathf.Cos(lonRad);

			// Set new camera position
			transform.position = targetPosition + direction * distance;

			// Rotate camera to face target
			transform.LookAt(viewTarget.transform);
		}
	}

	private void DollyInCamera(float delta)
	{
		GameObject viewTarget = CurrentViewTarget();
		if (viewTarget)
		{
			Vector3 targetPosition = viewTarget.transform.position;
			Vector3 cameraPosition = transform.position;
			float distance = Vector3.Distance(targetPosition, cameraPosition);
			Vector3 direction = (cameraPosition - targetPosition).normalized; 

			// Change the distance based on scroll wheel movement
			distance = Mathf.Pow(10.0f, Mathf.Log10(distance) - delta * 0.2f);

			// Clamp the values to min and max
			float distanceMin = viewTarget.transform.localScale.x;
			distance = (distance < distanceMin) ? distanceMin : distance;
			distance = (distance > distanceMax) ? distanceMax : distance;

			// Set new camera position
			transform.position = targetPosition + direction * distance;
		}

		// Debug.Log("Dolly: " + delta);
	}

	void Update()
	{
		// Shift key
		bool isShifted = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		float speed = isShifted ? 0.1f : 1.0f;

		// Mouse movement
		if (Input.GetMouseButtonDown(0))
		{
			lastMousePosition = Input.mousePosition;
		} else if (Input.GetMouseButton(0))
		{
			Vector3 delta = Input.mousePosition - lastMousePosition;
			if (delta.x != 0.0f || delta.y != 0.0f)
			{
				speed = speed * 0.1f; // Slow down pan and tilt
				PanTiltCamera(-delta.x * speed, -delta.y * speed);
			}
			lastMousePosition = Input.mousePosition;
		}

		// Scroll wheel
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll != 0.0f)
		{
			DollyInCamera(scroll * speed);
		}

		// Keyboard
		float movement = Time.deltaTime * 30.0f * speed; // About 0.5 degrees per second
		if (Input.GetKey(KeyCode.UpArrow))
		{
			PanTiltCamera(0.0f, movement);
		} else if (Input.GetKey(KeyCode.DownArrow))
		{
			PanTiltCamera(0.0f, -movement);
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			PanTiltCamera(-movement, 0.0f);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			PanTiltCamera(movement, 0.0f);
		}
	}
}
