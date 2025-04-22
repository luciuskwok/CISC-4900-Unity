using UnityEngine;
using UnityEngine.SceneManagement;

public class EndCreditsUIHandler : MonoBehaviour
{
	public GameObject moon;
	private float rotationSpeed = 0.5f; // degrees per second

	void Update()
	{
		Transform transform = moon.transform;
		transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
	}

	public void GoToTitleScene()
	{
		SceneManager.LoadScene(0);
	}

}
