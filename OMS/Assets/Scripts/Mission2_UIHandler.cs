using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mission2_UIHandler : MonoBehaviour
{
	// Text UI
	public Slider progradeSlider;
	public TMP_Text progradeReadout;
	public Slider timingSlider;
	public TMP_Text timingReadout;
	public TMP_Text maneuverStatsText;
	public TMP_Text targetStatsText;
	public TMP_Text approachStatsText;
	public TMP_Text infoText;
	
	// Orbit lines
	public GameObject playerOrbitLine;
	public GameObject plannedOrbitLine;
	public GameObject targetOrbitLine;

	// Node UI elements
	public GameObject maneuverNode;
	public GameObject playerNode;
	public GameObject targetNode;
	public GameObject approachPlayerNode1;
	public GameObject approachTargetNode1;
	public GameObject approachPlayerNode2;
	public GameObject approachTargetNode2;

	// Other UI elements
	public GameObject goButton;
	public GameObject successPanel;

	// Sound Effects
	public GameObject successSfx;
	public GameObject rocketSfx;
	private bool m_Success = false;

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
		const double playerMeanAnomaly = -45.0 * Kepler.Deg2Rad;
		double playerSMA = Attractor.Earth.radius + playerAltitude;
		const double targetAltitude = 4000.0; // km above surface
		const double targetEcc = 0.0;
		const double targetArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		const double targetMeanAnomaly = 55.0 * Kepler.Deg2Rad;
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
		playerOrbitPlot.PositionNodeAtTime(maneuverNode, GetManeuverTime());
		UpdatePlannedOrbit();
	}

	void PositionSpacecraftNodes(double atTime) {
		playerOrbitPlot.UpdateGradientWithTime(atTime);
		targetOrbitPlot.UpdateGradientWithTime(atTime);
		playerOrbitPlot.PositionNodeAtTime(playerNode, atTime);
		targetOrbitPlot.PositionNodeAtTime(targetNode, atTime);
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
		// Play SFX
		rocketSfx.GetComponent<AudioSource>().Play();
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
		timingReadout.SetText("T+" + StringUtil.FormatTimeWithLabels(maneuverTime));

		// Update the planned orbit resulting from maneuver
		double maneuverMeanAnomaly = maneuverSliderValue * Kepler.Deg2Rad;
		plannedOrbitPlot.SetOrbitByManeuver(playerOrbitPlot, maneuverMeanAnomaly, maneuverTime, progradeDeltaV, 0, 0);	

		// Maneuver Stats
		maneuverStatsText.text = plannedOrbitPlot.ToString();

		CalculateClosestApproach(maneuverTime);
	}

	void CalculateClosestApproach(double maneuverTime)
	{
		double time1, time2;
		int count = plannedOrbitPlot.CalculateClosestApproachesToCircularOrbit(maneuverTime, targetOrbitPlot, out time1, out time2);

		// Position or hide approach nodes
		plannedOrbitPlot.PositionApproachNodes(approachPlayerNode1, approachTargetNode1, targetOrbitPlot, time1);
		plannedOrbitPlot.PositionApproachNodes(approachPlayerNode2, approachTargetNode2, targetOrbitPlot, time2);

		// Calculate distances
		double distance1 = plannedOrbitPlot.DistanceToTargetAtTime(targetOrbitPlot, time1);
		double distance2 = plannedOrbitPlot.DistanceToTargetAtTime(targetOrbitPlot, time2);

		if (count == 0) {
			approachStatsText.text = "None";
		} else if (count == 1) {
			approachStatsText.text = "Distance: " + distance1.ToString("#,###.0") + " km\n" +
				"Time: " + StringUtil.FormatTimeWithLabels(time1);
		} else {
			approachStatsText.text = "Distance: " + distance1.ToString("#,###.0") + " km\n" +
				"Time: " + StringUtil.FormatTimeWithLabels(time1) + "\n" +
				"\nSecond Approach:\n" +
				"Distance: " + distance2.ToString("#,###.0") + " km\n" +
				"Time: " + StringUtil.FormatTimeWithLabels(time2) + "\n";
		}

		// GO button
		const double maxDistance = 80.0; // km
		SetSuccess(distance1 <= maxDistance || distance2 <= maxDistance);
	}

	void SetSuccess(bool success) {
		goButton.SetActive(success);
		SetInfoText(success);

		if (success && !m_Success) {
			successSfx.GetComponent<AudioSource>().Play();
		}
		m_Success = success;
	}

	void SetInfoText(bool success) {
		if (!success) {
			infoText.text = "This task involves both changing your orbit and timing the maneuver to rendezvous with the target object. Use the sliders below to adjust the delta-V and the timing so that closest approach is small emough for a meeting.";
		} else {
			infoText.text = "Great job! Now click on the Go button to execute the maneuver by firing your engines.";
		}
	}

}
