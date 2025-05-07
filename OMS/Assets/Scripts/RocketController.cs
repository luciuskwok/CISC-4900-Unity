using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RocketController : MonoBehaviour
{
	public double dryMass; // kg
	public double wetMass; // kg
	public double wetMassCapacity; // kg
	public double consumptionRate; // kg/second
	public double thrust; // kN
	public double missionTime; // seconds

	// UI Widgets
	public TMP_Text speedReadout; 
	public TMP_Text altitudeReadout;
	public TMP_Text timeReadout;
	public GameObject LOXGauge;
	public GameObject FuelGauge;
	public GameObject pitchIndicator;

	// Engine particle effects
	public GameObject[] engineParticles;

	// Sounds
	public GameObject engineSound;
	public GameObject[] birdSounds;

	private bool areEnginesRunning = false;
	private float engineStartTime = -8.6f; // engines start at T-8.9 to T-8.3 seconds
	private bool isHeldDown = true; // when true, gravity and thrust are not applied
	private float pitchRate = -0.5f; // degrees per second
	private float pitchProgramStart = 13.0f; 
	private float pitchProgramEnd = 38.0f;
	private double endMissionTime = 40.0; // seconds; when to go to next scene
	private Vector3 velocity = new();
	private double gravity = 9.81; // m/s^2 

	void Start()
	{
		UpdateUI();
		StartCoroutine(PlayBirdSounds());
	}

	IEnumerator PlayBirdSounds() {
		while (!areEnginesRunning) {
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, 5.0f));
			if (!areEnginesRunning) {
				int count = birdSounds.Length;
				int i = UnityEngine.Random.Range(0, count);
				birdSounds[i].GetComponent<AudioSource>().Play();
			}
		}
	}

	void Update()
	{
		// Update mission time and release hold downs
		double oldTIme = missionTime;
		missionTime += Time.deltaTime;
		if (missionTime >= 0.0) isHeldDown = false;
		if (missionTime >= endMissionTime) {
			GoToNextScene();
			return;
		}

		// Start engines
		if (!areEnginesRunning && missionTime >= engineStartTime) {
			areEnginesRunning = true;
			// Start all engine effects at same time in this sim, even though real-life engines had a staggered start
			for (int i = 0; i < engineParticles.Length; i++) {
				engineParticles[i].GetComponent<ParticleSystem>().Play();
			}
			// Start engine sound loop
			engineSound.GetComponent<AudioSource>().Play();
		}

		// Apply forces if hold downs are released
		if (!isHeldDown) {
			// Turn rocket to point at velocity vector
			
			
			// Pitch program
			if (missionTime >= pitchProgramStart && missionTime <= pitchProgramEnd) 
			{
				var rot = transform.localEulerAngles;
				rot.z += pitchRate * Time.deltaTime;
				transform.localEulerAngles = rot;
			}

			// Rocket thrust
			// force = mass * acceleration
			// acceleration = force / mass
			double totalMass = dryMass + wetMass;
			double rocketAcceleration = 0.0;
			if (wetMass > 0.0) {
				rocketAcceleration = thrust / totalMass * 1000.0;
				wetMass -= consumptionRate * Time.deltaTime;
				if (wetMass < 0.0) wetMass = 0.0;
			}

			// Apply rocket acceleration to rotated Y-axis to velocity vector
			Vector3 forwardVec = transform.TransformDirection(Vector3.up) * (float)rocketAcceleration;

			// Apply gravity
			Vector3 gravityVec = Vector3.down * (float)gravity;

			// Add both accelerations to rocket;
			velocity += (forwardVec + gravityVec) * Time.deltaTime;

			// Translate according to velocity;
			transform.Translate(velocity * Time.deltaTime);
		}

		UpdateUI();
	}

	void UpdateUI() {
		// Speed
		double speed = velocity.magnitude;
		speedReadout.text = speed.ToString("#,##0") + " m/s";

		// Altitude
		Vector3 position;
		Quaternion rotation; 
		transform.GetPositionAndRotation(out position, out rotation);
		double altitude = position.y;
		if (altitude < 2000.0) {
			altitudeReadout.text = altitude.ToString("#,##0") + " m";
		} else {
			altitudeReadout.text = (altitude/1000.0).ToString("#,##0") + " km";
		}

		// Time
		if (missionTime < 0.0) {
			double t = -missionTime;
			timeReadout.text = "T-" + StringUtil.FormatTimeWithColons(t);
		} else {
			timeReadout.text = "T+" + StringUtil.FormatTimeWithColons(missionTime);
		}

		// Fuel and LOX Gauges
		double x = wetMass / wetMassCapacity;
		FillGauge lox = LOXGauge.GetComponent<FillGauge>();
		lox.FillValue = (float)x;
		FillGauge fuel = FuelGauge.GetComponent<FillGauge>();
		fuel.FillValue = (float)x;

		// Pitch indicator
		var rot = transform.localEulerAngles;
		var pitch = rot.z;
		var pitchIndicatorRT = pitchIndicator.GetComponent<RectTransform>();
		pitchIndicatorRT.localEulerAngles = new Vector3(0, 0, pitch);

	}

	public void GoToNextScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.buildIndex + 1);
	}

}
