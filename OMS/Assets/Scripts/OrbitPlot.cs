using UnityEngine;
using System.Collections;

public class OrbitPlot : MonoBehaviour
{
	public float semiMajorAxis;
	public float eccentricity; // 0 = circle; 1 = parabola
	public float longitudeOfPeriapsis; // in degrees

	// Note: the coordinate system that is conventionally used for orbital mechanics has the ecliptic on the x-y plane, and positive z is towards the North Pole Star. The positive x axis is the direction of the Sun as seen from the Earth at the (spring) vernal equinox. This means that the Earth is at longitude 0 at the autumnal equinox, and at 180 at the spring equinox.
	// Unity uses the convention that the x-z plane is horizontal, and positive y points up. So the y and z axes are swapped. 
	// As for the x axis, that can be arbitrarily chosen as long as it is consistent throughout the solar system for this project. 

	private LineRenderer lineRenderer;
	private int pointCount = 90;

	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		UpdatePoints();
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
			// Rotate by the longitudde of periapsis plus 180 degrees so that the zero longitude is at the positive x axis.
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

	void Update()
	{
		
	}
}
