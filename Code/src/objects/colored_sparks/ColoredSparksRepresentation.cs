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

	public class ColoredSparksPanel : Panel, IDevUISignals {
		public ColoredSparksData Data => (this.parentNode as ColoredSparksRepresentation).Data;

		private readonly ColorPreview preview;

		private string Depth0Text => this.Data.depth0 ? "Front" : "------";
		private string Depth1Text => this.Data.depth1 ? "Ground" : "------";
		private string Depth2Text => this.Data.depth2 ? "Back" : "------";
		private string Depth3Text => this.Data.depth3 ? "Sky" : "------";

		public ColoredSparksPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(495f, 170f), "Colored Sparks") {
			float col1 = 5f;
			float col2 = 250f;
			float btnWidth = 117.5f;
			this.subNodes.Add(new ColoredSparksSlider(owner, "ColorH_Slider", this, new Vector2(col1, 155f), "Hue Base: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "VariaH_Slider", this, new Vector2(col2, 155f), "Hue Variation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "ColorS_Slider", this, new Vector2(col1, 135f), "Saturation Base: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "VariaS_Slider", this, new Vector2(col2, 135f), "Saturation Variation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "ColorL_Slider", this, new Vector2(col1, 115f), "Lightness Base: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "VariaL_Slider", this, new Vector2(col2, 115f), "Lightness Variation: "));
			this.subNodes.Add(this.preview = new ColorPreview(owner, "Color_Preview", this, new Vector2(5f, 65f), new Vector2(485f, 45f), new Vector2Int(32, 8)));
			this.subNodes.Add(new ColoredSparksSlider(owner, "Direct_Slider", this, new Vector2(col1, 45f), "Direction: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "DirVar_Slider", this, new Vector2(col2, 45f), "Direction Variation: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "Amount_Slider", this, new Vector2(col1, 25f), "Particle Density: "));
			this.subNodes.Add(new ColoredSparksSlider(owner, "SpeedS_Slider", this, new Vector2(col2, 25f), "Speed Scale: "));
			this.subNodes.Add(new Button(owner, "Depth0", this, new Vector2(5f, 5f), btnWidth, this.Depth0Text));
			this.subNodes.Add(new Button(owner, "Depth1", this, new Vector2(5f + btnWidth + 5f, 5f), btnWidth, this.Depth1Text));
			this.subNodes.Add(new Button(owner, "Depth2", this, new Vector2(5f + (btnWidth * 2f) + 10f, 5f), btnWidth, this.Depth2Text));
			this.subNodes.Add(new Button(owner, "Depth3", this, new Vector2(5f + (btnWidth * 3f) + 15f, 5f), btnWidth, this.Depth3Text));

			this.Refresh();
		}

		public override void Refresh() {
			base.Refresh();

			float baseH = this.Data.color.x;
			float baseS = this.Data.color.y;
			float baseL = this.Data.color.z;
			float varH = this.Data.colorVariation.x;
			float varS = this.Data.colorVariation.y;
			float varL = this.Data.colorVariation.z;

			Random.State state = Random.state;
			UnityEngine.Random.InitState(42);

			for (int x = 0; x < this.preview.gridCount.x; x++) {
				for (int y = 0; y < this.preview.gridCount.y; y++) {
					float randH = UnityEngine.Random.Range(-1f, 1f);
					float randS = UnityEngine.Random.Range(-1f, 1f);
					float randL = UnityEngine.Random.Range(-1f, 1f);

					float finalH = Mathf.Repeat(baseH + (varH * randH), 1f);
					float finalS = Mathf.Clamp01(baseS + (varS * randS));
					float finalL = Mathf.Clamp01(baseL + (varL * randL));

					Color displayColor = Custom.HSL2RGB(finalH, finalS, finalL);
					this.preview.SetColor(x, y, displayColor);
				}
			}

			Random.state = state;
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			switch (sender.IDstring) {
				case "Depth0": {
					this.Data.depth0 = !this.Data.depth0;
					(sender as Button).Text = this.Depth0Text;
					break;
				}

				case "Depth1": {
					this.Data.depth1 = !this.Data.depth1;
					(sender as Button).Text = this.Depth1Text;
					break;
				}

				case "Depth2": {
					this.Data.depth2 = !this.Data.depth2;
					(sender as Button).Text = this.Depth2Text;
					break;
				}

				case "Depth3": {
					this.Data.depth3 = !this.Data.depth3;
					(sender as Button).Text = this.Depth3Text;
					break;
				}
			}
		}

		public class ColorPreview : PositionedDevUINode {
			public Vector2 size;
			public Vector2Int gridCount;

			public ColorPreview(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, Vector2Int gridCount) : base(owner, IDstring, parentNode, pos) {
				this.size = size;
				this.gridCount = gridCount;
				
				float cellWidth = this.size.x / this.gridCount.x;
				float cellHeight = this.size.y / this.gridCount.y;

				for (int x = 0; x < this.gridCount.x; x++) {
					for (int y = 0; y < this.gridCount.y; y++) {
						FSprite p = new FSprite("pixel") {
							scaleX = cellWidth,
							scaleY = cellHeight,
							anchorX = 0f,
							anchorY = 0f
						};
						this.fSprites.Add(p);
						if (owner != null) {
							Futile.stage.AddChild(p);
						}
					}
				}
				this.Refresh();
			}

			public override void Refresh() {
				base.Refresh();

				float cellWidth = this.size.x / this.gridCount.x;
				float cellHeight = this.size.y / this.gridCount.y;
				for (int x = 0; x < this.gridCount.x; x++) {
					for (int y = 0; y < this.gridCount.y; y++) {
						this.MoveSprite(x * this.gridCount.y + y, this.absPos + new Vector2(x * cellWidth, y * cellHeight));
					}
				}
			}

			public void SetColor(int x, int y, Color color) {
				int spriteIndex = x * this.gridCount.y + y;
				
				if (spriteIndex >= 0 && spriteIndex < this.fSprites.Count) {
					this.fSprites[spriteIndex].color = color;
				}
			}
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
					case "SpeedS_Slider": { this.Data.speedScale = Mathf.LerpUnclamped(0.01f, 2f, nubPos); break; }
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

					case "SpeedS_Slider": {
						num = Custom.InverseLerpUnclamped(0.01f, 2f, this.Data.speedScale);
						base.NumberText = Mathf.RoundToInt(this.Data.speedScale * 100f).ToString() + "%";
						break;
					}
				}

				base.RefreshNubPos(num);
			}
		}
	}
}