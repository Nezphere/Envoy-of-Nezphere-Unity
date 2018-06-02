using Math = System.Math;

namespace Uif {
	public enum EasingType {
		Linear = 0,

		Sine,

		Quad,
		Cubic,
		Quart,
		Quint,
		Expo,

		Circ,

		Back,
		Elastic,
		Bounce
	}

	public enum EasingPhase {
		In = 0,
		Out,
		InOut
	}

	/// <summary>
	/// b: beginning value, c: change in value, t: current time, d: duration.
	/// </summary>
	public delegate double EasingFunction(double begin, double change, double time, double duration);

	public static class Easing {
		/// <summary>
		/// Calculate the eased value from begin to begin + change.
		/// </summary>
		/// <param name="type">Easing type.</param>
		/// <param name="phase">Easing phase.</param>
		/// <param name="begin">Begin value.</param>
		/// <param name="change">Value change.</param>
		/// <param name="time">Current time.</param>
		/// <param name="duration">Total duration.</param>
		public static float Ease(EasingType type, EasingPhase phase, double begin, double change, double time, double duration = 1) {
			return (float)GetEasingFuction(type, phase)(begin, change, time, duration);
		}

		/// <summary>
		/// Calculate the eased value from 0 to 1.
		/// </summary>
		/// <param name="type">Easing type.</param>
		/// <param name="phase">Easing phase.</param>
		/// <param name="time">Current time.</param>
		/// <param name="duration">Total duration.</param>
		public static float Ease(EasingType type, EasingPhase phase, double time, double duration = 1) {
			return (float)GetEasingFuction(type, phase)(0, 1, time, duration);
		}

		/// <summary>
		/// Get the easing fuction.
		/// </summary>
		/// <returns>The easing fuction.</returns>
		/// <param name="type">Easing type.</param>
		/// <param name="phase">Easing phase.</param>
		public static EasingFunction GetEasingFuction(EasingType type, EasingPhase phase) {
			return EasingFunctions[(int)type * 3 + (int)phase];
		}

#region Easing Functions

		public static readonly EasingFunction[] EasingFunctions = {
				Linear, Linear, Linear,

				SineIn, SineOut, SineInOut,

				QuadIn, QuadOut, QuadInOut,
				CubicIn, CubicOut, CubicInOut,
				QuartIn, QuartOut, QuartInOut,
				QuintIn, QuintOut, QuintInOut,

				ExpoIn, ExpoOut, ExpoInOut,

				CircIn, CircOut, CircInOut,

				BackIn, BackOut, BackInOut,
				ElasticIn, ElasticOut, ElasticInOut,
				BounceIn, BounceOut, BounceInOut
			};


		public static double Linear(double b, double c, double t, double d = 1) {
			return t / d * c + b;
		}

		public static double SineIn(double b, double c, double t, double d = 1) {
			return -c * Math.Cos(t / d * (Math.PI / 2)) + c + b;
		}

		public static double SineOut(double b, double c, double t, double d = 1) {
			return c * Math.Sin(t / d * (Math.PI / 2)) + b;
		}

		public static double SineInOut(double b, double c, double t, double d = 1) {
			return -c / 2 * (Math.Cos(Math.PI * t / d) - 1) + b;
		}

		public static double QuadIn(double b, double c, double t, double d = 1) {
			return c * (t /= d) * t + b;
		}

		public static double QuadOut(double b, double c, double t, double d = 1) {
			return -c * (t /= d) * (t - 2) + b;
		}

		public static double QuadInOut(double b, double c, double t, double d = 1) {
			if ((t /= d / 2) < 1) return c / 2 * t * t + b;
			return -c / 2 * ((--t) * (t - 2) - 1) + b;
		}

		public static double CubicIn(double b, double c, double t, double d = 1) {
			return c * (t /= d) * t * t + b;
		}

		public static double CubicOut(double b, double c, double t, double d = 1) {
			return c * ((t = t / d - 1) * t * t + 1) + b;
		}

		public static double CubicInOut(double b, double c, double t, double d = 1) {
			if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
			return c / 2 * ((t -= 2) * t * t + 2) + b;
		}

		public static double QuartIn(double b, double c, double t, double d = 1) {
			return c * (t /= d) * t * t * t + b;
		}

		public static double QuartOut(double b, double c, double t, double d = 1) {
			return -c * ((t = t / d - 1) * t * t * t - 1) + b;
		}

		public static double QuartInOut(double b, double c, double t, double d = 1) {
			if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
			return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
		}

