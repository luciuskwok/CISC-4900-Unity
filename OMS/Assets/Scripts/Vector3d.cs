using System;
using UnityEngine;

// Adapted from https://github.com/Karth42/SimpleKeplerOrbits


public struct Vector3d {
	public double x;
	public double y;
	public double z;
	private const double EPSILON = 1.401298E-45;

	public static Vector3d zero {
		get { return new Vector3d(0d, 0d, 0d); }
	}

	public static Vector3d one {
		get { return new Vector3d(1d, 1d, 1d); }
	}

	public Vector3d(double x, double y, double z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	// convert to float-based Vector3
	public Vector3 Vector3 {
		get { return new Vector3((float)this.x, (float)this.y, (float)this.z); }
	}
	
	public static Vector3d operator +(Vector3d a, Vector3d b) {
		return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static Vector3d operator -(Vector3d a, Vector3d b) {
		return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static Vector3d operator -(Vector3d a) {
		return new Vector3d(-a.x, -a.y, -a.z);
	}

	public static Vector3d operator *(Vector3d a, double d) {
		return new Vector3d(a.x * d, a.y * d, a.z * d);
	}

	public static Vector3d operator *(double d, Vector3d a) {
		return new Vector3d(a.x * d, a.y * d, a.z * d);
	}

	public static Vector3d operator /(Vector3d a, double d) {
		return new Vector3d(a.x / d, a.y / d, a.z / d);
	}

	public static bool operator ==(Vector3d lhs, Vector3d rhs) {
		return Vector3d.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
	}

	public static bool operator !=(Vector3d lhs, Vector3d rhs) {
		return Vector3d.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
	}

	public override int GetHashCode() {
		return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
	}

	public override bool Equals(object other) {
		if (!(other is Vector3d)) return false;

		Vector3d a = (Vector3d)other;
		return this.x == a.x && this.y == a.y && this.z == a.z;
	}

	public override string ToString() {
		return "(" + this.x + "; " + this.y + "; " + this.z + ")";
	}

	public string ToString(string format) {
		return "(" + this.x.ToString(format) + "; " + this.y.ToString(format) + "; " + this.z.ToString(format) + ")";
	}

	public Vector3d normalized {
		get { return Vector3d.Normalize(this); }
	}
	
	public static Vector3d Normalize(Vector3d value) {
		double mag = Vector3d.Magnitude(value);
		if (mag <= EPSILON) return Vector3d.zero;
		return value / mag;
	}

	public double magnitude {
		get { return Vector3d.Magnitude(this); }
	}

	public static double Magnitude(Vector3d a) {
		return Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
	}

	public double sqrMagnitude {
		get { return Vector3d.SqrMagnitude(this); }
	}

	public static double SqrMagnitude(Vector3d a) {
		return a.x * a.x + a.y * a.y + a.z * a.z;
	}

	public Vector3d Cross(Vector3d b) {
		return Vector3d.Cross(this, b);
	}

	public static Vector3d Cross(Vector3d a, Vector3d b) {
		return new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
	}

	public double Dot(Vector3d b) {
		return Vector3d.Dot(this, b);
	}

	public static double Dot(Vector3d a, Vector3d b) {
		return a.x * b.x + a.y * b.y + a.z * b.z;
	}

	public static double Distance(Vector3d a, Vector3d b) {
		Vector3d d = a - b;
		return Math.Sqrt(d.x * d.x + d.y * d.y + d.z * d.z);
	}

	public static double Angle(Vector3d from, Vector3d to) {
		double dot = Dot(from.normalized, to.normalized);
		return Math.Acos(dot < -1.0 ? -1.0 : (dot > 1.0 ? 1.0 : dot));
	}


}

