namespace Floodwaters.Objects;

public class ExitLockData : PlacedObject.Data {
	public Vector2 panelPos;
	public string key = "";
	public bool ignoreRegionSpecific = false;
	public bool ignoreSaveSpecific = false;

	public ExitLockData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.key = array[2];
			this.ignoreRegionSpecific = array[3] == "y";
			this.ignoreSaveSpecific = array[4] == "y";
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		} catch (Exception) {}
	}

	public override string ToString() {
		string text = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}",
			this.panelPos.x,
			this.panelPos.y,
			this.key,
			this.ignoreRegionSpecific ? "y" : "n",
			this.ignoreSaveSpecific ? "y" : "n"
		);
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}
