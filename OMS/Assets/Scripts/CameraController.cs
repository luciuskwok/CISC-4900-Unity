using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public GameObject viewTarget;

	public float distanceMax = 88000.0f;	// * 10^6 km
	public float panSpeed = 20.0f;			// degrees per second
	public float scrollSpeed = 20.0f;		// distance per scroll wheel unit depends on distance from object

	private Vector3 lastMousePosition;
 
	void Start()
	{
		
	}

	private void PrintLatLon()
	{
		Vector3 targetPosition = viewTarget.transform.position;
		Vector3 cameraPosition = transform.position;
		float distance = Vector3.Distance(targetPosition, cameraPosition);
		Vector3 direction = (cameraPosition - targetPosition).normalized;

		float latDeg = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
		float lonDeg = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

		// Normalize latDeg to range from -180 to +180 degrees
		if (latDeg > 180.0f) latDeg -= 360.0f;

		Debug.Log("Lat: " + latDeg + " Lon: " + lonDeg);
	}

	private void PanTiltCamera(float deltaX, float deltaY)
	{
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

			Debug.Log("Lat:" + latDeg + " Lon:" + lonDeg + " X:" + cameraPosition.x + " Y:" + cameraPosition.y + " Z:" + cameraPosition.z);

			latDeg += deltaY;
			lonDeg += deltaX;

			// Clamp lat values to min and max
			latDeg = (latDeg > 89.9f) ? 89.9f : latDeg;
			latDeg = (latDeg < -89.9f) ? 89.9f : latDeg;

			// Convert to radians
			float latRad = latDeg * Mathf.Deg2Rad;
			float lonRad = lonDeg * Mathf.Deg2Rad;

			direction.y = Mathf.Sin(latRad);
			direction.z = Mathf.Cos(latRad) * Mathf.Sin(lonRad);
			direction.x = Mathf.Cos(latRad) * Mathf.Cos(lonRad);

			// Set new camera position
			transform.position = targetPosition + direction * distance;

			// Rotate camera to face target



		}
	}

	private void DollyInCamera(float delta)
	{
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
		// Mouse movement
		if (Input.GetMouseButtonDown(0))
		{
			lastMousePosition = Input.mousePosition;
			PrintLatLon();
		} else if (Input.GetMouseButton(0))
		{
			Vector3 delta = Input.mousePosition - lastMousePosition;
			if (delta.x != 0.0f || delta.y != 0.0f)
			{
				PanTiltCamera(delta.x, delta.y);
			}
			lastMousePosition = Input.mousePosition;
		}

		// Scroll wheel
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll != 0.0f)
		{
			// Shift key slows the movement
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				scroll = scroll / 10.0f;
			}
			DollyInCamera(scroll);
		}

		// Keyboard
		float movement = 5.0f;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
			movement = 0.5f;
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			PanTiltCamera(0.0f, movement);
		} else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			PanTiltCamera(0.0f, -movement);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			PanTiltCamera(-movement, 0.0f);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			PanTiltCamera(movement, 0.0f);
		}
	}
}
