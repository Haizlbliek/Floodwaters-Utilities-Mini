namespace Floodwaters.Objects;

public class ExitKeyRepresentation : ResizeableObjectRepresentation {
	private ExitKeyData Data => this.pObj.data as ExitKeyData;

	private readonly ExitKeyControlPanel controlPanel;
	private readonly int lineSprite;
	private Vector2 lastPos;

	public ExitKeyRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, false) {
		this.controlPanel = new ExitKeyControlPanel(owner, "ExitKey_Panel", this, this.Data.panelPos);
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

	public class ExitKeyControlPanel : Panel, IDevUISignals {
		public ExitKeyData Data => (this.parentNode as ExitKeyRepresentation).Data;

		private string SaveSpecificText => this.Data.saveSpecific ? "Save-Specific" : "Region-Specific";

		public ExitKeyControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 105f), "Exit Key") {
			this.subNodes.Add(new DevUILabel(this.owner, "Announcement", this, new Vector2(5f, 85f), 240f, "Announcement:"));
			this.subNodes.Add(new TextInput(this.owner, "Announcement_TextInput", this, new Vector2(5f, 65f), 240f, this.Data.announcement));
			this.subNodes.Add(new DevUILabel(this.owner, "KeyName", this, new Vector2(5f, 45f), 240f, "Key Name:"));
			this.subNodes.Add(new TextInput(this.owner, "KeyName_TextInput", this, new Vector2(5f, 25f), 240f, this.Data.key));
			this.subNodes.Add(new Button(this.owner, "SaveSpecific_Button", this, new Vector2(5f, 5f), 190f, this.SaveSpecificText));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			if (type == Enums.Edited) {
				if (sender.IDstring == "Announcement_TextInput")
					this.Data.announcement = message;

				if (sender.IDstring == "KeyName_TextInput")
					this.Data.key = message;
			}
			else if (type == DevUISignalType.ButtonClick) {
				if (sender.IDstring == "SaveSpecific_Button") {
					this.Data.saveSpecific = !this.Data.saveSpecific;
					(sender as Button).Text = this.SaveSpecificText;
				}
			}
		}
	}
}