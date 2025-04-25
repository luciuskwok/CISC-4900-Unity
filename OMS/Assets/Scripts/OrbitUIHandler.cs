using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrbitUIHandler : MonoBehaviour
{
	public Slider eccentricitySlider;
	public TMP_Text eccentricityReadout;

	public Slider longitudeSlider;
	public TMP_Text longitudeReadout;

	public TMP_Text statisticsText;
	public GameObject playerOrbitLine;

	private float playerSemiMajorAxis = 6.973f;
	private float playerEccentricity = 0.0f;
	private float playerLongOfPeriapsis = 0.0f;


	void Start()
	{
		eccentricitySlider.SetValueWithoutNotify(playerEccentricity);
		longitudeSlider.SetValueWithoutNotify(playerLongOfPeriapsis);
		UpdateText();
		UpdatePlayerOrbitLine();
	}

	void Update()
	{
		
	}

	public void HandleExitButton() {
		SceneManager.LoadScene(1); // Chapter Select
	}

	public void eccentricityDidChange(float value) {
		playerEccentricity = value;
		eccentricityReadout.SetText(value.ToString("F3"));
		UpdateText();
		UpdatePlayerOrbitLine();
	}

	public void longitudeDidChange(float value) {
		playerLongOfPeriapsis = value;
		longitudeReadout.SetText(value.ToString("F0")+"°");
		UpdateText();
		UpdatePlayerOrbitLine();
	}

	void UpdateText() {
		statisticsText.text = "Semi-Major Axis: " + (playerSemiMajorAxis * 1000.0f).ToString("F0") + "\n" +
			"Orbital Period: ???";
	}

	void UpdatePlayerOrbitLine() {
		var x = playerOrbitLine.GetComponent<OrbitPlot>();
		x.semiMajorAxis = playerSemiMajorAxis;
		x.eccentricity = playerEccentricity;
		x.longitudeOfPeriapsis = playerLongOfPeriapsis;
		x.UpdatePoints();
	}


}
