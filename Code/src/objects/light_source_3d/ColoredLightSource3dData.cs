namespace Floodwaters.Objects;

public class ColoredLightSource3dData : LightSource3dData {
	public Color color = new Color(Random.value * 0.5f + 0.5f, Random.value * 0.5f + 0.5f, Random.value * 0.5f + 0.5f);

	public ColoredLightSource3dData(PlacedObject owner) : base(owner) {
		this.colorType = PlacedObject.LightSourceData.ColorType.White;
	}

	public new string BaseSaveString() {
		return base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}", this.color.r, this.color.g, this.color.b);
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.color.r = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.g = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.b = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 16);
		}
		catch {}
		this.flat = false;
		this.colorType = PlacedObject.LightSourceData.ColorType.White;
	}

	public override string ToString() {
		this.flat = false;
		this.colorType = PlacedObject.LightSourceData.ColorType.White;
		string baseString = this.BaseSaveString();
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
	}
}
