namespace Floodwaters.Objects;

public class AbstractColoredLantern : AbstractConsumable {
	public Color color1;
	public Color color2;
	public bool dead;

	public AbstractColoredLantern(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, Color color1, Color color2, bool dead, ColoredLanternData consumableData)
	: base(world, Enums.ColoredLantern, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData) {
		this.color1 = color1;
		this.color2 = color2;
		this.dead = dead;
		this.HandleDead();
	}

	public void HandleDead() {
		if (this.dead) {
			float average1 = (this.color1.r + this.color1.g + this.color1.b) / 3f;
			float average2 = (this.color2.r + this.color2.g + this.color2.b) / 3f;
			this.color1 = Color.Lerp(this.color1, new Color(average1, average1, average1, this.color1.a), 0.5f);
			this.color2 = Color.Lerp(this.color2, new Color(average2, average2, average2, this.color2.a), 0.5f);
		}
	}

	public override string ToString() {
		string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}<oA>{6}<oA>{7}",
			this.ID.ToString(),
			this.type.ToString(),
			this.pos.SaveToString(),
			this.originRoom,
			this.placedObjectIndex,
			Custom.colorToHex(this.color1),
			Custom.colorToHex(this.color2),
			this.dead ? "dead" : "alive"
		);
		text = SaveState.SetCustomData(this, text);
		return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", this.unrecognizedAttributes);
	}
}