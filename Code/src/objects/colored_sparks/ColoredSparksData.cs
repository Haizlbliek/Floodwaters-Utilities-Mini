namespace Floodwaters.Objects;

public class ColoredSparksData : PlacedObject.Data {
	public float amount = 0.1f;
	public Vector3 color = new Vector3(0f, 1f, 0.5f);
	public Vector3 colorVariation = new Vector3(0.1f, 0.1f, 0.1f);
	public float direction = 0f;
	public float directionVariation = 0.25f;
	public Vector2 panelPos = Vector2.zero;
	public float speedScale = 1f;
	public bool depth0 = true;
	public bool depth1 = true;
	public bool depth2 = true;
	public bool depth3 = true;

	public ColoredSparksData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		try {
			string[] array = Regex.Split(s, "~");
			this.amount = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.x = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.y = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color.z = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.direction = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.x = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorVariation.x = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorVariation.y = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorVariation.z = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.directionVariation = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.speedScale = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.depth0 = array[12].Contains("0");
			this.depth1 = array[12].Contains("1");
			this.depth2 = array[12].Contains("2");
			this.depth3 = array[12].Contains("3");
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 13);
		} catch {}
	}

	public string BaseSaveString() {
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}",
			this.amount,
			this.color.x,
			this.color.y,
			this.color.z,
			this.direction,
			this.panelPos.x,
			this.panelPos.y,
			this.colorVariation.x,
			this.colorVariation.y,
			this.colorVariation.z,
			this.directionVariation,
			this.speedScale,
			(this.depth0 ? "0" : "") + (this.depth1 ? "1" : "") + (this.depth2 ? "2" : "") + (this.depth3 ? "3" : "")
		);
	}

	public override string ToString() {
		string baseString = this.BaseSaveString();
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
	}
}