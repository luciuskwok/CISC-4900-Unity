using System;
using System.Numerics;
using UnityEngine;

public class Orbit {
	public Attractor attractor;

	// Primary variables that define this class
	private Vector3d m_SemiMajorAxisVec; // Vector from center of ellipse to periapse
	private Vector3d m_SemiMinorAxisVec; // Vector from center of ellipse representing the semi-minor axis
	
	private double m_Eccentricity;
	public double Eccentricity { get { return m_Eccentricity; } }

	private double m_EccentricAnomaly; 

	// Secondary variables that are derived from primary varibles
	private Vector3d m_Position; // Current position in orbit, derived from eccentric anomaly.

	// Constants
	public static readonly Vector3d EclipticNormal = new Vector3d(0, 0, 1); // positive z is the direction of north, towards the North Pole Star
	public static readonly Vector3d EclipticUp = new Vector3d(0, 1, 0);
	public static readonly Vector3d EclipticRight = new Vector3d(1, 0, 0);

	// # Constructors

	/// <summary>
	/// Construct orbit given orbital parameters.
	/// </summary>
	/// <param name="eccentricity">Eccentricity.</param>
	/// <param name="semiMajorAxis">Semi-major axis, in km.</param>
	/// <param name="meanAnomaly">Mean anomaly, in radians</param>
	/// <param name="inclination">Inclination from ecliptic, in radians.</param>
	/// <param name="argOfPerifocus">Argument of perifocus, in radians.</param>
	/// <param name="ascendingNode">Ascending node, in radians.</param>
	/// <param name="attractor">Parent gravitational body at focus of orbit.</param>
	public Orbit(double eccentricity, double semiMajorAxis, double meanAnomaly, double inclination, double argOfPerifocus, double ascendingNode, Attractor attractor) 
	{		
		// Attractor
		this.attractor = attractor;

		// Semi-minor axis
		double semiMinorAxis;
		if (eccentricity < 1.0) { // elliptical orbit
			semiMinorAxis = semiMajorAxis * Math.Sqrt(1.0 - eccentricity * eccentricity);
		} else if (eccentricity > 1.0) { // hyperbolic orbit
			semiMinorAxis = semiMajorAxis * Math.Sqrt(eccentricity * eccentricity - 1.0);
		} else { // parabolic orbit
			semiMinorAxis = 0.0;
		}
		
		// Ascending node, inclination, and argument of perifocus
		ascendingNode %= Kepler.PI_2;
		if (ascendingNode > Kepler.PI) ascendingNode -= Kepler.PI_2;
		inclination %= Kepler.PI_2;
		if (inclination > Kepler.PI) inclination -= Kepler.PI_2;
		argOfPerifocus %= Kepler.PI_2;
		if (argOfPerifocus > Kepler.PI) argOfPerifocus -= Kepler.PI_2;

		Vector3d ascendingNodeVec = Kepler.RotateVectorByAngle(EclipticRight, ascendingNode, EclipticNormal).normalized;
		Vector3d orbitNormalVec   = Kepler.RotateVectorByAngle(EclipticNormal, inclination, ascendingNodeVec).normalized;
		Vector3d periapseVec      = Kepler.RotateVectorByAngle(ascendingNodeVec, argOfPerifocus, orbitNormalVec).normalized;
		
		// Set primary variables
		m_SemiMajorAxisVec = periapseVec * semiMajorAxis;
		m_SemiMinorAxisVec = Vector3d.Cross(periapseVec, orbitNormalVec) * semiMinorAxis;
		m_Eccentricity = eccentricity;
		m_EccentricAnomaly = Kepler.EccentricAnomalyFromMean(meanAnomaly, eccentricity);

		// Set secondary variables
		m_Position = GetFocalPositionAtEccentricAnomaly(m_EccentricAnomaly);

	}

	/// <summary>
	/// Construct orbit given position and velocity relative to the attractor at the focus.
	/// </summary>
	/// <param name="position">Position relative to focus.</param>
	/// <param name="velocity">Velocity vector relative to focus.</param>
	/// <param name="attractor">Parent gravitational body at focus of orbit.</param>
	public Orbit(Vector3d position, Vector3d velocity, Attractor attractor) 
	{
		this.attractor = attractor;
		SetOrbitByThrowing(position, velocity);
	}

