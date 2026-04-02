namespace Floodwaters.Objects;

public class ColoredLantern : PlayerCarryableItem, IDrawable, IProvideWarmth {
	public Vector2 rotation;
	public Vector2 lastRotation;
	public Vector2? setRotation;
	public LightSource lightSource;
	public float[,] flicker;
	public ColoredLanternStick stick;

	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth;
	Room IProvideWarmth.loadedRoom => this.room;
	float IProvideWarmth.range => 350f;
	Vector2 IProvideWarmth.Position() => base.firstChunk.pos;

	public AbstractColoredLantern Abstr => this.abstractPhysicalObject as AbstractColoredLantern;


	public ColoredLantern(AbstractColoredLantern abstractPhysicalObject) : base(abstractPhysicalObject) {
		base.bodyChunks = [
			new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.2f),
		];
		this.bodyChunkConnections = [];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		this.bounce = 0.4f;
		this.surfaceFriction = 0.8f;
		this.collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 0.8f;
		this.flicker = new float[2, 3];
		for (int i = 0; i < this.flicker.GetLength(0); i++) {
			this.flicker[i, 0] = 1f;
			this.flicker[i, 1] = 1f;
			this.flicker[i, 2] = 1f;
		}
	}

	public override void Update(bool eu) {
		base.Update(eu);

		for (int i = 0; i < this.flicker.GetLength(0); i++) {
			this.flicker[i, 1] = this.flicker[i, 0];
			this.flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
			this.flicker[i, 0] = Custom.LerpAndTick(this.flicker[i, 0], this.flicker[i, 2], 0.05f, 0.033333335f);
			if (UnityEngine.Random.value < 0.2f) {
				this.flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
			}
			this.flicker[i, 2] = Mathf.Lerp(this.flicker[i, 2], 1f, 0.01f);
		}
		if (this.lightSource == null && !this.Abstr.dead) {
			this.lightSource = new LightSource(base.firstChunk.pos, false, this.Abstr.color2, this) {
				affectedByPaletteDarkness = 0.5f
			};
			this.room.AddObject(this.lightSource);
		}
		else if (this.lightSource != null) {
			this.lightSource.setPos = new Vector2?(base.firstChunk.pos);
			this.lightSource.setRad = new float?(250f * this.flicker[0, 0]);
			this.lightSource.setAlpha = new float?(1f);
			this.lightSource.color = this.Abstr.color2;
			if (this.lightSource.slatedForDeletetion || this.lightSource.room != this.room) {
				this.lightSource = null;
			}
		}
		this.lastRotation = this.rotation;
		if (this.stick != null) {
			base.firstChunk.pos = this.stick.po.pos;
			base.firstChunk.vel *= 0f;
			this.rotation = this.stick.Data.stickEnd.Value.normalized;
			base.firstChunk.collideWithObjects = false;
			base.firstChunk.collideWithTerrain = false;
			this.canBeHitByWeapons = false;
			return;
		}
		else {
			base.firstChunk.collideWithObjects = true;
			this.canBeHitByWeapons = true;
		}

		base.firstChunk.collideWithTerrain = this.grabbedBy.Count == 0;

		if (this.grabbedBy.Count > 0) {
			this.rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
			this.rotation.y = -Mathf.Abs(this.rotation.y);

			if (!this.Abstr.isConsumed) {
				this.Abstr.Consume();
			}
		}

		if (this.setRotation != null) {
			this.rotation = this.setRotation.Value;
			this.setRotation = null;
		}

		this.rotation = (this.rotation - Custom.PerpendicularVector(this.rotation) * ((base.firstChunk.ContactPoint.y < 0) ? 0.15f : 0.05f) * base.firstChunk.vel.x).normalized;
		if (base.firstChunk.ContactPoint.y < 0) {
			base.firstChunk.vel.x *= 0.8f;
		}
	}

	public override void PlaceInRoom(Room placeRoom) {
		base.PlaceInRoom(placeRoom);

		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
		this.rotation = Custom.RNV();
		this.lastRotation = this.rotation;
	}

	public override void HitByWeapon(Weapon weapon) {
		base.HitByWeapon(weapon);
		// TODO: Add setting to knock off pole
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) {
		base.TerrainImpact(chunk, direction, speed, firstContact);

		if (speed > 5f && firstContact) {
			Vector2 pos = base.bodyChunks[chunk].pos + direction.ToVector2() * base.bodyChunks[chunk].rad * 0.9f;

			for (int i = 0; i < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)); i++) {
				this.room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * UnityEngine.Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), Color.Lerp(this.Abstr.color1, new Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[(this.stick != null) ? 5 : 4];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A", true);
		sLeaser.sprites[1] = new FSprite("DangleFruit0B", true);
		for (int i = 0; i < 2; i++) {
			sLeaser.sprites[i].scaleX = 0.8f;
			sLeaser.sprites[i].scaleY = 0.9f;
		}
		sLeaser.sprites[2] = new FSprite("Futile_White", true) {
			shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"]
		};
		sLeaser.sprites[3] = new FSprite("Futile_White", true) {
			shader = rCam.game.rainWorld.Shaders["LightSource"]
		};
		if (this.stick != null) {
			sLeaser.sprites[4] = TriangleMesh.MakeLongMesh(this.stick.stickPositions.Length, false, false);
		}
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		this.SetColor(sLeaser);

		Vector2 currentPos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 currentRotation = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
		for (int i = 0; i < 2; i++) {
			sLeaser.sprites[i].x = currentPos.x - camPos.x;
			sLeaser.sprites[i].y = currentPos.y - camPos.y;
			sLeaser.sprites[i].rotation = Custom.VecToDeg(currentRotation);
		}

		if (this.Abstr.dead) {
			sLeaser.sprites[2].isVisible = false;
			sLeaser.sprites[3].isVisible = false;
		}
		else {
			sLeaser.sprites[2].isVisible = true;
			sLeaser.sprites[3].isVisible = true;
			sLeaser.sprites[2].x = currentPos.x /*- vector2.x * 3f*/ - camPos.x;
			sLeaser.sprites[2].y = currentPos.y /*- vector2.y * 3f*/ - camPos.y;
			sLeaser.sprites[2].scale = Mathf.Lerp(this.flicker[0, 1], this.flicker[0, 0], timeStacker) * 2f;
			sLeaser.sprites[3].x = currentPos.x /*- vector2.x * 3f*/ - camPos.x;
			sLeaser.sprites[3].y = currentPos.y /*- vector2.y * 3f*/ - camPos.y;
			sLeaser.sprites[3].scale = Mathf.Lerp(this.flicker[1, 1], this.flicker[1, 0], timeStacker) * 200f / 8f;
		}

		if (this.stick != null) {
			Vector2 stickStart = this.stick.po.pos + this.stick.Data.stickEnd.Value;
			Vector2 stickEnd = currentPos + Custom.DirVec(stickStart, this.stick.po.pos) * 25f;
			Vector2 a = stickStart + Custom.DirVec(stickEnd, stickStart) * 5f;
			float num = 1f;
			for (int j = 0; j < this.stick.stickPositions.Length; j++) {
				float t = j / (float) (this.stick.stickPositions.Length - 1);
				float num2 = Mathf.Lerp(1f + Mathf.Min(this.stick.Data.stickEnd.Value.magnitude / 190f, 3f), 0.5f, t);
				Vector2 vector5 = Vector2.Lerp(stickStart, stickEnd, t) + this.stick.stickPositions[j] * Mathf.Lerp(num2 * 0.6f, 1f, t);
				Vector2 normalized = (a - vector5).normalized;
				Vector2 a2 = Custom.PerpendicularVector(normalized);
				float d = Vector2.Distance(a, vector5) / 5f;
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4, a - normalized * d - a2 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 1, a - normalized * d + a2 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 2, vector5 + normalized * d - a2 * num2 - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 3, vector5 + normalized * d + a2 * num2 - camPos);
				a = vector5;
				num = num2;
			}
		}
		if (base.slatedForDeletetion || base.room != rCam.room) {
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void SetColor(RoomCamera.SpriteLeaser sLeaser) {
		sLeaser.sprites[0].color = this.Abstr.color1;
		sLeaser.sprites[2].color = Color.Lerp(this.Abstr.color1, new Color(1f, 1f, 1f), 0.3f);
		sLeaser.sprites[3].color = Color.Lerp(this.Abstr.color1, new Color(1.0f, 1.0f, 1.0f) * 0.333f * (this.Abstr.color1.r + this.Abstr.color1.g + this.Abstr.color1.b), 0.3f);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
		if (this.stick != null) {
			sLeaser.sprites[4].color = palette.blackColor;
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Items");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
		}

		if (this.stick != null) {
			newContatiner.AddChild(sLeaser.sprites[4]);
		}

		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[3]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
	}
}