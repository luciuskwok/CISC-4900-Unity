using UnityEngine;
using System;

public class OrbitPlot : MonoBehaviour
{
	public Color color;
	public float maxAlpha = 1.0f;
	public float minAlpha = 0.05f;

	// Note: the coordinate system that is conventionally used for orbital mechanics has the ecliptic on the x-y plane, and positive z is towards the North Pole Star. The positive x axis is the direction of the Sun as seen from the Earth at the (spring) vernal equinox. This means that the Earth is at longitude 0 at the autumnal equinox, and at 180 at the spring equinox.
	// Unity uses the convention that the x-z plane is horizontal, and positive y points up. So the y and z axes are swapped. 
	// As for the x axis, that can be arbitrarily chosen as long as it is consistent throughout the solar system for this project. 

	private Orbit m_Orbit;
	public Orbit Orbit {
		get { return m_Orbit; }
	}

	public Attractor attractor; 

	public bool animate = true;
	public double animationTimeScale = 1200.0; // factor to speed up time for the animation
	private double animationTime = 0.0;
	private readonly int pointCount = 180;

	public double PeriapsisTime {
		get { return m_Orbit.periapsisTime; }
		set { m_Orbit.periapsisTime = value; }
	}

	/// <summary>
	/// Sets the orbital parameters given the orbital elements. Resets the anomaly to zero.
	/// </summary>
	public void SetOrbitalElements(double eccentricity, double semiMajorAxis, double inclination, double argOfPerifocus, double ascendingNode) 
	{
		m_Orbit = new Orbit(eccentricity, semiMajorAxis, inclination, argOfPerifocus, ascendingNode, attractor);
		UpdateLineRenderer();
	}

	/// <summary>
	/// Sets the orbital parameters given the periapsis and apoapsis altitude, and the argument of perifocus. Resets the anomaly to zero.
	/// </summary>
	public void SetOrbitByAltitudes(double periapsisAltitude, double apoapsisAltitude, double argOfPerifocus) 
	{
		double pe = periapsisAltitude + attractor.radius; // km
		double ap = apoapsisAltitude + attractor.radius; // km
		double a = (pe + ap) / 2.0;
		double e = 1.0 - (pe / a);

		SetOrbitalElements(e, a, 0, argOfPerifocus, 0);
	}

	/// <summary>
	/// Sets the orbital parameters to the result of a maneuver on another orbit.
	/// </summary>
	/// <param name="originalOrbit">The original orbit the maneuver is on.</param>
	/// <param name="meanAnomaly">The position of the maneuver on the original orbit.</param>
	/// <param name="atTime">The point in time that the maneuver occurs.</param>
	/// <param name="prograde">The prograde delta-V.</param>
	/// <param name="normal">The normal delta-V.</param>
	/// <param name="inward">The inward delta-V.</param>
	public void SetOrbitByManeuver(OrbitPlot originalOrbit, double meanAnomaly, double atTime, double prograde, double normal, double inward) 
	{
		// Get original velocity and position at maneuver node
		double eccAnomaly = originalOrbit.Orbit.ConvertMeanAnomalyToEccentric(meanAnomaly);
		Vector3d originalVelocity = originalOrbit.Orbit.GetVelocityAtEccentricAnomaly(eccAnomaly);

		// Add delta-V for each direction
		Vector3d progradeDirection = originalVelocity.normalized;
		Vector3d normalDirection = originalOrbit.Orbit.OrbitNormal.normalized;
		Vector3d inwardDirection = progradeDirection.Cross(normalDirection).normalized;
		Vector3d deltaVelocity = progradeDirection * prograde + normalDirection * normal + inwardDirection * inward;

		// Calculate the new orbit based on the new velocity vector
		Vector3d newVelocity = originalVelocity + deltaVelocity;
		Vector3d nodePosition = originalOrbit.Orbit.GetFocalPositionAtEccentricAnomaly(eccAnomaly);

		// Update the planned orbit parameters
		m_Orbit = new Orbit(nodePosition, newVelocity, atTime, attractor);

		// Update line renderer and gradient
		UpdateLineRenderer();
		UpdateGradientWithTime(atTime);
	}

