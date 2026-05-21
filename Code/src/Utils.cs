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

	public static IntVector2? RayTraceTilesForTerrainReturnFirstSolidBackground(Room room, IntVector2 a, IntVector2 b) {
		return RayTraceTilesForTerrainReturnFirstSolidBackground(room, a.x, a.y, b.x, b.y);
	}

	public static IntVector2? RayTraceTilesForTerrainReturnFirstSolidBackground(Room room, int x0, int y0, int x1, int y1) {
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = (x1 > x0) ? 1 : (-1);
		int num7 = (y1 > y0) ? 1 : (-1);
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		int num9 = 0;
		while (num5 > 0) {
			if (room.GetTile(num3, num4).wallbehind) {
				return new IntVector2(num3, num4);
			}

			if (num8 > 0) {
				num3 += num6;
				num8 -= num2;
			}
			else {
				num4 += num7;
				num8 += num;
			}

			num9++;
			if (num9 > 10000) {
				Custom.LogWarning("GAH!!!!!!", x0.ToString(), y0.ToString(), x1.ToString(), y1.ToString());
				break;
			}

			num5--;
		}

		return null;
	}
}