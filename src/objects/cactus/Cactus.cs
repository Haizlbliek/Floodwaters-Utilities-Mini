namespace Floodwaters.Objects;

public class Cactus : UpdatableAndDeletable, IDrawable {
	public List<Lump> lumps;
	public Lump firstLump;
	public readonly PlacedObject pObj;

	public Vector2 previousPos;

	public float time;
	public bool createdProducts = false;
	public bool needsRefresh = false;

	public CactusData Data => this.pObj.data as CactusData;

	public Cactus(Room room, PlacedObject pObj) : base() {
		this.room = room;
		this.pObj = pObj;
		this.previousPos = this.pObj.pos;

		Random.State state = Random.state;
		Random.InitState(this.Data.seed);

		this.lumps = [];
		this.lumps.Add(this.firstLump = new Lump(Random.Range(-15f, 15f), Random.Range(0.9f, 1.2f) * this.Data.scale * this.Data.size, null, this));
		this.Grow(this.lumps[0], 100f * this.Data.size, this.Data.scale);
		this.lumps.Reverse();
		this.needsRefresh = true;

		Random.state = state;
	}

	public void Refresh() {
		Random.State state = Random.state;
		Random.InitState(this.Data.seed);

		this.lumps = [];
		this.lumps.Add(this.firstLump = new Lump(Random.Range(-15f, 15f), Random.Range(0.9f, 1.2f) * this.Data.scale * this.Data.size, null, this));
		this.Grow(this.lumps[0], 100f * this.Data.size, this.Data.scale);
		this.lumps.Reverse();
		this.needsRefresh = true;

		Random.state = state;
	}


	public void Grow(Lump lump, float chance, float scale) {
		int lumpCount = (int) Random.Range(chance / 80f, chance / 35f);

		for (int i = 0; i < lumpCount; i++) {
			Lump newLump = new Lump(Random.Range(-60f, 60f), Random.Range(0.9f, 1.2f) * Mathf.Max(chance, 20f) / 100f * scale, lump, this);
			this.lumps.Add(newLump);
			this.Grow(newLump, Random.Range(chance / 1.8f, chance / 1.2f), scale);
			lump.children.Add(newLump);
		}
	}

