namespace Floodwaters.Objects;

public class ColoredDeepProcessingData : PlacedObject.QuadObjectData {
	public Vector2 panelPos;

	public int fromDepth = 0;
	public int toDepth = 30;
	public float intensity = 0.5f;
	public Color color = Color.blue;

	public ColoredDeepProcessingData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.fromDepth = int.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.toDepth = int.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.intensity = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.r = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.g = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.b = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 14);
		}
		catch (Exception) { }
	}

	public override string ToString() {
		string baseString = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}",
			this.panelPos.x,
			this.panelPos.y,
			this.fromDepth,
			this.toDepth,
			this.intensity,
			this.color.r,
			this.color.g,
			this.color.b
		);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
	}
}