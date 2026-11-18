namespace Floodwaters.Objects;

public class GradientStyle : ExtEnum<GradientStyle> {
	public static readonly GradientStyle None = new GradientStyle(nameof(None), true);
	public static readonly GradientStyle Directional = new GradientStyle(nameof(Directional), true);
	public static readonly GradientStyle Center = new GradientStyle(nameof(Center), true);
	public static readonly GradientStyle Edges = new GradientStyle(nameof(Edges), true);
	public static readonly GradientStyle Depth = new GradientStyle(nameof(Depth), true);
	public static readonly GradientStyle CenterDepth = new GradientStyle(nameof(CenterDepth), true);
	public static readonly GradientStyle EdgesDepth = new GradientStyle(nameof(EdgesDepth), true);

	public static readonly Dictionary<GradientStyle, int> GradientIndices = new Dictionary<GradientStyle, int>() {
		{ None, 0 },
		{ Directional, 1 },
		{ Center, 2 },
		{ Depth, 3 },
		{ CenterDepth, 4 },
	};

	public GradientStyle(string value, bool register = false) : base(value, register) {
	}
}