	/// <summary>
	/// Changes the orbital parameters by "throwing" an object from position with velocity relative to focus.
	/// </summary>
	/// <param name="position">Position relative to focus.</param>
	/// <param name="velocity">Velocity vector relative to focus.</param>
	public void SetOrbitByThrowing(Vector3d position, Vector3d velocity) 
	{
		m_Position = position;
		
		double MG = attractor.mass * Kepler.G;
		double attractorDistance = position.magnitude;
		Vector3d angularMomentumVector = position.Cross(velocity);
		Vector3d orbitNormal = angularMomentumVector.normalized;
		Vector3d eccVector;
		if (orbitNormal.sqrMagnitude < 0.99) {
			orbitNormal = position.Cross(Orbit.EclipticUp).normalized;
			eccVector = new Vector3d();
			//Debug.Log("Invalid orbit normal.");
		} else {
			eccVector = velocity.Cross(angularMomentumVector) / MG - position / attractorDistance;
		}

		double focalParameter = angularMomentumVector.sqrMagnitude / MG;
		m_Eccentricity = eccVector.magnitude;

		Vector3d minor = angularMomentumVector.Cross(-eccVector).normalized;
		if (minor.sqrMagnitude < 0.99) {
			minor = orbitNormal.Cross(position).normalized;
			//Debug.Log("Invalid semiMinorAxisBasis.");
		}

		Vector3d major = orbitNormal.Cross(minor).normalized;

		double majorDistance = 0.0;
		double minorDistance = 0.0;
		double trueAnomaly = 0.0;

		if (m_Eccentricity < 1.0) { // Elliptical orbit
			double compression = 1.0 - m_Eccentricity * m_Eccentricity;
			majorDistance = focalParameter / compression;
			minorDistance = majorDistance * Math.Sqrt(compression);
			trueAnomaly = Vector3d.Angle(position, major);
			if (position.Cross(-major).Dot(orbitNormal) < 0.0) {
				trueAnomaly = Kepler.PI_2 - trueAnomaly;
			}
		} else if (m_Eccentricity > 1.0) { // Hyperbolic orbit
			double compression = m_Eccentricity * m_Eccentricity - 1.0;
			majorDistance = focalParameter / compression;
			minorDistance = majorDistance * Math.Sqrt(compression);
			trueAnomaly = Vector3d.Angle(position, eccVector);
			if (position.Cross(-major).Dot(orbitNormal) < 0.0) {
				trueAnomaly = -trueAnomaly;
			}
		} else { // Parabolic orbit
			// TODO: figure out how to encode this
			// The semi-major and semi-minor axes are used for plotting the path, and for getting the position. Maybe save periapsis distance?
			Debug.Log("Parabolic orbits not tested and may have unexpected results.");
		}

		// Calculate the primary variables
		m_SemiMajorAxisVec = major * majorDistance;
		m_SemiMinorAxisVec = minor * minorDistance;
		m_EccentricAnomaly = Kepler.EccentricAnomalyFromTrue(trueAnomaly, m_Eccentricity);
	}

	/// <summary>
	/// Gets orbit sample points by iterating over the eccentric anomaly.
	/// </summary>
	/// <param name="pointsCount">The points count.</param>
	/// <param name="origin">The origin.</param>
	/// <param name="maxDistance">The maximum distance.</param>
	/// <returns>Array of orbit curve points.</returns>
	public Vector3d[] GetOrbitPoints(int pointsCount = 180, double maxDistance = 1.0e6) 
	{
		double pe = this.PeriapsisDistance;
		if (pointsCount < 2 || maxDistance < pe) return new Vector3d[0];
		
		Vector3d[] result = new Vector3d[pointsCount];
		if (m_Eccentricity < 1.0) {
			if (this.ApoapsisDistance < maxDistance) {
				for (int i = 0; i < pointsCount; i++) {
					result[i] = GetFocalPositionAtEccentricAnomaly(Kepler.PI_2 * i / pointsCount);
				}
			} else {
				double maxAngle = Kepler.TrueAnomalyForDistance(maxDistance, m_Eccentricity, pe, pe);
				for (int i = 0; i < pointsCount; i++){
					double eccentricAnomaly = Kepler.EccentricAnomalyFromTrue(-maxAngle + i * 2d * maxAngle / (pointsCount - 1), m_Eccentricity);
					result[i] = GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
				}
			}
		} else {
			double maxAngle = Kepler.TrueAnomalyForDistance(maxDistance, m_Eccentricity, this.SemiMajorAxis, pe);

			for (int i = 0; i < pointsCount; i++) {
				double eccentricAnomaly = Kepler.EccentricAnomalyFromTrue(-maxAngle + i * 2d * maxAngle / (pointsCount - 1), m_Eccentricity);
				result[i] = GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
			}
		}

		return result;
	}

