namespace Floodwaters.Objects;

public class DeerSkull : UpdatableAndDeletable, IDrawable {
	public DeerSkullData Data {
		get {
			return this.pObj.data as DeerSkullData;
		}
	}

	public float Rad {
		get {
			return this.Data.Rad;
		}
	}

	public int PoleSprite {
		get {
			return 0;
		}
	}

	private int FirstAntlerSprite {
		get {
			return 1;
		}
	}

	private int LastAntlerSprite {
		get {
			return this.FirstAntlerSprite + this.antlers.SpritesClaimed - 1;
		}
	}

	private int FirstAntlerDetailSprite {
		get {
			return this.LastAntlerSprite + 1;
		}
	}

	private int LastAntlerDetailSprite {
		get {
			return this.FirstAntlerDetailSprite + this.antlers.SpritesClaimed - 1;
		}
	}

	private int SkullSprite(int part) {
		return this.LastAntlerDetailSprite + 1 + part;
	}

	public DeerSkull(Room room, PlacedObject pObj) {
		this.pObj = pObj;
		this.room = room;
		this.antlerFlip = Mathf.Lerp(-0.8f, 0.8f, this.Data.handlePos.normalized.x);

		Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.Data.skullSeed);
		this.antlers = new DeerGraphics.Antlers(70f, 0.7f);
		this.antlerPos = this.pObj.pos + this.Data.handlePos + this.Data.handlePos.normalized * this.antlers.rad;
		UnityEngine.Random.state = state;

		if (!Futile.atlasManager.DoesContainAtlas("outpostSkulls")) {
			Futile.atlasManager.LoadAtlas("Atlases/outPostSkulls");
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1 + this.antlers.SpritesClaimed * 2 + 3];
		sLeaser.sprites[this.PoleSprite] = new FSprite("Futile_White", true) {
			scaleX = 0.5f,
			scaleY = this.Data.handlePos.magnitude * 2f / 16f,
			shader = rCam.game.rainWorld.Shaders["JaggedSquare"],
			alpha = 0f,
			rotation = Custom.VecToDeg(this.Data.handlePos.normalized)
		};

