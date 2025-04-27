using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using System;

public class OrbitPlot : MonoBehaviour
{
	public double semiMajorAxis; // km
	public double eccentricity; // 0 = circle; 1 = parabola
	public double periapsisLongitude; // radians

	public Color color;

	public int plottingMethod = 0;

	// Note: the coordinate system that is conventionally used for orbital mechanics has the ecliptic on the x-y plane, and positive z is towards the North Pole Star. The positive x axis is the direction of the Sun as seen from the Earth at the (spring) vernal equinox. This means that the Earth is at longitude 0 at the autumnal equinox, and at 180 at the spring equinox.
	// Unity uses the convention that the x-z plane is horizontal, and positive y points up. So the y and z axes are swapped. 
	// As for the x axis, that can be arbitrarily chosen as long as it is consistent throughout the solar system for this project. 

	private int pointCount = 180;
	private double gradientAnimationTime = 4.0f; // seconds
	private float maxAlpha = 1.0f;
	private float minAlpha = 0.05f;


	void Start()
	{
		UpdatePoints();
	}

	void Update()
	{
		double x = Time.timeSinceLevelLoadAsDouble / gradientAnimationTime % 1.0f;
		double meanAnomaly = x * Kepler.PI_2;
		double eccentricAnomaly = Kepler.EccentricAnomalyFromMeanAnomaly(meanAnomaly, eccentricity);
		UpdateColors(eccentricAnomaly);
	}

	public void UpdatePoints() {
		var lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = pointCount;
		Vector3[] points;
		if (plottingMethod == 0) {
			points = EllipseWithPolarMethod(pointCount);
		} else {
			points = EllipseWithCartesianMethod(pointCount);
		}
		lineRenderer.SetPositions(points);
	}

	Vector3[] EllipseWithPolarMethod(int count) {
		var points = new Vector3[count];

		// First, calculate the polar form of ellipse relative to focus, then rotate it so its periapsis is at the specified longitude, and finally convert to cartesian coordinates.
		// Pre-convert the longitude of periapsis from degrees to radians
		double semiLactusRectum = semiMajorAxis * (1.0 - eccentricity * eccentricity);
		for (int i = 0; i < count; i++) {
			// Theta is the true anomaly of the point.
			// Calculate the orbit from -180 to 100 degrees.
			double theta = (double)i / count * Kepler.PI_2 - Kepler.PI;
			// This version of the equation has the reference direction theta = 0 pointing away from the center of the ellipse, so that the zero angle is at the periapsis of the orbit.
			double r = semiLactusRectum / ( 1.0 + eccentricity * Math.Cos(theta));
			// Rotate by the longitudde of periapsis, which is located at theta = 0, relative to the ecliptic coordinate system, where longitude = 0 is at the positive x axis.
			// Plot points with focus at center
			double x = Math.Cos(theta + periapsisLongitude) * r;
			double z = Math.Sin(theta + periapsisLongitude) * r;

			points[i] = new Vector3((float)x, 0, (float)z);
		}

		return points;
	}

	Vector3[] EllipseWithCartesianMethod(int count) {
		var points = new Vector3[count];

		double a = semiMajorAxis;
		double b = SemiMinorAxis();
		double f = a * eccentricity; // Distance from center to focus, f = a * e
		Vector2d point = new Vector2d();
		for (int i = 0; i < count; i++) {
			double theta = (360.0 * i / count - 180.0) * Kepler.Deg2Rad;
			// Plot a canonical ellipse with focus and center on the x-axis, centered on the right focus
			point.x = a * Math.Cos(theta) - f;
			point.y = b * Math.Sin(theta);
			// Rotate and scale the ellipse point
			point.Rotate(periapsisLongitude);
			// Add point
			points[i] = new Vector3((float)point.x, 0, (float)point.y);
		}
		return points;
	}

	void UpdateColors(double eccentricAnomaly) {
		// Given the eccentric anomaly as the point of maximum alpha, update the color gradient on the line renderer.

		// Convert radians to range 0.0 to 1.0
		float x1 = (float)((eccentricAnomaly + periapsisLongitude) / Kepler.PI_2);
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

	public double SemiMinorAxis() {
		return semiMajorAxis * Math.Sqrt(1.0 - eccentricity * eccentricity);
	}

	public Vector3 LocalPositionAtEccentricAnomaly(double eccentricAnomaly) {
		// Gets the xyz coordinates on the orbit line given the eccentric anomaly as the angle from the periapsis.
		// To get the position as a function of time, conver the time to a mean anomaly, then convert that into the eccentric anomaly.
		double a = semiMajorAxis;
		double b = SemiMinorAxis();
		double f = a * eccentricity; // Distance from center to focus, f = a * e

		Vector2d point = new Vector2d();
		point.x = a * Math.Cos(eccentricAnomaly) - f;
		point.y = b * Math.Sin(eccentricAnomaly);
		// Rotate for the longitude of periapsis
		point.Rotate(periapsisLongitude);

		return new Vector3((float)point.x, 0.0f, (float)point.y);
	}

}
