namespace Floodwaters.Objects;

public class ColoredFlameJetData : FlameJet.FlameJetData {
	public ColoredFlameJet Obj => this.obj as ColoredFlameJet;
	public bool paletteSmoke = false;
	public Color smokeColor = Color.grey;
	public Color darkColor = Color.blue;
	public Color midColor = Color.red;
	public Color lightColor = Color.yellow;
	public Color brightColor = Color.white;
	public int depth = 0;

	public ColoredFlameJetData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.smokeColor = Custom.hexToColor(array[24]);
			this.darkColor = Custom.hexToColor(array[25]);
			this.midColor = Custom.hexToColor(array[26]);
			this.lightColor = Custom.hexToColor(array[27]);
			this.brightColor = Custom.hexToColor(array[28]);
			this.paletteSmoke = array[29].Equals("palette", StringComparison.InvariantCultureIgnoreCase);
			this.depth = this.ParseInt(array[30]);
		}
		catch (Exception) {}
	}

	public override string ToString() {
		string[] backup = this.unrecognizedAttributes;
		this.unrecognizedAttributes = [];
		string baseString = base.ToString() + string.Format("~{0}~{1}~{2}~{3}~{4}~{5}~{6}",
			Custom.colorToHex(this.smokeColor),
			Custom.colorToHex(this.darkColor),
			Custom.colorToHex(this.midColor),
			Custom.colorToHex(this.lightColor),
			Custom.colorToHex(this.brightColor),
			this.paletteSmoke ? "palette" : "color",
			this.depth
		);
		this.unrecognizedAttributes = backup;
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
	}
}