using System;
using TMPro;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mission3_UIHandler : MonoBehaviour
{
	// Text UI
	public Slider progradeSlider;
	public TMP_Text progradeReadout;
	public Slider timingSlider;
	public TMP_Text timingReadout;
	public TMP_Text maneuverStatsText;
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
	public GameObject moon;

	// Maneuver node parameters
	private double progradeDeltaV = 2.0; // km/s
	private double maneuverSliderValue = 0.0; // degrees

	// Animation parameters
	private double animationTime = 0.0;
	private readonly double animationTimeScale = 300.0;

	// Cached Monobehaviour script objects
	private OrbitPlot playerOrbitPlot;
	private OrbitPlot plannedOrbitPlot;
	private OrbitPlot targetOrbitPlot;

	void Start()
	{
		// Constants
		const double playerAltitude = 420.0; // km above Earth's surface
		const double playerEcc = 0.0; // circular orbit
		const double playerArgOfPerifocus = 180.0 * Kepler.Deg2Rad;
		const double playerMeanAnomaly = -22.5 * Kepler.Deg2Rad;
		double playerSMA = Attractor.Earth.radius + playerAltitude;
		const double targetSMA = 384400; // km from Earth's center to Moon's center
		const double targetEcc = 0.0; // Circular orbit for simplicity
		const double targetArgOfPerifocus = 0.0 * Kepler.Deg2Rad;
		const double targetMeanAnomaly = 0.0 * Kepler.Deg2Rad;

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

		// Position nodes & update planned orbit
		PositionManeuverNode();
		PositionSpacecraftNodes(0);

		// Set the info text
		SetInfoText(false);

		// Hide items that should be hidden
		successPanel.SetActive(false);
		goButton.SetActive(false);
	}

	void Update()
	{
		//AnimateSpacecraftPositions();		
	}

	void AnimateSpacecraftPositions()
	{
		animationTime += Time.deltaTime * animationTimeScale;
		PositionSpacecraftNodes(animationTime);
	}

	double GetManeuverTime()
	{
		double maneuverMeanAnomaly = maneuverSliderValue * Kepler.Deg2Rad;
		double t1 = playerOrbitPlot.Orbit.periapsisTime;
		double t2 = maneuverMeanAnomaly / playerOrbitPlot.Orbit.MeanMotion;
		return t1 + t2;
	}

	void PositionManeuverNode()
	{
		PositionNodeWithOrbit(maneuverNode, playerOrbitPlot, GetManeuverTime());
		UpdatePlannedOrbit();
	}

	void PositionSpacecraftNodes(double atTime)
	{
		playerOrbitPlot.UpdateGradientWithTime(atTime);
		targetOrbitPlot.UpdateGradientWithTime(atTime);
		PositionNodeWithOrbit(playerNode, playerOrbitPlot, atTime);
		PositionNodeWithOrbit(targetNode, targetOrbitPlot, atTime);

		// Also move and rotate the Moon
		Vector3 moonPosition = targetOrbitPlot.GetWorldPositionAtTime(atTime);
		Quaternion moonRotation = new Quaternion();
		float yRotation = 90.0f + (float)targetOrbitPlot.Orbit.GetEccentricAnomalyAtTime(atTime);
		moonRotation.eulerAngles = new Vector3(0, yRotation, 0);
		moon.transform.SetLocalPositionAndRotation(moonPosition, moonRotation);
	}

	/// <summary>
	/// Sets the position of the node icon GameObject given an orbit GameObject and eccentric anomaly.
	/// </summary>
	/// <param name="node">GameObject for the node icon.</param>
	/// <param name="orbitPlot">OrbitPlot object for the orbit.</param>
	/// <param name="eccentricAnomaly">The eccentric anomaly in radians from perapsis.</param>
	void PositionNodeWithOrbit(GameObject node, OrbitPlot orbitPlot, double atTime)
	{
		Vector3 worldPos = orbitPlot.GetWorldPositionAtTime(atTime);
		node.GetComponent<UINode>().SetWorldPosition(worldPos);
	}

	public void HandleMenuButton()
	{
		// Chapter Select
		SceneManager.LoadScene(1);
	}

	public void HandleGoButton()
	{
		// Show the "Success!" message
		successPanel.SetActive(true);
		// Disable the sliders
		progradeSlider.enabled = false;
		timingSlider.enabled = false;
		// Hide the GO button
		goButton.SetActive(false);
	}

	public void HandleNextMissionButton()
	{
		// Go to next scene
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void ProgradeDidChange(float value)
	{
		progradeDeltaV = value / 1000.0; // Convert value from m/s to km/s
		UpdatePlannedOrbit();
	}

	public void TimingDidChange(float value)
	{
		maneuverSliderValue = value;
		PositionManeuverNode();
	}

	void UpdatePlannedOrbit()
	{
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

	void CalculateClosestApproach(double maneuverTime)
	{
		//double targetPeriapsis = targetOrbitPlot.Orbit.PeriapsisDistance;
		double planPeriapsis = targetOrbitPlot.Orbit.PeriapsisDistance;
		double targetApoapsis = targetOrbitPlot.Orbit.ApoapsisDistance;
		double planApoapsis = plannedOrbitPlot.Orbit.ApoapsisDistance;
		if (planApoapsis < targetApoapsis * 0.95 || planPeriapsis > targetApoapsis)
		{
			// No closest approach. Hide approach nodes and set text.
			approachPlayerNode1.SetActive(false);
			approachTargetNode1.SetActive(false);
			approachPlayerNode2.SetActive(false);
			approachTargetNode2.SetActive(false);

			approachStatsText.text = "None";
			return;
		}

		// Get the time of apoapsis, and then get the distance from the apoapsis to the target 
		double peTime = plannedOrbitPlot.Orbit.periapsisTime;
		double apTime = peTime + 0.5 * plannedOrbitPlot.Orbit.OrbitalPeriod;
		double distance1 = double.PositiveInfinity;
		double distance2 = double.PositiveInfinity;

		if (planApoapsis <= targetApoapsis)
		{
			// Use the planned apoapsis as a shortcut for finding the closest approach.
			// This works because the target orbit is circular and on the same plane as the planned orbit.

			Vector3d plannedPosition = plannedOrbitPlot.GetFocalPositionAtTime(apTime);
			Vector3d targetPosition = targetOrbitPlot.GetFocalPositionAtTime(apTime);
			distance1 = Vector3d.Distance(plannedPosition, targetPosition);

			approachStatsText.text = "Distance: " + distance1.ToString("F3") + " km\n" +
				"Time: " + OrbitPlot.FormattedTime(apTime);

			// Position and show nodes for first approach
			PositionNodeWithOrbit(approachPlayerNode1, plannedOrbitPlot, apTime);
			PositionNodeWithOrbit(approachTargetNode1, targetOrbitPlot, apTime);
			approachPlayerNode1.SetActive(true);
			approachTargetNode1.SetActive(true);

			// Hide nodes for second approach
			approachPlayerNode2.SetActive(false);
			approachTargetNode2.SetActive(false);

		}
		else
		{
			// Because the target orbit is circular and on the same plane as the planned orbit, getting the intersections at a specific distance will work.
			double true1 = plannedOrbitPlot.Orbit.TrueAnomalyForDistance(targetApoapsis);
			double true2 = Kepler.PI_2 - true1;

			double mean1 = plannedOrbitPlot.Orbit.ConvertTrueAnomalyToMean(true1);
			double mean2 = plannedOrbitPlot.Orbit.ConvertTrueAnomalyToMean(true2);

			double time1 = mean1 / plannedOrbitPlot.Orbit.MeanMotion + maneuverTime;
			double time2 = mean2 / plannedOrbitPlot.Orbit.MeanMotion + maneuverTime;

			Vector3d plannedPos1 = plannedOrbitPlot.GetFocalPositionAtTime(time1);
			Vector3d targetPos1 = targetOrbitPlot.GetFocalPositionAtTime(time1);
			distance1 = Vector3d.Distance(plannedPos1, targetPos1);

			Vector3d plannedPos2 = plannedOrbitPlot.GetFocalPositionAtTime(time2);
			Vector3d targetPos2 = targetOrbitPlot.GetFocalPositionAtTime(time2);
			distance2 = Vector3d.Distance(plannedPos2, targetPos2);

			approachStatsText.text = "Distance: " + distance1.ToString("F3") + " km\n" +
				"Time: " + OrbitPlot.FormattedTime(time1) + "\n" +
				"\nSecond Approach:\n" +
				"Distance: " + distance2.ToString("F3") + " km\n" +
				"Time: " + OrbitPlot.FormattedTime(time2) + "\n";

			// Position nodes for both approaches
			PositionNodeWithOrbit(approachPlayerNode1, plannedOrbitPlot, time1);
			PositionNodeWithOrbit(approachTargetNode1, targetOrbitPlot, time1);
			PositionNodeWithOrbit(approachPlayerNode2, plannedOrbitPlot, time2);
			PositionNodeWithOrbit(approachTargetNode2, targetOrbitPlot, time2);
			approachPlayerNode1.SetActive(true);
			approachTargetNode1.SetActive(true);
			approachPlayerNode2.SetActive(true);
			approachTargetNode2.SetActive(true);
		}

		// GO button
		const double maxDistance = 1000.0; // km
		SetSuccess(distance1 <= maxDistance || distance2 <= maxDistance);
	}

	void SetSuccess(bool success)
	{
		goButton.SetActive(success);
		SetInfoText(success);
	}

	void SetInfoText(bool success)
	{
		if (!success)
		{
			infoText.text = "This mission requires that you plan a fly-by of the Moon. Adjust the sliders below to bring the apoapsis of your planned orbit close to the Moon.";
		}
		else
		{
			infoText.text = "Great job! Now click on the Go button to execute the maneuver by firing your engines.";
		}
	}

}
