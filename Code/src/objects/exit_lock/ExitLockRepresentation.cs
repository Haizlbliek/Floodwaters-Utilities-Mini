namespace Floodwaters.Objects;

public class ExitLockRepresentation : TileObjectRepresentation {
	private ExitLockData Data => this.pObj.data as ExitLockData;

	private readonly ExitLockControlPanel controlPanel;
	private readonly int lineSprite;
	private Vector2 lastPos;

	public ExitLockRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.controlPanel = new ExitLockControlPanel(owner, "ExitLock_Panel", this, this.Data.panelPos);
		this.subNodes.Add(this.controlPanel);
		this.fSprites.Add(new FSprite("pixel", true));
		this.lineSprite = this.fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
		this.fSprites[this.lineSprite].anchorY = 0f;
	}

	public override void Refresh() {
		base.Refresh();
		base.MoveSprite(this.lineSprite, this.absPos);
		this.fSprites[this.lineSprite].scaleY = this.controlPanel.pos.magnitude;
		this.fSprites[this.lineSprite].rotation = Custom.VecToDeg(this.controlPanel.pos);
		this.Data.panelPos = this.controlPanel.pos;
		if (this.pObj.pos != this.lastPos) {
			this.lastPos = this.pObj.pos;
		}
	}

	public class ExitLockControlPanel : Panel, IDevUISignals {
		public ExitLockData Data => (this.parentNode as ExitLockRepresentation).Data;

		private string IgnoreSaveSpecificText => this.Data.ignoreSaveSpecific ? "Save specific: Ignored" : "Save specific: Listening";
		private string IgnoreRegionSpecificText => this.Data.ignoreRegionSpecific ? "Region specific: Ignored" : "Region specific: Listening";

		public ExitLockControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 85f), "Exit Lock") {
			this.subNodes.Add(new DevUILabel(this.owner, "KeyName", this, new Vector2(5f, 65f), 240f, "Key Name:"));
			this.subNodes.Add(new TextInput(this.owner, "KeyName_TextInput", this, new Vector2(5f, 45f), 240f, this.Data.key));
			this.subNodes.Add(new Button(this.owner, "IgnoreSaveSpecific_Button", this, new Vector2(5f, 25f), 190f, this.IgnoreSaveSpecificText));
			this.subNodes.Add(new Button(this.owner, "IgnoreRegionSpecific_Button", this, new Vector2(5f, 5f), 190f, this.IgnoreRegionSpecificText));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			if (type == Enums.Edited) {
				if (sender.IDstring == "KeyName_TextInput")
					this.Data.key = message;
			}
			else if (type == DevUISignalType.ButtonClick) {
				if (sender.IDstring == "IgnoreSaveSpecific_Button") {
					this.Data.ignoreSaveSpecific = !this.Data.ignoreSaveSpecific;
					(sender as Button).Text = this.IgnoreSaveSpecificText;
				}
				else if (sender.IDstring == "IgnoreRegionSpecific_Button") {
					this.Data.ignoreRegionSpecific = !this.Data.ignoreRegionSpecific;
					(sender as Button).Text = this.IgnoreRegionSpecificText;
				}
			}
		}
	}
}