namespace Floodwaters.Objects;

public class PaletteOverrideRepresentation : PlacedObjectRepresentation {
	public PaletteOverrideData Data => this.pObj.data as PaletteOverrideData;
	public PaletteOverride obj;

	private readonly int panel;
	private PaletteOverridePanel Panel => this.subNodes[this.panel] as PaletteOverridePanel;

	private readonly int pixel;
	private FSprite Pixel => this.fSprites[this.pixel];

	private readonly int handle;
	private Handle Handle => this.subNodes[this.handle] as Handle;

	private readonly int ring;
	private FSprite Ring => this.fSprites[this.ring];

	private readonly int rect;

	public PaletteOverrideRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
		this.obj = owner.room.updateList.OfType<PaletteOverride>().FirstOrDefault(o => o.pObj == pObj);
		if (this.obj == null) {
			this.obj = new PaletteOverride(owner.room, pObj);
			owner.room.AddObject(this.obj);
		}

		this.handle = this.subNodes.Count;
		this.subNodes.Add(new Handle(owner, "Rad_Handle", this, this.Data.handlePos));

		this.ring = this.fSprites.Count;
		this.fSprites.Add(new FSprite("Futile_White") {
			shader = owner.room.game.rainWorld.Shaders["VectorCircle"],
			isVisible = false,
		});
		owner.placedObjectsContainer.AddChild(this.Ring);

		this.panel = this.subNodes.Count;
		this.subNodes.Add(new PaletteOverridePanel(owner, "PaletteOverride_Panel", this, this.Data.panelPos));

		this.rect = this.fSprites.Count;
		for (int i = 0; i < 4; i++) {
			this.fSprites.Add(new FSprite("pixel"));
			owner.placedObjectsContainer.AddChild(this.fSprites[^1]);
			this.fSprites[^1].anchorX = 0f;
			this.fSprites[^1].anchorY = 0f;
			this.fSprites[^1].isVisible = false;
		}

		this.pixel = this.fSprites.Count;
		this.fSprites.Add(new FSprite("pixel") {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.Pixel);
	}

	public override void Refresh() {
		base.Refresh();
		this.Data.panelPos = this.Panel.pos;
		this.Data.handlePos = this.Handle.pos;

		base.MoveSprite(this.pixel, this.absPos);
		this.Pixel.scaleY = (this.Data.panelPos + (this.Panel.collapsed ? (Vector2.up * this.Panel.size.y) : Vector2.zero)).magnitude;
		this.Pixel.rotation = Custom.AimFromOneVectorToAnother(Vector2.zero, this.Data.panelPos + (this.Panel.collapsed ? (Vector2.up * this.Panel.size.y) : Vector2.zero));

		base.MoveSprite(this.ring, this.absPos);
		this.Ring.isVisible = this.Data.shape == PaletteOverrideData.Shape.Circle;
		this.Ring.scale = this.Handle.pos.magnitude / 8f;
		this.Ring.alpha = 2f / this.Handle.pos.magnitude;

		for (int i = 0; i < 4; i++) {
			this.fSprites[this.rect + i].isVisible = this.Data.shape == PaletteOverrideData.Shape.Rectangle;
		}
		if (this.Data.shape != PaletteOverrideData.Shape.Rectangle)
			return;

		this.MoveSprite(this.rect + 0, this.absPos);
		this.fSprites[this.rect + 1].scaleY = this.Data.handlePos.y;

		this.MoveSprite(this.rect + 1, this.absPos);
		this.fSprites[this.rect + 0].scaleX = this.Data.handlePos.x;

		this.MoveSprite(this.rect + 2, this.absPos + new Vector2(this.Data.handlePos.x, 0f));
		this.fSprites[this.rect + 2].scaleY = this.Data.handlePos.y;

		this.MoveSprite(this.rect + 3, this.absPos + new Vector2(0f, this.Data.handlePos.y));
		this.fSprites[this.rect + 3].scaleX = this.Data.handlePos.x;
	}

	public class PaletteOverridePanel : Panel, IDevUISignals {
		public PaletteOverrideData Data => (this.parentNode as PaletteOverrideRepresentation).Data;

		public bool shapeVisible = false;
		public bool colorVisible = false;
		public bool tweaksVisible = false;
		public bool paletteVisible = false;

