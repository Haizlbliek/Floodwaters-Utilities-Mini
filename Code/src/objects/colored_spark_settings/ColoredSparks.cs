namespace Floodwaters.Objects;

public class ColoredSparks : UpdatableAndDeletable {
	public PlacedObject pObj;
	public ColoredSparksData Data => this.pObj.data as ColoredSparksData;

	public List<ColoredSpark> sparks;

	public Vector2 wind;

	private int maxSparks;
	private float lastDataAmount = 0f;

	public Color SparkColor {
		get {
			Vector3 baseColor = this.Data.color;
			Vector3 variation = this.Data.colorVariation;
			return Custom.HSL2RGB(
				(baseColor.x + (Random.value * 2f - 1f) * variation.x) % 1f,
				Mathf.Clamp01(baseColor.y + (Random.value * 2f - 1f) * variation.y),
				Mathf.Clamp01(baseColor.z + (Random.value * 2f - 1f) * variation.z)
			);
		}
	}

	public ColoredSparks(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;

		this.sparks = [];
		this.Refresh();
	}

	public void Refresh() {
		this.maxSparks = Math.Max((int) (this.room.TileWidth * this.room.TileHeight * this.Data.amount), 0);
		this.lastDataAmount = this.Data.amount;
	}

	public override void Update(bool eu) {
		if (this.Data.amount != this.lastDataAmount) {
			this.Refresh();
		}

		base.Update(eu);
		for (int num = this.sparks.Count - 1; num >= 0; num--) {
			if (this.sparks[num].slatedForDeletetion) {
				this.sparks.RemoveAt(num);
			}
			else {
				this.sparks[num].vel += this.wind * 0.2f;
			}
		}

		if (this.sparks.Count < this.maxSparks) {
			this.AddSpark();
		}

		Vector2 wind = Custom.DegToVec(this.Data.direction);
		this.wind += (Custom.RNV() + wind * 0.333f) * 0.1f;
		this.wind *= 0.98f;
		this.wind = Vector2.ClampMagnitude(this.wind, 1f);
	}

	private IntVector2 SparkPosition() {
		float width = base.room.TileWidth;
		float height = base.room.TileHeight;
		Vector2 center = new Vector2(width / 2f, height / 2f);

		float rad = (this.Data.direction + this.Data.directionVariation * (Random.value * 2f - 1f)) * 360f * Mathf.Deg2Rad;
		Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
		Vector2 edgePoint = center + dir * center;

		return new IntVector2(
			Mathf.Clamp(Mathf.RoundToInt(edgePoint.x), 0, base.room.TileWidth - 1),
			Mathf.Clamp(Mathf.RoundToInt(edgePoint.y), 0, base.room.TileHeight - 1)
		);
	}

	private void AddSpark() {
		IntVector2 pos = this.SparkPosition();
		if (base.room.GetTile(pos).Solid || base.room.GetTile(pos).AnyWater) {
			return;
		}

		Vector2 vector = base.room.MiddleOfTile(pos);
		for (int i = 0; i < 10; i++) {
			if (!base.room.ViewedByAnyCamera(vector, 200f)) {
				break;
			}

			vector += Custom.DirVec(base.room.RoomRect.Center, vector) * 100f;
		}
		ColoredSpark coloredSpark = new ColoredSpark(vector, this);
		base.room.AddObject(coloredSpark);
		this.sparks.Add(coloredSpark);
	}

	public class ColoredSpark : CosmeticSprite {
		public ColoredSparks sparks;

		private Vector2 dir;
		private Vector2 lastLastPos;
		private LightSource light;
		public Color col;
		public float life;
		public float lifeTime;
		public float depth;
		public bool InPlayLayer => this.depth == 0f;


		public ColoredSpark(Vector2 pos, ColoredSparks sparks) {
			this.sparks = sparks;
			this.pos = pos;
			this.lastLastPos = pos;
			this.lastPos = pos;
			this.life = 1f;
			this.lifeTime = Mathf.Lerp(600f, 1200f, Random.value);
			this.col = this.sparks.SparkColor;
			if (Random.value < 0.4f) {
				this.depth = 0f;
			}
			else if (Random.value < 0.3f) {
				this.depth = -0.5f * Random.value;
			}
			else {
				this.depth = Mathf.Pow(Random.value, 1.5f) * 3f;
			}
		}

