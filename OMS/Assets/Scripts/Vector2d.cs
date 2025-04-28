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

	public Vector2d normalized {
		get { return Vector2d.Normalize(this); }
	}

	public static Vector2d Normalize(Vector2d a) {
		double mag = Vector2d.Magnitude(a);
		if (mag > EPSILON) {
			return a / mag;
		} else {
			return Vector2d.zero;
		}
	}
	public double magnitude {
		get { return Vector2d.Magnitude(this); }
	}

	public static double Magnitude(Vector2d a) {
		return Math.Sqrt(a.x * a.x + a.y * a.y);
	}

	public double sqrMagnitude {
		get { return Vector2d.SqrMagnitude(this); }
	}

	public static double SqrMagnitude(Vector2d a) {
		return a.x * a.x + a.y * a.y;
	}

	public static double Dot(Vector2d a, Vector2d b) {
		return a.x * b.x + a.y * b.y;
	}

	public Vector2d Rotated(double angle) {
		return Vector2d.Rotate(this, angle);
	}

	public static Vector2d Rotate(Vector2d vec, double angle) {
		double x1 = vec.x * Math.Cos(angle) - vec.y * Math.Sin(angle);
		double y1 = vec.x * Math.Sin(angle) + vec.y * Math.Cos(angle);
		return new Vector2d(x1, y1);
	}

	public Vector2d Scaled(Vector2d scale) {
		return Vector2d.Scale(this, scale);
	}

	public static Vector2d Scale(Vector2d vec, Vector2d scale) {
		return new Vector2d(vec.x * scale.x, vec.y * scale.y);
	}

}
