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

	private void PanTiltCamera(float deltaX, float deltaY)
	{
		if (viewTarget)
		{
			Vector3 targetPosition = viewTarget.transform.position;
			Vector3 cameraPosition = transform.position;
			Vector3 viewVector = targetPosition - cameraPosition;
			float distance = Vector3.Magnitude(viewVector);
			//Vector3 eulerAngles = viewVector.eulerAngles;

		}
		//float moveZ = Mathf.Cos(transform.eulerAngles.y * Mathf.PI / 180.0f) * inZ - Mathf.Sin(transform.eulerAngles.y * Mathf.PI / 180.0f) * inX;
		//float moveX = Mathf.Sin(transform.eulerAngles.y * Mathf.PI / 180.0f) * inZ + Mathf.Cos(transform.eulerAngles.y * Mathf.PI / 180.0f) * inX;
		//transform.position += new Vector3(moveX, 0, moveZ);
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
		} else if (Input.GetMouseButton(0))
		{
			Vector3 delta = Input.mousePosition - lastMousePosition;
			PanTiltCamera(delta.x, delta.y);
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
	}
}
