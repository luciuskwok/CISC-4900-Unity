// Orbit.cs
// Class that represents the orbit of a body around an attractor.
// Adapted from https://github.com/Karth42/SimpleKeplerOrbits

using System;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Orbit of a body around an attractor.
/// </summary>
public class Orbit {
	public Attractor attractor;

	// Primary variables that define this class
	private Vector3d m_SemiMajorAxisVec; // Vector from center of ellipse to periapse
	private Vector3d m_SemiMinorAxisVec; // Vector from center of ellipse representing the semi-minor axis
	private double m_PeriapsisDistance; // Only used for parabolic orbits
	private double m_Eccentricity;
	public double Eccentricity { get { return m_Eccentricity; } }

	private double m_EccentricAnomaly; 
	public double EccentricAnomaly { get { return m_EccentricAnomaly; } }

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
	/// <param name="inclination">Inclination from ecliptic, in radians.</param>
	/// <param name="argOfPerifocus">Argument of perifocus, in radians.</param>
	/// <param name="ascendingNode">Ascending node, in radians.</param>
	/// <param name="attractor">Parent gravitational body at focus of orbit.</param>
	public Orbit(double eccentricity, double semiMajorAxis, double inclination, double argOfPerifocus, double ascendingNode, Attractor attractor) 
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
			semiMinorAxis = 1.0;
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
		Vector3d periapsisVec      = Kepler.RotateVectorByAngle(ascendingNodeVec, argOfPerifocus, orbitNormalVec).normalized;
		
		// Set primary variables
		m_SemiMajorAxisVec = periapsisVec * semiMajorAxis;
		m_SemiMinorAxisVec = Vector3d.Cross(periapsisVec, orbitNormalVec) * semiMinorAxis;
		m_PeriapsisDistance = periapsisVec.magnitude;
		m_Eccentricity = eccentricity;
		m_EccentricAnomaly = 0.0; // default
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

		Vector3d minorDirection = angularMomentumVector.Cross(-eccVector).normalized;
		if (minorDirection.sqrMagnitude < 0.99) {
			minorDirection = orbitNormal.Cross(position).normalized;
			//Debug.Log("Invalid semiMinorAxisBasis.");
		}

		Vector3d majorDirection = orbitNormal.Cross(minorDirection).normalized;

		double majorDistance = 0.0; // semi-major axis distance
		double minorDistance = 0.0; // semi-minor axis distance
		double trueAnomaly = 0.0;

		if (m_Eccentricity < 1.0) { // Elliptical orbit
			double compression = 1.0 - m_Eccentricity * m_Eccentricity;
			majorDistance = focalParameter / compression;
			minorDistance = majorDistance * Math.Sqrt(compression);
			m_PeriapsisDistance = majorDistance * (1.0 - m_Eccentricity);
		} else if (m_Eccentricity > 1.0) { // Hyperbolic orbit
			double compression = m_Eccentricity * m_Eccentricity - 1.0;
			majorDistance = focalParameter / compression;
			minorDistance = majorDistance * Math.Sqrt(compression);
			m_PeriapsisDistance = majorDistance * (m_Eccentricity - 1.0);

		} else { // Parabolic orbit
			majorDistance = 1.0;
			minorDistance = 1.0;
			m_PeriapsisDistance = angularMomentumVector.sqrMagnitude / MG;
			Debug.Log("Parabolic orbits not tested and may have unexpected results.");
		}

		// Anomaly
		if (m_Eccentricity < 1.0) {
			trueAnomaly = Vector3d.Angle(position, majorDirection);
			if (position.Cross(-majorDirection).Dot(orbitNormal) < 0.0) {
				trueAnomaly = Kepler.PI_2 - trueAnomaly;
			}
		} else {
			trueAnomaly = Vector3d.Angle(position, eccVector);
			if (position.Cross(-majorDirection).Dot(orbitNormal) < 0.0) {
				trueAnomaly = -trueAnomaly;
			}
		}

		// Calculate the primary variables
		m_SemiMajorAxisVec = majorDirection * majorDistance;
		m_SemiMinorAxisVec = minorDirection * minorDistance;
		m_EccentricAnomaly = Kepler.GetEccentricAnomalyFromTrue(trueAnomaly, m_Eccentricity);