	void Update() {
		if (animate) {
			animationTime += Time.deltaTime * animationTimeScale;
			UpdateGradientWithTime(animationTime);
		}
	}

	/// <summary>
	/// Update the color gradient to show the position on the orbit at the given time.
	/// </summary>
	/// <param name="atTime">The point in time that the gradient represents.</param>
	public void UpdateGradientWithTime(double atTime) {
		double meanAnomaly = Orbit.GetMeanAnomalyAtTime(atTime);
		double eccentricAnomaly = Kepler.ConvertMeanAnomalyToEccentric(meanAnomaly, m_Orbit.Eccentricity);
		UpdateGradientWithEccentricAnomaly(eccentricAnomaly);
	}

	/// <summary>
	/// Update the orbit plot and animation time.
	/// </summary>
	public void UpdateLineRenderer() {
		// Unity will show an error if objects are too large or too far away from the world origin, so the max distance must be small. 
		double maxDistance = attractor.influence; // km

		bool loop = (Orbit.ApoapsisDistance <= maxDistance) && (Orbit.Eccentricity < 1.0);

		// Get points and convert from double to float
		Vector3d[] pointsd = m_Orbit.GetOrbitPoints(pointCount, maxDistance);
		int count = pointsd.Length;
		Vector3[] points = new Vector3[count];
		for (int i = 0; i < count; i++) {
			points[i] = pointsd[i].Vector3;
		}

		var lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.loop = loop;
		lineRenderer.positionCount = count;
		lineRenderer.SetPositions(points);
	}

	/// <summary>
	/// Updates the color gradient to end at the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly where the gradient should end.</param>
	public void UpdateGradientWithEccentricAnomaly(double eccentricAnomaly) {
		// Convert radians to range 0.0 to 1.0
		// Rotate 180 degrees because periapsis is set to middle of the line
		double trueAnomaly = Kepler.ConvertEccentricAnomalyToTrue(eccentricAnomaly, Orbit.Eccentricity);
		float x1 = (float)(trueAnomaly / Kepler.PI_2 + 0.5);
		x1 = x1 % 1.0f;
		if (x1 < 0.0f) x1 = 1.0f - x1;

		//Debug.Log("eccAnomaly=" + (m_Orbit.EccentricAnomaly * Kepler.Rad2Deg).ToString("F1") + "° trueAnomaly=" + (trueAnomaly * Kepler.Rad2Deg).ToString("F1") + "° x1=" + x1.ToString("F3"));

		// Calculate the locations and alpha values for a fade
		float x2 = x1 + 1.0f / pointCount;
		float a1 = maxAlpha;
		float a2 = minAlpha;
		float a0 = a2 + (a1 - a2) * (1.0f - x1);
		float a3 = a2 + (a1 - a2) * (1.0f - x2);

		if (x2 > 1.0f) {
			x2 = 0.0f;
			a3 = a2;
		}

		var gradient = new Gradient();
		gradient.SetKeys(
			// Use the same color for entire line
			new GradientColorKey[] { 
				new(color, 0.0f), 
				new(color, 1.0f) },
			// Vary the alpha 
			new GradientAlphaKey[] { 
				new(a0, 0.0f),
				new(a1, x1),
				new(a2, x2),
				new(a3, 1.0f),
			}
		);

		var lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.colorGradient = gradient;
	}

	/// <summary>
	/// Gets the position on the orbit in terms of Unity's world space, given the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly as radians from periapsis.</param>
	/// <returns>World position vector.</returns>
	public Vector3 GetWorldPositionAtEccentricAnomaly(double eccentricAnomaly) {
		// Get the local coordinates of the node and convert to world coordinates
		Vector3d localPos = Orbit.GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
		return gameObject.transform.TransformPoint(localPos.Vector3);
	}

	/// <summary>
	/// Gets the position on the orbit in terms of Unity's world space, given a point in time.
	/// </summary>
	/// <param name="atTime">The point in time.</param>
	/// <returns>World position vector.</returns>
	public Vector3 GetWorldPositionAtTime(double atTime) {
		double eccAnomaly = Orbit.GetEccentricAnomalyAtTime(atTime);
		return GetWorldPositionAtEccentricAnomaly(eccAnomaly);
	}

