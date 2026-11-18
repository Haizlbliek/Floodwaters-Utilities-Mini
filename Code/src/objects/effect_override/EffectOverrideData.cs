namespace Floodwaters.Objects;

public class EffectOverrideData : PlacedObject.ResizableObjectData {
	public Vector2 panelPos = new Vector2(50f, 50f);
	public int colorA = -1;
	public int colorB = -1;
	public Vector3 modifyA = new Vector3(0f, 0.5f, 0.5f);
	public Vector3 modifyB = new Vector3(0f, 0.5f, 0.5f);
	public int fromDepth = 0;
	public int toDepth = 29;
	public float gradient = 0f;

	public EffectOverrideData(PlacedObject owner) : base(owner) {
		this.handlePos = new Vector2(100f, 100f);
		if (owner.type == Enums.EffectOverrideRectPO) {
			this.gradient = -1f;
		}
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorB = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.colorA = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.modifyB.x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.modifyB.y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.modifyB.z = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.modifyA.x = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.modifyA.y = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.modifyA.z = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.fromDepth = int.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.toDepth = int.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.gradient = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 15);
		}
		catch (Exception) {}
	}

	public new string BaseSaveString() {
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}",
			this.handlePos.x,
			this.handlePos.y,
			this.panelPos.x,
			this.panelPos.y,
			this.colorB,
			this.colorA,
			this.modifyB.x,
			this.modifyB.y,
			this.modifyB.z,
			this.modifyA.x,
			this.modifyA.y,
			this.modifyA.z,
			this.fromDepth,
			this.toDepth,
			this.gradient
		);
	}

	public override string ToString() {
		string baseString = this.BaseSaveString();
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
	}
}