namespace Floodwaters.Objects;

public class ColoredCoralNeuronData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos = new Vector2(100f, 0f);
	public Color mainColor = Color.red;
	public Color tendrilStartColor = Color.red;
	public Color tendrilMidColor = new Color(0.102f, 0.302f, 0.286f);
	public Color tendrilEndColor = Color.blue;

	public ColoredCoralNeuronData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.mainColor.r = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.mainColor.g = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.mainColor.b = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilStartColor.r = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilStartColor.g = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilStartColor.b = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilMidColor.r = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilMidColor.g = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilMidColor.b = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilEndColor.r = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilEndColor.g = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.tendrilEndColor.b = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 16);
		} catch (Exception) {}
	}

	public override string ToString() {
		string baseString = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}",
			this.panelPos.x,
			this.panelPos.y,
			this.mainColor.r,
			this.mainColor.g,
			this.mainColor.b,
			this.tendrilStartColor.r,
			this.tendrilStartColor.g,
			this.tendrilStartColor.b,
			this.tendrilMidColor.r,
			this.tendrilMidColor.g,
			this.tendrilMidColor.b,
			this.tendrilEndColor.r,
			this.tendrilEndColor.g,
			this.tendrilEndColor.b
		);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
	}
}
