namespace Floodwaters.Objects;

public class CloverDodder : UpdatableAndDeletable, IDrawable {
	public readonly PlacedObject pObj;
	private readonly List<Vector2> points = [];
	private readonly List<String> strings = [];
	private Vector2 lastPos = Vector2.zero;
	private float lastRad = 0f;
	public bool dirty = false;
	public bool dirtyMesh = false;
	public bool dirtyPalette = false;

	private CloverDodderData Data => this.pObj.data as CloverDodderData;

	public CloverDodder(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;

		this.SetupPoints();
	}

	public void SetupPoints() {
		this.points.Clear();
		Random.State state = Random.state;
		Random.InitState(this.pObj.pos.GetHashCode());
		for (int i = 0; i < 100; i++) {
			Vector2? point = CastRay(this.room, this.pObj.pos, Custom.RNV(), this.Data.Rad);
			if (point == null)
				continue;

			this.points.Add(point.Value);
			if (this.points.Count >= this.Data.Rad * this.Data.Rad / 100f) {
				break;
			}
		}
		this.strings.Clear();
		float minDist = 80f;
		for (int j = 0; j < this.Data.Rad / 16f * this.Data.primaryDensity; j++) {
			int a = Random.Range(0, this.points.Count);
			int b = -1;
			bool valid = false;
			for (int k = 0; k < 20; k++) {
				b = Random.Range(0, this.points.Count);
				if ((this.points[a] - this.points[b]).sqrMagnitude > minDist * minDist) {
					valid = true;
					break;
				}
			}

			if (valid)
				this.strings.Add(new String(this, Mathf.Lerp(0.8f, 2.3f, Random.value), a, b, null, null));
		}
		if (this.strings.Count > 1) {
			for (int j = 0; j < this.Data.Rad / 16f * this.Data.secondaryDensity; j++) {
				String stringA = this.strings[Random.Range(0, this.strings.Count)];
				String stringB = this.strings[Random.Range(0, this.strings.Count)];

				int a = Random.Range(0, stringA.points.Length);
				int b = -1;
				bool valid = false;
				for (int k = 0; k < 20; k++) {
					b = Random.Range(0, stringB.points.Length);
					if ((stringA.points[a] - stringB.points[b]).sqrMagnitude > minDist * minDist) {
						valid = true;
						break;
					}
				}

				if (valid)
					this.strings.Add(new String(this, Mathf.Lerp(0.5f, 1.2f, Random.value), a, b, stringA, stringB));
			}
		}
		Random.state = state;
		this.lastPos = this.pObj.pos;
		this.lastRad = this.Data.Rad;
		this.dirtyMesh = true;
	}

	public static bool IsSolid(Room room, Vector2 pos) {
		if (room.GetTile(pos).Solid)
			return true;

		if (room.terrain != null && room.terrain.Contains(pos))
			return true;

		return false;
	}

	// TODO: Add poles
	public static Vector2? CastRay(Room room, Vector2 pos, Vector2 direction, float maxRange) {
		if (maxRange <= 0f || direction == Vector2.zero || IsSolid(room, pos))
			return pos;

		float tileSize = 20f;
		IntVector2 tile = room.GetTilePosition(pos);
		int stepX = direction.x > 0 ? 1 : (direction.x < 0 ? -1 : 0);
		int stepY = direction.y > 0 ? 1 : (direction.y < 0 ? -1 : 0);
		float deltaX = stepX != 0 ? Mathf.Abs(tileSize / direction.x) : float.PositiveInfinity;
		float deltaY = stepY != 0 ? Mathf.Abs(tileSize / direction.y) : float.PositiveInfinity;
		float maxX = stepX > 0
			? ((tile.x + 1) * tileSize - pos.x) / direction.x
			: (stepX < 0 ? (tile.x * tileSize - pos.x) / direction.x : float.PositiveInfinity);
		float maxY = stepY > 0
			? ((tile.y + 1) * tileSize - pos.y) / direction.y
			: (stepY < 0 ? (tile.y * tileSize - pos.y) / direction.y : float.PositiveInfinity);

		while (true) {
			float t;

			if (maxX < maxY) {
				t = maxX;
				if (t > maxRange)
					break;
				
				tile.x += stepX;
				maxX += deltaX;
				Vector2 checkPos = pos + direction * t;
				
				if (IsSolid(room, checkPos + new Vector2(stepX * 0.01f, 0f)))
					return checkPos;
			}
			else {
				t = maxY;
				if (t > maxRange)
					break;
				
				tile.y += stepY;
				maxY += deltaY;
				Vector2 checkPos = pos + direction * t;
				
				if (IsSolid(room, checkPos + new Vector2(0f, stepY * 0.01f)))
					return checkPos;
			}
		}

		return null;
	}

