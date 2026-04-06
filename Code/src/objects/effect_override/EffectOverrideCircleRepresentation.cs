namespace Floodwaters.Objects;

public class EffectOverrideCircleRepresentation : ResizeableObjectRepresentation {
	public EffectOverrideData Data => this.pObj.data as EffectOverrideData;
	public EffectOverride obj;

	public EffectOverrideCircleRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, true) {
		this.obj = owner.room.updateList.OfType<EffectOverride>().FirstOrDefault(o => o.pObj == pObj);
		if (this.obj == null) {
			this.obj = new EffectOverride(owner.room, pObj);
			owner.room.AddObject(this.obj);
		}

		this.subNodes.Add(new EffectOverridePanel(owner, "EffectOverride_Panel", this, this.Data.panelPos));
		this.fSprites.Add(new FSprite("pixel") {
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
	}

	public override void Refresh() {
		base.Refresh();
		EffectOverridePanel panel = base.subNodes[base.subNodes.Count - 1] as EffectOverridePanel;
		this.Data.panelPos = panel.pos;

		base.MoveSprite(base.fSprites.Count - 1, this.absPos);
		base.fSprites[base.fSprites.Count - 1].scaleY = (this.Data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		base.fSprites[base.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(Vector2.zero, this.Data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
	}

	public class EffectOverridePanel : Panel, IDevUISignals {
		public EffectOverrideData Data => (this.parentNode as EffectOverrideCircleRepresentation).Data;

		public EffectOverridePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "Effect Override Circle") {
			this.RefreshUI();
		}

		public void RefreshUI() {
			for (int num = this.subNodes.Count - 1; num > -1; num--) {
				this.subNodes[num].ClearSprites();
				this.subNodes.Pop();
			}

			this.size.y = -15f;
			this.subNodes.Add(new Button(this.owner, "Toggle_Gradient", this, new Vector2(5f, this.panelStep), 240f, this.GradientButtonText()));

			this.subNodes.Add(new ModifySlider(this.owner, "Depth_Max", this, new Vector2(5f, this.panelStep), "To Depth"));
			this.subNodes.Add(new ModifySlider(this.owner, "Depth_Min", this, new Vector2(5f, this.panelStep), "From Depth"));

			this.AddModifySettingsB();
			this.subNodes.Add(new PaletteController(this.owner, "Effect_Color_B", this, new Vector2(5f, this.panelStep), "Effect Color B: "));
			this.AddModifySettingsA();
			this.subNodes.Add(new PaletteController(this.owner, "Effect_Color_A", this, new Vector2(5f, this.panelStep), "Effect Color A: "));
			this.size.y += 20f;

			this.Refresh();
		}

		public string GradientButtonText() {
			return "Gradient: " + Mathf.RoundToInt(this.Data.gradient) switch {
				1 => "Center",
				2 => "Edges",
				3 => "Depth",
				4 => "Center Depth",
				5 => "Edges Depth",
				_ => "None",
			};
		}

		public void AddModifySettingsA() {
			if (this.Data.colorA == -1) return;

			this.subNodes.Add(new ModifySlider(this.owner, "A_Val", this, new Vector2(5f, this.panelStep), "A Value"));
			this.subNodes.Add(new ModifySlider(this.owner, "A_Sat", this, new Vector2(5f, this.panelStep), "A Saturation"));
			this.subNodes.Add(new ModifySlider(this.owner, "A_Hue", this, new Vector2(5f, this.panelStep), "A Hue"));
		}

		public void AddModifySettingsB() {
			if (this.Data.colorB == -1) return;

			this.subNodes.Add(new ModifySlider(this.owner, "B_Val", this, new Vector2(5f, this.panelStep), "B Value"));
			this.subNodes.Add(new ModifySlider(this.owner, "B_Sat", this, new Vector2(5f, this.panelStep), "B Saturation"));
			this.subNodes.Add(new ModifySlider(this.owner, "B_Hue", this, new Vector2(5f, this.panelStep), "B Hue"));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message) {
			if (sender.IDstring == "Toggle_Gradient") {
				if (Mathf.RoundToInt(this.Data.gradient) == 5) {
					this.Data.gradient = 0;
				} else {
					this.Data.gradient++;
				}
				(sender as Button).Text = this.GradientButtonText();
			}
		}

		public class ModifySlider : Slider {
			public ModifySlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
			}

			public override void Refresh() {
				base.Refresh();

				EffectOverrideData data = (this.parentNode.parentNode as EffectOverrideCircleRepresentation).Data;
				switch (this.IDstring) {
					case "Depth_Min":
						this.NumberText = data.fromDepth.ToString();
						this.RefreshNubPos(data.fromDepth / 29f);
						break;
					case "Depth_Max":
						this.NumberText = data.toDepth.ToString();
						this.RefreshNubPos(data.toDepth / 29f);
						break;

					case "A_Hue":
						this.NumberText = Mathf.RoundToInt(data.modifyA.x * 359f).ToString();
						this.RefreshNubPos(data.modifyA.x);
						break;
					case "A_Sat":
						this.NumberText = Mathf.RoundToInt(data.modifyA.y * 200f) + "%";
						this.RefreshNubPos(data.modifyA.y);
						break;
					case "A_Val":
						this.NumberText = Mathf.RoundToInt(data.modifyA.z * 200f) + "%";
						this.RefreshNubPos(data.modifyA.z);
						break;

					case "B_Hue":
						this.NumberText = Mathf.RoundToInt(data.modifyB.x * 359f).ToString();
						this.RefreshNubPos(data.modifyB.x);
						break;
					case "B_Sat":
						this.NumberText = Mathf.RoundToInt(data.modifyB.y * 200f) + "%";
						this.RefreshNubPos(data.modifyB.y);
						break;
					case "B_Val":
						this.NumberText = Mathf.RoundToInt(data.modifyB.z * 200f) + "%";
						this.RefreshNubPos(data.modifyB.z);
						break;
				}
			}

			public override void NubDragged(float nubPos) {
				EffectOverrideData data = (this.parentNode.parentNode as EffectOverrideCircleRepresentation).Data;
				switch (this.IDstring) {
					case "Depth_Min":
						data.fromDepth = Math.Min(data.toDepth, Mathf.RoundToInt(nubPos * 29f));
						break;
					case "Depth_Max":
						data.toDepth = Math.Max(data.fromDepth, Mathf.RoundToInt(nubPos * 29f));
						break;

					case "A_Hue":
						data.modifyA.x = nubPos;
						break;
					case "A_Sat":
						data.modifyA.y = nubPos;
						break;
					case "A_Val":
						data.modifyA.z = nubPos;
						break;

					case "B_Hue":
						data.modifyB.x = nubPos;
						break;
					case "B_Sat":
						data.modifyB.y = nubPos;
						break;
					case "B_Val":
						data.modifyB.z = nubPos;
						break;
				}

				this.Refresh();
				this.parentNode.parentNode.Refresh();
				(this.parentNode.parentNode as EffectOverrideCircleRepresentation).obj.Refresh();
			}
		}

		public class PaletteController : IntegerControl {
			public PaletteController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title) {
			}

			public override void Increment(int change) {
				EffectOverrideData data = (base.parentNode.parentNode as EffectOverrideCircleRepresentation).Data;
				switch (this.IDstring) {
					case "Effect_Color_A":
						data.colorA += change;
						if (data.colorA < -1) {
							data.colorA = 0;
						}
						break;
					case "Effect_Color_B":
						data.colorB += change;
						if (data.colorB < -1) {
							data.colorB = 0;
						}
						break;
				}

				this.Refresh();
				(base.parentNode as EffectOverridePanel).RefreshUI();
				(this.parentNode.parentNode as EffectOverrideCircleRepresentation).obj.Refresh();
			}

			public override void Update() {
				try {
					base.Update();
				}
				catch (Exception) {}
			}

			public override void Refresh() {
				EffectOverrideData data = (base.parentNode.parentNode as EffectOverrideCircleRepresentation).Data;
				switch (this.IDstring) {
					case "Effect_Color_A":
						base.NumberLabelText = data.colorA.ToString();
						break;
					case "Effect_Color_B":
						base.NumberLabelText = data.colorB.ToString();
						break;
				}
				base.Refresh();
			}
		}
	}
}