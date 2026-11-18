namespace Floodwaters.Objects;

public class PaletteOverrideData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos;

	// Visibility
	public int fromDepth = 0;
	public int toDepth = 29;
	public float minOpacity = 0f;
	public float maxOpacity = 1f;
	public bool affectSky = true;

	// Shape
	public Shape shape = Shape.Rectangle;
	public GradientStyle gradientStyle = GradientStyle.None;
	public float gradientValue = 0f;

	// Color
	public float hue = 0f;
	public float saturation = 1f;
	public float brightness = 0f;
	public float darkness = 0f;
	public float contrast = 0f;

	// Tweaks
	public float grime = 0.5f;
	public bool wetTerrain = true;
	public float clouds = 0f;

	// Palette
	public int palette = 0;
	public List<(int palette, float strength)> fadePalettes = [];

	public PaletteOverrideData(PlacedObject owner) : base(owner) {
		this.handlePos = new Vector2(100f, 100f);
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.fromDepth = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.toDepth = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.affectSky = array[6].Equals("yes", StringComparison.InvariantCultureIgnoreCase);
			this.minOpacity = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.maxOpacity = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.shape = (Shape) Shape.Parse(typeof(Shape), array[9], true);
			this.gradientStyle = (GradientStyle) GradientStyle.Parse(typeof(GradientStyle), array[10], true);
			this.gradientValue = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.hue = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.saturation = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.brightness = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.darkness = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.contrast = float.Parse(array[16], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.grime = float.Parse(array[17], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.wetTerrain = array[18].Equals("yes", StringComparison.InvariantCultureIgnoreCase);
			this.clouds = float.Parse(array[19], NumberStyles.Any, CultureInfo.InvariantCulture);

			this.palette = int.Parse(array[20], NumberStyles.Any, CultureInfo.InvariantCulture);
			int count = int.Parse(array[21], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.fadePalettes = new List<(int, float)>(count);
			for (int i = 0; i < count; i++) {
				int p = 22 + 2 * i;
				this.fadePalettes[i] = (
					int.Parse(array[p], NumberStyles.Any, CultureInfo.InvariantCulture),
					float.Parse(array[p + 1], NumberStyles.Any, CultureInfo.InvariantCulture)
				);
			}

			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, count * 2 + 22);
		} catch (Exception) {}
	}

	public override string ToString() {
		string text = this.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}~{19}",
			this.panelPos.x,
			this.panelPos.y,
			this.fromDepth,
			this.toDepth,
			this.affectSky ? "yes" : "no",
			this.minOpacity,
			this.maxOpacity,
			this.shape.value,
			this.gradientStyle.value,
			this.gradientValue,
			this.hue,
			this.saturation,
			this.brightness,
			this.darkness,
			this.contrast,
			this.grime,
			this.wetTerrain ? "yes" : "no",
			this.clouds,
			this.palette,
			this.fadePalettes.Count
		);
		for (int i = 0; i < this.fadePalettes.Count; i++) {
			text += string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", this.fadePalettes[i].palette, this.fadePalettes[i].strength);
		}
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}

	public class Shape : ExtEnum<Shape> {
		public static readonly Shape Rectangle = new Shape(nameof(Rectangle), true);
		public static readonly Shape Circle = new Shape(nameof(Circle), true);

		public Shape(string value, bool register = false) : base(value, register) {
		}
	}
}