using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class VideoController : MonoBehaviour
{
	private VideoPlayer videoPlayer;

	void Start()
	{
		videoPlayer = GetComponent<VideoPlayer>();
		videoPlayer.loopPointReached += EndReached;
		StartCoroutine(PlayVideo());
	}

	IEnumerator PlayVideo() {
		yield return new WaitForSeconds(0.25f);
		videoPlayer.Play();
	}

	private void EndReached(VideoPlayer vp) {
		// Go to next scene
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.buildIndex + 1);
	}

}
