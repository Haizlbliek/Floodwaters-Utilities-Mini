namespace Floodwaters.Objects;

public class ColoredLightSource3dRepresentation : PlacedObjectRepresentation {
	public class ColoredLightSource3dPanel : Panel, IDevUISignals {
		public class LightSource3dSlider : Slider {
			protected ColoredLightSource3dData Data => (base.parentNode.parentNode as ColoredLightSource3dRepresentation).pObj.data as ColoredLightSource3dData;
			protected PlacedObject PObj => (base.parentNode.parentNode as ColoredLightSource3dRepresentation).pObj;
			protected ColoredLightSource3d Light => (base.parentNode.parentNode as ColoredLightSource3dRepresentation).light;

			public LightSource3dSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;
				if (this.IDstring == "Strength_Slider") {
					num = this.Data.strength;
					base.NumberText = (int) (num * 100f) + "%";
				}
				if (base.IDstring == "BlinkRate_Slider") {
					num = this.Data.blinkRate;
					base.NumberText = (int) (num * 100f) + "%";
				}
				if (base.IDstring == "Depth_Slider") {
					num = this.Data.depth / 29f;
					base.NumberText = this.Data.depth.ToString();
				}
				if (base.IDstring == "DepthRange_Slider") {
					num = this.Data.depthRange / 29f;
					base.NumberText = this.Data.depthRange.ToString();
				}
				if (base.IDstring == "ColorR_Slider") {
					num = this.Data.color.r;
					base.NumberText = Mathf.RoundToInt(this.Data.color.r * 255f).ToString();
				}
				if (base.IDstring == "ColorG_Slider") {
					num = this.Data.color.g;
					base.NumberText = Mathf.RoundToInt(this.Data.color.g * 255f).ToString();
				}
				if (base.IDstring == "ColorB_Slider") {
					num = this.Data.color.b;
					base.NumberText = Mathf.RoundToInt(this.Data.color.b * 255f).ToString();
				}
				base.RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos) {
				if (base.IDstring == "Strength_Slider") {
					this.Data.strength = nubPos;
				}
				if (base.IDstring == "BlinkRate_Slider") {
					this.Data.blinkRate = nubPos;
					this.Light.blinkRate = nubPos;
				}
				if (base.IDstring == "Depth_Slider") {
					this.Data.depth = Mathf.RoundToInt(nubPos * 29f);
					this.Light.depth = Mathf.RoundToInt(nubPos * 29f);
				}
				if (base.IDstring == "DepthRange_Slider") {
					this.Data.depthRange = Mathf.RoundToInt(nubPos * 29f);
					this.Light.depthRange = Mathf.RoundToInt(nubPos * 29f);
				}
				if (base.IDstring == "ColorR_Slider") {
					this.Data.color.r = nubPos;
				}
				if (base.IDstring == "ColorG_Slider") {
					this.Data.color.g = nubPos;
				}
				if (base.IDstring == "ColorB_Slider") {
					this.Data.color.b = nubPos;
				}
				base.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}

