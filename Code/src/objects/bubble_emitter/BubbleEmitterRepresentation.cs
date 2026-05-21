namespace Floodwaters.Objects;

public class BubbleEmitterRepresentation : PlacedObjectRepresentation {
	private BubbleEmitterData Data => this.pObj.data as BubbleEmitterData;

	public BubbleEmitterRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.controlPanel = new BubbleEmitterControlPanel(owner, "Bubble_Emitter_Panel", this, new Vector2(0f, 100f));
		this.subNodes.Add(this.controlPanel);
		this.controlPanel.pos = this.Data.panelPos;
		this.fSprites.Add(new FSprite("pixel", true));
		this.lineSprite = this.fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
		this.fSprites[this.lineSprite].anchorY = 0f;
		this.emitter = owner.room.updateList.OfType<BubbleEmitter>().FirstOrDefault(b => b.po == pObj);

		if (this.emitter == null) {
			this.emitter = new BubbleEmitter(owner.room, pObj);
			owner.room.AddObject(this.emitter);
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

	private readonly BubbleEmitter emitter;

	private readonly BubbleEmitterControlPanel controlPanel;

	private readonly int lineSprite;

	private Vector2 lastPos;

	public class BubbleEmitterControlPanel : Panel {
		public BubbleEmitterControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 45f), "Bubble Emitter") {
			this.subNodes.Add(new BubbleEmitterSlider(owner, "Intensity_Slider", this, new Vector2(5f, 25f), "Intensity:"));
			this.subNodes.Add(new BubbleEmitterSlider(owner, "Age_Slider", this, new Vector2(5f, 5f), "Age:"));
		}

		public class BubbleEmitterSlider : Slider {
			private BubbleEmitterRepresentation Rep => this.parentNode.parentNode as BubbleEmitterRepresentation;

			private BubbleEmitterData Data => this.Rep.pObj.data as BubbleEmitterData;

			public BubbleEmitterSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 60f) {
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;
				if (this.IDstring == "Intensity_Slider") {
					num = this.Data.intensity;
					base.NumberText = Mathf.RoundToInt(num * 100f).ToString();
				} else {
					num = this.Data.age;
					base.NumberText = Mathf.RoundToInt((num + 0.1f) * 10f).ToString() + "x";
				}
				base.RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos) {
				if (this.IDstring == "Intensity_Slider")
					this.Data.intensity = nubPos;
				else if (this.IDstring == "Age_Slider")
					this.Data.age = nubPos;

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}