using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbitUIHandler : MonoBehaviour
{
	public void HandleExitButton() {
		SceneManager.LoadScene(1); // Chapter Select
	}
}
