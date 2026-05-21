namespace Floodwaters.Objects;

public class CustomLightRodData : PlacedObject.ResizableObjectData {
	public float brightness = 0.5f;
	public float depth;
	public Vector2 panelPos = Custom.DegToVec(120f) * 20f;
	public Color color1 = Color.red;
	public Color color2 = Color.green;

	public CustomLightRodData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);
		try {
			string[] array = Regex.Split(s, "~");
			this.color1.r = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color1.g = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color1.b = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.brightness = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.depth = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.x = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color2.r = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color2.g = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color2.b = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 12);
		} catch (Exception) {}
	}

	public override string ToString() {
		return base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}",
			this.color1.r,
			this.color1.g,
			this.color1.b,
			this.brightness,
			this.depth,
			this.panelPos.x,
			this.panelPos.y,
			this.color2.r,
			this.color2.g,
			this.color2.b
		);
	}
}