		public string ShapeName => $"Shape: {this.Data.shape}";
		public string GradientStyleName => $"Gradient Style: {this.Data.gradientStyle}";
		public string WetTerrainName => $"Wet Terrain: {(this.Data.wetTerrain ? "Enabled" : "Disabled")}";
		public string AffectSkyName => $"Affect Sky: {(this.Data.affectSky ? "True" : "False")}";

		public PaletteOverridePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "Palette Override") {
			this.RefreshUI();
		}

		public void RefreshUI() {
			for (int num = this.subNodes.Count - 1; num > -1; num--) {
				this.subNodes[num].ClearSprites();
				this.subNodes.Pop();
			}

			this.size.y = -15f;

			if (this.paletteVisible) {
				this.subNodes.Add(new Button(this.owner, "NewFadePalette", this, new Vector2(5f, this.size.y += 20f), 100f, "New"));
				for (int i = this.Data.fadePalettes.Count - 1; i >= 0; i--) {
					this.subNodes.Add(new PaletteOverrideSlider(this.owner, $"FadePalette_{i}", this, new Vector2(5f, this.size.y += 20f), "Strength"));
					this.subNodes.Add(new PaletteController(this.owner, $"FadePalette_{i}", this, new Vector2(5f, this.size.y += 20f), $"Fade {i}"));
					this.subNodes.Add(new Button(this.owner, $"DeleteFadePalette_{i}", this, new Vector2(205f, this.size.y), 40f, "Delete"));
				}
				this.subNodes.Add(new DevUILabel(this.owner, "FadePalettes", this, new Vector2(5f, this.size.y += 20f), 240f, "Fade Palettes:"));
				this.subNodes.Add(new PaletteController(this.owner, "Palette", this, new Vector2(5f, this.size.y += 20f), "Palette"));
			}
			this.subNodes.Add(new Button(this.owner, "PaletteToggle", this, new Vector2(5f, this.size.y += 20f), 240f, "Palette"));


			if (this.tweaksVisible) {
				this.size.y += 5f;
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "Clouds", this, new Vector2(5f, this.size.y += 20f), "Clouds"));
				this.subNodes.Add(new Button(this.owner, "WetTerrain", this, new Vector2(5f, this.size.y += 20f), 200f, this.WetTerrainName));
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "Grime", this, new Vector2(5f, this.size.y += 20f), "Grime"));
			}
			this.subNodes.Add(new Button(this.owner, "TweaksToggle", this, new Vector2(5f, this.size.y += 20f), 240f, "Tweaks"));


			if (this.colorVisible) {
				this.size.y += 5f;
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "Contrast", this, new Vector2(5f, this.size.y += 20f), "Contrast"));
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "Darkness", this, new Vector2(5f, this.size.y += 20f), "Darkness"));
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "Brightness", this, new Vector2(5f, this.size.y += 20f), "Brightness"));
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "Saturation", this, new Vector2(5f, this.size.y += 20f), "Saturation"));
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "Hue", this, new Vector2(5f, this.size.y += 20f), "Hue"));
			}
			this.subNodes.Add(new Button(this.owner, "ColorToggle", this, new Vector2(5f, this.size.y += 20f), 240f, "Color"));


			if (this.shapeVisible) {
				this.size.y += 5f;
				if (this.Data.gradientStyle == GradientStyle.Directional) {
					this.subNodes.Add(new PaletteOverrideSlider(this.owner, "GradientValue", this, new Vector2(5f, this.size.y += 20f), "Direction"));
				}
				this.subNodes.Add(new Button(this.owner, "GradientStyle", this, new Vector2(5f, this.size.y += 20f), 200f, this.GradientStyleName));
				this.subNodes.Add(new Button(this.owner, "Shape", this, new Vector2(5f, this.size.y += 20f), 200f, this.ShapeName));
				if (this.Data.gradientStyle == GradientStyle.None) {
					this.subNodes.Add(new PaletteOverrideSlider(this.owner, "MaxOpacity", this, new Vector2(5f, this.size.y += 20f), "Opacity"));
				}
				else {
					this.subNodes.Add(new PaletteOverrideSlider(this.owner, "MaxOpacity", this, new Vector2(5f, this.size.y += 20f), "Max Opacity"));
					this.subNodes.Add(new PaletteOverrideSlider(this.owner, "MinOpacity", this, new Vector2(5f, this.size.y += 20f), "Min Opacity"));
				}
				this.subNodes.Add(new Button(this.owner, "AffectSky", this, new Vector2(5f, this.size.y += 20f), 200f, this.AffectSkyName));
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "ToDepth", this, new Vector2(5f, this.size.y += 20f), "To Depth"));
				this.subNodes.Add(new PaletteOverrideSlider(this.owner, "FromDepth", this, new Vector2(5f, this.size.y += 20f), "From Depth"));
			}
			this.subNodes.Add(new Button(this.owner, "ShapeToggle", this, new Vector2(5f, this.size.y += 20f), 240f, "Shape"));

			this.size.y += 20f;

			this.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			if (sender.IDstring == "ShapeToggle") {
				this.shapeVisible = !this.shapeVisible;
				this.RefreshUI();
			}
			else if (sender.IDstring == "ColorToggle") {
				this.colorVisible = !this.colorVisible;
				this.RefreshUI();
			}
			else if (sender.IDstring == "TweaksToggle") {
				this.tweaksVisible = !this.tweaksVisible;
				this.RefreshUI();
			}
			else if (sender.IDstring == "PaletteToggle") {
				this.paletteVisible = !this.paletteVisible;
				this.RefreshUI();
			}
			else if (sender.IDstring == "Shape") {
				int num = this.Data.shape.Index + 1;
				if (num >= ExtEnum<PaletteOverrideData.Shape>.values.Count) {
					num = 0;
				}
				this.Data.shape = new PaletteOverrideData.Shape(ExtEnum<PaletteOverrideData.Shape>.values.GetEntry(num));
				(sender as Button).Text = this.ShapeName;
				(this.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}
			else if (sender.IDstring == "GradientStyle") {
				int num = this.Data.gradientStyle.Index + 1;
				if (num >= ExtEnum<GradientStyle>.values.Count) {
					num = 0;
				}
				this.Data.gradientStyle = new GradientStyle(ExtEnum<GradientStyle>.values.GetEntry(num));
				this.RefreshUI();
				(this.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}
			else if (sender.IDstring == "WetTerrain") {
				this.Data.wetTerrain = !this.Data.wetTerrain;
				(sender as Button).Text = this.WetTerrainName;
				(this.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}
			else if (sender.IDstring == "AffectSky") {
				this.Data.affectSky = !this.Data.affectSky;
				(sender as Button).Text = this.AffectSkyName;
				(this.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}
			else if (sender.IDstring == "NewFadePalette") {
				this.Data.fadePalettes.Add((0, 1f));
				this.RefreshUI();
				(this.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}
			else if (sender.IDstring.StartsWith("DeleteFadePalette_")) {
				int i = int.Parse(sender.IDstring.Substring(18), NumberStyles.Any, CultureInfo.InvariantCulture);
				this.Data.fadePalettes.RemoveAt(i);
				this.RefreshUI();
				(this.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}
		}

		public class PaletteOverrideSlider : Slider {
			public PaletteOverrideSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();

				PaletteOverrideData data = (this.parentNode.parentNode as PaletteOverrideRepresentation).Data;
				switch (this.IDstring) {
					case "FromDepth":
						this.NumberText = data.fromDepth.ToString();
						this.RefreshNubPos(data.fromDepth / 29f);
						break;

					case "ToDepth":
						this.NumberText = data.toDepth.ToString();
						this.RefreshNubPos(data.toDepth / 29f);
						break;

					case "MinOpacity":
						this.NumberText = Mathf.RoundToInt(data.minOpacity * 100f).ToString();
						this.RefreshNubPos(data.minOpacity);
						break;

					case "MaxOpacity":
						this.NumberText = Mathf.RoundToInt(data.maxOpacity * 100f).ToString();
						this.RefreshNubPos(data.maxOpacity);
						break;

					case "GradientValue":
						this.NumberText = Mathf.RoundToInt(data.gradientValue * 360f).ToString();
						this.RefreshNubPos(data.gradientValue);
						break;

					case "Hue":
						this.NumberText = Mathf.RoundToInt(data.hue * 100f).ToString();
						this.RefreshNubPos(data.hue);
						break;

					case "Saturation":
						this.NumberText = Mathf.RoundToInt(data.saturation * 100f).ToString();
						this.RefreshNubPos(data.saturation);
						break;

					case "Brightness":
						this.NumberText = Mathf.RoundToInt(data.brightness * 100f).ToString();
						this.RefreshNubPos(data.brightness);
						break;

					case "Darkness":
						this.NumberText = Mathf.RoundToInt(data.darkness * 100f).ToString();
						this.RefreshNubPos(data.darkness);
						break;

					case "Contrast":
						this.NumberText = Mathf.RoundToInt(data.contrast * 100f).ToString();
						this.RefreshNubPos(data.contrast);
						break;

					case "Grime":
						this.NumberText = Mathf.RoundToInt(data.grime * 100f).ToString();
						this.RefreshNubPos(data.grime);
						break;

					case "Clouds":
						this.NumberText = Mathf.RoundToInt(data.clouds * 100f).ToString();
						this.RefreshNubPos(data.clouds);
						break;
				}
				if (this.IDstring.StartsWith("FadePalette_")) {
					int i = int.Parse(this.IDstring.Substring(12), NumberStyles.Any, CultureInfo.InvariantCulture);
					this.NumberText = Mathf.RoundToInt(data.fadePalettes[i].strength * 100f).ToString();
					this.RefreshNubPos(data.fadePalettes[i].strength);
				}
			}

			public override void NubDragged(float nubPos) {
				PaletteOverrideData data = (this.parentNode.parentNode as PaletteOverrideRepresentation).Data;
				switch (this.IDstring) {
					case "FromDepth":
						data.fromDepth = Math.Min(data.toDepth, Mathf.RoundToInt(nubPos * 29f));
						break;

					case "ToDepth":
						data.toDepth = Math.Max(data.fromDepth, Mathf.RoundToInt(nubPos * 29f));
						break;

					case "MinOpacity":
						data.minOpacity = nubPos;
						break;

					case "MaxOpacity":
						data.maxOpacity = nubPos;
						break;

					case "GradientValue":
						data.gradientValue = nubPos;
						break;

					case "Hue":
						data.hue = nubPos;
						break;

					case "Saturation":
						data.saturation = nubPos;
						break;

					case "Brightness":
						data.brightness = nubPos;
						break;

					case "Darkness":
						data.darkness = nubPos;
						break;

					case "Contrast":
						data.contrast = nubPos;
						break;

					case "Grime":
						data.grime = nubPos;
						break;

					case "Clouds":
						data.clouds = nubPos;
						break;
				}
				if (this.IDstring.StartsWith("FadePalette_")) {
					int i = int.Parse(this.IDstring.Substring(12), NumberStyles.Any, CultureInfo.InvariantCulture);
					data.fadePalettes[i] = data.fadePalettes[i] with {
						strength = nubPos,
					};
				}

				this.Refresh();
				this.parentNode.parentNode.Refresh();
				(this.parentNode.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}
		}

		public class PaletteController : IntegerControl {
			public PaletteController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title) {
			}

			public override void Increment(int change) {
				PaletteOverrideData data = (base.parentNode.parentNode as PaletteOverrideRepresentation).Data;

				if (this.IDstring == "Palette") {
					data.palette += change;
					if (data.palette < 0) {
						data.palette = 0;
					}
				}
				else if (this.IDstring.StartsWith("FadePalette_")) {
					int i = int.Parse(this.IDstring.Substring(12), NumberStyles.Any, CultureInfo.InvariantCulture);
					int pal = data.fadePalettes[i].palette;
					pal += change;
					if (pal < 0) {
						pal = 0;
					}
					data.fadePalettes[i] = data.fadePalettes[i] with {
						palette = pal,
					};
				}

				this.Refresh();
				(base.parentNode as PaletteOverridePanel).RefreshUI();
				(this.parentNode.parentNode as PaletteOverrideRepresentation).obj.Refresh();
			}

			public override void Update() {
				try {
					base.Update();
				}
				catch (Exception) {}
			}

			public override void Refresh() {
				PaletteOverrideData data = (base.parentNode.parentNode as PaletteOverrideRepresentation).Data;

				if (this.IDstring == "Palette") {
					base.NumberLabelText = data.palette.ToString();
				}
				else if (this.IDstring.StartsWith("FadePalette_")) {
					int i = int.Parse(this.IDstring[12..], NumberStyles.Any, CultureInfo.InvariantCulture);
					base.NumberLabelText = data.fadePalettes[i].palette.ToString();
				}

				base.Refresh();
			}
		}
	}
}