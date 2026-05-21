namespace Floodwaters.Objects;

public class ColoredSparks : UpdatableAndDeletable {
	public PlacedObject pObj;
	public ColoredSparksData Data => this.pObj.data as ColoredSparksData;

	public ColoredSpark[] sparks = [];
	public List<ColoredSpark> inactiveSparks = [];

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

		this.Refresh();
	}

	public void Refresh() {
		this.maxSparks = Math.Max((int) (this.room.TileWidth * this.room.TileHeight * this.Data.amount), 0);
		this.lastDataAmount = this.Data.amount;
		int lastSize = this.sparks.Length;
		for (int i = this.maxSparks; i < lastSize; i++) {
			this.inactiveSparks.Remove(this.sparks[i]);
			this.sparks[i].Destroy();
		}
		Array.Resize(ref this.sparks, this.maxSparks);
		for (int i = lastSize; i < this.maxSparks; i++) {
			ColoredSpark spark = new ColoredSpark(this);
			this.sparks[i] = spark;
			this.room.AddObject(spark);
		}
	}

	private float particleCount = 0f;

	public override void Update(bool eu) {
		if (this.Data.amount != this.lastDataAmount) {
			this.Refresh();
		}

		base.Update(eu);
		for (int num = 0; num < this.maxSparks; num++) {
			this.sparks[num].vel += this.wind * 0.2f;
		}

		float spawn = this.maxSparks * this.Data.speedScale / 900f;
		this.particleCount += spawn;
		while (this.particleCount >= 1f) {
			this.particleCount -= 1f;
			this.AddSpark();
		}

		Vector2 wind = Custom.DegToVec(this.Data.direction * 360f);
		this.wind += (Custom.RNV() + wind * 0.333f) * 0.1f;
		this.wind *= 0.98f;
		this.wind = Vector2.ClampMagnitude(this.wind, 1f);
	}

	private IntVector2 SparkPosition() {
		Vector2 center = new Vector2(base.room.TileWidth / 2f, base.room.TileHeight / 2f);

		float rad = (this.Data.direction + this.Data.directionVariation * (Random.value * 2f - 1f)) * 360f;
		Vector2 dir = Custom.DegToVec(rad + 180f);
		Vector2 edgePoint = center + dir * center;

		return new IntVector2(
			Mathf.Clamp(Mathf.RoundToInt(edgePoint.x), 0, base.room.TileWidth - 1),
			Mathf.Clamp(Mathf.RoundToInt(edgePoint.y), 0, base.room.TileHeight - 1)
		);
	}

	private void AddSpark() {
		if (this.inactiveSparks.Count == 0) return;

		IntVector2 pos = this.SparkPosition();
		Room.Tile tile = base.room.GetTile(pos);
		if (tile.Solid || tile.AnyWater) {
			return;
		}

		Vector2 vector = base.room.MiddleOfTile(pos);
		for (int i = 0; i < 10; i++) {
			if (!base.room.ViewedByAnyCamera(vector, 200f)) {
				break;
			}

			vector += Custom.DirVec(base.room.RoomRect.Center, vector) * 100f;
		}
		this.inactiveSparks[0].Activate(vector);
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
		public bool InLayer2 => this.depth < 1f && this.depth > 0f;

		public bool active = false;
		public bool activeRefresh = false;



		public void Deactivate() {
			this.active = false;
			if (!this.slatedForDeletetion) {
				this.sparks.inactiveSparks.Add(this);
			}
		}

		public void Activate(Vector2 pos) {
			if (this.slatedForDeletetion) return;

			this.active = true;
			this.activeRefresh = true;

			this.vel = Vector2.zero;
			this.dir = Vector2.zero;
			this.pos = pos;
			this.lastPos = pos;
			this.lastLastPos = pos;
			this.life = 1f;
			this.lifeTime = Mathf.Lerp(600f, 1200f, Random.value) / Mathf.Max(0.01f, this.sparks.Data.speedScale);
			this.col = this.sparks.SparkColor;

			bool d0 = this.sparks.Data.depth0;
			bool d1 = this.sparks.Data.depth1;
			bool d2 = this.sparks.Data.depth2;
			bool d3 = this.sparks.Data.depth3;
			this.depth = 0f;

			if (d0 || d1 || d2 || d3) {
				float totalWeight = (d1 ? 0.4f : 0f) + (d0 ? 0.3f : 0f) + (d2 ? 0.3f : 0f) + (d3 ? 0.3f : 0f);
				float roll = Random.value * totalWeight;

				if (d1 && roll < 0.4f) {
					this.depth = 0f;
				}
				else if (d0 && roll < (d1 ? 0.7f : 0.3f)) {
					this.depth = -0.5f * Random.value;
				}
				else if (d2 && roll < (d1 ? 0.4f : 0f) + (d2 ? 0.3f : 0f)) {
					this.depth = Mathf.Lerp(0.001f, 0.999f, Random.value);
				}
				else {
					this.depth = Mathf.Pow(Random.value, 1.5f) * 3f + 1f;
				}
			}

			this.sparks.inactiveSparks.Remove(this);
		}

		public ColoredSpark(ColoredSparks sparks) {
			this.sparks = sparks;
			this.active = false;
			this.sparks.inactiveSparks.Add(this);
		}

		public override void Update(bool eu) {
			if (this.slatedForDeletetion) {
				this.light?.Destroy();
				return;
			}

			if (!this.active) {
				this.light?.setAlpha = 0f;
				return;
			}

			float speed = this.sparks.Data.speedScale;

			base.vel *= 0.99f;
			Vector2 wind = Custom.DegToVec(this.sparks.Data.direction * 360f);
			base.vel += wind * (0.11f * speed);
			base.vel += this.dir * (0.2f * speed);
			this.dir = (this.dir + Custom.RNV() * (0.6f * speed)).normalized;
			this.life -= 1f / this.lifeTime;
			this.lastLastPos = base.lastPos;
			base.lastPos = base.pos;
			base.pos += base.vel / (this.depth * 0.9f + 0.9f) * speed;
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
			if (this.InLayer2) {
				if (base.room.GetTile(base.pos).wallbehind) {
					this.life -= 0.025f;
					if (!base.room.GetTile(base.lastPos).wallbehind) {
						IntVector2? intVector = Utils.RayTraceTilesForTerrainReturnFirstSolidBackground(base.room, base.room.GetTilePosition(base.lastPos), base.room.GetTilePosition(base.pos));
						FloatRect floatRect = Custom.RectCollision(base.pos, base.lastPos, base.room.TileRect(intVector.Value).Grow(2f));
						base.pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
						float num = 0.3f;
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
				this.Deactivate();
				return;
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
				this.Deactivate();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
			sLeaser.sprites = [
				new FSprite("pixel") {
					anchorY = 0f
				}
			];
			this.AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			FSprite sprite = sLeaser.sprites[0];

			if (!this.active) {
				sprite.x = -10000f;
				sprite.y = -10000f;
				return;
			}

			if (this.activeRefresh) {
				this.activeRefresh = false;

				sprite.alpha = 1f;
				if (this.depth < 0f) {
					sprite.scaleX = Custom.LerpMap(this.depth, 0f, -0.5f, 1.5f, 2f);
				}
				else if (this.depth > 0f) {
					sprite.scaleX = Custom.LerpMap(this.depth, 0f, 6f, 1.5f, 0.1f);
				}
				else {
					sprite.scaleX = 1.5f;
				}
				if (this.depth > 1f) {
					sprite.shader = rCam.room.game.rainWorld.Shaders["CustomDepthBothSides"];
					sprite.alpha = 0f;
				}
				else if (this.depth > 0f) {
					sprite.shader = rCam.room.game.rainWorld.Shaders["CustomDepthBothSides"];
					sprite.alpha = 0.5f;
				}
				else {
					sprite.shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
				}

				sprite.RemoveFromContainer();
				rCam.ReturnFContainer(this.InPlayLayer ? "Items" : "Foreground").AddChild(sprite);
				this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}

			Vector2 interpLastPos = Vector2.Lerp(this.lastLastPos, base.lastPos, timeStacker);
			Vector2 interpCurrentPos = Vector2.Lerp(base.lastPos, base.pos, timeStacker);

			sprite.x = interpCurrentPos.x - camPos.x;
			sprite.y = interpCurrentPos.y - camPos.y;
			
			sprite.rotation = Custom.AimFromOneVectorToAnother(interpLastPos, interpCurrentPos);
			sprite.scaleY = Mathf.Max(2f, 2f + 1.1f * Vector2.Distance(interpLastPos, interpCurrentPos));
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
			if (this.depth <= 0f) {
				sLeaser.sprites[0].color = this.col;
			}
			else if (this.depth <= 1f) {
				sLeaser.sprites[0].color = Color.Lerp(this.col, palette.skyColor, Mathf.InverseLerp(0f, 5f, this.depth));
			} else {
				sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, this.col, Mathf.InverseLerp(1f, 5f, this.depth));
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		}
	}
}
