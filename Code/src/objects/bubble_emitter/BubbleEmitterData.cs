namespace Floodwaters.Objects;

public class BubbleEmitterData : PlacedObject.Data {
	public Vector2 panelPos;
	public float intensity;
	public float age;

	public BubbleEmitterData(PlacedObject owner) : base(owner) {
		this.intensity = 0.5f;
		this.age = 0f;
	}

	public override void FromString(string s) {
		base.FromString(s);
		string[] array = Regex.Split(s, "~");
		try {
			this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.intensity = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.age = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		} catch (Exception) {
		}
		this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
	}

	protected string BaseSaveString() {
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", [
			this.panelPos.x,
			this.panelPos.y,
			this.intensity,
			this.age
		]);
	}

	public override string ToString() {
		string text = this.BaseSaveString();
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}
