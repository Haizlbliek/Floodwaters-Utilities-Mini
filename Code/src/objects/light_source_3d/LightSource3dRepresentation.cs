namespace Floodwaters.Objects;

public class LightSource3dRepresentation : PlacedObjectRepresentation {
	public class LightSource3dPanel : Panel, IDevUISignals {
		public class LightSource3dSlider : Slider {
			public LightSource3dSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;
				if (this.IDstring == "Strength_Slider") {
					num = ((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).strength;
					base.NumberText = (int) (num * 100f) + "%";
				}
				if (base.IDstring == "BlinkRate_Slider") {
					num = ((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).blinkRate;
					base.NumberText = (int) (num * 100f) + "%";
				}
				if (base.IDstring == "Depth_Slider") {
					num = ((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).depth / 29f;
					base.NumberText = ((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).depth.ToString();
				}
				if (base.IDstring == "DepthRange_Slider") {
					num = ((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).depthRange / 29f;
					base.NumberText = ((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).depthRange.ToString();
				}
				base.RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos) {
				if (base.IDstring == "Strength_Slider") {
					((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).strength = nubPos;
				}
				if (base.IDstring == "BlinkRate_Slider") {
					((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).blinkRate = nubPos;
					(base.parentNode.parentNode as LightSource3dRepresentation).light.blinkRate = nubPos;
				}
				if (base.IDstring == "Depth_Slider") {
					((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).depth = Mathf.RoundToInt(nubPos * 29f);
					(base.parentNode.parentNode as LightSource3dRepresentation).light.depth = Mathf.RoundToInt(nubPos * 29f);
				}
				if (base.IDstring == "DepthRange_Slider") {
					((base.parentNode.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).depthRange = Mathf.RoundToInt(nubPos * 29f);
					(base.parentNode.parentNode as LightSource3dRepresentation).light.depthRange = Mathf.RoundToInt(nubPos * 29f);
				}
				base.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}

		public LightSource3dPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 125f), "3d Light Source") {
			base.subNodes.Add(new LightSource3dSlider(owner, "Depth_Slider", this, new Vector2(5f, 105f), "Depth: "));
			base.subNodes.Add(new LightSource3dSlider(owner, "DepthRange_Slider", this, new Vector2(5f, 85f), "Depth Range: "));
			base.subNodes.Add(new LightSource3dSlider(owner, "Strength_Slider", this, new Vector2(5f, 65f), "Strength: "));
			base.subNodes.Add(new Button(owner, "Color_Button", this, new Vector2(5f, 45f), 100f, ((parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).colorType.ToString()));
			base.subNodes.Add(new Button(owner, "Fade_With_Sun_Button", this, new Vector2(125f, 45f), 50f, ((parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).fadeWithSun ? "Sun" : "Static"));
			base.subNodes.Add(new LightSource3dSlider(owner, "BlinkRate_Slider", this, new Vector2(5f, 25f), "Blink Rate: "));
			base.subNodes.Add(new Button(owner, "BlinkType_Button", this, new Vector2(5f, 5f), 100f, ((parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).blinkType.ToString()));
			base.subNodes.Add(new Button(owner, "NightLight_Button", this, new Vector2(125f, 5f), 100f, ((parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData).nightLight ? "Night Only" : "Always On"));
		}

		public override void Move(Vector2 newPos) {
			base.Move(newPos);
			base.parentNode.Refresh();
		}

		public override void Refresh() {
			base.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			LightSource3dData data = (base.parentNode as LightSource3dRepresentation).pObj.data as LightSource3dData;

			switch (sender.IDstring) {
				case "Color_Button":
					data.colorType = (int) data.colorType >= ExtEnum<PlacedObject.LightSourceData.ColorType>.values.Count - 1
						? new PlacedObject.LightSourceData.ColorType(ExtEnum<PlacedObject.LightSourceData.ColorType>.values.GetEntry(0))
						: new PlacedObject.LightSourceData.ColorType(ExtEnum<PlacedObject.LightSourceData.ColorType>.values.GetEntry(data.colorType.Index + 1));
					(sender as Button).Text = data.colorType.ToString();
					(base.parentNode as LightSource3dRepresentation).light.color = new Color(1f, 1f, 1f);
					break;
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
					(base.parentNode as LightSource3dRepresentation).light.blinkType = data.blinkType;
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

	public LightSource3d light;

	public LightSource3dRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
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
		base.subNodes.Add(new LightSource3dPanel(owner, "Light_Control_Panel", this, new Vector2(0f, 100f)));
		(base.subNodes[base.subNodes.Count - 1] as LightSource3dPanel).pos = (pObj.data as LightSource3dData).panelPos;
		this.light = owner.room.updateList.OfType<LightSource3d>().FirstOrDefault(x => x.Pos == pObj.pos);
		if (this.light == null) {
			this.light = new LightSource3d(base.pos, environmentalLight: true, new Color(1f, 1f, 1f), null);
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
		base.fSprites[3].scaleY = (base.subNodes[1] as LightSource3dPanel).pos.magnitude;
		base.fSprites[3].rotation = Custom.AimFromOneVectorToAnother(this.absPos, (base.subNodes[1] as LightSource3dPanel).absPos);
		(base.pObj.data as LightSource3dData).handlePos = (base.subNodes[0] as Handle).pos;
		(base.pObj.data as LightSource3dData).panelPos = (base.subNodes[1] as Panel).pos;
		this.light.setPos = base.pObj.pos;
		this.light.setRad = (base.pObj.data as LightSource3dData).Rad;
		this.light.setAlpha = (base.pObj.data as LightSource3dData).strength;
		this.light.fadeWithSun = (base.pObj.data as LightSource3dData).fadeWithSun;
		this.light.colorFromEnvironment = (base.pObj.data as LightSource3dData).colorType == LightSource3dData.ColorType.Environment;
		this.light.depth = (base.pObj.data as LightSource3dData).depth;
		this.light.effectColor = Math.Max(-1, (int) (base.pObj.data as LightSource3dData).colorType - 2);
	}
}
