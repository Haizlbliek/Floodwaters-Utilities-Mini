namespace Floodwaters.Objects;

public class LanternfruitData : PlacedObject.ConsumableObjectData {

	public ColorType colorType = ColorType.EffectColorA;
	public Color color = Color.white;


	public LanternfruitData(PlacedObject owner) : base(owner) {
	}

    public override void FromString(string s) {
        base.FromString(s);

        try {
        	string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorType = new ColorType(array[4]);
			this.color.r = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.g = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.b = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
       }
       catch (Exception) {}
  }

	private string LanternfruitSaveString() {
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}",
			this.panelPos.x,
			this.panelPos.y,
			this.minRegen,
			this.maxRegen,
			this.colorType,
			this.color.r,
			this.color.g,
			this.color.b
		);
	}

	public override string ToString() {
		string text = this.LanternfruitSaveString();
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
	
