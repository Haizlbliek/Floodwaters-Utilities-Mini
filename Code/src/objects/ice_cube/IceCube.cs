namespace Floodwaters.Objects;

public class IceCube : PhysicalObject, IDrawable, TerrainManager.ITerrain {
	public AbstractIceCube Abstr => this.abstractPhysicalObject as AbstractIceCube;
	public PlacedObject PObj => this.Abstr.pObj;
	public PlacedObject.ResizableObjectData Data => this.PObj.data as PlacedObject.ResizableObjectData;

	public Vector2 left;
	public Vector2 right;
	public float lastRot = 0f;
	public float rot = 0f;
	public float rotPush = 0f;

	public Vector2 lastHandlePos;
	public Color snowColor;

	public IceCube(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject) {
		this.bodyChunks = [
			new BodyChunk(this, 0, new Vector2(0f, 0f), this.Data.Rad, 4f)
		];
		this.bodyChunkConnections = [];
		this.airFriction = 0.85f;
		this.gravity = 0.5f;
		this.bounce = 0.4f;
		this.surfaceFriction = 0.4f;
		this.collisionLayer = 0;
		this.waterFriction = 0.85f;
		this.buoyancy = 1.1f;
	}

	public bool BurrowAllowed => false;

	public override void PlaceInRoom(Room placeRoom) {
		base.PlaceInRoom(placeRoom);
		this.firstChunk.HardSetPosition(this.Abstr.pObj.pos);
	}

	public override void Update(bool eu) {
		Creatures.Creatures.DisableBodyChunkTerrainCollision = true;
		base.Update(eu);
		Creatures.Creatures.DisableBodyChunkTerrainCollision = false;

		if (this.room.waterObject == null) return;

		float rad = this.Data.Rad;
		this.firstChunk.rad = rad;

		float heightLeft = this.room.waterObject.DetailedWaterLevel(this.firstChunk.pos + rad * Vector2.left);
		float heightRight = this.room.waterObject.DetailedWaterLevel(this.firstChunk.pos + rad * Vector2.right);
		this.left = new Vector2(-rad, heightLeft);
		this.right = new Vector2(rad, heightRight);
		float push = this.rotPush * 0.1f;
		this.left = Custom.RotateAroundOrigo(this.left, push);
		this.right = Custom.RotateAroundOrigo(this.right, push);

		float rotTo = (this.right - this.left).GetAngle();
		this.lastRot = this.rot;
		this.rot = Mathf.LerpAngle(this.rot, rotTo, 0.1f);
		this.rotPush = Mathf.Lerp(this.rotPush, 0f, 0.5f);
	}

	public Color GetColor(float lightness, RoomCamera rCam) {
		return Color.Lerp(
			Color.Lerp(rCam.currentPalette.waterSurfaceColor1, rCam.currentPalette.waterSurfaceColor2, lightness * 2f),
			this.snowColor,
			lightness * 2f - 1.0f
		);
	}

	public FSprite CreateIsland(float rad) {
		TriangleMesh mesh = TriangleMesh.MakeLongMesh(Mathf.Max(Mathf.RoundToInt(rad / 6f), 2), false, true);
		float wasOffset = 0f;
		float spikeRand = Random.Range(0.8f, 1.5f);
		for (int i = 0; i < mesh.triangles.Length / 2; i += 2) {
			float left = i / (float) mesh.triangles.Length * 2f;
			float right = (i + 1) / (float) mesh.triangles.Length * 2f;
			Vector2 xL = new Vector2(Mathf.Lerp(-1f, 1f, left), 0f);
			Vector2 xR = new Vector2(Mathf.Lerp(-1f, 1f, right), 0f);
			wasOffset = Mathf.Lerp(wasOffset, Random.Range(-0.025f, 0.1f), 0.5f);
			spikeRand = Mathf.Lerp(spikeRand, Random.Range(0.8f, 1.5f), 0.33f);
			mesh.MoveVertice(i * 2 + 0, xL + Vector2.up * wasOffset * Mathf.Sin(Mathf.Acos(left * 2f - 1f)));
			mesh.MoveVertice(i * 2 + 1, xL + Vector2.down * (-0.5f + Mathf.Sin(Mathf.Acos(left * 1.5f - 0.75f))) * spikeRand);
			wasOffset = Mathf.Lerp(wasOffset, Random.Range(-0.025f, 0.1f), 0.5f);
			spikeRand = Mathf.Lerp(spikeRand, Random.Range(0.8f, 1.5f), 0.33f);
			mesh.MoveVertice(i * 2 + 2, xR + Vector2.up * wasOffset * Mathf.Sin(Mathf.Acos(right * 2f - 1f)));
			mesh.MoveVertice(i * 2 + 3, xR + Vector2.down * (-0.5f + Mathf.Sin(Mathf.Acos(right * 1.5f - 0.75f))) * spikeRand);
		}
		mesh._isAlphaDirty = true;
		return mesh;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = this.CreateIsland(this.Data.Rad);

		this.lastHandlePos = this.Data.handlePos;

		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Midground");

		foreach (FSprite sprite in sLeaser.sprites) {
			sprite.RemoveFromContainer();
			newContatiner.AddChild(sprite);
		}
	}

