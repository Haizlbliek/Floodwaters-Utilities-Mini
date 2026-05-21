namespace Floodwaters.Objects;

public class Lillypad : PhysicalObject, IDrawable, TerrainManager.ITerrain {
	public bool BurrowAllowed => false;

	public float lastHorizontalUpset;
	public float horizontalUpset;
	public Color blackColor;
	public float darkness;

	public Vector2 left;
	public Vector2 right;
	public Stalk stalk;

	public AbstractLillypad AbstrLilly => this.abstractPhysicalObject as AbstractLillypad;
	public PlacedObject pObj => this.AbstrLilly.pObj;
	public LillypadData Data => this.pObj.data as LillypadData;

	public Lillypad(AbstractLillypad abstractLillypad) : base(abstractLillypad) {
		this.bodyChunks = [
			new BodyChunk(this, 0, new Vector2(0f, 0f), 0.5f, 4f)
		];
		this.bodyChunkConnections = [];
		this.airFriction = 0.99f;
		this.gravity = 0.9f;
		this.bounce = 0.4f;
		this.surfaceFriction = 0.4f;
		this.collisionLayer = 1;
		this.waterFriction = 0.88f;
		this.buoyancy = 0f;
	}

	public override void PlaceInRoom(Room placeRoom) {
		base.PlaceInRoom(placeRoom);
		this.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
		this.stalk = new Stalk(this, this.room, this.firstChunk.pos);
		this.room.AddObject(this.stalk);
	}

	public override void Update(bool eu) {
		Creatures.Creatures.DisableBodyChunkTerrainCollision = true;
		base.Update(eu);
		Creatures.Creatures.DisableBodyChunkTerrainCollision = false;

		if (this.room.waterObject == null) return;

		this.lastHorizontalUpset = this.horizontalUpset;

		float rad = this.Data.Rad;

		this.firstChunk.pos.y = this.room.waterObject.DetailedWaterLevel(this.firstChunk.pos);
		this.firstChunk.vel.y = 0.0f;

		float heightLeft = this.room.waterObject.DetailedWaterLevel(this.firstChunk.pos + rad * Vector2.left);
		float heightRight = this.room.waterObject.DetailedWaterLevel(this.firstChunk.pos + rad * Vector2.right);

		this.left = new Vector2(-rad, heightLeft);
		this.right = new Vector2(rad, heightRight);
		this.horizontalUpset = Custom.AimFromOneVectorToAnother(this.left, this.right);
	}

	public float GetCoverage(int x, int y) {
		return 0f;
	}

	public bool ObstructsTile(int x, int y) {
		return false;
	}

	public Vector2 SnapToTerrain(Vector2 center, float radius, out Vector2 normal, Vector2? lastCenter = null) {
		Vector2 point = center - this.firstChunk.pos;

		normal = Vector2.zero;
		if (point.x < this.left.x - radius) return center;
		if (point.x > this.right.x + radius) return center;

		Vector2 dirRight = (this.right - this.left).normalized;
		Vector2 dirUp = new Vector2(-dirRight.y, dirRight.x);
		float dot = Vector2.Dot(dirUp, point);

		if (dot < 4f && dot > -12f && point.x < this.left.x - radius * 0.5f) {
			normal = -dirRight;
			return center + normal;
		}
		if (dot < 4f && dot > -12f && point.x > this.right.x + radius * 0.5f) {
			normal = dirRight;
			return center + normal;
		}

		if (dot >= -12f) {
			normal = dirUp;

			return center + dirUp * (-dot + radius);
		} else if (dot >= -24f) {
			normal = -dirUp;

			return center + dirUp * -2f;
		} else {
			return center;
		}
	}


	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[3];

		sLeaser.sprites[0] = TriangleMesh.MakeGridMesh("FWLillypad", 1);
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["CustomDepth"];

		sLeaser.sprites[1] = TriangleMesh.MakeGridMesh("FWLillypad", 1);
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["CustomDepth"];

