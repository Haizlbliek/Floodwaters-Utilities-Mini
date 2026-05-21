namespace Floodwaters.Objects;

public class LillypadData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos;
	public Color colorA;
	public Color colorB;
	public float darkness;

	public LillypadData(PlacedObject owner) : base(owner) {
		this.panelPos = new Vector2(100f, 50f);
		this.colorA = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.419f, 0.1f, 0.03f), Custom.ClampedRandomVariation(0.795f, 0.1f, 0.03f), Custom.ClampedRandomVariation(0.195f, 0.1f, 0.03f));
		this.colorB = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.419f, 0.1f, 0.03f), Custom.ClampedRandomVariation(0.7f, 0.1f, 0.03f), Custom.ClampedRandomVariation(0.1f, 0.1f, 0.03f));
		this.darkness = 0.5f;
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorA.r = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorA.g = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorA.b = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorB.r = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorB.g = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorB.b = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.darkness = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 11);
		} catch (Exception) {}
	}

	public override string ToString() {
		string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}",
			this.panelPos.x,
			this.panelPos.y,
			this.colorA.r,
			this.colorA.g,
			this.colorA.b,
			this.colorB.r,
			this.colorB.g,
			this.colorB.b,
			this.darkness
		);

		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}