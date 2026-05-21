namespace Floodwaters.Objects;

public class MagmaAreaData : PlacedObject.ResizableObjectData {
	public float burnTime = 16f;
	public Vector2 panelPos;
	public Color colorA = new Color(1f, 0.2f, 0f);
	public Color colorB = new Color(0.8f, 0.6f, 0f);

	public MagmaAreaData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.burnTime = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.x = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorA.r = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorA.g = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorA.b = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorB.r = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorB.g = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorB.b = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 11);
		} catch (Exception) {}
	}

	public override string ToString() {
		string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}",
			this.burnTime,
			this.panelPos.x,
			this.panelPos.y,
			this.colorA.r,
			this.colorA.g,
			this.colorA.b,
			this.colorB.r,
			this.colorB.g,
			this.colorB.b
		);
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}