		public ColoredLightSource3dPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 165f), "Colored 3d Light Source") {
			base.subNodes.Add(new LightSource3dSlider(owner, "Depth_Slider", this, new Vector2(5f, 145f), "Depth: "));
			base.subNodes.Add(new LightSource3dSlider(owner, "DepthRange_Slider", this, new Vector2(5f, 125f), "Depth Range: "));
			base.subNodes.Add(new LightSource3dSlider(owner, "Strength_Slider", this, new Vector2(5f, 105f), "Strength: "));
			base.subNodes.Add(new LightSource3dSlider(owner, "BlinkRate_Slider", this, new Vector2(5f, 85f), "Blink Rate: "));
			base.subNodes.Add(new Button(owner, "Fade_With_Sun_Button", this, new Vector2(195f, 65f), 50f, ((parentNode as ColoredLightSource3dRepresentation).pObj.data as ColoredLightSource3dData).fadeWithSun ? "Sun" : "Static"));
			base.subNodes.Add(new Button(owner, "BlinkType_Button", this, new Vector2(5f, 65f), 90f, ((parentNode as ColoredLightSource3dRepresentation).pObj.data as ColoredLightSource3dData).blinkType.ToString()));
			base.subNodes.Add(new Button(owner, "NightLight_Button", this, new Vector2(100f, 65f), 90f, ((parentNode as ColoredLightSource3dRepresentation).pObj.data as ColoredLightSource3dData).nightLight ? "Night Only" : "Always On"));

			base.subNodes.Add(new LightSource3dSlider(owner, "ColorR_Slider", this, new Vector2(5f, 45f), "Red: "));
			base.subNodes.Add(new LightSource3dSlider(owner, "ColorG_Slider", this, new Vector2(5f, 25f), "Green: "));
			base.subNodes.Add(new LightSource3dSlider(owner, "ColorB_Slider", this, new Vector2(5f, 5f), "Blue: "));
		}

		public override void Move(Vector2 newPos) {
			base.Move(newPos);
			base.parentNode.Refresh();
		}

		public override void Refresh() {
			base.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			LightSource3dData data = (base.parentNode as ColoredLightSource3dRepresentation).pObj.data as ColoredLightSource3dData;

			switch (sender.IDstring) {
				case "Fade_With_Sun_Button":
					data.fadeWithSun = !data.fadeWithSun;
					(sender as Button).Text = data.fadeWithSun ? "Sun" : "Static";
					break;
				case "BlinkType_Button": {
					int num;
					num = data.blinkType.Index + 1;
					if (num >= ExtEnum<PlacedObject.LightSourceData.BlinkType>.values.Count) {
						num = 0;
					}
					data.blinkType = new PlacedObject.LightSourceData.BlinkType(ExtEnum<PlacedObject.LightSourceData.BlinkType>.values.GetEntry(num));
					(base.parentNode as ColoredLightSource3dRepresentation).light.blinkType = data.blinkType;
					(sender as Button).Text = data.blinkType.ToString();
					break;
				}
				case "NightLight_Button":
					data.nightLight = !data.nightLight;
					(sender as Button).Text = (!data.nightLight) ? "Always On" : "Night Only";
					break;
			}
		}
	}

	public ColoredLightSource3d light;

	public ColoredLightSource3dRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name) {
		base.subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(base.subNodes[base.subNodes.Count - 1] as Handle).pos = (pObj.data as LightSource3dData).handlePos;
		base.fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(base.fSprites[1]);
		base.fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		base.fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(base.fSprites[2]);
		base.fSprites[2].anchorY = 0f;
		base.fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(base.fSprites[3]);
		base.fSprites[3].anchorY = 0f;
		base.subNodes.Add(new ColoredLightSource3dPanel(owner, "Colored_Light_Control_Panel", this, new Vector2(0f, 100f)));
		(base.subNodes[base.subNodes.Count - 1] as ColoredLightSource3dPanel).pos = (pObj.data as LightSource3dData).panelPos;
		this.light = owner.room.updateList.OfType<ColoredLightSource3d>().FirstOrDefault(x => x.Pos == pObj.pos);
		if (this.light == null) {
			ColoredLightSource3dData data = base.pObj.data as ColoredLightSource3dData;
			this.light = new ColoredLightSource3d(base.pos, environmentalLight: true, data.color, null, pObj) {
				depth = data.depth,
				depthRange = data.depthRange,
			};
			owner.room.AddObject(this.light);
		}
	}

	public override void Refresh() {
		base.Refresh();
		base.MoveSprite(1, this.absPos);
		base.fSprites[1].scale = (base.subNodes[0] as Handle).pos.magnitude / 8f;
		base.fSprites[1].alpha = 2f / (base.subNodes[0] as Handle).pos.magnitude;
		base.MoveSprite(2, this.absPos);
		base.fSprites[2].scaleY = (base.subNodes[0] as Handle).pos.magnitude;
		base.fSprites[2].rotation = Custom.AimFromOneVectorToAnother(this.absPos, (base.subNodes[0] as Handle).absPos);
		base.MoveSprite(3, this.absPos);
		base.fSprites[3].scaleY = (base.subNodes[1] as ColoredLightSource3dPanel).pos.magnitude;
		base.fSprites[3].rotation = Custom.AimFromOneVectorToAnother(this.absPos, (base.subNodes[1] as ColoredLightSource3dPanel).absPos);
		(base.pObj.data as LightSource3dData).handlePos = (base.subNodes[0] as Handle).pos;
		(base.pObj.data as LightSource3dData).panelPos = (base.subNodes[1] as Panel).pos;
		this.light.setPos = base.pObj.pos;
		this.light.setRad = (base.pObj.data as LightSource3dData).Rad;
		this.light.setAlpha = (base.pObj.data as LightSource3dData).strength;
		this.light.fadeWithSun = (base.pObj.data as LightSource3dData).fadeWithSun;
		this.light.colorFromEnvironment = (base.pObj.data as LightSource3dData).colorType == LightSource3dData.ColorType.Environment;
		this.light.depth = (base.pObj.data as LightSource3dData).depth;
		this.light.depthRange = (base.pObj.data as LightSource3dData).depthRange;
		this.light.effectColor = Math.Max(-1, (int) (base.pObj.data as LightSource3dData).colorType - 2);
	}
}
