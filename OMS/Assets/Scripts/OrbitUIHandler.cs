using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbitUIHandler : MonoBehaviour
{
	public void GoToTitleScene() {
		SceneManager.LoadScene(0);
	}
}
