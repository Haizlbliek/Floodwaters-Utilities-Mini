namespace Floodwaters;

public static class Utils {
	public static string StringColor(Color color) {
		return "Color(" + Mathf.RoundToInt(color.r * 255f) + ", " + Mathf.RoundToInt(color.g * 255f) + ", " + Mathf.RoundToInt(color.b * 255f) + ")";
	}
}