		public static double QuintIn(double b, double c, double t, double d = 1) {
			return c * (t /= d) * t * t * t * t + b;
		}

		public static double QuintOut(double b, double c, double t, double d = 1) {
			return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
		}

		public static double QuintInOut(double b, double c, double t, double d = 1) {
			if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
			return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
		}

		public static double ExpoIn(double b, double c, double t, double d = 1) {
			return (t == 0) ? b : c * Math.Pow(2, 10 * (t / d - 1)) + b;
		}

		public static double ExpoOut(double b, double c, double t, double d = 1) {
			return (t == d) ? b + c : c * (-Math.Pow(2, -10 * t / d) + 1) + b;
		}

		public static double ExpoInOut(double b, double c, double t, double d = 1) {
			if (t == 0) return b;
			if (t == d) return b + c;
			if ((t /= d / 2) < 1) return c / 2 * Math.Pow(2, 10 * (t - 1)) + b;
			return c / 2 * (-Math.Pow(2, -10 * --t) + 2) + b;
		}

		public static double CircIn(double b, double c, double t, double d = 1) {
			return -c * (Math.Sqrt(1 - (t /= d) * t) - 1) + b;
		}

		public static double CircOut(double b, double c, double t, double d = 1) {
			return c * Math.Sqrt(1 - (t = t / d - 1) * t) + b;
		}

		public static double CircInOut(double b, double c, double t, double d = 1) {
			if ((t /= d / 2) < 1) return -c / 2 * (Math.Sqrt(1 - t * t) - 1) + b;
			return c / 2 * (Math.Sqrt(1 - (t -= 2) * t) + 1) + b;
		}

		public static double BackIn(double b, double c, double t, double d = 1) {
			double s = 1.70158;
			return c * (t /= d) * t * ((s + 1) * t - s) + b;
		}

		public static double BackOut(double b, double c, double t, double d = 1) {
			double s = 1.70158;
			return c * ((t = t / d - 1) * t * ((s + 1) * t + s) + 1) + b;
		}

		public static double BackInOut(double b, double c, double t, double d = 1) {
			double s = 1.70158;
			if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;
			return c / 2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;
		}

		public static double ElasticIn(double b, double c, double t, double d = 1) {
			if (t == 0) return b;
			if ((t /= d) == 1) return b + c;
			double s;
			double a = c;
			double p = d * .3;
			if (a < Math.Abs(c)) {
				a = c;
				s = p / 4;
			} else s = p / (2 * Math.PI) * Math.Asin(c / a);
			return -(a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
		}

		public static double ElasticOut(double b, double c, double t, double d = 1) {
			if (t == 0) return b;
			if ((t /= d) == 1) return b + c;
			double s;
			double a = c;
			double p = d * .3;
			if (a < Math.Abs(c)) {
				a = c;
				s = p / 4;
			} else s = p / (2 * Math.PI) * Math.Asin(c / a);
			return a * Math.Pow(2, -10 * t) * Math.Sin((t * d - s) * (2 * Math.PI) / p) + c + b;
		}

		public static double ElasticInOut(double b, double c, double t, double d = 1) {
			if (t == 0) return b;
			if ((t /= d / 2) == 2) return b + c;
			double s;
			double a = c;
			double p = d * (.3 * 1.5);
			if (a < Math.Abs(c)) {
				a = c;
				s = p / 4;
			} else s = p / (2 * Math.PI) * Math.Asin(c / a);
			if (t < 1) return -.5 * (a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
			return a * Math.Pow(2, -10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p) * .5 + c + b;
		}

		public static double BounceIn(double b, double c, double t, double d = 1) {
			return c - BounceOut(0, c, d - t, d) + b;
		}

		public static double BounceOut(double b, double c, double t, double d = 1) {
			if ((t /= d) < (1 / 2.75)) {
				return c * (7.5625 * t * t) + b;
			} else if (t < (2 / 2.75)) {
				return c * (7.5625 * (t -= (1.5 / 2.75)) * t + .75) + b;
			} else if (t < (2.5 / 2.75)) {
				return c * (7.5625 * (t -= (2.25 / 2.75)) * t + .9375) + b;
			} else {
				return c * (7.5625 * (t -= (2.625 / 2.75)) * t + .984375) + b;
			}
		}

		public static double BounceInOut(double b, double c, double t, double d = 1) {
			if (t < d / 2) return BounceIn(0, c, t * 2, d) * .5 + b;
			return BounceOut(0, c, t * 2 - d, d) * .5 + c * .5 + b;
		}

#endregion
	}
}