		// Debugging
		//double c = m_Eccentricity * majorDistance;
		//Debug.Log("e="+m_Eccentricity+" a="+m_SemiMajorAxisVec.ToString()+" b="+m_SemiMinorAxisVec.ToString()+" c="+c+" pe="+m_PeriapsisDistance);
	}

	/// <summary>
	/// Sets the position on the orbit using the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly in radians from periapse.</param>
	public void SetEccentricAnomaly(double eccentricAnomaly) 
	{
		m_EccentricAnomaly = eccentricAnomaly;
	}

	/// <summary>
	/// Sets the position on the orbit using the mean anomaly.
	/// </summary>
	/// <param name="meanAnomaly">The mean anomaly in radians from periapse.</param>
	public void SetMeanAnomaly(double meanAnomaly) 
	{
		m_EccentricAnomaly = Kepler.GetEccentricAnomalyFromMean(meanAnomaly, m_Eccentricity);
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

		bool loop = m_Eccentricity < 1.0 && ApoapsisDistance < maxDistance;
		double adjustedCount = pointsCount;
		double maxAngle = Kepler.PI;
		if (!loop) {
			adjustedCount -= 1.0;
			maxAngle = Kepler.TrueAnomalyForDistance(maxDistance, m_Eccentricity, SemiMajorAxis, pe);
		}
		
		Vector3d[] result = new Vector3d[pointsCount];
		for (int i = 0; i < pointsCount; i++){
			double trueAnomaly = -maxAngle + i * 2d * maxAngle / adjustedCount;
			result[i] = GetFocalPositionAtTrueAnomaly(trueAnomaly);
		}

		//Debug.Log("pe=" + pe + " maxAngle=" + maxAngle * Kepler.Rad2Deg + " sma=" + SemiMajorAxis);
				
		return result;
	}

	/// <summary>
	/// Gets the velocity given the eccentric anomaly.
	/// </summary>
	/// <param name="eccentricAnomaly">The eccentric anomaly.</param>
	/// <returns>Velocity vector.</returns>
	public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly)
	{
		return GetVelocityAtTrueAnomaly(Kepler.GetTrueAnomalyFromEccentric(eccentricAnomaly, Eccentricity));
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
		double vX = sqrtMGdivP * Math.Sin(trueAnomaly);
		double vY = sqrtMGdivP * (m_Eccentricity + Math.Cos(trueAnomaly));
		Vector3d major = m_SemiMajorAxisVec.normalized;
		Vector3d minor = m_SemiMinorAxisVec.normalized;
		return -major * vX - minor * vY;
	}



	/// <summary>
	/// Gets the position relative to the focal point given the true anomaly.
	/// </summary>
	/// <param name="trueAnomaly">The true anomaly.</param>
	/// <returns>Position relative to orbit focus.</returns>
	public Vector3d GetFocalPositionAtTrueAnomaly(double trueAnomaly) {
		double ecc = Kepler.GetEccentricAnomalyFromTrue(trueAnomaly, m_Eccentricity);
		return GetFocalPositionAtEccentricAnomaly(ecc);
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
			return -major * x - minor * y;
		} else if (m_Eccentricity > 1.0) {
			double x = Math.Cosh(eccentricAnomaly) * this.SemiMajorAxis;
			double y = Math.Sinh(eccentricAnomaly) * this.SemiMinorAxis;
			return -major * x - minor * y;
		} else {
			double x = m_PeriapsisDistance * Math.Cos(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly));
			double y = m_PeriapsisDistance * Math.Sin(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly));
			return -major * x - minor * y;
		}
	}
	
	/// <summary>
	/// Updates the current eccentric anomaly by time delta.
	/// </summary>
	/// <param name="deltaTime">Time increment in seconds.</param>
	public void UpdateWithTime(double deltaTime) 
	{
		double meanAnomaly = MeanAnomaly + MeanMotion * deltaTime;
		if (Eccentricity < 1.0) {
			// Keep anomaly values within range of 0 to PI_2
			meanAnomaly %= Kepler.PI_2;
			if (meanAnomaly < 0.0) meanAnomaly = Kepler.PI_2 - meanAnomaly;
			m_EccentricAnomaly = Kepler.GetEccentricAnomalyFromMean(meanAnomaly, m_Eccentricity);
		}
		m_EccentricAnomaly = Kepler.GetEccentricAnomalyFromMean(meanAnomaly, m_Eccentricity);
	}

	public double SemiMajorAxis {
		get { return m_SemiMajorAxisVec.magnitude; }
	}

	public double SemiMinorAxis {
		get { return m_SemiMinorAxisVec.magnitude; }
	}

	public Vector3d OrbitNormal {
		get { return m_SemiMinorAxisVec.Cross(m_SemiMajorAxisVec); }
	}

	public Vector3d CenterPoint {
		get { 
			if (m_Eccentricity < 1.0) {
				return -m_SemiMajorAxisVec * m_Eccentricity; 
			} else if (m_Eccentricity > 1.0) {
				return m_SemiMajorAxisVec * m_Eccentricity;
			} else {
				return Vector3d.zero;
			}
		}
	}

	public double PeriapsisDistance {
		get { 
			if (m_Eccentricity < 1.0) {
				return SemiMajorAxis * (1.0 - m_Eccentricity); 
			} else if (m_Eccentricity > 1.0) {
				return SemiMajorAxis * (m_Eccentricity - 1.0);
			} else {
				return m_PeriapsisDistance;
			}
		}
	}

	public double ApoapsisDistance {
		get { 
			if (m_Eccentricity < 1.0) {
				return SemiMajorAxis * (1.0 + m_Eccentricity);
			} else {
				return double.PositiveInfinity;
			}
		}
	}

	public double PeriapsisAltitude {
		get { return PeriapsisDistance - attractor.radius; }
	}

	public double ApoapsisAltitude {
		get { return ApoapsisDistance - attractor.radius; }
	}

	/// <summary>
	/// Gets the orbital period, or the time it takes to make one revolution around the orbit.
	/// </summary>
	/// <returns>Orbital period in seconds.</returns>
	public double OrbitalPeriod { 
		get {
			if (m_Eccentricity < 1.0) {
				double GM = Kepler.G * attractor.mass;
				double a = this.SemiMajorAxis; 
				return Kepler.PI_2 * Math.Sqrt(a * a * a / GM);
			} else {
				return double.PositiveInfinity;
			}
		}
	}

	public double MeanMotion {
		get {
			double GM = Kepler.G * attractor.mass;
			if (m_Eccentricity < 1.0) {
				return Kepler.PI_2 / OrbitalPeriod;
			} else if (m_Eccentricity > 1.0) {
				return Math.Sqrt(GM / Math.Pow(SemiMajorAxis, 3));
			} else {
				return Math.Sqrt(GM * 0.5 / Math.Pow(m_PeriapsisDistance, 3));
			}
		}
	}

	public double MeanAnomaly {
		get {
			return Kepler.GetMeanAnomalyFromEccentric(m_EccentricAnomaly, m_Eccentricity);
		}
	}

}