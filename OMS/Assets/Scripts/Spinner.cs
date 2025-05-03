using UnityEngine;

public class Spinner : MonoBehaviour
{
	public float rotationSpeed = 0.5f; // degrees per second

	void Update()
	{
		gameObject.transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
	}

}
