namespace Floodwaters.Objects;

public class CustomLightRodRepresentation : ResizeableObjectRepresentation {
	public CustomLightRodData Data => this.pObj.data as CustomLightRodData;

	public ControlPanel panel;

	public int line;

	public CustomLightRodRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, bool createCustomRod = true) : base(owner, IDstring, parentNode, pObj, "CustomLightRod", false) {
		this.subNodes.Add(this.panel = new ControlPanel(owner, "Custom_Light_Rod_Panel", this, (pObj.data as CustomLightRodData).panelPos));
		this.line = this.fSprites.Count;
		this.fSprites.Add(new FSprite("pixel", true) {
			anchorY = 0f
		});
		this.owner.placedObjectsContainer.AddChild(this.fSprites[this.line]);

		CustomLightRod customRod = this.owner.room.updateList.OfType<CustomLightRod>().FirstOrDefault(x => x.pObj == pObj);

		if (customRod == null && createCustomRod) {
			customRod = new CustomLightRod(pObj, owner.room);
			owner.room.AddObject(customRod);
		}
	}

	public override void Refresh() {
		base.Refresh();
		base.MoveSprite(this.line, this.absPos);
		this.Data.panelPos = this.panel.pos;
		this.fSprites[this.line].scaleY = this.panel.pos.magnitude;
		this.fSprites[this.line].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.panel.absPos);
	}

	public class ControlPanel : Panel {
		public ControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 165f), "Custom Light Rod") {
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "ColorR_Slider", this, new Vector2(5f, 145f), "Color R: "));
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "ColorG_Slider", this, new Vector2(5f, 125f), "Color G: "));
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "ColorB_Slider", this, new Vector2(5f, 105f), "Color B: "));
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "Color2R_Slider", this, new Vector2(5f, 85f), "Color2 R: "));
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "Color2G_Slider", this, new Vector2(5f, 65f), "Color2 G: "));
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "Color2B_Slider", this, new Vector2(5f, 45f), "Color2 B: "));
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "Depth_Slider", this, new Vector2(5f, 25f), "Depth: "));
			this.subNodes.Add(new CustomLightRodControlSlider(owner, "Brightness_Slider", this, new Vector2(5f, 5f), "Brightness: "));
		}

		public class CustomLightRodControlSlider : Slider {
			public CustomLightRodData Data => (this.parentNode.parentNode as CustomLightRodRepresentation).pObj.data as CustomLightRodData;

			public CustomLightRodControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void NubDragged(float nubPos) {
				switch (this.IDstring) {
					case "ColorR_Slider":
						this.Data.color1.r = nubPos;
						break;
					case "ColorG_Slider":
						this.Data.color1.g = nubPos;
						break;
					case "ColorB_Slider":
						this.Data.color1.b = nubPos;
						break;
					case "Color2R_Slider":
						this.Data.color2.r = nubPos;
						break;
					case "Color2G_Slider":
						this.Data.color2.g = nubPos;
						break;
					case "Color2B_Slider":
						this.Data.color2.b = nubPos;
						break;
					case "Depth_Slider":
						this.Data.depth = nubPos;
						break;
					case "Brightness_Slider":
						this.Data.brightness = nubPos;
						break;
				}
				base.NubDragged(nubPos);
			}

			public override void Refresh() {
				base.Refresh();

				float num = 0f;
				switch (this.IDstring) {
					case "ColorR_Slider":
						num = this.Data.color1.r;
						this.NumberText = ((int) (num * 255f)).ToString();
						break;
					case "ColorG_Slider":
						num = this.Data.color1.g;
						this.NumberText = ((int) (num * 255f)).ToString();
						break;
					case "ColorB_Slider":
						num = this.Data.color1.b;
						this.NumberText = ((int) (num * 255f)).ToString();
						break;
					case "Color2R_Slider":
						num = this.Data.color2.r;
						this.NumberText = ((int) (num * 255f)).ToString();
						break;
					case "Color2G_Slider":
						num = this.Data.color2.g;
						this.NumberText = ((int) (num * 255f)).ToString();
						break;
					case "Color2B_Slider":
						num = this.Data.color2.b;
						this.NumberText = ((int) (num * 255f)).ToString();
						break;
					case "Depth_Slider":
						num = this.Data.depth;
						this.NumberText = ((int) (num * 30f)).ToString();
						break;
					case "Brightness_Slider":
						num = this.Data.brightness;
						this.NumberText = ((int) (num * 100f)).ToString() + "%";
						break;
				}

				base.RefreshNubPos(num);
			}
		}
	}
}