		sLeaser.sprites[2] = TriangleMesh.MakeGridMesh("FWLillypadDetails", 1);
		sLeaser.sprites[2].shader = Custom.rainWorld.Shaders["CustomDepth"];

		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		foreach (FSprite sprite in sLeaser.sprites) {
			sprite.RemoveFromContainer();
		}
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[2]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		this.blackColor = palette.blackColor;
		this.darkness = palette.darkness;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (this.room.waterObject == null) return;

		Water.Surface surface = this.room.waterObject.GetSurface(this.firstChunk.pos);
		int leftI = surface.PreviousPoint(this.firstChunk.pos.x + this.left.x);
		int leftJ = Custom.IntClamp(leftI + 1, 0, surface.totalPoints - 1);
		float leftO = Mathf.InverseLerp(surface.points[leftI, 0].defaultPos.x + surface.points[leftI, 0].pos.x, surface.points[leftJ, 0].defaultPos.x + surface.points[leftJ, 0].pos.x, this.firstChunk.pos.x + this.left.x);
		int rightI = surface.PreviousPoint(this.firstChunk.pos.x + this.right.x);
		int rightJ = Custom.IntClamp(rightI + 1, 0, surface.totalPoints - 1);
		float rightO = Mathf.InverseLerp(surface.points[rightI, 0].defaultPos.x + surface.points[rightI, 0].pos.x, surface.points[rightJ, 0].defaultPos.x + surface.points[rightJ, 0].pos.x, this.firstChunk.pos.x + this.right.x);

