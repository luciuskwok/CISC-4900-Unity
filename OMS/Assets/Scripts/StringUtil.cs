// StringUtil.cs

using System;


public class StringUtil {
	
	public static String FormatTimeWithLabels(double timeAsSeconds) {
		if (double.IsInfinity(timeAsSeconds)) return "Infinite";

		String s = "";
		double t = timeAsSeconds;
		const double day = 24.0 * 3600.0;

		if (timeAsSeconds >= day) {
			s += Math.Floor(t/day).ToString("F0") + "d ";
			t -= Math.Floor(t/day) * 3600.0;
		}

		if (timeAsSeconds >= 3600.0) {
			s += Math.Floor(t/3600.0).ToString("F0") + "h ";
			t -= Math.Floor(t/3600.0) * 3600.0;
		}

		if (timeAsSeconds >= 60.0) {
			s += Math.Floor(t/60.0).ToString("F0") + "m ";
			t -= Math.Floor(t/60.0) * 60.0;

		}

		if (timeAsSeconds >= 60.0) {
			s += Math.Floor(t).ToString("F0");
		} else {
			s += t.ToString("F2");
		}
		s += "s";
		return s;
	}

	public static String FormatTimeWithColons(double timeAsSeconds) {
		if (double.IsInfinity(timeAsSeconds)) return "Infinite";

		String s = "";
		double t = timeAsSeconds;
		// Hours
		s += Math.Floor(t/3600.0).ToString("00") + ":";
		t -= Math.Floor(t/3600.0) * 3600.0;

		// Minutes
		s += Math.Floor(t/60.0).ToString("00") + ":";
		t -= Math.Floor(t/60.0) * 60.0;

		s += Math.Floor(t).ToString("00");
	
		return s;
	}
}