using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneButtonHandler : MonoBehaviour
{
	// Go to the next scene
	public void GoToNextScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.buildIndex + 1);
	}

}
