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
		public CloverDodderControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 165f), "Clover Dodder") {
			this.UpdateUI();
		}

		private void UpdateUI() {
			for (int num = this.subNodes.Count - 1; num > -1; num--) {
				this.subNodes[num].ClearSprites();
				this.subNodes.Pop();
			}

			CloverDodderData data = (this.parentNode as CloverDodderRepresentation).Data;
			float y = 5f;
			float x = 190f;

			this.subNodes.Add(new Button(this.owner, "LanternfruitColor_Button", this, new Vector2(5f, y), 100f, data.lanternfruitColorType.ToString()));
			y += 20f;
			if (data.lanternfruitColorType == CloverDodderData.LanternfruitColorType.Custom) {
				this.subNodes.Add(new CloverDodderSlider(this.owner, "LanternfruitRed_Slider", this, new Vector2(5f, y), "Red:"));
				this.subNodes.Add(new CloverDodderSlider(this.owner, "LanternfruitGreen_Slider", this, new Vector2(5f, y + 20f), "Green:"));
				this.subNodes.Add(new CloverDodderSlider(this.owner, "LanternfruitBlue_Slider", this, new Vector2(5f, y + 40f), "Blue:"));
				y += 60f;
			}
			this.subNodes.Add(new CloverDodderSlider(this.owner, "LanternfruitChance_Slider", this, new Vector2(5f, y), "Lanternfruit:"));
			y += 25f;
			this.subNodes.Add(new CloverDodderSlider(this.owner, "Volume_Slider", this, new Vector2(5f, y), "Volume:"));
			y += 20f;
			this.subNodes.Add(new CloverDodderSlider(this.owner, "Stickiness_Slider", this, new Vector2(5f, y), "Stickiness:"));
			y += 20f;
			this.subNodes.Add(new CloverDodderSlider(this.owner, "SecondaryDensity_Slider", this, new Vector2(5f, y), "Secondary Density:"));
			y += 20f;
			this.subNodes.Add(new CloverDodderSlider(this.owner, "PrimaryDensity_Slider", this, new Vector2(5f, y), "Primary Density:"));
			y += 20f;
			this.subNodes.Add(new Button(this.owner, "Color_Button", this, new Vector2(5f, y), 100f, data.colorType.ToString()));
			y += 20f;
			if (data.colorType == CloverDodderData.ColorType.Custom) {
				this.subNodes.Add(new CloverDodderSlider(this.owner, "Red_Slider", this, new Vector2(5f, y), "Red:"));
				this.subNodes.Add(new CloverDodderSlider(this.owner, "Green_Slider", this, new Vector2(5f, y + 20f), "Green:"));
				this.subNodes.Add(new CloverDodderSlider(this.owner, "Blue_Slider", this, new Vector2(5f, y + 40f), "Blue:"));
				y += 60f;
			}
			this.size.y = y + 5f;
			this.size.x = x + 10;
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			CloverDodderData CloverDodderData = (this.parentNode as CloverDodderRepresentation).pObj.data as CloverDodderData;
			switch (sender.IDstring) {
				case "LanternfruitColor_Button":
					if ((int)CloverDodderData.lanternfruitColorType >= ExtEnum<CloverDodderData.LanternfruitColorType>.values.Count - 1) {
						CloverDodderData.lanternfruitColorType = new CloverDodderData.LanternfruitColorType(ExtEnum<CloverDodderData.LanternfruitColorType>.values.GetEntry(0));
					}
					else {
						CloverDodderData.lanternfruitColorType = new CloverDodderData.LanternfruitColorType(ExtEnum<CloverDodderData.LanternfruitColorType>.values.GetEntry(CloverDodderData.lanternfruitColorType.Index + 1));
					}
					this.UpdateUI();
					break;
				
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
					base.NumberText = (num * 100f).ToString("F2") + "%";
				}
				else if (this.IDstring == "SecondaryDensity_Slider") {
					num = this.Data.secondaryDensity;
					base.NumberText = (num * 100f).ToString("F2") + "%";
				}
				else if (this.IDstring == "Stickiness_Slider") {
					num = this.Data.stickiness;
					base.NumberText = (num * 100f).ToString("F2") + "%";
				}
				else if (this.IDstring == "Volume_Slider") {
					num = this.Data.volume;
					base.NumberText = (num * 100f).ToString("F2") + "%";
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
				else if (this.IDstring == "LanternfruitRed_Slider") {
					num = this.Data.lanternfruitColor.r;
					base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
				}
				else if (this.IDstring == "LanternfruitGreen_Slider") {
					num = this.Data.lanternfruitColor.g;
					base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
				}
				else if (this.IDstring == "LanternfruitBlue_Slider") {
					num = this.Data.lanternfruitColor.b;
					base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
				}
				else if (this.IDstring == "LanternfruitChance_Slider") {
					num = this.Data.lanternfruitChance;
					base.NumberText = Mathf.RoundToInt(num * 100f).ToString() + "%";
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
				else if (this.IDstring == "Volume_Slider") {
					this.Data.volume = nubPos;
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
				else if (this.IDstring == "LanternfruitRed_Slider") {
					this.Data.lanternfruitColor.r = nubPos;
				}
				else if (this.IDstring == "LanternfruitGreen_Slider") {
					this.Data.lanternfruitColor.g = nubPos;
				}
				else if (this.IDstring == "LanternfruitBlue_Slider") {
					this.Data.lanternfruitColor.b = nubPos;
				}

				else if (this.IDstring == "LanternfruitChance_Slider") {
					this.Data.lanternfruitChance = nubPos;
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}