namespace Floodwaters.Objects;

public class SandDripData : PlacedObject.Data {
	public Color sandColor;
	public Vector2 panelPos;
	public int pileSize;

	public SandDripData(PlacedObject owner) : base(owner) {
		this.sandColor = Color.white;
		this.panelPos = new Vector2(0f, 0f);
		this.pileSize = 10;
	}

	public override void FromString(string s) {
		base.FromString(s);
		string[] array = Regex.Split(s, "~");
		try {
			this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.sandColor.r = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.sandColor.g = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.sandColor.b = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.pileSize = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
		} catch (Exception) {
		}
		this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
	}

	protected string BaseSaveString() {
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", [
			this.panelPos.x,
			this.panelPos.y,
			this.sandColor.r,
			this.sandColor.g,
			this.sandColor.b,
			this.pileSize
		]);
	}

	public override string ToString() {
		string text = this.BaseSaveString();
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}