using System;
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

	private double playerSemiMajorAxis = 6.973;
	private double playerEccentricity = 0.0;
	private double playerLongOfPeriapsis = 0.0;

	private double earthGM = 3.986004418e14; // in m^3/s^2


	void Start()
	{
		eccentricitySlider.SetValueWithoutNotify((float)playerEccentricity);
		longitudeSlider.SetValueWithoutNotify((float)playerLongOfPeriapsis);
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
		UpdateText();
		UpdatePlayerOrbitLine();
	}

	public void longitudeDidChange(float value) {
		playerLongOfPeriapsis = value;
		UpdateText();
		UpdatePlayerOrbitLine();
	}

	void UpdateText() {
		eccentricityReadout.SetText(playerEccentricity.ToString("F3"));
		longitudeReadout.SetText(playerLongOfPeriapsis.ToString("F0")+"°");

		// Stats
		double op = orbitalPeriod(playerSemiMajorAxis);
		statisticsText.text = "Semi-Major Axis: " + (playerSemiMajorAxis * 1000.0).ToString("F0") + " km\n" +
			"Orbital Period: " + (op / 60.0).ToString("F0") + " min.";
	}

	void UpdatePlayerOrbitLine() {
		var x = playerOrbitLine.GetComponent<OrbitPlot>();
		x.semiMajorAxis = playerSemiMajorAxis;
		x.eccentricity = playerEccentricity;
		x.longitudeOfPeriapsis = playerLongOfPeriapsis;
		x.UpdatePoints();
	}

	double orbitalPeriod(double semiMajorAxis) {
		// Convert to meters from Unity units (1,000,000 m = 1 Unity unit)
		double r = semiMajorAxis * 1.0e6; 
		return 2.0 * Math.PI * Math.Sqrt(r *r * r / earthGM);
	}


}
