namespace Floodwaters.Objects;

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