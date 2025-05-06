using UnityEngine;
using UnityEngine.UI;

public class UINode : MonoBehaviour
{
	public float minimumVisibleDistance = 0.0f;
	public float maximumVisibleDistance = 1.0e6f;
	public float altVisibleDistance = 0.0f;
	
	// Texture swapping
	public GameObject nodeImage;
	public Texture altTexture;
	private Texture m_MainTexture;

	private Vector3 m_WorldPosition;


	void Start()
	{
		Image image = nodeImage.GetComponent<Image>();
		//m_MainTexture = image.texture;
	}

	public void SetWorldPosition(Vector3 worldPosition) {
		m_WorldPosition = worldPosition;
		UpdateCanvasPosition();
	}	

	public void UpdateCanvasPosition() {
		// Convert to 2d 
		Vector3 point = Camera.main.WorldToViewportPoint(m_WorldPosition);
		// Check if point is within visible range
		if (minimumVisibleDistance <= point.z && point.z <= maximumVisibleDistance) 
		{
			gameObject.SetActive (true);
			// Scale to canvas size
			GameObject canvas = GameObject.Find("Canvas");
			RectTransform canvasRT = canvas.GetComponent<RectTransform>();
			point.x *= canvasRT.rect.width;
			point.y *= canvasRT.rect.height;
			// Move node in UI
			RectTransform nodeRT = gameObject.GetComponent<RectTransform>();
			nodeRT.anchoredPosition = point;

			// Swap out textures depending ond istance
			if (point.z <= altVisibleDistance) {
				//nodeImage.GetComponent<Image>().image = altTexture;
			} else {
				//nodeImage.GetComponent<Image>().image = m_MainTexture;
			}
		} else {
			gameObject.SetActive (false);
		}
	}
}
