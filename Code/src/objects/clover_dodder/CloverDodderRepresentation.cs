namespace Floodwaters.Objects;

public class CloverDodderRepresentation : ResizeableObjectRepresentation {
	private CloverDodderData Data => this.pObj.data as CloverDodderData;

	private readonly CloverDodder clover;
	private readonly CloverDodderControlPanel controlPanel;
	private readonly int lineSprite;
	private Vector2 lastPos;

	public CloverDodderRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, true) {
		this.controlPanel = new CloverDodderControlPanel(owner, "CloverDodder_Panel", this, new Vector2(0f, 100f));
		this.subNodes.Add(this.controlPanel);
		this.controlPanel.pos = this.Data.panelPos;
		this.fSprites.Add(new FSprite("pixel", true));
		this.lineSprite = this.fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
		this.fSprites[this.lineSprite].anchorY = 0f;
		this.clover = owner.room.updateList.OfType<CloverDodder>().FirstOrDefault(b => b.pObj == pObj);

		if (this.clover == null) {
			this.clover = new CloverDodder(owner.room, pObj);
			owner.room.AddObject(this.clover);
		}
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

	public class CloverDodderControlPanel : Panel, IDevUISignals {
		public CloverDodderControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 145f), "Clover Dodder") {
			this.UpdateUI();
		}

		private void UpdateUI() {
			for (int num = this.subNodes.Count - 1; num > -1; num--) {
				this.subNodes[num].ClearSprites();
				this.subNodes.Pop();
			}

			CloverDodderData data = (this.parentNode as CloverDodderRepresentation).Data;

			float b = data.colorType == CloverDodderData.ColorType.Custom ? 65f : 5f;
			this.subNodes.Add(new Button(this.owner, "Color_Button", this, new Vector2(5f, b + 60f), 100f, data.colorType.ToString()));
			this.subNodes.Add(new CloverDodderSlider(this.owner, "PrimaryDensity_Slider", this, new Vector2(5f, b + 40f), "Primary Density:"));
			this.subNodes.Add(new CloverDodderSlider(this.owner, "SecondaryDensity_Slider", this, new Vector2(5f, b + 20f), "Secondary Density:"));
			this.subNodes.Add(new CloverDodderSlider(this.owner, "Stickiness_Slider", this, new Vector2(5f, b), "Stickiness:"));
			if (data.colorType == CloverDodderData.ColorType.Custom) {
				this.subNodes.Add(new CloverDodderSlider(this.owner, "Red_Slider", this, new Vector2(5f, 45f), "Red:"));
				this.subNodes.Add(new CloverDodderSlider(this.owner, "Green_Slider", this, new Vector2(5f, 25f), "Green:"));
				this.subNodes.Add(new CloverDodderSlider(this.owner, "Blue_Slider", this, new Vector2(5f, 5f), "Blue:"));
			}
			this.size.y = b + 80f;
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			CloverDodderData CloverDodderData = (this.parentNode as CloverDodderRepresentation).pObj.data as CloverDodderData;
			switch (sender.IDstring) {
				case "Color_Button":
					if ((int)CloverDodderData.colorType >= ExtEnum<CloverDodderData.ColorType>.values.Count - 1) {
						CloverDodderData.colorType = new CloverDodderData.ColorType(ExtEnum<CloverDodderData.ColorType>.values.GetEntry(0));
					}
					else {
						CloverDodderData.colorType = new CloverDodderData.ColorType(ExtEnum<CloverDodderData.ColorType>.values.GetEntry(CloverDodderData.colorType.Index + 1));
					}
					(sender as Button).Text = CloverDodderData.colorType.ToString();
					(this.parentNode as CloverDodderRepresentation).clover.dirtyPalette = true;
					this.UpdateUI();
					break;
			}
		}

		public class CloverDodderSlider : Slider {
			private CloverDodderRepresentation Rep => this.parentNode.parentNode as CloverDodderRepresentation;

			private CloverDodderData Data => this.Rep.pObj.data as CloverDodderData;

			public CloverDodderSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 60f) {
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;
				if (this.IDstring == "PrimaryDensity_Slider") {
					num = this.Data.primaryDensity;
					base.NumberText = num.ToString("F2") + "%";
				}
				else if (this.IDstring == "SecondaryDensity_Slider") {
					num = this.Data.secondaryDensity;
					base.NumberText = num.ToString("F2") + "%";
				}
				else if (this.IDstring == "Stickiness_Slider") {
					num = this.Data.stickiness;
					base.NumberText = num.ToString("F2") + "%";
				}
				else if (this.IDstring == "Red_Slider") {
					num = this.Data.color.r;
					base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
				}
				else if (this.IDstring == "Green_Slider") {
					num = this.Data.color.g;
					base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
				}
				else if (this.IDstring == "Blue_Slider") {
					num = this.Data.color.b;
					base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
				}
				base.RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos) {
				if (this.IDstring == "PrimaryDensity_Slider") {
					this.Data.primaryDensity = nubPos;
					(this.parentNode.parentNode as CloverDodderRepresentation).clover.dirty = true;
				}
				else if (this.IDstring == "SecondaryDensity_Slider") {
					this.Data.secondaryDensity = nubPos;
					(this.parentNode.parentNode as CloverDodderRepresentation).clover.dirty = true;
				}
				else if (this.IDstring == "Stickiness_Slider") {
					this.Data.stickiness = nubPos;
				}
				else if (this.IDstring == "Red_Slider") {
					this.Data.color.r = nubPos;
					(this.parentNode.parentNode as CloverDodderRepresentation).clover.dirtyPalette = true;
				}
				else if (this.IDstring == "Green_Slider") {
					this.Data.color.g = nubPos;
					(this.parentNode.parentNode as CloverDodderRepresentation).clover.dirtyPalette = true;
				}
				else if (this.IDstring == "Blue_Slider") {
					this.Data.color.b = nubPos;
					(this.parentNode.parentNode as CloverDodderRepresentation).clover.dirtyPalette = true;
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}