	public override void Update(bool eu) {
		if (this.lastPos != this.pObj.pos || this.lastRad != this.Data.Rad || this.dirty) {
			this.SetupPoints();
			this.dirty = false;
		}

		foreach (List<PhysicalObject> pos in this.room.physicalObjects) {
			foreach (PhysicalObject po in pos) {
				foreach (BodyChunk chunk in po.bodyChunks) {
					if (chunk.vel.sqrMagnitude <= 0.05f)
						continue;

					if ((chunk.pos - this.pObj.pos).sqrMagnitude > this.Data.Rad * this.Data.Rad)
						continue;

					float minDist = chunk.rad + 8f;
					int stickies = 0;

					foreach (String str in this.strings) {
						if (Custom.DistanceToLine(chunk.pos, str.stringA == null ? this.points[str.a] : str.stringA.points[str.a], str.stringB == null ? this.points[str.b] : str.stringB.points[str.b]) > chunk.rad + 32f)
							continue;

						for (int i = 1; i < str.points.Length - 1; i++) {
							if ((str.points[i] - chunk.pos).sqrMagnitude > minDist * minDist)
								continue;

							str.velocities[i] += chunk.vel * 0.1f;
							if (stickies < 15 && this.Data.stickiness >= 0.01f) {
								chunk.vel *= Mathf.Pow(0.95f, this.Data.stickiness);
								stickies++;
							}
						}
					}

					if (stickies > 0 && Random.value < stickies / 60f) {
						this.room.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, chunk);
					}
				}
			}
		}