	public override void Update(bool eu) {
		base.Update(eu);

		this.time += 0.005f;

		if (this.pObj.pos != this.previousPos) {
			this.previousPos = this.pObj.pos;
			this.Refresh();
		}

		if (this.createdProducts)
			return;

		this.createdProducts = true;

		if (this.Data.productType == 2 || this.Data.productType == 3) {
			foreach (Lump lump in this.lumps) {
				if (lump.children.Count == 0) {
					int fruitCount = Random.Range(0, 3);

					for (int i = 0; i < fruitCount; i++) {
						float fruitRotation = Random.Range(-60f, 60f);
						Vector2 fruitPosition = lump.OffshootPosition(fruitRotation) + lump.GlobalPos + this.pObj.pos;

						AbstractPhysicalObject fruit = new CactusFruit.AbstractCactusFruit(this.room.world, null, this.room.GetWorldCoordinate(fruitPosition), this.room.game.GetNewID());
						this.room.abstractRoom.entities.Add(fruit);
						fruit.Realize();
						(fruit.realizedObject as CactusFruit).Stuck(fruitPosition, Custom.DegToVec(lump.GlobalRot + fruitRotation));
						fruit.realizedObject.PlaceInRoom(this.room);
					}
				}
			}

			if (Random.Range(0, 12) == 0) {
				AbstractPhysicalObject fruit = new CactusFruit.AbstractCactusFruit(this.room.world, null, this.room.GetWorldCoordinate(this.pObj.pos + Custom.RNV() * 40f), this.room.game.GetNewID());
				this.room.abstractRoom.entities.Add(fruit);
				fruit.Realize();
				fruit.realizedObject.PlaceInRoom(this.room);
			}
		}

		if ((this.Data.productType == 1 || this.Data.productType == 3) && Random.Range(0, 4) == 0) {
			float angle = Random.Range(50f, 90f) * (Random.Range(0.0f, 1.0f) > 0.5f ? 1f : -1f);
			AbstractPhysicalObject spear = new CactusSpear.AbstractCactusSpear(this.room.world, null, this.room.GetWorldCoordinate(this.pObj.pos), this.room.game.GetNewID());
			this.room.abstractRoom.entities.Add(spear);
			spear.Realize();
			(spear.realizedObject as CactusSpear).Stuck(this.firstLump.OffshootPosition(angle) + this.pObj.pos, Custom.DegToFloat2(angle));
			spear.realizedObject.PlaceInRoom(this.room);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[this.lumps.Count];

		for (int i = 0; i < this.lumps.Count; i++) {
			sLeaser.sprites[i] = new FSprite("cactus" + this.lumps[i].type, true) {
				anchorX = 0.5f,
				anchorY = 0.0f
			};
		}

		this.AddToContainer(sLeaser, rCam, null);
		this.needsRefresh = false;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (sLeaser.sprites.Length != this.lumps.Count || this.needsRefresh) {
			foreach (FSprite sprite in sLeaser.sprites) {
				sprite.RemoveFromContainer();
			}

			this.InitiateSprites(sLeaser, rCam);
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		for (int i = 0; i < this.lumps.Count; i++) {
			Lump lump = this.lumps[i];

			Vector2 pos = lump.GlobalPos;
			sLeaser.sprites[i].x = this.pObj.pos.x - camPos.x + pos.x;
			sLeaser.sprites[i].y = this.pObj.pos.y - camPos.y + pos.y;
			sLeaser.sprites[i].rotation = lump.Rotation;
			sLeaser.sprites[i].scale = lump.size;
			sLeaser.sprites[i].color = lump.color;
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[i]);
		}
	}

	public class Lump {
		public readonly float relativeRot;
		public readonly float size;

		public readonly Lump parent;
		public readonly Cactus cactus;

		// Random self-based
		public readonly int type;
		public readonly Color color;
		public readonly float timeOffset;
		public readonly float timeScale;

		public List<Lump> children = [];

		private float EasedRandom(float min, float max) {
			return Mathf.SmoothStep(min, max, Random.Range(0f, 1f));
		}

		public Lump(float rot, float size, Lump parent, Cactus cactus) {
			this.relativeRot = rot;
			this.size = size;

			this.parent = parent;
			this.cactus = cactus;

			this.type = Random.Range(0, 4);
			Vector3 hsl = Custom.RGB2HSL(new Color(this.EasedRandom(0.40f, 0.58f), 0.61f, this.EasedRandom(0.36f, 0.52f)));
			hsl.x += this.cactus.Data.hueOffset;
			hsl.y += this.cactus.Data.satOffset;
			hsl.z += this.cactus.Data.valOffset;

			this.color = Custom.HSL2RGB((hsl.x + 1f) % 1f, Mathf.Clamp01(hsl.y), Mathf.Clamp01(hsl.z));
			this.timeOffset = Random.Range(0f, 360f);
			this.timeScale = Random.Range(0.5f, 2f);
		}

		public Vector2 ApplyRotation(Vector2 pos) {
			return Custom.rotateVectorDeg(pos, this.relativeRot);
		}

		public Vector2 OffshootPosition(float rotation) {
			return this.size * (Custom.DegToVec(this.Rotation) * 18f + Custom.DegToVec(rotation) * 5f);
		}

		public float Rotation {
			get {
				return this.relativeRot + Mathf.Cos(this.cactus.time * this.timeScale + this.timeOffset) * 5f;
			}
		}

		public Vector2 GlobalPos {
			get {
				return this.parent == null ? Vector2.zero : (this.parent.GlobalPos + this.parent.OffshootPosition(this.Rotation));
			}
		}

		public float GlobalRot {
			get {
				return this.parent == null ? this.Rotation : (this.Rotation + this.parent.GlobalRot);
			}
		}
	}

	public class CactusRepresentation : PlacedObjectRepresentation {
		public Cactus cactus;

		public CactusRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
			this.subNodes.Add(new CactusPanel(owner, "Cactus_Panel", this, new Vector2(0f, 100f)) {
				pos = (pObj.data as CactusData).panelPos
			});
			this.cactus = (Cactus) owner.room.updateList.FirstOrDefault(obj => obj is Cactus cactus && cactus.pObj == pObj);
			if (this.cactus == null) {
				this.cactus = new Cactus(owner.room, pObj);
				owner.room.AddObject(this.cactus);
			}
		}

		public override void Refresh() {
			base.Refresh();

			CactusData data = this.pObj.data as CactusData;
			data.panelPos = (this.subNodes[0] as Panel).pos;
			this.cactus.Refresh();
		}

		public class CactusPanel : Panel {
			public CactusPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 145f), "Cactus") {
				this.subNodes.Add(new CactusSlider(owner, "Scale_Slider", this, new Vector2(5f, 125f), "Scale:"));
				this.subNodes.Add(new CactusSlider(owner, "Size_Slider", this, new Vector2(5f, 105f), "Size:"));
				this.subNodes.Add(new CactusSlider(owner, "Seed_Slider", this, new Vector2(5f, 85f), "Seed:"));
				this.subNodes.Add(new CactusSlider(owner, "Hue_Slider", this, new Vector2(5f, 65f), "Hue:"));
				this.subNodes.Add(new CactusSlider(owner, "Sat_Slider", this, new Vector2(5f, 45f), "Sat:"));
				this.subNodes.Add(new CactusSlider(owner, "Val_Slider", this, new Vector2(5f, 25f), "Val:"));
				this.subNodes.Add(new CactusSlider(owner, "Product_Slider", this, new Vector2(5f, 5f), "Prod:"));
			}
	
