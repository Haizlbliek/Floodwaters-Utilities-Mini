namespace Floodwaters.Objects;

public class CustomLightArcRepresentation : CustomLightRodRepresentation {
	public new CustomLightArcData Data => this.pObj.data as CustomLightArcData;

	public Handle handleA;
	public Handle handleB;
	public int handleALine;
	public int handleBLine;

	public CustomLightArcRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, bool createCustomArc = true) : base(owner, IDstring, parentNode, pObj, false) {
		this.fLabels[0].text = "CustomLightArc";
		(this.subNodes[this.subNodes.Count - 1] as Panel).Title = "Custom Light Arc";

		CustomLightArc customArc = this.owner.room.updateList.OfType<CustomLightArc>().FirstOrDefault(x => x.pObj == pObj);

		if (customArc == null && createCustomArc) {
			customArc = new CustomLightArc(pObj, owner.room);
			owner.room.AddObject(customArc);
		}

		this.subNodes.Add(this.handleA = new Handle(owner, "HandleA", this, this.Data.handleA));
		this.subNodes.Add(this.handleB = new Handle(owner, "HandleB", this.subNodes[0], this.Data.handleB));

		this.handleALine = this.fSprites.Count;
		this.fSprites.Add(new FSprite("pixel", true) { anchorY = 0f });
		owner.placedObjectsContainer.AddChild(this.fSprites[this.handleALine]);

		this.handleBLine = this.fSprites.Count;
		this.fSprites.Add(new FSprite("pixel", true) { anchorY = 0f });
		owner.placedObjectsContainer.AddChild(this.fSprites[this.handleBLine]);
	}

	public override void Refresh() {
		base.Refresh();

		this.Data.handleA = this.handleA.pos;
		this.Data.handleB = this.handleB.pos;

		this.MoveSprite(this.handleALine, this.absPos);
		this.fSprites[this.handleALine].scaleY = this.handleA.pos.magnitude;
		this.fSprites[this.handleALine].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.handleA.absPos);

		this.MoveSprite(this.handleBLine, (this.subNodes[0] as Handle).absPos);
		this.fSprites[this.handleBLine].scaleY = this.handleB.pos.magnitude;
		this.fSprites[this.handleBLine].rotation = Custom.AimFromOneVectorToAnother((this.subNodes[0] as Handle).absPos, this.handleB.absPos);
	}
}