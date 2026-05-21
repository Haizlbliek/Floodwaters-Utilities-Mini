namespace Floodwaters.Objects;

public class CustomVineData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos = new Vector2(0, 100);
	public bool dropLeft = false;
	public bool dropRight = false;
	public string presetName = "vanilla vine";
	public float density = 1f;

	public CustomVineData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.dropLeft = bool.Parse(array[4]);
			this.dropRight = bool.Parse(array[5]);
			this.presetName = array[6].ToLowerInvariant();
			this.density = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		} catch (Exception) {}
	}

	public override string ToString() {
		string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}",
			this.panelPos.x,
			this.panelPos.y,
			this.dropLeft,
			this.dropRight,
			this.presetName,
			this.density
		);
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}
