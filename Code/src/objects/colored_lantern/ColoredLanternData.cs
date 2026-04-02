namespace Floodwaters.Objects;

public class ColoredLanternData : PlacedObject.ConsumableObjectData {
	public Color color1 = new Color(1.0f, 0.2f, 0.0f);
	public Color color2 = new Color(1.0f, 0.2f, 0.0f);
	public bool dead = false;
	public Vector2? stickEnd;

	public ColoredLanternData(PlacedObject owner, bool hasStick)
	: base(owner) {
		this.panelPos = new Vector2(0, 0);
		this.stickEnd = hasStick ? new Vector2(0f, 100f) : null;
	}

	public override void FromString(string s) {
		Debug.Log(s);
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.color1 = Custom.hexToColor(array[4]);
			this.color2 = Custom.hexToColor(array[5]);
			this.dead = bool.Parse(array[6]);
			if (array.Length >= 9) {
				this.stickEnd = new Vector2(
					float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture),
					float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture)
				);
			}
			else {
				this.stickEnd = null;
			}
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, array.Length >= 9 ? 9 : 7);
		} catch (Exception) {}
	}

	protected new string BaseSaveString() {
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}",
			this.panelPos.x,
			this.panelPos.y,
			this.minRegen,
			this.maxRegen,
			Custom.colorToHex(this.color1),
			Custom.colorToHex(this.color2),
			this.dead
		) + (this.stickEnd == null ? "" : string.Format(CultureInfo.InvariantCulture, "~{0}~{1}",
			this.stickEnd?.x ?? 100f,
			this.stickEnd?.y ?? 100f
		));
	}

	public override string ToString() {
		string text = this.BaseSaveString();
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}
}