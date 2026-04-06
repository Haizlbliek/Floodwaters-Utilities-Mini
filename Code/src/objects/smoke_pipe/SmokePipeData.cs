namespace Floodwaters.Objects;

public class SmokePipeData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos = new Vector2(100f, 100f);
	public bool eoc = false;

	public SmokePipeData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.eoc = array[2].Equals("EoC", StringComparison.InvariantCultureIgnoreCase);
			this.panelPos.x = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
		}
		catch (Exception) {}
	}

	public override string ToString() {
		string t = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}",
			this.handlePos.x,
			this.handlePos.y,
			this.eoc ? "EoC" : "Always",
			this.panelPos.x,
			this.panelPos.y
		);
		t = SaveState.SetCustomData(this, t);
		return SaveUtils.AppendUnrecognizedStringAttrs(t, "~", base.unrecognizedAttributes);
	}
}