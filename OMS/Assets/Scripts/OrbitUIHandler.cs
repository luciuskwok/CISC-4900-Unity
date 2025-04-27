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

		// Set up the target orbit
		double targetPeriaps = 1000.0 + EarthRadius; // km
		double targetApoaps = 4000.0 + EarthRadius; // km
		double targetSMA = (targetPeriaps + targetApoaps) / 2.0;

		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		targetOrbit.semiMajorAxis = targetSMA;
		targetOrbit.eccentricity = 1.0 - (targetPeriaps / targetSMA);
		targetOrbit.longitudeOfPeriapsis = 180.0;
		targetOrbit.UpdatePoints();



		/* 	
			pe = sma - f
			pe = sma - sma * e
			pe/sma = 1 - e
			e = 1 - pe/sma
		*/
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
		double orbitalPeriod = OrbitPlot.OrbitalPeriod(playerSemiMajorAxis, earthMass);
		double f = playerSemiMajorAxis * playerEccentricity;
		double apoapsis = playerSemiMajorAxis + f - EarthRadius;
		double periapsis = playerSemiMajorAxis - f - EarthRadius;

		statisticsText.text = "Semi-Major Axis: " + (playerSemiMajorAxis).ToString("#,##0") + " km\n" +
			"Orbital Period: " + (orbitalPeriod / 60.0).ToString("F2") + " min.\n" +
			"Apoapsis: " + apoapsis.ToString("#,##0") + " km\n" +
			"Periapsis: " + periapsis.ToString("#,##0") + " km\n";
	}

	void UpdatePlayerOrbitLine() {
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		playerOrbit.semiMajorAxis = playerSemiMajorAxis;
		playerOrbit.eccentricity = playerEccentricity;
		playerOrbit.longitudeOfPeriapsis = playerLongOfPeriapsis;
		playerOrbit.UpdatePoints();

	}

	// Utilities

	public static String FormattedTime(double timeAsSeconds) {
		String s = "";
		double t = timeAsSeconds;
		if (timeAsSeconds >= 3600.0) {
			s += Math.Floor(t/3600.0).ToString("F0") + "h";
			t -= Math.Floor(t/3600.0) * 3600.0;
		}
		if (timeAsSeconds >= 60.0) {
			s += Math.Floor(t/60.0).ToString("F0") + "m";
			t -= Math.Floor(t/60.0) * 60.0;

		}
		if (timeAsSeconds >= 60.0) {
			s += Math.Floor(t).ToString("F0");
		} else {
			s += t.ToString("F2");
		}
		return s;
	}

}
