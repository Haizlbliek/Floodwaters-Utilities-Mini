namespace Floodwaters.Objects;

public class CustomVineRepresentation : ResizeableObjectRepresentation {
	public int dotSpriteStart;
	public int dotCount;

	public ControlPanel controlPanel;
	public int controlPanelLine;

	public CustomVineData Data => this.pObj.data as CustomVineData;

	public CustomVineRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, false) {
		this.controlPanel = new ControlPanel(owner, "Custom_Vine_Control_Panel", this, this.Data.panelPos);
		this.subNodes.Add(this.controlPanel);
		this.controlPanelLine = this.fSprites.Count;
		this.fSprites.Add(new FSprite("pixel", true) {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[this.controlPanelLine]);

		this.dotSpriteStart = this.fSprites.Count;
		this.dotCount = Mathf.Max(Mathf.RoundToInt(this.Data.Rad / (10f / Mathf.Max(this.Data.density, 0.001f))), 2);

		for (int i = 0; i < this.dotCount; i++) {
			this.fSprites.Add(new FSprite("Futile_White", true) {
				shader = owner.room.game.rainWorld.Shaders["VectorCircle"]
			});
			owner.placedObjectsContainer.AddChild(this.fSprites[this.dotSpriteStart + i]);
		}
	}

	public override void Refresh() {
		base.Refresh();

		base.MoveSprite(this.controlPanelLine, this.absPos);
		this.fSprites[this.controlPanelLine].scaleY = this.controlPanel.pos.magnitude;
		this.fSprites[this.controlPanelLine].rotation = Custom.VecToDeg(this.controlPanel.pos);
		this.Data.panelPos = this.controlPanel.pos;

		int dotCount = Mathf.Max(Mathf.RoundToInt(this.Data.Rad / (10f / Mathf.Max(this.Data.density, 0.001f))), 2);
		if (dotCount > this.dotCount) {
			for (int i = this.dotCount; i < dotCount; i++) {
				this.fSprites.Add(new FSprite("Futile_White", true) {
					shader = this.owner.room.game.rainWorld.Shaders["VectorCircle"]
				});
				this.owner.placedObjectsContainer.AddChild(this.fSprites[this.dotSpriteStart + i]);
			}
		} else if (dotCount < this.dotCount) {
			for (int i = this.dotCount - 1; i >= dotCount; i--) {
				FSprite sprite = this.fSprites[this.dotSpriteStart + i];
				sprite.RemoveFromContainer();
				this.fSprites.Remove(sprite);
				this.owner.placedObjectsContainer.RemoveChild(sprite);
			}
		}
		this.dotCount = dotCount;

		for (int i = 0; i < this.dotCount; i++) {
			float t = i / (this.dotCount - 1f);
			Vector2 pos = this.absPos + this.Data.handlePos * t;
			this.MoveSprite(this.dotSpriteStart + i, pos);
			this.fSprites[this.dotSpriteStart + i].scale = 0.25f;
		}
	}

	public class ControlPanel : Panel, IDevUISignals {
		public CustomVineData Data => (this.parentNode as CustomVineRepresentation).Data;

		public Button presetSelectButton;
		public PresetSelectPanel presetSelectPanel;

		public ControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 85f), "Custom Vine") {
			this.subNodes.Add(new DevUILabel(owner, "Left_Label", this, new Vector2(5f, 65f), 110f, "Drop Left: "));
			this.subNodes.Add(new Button(owner, "Left_Button", this, new Vector2(115f, 65f), 125f, this.Data.dropLeft ? "Drop" : "Hold"));
			this.subNodes.Add(new DevUILabel(owner, "Right_Label", this, new Vector2(5f, 45f), 110f, "Drop Right: "));
			this.subNodes.Add(new Button(owner, "Right_Button", this, new Vector2(115f, 45f), 125f, this.Data.dropRight ? "Drop" : "Hold"));

			this.subNodes.Add(this.presetSelectButton = new Button(owner, "Change_Preset", this, new Vector2(5f, 25f), 240f, "Preset: " + this.Data.presetName));

			this.subNodes.Add(new CustomVineSlider(owner, "Density_Slider", this, new Vector2(5f, 5f), "Point Density: "));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			if (type != DevUISignalType.ButtonClick) {
				return;
			}

			if (sender.IDstring == "Left_Button") {
				this.Data.dropLeft = !this.Data.dropLeft;
				(sender as Button).Text = this.Data.dropLeft ? "Drop" : "Hold";
			}
			else if (sender.IDstring == "Right_Button") {
				this.Data.dropRight = !this.Data.dropRight;
				(sender as Button).Text = this.Data.dropRight ? "Drop" : "Hold";
			}
			else if (sender.IDstring == "Change_Preset") {
				if (this.presetSelectPanel != null) {
					this.subNodes.Remove(this.presetSelectPanel);
					this.presetSelectPanel.ClearSprites();
					this.presetSelectPanel = null;
					return;
				}

				this.presetSelectPanel = new PresetSelectPanel(this.owner, this, new Vector2(200f, 15f) - this.absPos);
				this.subNodes.Add(this.presetSelectPanel);
			}
			else if (sender.IDstring == "Preset_Prev_Page") {
				this.presetSelectPanel.PrevPage();
			}
			else if (sender.IDstring == "Preset_Next_Page") {
				this.presetSelectPanel.NextPage();
			}
			else {
				this.Data.presetName = sender.IDstring.ToLowerInvariant();
				this.presetSelectButton.Text = "Preset: " + this.Data.presetName;
				if (this.presetSelectPanel != null) {
					this.subNodes.Remove(this.presetSelectPanel);
					this.presetSelectPanel.ClearSprites();
					this.presetSelectPanel = null;
					return;
				}
			}
		}
	}

	public class PresetSelectPanel : Panel {
		public int currentOffset;
		public string[] presetNames;
		public int perPage;

		public PresetSelectPanel(DevUI owner, DevUINode parentNode, Vector2 pos) : base(owner, "Select_Preset_Panel", parentNode, pos, new Vector2(305f, 420f), "Select Preset") {
			string[] array = AssetManager.ListDirectory("vinepresets", false, false, false);
			List<string> list = [];
			for (int i = 0; i < array.Length; i++) {
				if (array[i].ToLowerInvariant().EndsWith(".txt")) {
					list.Add(Path.GetFileNameWithoutExtension(array[i]));
				}
			}
			this.presetNames = [.. list];

			this.currentOffset = 0;
			this.perPage = 36;
			this.PopulatePresets(this.currentOffset);
		}

		public void NextPage() {
			this.currentOffset += this.perPage;
			if (this.currentOffset > this.presetNames.Length) {
				this.currentOffset = this.perPage * (int) Mathf.Floor(this.presetNames.Length / (float) this.perPage);
			}
			this.PopulatePresets(this.currentOffset);
		}

		public void PopulatePresets(int offset) {
			this.currentOffset = offset;
			foreach (DevUINode devUINode in this.subNodes) {
				devUINode.ClearSprites();
			}
			this.subNodes.Clear();
			IntVector2 intVector = new IntVector2(0, 0);
			int num = this.currentOffset;
			while (num < this.presetNames.Length && num < this.currentOffset + this.perPage) {
				this.subNodes.Add(new Button(this.owner, this.presetNames[num], this, new Vector2(5f + intVector.x * 150f, this.size.y - 25f - 20f * intVector.y), 145f, this.presetNames[num]));
				intVector.y++;
				if (intVector.y >= (int) Mathf.Floor(this.perPage / 2f)) {
					intVector.x++;
					intVector.y = 0;
				}
				num++;
			}
			this.subNodes.Add(new Button(this.owner, "Preset_Prev_Page", this, new Vector2(5f, this.size.y - 25f - 20f * (this.perPage / 2 + 1f)), 145f, "Previous"));
			this.subNodes.Add(new Button(this.owner, "Preset_Next_Page", this, new Vector2(155f, this.size.y - 25f - 20f * (this.perPage / 2 + 1f)), 145f, "Next"));
		}

		public void PrevPage() {
			this.currentOffset -= this.perPage;
			if (this.currentOffset < 0) {
				this.currentOffset = 0;
			}
			this.PopulatePresets(this.currentOffset);
		}
	}

	public class CustomVineSlider : Slider {
		public CustomVineSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
		}

		public override void NubDragged(float nubPos) {
			CustomVineData data = (this.parentNode.parentNode as CustomVineRepresentation).Data;

			switch (this.IDstring) {
				case "Density_Slider": {
					data.density = nubPos * 4f;
					break;
				}
			}

			this.parentNode.parentNode.Refresh();
			this.Refresh();
		}

		public override void Refresh() {
			base.Refresh();
			CustomVineData data = (this.parentNode.parentNode as CustomVineRepresentation).Data;
			float num = 0f;

			switch (this.IDstring) {
				case "Density_Slider": {
					num = data.density / 4f;
					base.NumberText = data.density.ToString();
					break;
				}
			}

			base.RefreshNubPos(num);
		}
	}
}
