using UnityEngine;

public class BodyLabelScript : MonoBehaviour
{
	public GameObject bodyIcon;
	public GameObject textLabel;

	private Camera mainCamera;
	//Renderer renderer;

	void Start()
	{
		mainCamera = Camera.main;
		//renderer = gameObject.GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update()
	{
		// Rotate so billboard always faces camera
		//transform.LookAt(mainCamera.transform);
		//transform.Rotate(0, 180, 0);
		transform.rotation = mainCamera.transform.rotation;

		// Adjust scale so billboard always appears the same size in camera
		Vector3 cameraPosition = mainCamera.transform.position;
		float distance = Vector3.Distance(transform.position, cameraPosition);
		float s = distance / 125.0f;
		Vector3 scale = transform.localScale;
		scale.Set(s, s, s);
		transform.localScale = scale;

		// Fade out label between 4.0 and 5.0 (x10^6) km
		float a = 1.0f;
		if (distance <= 4.0f)
		{
			a = 0.0f;
		} else if (distance < 5.0f)
		{
			a = (distance - 4.0f);
		}
		//renderer.material.color.a = a;

	}
}
