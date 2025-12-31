namespace Floodwaters.Objects;

public class ColoredCoralNeuron : CoralNeuron {
	public readonly PlacedObject pObj;

	public ColoredCoralNeuronData Data => this.pObj.data as ColoredCoralNeuronData;

	public ColoredCoralNeuron(CoralNeuronSystem system, Room room, float length, Vector2? posA, Vector2? posB, PlacedObject pObj) : base(system, room, length, posA, posB) {
		this.pObj = pObj;
	}

	public Color MeshColor(float f, Color from) {
		if (this.Data == null) {
			return Color.black;
		}

		f = Mathf.Abs(f - 0.5f) * 2f;
		Vector3 offset = Custom.RGB2HSL(from);
	
		float hue = (offset.x + Custom.Decimal(Mathf.Lerp(1.025f, 0.9638889f, 0.5f + 0.5f * Mathf.Pow(f, 3f)))) % 1f;
		float sat = offset.y * Custom.LerpMap(f, 0.8f, 1f, 1f, 0.5f);
		float light = Mathf.Clamp01(offset.z * 2f * Custom.LerpMap(f, 0.7f, 1f, 0.5f, 0.15f));
	
		return Custom.HSL2RGB(hue, sat, light);
	}

	public void UpdateMyceliaColor(Mycelium self, Color startColor, Color midColor, Color endColor, float gradientStart, TriangleMesh triangle) {
		self.color = startColor;
		for (int i = 0; i < triangle.verticeColors.Length; i++) {
			float value = i / (float) (triangle.verticeColors.Length - 1);
			triangle.verticeColors[i] = Color.Lerp(startColor, midColor, Mathf.InverseLerp(gradientStart, 1f, value));
		}
		for (int j = 1; j < 3; j++) {
			triangle.verticeColors[triangle.verticeColors.Length - j] = endColor;
		}
	}

