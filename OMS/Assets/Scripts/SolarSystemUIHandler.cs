using UnityEngine;
using UnityEngine.SceneManagement;

public class SolarSystemUIHandler : MonoBehaviour
{

	public void HandleNextButton() {
		// Go to next scene
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
	}

	
}
