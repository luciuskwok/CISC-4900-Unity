using UnityEngine;
using System;

public class OrbitPlot : MonoBehaviour
{
	public Color color;

	// Note: the coordinate system that is conventionally used for orbital mechanics has the ecliptic on the x-y plane, and positive z is towards the North Pole Star. The positive x axis is the direction of the Sun as seen from the Earth at the (spring) vernal equinox. This means that the Earth is at longitude 0 at the autumnal equinox, and at 180 at the spring equinox.
	// Unity uses the convention that the x-z plane is horizontal, and positive y points up. So the y and z axes are swapped. 
	// As for the x axis, that can be arbitrarily chosen as long as it is consistent throughout the solar system for this project. 

	private Orbit m_Orbit;
	public Orbit Orbit {
		get { return m_Orbit; }
	}

	public Attractor attractor; 

	public bool animate = true;
	public double gradientAnimationTime = 4.0f; // seconds
	private readonly double gradeintAnimationTimeScale = 1200.0; // factor to speed up time for the animation
	private readonly int pointCount = 180;
	private readonly float maxAlpha = 1.0f;
	private readonly float minAlpha = 0.05f;


	public void SetOrbitalElements(double eccentricity, double semiMajorAxis, double meanAnomaly, double inclination, double argOfPerifocus, double ascendingNode) 
	{
		m_Orbit = new Orbit(eccentricity, semiMajorAxis, meanAnomaly, inclination, argOfPerifocus, ascendingNode, attractor);
		UpdateLineRenderer();
	}

	public void SetOrbitByAltitudes(double periapsisAltitude, double apoapsisAltitude, double argOfPerifocus) 
	{
		double pe = periapsisAltitude + attractor.radius; // km
		double ap = apoapsisAltitude + attractor.radius; // km
		double a = (pe + ap) / 2.0;
		double e = 1.0 - (pe / a);

		SetOrbitalElements(e, a, 0, 0, argOfPerifocus, 0);
	}

	public void SetOrbitByThrow(Vector3d position, Vector3d velocity) 
	{
		m_Orbit = new Orbit(position, velocity, attractor);
		UpdateLineRenderer();
	}

	void Update() {
		if (animate) {
			double x = Time.timeSinceLevelLoadAsDouble / gradientAnimationTime % 1.0f;
			double meanAnomaly = x * Kepler.PI_2;
			double eccentricAnomaly = Kepler.EccentricAnomalyFromMean(meanAnomaly, m_Orbit.Eccentricity);
			SetGradientByEccentricAnomaly(eccentricAnomaly);
		}
	}

	public void UpdateLineRenderer() {
		// Get points and convert from double to float
		Vector3d[] pointsd = m_Orbit.GetOrbitPoints(pointCount, 1.0e6);
		Vector3[] points = new Vector3[pointCount];
		for (int i = 0; i < pointCount; i++) {
			points[i] = pointsd[i].Vector3;
		}

		var lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = pointCount;
		lineRenderer.SetPositions(points);

		// Set animation time with time scale
		gradientAnimationTime = Orbit.OrbitalPeriod / gradeintAnimationTimeScale;
	}

	/// <summary>
	/// Sets the point in the orbit where the color gradient ends.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly as measured from periapsis that represents the point of maximum alpha.</param>
	public void SetGradientByEccentricAnomaly(double eccentricAnomaly) {
		// Convert radians to range 0.0 to 1.0
		float x1 = (float)(eccentricAnomaly / Kepler.PI_2);
		x1 = x1 % 1.0f;
		if (x1 < 0.0f) x1 += 1.0f;

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
	/// Gets the velocity given the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly as measured from periapsis.</param>
	/// <returns>Velocity vector.</returns>
	public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomlay) {
		double trueAnomaly = Kepler.TrueAnomalyFromEccentric(eccentricAnomlay, m_Orbit.Eccentricity);
		return m_Orbit.GetVelocityAtTrueAnomaly(trueAnomaly);
	}

	/// <summary>
	/// Gets the position relative to the focus given the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly as measured from periapsis.</param>
	/// <returns>Position vector.</returns>
	public Vector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly) {
		// To get the position as a function of time, conver the time to a mean anomaly, then convert that into the eccentric anomaly.
		return m_Orbit.GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
	}

	/// <summary>
	/// Gets the eccentric anomaly given the mean anomaly.
	/// </summary>
	/// <param name="meanAnomaly">The mean anomaly as radians from periapsis.</param>
	/// <returns>Eccentric anomaly as radians from periapsis.</returns>
	public double GetEccentricAnomalyFromMean(double meanAnomaly) {
		return Kepler.EccentricAnomalyFromMean(meanAnomaly, m_Orbit.Eccentricity);
	}

	public override String ToString() {
		double ap = Orbit.ApoapsisAltitude;
		double pe = Orbit.PeriapsisAltitude;
		double period = Orbit.OrbitalPeriod;
		return "Apoapsis: " + ap.ToString("#,##0") + " km\n" +
			"Periapsis: " + pe.ToString("#,##0") + " km\n" +
			"Period: " + OrbitPlot.FormattedTime(period) + "\n";
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
