using TMPro;
using UnityEngine;

public class SolarSystemController : MonoBehaviour
{
	public GameObject orbitPlotPrefab;
	public GameObject orbitsParent;
	public GameObject nodesParent;
	public GameObject targetsParent;

	// Camera
	public GameObject cameraTarget;
	private float m_DistanceMax = 8.8e10f; // km
	private float m_DistanceMin = 1.0f; // varies based on target
	private float m_MousePanTiltSpeed = 0.1f;
	private float m_KeyboardPanTiltSpeed = 30.0f;

	// UI elements
	public TMP_Text targetReadout;
	public TMP_Text distanceReadout;
	

	private Attractor m_Sun;
	private Attractor m_Earth;
	private int m_TargetIndex;
	private Vector3 m_LastMousePosition;
	

	void Start()
	{
		// Animation time scale: 1 year = 15 seconds
		//m_AnimationTimeScale = 60.0/15.0 * 60.0 * 24.0 * 365.0;

		// Attractors
		m_Sun = Attractor.Sun;
		m_Earth = Attractor.Earth;		
	
		// Orbit Plots
		SetUpOrbit(0, 0.2056, 5.79091e7, 7.006,  29.12,  48.34, 252.25, m_Sun); // Mercury
		SetUpOrbit(1, 0.0068, 1.08209e8, 3.398,  54.88,  76.67, 181.98, m_Sun); // Venus
		SetUpOrbit(2, 0.0167, 1.49598e8, 0.000, 114.21,   0.00, 100.47, m_Sun); // Earth
		SetUpOrbit(3, 0.0055, 3.84748e5, 5.100,      0,   0.00,      0, m_Earth); // Moon
		SetUpOrbit(4, 0.0934, 2.27940e8, 1.852, 286.50,  49.71, 355.43, m_Sun); // Mars
		SetUpOrbit(5, 0.0489, 7.78478e8, 1.299, 273.87, 100.29,  34.33, m_Sun); // Jupiter
		SetUpOrbit(6, 0.0565, 1.43354e9, 2.494, 339.39, 113.64,  50.08, m_Sun); // Saturn
		SetUpOrbit(7, 0.0472, 2.87097e9, 0.077,  97.00,  73.96, 314.20, m_Sun); // Uranus
		SetUpOrbit(8, 0.0087, 4.49841e9, 1.770, 273.19, 131.79, 304.22, m_Sun); // Neptune
		
		// Move planets into position
		UpdatePlanetPositions(0);

		// Target
		SetCameraTargetAtIndex(2); // Start with Earth
	}

	GameObject SetUpOrbit(int index, double eccentricity, double semiMajorAxis, double inclinationDeg, double argOfPerifocusDeg, double ascendingNodeDeg, double meanLongAtEpoch, Attractor attractor) 
	{
		// Get the orbit plot
		GameObject orbit = orbitsParent.transform.GetChild(index).gameObject;

		// Set up the orbital elements
		OrbitPlot plot = orbit.GetComponent<OrbitPlot>();
		plot.attractor = attractor;
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
		float lineWidth = semiMajorAxis > 1.0e6? 20.0f : 0.2f;
		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;
		
		return orbit;
	}

	void Update()
	{
		UpdatePlanetPositions(0);
	}

	void UpdatePlanetPositions(double atTime) {
		int count = orbitsParent.transform.childCount;

		// Update gradient, node, and bodiesd of all planets and moons
		for (int i = 0; i < count; i++) {
			OrbitPlot plot = orbitsParent.transform.GetChild(i).gameObject.GetComponent<OrbitPlot>();
			Vector3 position = plot.GetWorldPositionAtTime(atTime);

			if (i == 2) { // Special case for Moon-Earth system
				GameObject moon = orbitsParent.transform.GetChild(3).gameObject;
				moon.transform.localPosition = moon.transform.worldToLocalMatrix * position;
			}

			// Set orbit plot color gradient
			plot.UpdateGradientWithTime(atTime);

			// Position node on 2D canvas
			if (i < nodesParent.transform.childCount) {
				UINode node = nodesParent.transform.GetChild(i).gameObject.GetComponent<UINode>();
				node.SetWorldPosition(position);
			}

			// Position planets in 3d world space
			if (i < targetsParent.transform.childCount) {
				Transform target = targetsParent.transform.GetChild(i);
				target.localPosition = target.worldToLocalMatrix * position;
			}
		}
	}

