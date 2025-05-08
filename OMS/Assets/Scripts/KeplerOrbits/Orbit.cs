// Orbit.cs
// Class that represents the orbit of a body around an attractor.
// Adapted from https://github.com/Karth42/SimpleKeplerOrbits

using System;
using UnityEngine;

/// <summary>
/// Orbit of a body around an attractor. This class uses the convention that positive Z-axis is north, which is different from Unity.
/// </summary>
public class Orbit {

	// ## Primary fields which are the minimum to describe an orbit

	private Vector3d m_SemiMajorAxisVec; // Semi-major axis (a): Vector from center to periapsis

	private Vector3d m_SemiMinorAxisVec; // Semi-minor axis (b)

	private double m_PeriapsisDistance; // Needed for parabolic orbits.

	private double m_Eccentricity; // Determines if orbit is elliptical, parabolic, or hyperbolic, and how the above fields are interpreted.

	/// <summary>
	/// Time of periapsis passage (T₀): time at which the orbiting body is at periapsis, which is when the mean anomaly and true anomaly are zero.
	/// </summary>
	public double periapsisTime; 

	/// <summary>
	/// The gravitational body at the focus of the orbit.
	/// </summary>
	public Attractor attractor;

	// ## Accessors for the primary fields

	/// <summary>
	/// Eccentricty (e): Describes whether the orbit is elliptical, parabolic, or hyperbolic, which affects how the semi-major and semi-minor axes are interpreted.
	/// </summary>
	public double Eccentricity { get { return m_Eccentricity; } }

	// ## Derived values from the primary fields

	/// <summary>
	/// Length of the semi-major axis (a). For parabolic orbits, this value is 0.
	/// </summary>
	public double SemiMajorAxisLength { 
		get { return m_Eccentricity == 1.0? 0.0 : m_SemiMajorAxisVec.magnitude; } 
	}

	/// <summary>
	/// Length of the semi-minor axis (b). For parabolic orbits, this value is 0.
	/// </summary>
	public double SemiMinorAxisLength { 
		get { return m_Eccentricity == 1.0? 0.0 : m_SemiMinorAxisVec.magnitude; } 
	}


	// Constants

	/// <summary>
	/// Ecliptic Normal: the north direction, or the positive z-axis.
	/// </summary>
	public static readonly Vector3d EclipticNormal = new Vector3d(0, 0, 1);

	/// <summary>
	/// Ecliptic Up: the positive y-axis.
	/// </summary>
	public static readonly Vector3d EclipticUp = new Vector3d(0, 1, 0);

	/// <summary>
	/// Ecliptic Right: the positive x-axis.
	/// </summary>
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
	/// <param name="periapsisTime">Time of periapsis passage (T₀): point in time at which the orbiting body is at periapsis.</param>
	/// <param name="attractor">Parent gravitational body at focus of orbit.</param>
	public Orbit(double eccentricity, double semiMajorAxis, double inclination, double argOfPerifocus, double ascendingNode, Attractor attractor) 
	{		
		// Public fields
		this.attractor = attractor;
		this.periapsisTime = 0.0;

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
	}

	/// <summary>
	/// Construct an orbit, given the position and velocity relative to the focus, at a given point in time.
	/// </summary>
	/// <param name="position">Position relative to focus.</param>
	/// <param name="velocity">Velocity vector relative to focus.</param>
	/// <param name="atTime">Point in time of the maneuver.</param>
	/// <param name="attractor">Parent gravitational body at focus of orbit.</param>
	public Orbit(Vector3d position, Vector3d velocity, double atTime, Attractor attractor) 
	{
		this.attractor = attractor;
		SetOrbitByThrowing(position, velocity, atTime);
	}