			public override void Move(Vector2 newPos) {
				base.Move(newPos);
	
				this.parentNode.Refresh();
			}
	
			public class CactusSlider : Slider {
				public CactusSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
				}
	
				public override void Refresh() {
					base.Refresh();
	
					CactusData data = (this.parentNode.parentNode as CactusRepresentation).pObj.data as CactusData;
	
					if (this.IDstring == "Seed_Slider") {
						this.NumberText = data.seed.ToString();
						this.RefreshNubPos(data.seed / 10000f);
					}
					else if (this.IDstring == "Size_Slider") {
						this.NumberText = ((int) (data.size * 100f)).ToString() + "%";
						this.RefreshNubPos(Mathf.InverseLerp(0.5f, 2.0f, data.size));
					}
					else if (this.IDstring == "Scale_Slider") {
						this.NumberText = ((int) (data.scale * 100f)).ToString() + "%";
						this.RefreshNubPos(Mathf.InverseLerp(0.5f, 2.0f, data.scale));
					}
					else if (this.IDstring == "Hue_Slider") {
						this.NumberText = ((int) (data.hueOffset * 100f)).ToString();
						this.RefreshNubPos(data.hueOffset * 0.5f + 0.5f);
					}
					else if (this.IDstring == "Sat_Slider") {
						this.NumberText = ((int) (data.satOffset * 400f)).ToString();
						this.RefreshNubPos(data.satOffset * 2f + 0.5f);
					}
					else if (this.IDstring == "Val_Slider") {
						this.NumberText = ((int) (data.valOffset * 400f)).ToString();
						this.RefreshNubPos(data.valOffset * 2f + 0.5f);
					}
					else if (this.IDstring == "Product_Slider") {
						if (data.productType == 0)
							this.NumberText = "None";
						if (data.productType == 1)
							this.NumberText = "Spear";
						if (data.productType == 2)
							this.NumberText = "Fruit";
						if (data.productType == 3)
							this.NumberText = "Both";
						this.RefreshNubPos(data.productType / 3f);
					}
				}
	
				public override void NubDragged(float nubPos) {
					CactusData data = (this.parentNode.parentNode as CactusRepresentation).pObj.data as CactusData;
	
					if (this.IDstring == "Seed_Slider") {
						data.seed = (int) (nubPos * 10000f);
					}
					else if (this.IDstring == "Size_Slider") {
						data.size = Mathf.Lerp(0.5f, 2.0f, nubPos);
					}
					else if (this.IDstring == "Scale_Slider") {
						data.scale = Mathf.Lerp(0.5f, 2.0f, nubPos);
					}
					else if (this.IDstring == "Hue_Slider") {
						data.hueOffset = nubPos * 2f - 1f;
					}
					else if (this.IDstring == "Sat_Slider") {
						data.satOffset = (nubPos - 0.5f) * 0.5f;
					}
					else if (this.IDstring == "Val_Slider") {
						data.valOffset = (nubPos - 0.5f) * 0.5f;
					}
					else if (this.IDstring == "Product_Slider") {
						data.productType = (int) (nubPos * 4f);
					}
	
					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
			}
		}
	}

	public class CactusData : PlacedObject.Data {
		public CactusData(PlacedObject owner) : base(owner) {
			this.seed = Random.Range(0, 10001);
			this.size = Random.Range(0.95f, 1.1f);
			this.scale = Random.Range(0.9f, 1.2f);
			this.hueOffset = 0.0f;
			this.satOffset = 0.0f;
			this.valOffset = 0.0f;
			this.productType = Random.Range(0, 3);
		}

		protected string BaseSaveString() {
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}", [
				this.panelPos.x,
				this.panelPos.y,
				this.seed,
				this.size,
				this.scale,
				this.hueOffset,
				this.satOffset,
				this.valOffset,
				this.productType
			]);
		}

		public override string ToString() {
			string text = this.BaseSaveString();
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}

		public override void FromString(string s) {
			string[] array = Regex.Split(s, "~");
			this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			this.seed = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (array.Length >= 5) {
				this.size = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.scale = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array.Length >= 9) {
				this.hueOffset = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.satOffset = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.valOffset = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.productType = int.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			} else {
				this.productType = 3;
			}

			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 9);
		}

		public Vector2 panelPos;
		public int seed;
		public float size;
		public float scale;
		public float hueOffset;
		public float satOffset;
		public float valOffset;
		public int productType;
	}
}