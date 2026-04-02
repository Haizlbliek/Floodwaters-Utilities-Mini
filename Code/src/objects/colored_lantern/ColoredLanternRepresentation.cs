namespace Floodwaters.Objects;

public class ColoredLanternRepresentaion : PlacedObjectRepresentation {
	public class ColoredLanternControlPanel : Panel, IDevUISignals {
		public class ColorSlider : Slider {
			public ColorSlider(DevUI owner, string IDString, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDString, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();

				ColoredLanternData data = (this.parentNode.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternData;

				float from = -1.0f;

				if (this.IDstring == null)
					return;

				switch (this.IDstring) {
					case "color1r":
						from = data.color1.r;
						break;

					case "color1g":
						from = data.color1.g;
						break;

					case "color1b":
						from = data.color1.b;
						break;

					case "color2r":
						from = data.color2.r;
						break;

					case "color2g":
						from = data.color2.g;
						break;

					case "color2b":
						from = data.color2.b;
						break;
				}

				if (from >= 0.0f) {
					base.NumberText = ((int) (from * 255f)).ToString();
					base.RefreshNubPos(from);
				}
			}

			public override void NubDragged(float nubPos) {
				bool set = false;

				ColoredLanternData data = (this.parentNode.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternData;

				if (this.IDstring == null)
					return;

				switch (this.IDstring) {
					case "color1r":
						set = true;
						data.color1.r = nubPos;
						break;

					case "color1g":
						set = true;
						data.color1.g = nubPos;
						break;

					case "color1b":
						set = true;
						data.color1.b = nubPos;
						break;

					case "color2r":
						set = true;
						data.color2.r = nubPos;
						break;

					case "color2g":
						set = true;
						data.color2.g = nubPos;
						break;

					case "color2b":
						set = true;
						data.color2.b = nubPos;
						break;
				}

				if (set) {
					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
				else {
					base.NubDragged(nubPos);
				}
			}
		}

		public readonly Button typeButton;

		public ColoredLanternControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
		: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), name) {
			this.size.y += 20f * 5f;
			this.typeButton = new Button(owner, "type", this, new Vector2(5f, 125f), 240f, "TYPE");

			this.subNodes.Add(this.typeButton);
			this.subNodes.Add(new ColorSlider(owner, "color1r", this, new Vector2(5f, 105f), "Color 1 r: "));
			this.subNodes.Add(new ColorSlider(owner, "color1g", this, new Vector2(5f, 85f), "Color 1 g: "));
			this.subNodes.Add(new ColorSlider(owner, "color1b", this, new Vector2(5f, 65f), "Color 1 b: "));
			this.subNodes.Add(new ColorSlider(owner, "color2r", this, new Vector2(5f, 45f), "Color 2 r: "));
			this.subNodes.Add(new ColorSlider(owner, "color2g", this, new Vector2(5f, 25f), "Color 2 g: "));
			this.subNodes.Add(new ColorSlider(owner, "color2b", this, new Vector2(5f, 5f), "Color 2 b: "));
		}

		public override void Refresh() {
			base.Refresh();

			if (((this.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternData).dead) {
				this.typeButton.Text = "Dead";
			}
			else {
				this.typeButton.Text = "Normal";
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			ColoredLanternData data = (this.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternData;

			string idstring = sender.IDstring;
			if (idstring != null) {
				if (idstring == "type")
					data.dead = !data.dead;
			}
			this.Refresh();
		}
	}

	public ColoredLanternData Data => this.pObj.data as ColoredLanternData;
	public readonly Handle stickEndHandle;
	public readonly ColoredLanternControlPanel controlPanel;

	public readonly ColoredLanternStick lanternStick;

	public ColoredLanternRepresentaion(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.controlPanel = new ColoredLanternControlPanel(owner, "Panel", this, new Vector2(0f, 100f), pObj.type.ToString());
		this.subNodes.Add(this.controlPanel);
		this.controlPanel.pos = this.Data.panelPos;

		this.fSprites.Add(new FSprite("pixel", true));
		owner.placedObjectsContainer.AddChild(this.fSprites[1]);
		this.fSprites[1].anchorY = 0f;

		if (this.Data.stickEnd != null) {
			this.fSprites.Add(new FSprite("pixel", true));
			owner.placedObjectsContainer.AddChild(this.fSprites[2]);
			this.fSprites[2].anchorY = 0f;

			this.stickEndHandle = new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f));
			this.subNodes.Add(this.stickEndHandle);
			this.stickEndHandle.pos = this.Data.stickEnd.Value;

			this.lanternStick = this.owner.room.updateList.OfType<ColoredLanternStick>().FirstOrDefault(l => l.po == pObj);
			if (this.lanternStick == null) {
				owner.room.AddObject(this.lanternStick = new ColoredLanternStick(owner.room, base.pObj, owner.room.abstractRoom.index, owner.room.roomSettings.placedObjects.IndexOf(pObj)));
			}
		}
	}

	public override void Refresh() {
		base.Refresh();

		base.MoveSprite(1, this.absPos);
		this.fSprites[1].scaleY = this.controlPanel.pos.magnitude;
		this.fSprites[1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.controlPanel.absPos);
		this.Data.panelPos = this.controlPanel.pos;

		if (this.stickEndHandle != null) {
			base.MoveSprite(2, this.absPos);
			this.fSprites[2].scaleY = this.stickEndHandle.pos.magnitude;
			this.fSprites[2].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.stickEndHandle.absPos);
			this.Data.stickEnd = this.stickEndHandle.pos;
		}

		this.lanternStick?.Refresh();
	}
}