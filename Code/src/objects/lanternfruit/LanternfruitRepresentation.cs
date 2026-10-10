namespace Floodwaters.Objects;

public class LanternfruitRepresentation : PlacedObjectRepresentation {

	public Lanternfruit Lanternfruit => this.Lfruit;
	private LanternfruitData Data => this.pObj.data as LanternfruitData;
	public readonly Button typeButton;
	private readonly Lanternfruit Lfruit;
	public readonly LanternfruitControlPanel controlPanel;

	private readonly int lineSprite;
	private Vector2 lastPos;

    public LanternfruitRepresentation(DevUI owner,string idString, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, idString, parentNode, pObj, name) {
		this.controlPanel = new LanternfruitControlPanel(owner, "Lanternfruit_Panel", this, new Vector2(0f, 100f));
		this.subNodes.Add(this.controlPanel);
		this.controlPanel.pos = this.Data.panelPos;

		this.fSprites.Add(new FSprite("pixel", true));
		this.lineSprite = this.fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
		this.fSprites[this.lineSprite].anchorY = 0f;
		this.Lfruit = owner.room.updateList.OfType<Lanternfruit>().FirstOrDefault(b => b.pObj == pObj);

	}
	//creates the line connecting panel and object
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
	//makes the line update its position when moved



    public class LanternfruitControlPanel : Panel, IDevUISignals {
		public LanternfruitControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 10f), "Lanternfruit") {
			this.UpdateUI();
		}

		private void UpdateUI() {
			for (int num = this.subNodes.Count - 1; num > -1; num--) {
				this.subNodes[num].ClearSprites();
				this.subNodes.Pop();
			}

			LanternfruitData data = (this.parentNode as LanternfruitRepresentation).Data;
			float y = 5f;
			float x = 200f;

		
		this.subNodes.Add(new ColorSlider(this.owner, "MaxRegen", this, new Vector2(5f, y), "Max Regen:"));
			y += 20f;
		this.subNodes.Add(new ColorSlider(this.owner, "MinRegen", this, new Vector2(5f, y), "Min Regen:"));
			y += 25f;
		
		
		this.subNodes.Add(new Button(this.owner, "Color_Button", this, new Vector2(5f, y), 100f, data.colorType.ToString()));
			y += 20f;
			if (data.colorType == LanternfruitData.ColorType.Custom) {
				this.subNodes.Add(new ColorSlider(this.owner, "color1r", this, new Vector2(5f, y), "Red:"));
				this.subNodes.Add(new ColorSlider(this.owner, "color1g", this, new Vector2(5f, y + 20f), "Green:"));
				this.subNodes.Add(new ColorSlider(this.owner, "color1b", this, new Vector2(5f, y + 40f), "Blue:"));
				y += 60f;
			}
		
		this.size.y = y + 5f;
		this.size.x = x + 50f;
		
	
        }

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			LanternfruitData LanternfruitData = (this.parentNode as LanternfruitRepresentation).pObj.data as LanternfruitData;
			switch (sender.IDstring) {
				case "Color_Button":
					if ((int)LanternfruitData.colorType >= ExtEnum<LanternfruitData.ColorType>.values.Count - 1) {
						LanternfruitData.colorType = new LanternfruitData.ColorType(ExtEnum<LanternfruitData.ColorType>.values.GetEntry(0));
					}
					else {
						LanternfruitData.colorType = new LanternfruitData.ColorType(ExtEnum<LanternfruitData.ColorType>.values.GetEntry(LanternfruitData.colorType.Index + 1));
					}
					this.UpdateUI();
					break;
			}
		}

		public class ColorSlider : Slider {
		
	
			public ColorSlider(DevUI owner, string IDString, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDString, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();
			
				float from = 0f;

				if (this.IDstring == null)
					return;

				LanternfruitData data = (this.parentNode.parentNode as LanternfruitRepresentation).pObj.data as LanternfruitData;

				switch (this.IDstring) {
					case "color1r":
						from = data.color.r;
						base.NumberText = Mathf.RoundToInt(from * 255f).ToString();
						break;

					case "color1g":
						from = data.color.g;
						base.NumberText = Mathf.RoundToInt(from * 255f).ToString();
						break;

					case "color1b":
						from = data.color.b;
						base.NumberText = Mathf.RoundToInt(from * 255f).ToString();
						break;

					case "MinRegen":
						from = data.minRegen / 50f;
						base.NumberText = data.minRegen == 0 ? "N/A" : data.minRegen.ToString();
						break;

					case "MaxRegen":
						from = data.maxRegen / 50f;
						if (data.minRegen == 0) {
							base.NumberText = "N/A";
						}
						else
						{
						base.NumberText = data.maxRegen.ToString();
						}
						break;
	            }
					base.RefreshNubPos(from);
				}

			public override void NubDragged(float nubPos) {
				LanternfruitData data = (this.parentNode.parentNode as LanternfruitRepresentation).pObj.data as LanternfruitData;

				if (this.IDstring == null)
					return;

				switch (this.IDstring) {
					case "color1r":
						data.color.r = nubPos;
						break;

					case "color1g":
						data.color.g = nubPos;
						break;

					case "color1b":
						data.color.b = nubPos;
						break;

					case "MinRegen":
						data.minRegen = Mathf.Min(Mathf.RoundToInt(nubPos * 50f), data.maxRegen);
						break;

					case "MaxRegen":
						data.maxRegen = Mathf.Max(Mathf.RoundToInt(nubPos * 50f), data.minRegen);
						break;

					default:
						base.NubDragged(nubPos);
						return;
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
        }

	}
}
	
	