		Vector2 p0 = Vector2.Lerp(surface.points[leftI, 0].DrawPos(timeStacker), surface.points[leftJ, 0].DrawPos(timeStacker), leftO) - camPos + new Vector2(0f, this.room.waterObject.cosmeticSurfaceDisplace) + Vector2.down * 3f;
		Vector2 p1 = Vector2.Lerp(surface.points[rightI, 0].DrawPos(timeStacker), surface.points[rightJ, 0].DrawPos(timeStacker), rightO) - camPos + new Vector2(0f, this.room.waterObject.cosmeticSurfaceDisplace) + Vector2.down * 3f;
		Vector2 p2 = Vector2.Lerp(Vector2.Lerp(surface.points[leftI, 0].DrawPos(timeStacker), surface.points[leftI, 1].DrawPos(timeStacker), 0.5f), Vector2.Lerp(surface.points[leftJ, 0].DrawPos(timeStacker), surface.points[leftJ, 1].DrawPos(timeStacker), 0.5f), leftO) - camPos + new Vector2(0f, this.room.waterObject.cosmeticSurfaceDisplace) + Vector2.down * 3f;
		Vector2 p3 = Vector2.Lerp(Vector2.Lerp(surface.points[rightI, 0].DrawPos(timeStacker), surface.points[rightI, 1].DrawPos(timeStacker), 0.5f), Vector2.Lerp(surface.points[rightJ, 0].DrawPos(timeStacker), surface.points[rightJ, 1].DrawPos(timeStacker), 0.5f), rightO) - camPos + new Vector2(0f, this.room.waterObject.cosmeticSurfaceDisplace) + Vector2.down * 3f;
		Vector2 d0 = Custom.ApplyDepthOnVector(p0 + Vector2.down * 2.5f, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), -5f);
		Vector2 d1 = Custom.ApplyDepthOnVector(p1 + Vector2.down * 2.5f, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), -5f);
		Vector2 d2 = Custom.ApplyDepthOnVector(p2 + Vector2.down * 2.5f, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), 20f);
		Vector2 d3 = Custom.ApplyDepthOnVector(p3 + Vector2.down * 2.5f, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), 20f);
		p0 = Custom.ApplyDepthOnVector(p0, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), -5f);
		p1 = Custom.ApplyDepthOnVector(p1, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), -5f);
		p2 = Custom.ApplyDepthOnVector(p2, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), 20f);
		p3 = Custom.ApplyDepthOnVector(p3, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), 20f);

		float darkness = this.Data.darkness < 0.5f ? Mathf.Lerp(0f, this.darkness, this.Data.darkness * 2f) : Mathf.Lerp(this.darkness, 1f, this.Data.darkness * 2f - 1f);

		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, d0);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, d1);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, d2);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, d3);
		sLeaser.sprites[0].color = Color.Lerp(this.Data.colorA, this.blackColor, darkness + 0.35f);
		sLeaser.sprites[0].alpha = 0.7f;

		(sLeaser.sprites[1] as TriangleMesh).MoveVertice(0, p0);
		(sLeaser.sprites[1] as TriangleMesh).MoveVertice(1, p1);
		(sLeaser.sprites[1] as TriangleMesh).MoveVertice(2, p2);
		(sLeaser.sprites[1] as TriangleMesh).MoveVertice(3, p3);
		sLeaser.sprites[1].color = Color.Lerp(this.Data.colorA, this.blackColor, darkness);
		sLeaser.sprites[1].alpha = 0.7f;

		(sLeaser.sprites[2] as TriangleMesh).MoveVertice(0, p0);
		(sLeaser.sprites[2] as TriangleMesh).MoveVertice(1, p1);
		(sLeaser.sprites[2] as TriangleMesh).MoveVertice(2, p2);
		(sLeaser.sprites[2] as TriangleMesh).MoveVertice(3, p3);
		sLeaser.sprites[2].color = Color.Lerp(this.Data.colorB, this.blackColor, darkness * 1.5f);
		sLeaser.sprites[2].alpha = 0.7f;

		if (this.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public class Stalk : UpdatableAndDeletable, IDrawable {
		public Stalk(Lillypad pad, Room room, Vector2 fruitPos) {
			this.pad = pad;
			this.pad.firstChunk.HardSetPosition(fruitPos);
			this.stuckPos.x = fruitPos.x;
			this.ropeLength = -1f;
			int x = room.GetTilePosition(fruitPos).x;
			int num = room.GetTilePosition(fruitPos).y;
			float waterY = room.waterObject?.DetailedWaterLevel(this.stuckPos) ?? -1f;
			if (waterY >= 0 && num < waterY) {
				num = room.GetTilePosition(new Vector2(0f, waterY)).y;
			}
			for (int i = num; i > 0; i--) {
				if (room.GetTile(x, i).Solid) {
					this.stuckPos.y = room.MiddleOfTile(x, i).y + 10f;
					this.ropeLength = Mathf.Abs(this.stuckPos.y - fruitPos.y);
					break;
				}
			}
			this.segs = new Vector2[Math.Max(1, (int)(this.ropeLength / 15f)), 3];
			for (int j = 0; j < this.segs.GetLength(0); j++) {
				float t = j / (float)(this.segs.GetLength(0) - 1);
				this.segs[j, 0] = Vector2.Lerp(this.stuckPos, fruitPos, t);
				this.segs[j, 1] = this.segs[j, 0];
			}
			this.connRad = this.ropeLength / Mathf.Pow(this.segs.GetLength(0), 1.1f);
			this.displacements = new Vector2[this.segs.GetLength(0)];
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(pad.abstractPhysicalObject.ID.RandomSeed);
			for (int k = 0; k < this.displacements.Length; k++) {
				this.displacements[k] = Custom.RNV() * 0.5f;
			}
			UnityEngine.Random.state = state;
		}

		public override void Update(bool eu) {
			base.Update(eu);
			if (this.ropeLength == -1f) {
				this.Destroy();
				return;
			}
			this.ConnectSegments(true);
			this.ConnectSegments(false);
			for (int i = 0; i < this.segs.GetLength(0); i++) {
				float num = i / (float)(this.segs.GetLength(0) - 1);
				this.segs[i, 1] = this.segs[i, 0];
				this.segs[i, 0] += this.segs[i, 2];
				this.segs[i, 2] *= 0.99f;
				this.segs[i, 2].y += 0.9f;
			}
			this.ConnectSegments(false);
			this.ConnectSegments(true);
			List<Vector2> list = [ this.stuckPos ];
			for (int j = 0; j < this.segs.GetLength(0); j++) {
				list.Add(this.segs[j, 0]);
			}
			if (this.releaseCounter > 0) {
				this.releaseCounter--;
			}
		}

		private void ConnectSegments(bool dir) {
			int num = (!dir) ? (this.segs.GetLength(0) - 1) : 0;
			bool flag = false;
			while (!flag) {
				if (num == 0) {
					if (!Custom.DistLess(this.segs[num, 0], this.stuckPos, this.connRad)) {
						Vector2 b = Custom.DirVec(this.segs[num, 0], this.stuckPos) * (Vector2.Distance(this.segs[num, 0], this.stuckPos) - this.connRad);
						this.segs[num, 0] += b;
						this.segs[num, 2] += b;
					}
				}
				else {
					if (!Custom.DistLess(this.segs[num, 0], this.segs[num - 1, 0], this.connRad)) {
						Vector2 a = Custom.DirVec(this.segs[num, 0], this.segs[num - 1, 0]) * (Vector2.Distance(this.segs[num, 0], this.segs[num - 1, 0]) - this.connRad);
						this.segs[num, 0] += a * 0.5f;
						this.segs[num, 2] += a * 0.5f;
						this.segs[num - 1, 0] -= a * 0.5f;
						this.segs[num - 1, 2] -= a * 0.5f;
					}
					if (num == this.segs.GetLength(0) - 1 && this.pad != null && !Custom.DistLess(this.segs[num, 0], this.pad.firstChunk.pos, this.connRad)) {
						Vector2 a2 = Custom.DirVec(this.segs[num, 0], this.pad.firstChunk.pos) * (Vector2.Distance(this.segs[num, 0], this.pad.firstChunk.pos) - this.connRad);
						this.segs[num, 0] += a2 * 0.75f;
						this.segs[num, 2] += a2 * 0.75f;
						this.pad.firstChunk.vel -= a2 * 0.25f;
					}
				}
				num += ((!dir) ? -1 : 1);
				if (dir && num >= this.segs.GetLength(0)) {
					flag = true;
				}
				else if (!dir && num < 0) {
					flag = true;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(this.segs.GetLength(0), false, false);
			this.AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
			Vector2 a = this.stuckPos;
			float d = 1.5f;
			for (int i = 0; i < this.segs.GetLength(0); i++) {
				float num = i / (float)(this.segs.GetLength(0) - 1);
				float num2 = Custom.LerpMap(num, 0f, 0.5f, 1f, 0f) + Mathf.Lerp(1f, 0.5f, Mathf.Sin(Mathf.Pow(num, 3.5f) * 3.1415927f));
				num2 *= this.pad.Data.Rad / 35f;
				Vector2 vector = Vector2.Lerp(this.segs[i, 1], this.segs[i, 0], timeStacker);
				if (i == this.segs.GetLength(0) - 1 && this.pad != null) {
					vector = Vector2.Lerp(this.pad.firstChunk.lastPos, this.pad.firstChunk.pos, timeStacker) + Vector2.down * 8f;
				}
				Vector2 normalized = (a - vector).normalized;
				Vector2 a2 = Custom.PerpendicularVector(normalized);
				if (i < this.segs.GetLength(0) - 1) {
					vector += (normalized * this.displacements[i].y + a2 * this.displacements[i].x) * Custom.LerpMap(Vector2.Distance(a, vector), this.connRad, this.connRad * 5f, 4f, 0f);
				}
				vector = new Vector2(Mathf.Floor(vector.x) + 0.5f, Mathf.Floor(vector.y) + 0.5f);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, a - a2 * d - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, a + a2 * d - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector - a2 * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector + a2 * num2 - camPos);
				a = vector;
				d = num2;
			}

			if (base.slatedForDeletetion) {
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
			for (int i = 0; i < sLeaser.sprites.Length; i++) {
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
			for (int i = 0; i < sLeaser.sprites.Length; i++) {
				sLeaser.sprites[i].RemoveFromContainer();
				rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[i]);
			}
		}

		public Lillypad pad;

		public Vector2 stuckPos;

		public float ropeLength;

		public Vector2[] displacements;

		public Vector2[,] segs;

		public int releaseCounter;

		private float connRad;
	}

	public class AbstractLillypad : AbstractPhysicalObject {
		public PlacedObject pObj;

		public AbstractLillypad(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, PlacedObject pObj) : base(world, Enums.Lillypad, realizedObject, pos, ID) {
			this.pObj = pObj;
		}
	}
}