	/// <summary>
	/// Changes the orbital parameters by "throwing" an object from position with velocity relative to focus.
	/// </summary>
	/// <param name="position">Position relative to focus.</param>
	/// <param name="velocity">Velocity vector relative to focus.</param>
	/// <param name="atTime">Point in time of the maneuver.</param>
	public void SetOrbitByThrowing(Vector3d position, Vector3d velocity, double atTime) 
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
			Debug.Log("Parabolic orbits have not been tested and may have unexpected results.");
		}

		// Epoch-describing elements
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

		// Set the periapsis time by working backwards from mean anomaly
		double eccAnomaly = Kepler.ConvertTrueAnomalyToEccentric(trueAnomaly, m_Eccentricity);
		double meanAnomaly = Kepler.ConvertEccentricAnomalyToMean(eccAnomaly, m_Eccentricity);
		double timeSincePeriapsis = meanAnomaly * MeanMotion;
		periapsisTime = atTime - timeSincePeriapsis;

		// Debugging
		//double c = m_Eccentricity * majorDistance;
		//Debug.Log("e="+m_Eccentricity+" a="+m_SemiMajorAxisVec.ToString()+" b="+m_SemiMinorAxisVec.ToString()+" c="+c+" pe="+m_PeriapsisDistance);
		
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
			maxAngle = Kepler.TrueAnomalyForDistance(maxDistance, m_Eccentricity, SemiMajorAxisLength, pe);
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
		return GetVelocityAtTrueAnomaly(Kepler.ConvertEccentricAnomalyToTrue(eccentricAnomaly, Eccentricity));
	}

	/// <summary>
	/// Gets the velocity given the true anomaly.
	/// </summary>
	/// <param name="trueAnomaly">The true anomaly.</param>
	/// <returns>Velocity vector.</returns>
	public Vector3d GetVelocityAtTrueAnomaly(double trueAnomaly) {
		double e = m_Eccentricity;
		double compression = e < 1.0 ? (1.0 - e * e) : (e * e - 1.0);
		double focalParameter = this.SemiMajorAxisLength * compression;

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
		double ecc = Kepler.ConvertTrueAnomalyToEccentric(trueAnomaly, m_Eccentricity);
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
			double x = -Math.Cos(eccentricAnomaly) * this.SemiMajorAxisLength;
			double y = Math.Sin(eccentricAnomaly) * this.SemiMinorAxisLength;
			return -major * x - minor * y;
		} else if (m_Eccentricity > 1.0) {
			double x = Math.Cosh(eccentricAnomaly) * this.SemiMajorAxisLength;
			double y = Math.Sinh(eccentricAnomaly) * this.SemiMinorAxisLength;
			return -major * x - minor * y;
		} else {
			double x = m_PeriapsisDistance * Math.Cos(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly));
			double y = m_PeriapsisDistance * Math.Sin(eccentricAnomaly) / (1.0 + Math.Cos(eccentricAnomaly));
			return -major * x - minor * y;
		}
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
				return SemiMajorAxisLength * (1.0 - m_Eccentricity); 
			} else if (m_Eccentricity > 1.0) {
				return SemiMajorAxisLength * (m_Eccentricity - 1.0);
			} else {
				return m_PeriapsisDistance;
			}
		}
	}

	public double ApoapsisDistance {
		get { 
			if (m_Eccentricity < 1.0) {
				return SemiMajorAxisLength * (1.0 + m_Eccentricity);
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
				double a = this.SemiMajorAxisLength; 
				return Kepler.PI_2 * Math.Sqrt(a * a * a / GM);
			} else {
				return double.PositiveInfinity;
			}
		}
	}

	/// <summary>
	/// Mean motion: the change in the mean anomaly, in the form of the angular velocity.
	/// </summary>
	public double MeanMotion {
		get {
			double GM = Kepler.G * attractor.mass;
			if (m_Eccentricity < 1.0) {
				return Kepler.PI_2 / OrbitalPeriod;
			} else if (m_Eccentricity > 1.0) {
				return Math.Sqrt(GM / Math.Pow(SemiMajorAxisLength, 3));
			} else {
				return Math.Sqrt(GM * 0.5 / Math.Pow(m_PeriapsisDistance, 3));
			}
		}
	}

	/// <summary>
	/// Sets the time of periapsis passage, given a mean anomaly at a point in time.
	/// </summary>
	/// <param name="deltaTime">Time increment in seconds.</param>
	public void SetPeriapsisTimeWithMeanAnomaly(double meanAnomaly, double atTime) 
	{
		double timeSincePeriapsis = meanAnomaly / MeanMotion;
		periapsisTime = atTime - timeSincePeriapsis;
		//Debug.Log("periapsisTime="+periapsisTime);
	}

	/// <summary>
	/// Gets the mean anomaly, given a point in time, calculated from the time of periapsis passage and the mean motion.
	/// </summary>
	/// <param name="atTime">The point in time.</param>
	/// <returns>Mean anomaly in radians.</returns>
	public double GetMeanAnomalyAtTime(double atTime) 
	{
		double timeDiff = atTime - periapsisTime;
		double meanAnomaly = timeDiff * MeanMotion;
		if (m_Eccentricity < 1.0) meanAnomaly = Kepler.NormalizedAnomaly(meanAnomaly);
		return meanAnomaly;
	}

	/// <summary>
	/// Gets the eccentric anomaly, given a point in time.
	/// </summary>
	/// <param name="atTime">The point in time.</param>
	/// <returns>Eccentric anomaly in radians.</returns>
	public double GetEccentricAnomalyAtTime(double atTime) 
	{
		double meanAnomaly = GetMeanAnomalyAtTime(atTime);
		return Kepler.ConvertMeanAnomalyToEccentric(meanAnomaly, m_Eccentricity);
	}

	/// <summary>
	/// Gets the position on the orbit relative to the focus, given a point in time.
	/// </summary>
	/// <param name="atTime">The point in time.</param>
	/// <returns>World position vector.</returns>
	public Vector3d GetFocalPositionAtTime(double atTime) {
		double eccAnomaly = GetEccentricAnomalyAtTime(atTime);
		return GetFocalPositionAtEccentricAnomaly(eccAnomaly);
	}

	/// <summary>
	/// Converts mean anomaly to eccentric anomaly.
	/// </summary>
	/// <param name="meanAnomaly">Mean anomaly in radians.</param>
	/// <returns>Eccentric anomaly in radians.</returns>
	public double ConvertMeanAnomalyToEccentric(double meanAnomaly) 
	{
		return Kepler.ConvertMeanAnomalyToEccentric(meanAnomaly, m_Eccentricity);
	}

	/// <summary>
	/// Converts true anomaly to mean anomaly.
	/// </summary>
	/// <param name="trueAnomaly">True anomaly in radians.</param>
	/// <returns>Mean anomaly in radians.</returns>
	public double ConvertTrueAnomalyToMean(double trueAnomaly) 
	{
		double ecc = Kepler.ConvertTrueAnomalyToEccentric(trueAnomaly, m_Eccentricity);
		return Kepler.ConvertEccentricAnomalyToMean(ecc, m_Eccentricity);
	}

	/// <summary>
	/// Converts true anomaly to eccentric anomaly.
	/// </summary>
	/// <param name="trueAnomaly">True anomaly in radians.</param>
	/// <returns>Eccentric anomaly in radians.</returns>
	public double ConvertTrueAnomalyToEccentric(double trueAnomaly) 
	{
		return Kepler.ConvertTrueAnomalyToEccentric(trueAnomaly, m_Eccentricity);
	}

	/// <summary>
	/// Gets the true anomaly value for a distance from the focus.
	/// </summary>
	/// <param name="distance">The distance from the focus.</param>
	/// <returns>True anomaly in radians.</returns>
	public double TrueAnomalyForDistance(double distance)
	{
		return Kepler.TrueAnomalyForDistance(distance, m_Eccentricity, SemiMajorAxisLength, m_PeriapsisDistance);
	}


}