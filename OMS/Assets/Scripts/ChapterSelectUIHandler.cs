using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class ChapterSelectUIHandler : MonoBehaviour
{
	public void GoToScene(int index)
	{
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
