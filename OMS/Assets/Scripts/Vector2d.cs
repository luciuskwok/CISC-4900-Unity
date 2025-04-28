using System;
using UnityEngine;

public struct Vector2d {
	public double x;
	public double y;
	private const double EPSILON = 1.401298E-45;

	public static Vector2d zero {
		get { return new Vector2d(0d, 0d); }
	}

	public static Vector2d one {
		get { return new Vector2d(1d, 1d); }
	}

	public Vector2d(double x, double y) {
		this.x = x;
		this.y = y;
	}

	// Convert to float-based Vector2
	public Vector2 vector2 {
		get { return new Vector2((float)this.x, (float)this.y); }
	}

	public static Vector2d operator +(Vector2d a, Vector2d b) {
		return new Vector2d(a.x + b.x, a.y + b.y);
	}

	public static Vector2d operator -(Vector2d a, Vector2d b) {
		return new Vector2d(a.x - b.x, a.y - b.y);
	}

	public static Vector2d operator *(Vector2d a, double d) {
		return new Vector2d(a.x * d, a.y *d);
	}

	public static Vector2d operator *(double d, Vector2d a) {
		return new Vector2d(a.x * d, a.y * d);
	}

	public static Vector2d operator /(Vector2d a, double b) {
		return new Vector2d(a.x / b, a.y / b);
	}

	public double magnitude {
		get { return Vector2d.Magnitude(this); }
	}

	public double sqrMagnitude {
		get { return Vector2d.SqrMagnitude(this); }
	}

	public Vector2d normalized {
		get { return Vector2d.Normalize(this); }
	}

	public static double Dot(Vector2d a, Vector2d b) {
		return a.x * b.x + a.y * b.y;
	}
	public static double Magnitude(Vector2d a) {
		return Math.Sqrt(a.x * a.x + a.y * a.y);
	}

	public static double SqrMagnitude(Vector2d a) {
		return a.x * a.x + a.y * a.y;
	}

	public static Vector2d Normalize(Vector2d a) {
		double mag = Vector2d.Magnitude(a);
		if (mag > EPSILON) {
			return a / mag;
		} else {
			return Vector2d.zero;
		}
	}

	public void Rotate(double angle) {
		double x1 = this.x * Math.Cos(angle) - this.y * Math.Sin(angle);
		double y1 = this.x * Math.Sin(angle) + this.y * Math.Cos(angle);
		this.x = x1;
		this.y = y1;
	}

	public void Scale(Vector2d scale) {
		this.x *= scale.x;
		this.y *= scale.y;
	}

}
