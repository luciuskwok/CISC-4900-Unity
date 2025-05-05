using UnityEngine;

public class UINode : MonoBehaviour
{
	private Vector3 m_WorldPosition;

	public void SetWorldPosition(Vector3 worldPosition) {
		m_WorldPosition = worldPosition;
		UpdateCanvasPosition();
	}	

	public void UpdateCanvasPosition() {
		// Convert to 2d 
		Vector3 point = Camera.main.WorldToViewportPoint(m_WorldPosition);
		// Check if point is in front of camera
		if (point.z > 0.0) {
			gameObject.SetActive (true);
			// Scale to canvas size
			GameObject canvas = GameObject.Find("Canvas");
			RectTransform canvasRT = canvas.GetComponent<RectTransform>();
			point.x *= canvasRT.rect.width;
			point.y *= canvasRT.rect.height;
			// Move node in UI
			RectTransform nodeRT = gameObject.GetComponent<RectTransform>();
			nodeRT.anchoredPosition = point;
		} else {
			gameObject.SetActive (false);
		}
	}
}
