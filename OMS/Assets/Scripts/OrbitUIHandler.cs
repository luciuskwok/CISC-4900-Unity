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
	public GameObject targetOrbitLine;


	public static double EarthRadius = 6378.0; // km
	public static double UnityUnitToKm = 1.0e3; //  1 Unity Unit = 1,000 km
	public static double KmToUnityUnit = 1.0 / UnityUnitToKm;


	private double playerSemiMajorAxis = EarthRadius + 1000; // km
	private double playerEccentricity = 0.0;
	private double playerLongOfPeriapsis = 0.0; // degrees

	// private double earthGM = 3.986004418e5; // in km^3/s^2
	private double earthMass = 5.9722e24; // kg

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

	public void EccentricityDidChange(float value) {
		playerEccentricity = value;
		UpdateText();
		UpdatePlayerOrbitLine();
	}

	public void LongitudeDidChange(float value) {
		playerLongOfPeriapsis = value;
		UpdateText();
		UpdatePlayerOrbitLine();
	}

	void UpdateText() {
		eccentricityReadout.SetText(playerEccentricity.ToString("F3"));
		longitudeReadout.SetText(playerLongOfPeriapsis.ToString("F0")+"°");

		// Stats
		double op = OrbitPlot.OrbitalPeriod(playerSemiMajorAxis, earthMass);
		statisticsText.text = "Semi-Major Axis: " + (playerSemiMajorAxis).ToString("#,##0") + " km\n" +
			"Orbital Period: " + (op / 60.0).ToString("F2") + " min.";
	}

	void UpdatePlayerOrbitLine() {
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		playerOrbit.semiMajorAxis = playerSemiMajorAxis;
		playerOrbit.eccentricity = playerEccentricity;
		playerOrbit.longitudeOfPeriapsis = playerLongOfPeriapsis;
		playerOrbit.UpdatePoints();

		// For testing, also change the target
		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		targetOrbit.semiMajorAxis = playerSemiMajorAxis;
		targetOrbit.eccentricity = playerEccentricity;
		targetOrbit.longitudeOfPeriapsis = playerLongOfPeriapsis;
		targetOrbit.UpdatePoints();
	}

}
