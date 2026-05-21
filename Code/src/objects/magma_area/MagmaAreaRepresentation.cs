namespace Floodwaters.Objects;

public class MagmaAreaRepresentation : ResizeableObjectRepresentation {
	public MagmaAreaRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, true) {
		this.subNodes.Add(new MagmaAreaPanel(owner, "MagmaArea_Panel", this, (pObj.data as MagmaAreaData).panelPos));
		this.fSprites.Add(new FSprite("pixel", true) {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
	}

	public override void Refresh() {
		base.Refresh();

		MagmaAreaData data = this.pObj.data as MagmaAreaData;
		MagmaAreaPanel panel = this.subNodes[this.subNodes.Count - 1] as MagmaAreaPanel;
		base.MoveSprite(this.fSprites.Count - 1, this.absPos);
		this.fSprites[this.fSprites.Count - 1].scaleY = panel.pos.magnitude;
		this.fSprites[this.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, panel.nonCollapsedAbsPos);
		data.panelPos = panel.pos;
	}

	public class MagmaAreaPanel : Panel {
		public MagmaAreaPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 145f), "Magma Area") {
			this.subNodes.Add(new MagmaAreaSlider(owner, "Burn_Time_Slider", this, new Vector2(5f, 125f), "Burn Time: "));
			this.subNodes.Add(new MagmaAreaSlider(owner, "ColorA_R_Slider", this, new Vector2(5f, 105f), "Color A R: "));
			this.subNodes.Add(new MagmaAreaSlider(owner, "ColorA_G_Slider", this, new Vector2(5f, 85f), "Color A G: "));
			this.subNodes.Add(new MagmaAreaSlider(owner, "ColorA_B_Slider", this, new Vector2(5f, 65f), "Color A B: "));
			this.subNodes.Add(new MagmaAreaSlider(owner, "ColorB_R_Slider", this, new Vector2(5f, 45f), "Color B R: "));
			this.subNodes.Add(new MagmaAreaSlider(owner, "ColorB_G_Slider", this, new Vector2(5f, 25f), "Color B G: "));
			this.subNodes.Add(new MagmaAreaSlider(owner, "ColorB_B_Slider", this, new Vector2(5f, 5f), "Color B B: "));
		}

		public class MagmaAreaSlider : Slider {
			public MagmaAreaSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();
				MagmaAreaData data = (this.parentNode.parentNode as MagmaAreaRepresentation).pObj.data as MagmaAreaData;

				switch (this.IDstring) {
					case "Burn_Time_Slider": {
						base.NumberText = data.burnTime.ToString();
						base.RefreshNubPos(Mathf.InverseLerp(0.1f, 32f, data.burnTime));
						break;
					}

					case "ColorA_R_Slider": {
						base.NumberText = Mathf.FloorToInt(data.colorA.r * 255f).ToString();
						base.RefreshNubPos(data.colorA.r);
						break;
					}

					case "ColorA_G_Slider": {
						base.NumberText = Mathf.FloorToInt(data.colorA.g * 255f).ToString();
						base.RefreshNubPos(data.colorA.g);
						break;
					}

					case "ColorA_B_Slider": {
						base.NumberText = Mathf.FloorToInt(data.colorA.b * 255f).ToString();
						base.RefreshNubPos(data.colorA.b);
						break;
					}

					case "ColorB_R_Slider": {
						base.NumberText = Mathf.FloorToInt(data.colorB.r * 255f).ToString();
						base.RefreshNubPos(data.colorB.r);
						break;
					}

					case "ColorB_G_Slider": {
						base.NumberText = Mathf.FloorToInt(data.colorB.g * 255f).ToString();
						base.RefreshNubPos(data.colorB.g);
						break;
					}

					case "ColorB_B_Slider": {
						base.NumberText = Mathf.FloorToInt(data.colorB.b * 255f).ToString();
						base.RefreshNubPos(data.colorB.b);
						break;
					}

					default: {
						base.NumberText = "0";
						base.RefreshNubPos(0f);
						break;
					}
				}
			}

			public override void NubDragged(float nubPos) {
				MagmaAreaData data = (this.parentNode.parentNode as MagmaAreaRepresentation).pObj.data as MagmaAreaData;

				switch (this.IDstring) {
					case "Burn_Time_Slider": {
						data.burnTime = Mathf.Lerp(0.1f, 32f, nubPos);
						break;
					}

					case "ColorA_R_Slider": {
						data.colorA.r = nubPos;
						break;
					}

					case "ColorA_G_Slider": {
						data.colorA.g = nubPos;
						break;
					}

					case "ColorA_B_Slider": {
						data.colorA.b = nubPos;
						break;
					}

					case "ColorB_R_Slider": {
						data.colorB.r = nubPos;
						break;
					}

					case "ColorB_G_Slider": {
						data.colorB.g = nubPos;
						break;
					}

					case "ColorB_B_Slider": {
						data.colorB.b = nubPos;
						break;
					}
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}
