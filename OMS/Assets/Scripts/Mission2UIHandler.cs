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
	private double maneuverSliderValue = 0.0; // degrees

	// Animation parameters
	private double animationTime = 0.0;
	private readonly double animationTimeScale = 300.0;

	// Cached Monobehaviour script objects
	private OrbitPlot playerOrbitPlot;
	private OrbitPlot plannedOrbitPlot;
	private OrbitPlot targetOrbitPlot;

	void Start() {
		// Constants
		const double playerAltitude = 420.0; // km above Earth's surface
		const double playerEcc = 0.0; // circular orbit
		const double playerArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		const double playerMeanAnomaly = -30.0 * Kepler.Deg2Rad;
		double playerSMA = Attractor.Earth.radius + playerAltitude;
		const double targetAltitude = 4000.0; // km above surface
		const double targetEcc = 0.0;
		const double targetArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		const double targetMeanAnomaly = 75.0 * Kepler.Deg2Rad;
		double targetSMA = Attractor.Earth.radius + targetAltitude;

		// Get and cache OrbitPlot objects
		playerOrbitPlot = playerOrbitLine.GetComponent<OrbitPlot>();
		plannedOrbitPlot = plannedOrbitLine.GetComponent<OrbitPlot>();
		targetOrbitPlot = targetOrbitLine.GetComponent<OrbitPlot>();

		// Set prograde slider to initial value
		progradeSlider.SetValueWithoutNotify((float)progradeDeltaV * 1000);
		timingSlider.SetValueWithoutNotify((float)maneuverSliderValue);

		// Set up the current player orbit
		playerOrbitPlot.attractor = Attractor.Earth;
		playerOrbitPlot.SetOrbitalElements(playerEcc, playerSMA, 0, playerArgOfPerifocus, 0);
		playerOrbitPlot.Orbit.SetPeriapsisTimeWithMeanAnomaly(playerMeanAnomaly, 0);

		// Set up planned orbit
		plannedOrbitPlot.attractor = Attractor.Earth;

		// Set up the target orbit
		targetOrbitPlot.attractor = Attractor.Earth;
		targetOrbitPlot.SetOrbitalElements(targetEcc, targetSMA, 0, targetArgOfPerifocus, 0);
		targetOrbitPlot.Orbit.SetPeriapsisTimeWithMeanAnomaly(targetMeanAnomaly, 0);
	
		// Target Stats Text
		targetStatsText.text = targetOrbitPlot.ToString();
		
		// Position nodes & update planned orbit
		PositionManeuverNode();
		PositionSpacecraftNodes(0);

		// Set the info text
		SetInfoText(false);

		// Hide items that should be hidden
		successPanel.SetActive(false);
		goButton.SetActive(false);
	}

	void Update() {
		//AnimateSpacecraftPositions();		
	}

	void AnimateSpacecraftPositions() {
		animationTime += Time.deltaTime * animationTimeScale;
		PositionSpacecraftNodes(animationTime);
	}

	double GetManeuverTime() {
		double maneuverMeanAnomaly = maneuverSliderValue * Kepler.Deg2Rad;
		double t1 = playerOrbitPlot.Orbit.periapsisTime;
		double t2 = maneuverMeanAnomaly / playerOrbitPlot.Orbit.MeanMotion;
		return t1 + t2;
	}

	void PositionManeuverNode() {
		PositionNodeWithOrbit(maneuverNode, playerOrbitPlot, GetManeuverTime());
		UpdatePlannedOrbit();
	}

	void PositionSpacecraftNodes(double atTime) {
		playerOrbitPlot.UpdateGradientWithTime(atTime);
		targetOrbitPlot.UpdateGradientWithTime(atTime);
		PositionNodeWithOrbit(playerNode, playerOrbitPlot, atTime);
		PositionNodeWithOrbit(targetNode, targetOrbitPlot, atTime);
	}

	/// <summary>
	/// Sets the position of the node icon GameObject given an orbit GameObject and eccentric anomaly.
	/// </summary>
	/// <param name="node">GameObject for the node icon.</param>
	/// <param name="orbitPlot">OrbitPlot object for the orbit.</param>
	/// <param name="eccentricAnomaly">The eccentric anomaly in radians from perapsis.</param>
	void PositionNodeWithOrbit(GameObject node, OrbitPlot orbitPlot, double atTime) {
		Vector3 worldPos = orbitPlot.GetWorldPositionAtTime(atTime);
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
		maneuverSliderValue = value;
		PositionManeuverNode();
	}

	void UpdatePlannedOrbit() {
		double maneuverTime = GetManeuverTime();

		// Update delta-V readout
		progradeReadout.SetText((progradeDeltaV * 1000.0).ToString("F1") + " m/s");

		// Update time readout
		timingReadout.SetText("T+" + OrbitPlot.FormattedTime(maneuverTime));

		// Update the planned orbit resulting from maneuver
		double maneuverMeanAnomaly = maneuverSliderValue * Kepler.Deg2Rad;
		plannedOrbitPlot.SetOrbitByManeuver(playerOrbitPlot, maneuverMeanAnomaly, maneuverTime, progradeDeltaV, 0, 0);	

		// Maneuver Stats
		maneuverStatsText.text = plannedOrbitPlot.ToString();

		CalculateClosestApproach(maneuverTime);
	}

	void CalculateClosestApproach(double maneuverTime) {
		double targetApoapsis = targetOrbitPlot.Orbit.ApoapsisDistance;
		double planApoapsis = plannedOrbitPlot.Orbit.ApoapsisDistance;
		if (planApoapsis < targetApoapsis * 0.95) return;

		if (planApoapsis <= targetApoapsis) {
			// Calculate the one closest approach at the apoapsis

		}

	}

	void SetSuccess(bool success) {
		goButton.SetActive(success);
		SetInfoText(success);
	}

	void SetInfoText(bool success) {
		if (!success) {
			infoText.text = "This task involves both changing your orbit and timing the maneuver to rendezvous with the target object. Use the sliders below to adjust the delta-V and the timing so that closest approach is small emough for a meeting.";
		} else {
			infoText.text = "Great job! Now click on the Go button to execute the maneuver by firing your engines.";
		}
	}

}
