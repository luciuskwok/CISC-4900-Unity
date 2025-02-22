using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;

public class CameraController : MonoBehaviour
{
	public GameObject targetsGroup;

	public float distanceMax = 88000.0f;	// * 10^6 km
	public float panSpeed = 20.0f;			// degrees per second
	public float scrollSpeed = 20.0f;		// distance per scroll wheel unit depends on distance from object

	private Vector3 lastMousePosition;
	private GameObject[] targets;
	private int targetIndex = 0;
 
	void Start()
	{
		//targets = GameObject.FindGameObjectsWithTag("Targetable");
		Transform parent = targetsGroup.transform;
		int count = parent.childCount;
		targets = new GameObject[count];
		for (int i = 0; i < count; i++)
		{
			targets[i] = parent.GetChild(i).gameObject;
		}

		if (count == 0)
		{
			Debug.Log("No children found in targetsGroup.");
		} else
		{
			// Point camera at target
			SetTargetAtIndex(0);
		}
	}

	private GameObject CurrentViewTarget()
	{
		if (targetIndex < 0 || targetIndex >= targets.Length) { Debug.Log("targetIndex is out of range of targets[] array!"); return null; }
		return targets[targetIndex];
	}

	private void SetTargetAtIndex(int index)
	{
		if (index < 0 || index >= targets.Length) { Debug.Log("Target index is out of range of targets[] array!"); return; }

		// Save old target so that distance and direction stay the same when switching targets
		GameObject oldTarget = CurrentViewTarget();
		targetIndex = index;
		GameObject newTarget = CurrentViewTarget();

		Vector3 cameraPosition = transform.position;
		Vector3 relativePosition = cameraPosition - oldTarget.transform.position;

		// Check for minimum distance
		float minDistance = newTarget.transform.localScale.x * 2.0f;
		float distance = Vector3.Magnitude(relativePosition);
		if (distance < minDistance) distance = minDistance;

		// Set new camera position
		Vector3 direction = relativePosition.normalized;
		transform.position = newTarget.transform.position + direction * distance;

		// Rotate camera to face target
		transform.LookAt(newTarget.transform);
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

	void SwitchTarget(int offset)
	{
		// Move through target array by offset, with wrap around
		int index = targetIndex + offset;
		while (index < 0) index += targets.Length;
		while (index >= targets.Length) index -= targets.Length;
		SetTargetAtIndex(index);
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
		// -- Arrow Keys: pan & tilt camera
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
		// -- Plus ('=' or '+') / Minus ('-' or '_'): dolly in/out
		else if (Input.GetKey(KeyCode.Minus))
		{
			DollyInCamera(-movement * 0.2f);
		} else if (Input.GetKey(KeyCode.Equals))
		{
			DollyInCamera(movement * 0.2f);
		}

		// -- Square brackets: switch target
		if (Input.GetKeyDown(KeyCode.LeftBracket))
		{
			SwitchTarget(-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightBracket))
		{
			SwitchTarget(1);
		}
	}
}