		public override void Update(bool eu) {
			base.vel *= 0.99f;
			Vector2 wind = Custom.DegToVec(this.sparks.Data.direction);
			base.vel += wind * 0.11f;
			base.vel += new Vector2(wind.y, -wind.x) * Custom.LerpMap(this.life, 0f, 0.5f, -0.1f, 0.05f);
			base.vel += this.dir * 0.2f;
			this.dir = (this.dir + Custom.RNV() * 0.6f).normalized;
			this.life -= 1f / this.lifeTime;
			this.lastLastPos = base.lastPos;
			base.lastPos = base.pos;
			base.pos += base.vel / (this.depth + 1f);
			if (this.InPlayLayer) {
				if (base.room.GetTile(base.pos).Solid) {
					this.life -= 0.025f;
					if (!base.room.GetTile(base.lastPos).Solid) {
						IntVector2? intVector;
						intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(base.room, base.room.GetTilePosition(base.lastPos), base.room.GetTilePosition(base.pos));
						FloatRect floatRect;
						floatRect = Custom.RectCollision(base.pos, base.lastPos, base.room.TileRect(intVector.Value).Grow(2f));
						base.pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
						float num;
						num = 0.3f;
						if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f) {
							base.vel.x = Mathf.Abs(base.vel.x) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f) {
							base.vel.x = (0f - Mathf.Abs(base.vel.x)) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f) {
							base.vel.y = Mathf.Abs(base.vel.y) * num;
						}
						else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f) {
							base.vel.y = (0f - Mathf.Abs(base.vel.y)) * num;
						}
					}
					else {
						base.pos.y = base.room.MiddleOfTile(base.pos).y + 10f;
					}
				}
				if (base.room.PointSubmerged(base.pos)) {
					base.pos.y = base.room.FloatWaterLevel(base.pos);
					this.life -= 0.025f;
				}
			}
			if (this.life < 0f || (Custom.VectorRectDistance(base.pos, base.room.RoomRect) > 100f && !base.room.ViewedByAnyCamera(base.pos, 400f))) {
				this.Destroy();
			}
			if (this.depth <= 0f && base.room.Darkness(base.pos) > 0f) {
				if (this.light == null) {
					this.light = new LightSource(base.pos, environmentalLight: false, this.col, this);
					if (ModManager.MMF) {
						this.light.noGameplayImpact = true;
					}
					base.room.AddObject(this.light);
					this.light.requireUpKeep = true;
				}
				this.light.setPos = base.pos;
				this.light.setAlpha = 0.4f * Mathf.InverseLerp(0f, 0.2f, this.life) * Mathf.InverseLerp(-0.6f, 0f, this.depth);
				this.light.setRad = 80f;
				this.light.stayAlive = true;
			}
			else {
				this.light?.Destroy();
				this.light = null;
			}
			if (!base.room.BeingViewed) {
				this.Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			if (this.depth < 0f) {
				sLeaser.sprites[0].scaleX = Custom.LerpMap(this.depth, 0f, -0.5f, 1.5f, 2f);
			}
			else if (this.depth > 0f) {
				sLeaser.sprites[0].scaleX = Custom.LerpMap(this.depth, 0f, 5f, 1.5f, 0.1f);
			}
			else {
				sLeaser.sprites[0].scaleX = 1.5f;
			}
			sLeaser.sprites[0].anchorY = 0f;
			if (this.depth > 0f) {
				sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["CustomDepthBothSides"];
				sLeaser.sprites[0].alpha = 0f;
			}
			else {
				sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
			}
			this.AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
			sLeaser.sprites[0].x = Mathf.Lerp(base.lastPos.x, base.pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(base.lastPos.y, base.pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.lastLastPos, base.lastPos, timeStacker), Vector2.Lerp(base.lastPos, base.pos, timeStacker));
			sLeaser.sprites[0].scaleY = Mathf.Max(2f, 2f + 1.1f * Vector2.Distance(Vector2.Lerp(this.lastLastPos, base.lastPos, timeStacker), Vector2.Lerp(base.lastPos, base.pos, timeStacker)));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
			if (this.depth <= 0f) {
				sLeaser.sprites[0].color = this.col;
			}
			else {
				sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, this.col, Mathf.InverseLerp(0f, 5f, this.depth));
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
			newContatiner = rCam.ReturnFContainer(this.InPlayLayer ? "Items" : "Foreground");
			sLeaser.sprites[0].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[0]);
		}
	}
}
