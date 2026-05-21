namespace Floodwaters.Objects;

public class LillypadRepresentation : ResizeableObjectRepresentation {
	public LillypadRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, true) {
		this.subNodes.Add(new LillypadPanel(owner, "Lillypad_Panel", this, (pObj.data as LillypadData).panelPos));
		this.fSprites.Add(new FSprite("pixel", true) {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
	}

	public override void Refresh() {
		base.Refresh();
		this.fSprites[1].scaleY *= 0.5f;

		LillypadData data = this.pObj.data as LillypadData;
		LillypadPanel panel = this.subNodes[this.subNodes.Count - 1] as LillypadPanel;
		base.MoveSprite(this.fSprites.Count - 1, this.absPos);
		this.fSprites[this.fSprites.Count - 1].scaleY = panel.pos.magnitude;
		this.fSprites[this.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, panel.nonCollapsedAbsPos);
		data.panelPos = panel.pos;
	}

	public class LillypadPanel : Panel {
		public LillypadPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 155f), "Lillypad") {
			this.subNodes.Add(new LillypadSlider(owner, "Dark_Slider", this, new Vector2(5f, 125f), "Darkness: "));
			this.subNodes.Add(new LillypadSlider(owner, "Red1_Slider", this, new Vector2(5f, 105f), "Color1 R: "));
			this.subNodes.Add(new LillypadSlider(owner, "Green1_Slider", this, new Vector2(5f, 85f), "Color1 G: "));
			this.subNodes.Add(new LillypadSlider(owner, "Blue1_Slider", this, new Vector2(5f, 65f), "Color1 B: "));
			this.subNodes.Add(new LillypadSlider(owner, "Red2_Slider", this, new Vector2(5f, 45f), "Color2 R: "));
			this.subNodes.Add(new LillypadSlider(owner, "Green2_Slider", this, new Vector2(5f, 25f), "Color2 G: "));
			this.subNodes.Add(new LillypadSlider(owner, "Blue2_Slider", this, new Vector2(5f, 5f), "Color2 B: "));
		}

		public class LillypadSlider : Slider {
			public LillypadSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;
				LillypadData data = (this.parentNode.parentNode as LillypadRepresentation).pObj.data as LillypadData;

				switch (this.IDstring) {
					case "Red1_Slider":
						num = data.colorA.r;
						break;

					case "Green1_Slider":
						num = data.colorA.g;
						break;

					case "Blue1_Slider":
						num = data.colorA.b;
						break;

					case "Red2_Slider":
						num = data.colorB.r;
						break;

					case "Green2_Slider":
						num = data.colorB.g;
						break;

					case "Blue2_Slider":
						num = data.colorB.b;
						break;

					case "Dark_Slider":
						num = data.darkness;
						break;
				}

				base.NumberText = ((int) Mathf.Lerp(0f, 255f, num)).ToString();

				base.RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos) {
				LillypadData data = (this.parentNode.parentNode as LillypadRepresentation).pObj.data as LillypadData;

				switch (this.IDstring) {
					case "Red1_Slider":
						data.colorA.r = nubPos;
						break;

					case "Green1_Slider":
						data.colorA.g = nubPos;
						break;

					case "Blue1_Slider":
						data.colorA.b = nubPos;
						break;

					case "Red2_Slider":
						data.colorB.r = nubPos;
						break;

					case "Green2_Slider":
						data.colorB.g = nubPos;
						break;

					case "Blue2_Slider":
						data.colorB.b = nubPos;
						break;

					case "Dark_Slider":
						data.darkness = nubPos;
						break;
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}