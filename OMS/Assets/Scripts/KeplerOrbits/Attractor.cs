// Attractor.cs
// Class that represents a gravitational attractor body.
// Adapted from https://github.com/Karth42/SimpleKeplerOrbits

using System;
using Unity;
using UnityEngine;

public class Attractor {
	public double mass; // kg
	public double radius; // km
	public double influence; // km, for sphere of influence
	public Vector3d focusPosition; // position of focus in universe coordinates

	public static Attractor Sun {
		get { 
			return new Attractor(1.9885e30 /* kg mass */, 1.3914e6 /* km radius */, 1.0e12 /* km influence */ );
		}
	}

	public static Attractor Earth {
		get { 
			return new Attractor(5.9722e24 /* kg mass */, 6378.0 /* km radius */, 9.29e5 /* km influence */ );
		}
	}

	public Attractor(double mass, double radius, double influence) {
		this.mass = mass;
		this.radius = radius;
		this.influence = influence;
		focusPosition = Vector3d.zero;
	}

}