	public void Refresh(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; i++) {
			float f = i / (float)((sLeaser.sprites[0] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = this.MeshColor(f, this.Data.mainColor);
		}

		for (int i = 0; i < this.bumps.Length; i++) {
			int idx = 2 + this.mycelia.GetLength(0) * 2 + 3 * i;
			sLeaser.sprites[idx].color = Color.Lerp(this.MeshColor(this.bumps[i].y, this.Data.mainColor), this.Data.mainColor * 0.2f, 0.25f);
			Vector3 offset = Custom.RGB2HSL(this.Data.mainColor);
			sLeaser.sprites[idx + 2].color = Custom.HSL2RGB(offset.x, offset.y, Mathf.Clamp01(offset.z * 2f * (0.1f + 0.9f * Mathf.Lerp(this.bumpPings[i, 1], this.bumpPings[i, 0], timeStacker))));
		}

		int myceliaSpriteIndex = 2;
		for (int i = 0; i < this.mycelia.GetLength(0); i++) {
			for (int j = 0; j < 2; j++) {
				this.UpdateMyceliaColor(
					this.mycelia[i, j],
					this.MeshColor(Mathf.InverseLerp(0f, this.segments.GetLength(0) - 1, this.SegmentOfMycelium(i * 2 + j)), this.Data.tendrilStartColor),
					this.MeshColor(Mathf.InverseLerp(0f, this.segments.GetLength(0) - 1, this.SegmentOfMycelium(i * 2 + j)), this.Data.tendrilMidColor),
					this.MeshColor(Mathf.InverseLerp(0f, this.segments.GetLength(0) - 1, this.SegmentOfMycelium(i * 2 + j)), this.Data.tendrilEndColor),
					0f, sLeaser.sprites[myceliaSpriteIndex] as TriangleMesh
				);
				myceliaSpriteIndex++;
			}
		}
	}


	public class ColoredCoralNeuronData : PlacedObject.ResizableObjectData {
		public Vector2 panelPos = new Vector2(100f, 0f);
		public Color mainColor = Color.red;
		public Color tendrilStartColor = Color.red;
		public Color tendrilMidColor = new Color(0.102f, 0.302f, 0.286f);
		public Color tendrilEndColor = Color.blue;

		public ColoredCoralNeuronData(PlacedObject owner) : base(owner) {
		}

		public override void FromString(string s) {
			base.FromString(s);

			try {
				string[] array = Regex.Split(s, "~");
				this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.mainColor.r = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.mainColor.g = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.mainColor.b = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilStartColor.r = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilStartColor.g = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilStartColor.b = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilMidColor.r = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilMidColor.g = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilMidColor.b = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilEndColor.r = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilEndColor.g = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.tendrilEndColor.b = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 16);
			} catch (Exception) {}
		}

		public override string ToString() {
			string baseString = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}",
				this.panelPos.x,
				this.panelPos.y,
				this.mainColor.r,
				this.mainColor.g,
				this.mainColor.b,
				this.tendrilStartColor.r,
				this.tendrilStartColor.g,
				this.tendrilStartColor.b,
				this.tendrilMidColor.r,
				this.tendrilMidColor.g,
				this.tendrilMidColor.b,
				this.tendrilEndColor.r,
				this.tendrilEndColor.g,
				this.tendrilEndColor.b
			);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
		}
	}

	public class ColoredCoralNeuronRepresentation : ResizeableObjectRepresentation {
		public ColoredCoralNeuron neuron;

		public ColoredCoralNeuronRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, false) {
			this.subNodes.Add(new ColoredCoralNeuronPanel(owner, "ColoredCoralNeuron_Panel", this, (pObj.data as ColoredCoralNeuronData).panelPos));
			this.fSprites.Add(new FSprite("pixel", true) {
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);

			this.neuron = owner.room.updateList.OfType<ColoredCoralNeuron>().FirstOrDefault(x => x.pObj == this.pObj);
			if (this.neuron == null) {
				if (!owner.room.updateList.Any(x => x is CoralNeuronSystem)) {
					owner.room.AddObject(new CoralNeuronSystem());
				}

				ColoredCoralNeuronData data = pObj.data as ColoredCoralNeuronData;
				this.neuron = new ColoredCoralNeuron(owner.room.updateList.OfType<CoralNeuronSystem>().FirstOrDefault(), owner.room, data.handlePos.magnitude, pObj.pos, pObj.pos + data.handlePos, pObj);
				owner.room.AddObject(this.neuron);
			}
		}

		public override void Refresh() {
			base.Refresh();

			ColoredCoralNeuronData data = this.pObj.data as ColoredCoralNeuronData;
			ColoredCoralNeuronPanel panel = this.subNodes[this.subNodes.Count - 1] as ColoredCoralNeuronPanel;
			base.MoveSprite(this.fSprites.Count - 1, this.absPos);
			this.fSprites[this.fSprites.Count - 1].scaleY = panel.pos.magnitude;
			this.fSprites[this.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, panel.nonCollapsedAbsPos);
			data.panelPos = panel.pos;
		}

		public class ColoredCoralNeuronPanel : Panel {
			public ColoredCoralNeuronPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 245f), "Colored Coral Neuron") {
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Main_R_Slider", this, new Vector2(5f, 225f), "Main Red: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Main_G_Slider", this, new Vector2(5f, 205f), "Main Green: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Main_B_Slider", this, new Vector2(5f, 185f), "Main Blue: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Start_R_Slider", this, new Vector2(5f, 165f), "Tendril Start Red: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Start_G_Slider", this, new Vector2(5f, 145f), "Tendril Start Green: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Start_B_Slider", this, new Vector2(5f, 125f), "Tendril Start Blue: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Mid_R_Slider", this, new Vector2(5f, 105f), "Tendril Mid Red: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Mid_G_Slider", this, new Vector2(5f, 85f), "Tendril Mid Green: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "Mid_B_Slider", this, new Vector2(5f, 65f), "Tendril Mid Blue: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "End_R_Slider", this, new Vector2(5f, 45f), "Tendril End Red: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "End_G_Slider", this, new Vector2(5f, 25f), "Tendril End Green: "));
				this.subNodes.Add(new ColoredCoralNeuronSlider(owner, "End_B_Slider", this, new Vector2(5f, 5f), "Tendril End Blue: "));
			}

			public class ColoredCoralNeuronSlider : Slider {
				public ColoredCoralNeuronSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
				}

				public override void Refresh() {
					base.Refresh();
					ColoredCoralNeuronData data = (this.parentNode.parentNode as ColoredCoralNeuronRepresentation).pObj.data as ColoredCoralNeuronData;

					switch (this.IDstring) {
						case "Main_R_Slider": {
							base.NumberText = Mathf.FloorToInt(data.mainColor.r * 255f).ToString();
							base.RefreshNubPos(data.mainColor.r);
							break;
						}

						case "Main_G_Slider": {
							base.NumberText = Mathf.FloorToInt(data.mainColor.g * 255f).ToString();
							base.RefreshNubPos(data.mainColor.g);
							break;
						}

						case "Main_B_Slider": {
							base.NumberText = Mathf.FloorToInt(data.mainColor.b * 255f).ToString();
							base.RefreshNubPos(data.mainColor.b);
							break;
						}

						case "Start_R_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilStartColor.r * 255f).ToString();
							base.RefreshNubPos(data.tendrilStartColor.r);
							break;
						}

						case "Start_G_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilStartColor.g * 255f).ToString();
							base.RefreshNubPos(data.tendrilStartColor.g);
							break;
						}

						case "Start_B_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilStartColor.b * 255f).ToString();
							base.RefreshNubPos(data.tendrilStartColor.b);
							break;
						}

						case "Mid_R_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilMidColor.r * 255f).ToString();
							base.RefreshNubPos(data.tendrilMidColor.r);
							break;
						}

						case "Mid_G_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilMidColor.g * 255f).ToString();
							base.RefreshNubPos(data.tendrilMidColor.g);
							break;
						}

						case "Mid_B_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilMidColor.b * 255f).ToString();
							base.RefreshNubPos(data.tendrilMidColor.b);
							break;
						}

						case "End_R_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilEndColor.r * 255f).ToString();
							base.RefreshNubPos(data.tendrilEndColor.r);
							break;
						}

						case "End_G_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilEndColor.g * 255f).ToString();
							base.RefreshNubPos(data.tendrilEndColor.g);
							break;
						}

						case "End_B_Slider": {
							base.NumberText = Mathf.FloorToInt(data.tendrilEndColor.b * 255f).ToString();
							base.RefreshNubPos(data.tendrilEndColor.b);
							break;
						}
					}
				}

				public override void NubDragged(float nubPos) {
					ColoredCoralNeuronData data = (this.parentNode.parentNode as ColoredCoralNeuronRepresentation).pObj.data as ColoredCoralNeuronData;

					switch (this.IDstring) {
						case "Main_R_Slider": {
							data.mainColor.r = nubPos;
							break;
						}

						case "Main_G_Slider": {
							data.mainColor.g = nubPos;
							break;
						}

						case "Main_B_Slider": {
							data.mainColor.b = nubPos;
							break;
						}

						case "Start_R_Slider": {
							data.tendrilStartColor.r = nubPos;
							break;
						}

						case "Start_G_Slider": {
							data.tendrilStartColor.g = nubPos;
							break;
						}

						case "Start_B_Slider": {
							data.tendrilStartColor.b = nubPos;
							break;
						}

						case "Mid_R_Slider": {
							data.tendrilMidColor.r = nubPos;
							break;
						}

						case "Mid_G_Slider": {
							data.tendrilMidColor.g = nubPos;
							break;
						}

						case "Mid_B_Slider": {
							data.tendrilMidColor.b = nubPos;
							break;
						}

						case "End_R_Slider": {
							data.tendrilEndColor.r = nubPos;
							break;
						}

						case "End_G_Slider": {
							data.tendrilEndColor.g = nubPos;
							break;
						}

						case "End_B_Slider": {
							data.tendrilEndColor.b = nubPos;
							break;
						}
					}

					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
			}
		}
	}
}