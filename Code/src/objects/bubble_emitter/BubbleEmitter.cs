namespace Floodwaters.Objects;

public class BubbleEmitter : UpdatableAndDeletable {
	public PlacedObject po;

	public BubbleEmitterData Data => this.po.data as BubbleEmitterData;

	public float delay;

	public BubbleEmitter(Room room, PlacedObject po) {
		this.room = room;
		this.po = po;
	}

	public override void Update(bool eu) {
		base.Update(eu);

		this.delay -= 1f;

		if (this.delay <= 0f) {
			this.room.AddObject(new Bubble(this.po.pos + Custom.RNV() * 4f, Custom.RNV() * Mathf.Lerp(6f, 16f, Random.value) * 0.25f, false, false) {
				age = (int) (600f - Mathf.Lerp(1f, 10f, this.Data.age) * Random.Range(20f, Random.Range(30f, 80f))),
			});
			this.delay = Random.value * (1f - this.Data.intensity) * 40f;
		}
	}

	public class BubbleEmitterData : PlacedObject.Data {
		public Vector2 panelPos;
		public float intensity;
		public float age;

		public BubbleEmitterData(PlacedObject owner) : base(owner) {
			this.intensity = 0.5f;
			this.age = 0f;
		}

		public override void FromString(string s) {
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			try {
				this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.intensity = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.age = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			} catch (Exception) {
			}
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}

		protected string BaseSaveString() {
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", [
				this.panelPos.x,
				this.panelPos.y,
				this.intensity,
				this.age
			]);
		}

		public override string ToString() {
			string text = this.BaseSaveString();
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}
	}


	public class BubbleEmitterRepresentation : PlacedObjectRepresentation {
		private BubbleEmitterData Data {
			get {
				return this.pObj.data as BubbleEmitterData;
			}
		}

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
				private BubbleEmitterRepresentation Rep {
					get {
						return this.parentNode.parentNode as BubbleEmitterRepresentation;
					}
				}

				private BubbleEmitterData Data {
					get {
						return this.Rep.pObj.data as BubbleEmitterData;
					}
				}

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
}