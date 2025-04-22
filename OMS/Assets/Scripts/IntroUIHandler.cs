using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroUIHandler : MonoBehaviour
{
	public GameObject earth;
	private float rotationSpeed = 0.5f; // degrees per second

	void Update()
	{
		Transform transform = earth.transform;
		transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
	}

	public void GoToNextScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.buildIndex + 1);
	}

}
