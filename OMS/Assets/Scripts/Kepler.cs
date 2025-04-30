// Kepler.cs
// Utilities for calculating orbits.
// Adapted from https://github.com/Karth42/SimpleKeplerOrbits

using System;

/// <summary>
/// Utility functions for working with Kepler orbits.
/// </summary>
public static class Kepler {
	public const double PI_2 = 6.2831853071796d; // 2 * PI
	public const double PI = 3.14159265358979;
	public const double Deg2Rad = 0.017453292519943d;
	public const double Rad2Deg = 57.295779513082d;
	public const double G = 6.67430e-20; // (km^3)/(kg*s^2) Gravitational Constant


	/// <summary>
	/// Regular Acosh, but without exception when out of possible range.
	/// </summary>
	/// <param name="x">The input value.</param>
	/// <returns>Calculated Acos value or 0.</returns>
	public static double Acosh(double x) {
		if (x < 1.0) return 0;
		return Math.Log(x + Math.Sqrt(x * x - 1.0));
	}

	/// <summary>
	/// Converts the eccentric to true anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly in radians from periapsis. This value must be in the range of 0 to PI_2.</param>
	/// <param name="eccentricity">The eccentricity.</param>
	/// <returns>True anomaly in radians.</returns>
	public static double GetTrueAnomalyFromEccentric(double eccentricAnomaly, double eccentricity) {
		if (eccentricity < 1.0) {
			double cosE  = Math.Cos(eccentricAnomaly);
			double tAnom = Math.Acos((cosE - eccentricity) / (1d - eccentricity * cosE));
			if (eccentricAnomaly > PI) tAnom = PI_2 - tAnom;
			return tAnom;
		} else if (eccentricity > 1.0) {
			double tAnom = Math.Atan2(
				Math.Sqrt(eccentricity * eccentricity - 1d) * Math.Sinh(eccentricAnomaly),
				eccentricity - Math.Cosh(eccentricAnomaly)
			);
			return tAnom;
		} else {
			return eccentricAnomaly;
		}
	}

	/// <summary>
	/// Converts the true to eccentric anomaly.
	/// </summary>
	/// <param name="trueAnomaly">The true anomaly in radians from periapsis. This value must be in the range of 0 to PI_2.</param>
	/// <param name="eccentricity">The eccentricity.</param>
	/// <returns>Eccentric anomaly in radians.</returns>
	public static double GetEccentricAnomalyFromTrue(double trueAnomaly, double eccentricity) {
		if (double.IsNaN(eccentricity) || double.IsInfinity(eccentricity)) return trueAnomaly;

		trueAnomaly %= PI_2;
		if (eccentricity < 1.0) {
			if (trueAnomaly < 0) trueAnomaly += PI_2;

			double cosT2   = Math.Cos(trueAnomaly);
			double eccAnom = Math.Acos((eccentricity + cosT2) / (1d + eccentricity * cosT2));
			if (trueAnomaly > PI) eccAnom = PI_2 - eccAnom;

			return eccAnom;
		} else if (eccentricity > 1.0) {
			double cosT    = Math.Cos(trueAnomaly);
			double eccAnom = Acosh((eccentricity + cosT) / (1d + eccentricity * cosT)) * Math.Sign(trueAnomaly);
			return eccAnom;
		} else {
			// For parabolic trajectories
			// there is no Eccentric anomaly defined,
			// because 'True anomaly' to 'Time' relation can be resolved analytically.
			return trueAnomaly;
		}
	}

