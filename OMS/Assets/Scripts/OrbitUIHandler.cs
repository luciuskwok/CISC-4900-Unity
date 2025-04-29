using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrbitUIHandler : MonoBehaviour
{
	// Text UI
	public Slider progradeSlider;
	public TMP_Text progradeReadout;
	public TMP_Text playerStatsText;
	public TMP_Text targetStatsText;
	public TMP_Text infoText;
	
	// Orbit lines
	public GameObject playerOrbitLine;
	public GameObject plannedOrbitLine;
	public GameObject targetOrbitLine;

	// Other UI elements
	public GameObject maneuverNode;
	public GameObject goButton;

	// Other objects
	public Camera mainCamera;
	public Canvas canvas;

	// Player parameters
	private const double playerAltitude = 420.0; // km above Earth's surface
	private double playerSemiMajorAxis = EarthRadius + playerAltitude; // km
	private double playerEccentricity = 0.0;
	private double playerArgOfPerifocus = 180.0 * Kepler.Deg2Rad;

	// Maneuver node parameters
	private double progradeDeltaV = 0.0; // m/s
	private double nodeMeanAnomaly = 0.0 * Kepler.Deg2Rad; // radians

	// Target orbit parameters
	private double targetApoapsis = EarthRadius + 4000.0;

	// Constants
	public const double EarthRadius = 6378.0; // km
	public const double EarthMass = 5.9722e24; // kg

	void Start() {
		progradeSlider.SetValueWithoutNotify((float)progradeDeltaV);

		// Set up the current player orbit
		Attractor earth = Attractor.Earth;
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		playerOrbit.SetOrbitalElements(playerEccentricity, playerSemiMajorAxis, 0, 0, playerArgOfPerifocus, 0, earth);

		// Set up the target orbit
		double targetPeriaps = playerAltitude + earth.radius; // km
		double targetSMA = (targetPeriaps + targetApoapsis) / 2.0;
		double targetEccen = 1.0 - (targetPeriaps / targetSMA);

		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		targetOrbit.SetOrbitalElements(targetEccen, targetSMA, 0, 0, playerArgOfPerifocus, 0, earth);

		// Target Stats Text
		double f = targetSMA * targetEccen;
		double apoAlt = targetOrbit.Orbit.ApoapsisAltitude;
		double periAlt = targetOrbit.Orbit.PeriapsisAltitude;
		double period = targetOrbit.Orbit.OrbitalPeriod;
		targetStatsText.text = "Apoapsis: " + apoAlt.ToString("#,##0") + " km\n" +
			"Periapsis: " + periAlt.ToString("#,##0") + " km\n" +
			"Period: " + OrbitUIHandler.FormattedTime(period) + "\n";

		// Update maneuver node & planned orbit
		PositionManeuverNodeWithMeanAnomaly(nodeMeanAnomaly, playerOrbitLine);
		UpdatePlannedOrbit();

		// Set the info text
		SetInfoText(false);
	}

	void Update() {
		//AnimateManeuverNode();
	}

	void AnimateManeuverNode() {
		const double nodeAnimationTime = 30.0;
		double x = Time.timeSinceLevelLoadAsDouble / nodeAnimationTime % 1.0f;

		// Rotate the maneuver node around for testing
		nodeMeanAnomaly = x * Kepler.PI_2;
		PositionManeuverNodeWithMeanAnomaly(nodeMeanAnomaly, playerOrbitLine);
		UpdatePlannedOrbit();
	}

	void PositionManeuverNodeWithMeanAnomaly(double meanAnomaly, GameObject orbitGameObject) {
		// Move Maneuver Node UI element to specific point on orbit
		OrbitPlot plot = orbitGameObject.GetComponent<OrbitPlot>();

		double eccentricAnomaly = Kepler.EccentricAnomalyFromMean(meanAnomaly, plot.Orbit.Eccentricity);

		// Get the local coordinates of the node and convert to world coordinates
		Vector3d localPos = plot.Orbit.GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
		Vector3 worldPos = orbitGameObject.transform.TransformPoint(localPos.Vector3);
		// Convert to 2d 
		Vector3 point = mainCamera.WorldToViewportPoint(worldPos);
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
		progradeDeltaV = value / 1000.0; // Convert value from m/s to km/s
		UpdatePlannedOrbit();
	}

	void UpdatePlannedOrbit() {
		// Calculate the orbital parameters resulting from maneuver
		// Get original velocity and position at maneuver node
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		double nodeEccentricAnomaly = Kepler.EccentricAnomalyFromMean(nodeMeanAnomaly, playerEccentricity);
		Vector3d originalVelocity = playerOrbit.Orbit.GetVelocityAtEccentricAnomaly(nodeEccentricAnomaly);
		// Add prograde delta-V
		Vector3d progradeDirection = originalVelocity.normalized;
		Vector3d deltaVelocity = progradeDirection * progradeDeltaV;
		// Calculate the new orbit based on the new velocity vector
		Vector3d newVelocity = originalVelocity + deltaVelocity;
		Vector3d nodePosition = playerOrbit.Orbit.GetFocalPositionAtEccentricAnomaly(nodeEccentricAnomaly);

		// Update the planned orbit parameters
		OrbitPlot planOrbit = plannedOrbitLine.GetComponent<OrbitPlot>();
		planOrbit.SetOrbitByThrow(nodePosition, newVelocity, Attractor.Earth);

		// Maneuver Controls
		progradeReadout.SetText((progradeDeltaV * 1000.0).ToString("F1") + " m/s");

		// Maneuver Stats
		double apoAlt = planOrbit.Orbit.ApoapsisAltitude;
		double periAlt = planOrbit.Orbit.PeriapsisAltitude;
		double period = planOrbit.Orbit.OrbitalPeriod;
		playerStatsText.text = "Apoapsis: " + apoAlt.ToString("#,##0") + " km\n" +
			"Periapsis: " + periAlt.ToString("#,##0") + " km\n" +
			"Period: " + OrbitUIHandler.FormattedTime(period) + "\n";

		// Check if planned orbit is within tolerances and enable Go button
		double planApo = planOrbit.Orbit.ApoapsisDistance;
		if (Math.Abs(targetApoapsis - planApo) < targetApoapsis * 0.02) {
			goButton.SetActive(true);
			SetInfoText(true);
		} else {
			goButton.SetActive(false);
			SetInfoText(false);
		}
	}

	void SetInfoText(bool success) {
		if (!success) {
			infoText.text = "Welcome! Your first task is to adjust this maneuver node to match the target orbit. Use the slider below to adjust the planned change in velocity for this maneuver.";
		} else {
			infoText.text = "Great job! Now click on the Go button to execute the maneuver by firing your engines.";
		}
	}

	// Utilities

	public static String FormattedTime(double timeAsSeconds) {
		if (double.IsInfinity(timeAsSeconds)) return "Infinite";

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
