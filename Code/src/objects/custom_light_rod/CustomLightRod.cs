namespace Floodwaters.Objects;

public class CustomLightRod : UpdatableAndDeletable, IDrawable {
	public PlacedObject pObj;
	public readonly List<LightVessel> lights = [];
	public Vector2 lastHandlePos;

	public CustomLightRodData Data => this.pObj.data as CustomLightRodData;

	public CustomLightRod(PlacedObject pObj, Room room) {
		this.room = room;
		this.pObj = pObj;
		this.UpdateLightAmount();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("pixel", true) {
			anchorX = 0f,
			scaleY = 4f,
			shader = rCam.game.rainWorld.Shaders["CustomDepth"]
		};
		sLeaser.sprites[1] = new FSprite("Futile_White", true) {
			anchorY = 0f,
			scaleX = 0.25f,
			shader = rCam.game.rainWorld.Shaders["CustomDepthGradient"]
		};
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("GrabShaders");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			FSprite fsprite = sLeaser.sprites[i];
			fsprite.RemoveFromContainer();
			newContatiner.AddChildAtIndex(fsprite, i);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		sLeaser.sprites[0].x = this.pObj.pos.x - camPos.x;
		sLeaser.sprites[0].y = this.pObj.pos.y - camPos.y;
		sLeaser.sprites[0].alpha = 1f - this.Data.depth;
		sLeaser.sprites[0].scaleX = this.Data.Rad;
		sLeaser.sprites[0].rotation = this.Data.handlePos.GetAngle();

		sLeaser.sprites[1].x = this.pObj.pos.x - camPos.x;
		sLeaser.sprites[1].y = this.pObj.pos.y - camPos.y;
		sLeaser.sprites[1].alpha = 1f - this.Data.depth;
		sLeaser.sprites[1].scaleY = this.Data.Rad / 16f;
		sLeaser.sprites[1].rotation = this.Data.handlePos.GetAngle() + 90f;

		float t = Mathf.Lerp(rCam.currentPalette.fogAmount / 4f, rCam.currentPalette.fogAmount, this.Data.depth / 1.1f);
		sLeaser.sprites[0].color = Color.Lerp(this.Data.color1, Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, t), (1f - rCam.room.ElectricPower) * 0.9f);
		sLeaser.sprites[1].color = Color.Lerp(this.Data.color2, Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, t), (1f - rCam.room.ElectricPower) * 0.9f);

		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
			foreach (LightVessel light in this.lights) {
				light.light.RemoveFromRoom();
			}
		}
	}

	public override void Update(bool eu) {
		base.Update(eu);

		foreach (LightVessel light in this.lights) {
			light.Update();
			light.light.color = Color.Lerp(this.Data.color1, this.Data.color2, light.progression);
		}

		if (this.Data.handlePos != this.lastHandlePos) {
			this.UpdateLightAmount();
		}
	}

	private void UpdateLightAmount() {
		int num = Custom.IntClamp((int) (this.Data.Rad / 45f), 2, 30);
		if (num == this.lights.Count) {
			return;
		}
		for (int i = 0; i < this.lights.Count; i++) {
			this.lights[i].light.Destroy();
		}
		this.lights.Clear();
		for (int j = 0; j < num; j++) {
			this.lights.Add(new LightVessel(this));
		}
		this.lastHandlePos = this.Data.handlePos;
	}

	public class LightVessel {
		public readonly CustomLightRod rod;

		public LightSource light;
		public float progression;
		public float size;
		public float speed;
		public float strength;
		public float visible;

		public PlacedObject PObj => this.rod.pObj;

		public CustomLightRodData Data => this.PObj.data as CustomLightRodData;

		public LightVessel(CustomLightRod rod) {
			this.rod = rod;
			this.Reset();
			this.progression = UnityEngine.Random.value;
			this.light = new LightSource(this.PObj.pos, false, Color.Lerp(this.rod.Data.color1, this.rod.Data.color2, this.progression), rod);
			this.rod.room.AddObject(this.light);
		}

		private void Reset() {
			this.progression = 0f;
			this.speed = Mathf.Lerp(0.5f, 2f, UnityEngine.Random.value);
			this.size = UnityEngine.Random.value;
		}

		public void Update() {
			this.strength = Mathf.InverseLerp(0.1f, 1f, Mathf.Pow(Mathf.Sin(this.progression * 3.1415927f), 0.5f));
			Vector2 vector = this.PObj.pos + this.Data.handlePos * this.progression;
			float num = 0.7f;
			if (this.rod.room.ViewedByAnyCamera(vector, 100f)) {
				num = 0f;
				for (int i = -3; i < 4; i++) {
					Vector2 coord = vector + this.Data.handlePos.normalized * (2f * i);
					if (this.rod.room.game.cameras[0].DepthAtCoordinate(coord) >= this.Data.depth) {
						num += 1f;
					}
				}
				num /= 7f;
			}
			this.visible = Mathf.Lerp(this.visible, num, Mathf.Lerp(0.2f, 0.05f, this.Data.brightness));
			this.strength *= this.visible;
			this.light.setAlpha = Mathf.Lerp(1f, 0.5f, this.size) * this.strength * this.rod.room.ElectricPower;
			this.light.setPos = vector;
			this.light.setRad = Mathf.Lerp(100f, 400f, this.size) * this.visible * Mathf.Lerp(0.2f, 1.5f, this.Data.brightness);
			this.progression = Mathf.Min(1f, this.progression + this.speed / this.Data.Rad);
			if (this.progression >= 1f) {
				this.Reset();
			}
		}
	}

	public class CustomLightRodData : PlacedObject.ResizableObjectData {
		public float brightness = 0.5f;
		public float depth;
		public Vector2 panelPos = Custom.DegToVec(120f) * 20f;
		public Color color1 = Color.red;
		public Color color2 = Color.green;

		public CustomLightRodData(PlacedObject owner) : base(owner) {
		}

		public override void FromString(string s) {
			base.FromString(s);
			try {
				string[] array = Regex.Split(s, "~");
				this.color1.r = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color1.g = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color1.b = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.brightness = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.depth = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.x = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.y = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color2.r = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color2.g = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color2.b = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 12);
			} catch (Exception) {}
		}

		public override string ToString() {
			return base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}",
				this.color1.r,
				this.color1.g,
				this.color1.b,
				this.brightness,
				this.depth,
				this.panelPos.x,
				this.panelPos.y,
				this.color2.r,
				this.color2.g,
				this.color2.b
			);
		}
	}

	public class CustomLightRodRepresentation : ResizeableObjectRepresentation {
		public CustomLightRodData Data => this.pObj.data as CustomLightRodData;

		public ControlPanel panel;

		public int line;

		public CustomLightRodRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, bool createCustomRod = true) : base(owner, IDstring, parentNode, pObj, "CustomLightRod", false) {
			this.subNodes.Add(this.panel = new ControlPanel(owner, "Custom_Light_Rod_Panel", this, (pObj.data as CustomLightRodData).panelPos));
			this.line = this.fSprites.Count;
			this.fSprites.Add(new FSprite("pixel", true) {
				anchorY = 0f
			});
			this.owner.placedObjectsContainer.AddChild(this.fSprites[this.line]);

			CustomLightRod customRod = this.owner.room.updateList.OfType<CustomLightRod>().FirstOrDefault(x => x.pObj == pObj);

			if (customRod == null && createCustomRod) {
				customRod = new CustomLightRod(pObj, owner.room);
				owner.room.AddObject(customRod);
			}
		}

		public override void Refresh() {
			base.Refresh();
			base.MoveSprite(this.line, this.absPos);
			this.Data.panelPos = this.panel.pos;
			this.fSprites[this.line].scaleY = this.panel.pos.magnitude;
			this.fSprites[this.line].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.panel.absPos);
		}

		public class ControlPanel : Panel {
			public ControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 165f), "Custom Light Rod") {
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "ColorR_Slider", this, new Vector2(5f, 145f), "Color R: "));
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "ColorG_Slider", this, new Vector2(5f, 125f), "Color G: "));
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "ColorB_Slider", this, new Vector2(5f, 105f), "Color B: "));
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "Color2R_Slider", this, new Vector2(5f, 85f), "Color2 R: "));
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "Color2G_Slider", this, new Vector2(5f, 65f), "Color2 G: "));
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "Color2B_Slider", this, new Vector2(5f, 45f), "Color2 B: "));
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "Depth_Slider", this, new Vector2(5f, 25f), "Depth: "));
				this.subNodes.Add(new CustomLightRodControlSlider(owner, "Brightness_Slider", this, new Vector2(5f, 5f), "Brightness: "));
			}

			public class CustomLightRodControlSlider : Slider {
				public CustomLightRodData Data => (this.parentNode.parentNode as CustomLightRodRepresentation).pObj.data as CustomLightRodData;

				public CustomLightRodControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
				}

				public override void NubDragged(float nubPos) {
					switch (this.IDstring) {
						case "ColorR_Slider":
							this.Data.color1.r = nubPos;
							break;
						case "ColorG_Slider":
							this.Data.color1.g = nubPos;
							break;
						case "ColorB_Slider":
							this.Data.color1.b = nubPos;
							break;
						case "Color2R_Slider":
							this.Data.color2.r = nubPos;
							break;
						case "Color2G_Slider":
							this.Data.color2.g = nubPos;
							break;
						case "Color2B_Slider":
							this.Data.color2.b = nubPos;
							break;
						case "Depth_Slider":
							this.Data.depth = nubPos;
							break;
						case "Brightness_Slider":
							this.Data.brightness = nubPos;
							break;
					}
					base.NubDragged(nubPos);
				}

				public override void Refresh() {
					base.Refresh();

					float num = 0f;
					switch (this.IDstring) {
						case "ColorR_Slider":
							num = this.Data.color1.r;
							this.NumberText = ((int) (num * 255f)).ToString();
							break;
						case "ColorG_Slider":
							num = this.Data.color1.g;
							this.NumberText = ((int) (num * 255f)).ToString();
							break;
						case "ColorB_Slider":
							num = this.Data.color1.b;
							this.NumberText = ((int) (num * 255f)).ToString();
							break;
						case "Color2R_Slider":
							num = this.Data.color2.r;
							this.NumberText = ((int) (num * 255f)).ToString();
							break;
						case "Color2G_Slider":
							num = this.Data.color2.g;
							this.NumberText = ((int) (num * 255f)).ToString();
							break;
						case "Color2B_Slider":
							num = this.Data.color2.b;
							this.NumberText = ((int) (num * 255f)).ToString();
							break;
						case "Depth_Slider":
							num = this.Data.depth;
							this.NumberText = ((int) (num * 30f)).ToString();
							break;
						case "Brightness_Slider":
							num = this.Data.brightness;
							this.NumberText = ((int) (num * 100f)).ToString() + "%";
							break;
					}

					base.RefreshNubPos(num);
				}
			}
		}
	}
}