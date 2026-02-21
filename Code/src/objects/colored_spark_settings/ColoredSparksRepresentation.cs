namespace Floodwaters.Objects;

public class ColoredSparksRepresentation : PlacedObjectRepresentation {
	public ColoredSparks sparks;
	public ColoredSparksPanel panel;

	public ColoredSparksData Data => this.pObj.data as ColoredSparksData;

	public ColoredSparksRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.sparks = this.owner.room.updateList.OfType<ColoredSparks>().FirstOrDefault(x => x.pObj == pObj);

		if (this.sparks == null) {
			this.sparks = new ColoredSparks(this.owner.room, pObj);
			this.owner.room.AddObject(this.sparks);
		}

		this.panel = new ColoredSparksPanel(owner, "Colored_Sparks_Control_Panel", this, this.Data.panelPos);
		this.subNodes.Add(this.panel);
	}

	public class ColoredSparksPanel : Panel {
		public ColoredSparksPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 185f), "Colored Sparks") {
			this.subNodes.Add(new ColoredSparksSlider(owner, "ColorH_Slider", this, new Vector2(5f, 165f), "Base Hue: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "ColorS_Slider", this, new Vector2(5f, 145f), "Base Saturation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "ColorL_Slider", this, new Vector2(5f, 125f), "Base Lightness: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "VariaH_Slider", this, new Vector2(5f, 105f), "Hue Variation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "VariaS_Slider", this, new Vector2(5f, 85f), "Saturation Variation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "VariaL_Slider", this, new Vector2(5f, 65f), "Lightness Variation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "Direct_Slider", this, new Vector2(5f, 45f), "Direction: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "DirVar_Slider", this, new Vector2(5f, 25f), "Direction Variation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "Amount_Slider", this, new Vector2(5f, 5f), "Particle Density: "));
		}

		public class ColoredSparksSlider : Slider {
			public ColoredSparksData Data => (this.parentNode.parentNode as ColoredSparksRepresentation).Data;

			public ColoredSparksSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void NubDragged(float nubPos) {
				switch (this.IDstring) {
					case "ColorH_Slider": { this.Data.color.x = nubPos; break; }
					case "ColorS_Slider": { this.Data.color.y = nubPos; break; }
					case "ColorL_Slider": { this.Data.color.z = nubPos; break; }
					case "VariaH_Slider": { this.Data.colorVariation.x = nubPos; break; }
					case "VariaS_Slider": { this.Data.colorVariation.y = nubPos; break; }
					case "VariaL_Slider": { this.Data.colorVariation.z = nubPos; break; }
					case "Direct_Slider": { this.Data.direction = nubPos; break; }
					case "DirVar_Slider": { this.Data.directionVariation = nubPos; break; }
					case "Amount_Slider": { this.Data.amount = nubPos; break; }
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}

			public override void Refresh() {
				base.Refresh();
				float num = 0f;

				switch (this.IDstring) {
					case "ColorH_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.color.x * 359f).ToString();
						num = this.Data.color.x;
						break;
					}

					case "ColorS_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.color.y * 100f) + "%";
						num = this.Data.color.y;
						break;
					}

					case "ColorL_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.color.z * 100f) + "%";
						num = this.Data.color.z;
						break;
					}

					case "VariaH_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.colorVariation.x * 359f).ToString();
						num = this.Data.colorVariation.x;
						break;
					}

					case "VariaS_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.colorVariation.y * 100f) + "%";
						num = this.Data.colorVariation.y;
						break;
					}

					case "VariaL_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.colorVariation.z * 100f) + "%";
						num = this.Data.colorVariation.z;
						break;
					}

					case "Direct_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.direction * 359f) + " deg";
						num = this.Data.direction;
						break;
					}

					case "DirVar_Slider": {
						base.NumberText = Mathf.RoundToInt(this.Data.directionVariation * 359f) + " deg";
						num = this.Data.directionVariation;
						break;
					}

					case "Amount_Slider": {
						base.NumberText = this.Data.amount.ToString();
						num = this.Data.amount;
						break;
					}
				}

				base.RefreshNubPos(num);
			}
		}
	}
}