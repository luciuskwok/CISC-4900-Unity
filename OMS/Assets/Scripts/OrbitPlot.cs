using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using System;

public class OrbitPlot : MonoBehaviour
{
	public float semiMajorAxis;
	public float eccentricity; // 0 = circle; 1 = parabola
	public float longitudeOfPeriapsis; // in degrees
	public Color color;

	// Note: the coordinate system that is conventionally used for orbital mechanics has the ecliptic on the x-y plane, and positive z is towards the North Pole Star. The positive x axis is the direction of the Sun as seen from the Earth at the (spring) vernal equinox. This means that the Earth is at longitude 0 at the autumnal equinox, and at 180 at the spring equinox.
	// Unity uses the convention that the x-z plane is horizontal, and positive y points up. So the y and z axes are swapped. 
	// As for the x axis, that can be arbitrarily chosen as long as it is consistent throughout the solar system for this project. 

	private LineRenderer lineRenderer;
	private int pointCount = 90;
	private double gradientAnimationTime = 4.0f; // seconds

	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		UpdatePoints();
	}

	void Update()
	{
		double x = (Time.timeSinceLevelLoadAsDouble / gradientAnimationTime) % 1.0f;
		int step = (int)Math.Round(x * pointCount);
		UpdateColors(step);
	}

	public void UpdatePoints() {
		lineRenderer.positionCount = pointCount;
		var points = GetOrbitPoints(pointCount);
		lineRenderer.SetPositions(points);
	}

	Vector3[] GetOrbitPoints(int count) {
		var points = new Vector3[count];

		// First, calculate the olar form of ellipse relative to focus, then rotate it so its periapsis is at the specified longitude, and finally convert to cartesian coordinates.
		// Pre-convert the longitude of periapsis from degrees to radians
		float longOfPeriapsisRadians = Mathf.Deg2Rad * longitudeOfPeriapsis;
		for (int i = 0; i < count; i++) {
			// Theta is the true anomaly of the point.
			// Calculate the orbit from -180 to 100 degrees.
			float theta = ((float)i / (float)count * 360.0f - 180.0f) * Mathf.Deg2Rad;
			// This version of the equation has the reference direction theta = 0 pointing away from the center of the ellipse, so that the zero angle is at the periapsis of the orbit.
			float r = radiusWithTrueAnomaly(theta);
			// Rotate by the longitudde of periapsis, which is locade at theta = 0, relative to the ecliptic coordinate system, where longitude = 0 is at the positive x axis.
			float a = theta + longOfPeriapsisRadians;
			// Convert from polar to cartesian coordinates.
			float x = Mathf.Cos(a) * r;
			float z = Mathf.Sin(a) * r;
			points[i] = new Vector3(x, 0, z);
		}

		return points;
	}

	float radiusWithTrueAnomaly(float theta) {
		// Given a value for the true anomaly (theta), calculate the radius of the polar coordinate point on the orbit.
		float semiLactusRectum = semiMajorAxis * (1.0f + eccentricity * eccentricity);

		// This version of the equation has the reference direction theta = 0 pointing away from the center of the ellipse, so that the zero angle is at the periapsis of the orbit.
		return semiLactusRectum / ( 1.0f + eccentricity * Mathf.Cos(theta));
	}

	float trueAnomayWithEccentricAnomaly(float e) {
		
		return 0;
	}

	void UpdateColors(int step) {
		// Given step as the index of a point, create a gradient.
		Gradient gradient = new Gradient();

		// Calculate the locations and alpha values for a fade
		float countf = (float)(pointCount + 1);
		float x1 = (float)step / countf;
		float x2 = (float)(step + 1) / countf;
		float a1 = 1.0f;
		float a2 = 0.25f;
		float a0 = a2 + (a1 - a2) * (1.0f - x1);
		float a3 = a2 + (a1 - a2) * (1.0f - x2);

		if (x2 > 1.0f) {
			x2 = 0.0f;
			a3 = a2;
		}


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

		lineRenderer.colorGradient = gradient;
	}
}
