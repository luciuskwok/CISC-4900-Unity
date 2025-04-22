using UnityEngine;
using UnityEngine.SceneManagement;

public class CelestialUIHandler : MonoBehaviour
{
	public void HandleExitButton() {
		SceneManager.LoadScene(1); // Chapter Select
	}
	
}
