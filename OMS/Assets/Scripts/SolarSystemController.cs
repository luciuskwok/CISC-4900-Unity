using UnityEngine;

public class SolarSystemController : MonoBehaviour
{
	public GameObject orbitPlotPrefab;
	public GameObject orbitsParent;

	private Attractor m_Sun;
	private double m_AnimationTimeScale;
	

	void Start()
	{
		// Animation time scale: 1 year = 1 minute
		m_AnimationTimeScale = 60.0 * 24.0 * 365.0;

		// Sun
		m_Sun = new(1.9885e30, 1.3914e6, 1.0e12);

		// Planets
		SpawnOrbit("Mercury", 0.206, 5.7909e7, 7.006,  29.124, 48.34, 252.25);
		SpawnOrbit("Venus",   0.007, 1.0821e8, 3.398,  54.884, 76.67, 252.25);
		SpawnOrbit("Earth",   0.017, 1.49598e8, 0.00, 114.21,   0.00, 100.47);
		SpawnOrbit("Mars",    0.094, 2.2794e8, 1.852, 286.5,   49.71, 355.43);
	}

	GameObject SpawnOrbit(string name, double eccentricity, double semiMajorAxis, double inclinationDeg, double argOfPerifocusDeg, double ascendingNodeDeg, double meanLongAtEpoch) 
	{
		// Create the game object from a prefab
		GameObject orbit = Instantiate(orbitPlotPrefab, Vector3.zero, Quaternion.identity);
		orbit.transform.parent = orbitsParent.transform;
		// Reset the transform because assigning a parent causes the child's transform to change
		orbit.transform.localPosition = Vector3.zero;
		orbit.transform.localRotation = Quaternion.identity;
		orbit.transform.localScale = Vector3.one;

		// Set up the orbital elements
		OrbitPlot plot = orbit.GetComponent<OrbitPlot>();
		plot.animationTimeScale = m_AnimationTimeScale;
		plot.attractor = m_Sun;
		plot.SetOrbitalElements(eccentricity, 
			semiMajorAxis, 
			inclinationDeg * Kepler.Deg2Rad, 
			argOfPerifocusDeg * Kepler.Deg2Rad, 
			ascendingNodeDeg * Kepler.Deg2Rad);
		plot.PeriapsisTime = 0.0;
		// TODO: calculate the time of periapsis passage from the meanLongAtEpoch where epoch is Epoch J2000.

		// Set up the Line Renderer
		LineRenderer lineRenderer = orbit.GetComponent<LineRenderer>();
		const float lineWidth = 20.0f;
		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;

		// TODO: save name somewhere

		return orbit;
	}


	void Update()
	{
		
	}
}
