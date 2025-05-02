using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterSelectButtonHandler : MonoBehaviour
{
	// Go to the Chapter Select scene
	public void GoToChapterSelectScene()
	{
		const int chapterSelectSceneIndex = 1;
		SceneManager.LoadScene(chapterSelectSceneIndex);
	}

}
