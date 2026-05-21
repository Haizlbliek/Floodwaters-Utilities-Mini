namespace Floodwaters.Objects;

public class DeerSkullRepresentation : ResizeableObjectRepresentation {
	public DeerSkullRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, "Deer Skull", false) {
		this.subNodes.Add(new DeerSkullPanel(owner, "DeerSkull_Panel", this, new Vector2(0f, 100f)) {
			pos = (pObj.data as DeerSkullData).panelPos
		});

		this.fSprites.Add(new FSprite("pixel", true) {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
	}

	public override void Refresh() {
		base.Refresh();

		DeerSkullData data = this.pObj.data as DeerSkullData;
		DeerSkullPanel panel = this.subNodes[this.subNodes.Count - 1] as DeerSkullPanel;

		base.MoveSprite(this.fSprites.Count - 1, this.absPos);
		this.fSprites[this.fSprites.Count - 1].scaleY = panel.pos.magnitude;
		this.fSprites[this.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, panel.absPos);
		data.panelPos = panel.pos;
	}

	public class DeerSkullPanel : Panel {
		public DeerSkullPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 95f), "Deer Skull") {
			this.subNodes.Add(new DeerSkullSlider(owner, "Direction_Slider", this, new Vector2(5f, 65f), "Direction: "));
			this.subNodes.Add(new DeerSkullSlider(owner, "Skull_Seed_Slider", this, new Vector2(5f, 45f), "Skull Seed: "));
			this.subNodes.Add(new DeerSkullSlider(owner, "Paint_Hue_Slider", this, new Vector2(5f, 25f), "Paint Hue: "));
			this.subNodes.Add(new DeerSkullButton(owner, "Has_Paint", this, new Vector2(5f, 5f), "Has Paint: "));
		}

		public class DeerSkullButton : Button {
			public DeerSkullButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string text) : base(owner, IDstring, parentNode, pos, 110f, text) {
			}

			public override void Refresh() {
				base.Refresh();

				DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;
				this.Text = data.hasPaint ? "Has Paint" : "Has No Paint";
			}

			public override void Clicked() {
				base.Clicked();
				
				DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;
				data.hasPaint = !data.hasPaint;
				this.Refresh();
			}
		}

		public class DeerSkullSlider : Slider {
			public DeerSkullSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;
				DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;

				switch (this.IDstring) {
					case "Direction_Slider":
						num = data.direction;
						base.NumberText = ((int) Mathf.Lerp(-100f, 100f, num)).ToString();
						break;

					case "Paint_Hue_Slider":
						num = data.paintHue;
						base.NumberText = Mathf.FloorToInt(data.paintHue * 100f).ToString();
						break;

					case "Skull_Seed_Slider":
						num = data.skullSeed / 100f;
						base.NumberText = data.skullSeed.ToString();
						break;
				}

				base.RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos) {
				DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;
				
				switch (this.IDstring) {
					case "Direction_Slider":
						data.direction = nubPos;
						break;

					case "Paint_Hue_Slider":
						data.paintHue = nubPos;
						break;

					case "Skull_Seed_Slider":
						data.skullSeed = (int) (nubPos * 100f);
						break;
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}