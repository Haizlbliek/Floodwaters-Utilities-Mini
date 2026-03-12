namespace Floodwaters.Objects;

public class VerticalGateRepresentation : PlacedObjectRepresentation {
	public VerticalGateRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.fSprites.Add(new FSprite("pixel") {
			anchorX = 0.5f,
			anchorY = 0.5f,
			alpha = 0.25f,
			scaleX = 20f * 11f,
			scaleY = 20f * 4f
		});
		this.fSprites.Add(new FSprite("pixel") {
			anchorX = 0.5f,
			anchorY = 0.5f,
			alpha = 0.25f,
			scaleX = 20f * 11f,
			scaleY = 20f * 4f
		});
		this.fSprites.Add(new FSprite("pixel") {
			anchorX = 0.5f,
			anchorY = 0.5f,
			alpha = 0.25f,
			scaleX = 20f * 11f,
			scaleY = 20f * 4f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[1]);
		owner.placedObjectsContainer.AddChild(this.fSprites[2]);
		owner.placedObjectsContainer.AddChild(this.fSprites[3]);
	}

	public override void Refresh() {
		base.Refresh();
		this.MoveSprite(1, new Vector2(
			Mathf.FloorToInt(this.absPos.x / 20f) * 20f + 10f,
			Mathf.FloorToInt(this.absPos.y / 20f) * 20f + 10f
		));
		this.MoveSprite(2, new Vector2(
			Mathf.FloorToInt(this.absPos.x / 20f) * 20f + 10f,
			Mathf.FloorToInt(this.absPos.y / 20f + 9f) * 20f + 10f
		));
		this.MoveSprite(3, new Vector2(
			Mathf.FloorToInt(this.absPos.x / 20f) * 20f + 10f,
			Mathf.FloorToInt(this.absPos.y / 20f - 9f) * 20f + 10f
		));
	}
}