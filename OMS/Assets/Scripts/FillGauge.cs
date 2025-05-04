using Unity.VisualScripting;
using UnityEngine;

public class FillGauge : MonoBehaviour
{
	public GameObject fillerImage;
	private float fillerWidth;

	private float m_FillValue;
	public float FillValue {
		get {
			return m_FillValue;
		}
		set {
			m_FillValue = value;
			float width = value * fillerWidth;
			float rightInset = (1.0f - value) * fillerWidth;
			RectTransform rt = fillerImage.GetComponent<RectTransform>();
			rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, rightInset, width);
		}
	}

	void Start()
	{
		RectTransform rt = fillerImage.GetComponent<RectTransform>();
		fillerWidth = rt.rect.width;
	}


}
