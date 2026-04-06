namespace Floodwaters.Objects;

public class RectRepresentation : PlacedObjectRepresentation {
	public PlacedObject.ResizableObjectData Data => this.pObj.data as PlacedObject.ResizableObjectData;

	public RectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.subNodes.Add(new Handle(owner, "Rect_Handle", this, this.Data.handlePos));

		for (int i = 0; i < 5; i++) {
			this.fSprites.Add(new FSprite("pixel"));
			owner.placedObjectsContainer.AddChild(this.fSprites[1 + i]);
			this.fSprites[1 + i].anchorX = 0f;
			this.fSprites[1 + i].anchorY = 0f;
		}

		this.fSprites[5].alpha = 0.05f;
	}

	public override void Refresh() {
		base.Refresh();
		this.MoveSprite(1, this.absPos);
		this.Data.handlePos = (this.subNodes[0] as Handle).pos;
		Vector2 pos = this.absPos;
		Vector2 size = this.Data.handlePos;
		Rect rect = new Rect(pos, size);
		rect.xMax++;
		rect.yMin--;
		rect.yMax--;

		this.MoveSprite(1, new Vector2(rect.xMin, rect.yMin));
		this.fSprites[1].scaleY = rect.height;
		this.MoveSprite(2, new Vector2(rect.xMin, rect.yMin));
		this.fSprites[2].scaleX = rect.width;

		this.MoveSprite(3, new Vector2(rect.xMax, rect.yMin));
		this.fSprites[3].scaleY = rect.height;
		this.MoveSprite(4, new Vector2(rect.xMin, rect.yMax));
		this.fSprites[4].scaleX = rect.width;

		this.MoveSprite(5, new Vector2(rect.xMin, rect.yMin));
		this.fSprites[5].scaleX = rect.width;
		this.fSprites[5].scaleY = rect.height;
	}
}