	public void ApplyIslandColor(FSprite sprite, RoomCamera rCam, float bottomLightness) {
		TriangleMesh mesh = sprite as TriangleMesh;
		for (int i = 0; i < mesh.triangles.Length / 2; i += 2) {
			float left = i / (float) mesh.triangles.Length * 2f;
			float right = (i + 1) / (float) mesh.triangles.Length * 2f;
			mesh.verticeColors[i * 2 + 0] = this.GetColor(1f, rCam);
			mesh.verticeColors[i * 2 + 1] = this.GetColor(1f - (1f - bottomLightness) * Mathf.Sin(Mathf.Acos(left * 1.5f - 0.75f)), rCam);
			mesh.verticeColors[i * 2 + 2] = this.GetColor(1f, rCam);
			mesh.verticeColors[i * 2 + 3] = this.GetColor(1f - (1f - bottomLightness) * Mathf.Sin(Mathf.Acos(right * 1.5f - 0.75f)), rCam);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		Color snow = palette.texture.GetPixel(6, 1);
		Color snowLight = palette.texture.GetPixel(6, 9);
		this.snowColor = Color.Lerp(Color.Lerp(snow, snowLight, 0.75f), Color.white, 0.75f);
		this.ApplyIslandColor(sLeaser.sprites[0], rCam, 0f);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (this.lastHandlePos != this.Data.handlePos) {
			foreach (FSprite sprite in sLeaser.sprites) {
				sprite.RemoveFromContainer();
			}
			this.InitiateSprites(sLeaser, rCam);
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		sLeaser.sprites[0].SetPosition(Vector2.Lerp(this.firstChunk.lastPos, this.firstChunk.pos, timeStacker) - camPos);
		sLeaser.sprites[0].scale = this.firstChunk.rad;
		sLeaser.sprites[0].rotation = this.rot;

		if (sLeaser.deleteMeNextFrame || this.slatedForDeletetion) {
			this.slatedForDeletetion = true;
			sLeaser.CleanSpritesAndRemove();
		}
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

		if (dot > radius) {
			return center;
		}

		if (dot < 4f && dot > -12f && point.x < this.left.x - radius * 0.5f) {
			normal = -dirRight;
			return center + normal;
		}
		if (dot < 4f && dot > -12f && point.x > this.right.x + radius * 0.5f) {
			normal = dirRight;
			return center + normal;
		}

		Vector2 circlePoint = point;
		circlePoint.y *= 2f;
		if (point.y < radius && (circlePoint.magnitude < radius + this.firstChunk.rad - 10f)) {
			normal = dirUp;
			this.rotPush += point.x / this.Data.Rad;

			return center + dirUp * (-dot + radius);
		} else if (circlePoint.magnitude < (radius + this.firstChunk.rad) && point.y < 0f) {
			normal = circlePoint.normalized;

			return circlePoint.normalized * (this.firstChunk.rad + radius) * new Vector2(1f, 0.5f) + this.firstChunk.pos;
		} else {
			return center;
		}
	}


	public class AbstractIceCube : AbstractPhysicalObject {
		public PlacedObject pObj;

		public AbstractIceCube(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, PlacedObject pObj) : base(world, Enums.IceCube, realizedObject, pos, ID) {
			this.pObj = pObj;
		}
	}
}