	void LateUpdate()
	{
		// Shift key
		bool isShifted = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		float shiftSpeed = isShifted ? 0.1f : 1.0f;

		// Scroll wheel
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll != 0.0f) {
			DollyCamera(scroll * shiftSpeed);
		}

		// Mouse movement
		if (Input.GetMouseButtonDown(0)) {
			// Initial mouse down
			m_LastMousePosition = Input.mousePosition;
			// TODO: ignore mouse clicks in buttons or other UI elements
		} else if (Input.GetMouseButton(0)) {
			Vector3 delta = Input.mousePosition - m_LastMousePosition;
			if (delta.x != 0.0f || delta.y != 0.0f) {
				// Invert y-axis look
				PanTiltCamera(delta.x * shiftSpeed * m_MousePanTiltSpeed, -delta.y * shiftSpeed * m_MousePanTiltSpeed);
			}
			m_LastMousePosition = Input.mousePosition;
		}

		// ## Keyboard
		// Pan/Tilt
		float movement = Time.deltaTime * m_KeyboardPanTiltSpeed * shiftSpeed; 
		if (Input.GetKey(KeyCode.UpArrow)) {
			PanTiltCamera(0.0f, movement);
		} else if (Input.GetKey(KeyCode.DownArrow)) {
			PanTiltCamera(0.0f, -movement);
		} else if (Input.GetKey(KeyCode.LeftArrow)) {
			PanTiltCamera(movement, 0.0f);
		} else if (Input.GetKey(KeyCode.RightArrow)) {
			PanTiltCamera(-movement, 0.0f);
		}
		// Dolly
		else if (Input.GetKey(KeyCode.Minus)) {
			DollyCamera(-movement * 0.2f);
		} else if (Input.GetKey(KeyCode.Equals)) {
			DollyCamera(movement * 0.2f);
		}

		// Switching Targets
		if (Input.GetKeyDown(KeyCode.LeftBracket))
		{
			GoToPreviousTarget();
		}
		else if (Input.GetKeyDown(KeyCode.RightBracket))
		{
			GoToNextTarget();
		}


		UpdateReadouts();
	}


	void DollyCamera(float delta) {
		Camera camera = Camera.main;
		float distance = -camera.transform.localPosition.z;

		// Change the distance based on scroll wheel movement
		distance = Mathf.Pow(10.0f, Mathf.Log10(distance) - delta * 0.2f);

		// Clamp the values to min and max
		distance = (distance < m_DistanceMin) ? m_DistanceMin : distance;
		distance = (distance > m_DistanceMax) ? m_DistanceMax : distance;

		// Set new camera position
		camera.transform.localPosition = new Vector3(0, 0, -distance);
	}

	void PanTiltCamera(float deltaX, float deltaY)
	{
		const float tiltLimit = 89.9f;
		float pan = cameraTarget.transform.eulerAngles.y;
		float tilt = cameraTarget.transform.eulerAngles.x;

		pan += deltaX;
		tilt += deltaY;

		// Limit tilt
		tilt = (tilt + 360.0f) % 360.0f;
		if (tilt > 180.0f) tilt -= 360.0f;

		if (tilt < -tiltLimit) tilt = -tiltLimit;
		if (tilt > tiltLimit) tilt = tiltLimit;

		cameraTarget.transform.localEulerAngles = new Vector3(tilt, pan, 0);
	}

	public void GoToNextTarget() {
		SetCameraTargetAtIndex(m_TargetIndex + 1);
	}

	public void GoToPreviousTarget() {
		SetCameraTargetAtIndex(m_TargetIndex - 1);
	}

	void SetCameraTargetAtIndex(int index)
	{
		int count = targetsParent.transform.childCount;
		index %= count;
		if (index < 0) index += count;
		m_TargetIndex = index;
		GameObject newTarget = targetsParent.transform.GetChild(index).gameObject;

		// Update minimum distance
		Transform planetBody = newTarget.transform.GetChild(0);
		m_DistanceMin = planetBody.localScale.x * 1.5f;

		// Move camera if needed
		Camera camera = Camera.main;
		if (-camera.transform.localPosition.z < m_DistanceMin) {
			camera.transform.localPosition = new Vector3(0, 0, -m_DistanceMin);
		}

		// Set new target position
		cameraTarget.transform.position = newTarget.transform.position;

		// Set target readout
		targetReadout.text = newTarget.name;
	}

	void UpdateReadouts() {
		Camera camera = Camera.main;
		float distance = -camera.transform.localPosition.z;

		distanceReadout.text = distance.ToString("#,###,###") + " km";
	}


}
