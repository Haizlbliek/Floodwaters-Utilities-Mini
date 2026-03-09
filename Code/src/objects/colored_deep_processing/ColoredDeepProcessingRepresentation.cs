namespace Floodwaters.Objects;

public class ColoredDeepProcessingRepresentation : QuadObjectRepresentation {
	public ColoredDeepProcessing DP;
	private readonly ColoredDeepProcessingPanel controlPanel;
	private readonly int lineSprite;

	public ColoredDeepProcessingRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.controlPanel = new ColoredDeepProcessingPanel(owner, "Deep_Processing_Panel", this, new Vector2(0f, 100f));
		this.subNodes.Add(this.controlPanel);
		this.controlPanel.pos = (pObj.data as ColoredDeepProcessingData).panelPos;
		this.fSprites.Add(new FSprite("pixel", true));
		this.lineSprite = this.fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
		this.fSprites[this.lineSprite].anchorY = 0f;

		this.DP = owner.room.updateList.OfType<ColoredDeepProcessing>().FirstOrDefault(x => x.pObj == this.pObj);
		if (this.DP == null) {
			this.DP = new ColoredDeepProcessing(pObj, owner.room);
			owner.room.AddObject(this.DP);
		}
	}

	public override void Refresh() {
		base.Refresh();
		base.MoveSprite(this.lineSprite, this.absPos);
		this.fSprites[this.lineSprite].scaleY = this.controlPanel.pos.magnitude;
		this.fSprites[this.lineSprite].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.controlPanel.absPos);
		(this.pObj.data as ColoredDeepProcessingData).panelPos = this.controlPanel.pos;
	}


	public class ColoredDeepProcessingPanel : Panel {
		public ColoredDeepProcessingPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 125f), "Colored Deep Processing") {
			this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Color_R_Slider", this, new Vector2(5f, 105f), "Red: "));
			this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Color_G_Slider", this, new Vector2(5f, 85f), "Green: "));
			this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Color_B_Slider", this, new Vector2(5f, 65f), "Blue: "));
			this.subNodes.Add(new ColoredDeepProcessSlider(owner, "From_Depth_Slider", this, new Vector2(5f, 45f), "From Depth: "));
			this.subNodes.Add(new ColoredDeepProcessSlider(owner, "To_Depth_Slider", this, new Vector2(5f, 25f), "To Depth: "));
			this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Intensity_Slider", this, new Vector2(5f, 5f), "Intensity: "));
		}

		public class ColoredDeepProcessSlider : Slider {
			public ColoredDeepProcessSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();
				ColoredDeepProcessingData data = (this.parentNode.parentNode as ColoredDeepProcessingRepresentation).pObj.data as ColoredDeepProcessingData;

				switch (this.IDstring) {
					case "From_Depth_Slider": {
						base.NumberText = data.fromDepth.ToString();
						base.RefreshNubPos(data.fromDepth / 30f);
						break;
					}

					case "To_Depth_Slider": {
						base.NumberText = data.toDepth.ToString();
						base.RefreshNubPos(data.toDepth / 30f);
						break;
					}

					case "Intensity_Slider": {
						base.NumberText = Mathf.FloorToInt(data.intensity * 100f).ToString();
						base.RefreshNubPos(data.intensity);
						break;
					}

					case "Color_R_Slider": {
						base.NumberText = Mathf.FloorToInt(data.color.r * 255f).ToString();
						base.RefreshNubPos(data.color.r);
						break;
					}

					case "Color_G_Slider": {
						base.NumberText = Mathf.FloorToInt(data.color.g * 255f).ToString();
						base.RefreshNubPos(data.color.g);
						break;
					}

					case "Color_B_Slider": {
						base.NumberText = Mathf.FloorToInt(data.color.b * 255f).ToString();
						base.RefreshNubPos(data.color.b);
						break;
					}
				}
			}

			public override void NubDragged(float nubPos) {
				ColoredDeepProcessingData data = (this.parentNode.parentNode as ColoredDeepProcessingRepresentation).pObj.data as ColoredDeepProcessingData;

				switch (this.IDstring) {
					case "From_Depth_Slider": {
						data.fromDepth = Mathf.Min(Mathf.RoundToInt(nubPos * 30), data.toDepth);
						break;
					}

					case "To_Depth_Slider": {
						data.toDepth = Mathf.Max(Mathf.RoundToInt(nubPos * 30), data.fromDepth);
						break;
					}

					case "Intensity_Slider": {
						data.intensity = nubPos;
						break;
					}

					case "Color_R_Slider": {
						data.color.r = nubPos;
						break;
					}

					case "Color_G_Slider": {
						data.color.g = nubPos;
						break;
					}

					case "Color_B_Slider": {
						data.color.b = nubPos;
						break;
					}
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}