		int num = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(this.antlerFlip) * 4f) + 1, 1, 4);
		for (int i = 0; i < 3; i++) {
			sLeaser.sprites[this.SkullSprite(i)] = new FSprite("skull" + num.ToString() + "_" + (((this.Data.hasPaint && i == 2) ? 2 : 1) + i).ToString(), true) {
				anchorY = 0.85f,
				rotation = Custom.VecToDeg(this.Data.handlePos.normalized) + num * 2.5f * Mathf.Sign(this.antlerFlip),
				scaleX = -Mathf.Sign(this.antlerFlip)
			};
		}

		this.antlers.InitiateSprites(this.FirstAntlerSprite, sLeaser, rCam);
		this.antlers.InitiateSprites(this.FirstAntlerDetailSprite, sLeaser, rCam);
		for (int j = this.FirstAntlerDetailSprite; j <= this.LastAntlerDetailSprite; j++) {
			sLeaser.sprites[j].shader = rCam.game.rainWorld.Shaders["OutPostAntler"];
		}
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		sLeaser.sprites[this.PoleSprite].x = this.pObj.pos.x - camPos.x;
		sLeaser.sprites[this.PoleSprite].y = this.pObj.pos.y - camPos.y;
		Vector2 vector = this.pObj.pos + this.Data.handlePos;
		for (int i = 0; i < 3; i++) {
			sLeaser.sprites[this.SkullSprite(i)].x = vector.x - camPos.x;
			sLeaser.sprites[this.SkullSprite(i)].y = vector.y - camPos.y;
		}
		this.antlers.DrawSprites(this.FirstAntlerSprite, sLeaser, rCam, timeStacker, camPos, this.pObj.pos, this.antlerPos, this.antlerFlip, this.boneColor, this.boneColor);
		this.antlers.DrawSprites(this.FirstAntlerDetailSprite, sLeaser, rCam, timeStacker, camPos, this.pObj.pos, this.antlerPos, this.antlerFlip, this.paintColor, this.paintColor);

		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		Color paintFullColor = Custom.HSL2RGB(this.Data.paintHue, 1f, 0.5f);

		this.blackColor = palette.blackColor;
		this.boneColor = Color.Lerp(palette.blackColor, new Color(0.9f, 0.9f, 0.8f), Mathf.Lerp(0.9f, 0.2f, rCam.room.Darkness(this.pObj.pos)));
		this.paintColor = Color.Lerp(palette.blackColor, Color.Lerp(this.boneColor, paintFullColor, 0.9f), Mathf.Lerp(0.5f, 0.15f, rCam.room.Darkness(this.pObj.pos)));

		if (!this.Data.hasPaint)
			this.paintColor = this.boneColor;

		sLeaser.sprites[0].color = palette.blackColor;
		sLeaser.sprites[this.SkullSprite(0)].color = Color.Lerp(Color.Lerp(this.boneColor, new Color(0.6f, 0.5f, 0.1f), 0.3f), this.blackColor, Mathf.Lerp(0.6f, 1f, rCam.room.Darkness(this.pObj.pos)));
		sLeaser.sprites[this.SkullSprite(1)].color = this.boneColor;
		sLeaser.sprites[this.SkullSprite(2)].color = this.paintColor;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Midground");
		FSprite[] sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++) {
			sprites[i].RemoveFromContainer();
		}
		for (int j = this.FirstAntlerSprite; j < this.FirstAntlerSprite + this.antlers.parts.Length; j++) {
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		for (int k = this.FirstAntlerDetailSprite; k < this.FirstAntlerDetailSprite + this.antlers.parts.Length; k++) {
			newContatiner.AddChild(sLeaser.sprites[k]);
		}
		newContatiner.AddChild(sLeaser.sprites[this.PoleSprite]);
		for (int l = this.FirstAntlerSprite + this.antlers.parts.Length; l <= this.LastAntlerSprite; l++) {
			newContatiner.AddChild(sLeaser.sprites[l]);
		}
		for (int m = this.FirstAntlerDetailSprite + this.antlers.parts.Length; m <= this.LastAntlerDetailSprite; m++) {
			newContatiner.AddChild(sLeaser.sprites[m]);
		}
		for (int n = 0; n < 3; n++) {
			newContatiner.AddChild(sLeaser.sprites[this.SkullSprite(n)]);
		}
	}

	public PlacedObject pObj;

	public DeerGraphics.Antlers antlers;

	public Vector2 antlerPos;

	public float antlerFlip;

	public Color blackColor;

	public Color boneColor;

	public Color paintColor;

	public class DeerSkullRepresentation : ResizeableObjectRepresentation {
		public DeerSkullRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, "Deer Skull", false) {
			this.subNodes.Add(new DeerSkullPanel(owner, "DeerSkull_Panel", this, new Vector2(0f, 100f)) {
				pos = (pObj.data as DeerSkullData).panelPos
			});

			this.fSprites.Add(new FSprite("pixel", true) {
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
		}

		public override void Refresh() {
			base.Refresh();

			DeerSkullData data = this.pObj.data as DeerSkullData;
			DeerSkullPanel panel = this.subNodes[this.subNodes.Count - 1] as DeerSkullPanel;

			base.MoveSprite(this.fSprites.Count - 1, this.absPos);
			this.fSprites[this.fSprites.Count - 1].scaleY = panel.pos.magnitude;
			this.fSprites[this.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, panel.absPos);
			data.panelPos = panel.pos;
		}

		public class DeerSkullPanel : Panel {
			public DeerSkullPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 95f), "Deer Skull") {
				this.subNodes.Add(new DeerSkullSlider(owner, "Direction_Slider", this, new Vector2(5f, 65f), "Direction: "));
				this.subNodes.Add(new DeerSkullSlider(owner, "Skull_Seed_Slider", this, new Vector2(5f, 45f), "Skull Seed: "));
				this.subNodes.Add(new DeerSkullSlider(owner, "Paint_Hue_Slider", this, new Vector2(5f, 25f), "Paint Hue: "));
				this.subNodes.Add(new DeerSkullButton(owner, "Has_Paint", this, new Vector2(5f, 5f), "Has Paint: "));
			}

			public class DeerSkullButton : Button {
				public DeerSkullButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string text) : base(owner, IDstring, parentNode, pos, 110f, text) {
				}

				public override void Refresh() {
					base.Refresh();

					DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;
					this.Text = data.hasPaint ? "Has Paint" : "Has No Paint";
				}

				public override void Clicked() {
					base.Clicked();
					
					DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;
					data.hasPaint = !data.hasPaint;
					this.Refresh();
				}
			}

			public class DeerSkullSlider : Slider {
				public DeerSkullSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
				}

				public override void Refresh() {
					base.Refresh();
					float num = 0f;
					DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;

					switch (this.IDstring) {
						case "Direction_Slider":
							num = data.direction;
							base.NumberText = ((int) Mathf.Lerp(-100f, 100f, num)).ToString();
							break;

						case "Paint_Hue_Slider":
							num = data.paintHue;
							base.NumberText = Mathf.FloorToInt(data.paintHue * 100f).ToString();
							break;

						case "Skull_Seed_Slider":
							num = data.skullSeed / 100f;
							base.NumberText = data.skullSeed.ToString();
							break;
					}

					base.RefreshNubPos(num);
				}

				public override void NubDragged(float nubPos) {
					DeerSkullData data = (this.parentNode.parentNode as DeerSkullRepresentation).pObj.data as DeerSkullData;
					
					switch (this.IDstring) {
						case "Direction_Slider":
							data.direction = nubPos;
							break;

						case "Paint_Hue_Slider":
							data.paintHue = nubPos;
							break;

						case "Skull_Seed_Slider":
							data.skullSeed = (int) (nubPos * 100f);
							break;
					}

					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
			}
		}
	}
	
	public class DeerSkullData : PlacedObject.ResizableObjectData {
		public DeerSkullData(PlacedObject owner) : base(owner) {
			System.Random random = new System.Random();
			this.direction = (float)random.NextDouble();
			this.skullSeed = random.Next(0, 101);
			this.hasPaint = true;
			this.paintHue = (float) random.NextDouble();
		}

		public override void FromString(string s) {
			base.FromString(s);

			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.direction = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.skullSeed = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.hasPaint = bool.Parse(array[6]);
			this.paintHue = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
		}

		public override string ToString() {
			string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}", [
				this.panelPos.x,
				this.panelPos.y,
				this.direction,
				this.skullSeed,
				this.hasPaint,
				this.paintHue
			]);

			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}

		public Vector2 panelPos;
		public float direction;
		public int skullSeed;
		public bool hasPaint;
		public float paintHue;
	}
}
