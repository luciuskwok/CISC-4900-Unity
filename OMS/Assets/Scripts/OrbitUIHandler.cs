using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrbitUIHandler : MonoBehaviour
{
	// Text UI
	public Slider progradeSlider;
	public TMP_Text progradeReadout;
	public TMP_Text playerStatsText;
	public TMP_Text targetStatsText;
	public TMP_Text shipStatsText;
	
	// Orbit lines
	public GameObject playerOrbitLine;
	public GameObject plannedOrbitLine;
	public GameObject targetOrbitLine;
	public GameObject maneuverNode;

	// Other UI
	public Camera mainCamera;
	public Canvas canvas;

	// Player parameters
	private const double playerAltitude = 420.0; // km above Earth's surface
	private double playerSemiMajorAxis = EarthRadius + playerAltitude; // km
	private double playerEccentricity = 0.0;
	private double playerPeriapsisLongitude = 180.0 * Kepler.Deg2Rad;

	// Maneuver node parameters
	private double progradeDeltaV = 0.0; // m/s
	private double nodeMeanAnomaly = 30.0 * Kepler.Deg2Rad; // radians

	// private double earthGM = 3.986004418e5; // in km^3/s^2

	// Constants
	public const double EarthRadius = 6378.0; // km
	public const double EarthMass = 5.9722e24; // kg

	void Start() {
		progradeSlider.SetValueWithoutNotify((float)progradeDeltaV);

		// Set up the current player orbit
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		playerOrbit.semiMajorAxis = playerSemiMajorAxis;
		playerOrbit.eccentricity = playerEccentricity;
		playerOrbit.periapsisLongitude = playerPeriapsisLongitude;
		playerOrbit.attractorMass = EarthMass;
		playerOrbit.UpdatePoints();

		// Set up the target orbit
		double targetPeriaps = playerAltitude + EarthRadius; // km
		double targetApoaps = 4000.0 + EarthRadius; // km
		double targetSMA = (targetPeriaps + targetApoaps) / 2.0;
		double targetEccen = 1.0 - (targetPeriaps / targetSMA);

		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		targetOrbit.semiMajorAxis = targetSMA;
		targetOrbit.eccentricity = targetEccen;
		targetOrbit.periapsisLongitude = playerPeriapsisLongitude;
		targetOrbit.attractorMass = EarthMass;
		targetOrbit.UpdatePoints();

		// Target Stats Text
		double f = targetSMA * targetEccen;
		double apoAlt = targetOrbit.ApoapsisFromFocus() - EarthRadius;
		double periAlt = targetOrbit.PeriapsisFromFocus() - EarthRadius;
		double period = targetOrbit.OrbitalPeriod();
		targetStatsText.text = "Apoapsis: " + apoAlt.ToString("#,##0") + " km\n" +
			"Periapsis: " + periAlt.ToString("#,##0") + " km\n" +
			"Period: " + OrbitUIHandler.FormattedTime(period) + "\n";

		// Set initial planned orbit parameters
		OrbitPlot planOrbit = plannedOrbitLine.GetComponent<OrbitPlot>();
		planOrbit.semiMajorAxis = playerSemiMajorAxis;
		planOrbit.eccentricity = playerEccentricity;
		planOrbit.periapsisLongitude = playerPeriapsisLongitude;
		planOrbit.attractorMass = EarthMass;

		// Update maneuver node & planned orbit
		PositionManeuverNodeWithMeanAnomaly(nodeMeanAnomaly, playerOrbitLine);
		UpdatePlannedOrbit();
	}

	void Update() {
		AnimateManeuverNode();
	}

	void AnimateManeuverNode() {
		const double nodeAnimationTime = 30.0;
		double x = Time.timeSinceLevelLoadAsDouble / nodeAnimationTime % 1.0f;

		// Rotate the maneuver node around for testing
		nodeMeanAnomaly = x * Kepler.PI_2;
		PositionManeuverNodeWithMeanAnomaly(nodeMeanAnomaly, playerOrbitLine);
		UpdatePlannedOrbit();
	}

	void PositionManeuverNodeWithMeanAnomaly(double meanAnomaly, GameObject orbit) {
		// Move Maneuver Node UI element to specific point on orbit
		OrbitPlot plot = orbit.GetComponent<OrbitPlot>();

		double eccentricAnomaly = Kepler.EccentricAnomalyFromMean(meanAnomaly, plot.eccentricity);
		// Get the local coordinates of the node and convert to world coordinates
		Vector2d localPos = plot.LocalPositionAtEccentricAnomaly(eccentricAnomaly);
		Vector3 worldPos = orbit.transform.TransformPoint(new Vector3((float)localPos.x, 0, (float)localPos.y));
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
		Vector2d originalVelocity = playerOrbit.VelocityAtEccentricAnomaly(nodeEccentricAnomaly);
		// Add prograde delta-V
		Vector2d progradeDirection = originalVelocity.normalized;
		Vector2d deltaVelocity = progradeDirection * progradeDeltaV;
		// Calculate the new orbit based on the new velocity vector
		Vector2d newVelocity = originalVelocity + deltaVelocity;
		Vector2d nodePosition = playerOrbit.LocalPositionAtEccentricAnomaly(nodeEccentricAnomaly);

		// Update the planned orbit parameters
		OrbitPlot planOrbit = plannedOrbitLine.GetComponent<OrbitPlot>();
		planOrbit.SetOrbitWithPositionVelocity(nodePosition, newVelocity);
		planOrbit.UpdatePoints();

		// Maneuver Controls
		progradeReadout.SetText((progradeDeltaV * 1000.0).ToString("F1") + " m/s");

		// Maneuver Stats
		double apoAlt = planOrbit.ApoapsisFromFocus() - EarthRadius;
		double periAlt = planOrbit.PeriapsisFromFocus() - EarthRadius;
		double period = planOrbit.OrbitalPeriod();
		playerStatsText.text = "Apoapsis: " + apoAlt.ToString("#,##0") + " km\n" +
			"Periapsis: " + periAlt.ToString("#,##0") + " km\n" +
			"Period: " + OrbitUIHandler.FormattedTime(period) + "\n";

		// Use Ship stats text box for debugging info
		shipStatsText.text = "New velocity: " + newVelocity.magnitude.ToString("F3") + " km/s\n" +
			"vel.X: " + newVelocity.x.ToString("F3") + " km/s\n" + 
			"vel.Y: " + newVelocity.y.ToString("F3") + " km/s\n" + 
			"pos.x: " + nodePosition.x.ToString("F3") + " km\n" + 
			"pos.y: " + nodePosition.y.ToString("F3") + " km\n";
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
