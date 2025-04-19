using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class VideoSceneHandler : MonoBehaviour
{
	public VideoPlayer videoPlayer;

	void Start()
	{
		videoPlayer.loopPointReached += EndReached;
	}

	private void EndReached(VideoPlayer vp) {
		GoToNextScene();
	}

	public void GoToNextScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.buildIndex + 1);
	}
}
