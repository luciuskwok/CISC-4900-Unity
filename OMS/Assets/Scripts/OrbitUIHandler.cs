using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrbitUIHandler : MonoBehaviour
{
	// UI
	public Slider progradeSlider;
	public TMP_Text progradeReadout;
	public TMP_Text playerStatsText;
	public GameObject playerOrbitLine;
	public TMP_Text targetStatsText;
	public GameObject targetOrbitLine;
	public GameObject maneuverNode;
	public Camera mainCamera;
	public Canvas canvas;

	// Player parameters
	private double playerSemiMajorAxis = EarthRadius + 1000; // km
	private double playerEccentricity = 0.0;
	private double playerPeriapsisLongitude = 180.0 * Kepler.Deg2Rad;

	// Maneuver node parameters
	private double progradeDeltaV = 0.0; // m/s
	private double nodeMeanAnomaly = 0.0; // radians

	// private double earthGM = 3.986004418e5; // in km^3/s^2

	// Constants
	public const double EarthRadius = 6378.0; // km
	public const double EarthMass = 5.9722e24; // kg
//	public const double UnityUnitToKm = 1.0e3; //  1 Unity Unit = 1,000 km
//	public const double KmToUnityUnit = 1.0 / UnityUnitToKm;

	void Start()
	{
		progradeSlider.SetValueWithoutNotify((float)progradeDeltaV);
		UpdateOrbit();

		// Set up the target orbit
		double targetPeriaps = 1000.0 + EarthRadius; // km
		double targetApoaps = 4000.0 + EarthRadius; // km
		double targetSMA = (targetPeriaps + targetApoaps) / 2.0;
		double targetEccen = 1.0 - (targetPeriaps / targetSMA);

		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		targetOrbit.semiMajorAxis = targetSMA;
		targetOrbit.eccentricity = targetEccen;
		targetOrbit.periapsisLongitude = playerPeriapsisLongitude;
		targetOrbit.UpdatePoints();

		// Target Stats Text
		double f = targetSMA * targetEccen;
		double apoapsis = targetSMA + f - EarthRadius;
		double periapsis = targetSMA - f - EarthRadius;
		double period = Kepler.OrbitalPeriod(targetSMA, EarthMass);
		targetStatsText.text = "Apoapsis: " + apoapsis.ToString("#,##0") + " km\n" +
			"Periapsis: " + periapsis.ToString("#,##0") + " km\n" +
			"Period: " + OrbitUIHandler.FormattedTime(period) + "\n";

		// Update maneuver node
		SetManeuverNodeAtEccentricAnomaly(nodeMeanAnomaly);
	}

	void Update()
	{
		AnimateManeuverNode();
	}

	void AnimateManeuverNode() {
		const double nodeAnimationTime = 4.0;
		double x = Time.timeSinceLevelLoadAsDouble / nodeAnimationTime % 1.0f;
		double meanAnomaly = x * Kepler.PI_2;
		double eccentricAnomaly = Kepler.EccentricAnomalyFromMeanAnomaly(meanAnomaly, playerEccentricity);
		SetManeuverNodeAtEccentricAnomaly(eccentricAnomaly);
	}

	void SetManeuverNodeAtEccentricAnomaly(double eccentricAnomaly) {
		// Move Maneuver Node UI element to aligh with view
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		// Get the local coordinates of the node and convert to world coordinates
		Vector3 position = playerOrbit.LocalPositionAtEccentricAnomaly(eccentricAnomaly);
		position = playerOrbitLine.transform.TransformPoint(position);
		// Convert to 2d 
		Vector3 point = mainCamera.WorldToViewportPoint(position);
		// Scale to canvas size
		RectTransform canvasRT = canvas.GetComponent<RectTransform>();
		point.x *= canvasRT.rect.width;
		point.y *= canvasRT.rect.height;
		// Move maneuver node in UI
		RectTransform nodeRT = maneuverNode.GetComponent<RectTransform>();
		nodeRT.anchoredPosition = point;

		//Debug.Log("Node: x = " + point.x + ", y = " + point.y);
	}

	public void HandleMenuButton() {
		SceneManager.LoadScene(1); // Chapter Select
	}

	public void ProgradeDidChange(float value) {
		progradeDeltaV = value;
		UpdateOrbit();
	}

	void UpdateOrbit() {
		// Recalculate orbital parameters
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		playerOrbit.semiMajorAxis = playerSemiMajorAxis;
		playerOrbit.eccentricity = playerEccentricity;
		playerOrbit.periapsisLongitude = playerPeriapsisLongitude;
		playerOrbit.UpdatePoints();

		// Maneuver Controls
		progradeReadout.SetText(progradeDeltaV.ToString("F1") + " m/s");

		// Player Stats
		double f = playerSemiMajorAxis * playerEccentricity;
		double apoapsis = playerSemiMajorAxis + f - EarthRadius;
		double periapsis = playerSemiMajorAxis - f - EarthRadius;
		double period = Kepler.OrbitalPeriod(playerSemiMajorAxis, EarthMass);
		playerStatsText.text = "Apoapsis: " + apoapsis.ToString("#,##0") + " km\n" +
			"Periapsis: " + periapsis.ToString("#,##0") + " km\n" +
			"Period: " + OrbitUIHandler.FormattedTime(period) + "\n";
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
		s += "s";
		return s;
	}

}
