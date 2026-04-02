namespace Floodwaters.Objects;

public class SandDripRepresentation : PlacedObjectRepresentation {
	private SandDripData Data {
		get {
			return this.pObj.data as SandDripData;
		}
	}

	public SandDripRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.controlPanel = new SandDripControlPanel(owner, "Sand_Drips_Panel", this, new Vector2(0f, 100f));
		this.subNodes.Add(this.controlPanel);
		this.controlPanel.pos = this.Data.panelPos;
		this.fSprites.Add(new FSprite("pixel", true));
		this.lineSprite = this.fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
		this.fSprites[this.lineSprite].anchorY = 0f;
		this.drip = owner.room.updateList.OfType<SandDrip>().FirstOrDefault(obj => obj.pObj == pObj);

		if (this.drip == null) {
			this.drip = new SandDrip(owner.room, pObj);
			owner.room.AddObject(this.drip);
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

	private readonly SandDrip drip;

	private readonly SandDripControlPanel controlPanel;

	private readonly int lineSprite;

	private Vector2 lastPos;

	public class SandDripControlPanel : Panel {
		public SandDripControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 85f), "Sand Drip") {
			this.subNodes.Add(new SandDripSlider(owner, "Red_Slider", this, new Vector2(5f, 65f), "Red: "));
			this.subNodes.Add(new SandDripSlider(owner, "Green_Slider", this, new Vector2(5f, 45f), "Green: "));
			this.subNodes.Add(new SandDripSlider(owner, "Blue_Slider", this, new Vector2(5f, 25f), "Blue: "));
			this.subNodes.Add(new SandDripSlider(owner, "Pile_Slider", this, new Vector2(5f, 5f), "Pile Size: "));
		}

		public class SandDripSlider : Slider {
			private SandDripRepresentation Rep {
				get {
					return this.parentNode.parentNode as SandDripRepresentation;
				}
			}

			private SandDripData Data {
				get {
					return this.Rep.pObj.data as SandDripData;
				}
			}

			public SandDripSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 60f) {
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;
				switch (this.IDstring) {
					case "Red_Slider":
						num = this.Data.sandColor.r;
						base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
						break;
					case "Green_Slider":
						num = this.Data.sandColor.g;
						base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
						break;
					case "Blue_Slider":
						num = this.Data.sandColor.b;
						base.NumberText = Mathf.RoundToInt(num * 255f).ToString();
						break;
					case "Pile_Slider":
						num = this.Data.pileSize / 100f;
						base.NumberText = this.Data.pileSize.ToString();
						break;
				}

				base.RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos) {
				switch (this.IDstring) {
					case "Red_Slider":
						this.Data.sandColor.r = nubPos;
						break;
					case "Green_Slider":
						this.Data.sandColor.g = nubPos;
						break;
					case "Blue_Slider":
						this.Data.sandColor.b = nubPos;
						break;
					case "Pile_Slider":
						this.Data.pileSize = Mathf.RoundToInt(nubPos * 100f);
						break;
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}
