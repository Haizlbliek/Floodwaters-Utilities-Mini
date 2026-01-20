namespace Floodwaters.Objects;

public class CustomVineConnectorData : PlacedObject.ResizableObjectData {
	public bool onePerVine = true;
	public Vector2 panelPos = new Vector2(0, 100);

	public CustomVineConnectorData(PlacedObject owner) : base(owner) {
	}

	public override void FromString(string s) {
		base.FromString(s);

		try {
			string[] array = Regex.Split(s, "~");
			this.onePerVine = bool.Parse(array[2]);
			this.panelPos.x = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		} catch (Exception) {}
	}

	public override string ToString() {
		return base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}", this.onePerVine, this.panelPos.x, this.panelPos.y);
	}
}

public class CustomVineConnectorRepresentation : ResizeableObjectRepresentation {
	public ControlPanel controlPanel;
	public int controlPanelLine;

	public CustomVineConnectorData Data => this.pObj.data as CustomVineConnectorData;

	public CustomVineConnectorRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, true) {
		this.controlPanel = new ControlPanel(owner, "Custom_Vine_Connector_Control_Panel", this, this.Data.panelPos);
		this.subNodes.Add(this.controlPanel);
		this.controlPanelLine = this.fSprites.Count;
		this.fSprites.Add(new FSprite("pixel", true) {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[this.controlPanelLine]);
	}

	public override void Refresh() {
		base.Refresh();

		base.MoveSprite(this.controlPanelLine, this.absPos);
		this.fSprites[this.controlPanelLine].scaleY = this.controlPanel.pos.magnitude;
		this.fSprites[this.controlPanelLine].rotation = Custom.VecToDeg(this.controlPanel.pos);
		this.Data.panelPos = this.controlPanel.pos;
	}

	public class ControlPanel : Panel, IDevUISignals {
		public CustomVineConnectorData Data => (this.parentNode as CustomVineConnectorRepresentation).Data;

		public ControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "Custom Vine Connector") {
			this.subNodes.Add(new DevUILabel(owner, "PerVine_Label", this, new Vector2(5f, 5f), 110f, "Points per vine: "));
			this.subNodes.Add(new Button(owner, "PerVine_Button", this, new Vector2(115f, 5f), 125f, this.Data.onePerVine ? "One" : "All"));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			if (type != DevUISignalType.ButtonClick) {
				return;
			}

			if (sender.IDstring == "PerVine_Button") {
				this.Data.onePerVine = !this.Data.onePerVine;
				(sender as Button).Text = this.Data.onePerVine ? "One" : "All";
			}
		}
	}
}
