namespace Floodwaters.Objects;

public class CactusRepresentation : PlacedObjectRepresentation {
	public Cactus cactus;

	public CactusRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.subNodes.Add(new CactusPanel(owner, "Cactus_Panel", this, new Vector2(0f, 100f)) {
			pos = (pObj.data as CactusData).panelPos
		});
		this.cactus = owner.room.updateList.OfType<Cactus>().FirstOrDefault(obj => obj.pObj == pObj);
		if (this.cactus == null) {
			this.cactus = new Cactus(owner.room, pObj);
			owner.room.AddObject(this.cactus);
		}
	}

	public override void Refresh() {
		base.Refresh();

		CactusData data = this.pObj.data as CactusData;
		data.panelPos = (this.subNodes[0] as Panel).pos;
		this.cactus.Refresh();
	}

	public class CactusPanel : Panel {
		public CactusPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 145f), "Cactus") {
			this.subNodes.Add(new CactusSlider(owner, "Scale_Slider", this, new Vector2(5f, 125f), "Scale:"));
			this.subNodes.Add(new CactusSlider(owner, "Size_Slider", this, new Vector2(5f, 105f), "Size:"));
			this.subNodes.Add(new CactusSlider(owner, "Seed_Slider", this, new Vector2(5f, 85f), "Seed:"));
			this.subNodes.Add(new CactusSlider(owner, "Hue_Slider", this, new Vector2(5f, 65f), "Hue:"));
			this.subNodes.Add(new CactusSlider(owner, "Sat_Slider", this, new Vector2(5f, 45f), "Saturation:"));
			this.subNodes.Add(new CactusSlider(owner, "Val_Slider", this, new Vector2(5f, 25f), "Value:"));
			this.subNodes.Add(new CactusSlider(owner, "Product_Slider", this, new Vector2(5f, 5f), "Product:"));
		}

		public override void Move(Vector2 newPos) {
			base.Move(newPos);

			this.parentNode.Refresh();
		}

		public class CactusSlider : Slider {
			public CactusSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();

				CactusData data = (this.parentNode.parentNode as CactusRepresentation).pObj.data as CactusData;

				if (this.IDstring == "Seed_Slider") {
					this.NumberText = data.seed.ToString();
					this.RefreshNubPos(data.seed / 10000f);
				}
				else if (this.IDstring == "Size_Slider") {
					this.NumberText = ((int) (data.size * 100f)).ToString() + "%";
					this.RefreshNubPos(Mathf.InverseLerp(0.5f, 2.0f, data.size));
				}
				else if (this.IDstring == "Scale_Slider") {
					this.NumberText = ((int) (data.scale * 100f)).ToString() + "%";
					this.RefreshNubPos(Mathf.InverseLerp(0.5f, 2.0f, data.scale));
				}
				else if (this.IDstring == "Hue_Slider") {
					this.NumberText = ((int) (data.hueOffset * 100f)).ToString();
					this.RefreshNubPos(data.hueOffset * 0.5f + 0.5f);
				}
				else if (this.IDstring == "Sat_Slider") {
					this.NumberText = ((int) (data.satOffset * 400f)).ToString();
					this.RefreshNubPos(data.satOffset * 2f + 0.5f);
				}
				else if (this.IDstring == "Val_Slider") {
					this.NumberText = ((int) (data.valOffset * 400f)).ToString();
					this.RefreshNubPos(data.valOffset * 2f + 0.5f);
				}
				else if (this.IDstring == "Product_Slider") {
					if (data.productType == 0)
						this.NumberText = "None";
					if (data.productType == 1)
						this.NumberText = "Spear";
					if (data.productType == 2)
						this.NumberText = "Fruit";
					if (data.productType == 3)
						this.NumberText = "Both";
					this.RefreshNubPos(data.productType / 3f);
				}
			}

			public override void NubDragged(float nubPos) {
				CactusData data = (this.parentNode.parentNode as CactusRepresentation).pObj.data as CactusData;

				if (this.IDstring == "Seed_Slider") {
					data.seed = (int) (nubPos * 10000f);
				}
				else if (this.IDstring == "Size_Slider") {
					data.size = Mathf.Lerp(0.5f, 2.0f, nubPos);
				}
				else if (this.IDstring == "Scale_Slider") {
					data.scale = Mathf.Lerp(0.5f, 2.0f, nubPos);
				}
				else if (this.IDstring == "Hue_Slider") {
					data.hueOffset = nubPos * 2f - 1f;
				}
				else if (this.IDstring == "Sat_Slider") {
					data.satOffset = (nubPos - 0.5f) * 0.5f;
				}
				else if (this.IDstring == "Val_Slider") {
					data.valOffset = (nubPos - 0.5f) * 0.5f;
				}
				else if (this.IDstring == "Product_Slider") {
					data.productType = (int) (nubPos * 4f);
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}
		}
	}
}