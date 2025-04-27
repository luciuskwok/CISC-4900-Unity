// Kepler
// Utilities for calculating orbits.
// Adapted from https://github.com/Karth42/SimpleKeplerOrbits

using System;

public static class Kepler {
	public const double PI_2 = 6.2831853071796d; // 2 * PI
	public const double PI = 3.14159265358979;
	public const double Deg2Rad = 0.017453292519943d;
	public const double Rad2Deg = 57.295779513082d;
	public const double G = 6.67430e-20; // (km^3)/(kg*s^2) Gravitational Constant

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

	public static double OrbitalPeriod(double semiMajorAxis, double mass) {
		// Parameters: semi-major axis of the orbit; mass of the two bodies.
		// Returns the orbital period in seconds.
		double gm = G * mass;
		double a = semiMajorAxis; 
		return 2.0 * Math.PI * Math.Sqrt(a * a * a / gm);
	}
}