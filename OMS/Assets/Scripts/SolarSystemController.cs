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

		// Earth
		SpawnOrbit("Earth", 0.017, 1.49598e8, 0, 114.21, 0.0);

	}

	GameObject SpawnOrbit(string name, double eccentricity, double semiMajorAxis, double inclination, double argOfPerifocusDeg, double ascendingNode) 
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
		plot.attractor = m_Sun;
		plot.SetOrbitalElements(eccentricity, semiMajorAxis, inclination, argOfPerifocusDeg * Kepler.Deg2Rad, ascendingNode);
		plot.PeriapsisTime = 0.0;
		plot.animationTimeScale = m_AnimationTimeScale;

		// Set up the Line Renderer
		LineRenderer lineRenderer = orbit.GetComponent<LineRenderer>();
		const float lineWidth = 20.0f;
		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;

		return orbit;
	}


	void Update()
	{
		
	}
}
