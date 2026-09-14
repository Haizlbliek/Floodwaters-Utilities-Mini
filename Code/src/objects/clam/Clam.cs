namespace Floodwaters.Objects;

public class Clam : PhysicalObject, IDrawable, TerrainManager.ITerrain {
	public Vector2 pos;

	private AbstractClam AbstrClam => this.abstractPhysicalObject as AbstractClam;

	private PlacedObject.ResizableObjectData Data => this.AbstrClam.po.data as PlacedObject.ResizableObjectData;

	public bool BurrowAllowed => false;

	private float openDirection = 1f;
	private float openClose = 0f;
	private float t = 0f;
	private bool paletteDirty = false;

	public Clam(AbstractClam abstractClam) : base(abstractClam) {
		this.bodyChunks = [
			new BodyChunk(this, 0, new Vector2(1000f, 0f), this.Data.Rad, 4f)
		];
		this.bodyChunkConnections = [];
		this.airFriction = 0.85f;
		this.gravity = 0.5f;
		this.bounce = 0.4f;
		this.surfaceFriction = 0.4f;
		this.collisionLayer = 0;
		this.waterFriction = 0.85f;
		this.buoyancy = 1.1f;

		this.pos = this.AbstrClam.po.pos;
	}

	public override void Update(bool eu) {
		base.Update(eu);

		this.t += 0.01f;
		this.openClose = Mathf.Clamp01(Mathf.Cos(this.t) + 0.5f);

		if (this.pos != this.AbstrClam.po.pos) {
			this.pos = this.AbstrClam.po.pos;
			this.paletteDirty = true;
		}


		Vector2 root = this.pos - this.Data.handlePos;
		Vector2 shellDirection = this.Data.handlePos;
		Vector2 shellUp = new Vector2(this.Data.handlePos.y, -this.Data.handlePos.x);
		if (shellUp.y < 0f) {
			this.openDirection = -1f;
		}
		else {
			this.openDirection = 1f;
		}

		if (this.openClose > 0f) {
			this.room.AddObject(new Bubble(root + shellDirection * Random.value * 2f, Custom.RNV() * Mathf.Lerp(6f, 16f, Random.value) * this.openClose, false, false) {
				age = 600 - Random.Range(20, Random.Range(30, 80))
			});
		}
		else {
			foreach (AbstractCreature creature in this.room.abstractRoom.creatures) {
				if (creature.realizedCreature is not Player player)
					continue;

				if (Custom.DistLess(player.bodyChunks[0].pos, this.pos, shellDirection.magnitude - 8f) || Custom.DistLess(player.bodyChunks[1].pos, this.pos, shellDirection.magnitude - 8f)) {
					player.airInLungs = 1f;
				}
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[3];

		sLeaser.sprites[0] = new FSprite("clam_c") { anchorY = 0.75f, anchorX = 0.9f, };
		sLeaser.sprites[1] = new FSprite("clam_b") { anchorY = 0.75f, anchorX = 0.9f, };
		sLeaser.sprites[2] = new FSprite("clam_a") { anchorY = 0.25f, anchorX = 0.9f, };

		this.AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (this.paletteDirty) {
			this.paletteDirty = false;
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		Vector2 root = this.pos - this.Data.handlePos;
		Vector2 shellDirection = this.Data.handlePos;

		float scaleFactor = 24f;
		float flip = -this.openDirection;

		FSprite clamC = sLeaser.sprites[0];
		clamC.x = root.x - camPos.x;
		clamC.y = root.y - camPos.y;
		clamC.rotation = shellDirection.GetAngle();
		clamC.scaleX = shellDirection.magnitude / -scaleFactor;
		clamC.scaleY = shellDirection.magnitude / scaleFactor * flip;

		FSprite clamB = sLeaser.sprites[1];
		clamB.x = root.x - camPos.x;
		clamB.y = root.y - camPos.y;
		clamB.rotation = shellDirection.GetAngle();
		clamB.scaleX = shellDirection.magnitude / -scaleFactor;
		clamB.scaleY = shellDirection.magnitude / scaleFactor * flip;

		FSprite clamA = sLeaser.sprites[2];
		clamA.x = root.x - camPos.x;
		clamA.y = root.y - camPos.y;
		clamA.rotation = shellDirection.GetAngle() + 60f * this.openClose * this.openDirection;
		clamA.scaleX = shellDirection.magnitude / -scaleFactor;
		clamA.scaleY = shellDirection.magnitude / scaleFactor * flip;

		if (sLeaser.deleteMeNextFrame || this.slatedForDeletetion) {
			this.slatedForDeletetion = true;
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		Random.State state = Random.state;
		Random.InitState(this.AbstrClam.po.pos.GetHashCode());
		float r = Random.Range(0.6f, 0.85f);
		float g = r * Random.Range(0.7f, 0.85f);
		float b = g * Random.Range(0.4f, 0.65f);
		Color color = new Color(r, g, b);
		Random.state = state;

		foreach (FSprite sprite in sLeaser.sprites) {
			sprite.color = color;
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		FContainer background = rCam.ReturnFContainer("Background");
		FContainer foreground = rCam.ReturnFContainer("Foreground");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
		}
		background.AddChild(sLeaser.sprites[0]);
		foreground.AddChild(sLeaser.sprites[1]);
		foreground.AddChild(sLeaser.sprites[2]);
	}

	public Vector2 SnapToTerrain(Vector2 center, float radius, out Vector2 normal, Vector2? lastCenter = null) {
		Vector2 root = this.pos - this.Data.handlePos + this.Data.handlePos.normalized * 8f;
		Vector2 shellDirection = this.Data.handlePos;

		float R = shellDirection.magnitude * 2f;
		float flipSign = Mathf.Sign(-this.openDirection);

		float baseAngle = shellDirection.GetAngle();
		float angleB = baseAngle;
		float angleA = baseAngle + 60f * this.openClose * this.openDirection;

		float bowSideB = -flipSign;
		float bowSideA = flipSign;

		Vector2 closestPtA = this.GetClosestPointOnHalf(center, root, angleA, R, bowSideA);
		Vector2 closestPtB = this.GetClosestPointOnHalf(center, root, angleB, R, bowSideB);

		float distA = Vector2.Distance(center, closestPtA);
		float distB = Vector2.Distance(center, closestPtB);

		Vector2 targetClosest = distA < distB ? closestPtA : closestPtB;
		float targetDist = distA < distB ? distA : distB;

		float wallThickness = 15f;
		float collisionThreshold = radius + wallThickness;

		if (targetDist < collisionThreshold) {
			if (targetDist > 0.001f) {
				normal = (center - targetClosest).normalized;
			}
			else {
				float targetAngle = distA < distB ? angleA : angleB;
				float targetBow = distA < distB ? bowSideA : bowSideB;
				normal = Custom.RotateAroundOrigo(Vector2.up * targetBow, targetAngle);
			}

			return targetClosest + normal * collisionThreshold;
		}

		normal = Vector2.zero;
		return center;
	}

	private Vector2 GetClosestPointOnHalf(Vector2 worldCenter, Vector2 root, float angle, float R, float bowSide) {
		Vector2 localPos = worldCenter - root;
		localPos = Custom.RotateAroundOrigo(localPos, -angle);

		float a = R / 2f;
		float b = 0.65f * a;
		float cx = a;

		float pyScaled = localPos.y * (a / b);
		float dx = localPos.x - cx;
		float dy = pyScaled;

		float distToCenter = Mathf.Sqrt(dx * dx + dy * dy);
		if (distToCenter < 0.001f) {
			return root + Custom.RotateAroundOrigo(new Vector2(cx, bowSide * b), angle);
		}

		float clX = cx + dx / distToCenter * a;
		float clYScaled = dy / distToCenter * a;

		if (bowSide > 0) {
			if (clYScaled < 0) {
				clYScaled = 0f;
				clX = dx < 0 ? 0f : R;
			}
		}
		else {
			if (clYScaled > 0) {
				clYScaled = 0f;
				clX = dx < 0 ? 0f : R;
			}
		}

		float clY = clYScaled * (b / a);

		return root + Custom.RotateAroundOrigo(new Vector2(clX, clY), angle);
	}


	public bool ObstructsTile(int x, int y) {
		return false;
	}

	public float GetCoverage(int x, int y) {
		return 0f;
	}

	public class AbstractClam : AbstractPhysicalObject {
		public PlacedObject po;

		public AbstractClam(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, PlacedObject po) : base(world, type, realizedObject, pos, ID) {
			this.po = po;
		}
	}
}