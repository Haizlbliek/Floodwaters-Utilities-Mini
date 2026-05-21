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
			sLeaser.sprites[i] = new FSprite("FWCactus" + this.lumps[i].type, true) {
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

		public float Rotation => this.relativeRot + Mathf.Cos(this.cactus.time * this.timeScale + this.timeOffset) * 5f;

		public Vector2 GlobalPos => this.parent == null ? Vector2.zero : (this.parent.GlobalPos + this.parent.OffshootPosition(this.Rotation));

		public float GlobalRot => this.parent == null ? this.Rotation : (this.Rotation + this.parent.GlobalRot);
	}
}