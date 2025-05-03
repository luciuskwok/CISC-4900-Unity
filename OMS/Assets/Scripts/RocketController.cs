using UnityEngine;

public class RocketController : MonoBehaviour
{
	public double dryMass; // kg
	public double wetMass; // kg
	public double consumptionRate; // kg/second
	public double thrust; // kN

	public double countdown; // seconds to wait before release

	private bool isHeldDown = true; // when true, gravity and thrust are not applied
	private Vector3 velocity = new();
	private double gravity = 9.81; // m/s^2 

	void Start()
	{
		
	}

	void Update()
	{
		if (countdown > 0.0) {
			countdown -= Time.deltaTime;
			if (countdown <= 0.0) {
				countdown = 0.0;
			} else {
				return;
			}
		}

		if (isHeldDown) return;

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
}
