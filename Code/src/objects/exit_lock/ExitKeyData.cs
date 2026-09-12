namespace Floodwaters.Objects;

public class ExitKeyData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos;
	public string key = "";
	public string announcement = "";
	public bool saveSpecific = false;

	public ExitKeyData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.key = array[4];
			this.announcement = array[5];
			this.saveSpecific = array[6] == "y";
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		} catch (Exception) {}
	}

	public override string ToString() {
		string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}",
			this.panelPos.x,
			this.panelPos.y,
			this.key,
			this.announcement,
			this.saveSpecific ? "y" : "n"
		);
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}
