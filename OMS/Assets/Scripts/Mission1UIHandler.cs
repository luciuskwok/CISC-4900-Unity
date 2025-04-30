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
	public GameObject successPanel;

	// Maneuver node parameters
	private double progradeDeltaV = 0.0; // m/s
	private double nodeMeanAnomaly = 0.0 * Kepler.Deg2Rad; // radians

	// Cached Monobehaviour script objects
	private OrbitPlot playerOrbitPlot;
	private OrbitPlot plannedOrbitPlot;
	private OrbitPlot targetOrbitPlot;

	void Start() {
		// Constants
		const double playerAltitude = 420.0; // km above Earth's surface
		const double playerEcc = 0.0; // circular orbit
		const double playerArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		double playerSMA = Attractor.Earth.radius + playerAltitude;
		double targetApoapsisAlt = 4000.0; // km above surface

		// Get and cache OrbitPlot objects
		playerOrbitPlot = playerOrbitLine.GetComponent<OrbitPlot>();
		plannedOrbitPlot = plannedOrbitLine.GetComponent<OrbitPlot>();
		targetOrbitPlot = targetOrbitLine.GetComponent<OrbitPlot>();

		// Set prograde slider to initial value
		progradeSlider.SetValueWithoutNotify((float)progradeDeltaV);

		// Set up the current player orbit
		playerOrbitPlot.attractor = Attractor.Earth;
		playerOrbitPlot.SetOrbitalElements(playerEcc, playerSMA, 0, playerArgOfPerifocus, 0);

		// Set up planned orbit
		plannedOrbitPlot.attractor = Attractor.Earth;

		// Set up the target orbit
		targetOrbitPlot.attractor = Attractor.Earth;
		targetOrbitPlot.SetOrbitByAltitudes(playerAltitude, targetApoapsisAlt, playerArgOfPerifocus);

		// Target Stats Text
		targetStatsText.text = targetOrbitPlot.ToString();
		
		// Update maneuver node & planned orbit
		PositionManeuverNode();
	
		// Set the info text
		SetInfoText(false);

		// Hide items that should be hidden
		successPanel.SetActive(false);
		goButton.SetActive(false);
	}

	void Update() {
		//AnimateManeuverNode();
	}

	void AnimateManeuverNode() {
		const double nodeAnimationTime = 30.0;
		double x = Time.timeSinceLevelLoadAsDouble / nodeAnimationTime % 1.0f;

		// Rotate the maneuver node around for testing
		nodeMeanAnomaly = x * Kepler.PI_2;
		PositionManeuverNode();
	}

	void PositionManeuverNode() {
		double nodeEccAnomaly = playerOrbitPlot.Orbit.ConvertMeanAnomalyToEccentric(nodeMeanAnomaly);
		Vector3 worldPos = playerOrbitPlot.GetWorldPositionAtEccentricAnomaly(nodeEccAnomaly);

		UINode node = maneuverNode.GetComponent<UINode>();
		node.SetWorldPosition(worldPos);
		UpdatePlannedOrbit();
	}

	public void HandleMenuButton() {
		// Chapter Select
		SceneManager.LoadScene(1);
	}

	public void HandleGoButton() {
		// Show the "Success!" message
		successPanel.SetActive(true);
		// Disable the slider
		progradeSlider.enabled = false;
		// Hide the GO button
		goButton.SetActive(false);
	}

	public void HandleNextMissionButton() {
		// Go to next scene
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
	}

	public void ProgradeDidChange(float value) {
		progradeDeltaV = value / 1000.0; // Convert value from m/s to km/s
		UpdatePlannedOrbit();
	}

	void UpdatePlannedOrbit() {
		// Update the planned orbit resulting from maneuver
		plannedOrbitPlot.SetOrbitByManeuver(playerOrbitPlot, nodeMeanAnomaly, 0, progradeDeltaV, 0, 0);	

		// Maneuver Controls
		progradeReadout.SetText((progradeDeltaV * 1000.0).ToString("F1") + " m/s");

		// Maneuver Stats
		playerStatsText.text = plannedOrbitPlot.ToString();

		// Target orbit parameter
		double targetApoapsis = targetOrbitPlot.Orbit.ApoapsisDistance;

		// Check if planned orbit is within tolerances and enable Go button
		double planApoapsis = plannedOrbitPlot.Orbit.ApoapsisDistance;
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
