namespace Floodwaters.Objects;

public class DeerSkullData : PlacedObject.ResizableObjectData {
	public DeerSkullData(PlacedObject owner) : base(owner) {
		System.Random random = new System.Random();
		this.direction = (float)random.NextDouble();
		this.skullSeed = random.Next(0, 101);
		this.hasPaint = true;
		this.paintHue = (float) random.NextDouble();
	}

	public override void FromString(string s) {
		base.FromString(s);

		string[] array = Regex.Split(s, "~");
		this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		this.direction = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
		this.skullSeed = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
		this.hasPaint = bool.Parse(array[6]);
		this.paintHue = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
		this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
	}

	public override string ToString() {
		string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}", [
			this.panelPos.x,
			this.panelPos.y,
			this.direction,
			this.skullSeed,
			this.hasPaint,
			this.paintHue
		]);

		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
	}

	public Vector2 panelPos;
	public float direction;
	public int skullSeed;
	public bool hasPaint;
	public float paintHue;
}