using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float panSpeed = 20.0f;
	public float scrollSpeed = 20.0f;
	public float minY = 5.0f;
	public float maxY = 30.0f;

	private Vector3 lastMousePosition;
 
	void Start()
	{
		
	}

	private void MoveCamera(float inX, float inZ)
	{
		float moveZ = Mathf.Cos(transform.eulerAngles.y * Mathf.PI / 180.0f) * inZ - Mathf.Sin(transform.eulerAngles.y * Mathf.PI / 180.0f) * inX;
		float moveX = Mathf.Sin(transform.eulerAngles.y * Mathf.PI / 180.0f) * inZ + Mathf.Cos(transform.eulerAngles.y * Mathf.PI / 180.0f) * inX;
		transform.position += new Vector3(moveX, 0, moveZ);
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
			MoveCamera(delta.x, delta.y);
			lastMousePosition = Input.mousePosition;
		}

		// Scroll wheel
		Vector3 pos = transform.position;
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		pos.y -= scroll * scrollSpeed * Time.deltaTime * 300.0f;
		pos.y = Mathf.Clamp(pos.y, minY, maxY);
		transform.position = pos;
	}
}