	/// <summary>
	/// Gets the velocity given the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly.</param>
	/// <returns>Velocity vector.</returns>
	public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly)
	{
		return GetVelocityAtTrueAnomaly(Kepler.TrueAnomalyFromEccentric(eccentricAnomaly, Eccentricity));
	}

	/// <summary>
	/// Gets the velocity given the true anomaly.
	/// </summary>
	/// <param name="trueAnomaly">The true anomaly.</param>
	/// <returns>Velocity vector.</returns>
	public Vector3d GetVelocityAtTrueAnomaly(double trueAnomaly) {
		double e = m_Eccentricity;
		double compression = e < 1.0 ? (1.0 - e * e) : (e * e - 1.0);
		double focalParameter = this.SemiMajorAxis * compression;

		if (focalParameter <= 0.0) return Vector3d.zero;
		
		double sqrtMGdivP = Math.Sqrt(attractor.mass * Kepler.G / focalParameter);
		double vX = -sqrtMGdivP * Math.Sin(trueAnomaly);
		double vY = sqrtMGdivP * (m_Eccentricity + Math.Cos(trueAnomaly));
		Vector3d major = m_SemiMajorAxisVec.normalized;
		Vector3d minor = m_SemiMinorAxisVec.normalized;
		return -major * vX - minor * vY;
	}

	/// <summary>
	/// Gets the position relative to the focal point given the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly.</param>
	/// <returns>Position relative to orbit focus.</returns>
	public Vector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly) {
		return GetCentralPositionAtEccentricAnomaly(eccentricAnomaly) + this.CenterPoint;
	}
	
	/// <summary>
	/// Gets the position relative to the center point given the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly.</param>
	/// <returns>Position relative to orbit center.</returns>
	/// <remarks>
	/// Note: central position is not same as focal position.
	/// </remarks>
	public Vector3d GetCentralPositionAtEccentricAnomaly(double eccentricAnomaly) {
		Vector3d major = m_SemiMajorAxisVec.normalized;
		Vector3d minor = m_SemiMinorAxisVec.normalized;
		if (m_Eccentricity < 1.0) {
			double x = -Math.Cos(eccentricAnomaly) * this.SemiMajorAxis;
			double y = Math.Sin(eccentricAnomaly) * this.SemiMinorAxis;
			return -major.normalized * x - minor * y;
		} else if (m_Eccentricity > 1.0) {
			double x = Math.Cosh(eccentricAnomaly) * this.SemiMajorAxis;
			double y = Math.Sinh(eccentricAnomaly) * this.SemiMinorAxis;
			return -major * x - minor * y;
		} else {
			double pe = this.PeriapsisDistance;
			double x = pe * Math.Cos(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly));
			double y = pe * Math.Sin(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly));
			return -major * x - minor * y;
		}
	}

	public double SemiMajorAxis {
		get { return m_SemiMajorAxisVec.magnitude; }
	}

	public double SemiMinorAxis {
		get { return m_SemiMinorAxisVec.magnitude; }
	}

	public Vector3d CenterPoint {
		get { return -m_SemiMajorAxisVec * m_Eccentricity; }
	}

	public double PeriapsisDistance {
		get { return SemiMajorAxis * (1.0 - m_Eccentricity); }
	}

	public double ApoapsisDistance {
		get { return SemiMajorAxis * (1.0 + m_Eccentricity); }
	}

	public double PeriapsisAltitude {
		get { return PeriapsisDistance - attractor.radius; }
	}

	public double ApoapsisAltitude {
		get { return ApoapsisDistance - attractor.radius; }
	}

	public double OrbitalPeriod { 
		get {
			if (m_Eccentricity < 1.0) {
				double gm = Kepler.G * attractor.mass;
				double a = this.SemiMajorAxis; 
				return Kepler.PI_2 * Math.Sqrt(a * a * a / gm);
			} else {
				return double.PositiveInfinity;
			}
		}
	}


}