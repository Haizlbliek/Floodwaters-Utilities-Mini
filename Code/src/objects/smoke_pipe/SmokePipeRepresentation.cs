namespace Floodwaters.Objects;

public class SmokePipeRepresentation : ResizeableObjectRepresentation {
	public SmokePipe smokePipe;

	public SmokePipePanel panel;

	public SmokePipeData Data => this.pObj.data as SmokePipeData;

	public SmokePipeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, false) {
		this.smokePipe = owner.room.updateList.OfType<SmokePipe>().FirstOrDefault(p => p.pObj == pObj);
		if (this.smokePipe == null) {
			this.smokePipe = new SmokePipe(owner.room, pObj);
			this.owner.room.AddObject(this.smokePipe);
		}

		this.subNodes.Add(this.panel = new SmokePipePanel(owner, "SmokePipe_Panel", this, this.Data.panelPos));
		this.fSprites.Add(new FSprite("pixel") {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
	}

	public override void Refresh() {
		base.Refresh();
		this.Data.panelPos = this.panel.pos;

		base.MoveSprite(base.fSprites.Count - 1, this.absPos);
		base.fSprites[base.fSprites.Count - 1].scaleY = (this.Data.panelPos + (this.panel.collapsed ? (Vector2.up * this.panel.size.y) : Vector2.zero)).magnitude;
		base.fSprites[base.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(Vector2.zero, this.Data.panelPos + (this.panel.collapsed ? (Vector2.up * this.panel.size.y) : Vector2.zero));
	}

	public class SmokePipePanel : Panel, IDevUISignals {
		public SmokePipeRepresentation Parent => this.parentNode as SmokePipeRepresentation;

		public SmokePipePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "Smoke Pipe") {
			this.subNodes.Add(new Button(this.owner, "Toggle_Mode", this, new Vector2(5f, 5f), 240f, this.ModeText));
		}

		public string ModeText => this.Parent.Data.eoc ? "Mode: EoC only" : "Mode: Always";

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			if (sender.IDstring == "Toggle_Mode") {
				this.Parent.Data.eoc = !this.Parent.Data.eoc;
				(sender as Button).Text = this.ModeText;
			}
		}
	}
}