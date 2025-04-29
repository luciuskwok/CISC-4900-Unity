using UnityEngine;
using System.Collections;
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

	private int pointCount = 180;
	private double gradientAnimationTime = 4.0f; // seconds
	private float maxAlpha = 1.0f;
	private float minAlpha = 0.05f;


	public void SetOrbitalElements(double eccentricity, double semiMajorAxis, double meanAnomaly, double inclination, double argOfPerifocus, double ascendingNode, Attractor attractor) 
	{
		m_Orbit = new Orbit(eccentricity, semiMajorAxis, meanAnomaly, inclination, argOfPerifocus, ascendingNode, attractor);
		UpdatePoints();
	}

	public void SetOrbitByThrow(Vector3d position, Vector3d velocity, Attractor attractor) 
	{
		m_Orbit = new Orbit(position, velocity, attractor);
		UpdatePoints();
	}

	void Update() {
		double x = Time.timeSinceLevelLoadAsDouble / gradientAnimationTime % 1.0f;
		double meanAnomaly = x * Kepler.PI_2;
		double eccentricAnomaly = Kepler.EccentricAnomalyFromMean(meanAnomaly, m_Orbit.Eccentricity);
		UpdateColors(eccentricAnomaly);
	}

	public void UpdatePoints() {
		// Get points and convert from double to float
		Vector3d[] pointsd = m_Orbit.GetOrbitPoints(pointCount, 1.0e6);
		Vector3[] points = new Vector3[pointCount];
		for (int i = 0; i < pointCount; i++) {
			points[i] = pointsd[i].Vector3;
		}

		var lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = pointCount;
		lineRenderer.SetPositions(points);
	}

	void UpdateColors(double eccentricAnomaly) {
		// Given the eccentric anomaly as the point of maximum alpha, update the color gradient on the line renderer.

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
				new GradientColorKey(color, 0.0f), 
				new GradientColorKey(color, 1.0f) },
			// Vary the alpha 
			new GradientAlphaKey[] { 
				new GradientAlphaKey(a0, 0.0f),
				new GradientAlphaKey(a1, x1),
				new GradientAlphaKey(a2, x2),
				new GradientAlphaKey(a3, 1.0f),
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

}
