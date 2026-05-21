namespace Floodwaters.Objects;

public class CustomLightArcData : CustomLightRodData {
	public Vector2 handleA = new Vector2(50f, 0f);
	public Vector2 handleB = new Vector2(-50f, 0f);

	public CustomLightArcData(PlacedObject owner) : base(owner) {
		this.handlePos = new Vector2(100f, 100f);
	}

	public override void FromString(string s) {
		base.FromString(s);
		try {
			string[] array = Regex.Split(s, "~");
			this.handleA.x = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.handleA.y = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.handleB.x = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.handleB.y = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 16);
		} catch (Exception) {}
	}

	public override string ToString() {
		return base.ToString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}",
			this.handleA.x,
			this.handleA.y,
			this.handleB.x,
			this.handleB.y
		);
	}
}