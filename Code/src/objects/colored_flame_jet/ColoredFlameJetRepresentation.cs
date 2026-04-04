namespace Floodwaters.Objects;

public class ColoredFlameJetRepresentation : PlacedObjectRepresentation {
	public ColoredFlameJet jet;
	public ColoredFlameJetData data;

	public ColoredFlameJetRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj) : base(owner, IDstring, parentNode, pObj, "ColoredFlameJet") {
		this.data = pObj.data as ColoredFlameJetData;
		if (this.data.obj == null) {
			this.data.obj = new ColoredFlameJet(owner.room, this.data) {
				setPos = pObj.pos
			};
			owner.room.AddObject(this.data.obj);
		}

		this.jet = this.data.Obj;
		ColoredFlameJetPanel flameJetPanel = new ColoredFlameJetPanel(owner, "ColoredFlameJetPanel", this, default);
		this.subNodes.Add(flameJetPanel);
		flameJetPanel.pos = this.data.panelPos;
		FSprite fSprite = new FSprite("pixel") {
			anchorY = 0f
		};
		Handle handle = new Handle(owner, "FlameJetTarget", this, default);
		this.subNodes.Add(handle);
		handle.pos = this.data.target;
		FSprite fSprite2 = new FSprite("pixel") {
			anchorY = 0f
		};
		this.fSprites.Add(fSprite);
		owner.placedObjectsContainer.AddChild(fSprite);
		this.fSprites.Add(fSprite2);
		owner.placedObjectsContainer.AddChild(fSprite2);
	}

	public override void Refresh() {
		base.Refresh();
		base.MoveSprite(1, this.absPos);
		Panel panel = base.subNodes[0] as ColoredFlameJetPanel;
		this.data.panelPos = panel.pos;
		base.fSprites[1].scaleY = (this.data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		base.fSprites[1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.absPos + this.data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		base.MoveSprite(2, this.absPos);
		Handle handle = base.subNodes[1] as Handle;
		base.fSprites[2].scaleY = this.data.target.magnitude;
		base.fSprites[2].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.absPos + this.data.target);
		this.jet.setPos = this.data.pos = base.pObj.pos;
		this.jet.setTarget = this.data.target = handle.pos;
	}

	public class ColoredFlameJetPanel : Panel, IDevUISignals {
		public bool soundControlsVisible;
		public int selectedColor = -1;

		public ColoredFlameJetData Data => (this.parentNode as ColoredFlameJetRepresentation).data;

		public ColoredFlameJetPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Colored Flame Jet") {
			this.AddPanelElements();
		}

		public void RefreshUI() {
			for (int num = this.subNodes.Count - 1; num > -1; num--) {
				this.subNodes[num].ClearSprites();
				this.subNodes.Pop();
			}

			this.AddPanelElements();
		}

		public string InteractionTypeText() {
			if (this.Data.lethality >= 2) {
				return "Lethal";
			}

			if (this.Data.lethality >= 1) {
				return "Exhaustion";
			}

			return "Cosmetic";
		}

		public string SoundButtonText() {
			if (this.soundControlsVisible) {
				return "Hide Sound";
			}

			return "Show Sound";
		}

		public void AddPanelElements() {
			this.size.y = -15f;
			this.AddColorControls();
			this.size.y += 20f;
			this.subNodes.Add(new Button(this.owner, "EditSmoke", this, new Vector2(5f, this.size.y), 44f, "Smoke"));
			this.subNodes.Add(new Button(this.owner, "EditDark", this, new Vector2(54f, this.size.y), 44f, "Dark"));
			this.subNodes.Add(new Button(this.owner, "EditMid", this, new Vector2(103f, this.size.y), 44f, "Mid"));
			this.subNodes.Add(new Button(this.owner, "EditLight", this, new Vector2(152f, this.size.y), 44f, "Light"));
			this.subNodes.Add(new Button(this.owner, "EditBright", this, new Vector2(201f, this.size.y), 44f, "Bright"));
			this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "Depth", this, new Vector2(5f, base.panelStep), "Depth:"));
			this.AddSoundControls();
			this.subNodes.Add(new Button(this.owner, "ShowSound", this, new Vector2(125f, base.panelStep), 115f, this.SoundButtonText()));
			this.subNodes.Add(new Button(this.owner, "Lethality", this, new Vector2(5f, this.size.y), 115f, this.InteractionTypeText()));
			this.subNodes.Add(new Button(this.owner, "LinkTempToIntens", this, new Vector2(125f, base.panelStep), 115f, $"Temp=Intens: {this.Data.linkTempToIntens}"));
			this.subNodes.Add(new Button(this.owner, "ActiveDuring", this, new Vector2(5f, this.size.y), 115f, "Active: " + this.Data.activeDuring.value));
			this.AddTemperatureControls();
			this.AddIntensityAnimationControls();
			this.subNodes.Add(new Button(this.owner, "IntensityAnim", this, new Vector2(5f, base.panelStep), 245f, "Intensity animation: " + this.Data.intensityAnim.value));
			this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "Intensity", this, new Vector2(5f, base.panelStep), "Intensity:"));
			this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "Width", this, new Vector2(5f, base.panelStep), "Jet Width:"));
			this.size.y += 20f;
		}

		public void AddSoundControls() {
			if (this.soundControlsVisible) {
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "SmokeVolumeMax", this, new Vector2(5f, base.panelStep), "Smoke Volume:"));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "FireVolumeMax", this, new Vector2(5f, base.panelStep), "Fire Volume:"));
			}
		}

		public void AddIntensityAnimationControls() {
			if (!((this.parentNode as ColoredFlameJetRepresentation).data.intensityAnim == FlameJet.Animation.None)) {
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "IntensityAnimOffset", this, new Vector2(5f, base.panelStep), "Intens. Anim Offset:"));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "IntensityAnimSpeed", this, new Vector2(5f, base.panelStep), "Intens. Anim Speed:"));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "IntensityMax", this, new Vector2(5f, base.panelStep), "Intensity Max:"));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "IntensityMin", this, new Vector2(5f, base.panelStep), "Intensity Min:"));
			}
		}

		public void AddTemperatureControls() {
			ColoredFlameJetData data = (this.parentNode as ColoredFlameJetRepresentation).data;
			if (!data.linkTempToIntens) {
				this.AddTemperatureAnimationControls();
				this.subNodes.Add(new Button(this.owner, "TemperatureAnim", this, new Vector2(5f, base.panelStep), 245f, "Temperature animation: " + data.temperatureAnim.value));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "Temperature", this, new Vector2(5f, base.panelStep), "Temperature:"));
			}
		}

		public void AddTemperatureAnimationControls() {
			if (!((this.parentNode as ColoredFlameJetRepresentation).data.temperatureAnim == FlameJet.Animation.None)) {
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "TemperatureAnimOffset", this, new Vector2(5f, base.panelStep), "Temp. Anim Offset:"));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "TemperatureAnimSpeed", this, new Vector2(5f, base.panelStep), "Temp. Anim Speed:"));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "TemperatureMax", this, new Vector2(5f, base.panelStep), "Temperature Max:"));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "TemperatureMin", this, new Vector2(5f, base.panelStep), "Temperature Min:"));
			}
		}

		public void AddColorControls() {
			if (this.selectedColor == -1) return;

			string name = this.selectedColor switch {
				0 => "Smoke",
				1 => "Dark",
				2 => "Mid",
				3 => "Light",
				4 => "Bright",
				_ => "",
			};

			ColoredFlameJetData data = (this.parentNode as ColoredFlameJetRepresentation).data;
			if (this.selectedColor == 0) {
				this.subNodes.Add(new Button(this.owner, "PaletteSmoke", this, new Vector2(5f, base.panelStep), 245f, "Color from: " + (data.paletteSmoke ? "Palette" : "Custom")));
			}
			if (this.selectedColor != 0 || !data.paletteSmoke) {
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "Blue", this, new Vector2(5f, base.panelStep), $"{name} Blue: "));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "Green", this, new Vector2(5f, base.panelStep), $"{name} Green: "));
				this.subNodes.Add(new ColoredFlameJetSlider(this.owner, "Red", this, new Vector2(5f, base.panelStep), $"{name} Red: "));
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			ColoredFlameJetData flameJetData = (this.parentNode as ColoredFlameJetRepresentation).data;
			if (sender.IDstring == "ShowSound") {
				this.soundControlsVisible = !this.soundControlsVisible;
				this.RefreshUI();
			}
			else if (sender.IDstring == "Lethality") {
				flameJetData.lethality = (flameJetData.lethality + 1) % 3;
				flameJetData.obj.lethality = flameJetData.lethality;
				this.RefreshUI();
			}
			else if (sender.IDstring == "LinkTempToIntens") {
				flameJetData.linkTempToIntens = !flameJetData.linkTempToIntens;
				flameJetData.obj.linkTempToIntens = flameJetData.linkTempToIntens;
				this.RefreshUI();
			}
			else if (sender.IDstring == "ActiveDuring") {
				flameJetData.activeDuring = new FlameJet.ActiveDuring(ExtEnum<FlameJet.ActiveDuring>.values.GetEntry((flameJetData.activeDuring.Index + 1) % ExtEnum<FlameJet.ActiveDuring>.values.Count));
				flameJetData.obj.activeDuring = flameJetData.activeDuring;
				(sender as Button).Text = "Active: " + flameJetData.activeDuring.value;
			}
			else if (sender.IDstring == "TemperatureAnim") {
				flameJetData.temperatureAnim = new FlameJet.Animation(ExtEnum<FlameJet.Animation>.values.GetEntry((flameJetData.temperatureAnim.Index + 1) % ExtEnum<FlameJet.Animation>.values.Count));
				flameJetData.obj.temperatureAnim = flameJetData.temperatureAnim;
				this.RefreshUI();
			}
			else if (sender.IDstring == "IntensityAnim") {
				flameJetData.intensityAnim = new FlameJet.Animation(ExtEnum<FlameJet.Animation>.values.GetEntry((flameJetData.intensityAnim.Index + 1) % ExtEnum<FlameJet.Animation>.values.Count));
				flameJetData.obj.intensityAnim = flameJetData.intensityAnim;
				this.RefreshUI();
			}
			else if (sender.IDstring == "EditSmoke") {
				this.selectedColor = (this.selectedColor == 0) ? -1 : 0;
				this.RefreshUI();
			}
			else if (sender.IDstring == "EditDark") {
				this.selectedColor = (this.selectedColor == 1) ? -1 : 1;
				this.RefreshUI();
			}
			else if (sender.IDstring == "EditMid") {
				this.selectedColor = (this.selectedColor == 2) ? -1 : 2;
				this.RefreshUI();
			}
			else if (sender.IDstring == "EditLight") {
				this.selectedColor = (this.selectedColor == 3) ? -1 : 3;
				this.RefreshUI();
			}
			else if (sender.IDstring == "EditBright") {
				this.selectedColor = (this.selectedColor == 4) ? -1 : 4;
				this.RefreshUI();
			}
			else if (sender.IDstring == "PaletteSmoke") {
				this.Data.paletteSmoke = !this.Data.paletteSmoke;
				this.RefreshUI();
			}
		}

		public ref Color SelectedColor() {
			ColoredFlameJetData data = (this.parentNode as ColoredFlameJetRepresentation).data;

			switch (this.selectedColor) {
				case 0: return ref data.smokeColor;
				case 1: return ref data.darkColor;
				case 2: return ref data.midColor;
				case 3: return ref data.lightColor;
				case 4: return ref data.brightColor;
				default: return ref data.smokeColor;
			};
		}

		public class ColoredFlameJetSlider : Slider {
			public ColoredFlameJetSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f) {
			}

			public override void NubDragged(float nubPos) {
				ColoredFlameJetPanel panel = this.parentNode as ColoredFlameJetPanel;
				ColoredFlameJetRepresentation repr = panel.parentNode as ColoredFlameJetRepresentation;
				ColoredFlameJetData data = repr.data;

				switch (this.IDstring) {
					case "Intensity":
						data.intensity = nubPos;
						data.obj.intensity = nubPos;
						break;
					case "Width":
						data.width = nubPos;
						data.obj.width = nubPos;
						break;
					case "Temperature":
						data.temperature = nubPos;
						data.obj.temperature = nubPos;
						break;
					case "IntensityAnimSpeed":
						data.intensityAnimSpeed = nubPos;
						data.obj.intensityAnimSpeed = nubPos;
						break;
					case "IntensityAnimOffset":
						data.intensityAnimOffset = nubPos;
						data.obj.intensityAnimOffset = nubPos;
						break;
					case "TemperatureAnimSpeed":
						data.temperatureAnimSpeed = nubPos;
						data.obj.temperatureAnimSpeed = nubPos;
						break;
					case "TemperatureAnimOffset":
						data.temperatureAnimOffset = nubPos;
						data.obj.temperatureAnimOffset = nubPos;
						break;
					case "IntensityMin":
						data.obj.intensityMin = nubPos;
						data.intensityMin = data.obj.intensityMin;
						break;
					case "IntensityMax":
						data.obj.intensityMax = nubPos;
						data.intensityMax = data.obj.intensityMax;
						break;
					case "TemperatureMin":
						data.obj.temperatureMin = nubPos;
						data.temperatureMin = data.obj.temperatureMin;
						break;
					case "TemperatureMax":
						data.obj.temperatureMax = nubPos;
						data.temperatureMax = data.obj.temperatureMax;
						break;
					case "FireVolumeMax":
						data.obj.fireVolumeMax = nubPos;
						data.fireVolumeMax = data.obj.fireVolumeMax;
						break;
					case "SmokeVolumeMax":
						data.obj.smokeVolumeMax = nubPos;
						data.smokeVolumeMax = data.obj.smokeVolumeMax;
						break;
					case "Red":
						panel.SelectedColor().r = nubPos;
						break;
					case "Green":
						panel.SelectedColor().g = nubPos;
						break;
					case "Blue":
						panel.SelectedColor().b = nubPos;
						break;
					case "Depth":
						data.depth = Mathf.RoundToInt(nubPos * 29f);
						break;
				}

				this.parentNode.parentNode.Refresh();
				this.Refresh();
			}

			public override void Refresh() {
				base.Refresh();

				float nubPos = 0f;
				ColoredFlameJetPanel panel = this.parentNode as ColoredFlameJetPanel;
				ColoredFlameJetData data = (panel.parentNode as ColoredFlameJetRepresentation).data;

				switch (this.IDstring) {
					case "Intensity":
						nubPos = data.intensity;
						base.NumberText = nubPos + " ";
						break;
					case "Width":
						nubPos = data.width;
						base.NumberText = nubPos + " ";
						break;
					case "Temperature":
						nubPos = data.temperature;
						base.NumberText = nubPos + " ";
						break;
					case "IntensityAnimSpeed":
						nubPos = data.intensityAnimSpeed;
						base.NumberText = nubPos + " ";
						break;
					case "IntensityAnimOffset":
						nubPos = data.intensityAnimOffset;
						base.NumberText = nubPos + " ";
						break;
					case "TemperatureAnimSpeed":
						nubPos = data.temperatureAnimSpeed;
						base.NumberText = nubPos + " ";
						break;
					case "TemperatureAnimOffset":
						nubPos = data.temperatureAnimOffset;
						base.NumberText = nubPos + " ";
						break;
					case "IntensityMin":
						nubPos = data.intensityMin;
						base.NumberText = nubPos + " ";
						break;
					case "IntensityMax":
						nubPos = data.intensityMax;
						base.NumberText = nubPos + " ";
						break;
					case "TemperatureMin":
						nubPos = data.temperatureMin;
						base.NumberText = nubPos + " ";
						break;
					case "TemperatureMax":
						nubPos = data.temperatureMax;
						base.NumberText = nubPos + " ";
						break;
					case "FireVolumeMax":
						nubPos = data.fireVolumeMax;
						base.NumberText = nubPos + " ";
						break;
					case "SmokeVolumeMax":
						nubPos = data.smokeVolumeMax;
						base.NumberText = nubPos + " ";
						break;
					case "Red":
						nubPos = panel.SelectedColor().r;
						base.NumberText = Mathf.RoundToInt(nubPos * 255f).ToString();
						break;
					case "Green":
						nubPos = panel.SelectedColor().g;
						base.NumberText = Mathf.RoundToInt(nubPos * 255f).ToString();
						break;
					case "Blue":
						nubPos = panel.SelectedColor().b;
						base.NumberText = Mathf.RoundToInt(nubPos * 255f).ToString();
						break;
					case "Depth":
						nubPos = data.depth / 29f;
						base.NumberText = data.depth.ToString();
						break;
				}

				this.RefreshNubPos(nubPos);
			}
		}
	}
}