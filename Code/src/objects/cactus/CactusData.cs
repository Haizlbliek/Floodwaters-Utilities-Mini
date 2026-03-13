namespace Floodwaters.Objects;

public class CactusData : PlacedObject.Data {
	public CactusData(PlacedObject owner) : base(owner) {
		this.seed = Random.Range(0, 10001);
		this.size = Random.Range(0.95f, 1.1f);
		this.scale = Random.Range(0.9f, 1.2f);
		this.hueOffset = 0.0f;
		this.satOffset = 0.0f;
		this.valOffset = 0.0f;
		this.productType = Random.Range(0, 3);
	}

	protected string BaseSaveString() {
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}", [
			this.panelPos.x,
			this.panelPos.y,
			this.seed,
			this.size,
			this.scale,
			this.hueOffset,
			this.satOffset,
			this.valOffset,
			this.productType
		]);
	}

	public override string ToString() {
		string text = this.BaseSaveString();
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}

	public override void FromString(string s) {
		string[] array = Regex.Split(s, "~");
		this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
		this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		this.seed = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		if (array.Length >= 5) {
			this.size = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.scale = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		if (array.Length >= 9) {
			this.hueOffset = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.satOffset = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.valOffset = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.productType = int.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
		} else {
			this.productType = 3;
		}

		this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 9);
	}

	public Vector2 panelPos;
	public int seed;
	public float size;
	public float scale;
	public float hueOffset;
	public float satOffset;
	public float valOffset;
	public int productType;
}