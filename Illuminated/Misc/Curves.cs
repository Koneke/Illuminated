using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Illuminated.Client
{
	public abstract class Curve
	{
		protected Vector2 delta;
		protected int duration;
		protected int t;

		public Vector2 Current =>
			this.evaluate();

		public bool Expired =>
			t >= duration;

		protected float factor =>
			(float)this.t / this.duration;

		public Curve(Vector2 delta, int duration)
		{
			this.delta = delta;
			this.duration = duration;
		}

		protected abstract Vector2 evaluate();

		public void Tick(int ms)
		{
			this.t += ms;
		}
	}

	public class LinearCurve : Curve
	{
		protected override Vector2 evaluate() =>
			this.delta * this.factor;

		public LinearCurve(Vector2 ds, int d) : base(ds, d) { }
	}

	public class CurveSystem
	{
		private List<Curve> curves = new List<Curve>();

		public Vector2 Current => this.curves.Any()
			? this.curves
				.Select(c => c.Current)
				.Aggregate((acc, c) => acc + c)
			: Vector2.Zero;

		public void Tick(int ms)
		{
			foreach (var curve in this.curves)
			{
				curve.Tick(ms);
			}

			this.curves = this.curves
				.Where(c => !(c.Expired))
				.ToList();
		}

		public CurveSystem Add(Curve curve)
		{
			this.curves.Add(curve);

			return this;
		}

		public CurveSystem Remove(Curve curve)
		{
			this.curves.Remove(curve);

			return this;
		}
	}
}
