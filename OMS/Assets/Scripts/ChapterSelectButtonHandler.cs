using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterSelectButtonHandler : MonoBehaviour
{
	void Start()
	{
		
	}

	// Go to the Chapter Select scene
	public void GoToChapterSelectScene()
	{
		SceneManager.LoadScene(1);
	}

}