	/// <summary>
	/// Sets the position of the node icon GameObject given a point in time.
	/// </summary>
	/// <param name="node">GameObject for the node icon.</param>
	/// <param name="orbitPlot">OrbitPlot object for the orbit.</param>
	/// <param name="atTime">Point in time.</param>
	public void PositionNodeAtTime(GameObject node, double atTime) {
		Vector3 worldPos = GetWorldPositionAtTime(atTime);
		node.GetComponent<UINode>().SetWorldPosition(worldPos);
	}

	/// <summary>
	/// Sets the position of a pair of player and target nodes at a given time in their respective orbits. Pass in NaN for atTime to hide the nodes.
	/// </summary>
	public void PositionApproachNodes(GameObject playerNode, GameObject targetNode, OrbitPlot targetOrbit, double atTime) 
	{
		if (double.IsNaN(atTime)) {
			playerNode.SetActive(false);
			targetNode.SetActive(false);
		} else {
			// Player node
			PositionNodeAtTime(playerNode, atTime);
			targetOrbit.PositionNodeAtTime(targetNode, atTime);
			playerNode.SetActive(true);
			targetNode.SetActive(true);
		}
	}

	/// <summary>
	/// Calculates the distance between two objects at a given time in their orbits.
	/// </summary>
	public double DistanceToTargetAtTime(OrbitPlot targetOrbit, double atTime) {
		if (double.IsNaN(atTime)) return double.PositiveInfinity;

		Vector3d pos1 = Orbit.GetFocalPositionAtTime(atTime);
		Vector3d pos2 = targetOrbit.Orbit.GetFocalPositionAtTime(atTime);
		return Vector3d.Distance(pos1, pos2);
	}

	/// <summary>
	/// Gets up to 2 closest approaches to the target orbit.
	/// </summary>
	/// <returns>Number of valid times</returns>
	public int CalculateClosestApproachesToCircularOrbit(double maneuverTime, OrbitPlot target, out double time1, out double time2) 
	{
		time1 = double.NaN;
		time2 = double.NaN;

		double myPeriapsis = Orbit.PeriapsisDistance;
		double targetRadius = target.Orbit.ApoapsisDistance;
		double myApoapsis = Orbit.ApoapsisDistance;
		if (myApoapsis < targetRadius * 0.95 || myPeriapsis > targetRadius) 
		{
			return 0;
		}

		// My orbit is elliptical and smaller than the target orbit
		if (myApoapsis <= targetRadius) {
			double peTime = Orbit.periapsisTime; // TODO: handle case where maneuverTime is past this time
			double apTime = peTime + 0.5 * Orbit.OrbitalPeriod;
			time1 = apTime;
			return 1;
		}

		// My orbit is elliptical and the apoapsis exceeds the radius 
		// Because the target orbit is circular and on the same plane as the planned orbit, getting the intersections at a specific distance will work.
		double true1 = Orbit.TrueAnomalyForDistance(targetRadius);
		double mean1 = Orbit.ConvertTrueAnomalyToMean(true1);
		time1 = mean1 / Orbit.MeanMotion + maneuverTime;

		// For non-elliptical orbits, only return one approach
		if (Orbit.Eccentricity >= 1.0) {
			return 1;
		}

		// For elliptical orbits, also return the second approach
		double true2 = Kepler.PI_2 - true1;
		double mean2 = Orbit.ConvertTrueAnomalyToMean(true2);
		time2 = mean2 / Orbit.MeanMotion + maneuverTime;
		return 2;
	}

	public override String ToString() {
		double ap = Orbit.ApoapsisAltitude;
		double pe = Orbit.PeriapsisAltitude;
		double period = Orbit.OrbitalPeriod;
		return "Apoapsis: " + ap.ToString("#,##0") + " km\n" +
			"Periapsis: " + pe.ToString("#,##0") + " km\n" +
			"Period: " + StringUtil.FormatTimeWithLabels(period) + "\n";
	}


}
