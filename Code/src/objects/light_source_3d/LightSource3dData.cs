namespace Floodwaters.Objects;

public class LightSource3dData : PlacedObject.LightSourceData {
	public int depthRange = 3;
	public int depth = 0;

	public LightSource3dData(PlacedObject owner) : base(owner) {
		this.flat = false;
	}

	public new string BaseSaveString() {
		return base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", this.depth, this.depthRange);
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.depth = int.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.depthRange = int.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 13);
		}
		catch {}
	}

	public override string ToString() {
		this.flat = false;
		string baseString = this.BaseSaveString();
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
	}
}