		foreach (String str in this.strings) {
			str.Update();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[this.strings.Count];
		for (int i = 0; i < this.strings.Count; i++) {
			sLeaser.sprites[i] = TriangleMesh.MakeLongMesh(this.strings[i].points.Length - 1, false, false);
		}
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		foreach (FSprite sprite in sLeaser.sprites) {
			sprite.RemoveFromContainer();
			(Random.value > 0.5f ? rCam.ReturnFContainer("Items") : rCam.ReturnFContainer("Background")).AddChild(sprite);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (sLeaser.sprites.Length != this.strings.Count || this.dirtyMesh) {
			sLeaser.RemoveAllSpritesFromContainer();
			this.InitiateSprites(sLeaser, rCam);
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			this.dirtyMesh = false;
			this.dirtyPalette = false;
		}
		if (this.dirtyPalette) {
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			this.dirtyPalette = false;
		}

		for (int i = 0; i < this.strings.Count; i++) {
			String str = this.strings[i];
			TriangleMesh mesh = sLeaser.sprites[i] as TriangleMesh;
			for (int j = 0; j < mesh.vertices.Length; j += 2) {
				float t = j / (mesh.vertices.Length - 2f);
				float pi = t * (str.points.Length - 1);
				int pa = Mathf.Clamp(Mathf.FloorToInt(pi), 0, str.points.Length - 2);
				float pt = Mathf.InverseLerp(pa / (str.points.Length - 1f), (pa + 1) / (str.points.Length - 1f), t);
				Vector2 a = str.points[pa];
				Vector2 b = str.points[pa + 1];
				Vector2 mid = Vector2.Lerp(a, b, pt);
				Vector2 perp = Vector2.Perpendicular(a - b).normalized * str.thickness;
				mesh.MoveVertice(j, mid + perp);
				mesh.MoveVertice(j + 1, mid - perp);
			}
			mesh.x = -camPos.x;
			mesh.y = -camPos.y;
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		int ecol = this.Data.colorType == CloverDodderData.ColorType.EffectColorB ? 1 : 0;

		if (this.Data.colorType == CloverDodderData.ColorType.Dead) {
			for (int i = 0; i < this.strings.Count; i++) {
				int x = Random.Range(0, 10);
				sLeaser.sprites[i].color = Color.Lerp(palette.texture.GetPixel(x, 3), palette.texture.GetPixel(x, 3), Random.value);
			}
		}
		else if (this.Data.colorType == CloverDodderData.ColorType.Custom) {
			for (int i = 0; i < this.strings.Count; i++) {
				sLeaser.sprites[i].color = (this.Data.color * Mathf.Lerp(0.8f, 1.3f, Random.value)) with { a = 1f };
			}
		}
		else {
			for (int i = 0; i < this.strings.Count; i++) {
				sLeaser.sprites[i].color = Color.Lerp(palette.texture.GetPixel(30, 4 - 2 * ecol), palette.texture.GetPixel(30, 5 - 2 * ecol), Random.value);
			}
		}
	}

	public class String {
		public CloverDodder owner;
		public int a;
		public int b;
		public String stringA;
		public String stringB;
		public Vector2[] points;
		public Vector2[] velocities;
		public float targetGap;
		public float thickness;

		public const float POINT_GAP = 12f;

		public String(CloverDodder owner, float thickness, int a, int b, String stringA, String stringB) {
			this.owner = owner;
			this.a = a;
			this.b = b;
			this.stringA = stringA;
			this.stringB = stringB;
			Vector2 ap = stringA == null ? this.owner.points[a] : stringA.points[a];
			Vector2 bp = stringB == null ? this.owner.points[b] : stringB.points[b];
			float verticality = Mathf.Abs((bp - ap).normalized.y);
			float tautness = Mathf.Lerp(1.2f, 0.25f, verticality);
			this.targetGap = POINT_GAP / tautness;
			this.thickness = thickness;
			this.points = new Vector2[Mathf.Max(Mathf.RoundToInt((ap - bp).magnitude / this.targetGap), 3)];
			this.velocities = new Vector2[this.points.Length];
			for (int i = 0; i < this.points.Length; i++) {
				this.points[i] = Vector2.Lerp(ap, bp, i / (this.points.Length - 1f));
			}
		}

		public void Update() {
			if (this.stringA != null)
				this.points[0] = this.stringA.points[this.a];
			if (this.stringB != null)
				this.points[this.points.Length - 1] = this.stringB.points[this.b];

			for (int i = 1; i < this.points.Length - 1; i++) {
				this.velocities[i] += Vector2.down * 0.5f;
			}
			for (int i = 1; i < this.points.Length; i++) {
				Vector2 d = this.points[i - 1] - this.points[i];
				float strength = (d.magnitude - this.targetGap) * 0.7f;
				d = d.normalized;
				this.velocities[i - 1] -= d * strength;
				this.velocities[i] += d * strength;
			}
			for (int i = 1; i < this.points.Length - 1; i++) {
				Vector2 lastPos = this.points[i];
				this.velocities[i] *= 0.9f;
				this.points[i] += this.velocities[i];

				if (this.owner.room.readyForAI && this.owner.room.aimap.getTerrainProximity(this.points[i]) < 3) {
					SharedPhysics.TerrainCollisionData terrainCollisionData = new SharedPhysics.TerrainCollisionData(this.points[i], lastPos, this.velocities[i], 2f, new IntVector2(0, 0), true);
					terrainCollisionData = SharedPhysics.VerticalCollision(this.owner.room, terrainCollisionData);
					terrainCollisionData = SharedPhysics.HorizontalCollision(this.owner.room, terrainCollisionData);
					this.points[i] = terrainCollisionData.pos;
					this.velocities[i] = terrainCollisionData.vel;
					if (terrainCollisionData.contactPoint.x != 0) {
						this.velocities[i].y *= 0.6f;
					}
					if (terrainCollisionData.contactPoint.y != 0) {
						this.velocities[i].x *= 0.6f;
					}
				}
			}
		}
	}
}