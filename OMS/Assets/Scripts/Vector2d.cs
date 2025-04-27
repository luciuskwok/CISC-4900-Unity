using System;

public struct Vector2d {
	public double x;
	public double y;

	public double magnitude {
		get { return Math.Sqrt(this.x * this.x + this.y * this.y); }
	}

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
