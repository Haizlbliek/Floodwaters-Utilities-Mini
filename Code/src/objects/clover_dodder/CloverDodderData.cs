namespace Floodwaters.Objects;

public class CloverDodderData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos = new Vector2(0, 100);
	public ColorType colorType = ColorType.EffectColorA;
	public float primaryDensity = 0.8f;
	public float secondaryDensity = 0.7f;
	public float stickiness = 0.5f;
	public Color color;


	public CloverDodderData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorType = new ColorType(array[4]);
			this.primaryDensity = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.secondaryDensity = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.stickiness = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.r = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.g = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.b = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 11);
		} catch (Exception) {}
	}

	public override string ToString() {
		string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}",
			this.panelPos.x,
			this.panelPos.y,
			this.colorType,
			this.primaryDensity,
			this.secondaryDensity,
			this.stickiness,
			this.color.r,
			this.color.g,
			this.color.b
		);
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}

	public class ColorType : ExtEnum<ColorType> {
		public static readonly ColorType EffectColorA = new ColorType(nameof(EffectColorA), true);
		public static readonly ColorType EffectColorB = new ColorType(nameof(EffectColorB), true);
		public static readonly ColorType Custom = new ColorType(nameof(Custom), true);
		public static readonly ColorType Dead = new ColorType(nameof(Dead), true);

		public ColorType(string value, bool register = false) : base(value, register) {
		}
	}
}