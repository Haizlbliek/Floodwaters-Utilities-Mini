namespace Floodwaters;

public static class Utils {
	public static string StringColor(Color color) {
		return "Color(" + Mathf.RoundToInt(color.r * 255f) + ", " + Mathf.RoundToInt(color.g * 255f) + ", " + Mathf.RoundToInt(color.b * 255f) + ")";
	}

	public static (Vector2[], int[]) TrianglesToPoints(TriangleMesh.Triangle[] triangles) {
		List<int> indices = [];
		int vertexCount = 0;
		foreach (TriangleMesh.Triangle triangle in triangles) {
			vertexCount = Math.Max(Math.Max(Math.Max(vertexCount, triangle.a), triangle.b), triangle.c);
			indices.Add(triangle.a);
			indices.Add(triangle.b);
			indices.Add(triangle.c);
		}

		return (new Vector2[vertexCount + 1], [.. indices]);
	}
}