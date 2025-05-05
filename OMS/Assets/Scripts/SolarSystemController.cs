using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SolarSystemController : MonoBehaviour
{
	public GameObject orbitPlotPrefab;
	public GameObject orbitsParent;
	public GameObject nodesParent;

	private Attractor m_Sun;
	private double m_AnimationTimeScale;
	private int m_PlanetCount = 8;
	private GameObject[] m_OrbitPlots;
	private GameObject[] m_Nodes;
	

	void Start()
	{
		// Animation time scale: 1 year = 15 seconds
		m_AnimationTimeScale = 15.0 * 60.0 * 24.0 * 365.0;

		// Sun      mass, kg   radius    influence, km
		m_Sun = new(1.9885e30, 1.3914e6, 1.0e12);

		// Nodes
		int count = nodesParent.transform.childCount;
		m_Nodes = new GameObject[count];
		for (int index = 0; index < count; index++) {
			m_Nodes[index] = nodesParent.transform.GetChild(index).gameObject;
		}

		// Orbit Plots
		m_OrbitPlots = new GameObject[m_PlanetCount];
		int i = 0;
		// Planets                               hue     ecc        SMA   incl     AOP      AN    MLAE
		m_OrbitPlots[i++] = SpawnOrbit("Mercury",  0, 0.2056, 5.79091e7, 7.006,  29.12,  48.34, 252.25);
		m_OrbitPlots[i++] = SpawnOrbit("Venus",  290, 0.0068, 1.08209e8, 3.398,  54.88,  76.67, 181.98);
		m_OrbitPlots[i++] = SpawnOrbit("Earth",  190, 0.0167, 1.49598e8, 0.000, 114.21,   0.00, 100.47);
		m_OrbitPlots[i++] = SpawnOrbit("Mars",    20, 0.0934, 2.27940e8, 1.852, 286.50,  49.71, 355.43);
		m_OrbitPlots[i++] = SpawnOrbit("Jupiter", 40, 0.0489, 7.78478e8, 1.299, 273.87, 100.29,  34.33);
		m_OrbitPlots[i++] = SpawnOrbit("Saturn",  60, 0.0565, 1.43354e9, 2.494, 339.39, 113.64,  50.08);
		m_OrbitPlots[i++] = SpawnOrbit("Uranus", 160, 0.0472, 2.87097e9, 0.077,  97.00,  73.96, 314.20);
		m_OrbitPlots[i++] = SpawnOrbit("Neptune",210, 0.0087, 4.49841e9, 1.770, 273.19, 131.79, 304.22);
		
	}

	GameObject SpawnOrbit(string name, float hue, double eccentricity, double semiMajorAxis, double inclinationDeg, double argOfPerifocusDeg, double ascendingNodeDeg, double meanLongAtEpoch) 
	{
		// Create the game object from a prefab
		GameObject orbit = Instantiate(orbitPlotPrefab, Vector3.zero, Quaternion.identity);
		orbit.transform.parent = orbitsParent.transform;
		// Reset the transform because assigning a parent causes the child's transform to change
		orbit.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		orbit.transform.localScale = Vector3.one;

		// Set up the orbital elements
		OrbitPlot plot = orbit.GetComponent<OrbitPlot>();
		plot.animationTimeScale = m_AnimationTimeScale;
		plot.color = Color.HSVToRGB(hue / 360.0f, 0.925f, 0.925f);
		plot.attractor = m_Sun;
		plot.SetOrbitalElements(eccentricity, 
			semiMajorAxis, 
			inclinationDeg * Kepler.Deg2Rad, 
			argOfPerifocusDeg * Kepler.Deg2Rad, 
			ascendingNodeDeg * Kepler.Deg2Rad);

		// Calculate the time of periapsis passage
		// Epoch J2000, 00:00 UTC on 1/1/2000, is the zero value
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
		UpdatePlanetPositions(0);
	}

	void UpdatePlanetPositions(double atTime) {
		for (int i = 0; i < m_PlanetCount; i++) {
			OrbitPlot plot = m_OrbitPlots[i].GetComponent<OrbitPlot>();
			Vector3 position = plot.GetWorldPositionAtTime(atTime);

			if (i < m_Nodes.Length) {
				UINode node = m_Nodes[i].GetComponent<UINode>();
				node.SetWorldPosition(position);
			}
		}
	}
}
