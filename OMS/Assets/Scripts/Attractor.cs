using System;
using Unity;

public class Attractor {
	public double mass; // kg
	public double radius; // km
	public double influence; // km

	private static Attractor m_Earth;
	public static Attractor Earth {
		get { 
			if (m_Earth == null) {
				m_Earth = new Attractor(
					5.9722e24, // kg mass
					6378.0, // km radius
					9.29e5 // km influence
				);
			}
			return m_Earth;
		}
	}

	public Attractor(double mass, double radius, double influence) {
		this.mass = mass;
		this.radius = radius;
		this.influence = influence;
	}

}