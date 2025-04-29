using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mission1UIHandler : MonoBehaviour
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

	// Maneuver node parameters
	private double progradeDeltaV = 0.0; // m/s
	private double nodeMeanAnomaly = 0.0 * Kepler.Deg2Rad; // radians

	void Start() {
		// Constants
		const double playerAltitude = 420.0; // km above Earth's surface
		const double playerEcc = 0.0; // circular orbit
		const double playerArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		double playerSMA = Attractor.Earth.radius + playerAltitude;
		double targetApoapsisAlt = 4000.0; // km above surface

		// Set prograde slider to initial value
		progradeSlider.SetValueWithoutNotify((float)progradeDeltaV);

		// Set up the current player orbit
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		playerOrbit.attractor = Attractor.Earth;
		playerOrbit.SetOrbitalElements(playerEcc, playerSMA, 0, 0, playerArgOfPerifocus, 0);

		// Set up the target orbit
		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		targetOrbit.attractor = Attractor.Earth;
		targetOrbit.SetOrbitByAltitudes(playerAltitude, targetApoapsisAlt, playerArgOfPerifocus);

		// Target Stats Text
		targetStatsText.text = targetOrbit.ToString();
		
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
		double nodeEccentricAnomaly = playerOrbit.GetEccentricAnomalyFromMean(nodeMeanAnomaly);
		Vector3d originalVelocity = playerOrbit.Orbit.GetVelocityAtEccentricAnomaly(nodeEccentricAnomaly);
		// Add prograde delta-V
		Vector3d progradeDirection = originalVelocity.normalized;
		Vector3d deltaVelocity = progradeDirection * progradeDeltaV;
		// Calculate the new orbit based on the new velocity vector
		Vector3d newVelocity = originalVelocity + deltaVelocity;
		Vector3d nodePosition = playerOrbit.Orbit.GetFocalPositionAtEccentricAnomaly(nodeEccentricAnomaly);

		// Update the planned orbit parameters
		OrbitPlot planOrbit = plannedOrbitLine.GetComponent<OrbitPlot>();
		planOrbit.attractor = Attractor.Earth;
		planOrbit.SetOrbitByThrow(nodePosition, newVelocity);

		// Maneuver Controls
		progradeReadout.SetText((progradeDeltaV * 1000.0).ToString("F1") + " m/s");

		// Maneuver Stats
		playerStatsText.text = planOrbit.ToString();

		// Target orbit parameter
		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		double targetApoapsis = targetOrbit.Orbit.ApoapsisDistance;

		// Check if planned orbit is within tolerances and enable Go button
		double planApoapsis = planOrbit.Orbit.ApoapsisDistance;
		if (Math.Abs(targetApoapsis - planApoapsis) < targetApoapsis * 0.02) {
			goButton.SetActive(true);
			SetInfoText(true);
		} else {
			goButton.SetActive(false);
			SetInfoText(false);
		}
	}

	void SetInfoText(bool success) {
		if (!success) {
			infoText.text = "Welcome! Your first task is to adjust your orbit to match the target orbit. Use the slider below to adjust the delta-V, or the change in velocity, for this maneuver node, so that the orbits line up.";
		} else {
			infoText.text = "Great job! Now click on the Go button to execute the maneuver by firing your engines.";
		}
	}

}