		/// <summary>
		/// Converts the eccentric to mean anomaly.
		/// </summary>
		/// <param name="eccentricAnomaly">The eccentric anomaly in radians from periapsis.</param>
		/// <param name="eccentricity">The eccentricity.</param>
		/// <returns>Mean anomaly in radians.</returns>
	public static double GetMeanAnomalyFromEccentric(double eccentricAnomaly, double eccentricity) {
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

	/// <summary>
	/// Converts the mean to eccentric anomaly, by means of Newton's method.
	/// </summary>
	/// <param name="meanAnomaly">The mean anomaly in radians from the periapsis.</param>
	/// <param name="eccentricity">The eccentricity.</param>
	/// <returns>Eccentric anomaly in radians.</returns>
	public static double GetEccentricAnomalyFromMean(double meanAnomaly, double eccentricity) {
		if (eccentricity < 1.0) {
			return Mean2EccentricAnomalyElliptical(meanAnomaly, eccentricity);
		} else if (eccentricity > 1.0) {
			return Mean2EccentricAnomalyHyperbolic(meanAnomaly, eccentricity);
		} else {
			var m   = meanAnomaly * 2;
			var v   = 12d * m + 4d * Math.Sqrt(4d + 9d * m * m);
			var pow = Math.Pow(v, 1d / 3d);
			var t   = 0.5 * pow - 2 / pow;
			return 2 * Math.Atan(t);
		}
	}

	private static double Mean2EccentricAnomalyElliptical(double meanAnomaly, double eccentricity) {
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

	private static double Mean2EccentricAnomalyHyperbolic(double meanAnomaly, double eccentricity) {
		double delta = 1d;

		// Danby guess.
		double F = Math.Log(2d * Math.Abs(meanAnomaly) / eccentricity + 1.8d);
		if (double.IsNaN(F) || double.IsInfinity(F)) return meanAnomaly;
		while (delta > 1e-8 || delta < -1e-8) {
			delta =  (eccentricity * Math.Sinh(F) - F - meanAnomaly) / (eccentricity * Math.Cosh(F) - 1d);
			F     -= delta;
		}
		return F;
	}

	/// <summary>
	/// Rotate vector around another vector (double).
	/// </summary>
	/// <param name="v">Vector to rotate.</param>
	/// <param name="angleRad">angle in radians.</param>
	/// <param name="n">normalized vector to rotate around, or normal of rotation plane.</param>
	public static Vector3d RotateVectorByAngle(Vector3d v, double angleRad, Vector3d n)
	{
		double cosT        = Math.Cos(angleRad);
		double sinT        = Math.Sin(angleRad);
		double oneMinusCos = 1f - cosT;
		// Rotation matrix:
		double a11 = oneMinusCos * n.x * n.x + cosT;
		double a12 = oneMinusCos * n.x * n.y - n.z * sinT;
		double a13 = oneMinusCos * n.x * n.z + n.y * sinT;
		double a21 = oneMinusCos * n.x * n.y + n.z * sinT;
		double a22 = oneMinusCos * n.y * n.y + cosT;
		double a23 = oneMinusCos * n.y * n.z - n.x * sinT;
		double a31 = oneMinusCos * n.x * n.z - n.y * sinT;
		double a32 = oneMinusCos * n.y * n.z + n.x * sinT;
		double a33 = oneMinusCos * n.z * n.z + cosT;
		return new Vector3d(
			v.x * a11 + v.y * a12 + v.z * a13,
			v.x * a21 + v.y * a22 + v.z * a23,
			v.x * a31 + v.y * a32 + v.z * a33
		);
	}

	/// <summary>
	/// Gets the True anomaly value from current distance from the focus (attractor).
	/// </summary>
	/// <param name="distance">The distance from attractor.</param>
	/// <param name="eccentricity">The eccentricity.</param>
	/// <param name="semiMajorAxis">The semi major axis.</param>
	/// <param name="periapsisDistance">The periapsis distance value.</param>
	/// <returns>True anomaly in radians.</returns>
	public static double TrueAnomalyForDistance(double distance, double eccentricity, double semiMajorAxis, double periapsisDistance)
	{
		if (eccentricity < 1.0) {
			return Math.Acos((semiMajorAxis * (1d - eccentricity * eccentricity) - distance) / (distance * eccentricity));
		} else if (eccentricity > 1.0) {
			return Math.Acos((semiMajorAxis * (eccentricity * eccentricity - 1d) - distance) / (distance * eccentricity));
		} else {
			return Math.Acos((periapsisDistance / distance) - 1d);
		}
	}

	/// <summary>
	/// Maps an anomaly value to the range of 0 to PI_2.
	/// </summary>
	public static double NormalizedAnomaly(double a) {
		a = a % PI_2;
		if (a < 0) a += PI_2;
		return a;
	}

}