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
		// Scale to canvas size
		GameObject canvas = GameObject.Find("Canvas");
		RectTransform canvasRT = canvas.GetComponent<RectTransform>();
		point.x *= canvasRT.rect.width;
		point.y *= canvasRT.rect.height;
		// Move node in UI
		RectTransform nodeRT = gameObject.GetComponent<RectTransform>();
		nodeRT.anchoredPosition = point;

	}
}
