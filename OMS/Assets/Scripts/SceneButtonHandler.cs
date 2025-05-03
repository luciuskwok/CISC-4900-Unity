using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneButtonHandler : MonoBehaviour
{
	
	public void GoToNextScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.buildIndex + 1);
	}

	public void GoToChapterSelectScene()
	{
		const int chapterSelectSceneIndex = 1;
		SceneManager.LoadScene(chapterSelectSceneIndex);
	}

	public void GoToSceneAtIndex(int index) {
		SceneManager.LoadScene(index);
	}

	public void Exit()
	{
		// Save any persistent data here
#if UNITY_EDITOR
		EditorApplication.ExitPlaymode();
#else
		Application.Quit();
#endif
	}

}
