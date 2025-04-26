using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif


public class VideoSceneHandler : MonoBehaviour
{
	public VideoPlayer videoPlayer;

	void Start()
	{
		videoPlayer.loopPointReached += EndReached;
		StartCoroutine(PlayVideo());
	}

	IEnumerator PlayVideo() {
		yield return new WaitForSeconds(0.25f);
		videoPlayer.Play();
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
