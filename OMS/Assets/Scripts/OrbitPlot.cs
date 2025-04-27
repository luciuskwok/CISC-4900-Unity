using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using System;

public class OrbitPlot : MonoBehaviour
{
	public double semiMajorAxis; // in km
	public double eccentricity; // 0 = circle; 1 = parabola
	public double longitudeOfPeriapsis; // in degrees

	public Color color;

	public int plottingMethod = 0;

	// Note: the coordinate system that is conventionally used for orbital mechanics has the ecliptic on the x-y plane, and positive z is towards the North Pole Star. The positive x axis is the direction of the Sun as seen from the Earth at the (spring) vernal equinox. This means that the Earth is at longitude 0 at the autumnal equinox, and at 180 at the spring equinox.
	// Unity uses the convention that the x-z plane is horizontal, and positive y points up. So the y and z axes are swapped. 
	// As for the x axis, that can be arbitrarily chosen as long as it is consistent throughout the solar system for this project. 

	private int pointCount = 180;
	private double gradientAnimationTime = 4.0f; // seconds
	private float maxAlpha = 1.0f;
	private float minAlpha = 0.05f;

	private static double gravitationalConstant = 6.67430e-20; // (km^3)/(kg*s^2)

	void Start()
	{
		UpdatePoints();
	}

	void Update()
	{
		double x = (Time.timeSinceLevelLoadAsDouble / gradientAnimationTime) % 1.0f;
		int step = (int)Math.Round(x * pointCount);
		UpdateColors(step);
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
		double longOfPeriapsisRadians = longitudeOfPeriapsis / 180.0 * Math.PI;
		double semiLactusRectum = semiMajorAxis * (1.0 - eccentricity * eccentricity);
		for (int i = 0; i < count; i++) {
			// Theta is the true anomaly of the point.
			// Calculate the orbit from -180 to 100 degrees.
			double theta = ((double)i / count * 360.0 - 180.0)  / 180.0 * Math.PI;
			// This version of the equation has the reference direction theta = 0 pointing away from the center of the ellipse, so that the zero angle is at the periapsis of the orbit.
			double r = semiLactusRectum / ( 1.0 + eccentricity * Math.Cos(theta));
			// Rotate by the longitudde of periapsis, which is located at theta = 0, relative to the ecliptic coordinate system, where longitude = 0 is at the positive x axis.
			// Plot points with focus at center
			double x = Math.Cos(theta + longOfPeriapsisRadians) * r;
			double z = Math.Sin(theta + longOfPeriapsisRadians) * r;

			// Convert from km to Unity Units
			double scale = OrbitUIHandler.KmToUnityUnit;
			points[i] = new Vector3((float)(x * scale), 0, (float)(z * scale));
		}

		return points;
	}

	Vector3[] EllipseWithCartesianMethod(int count) {
		var points = new Vector3[count];

		double a = semiMajorAxis;
		double b = SemiMinorAxis();
		double f = a * eccentricity; // Distance from center to focus, f = a * e
		double rot = longitudeOfPeriapsis / 180.0 * Math.PI; // convert to radians
		Vector2d point = new Vector2d();
		Vector2d scale = new Vector2d(OrbitUIHandler.KmToUnityUnit, OrbitUIHandler.KmToUnityUnit);
		for (int i = 0; i < count; i++) {
			double theta = (360.0 * i / count - 180.0)  / 180.0 * Math.PI;
			// Plot a canonical ellipse with focus and center on the x-axis, centered on the right focus
			point.x = a * Math.Cos(theta) - f;
			point.y = b * Math.Sin(theta);
			// Rotate and scale the ellipse point
			point.Rotate(rot);
			point.Scale(scale);
			// Add point
			points[i] = new Vector3((float)point.x, 0, (float)point.y);
		}
		return points;
	}

	void UpdateColors(int step) {
		// Given step as the index of a point, create a gradient.

		// Calculate the locations and alpha values for a fade
		float countf = (float)(pointCount + 1);
		float x1 = (float)step / countf;
		float x2 = (float)(step + 1) / countf;
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

	public Vector3 PositionAtEccentricAnomaly(double eccentricAnomaly) {
		// Gets the xyz coordinates on the orbit line given the eccentric anomaly as the angle from the periapsis.
		// To get the position as a function of time, conver the time to a mean anomaly, then convert that into the eccentric anomaly.
		double a = semiMajorAxis;
		double b = SemiMinorAxis();
		double f = a * eccentricity; // Distance from center to focus, f = a * e
		double rot = longitudeOfPeriapsis / 180.0 * Math.PI; // convert to radians
		Vector2d scale = new Vector2d(OrbitUIHandler.KmToUnityUnit, OrbitUIHandler.KmToUnityUnit);

		Vector2d point = new Vector2d();
		point.x = a * Math.Cos(eccentricAnomaly) - f;
		point.y = b * Math.Sin(eccentricAnomaly);
		// Rotate and scale the ellipse point
		point.Rotate(rot);
		point.Scale(scale);

		return new Vector3((float)point.x, 0, (float)point.y);
	}

	// Utility functions

	public static double MeanAnomalyFromEccentricAnomaly(double eccentricAnomaly, double eccentricity) {
		// Adapted from code in https://github.com/Karth42/SimpleKeplerOrbits
		// This handles all the cases of eccentricity
		if (eccentricity < 1.0) {
			return eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
		} else if (eccentricity > 1.0) {
			return Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
		} else {
			var t = Math.Tan(eccentricAnomaly * 0.5);
			return (t + t * t * t / 3d) * 0.5d;
		}	
	}

	public static double EccentricAnomalyFromMeanAnomaly(double meanAnomaly, double eccentricity) {
		// Adapted from code in https://github.com/Karth42/SimpleKeplerOrbits
		// Converts mean anomaly to eccentric anomaly using Kepler Solver.
		// This is only valid for elliptical orbits. For hyperbolic orbits, see the original code.
		// Iterations count range from 2 to 6 when eccentricity is in range from 0 to 1.
		int    iterations = (int)(Math.Ceiling((eccentricity + 0.7d) * 1.25d)) << 1;
		double m          = meanAnomaly;
		double esinE, ecosE, deltaE, n;
		for (int i = 0; i < iterations; i++) {
			esinE  =  eccentricity * Math.Sin(m);
			ecosE  =  eccentricity * Math.Cos(m);
			deltaE =  m - esinE - meanAnomaly;
			n      =  1.0 - ecosE;
			m      += -5d * deltaE / (n + Math.Sign(n) * Math.Sqrt(Math.Abs(16d * n * n - 20d * deltaE * esinE)));
		}
		return m;
	}

	public static double OrbitalPeriod(double semiMajorAxis, double parentBodyMass) {
		double gm = gravitationalConstant * parentBodyMass;
		double a = semiMajorAxis; 
		return 2.0 * Math.PI * Math.Sqrt(a * a * a / gm);
	}


}
