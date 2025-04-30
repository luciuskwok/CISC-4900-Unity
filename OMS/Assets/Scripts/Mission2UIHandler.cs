using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mission2UIHandler : MonoBehaviour
{
	// Text UI
	public Slider progradeSlider;
	public TMP_Text progradeReadout;
	public Slider timingSlider;
	public TMP_Text timingReadout;
	public TMP_Text maneuverStatsText;
	public TMP_Text targetStatsText;
	public TMP_Text infoText;
	
	// Orbit lines
	public GameObject playerOrbitLine;
	public GameObject plannedOrbitLine;
	public GameObject targetOrbitLine;

	// Other UI elements
	public GameObject maneuverNode;
	public GameObject playerNode;
	public GameObject targetNode;
	public GameObject goButton;
	public GameObject successPanel;

	// Maneuver node parameters
	private double progradeDeltaV = 0.1; // km/s
	private double timing = 0.0; // seconds
	private double nodeMeanAnomaly = 0.0 * Kepler.Deg2Rad; // radians

	void Start() {
		// Constants
		const double playerAltitude = 420.0; // km above Earth's surface
		const double playerEcc = 0.0; // circular orbit
		const double playerArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		const double playerMeanAnomaly = -1.0/8.0;
		double playerSMA = Attractor.Earth.radius + playerAltitude;
		const double targetAltitude = 4000.0; // km above surface
		const double targetEcc = 0.0;
		const double targetArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		const double targetMeanAnomaly = 3.0/8.0;
		double targetSMA = Attractor.Earth.radius + targetAltitude;

		// Set prograde slider to initial value
		progradeSlider.SetValueWithoutNotify((float)progradeDeltaV * 1000);
		timingSlider.SetValueWithoutNotify((float)timing);

		// Set up the current player orbit
		OrbitPlot playerOrbit = playerOrbitLine.GetComponent<OrbitPlot>();
		playerOrbit.attractor = Attractor.Earth;
		playerOrbit.SetOrbitalElements(playerEcc, playerSMA, 0, playerArgOfPerifocus, 0);
		playerOrbit.SetMeanAnomaly(playerMeanAnomaly);

		// Set up the target orbit
		OrbitPlot targetOrbit = targetOrbitLine.GetComponent<OrbitPlot>();
		targetOrbit.attractor = Attractor.Earth;
		targetOrbit.SetOrbitalElements(targetEcc, targetSMA, 0, targetArgOfPerifocus, 0);
		targetOrbit.SetMeanAnomaly(targetMeanAnomaly);

		// Target Stats Text
		targetStatsText.text = targetOrbit.ToString();
		
		// Position nodes & update planned orbit
		PositionManeuverNode();
		PositionNodeWithOrbit(playerNode, playerOrbit, playerOrbit.EccentricAnomaly);
		PositionNodeWithOrbit(targetNode, targetOrbit, targetOrbit.EccentricAnomaly);

		// Set the info text
		SetInfoText(false);

		// Hide items that should be hidden
		successPanel.SetActive(false);
		goButton.SetActive(false);
	}

	void Update() {
	}

	void PositionManeuverNode() {
		nodeMeanAnomaly = timing * Kepler.Deg2Rad;

		OrbitPlot playerOrbitPlot = playerOrbitLine.GetComponent<OrbitPlot>();
		double nodeEccAnomaly = playerOrbitPlot.GetEccentricAnomalyFromMean(nodeMeanAnomaly);

		PositionNodeWithOrbit(maneuverNode, playerOrbitPlot, nodeEccAnomaly);
		UpdatePlannedOrbit();
	}

	/// <summary>
	/// Sets the position of the node icon GameObject given an orbit GameObject and eccentric anomaly.
	/// </summary>
	/// <param name="node">GameObject for the node icon.</param>
	/// <param name="orbitPlot">OrbitPlot object for the orbit.</param>
	/// <param name="eccentricAnomaly">The eccentric anomaly in radians from perapsis.</param>
	void PositionNodeWithOrbit(GameObject node, OrbitPlot orbitPlot, double eccentricAnomaly) {
		Vector3 worldPos = orbitPlot.GetWorldPositionAtEccentricAnomaly(eccentricAnomaly);
		node.GetComponent<UINode>().SetWorldPosition(worldPos);
	}

	public void HandleMenuButton() {
		// Chapter Select
		SceneManager.LoadScene(1);
	}

	public void HandleGoButton() {
		// Show the "Success!" message
		successPanel.SetActive(true);
		// Disable the sliders
		progradeSlider.enabled = false;
		timingSlider.enabled = false;
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

	public void TimingDidChange(float value) {
		timing = value;
		PositionManeuverNode();
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

		// Update readouts
		progradeReadout.SetText((progradeDeltaV * 1000.0).ToString("F1") + " m/s");
		timingReadout.SetText(timing.ToString("F0") + "s");

		// Maneuver Stats
		maneuverStatsText.text = planOrbit.ToString();

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
			infoText.text = "This task involves both changing your orbit and timing the maneuver to rendezvous with the target object. Use the sliders below to adjust the delta-V and the timing so that closest approach is small emough for a meeting.";
		} else {
			infoText.text = "Great job! Now click on the Go button to execute the maneuver by firing your engines.";
		}
	}

}
