using UnityEngine;

public class UINode : MonoBehaviour
{
	public float minimumVisibleDistance = 0.0f;
	public float maximumVisibleDistance = 1.0e6f;
	public float altVisibleDistance = 0.0f;
	
	// Image swapping
	public GameObject mainImage;
	public GameObject altImage;

	private Vector3 m_WorldPosition;


	void Start()
	{
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
				mainImage.SetActive(false);
				altImage.SetActive(true);
			} else {
				mainImage.SetActive(true);
				altImage.SetActive(false);
			}
		} else {
			gameObject.SetActive (false);
		}
	}
}
