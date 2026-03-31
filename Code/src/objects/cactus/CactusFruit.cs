namespace Floodwaters.Objects;

public class CactusFruit : PlayerCarryableItem, IDrawable, IPlayerEdible {
	public Vector2 lastRotation;
	public Vector2 rotation;
	public float darkness;
	public float lastDarkness;

	private bool stuck;
	private Vector2 stuckPos;
	private Vector2 stuckRot;

	public int bites = 2;

	public CactusFruit(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject) {
		base.bodyChunks = [
			new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.2f)
		];
		this.bodyChunkConnections = [];

		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		this.bounce = 0.2f;
		this.surfaceFriction = 0.7f;
		this.collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
	}


	public int BitesLeft => this.bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public override void Update(bool eu) {
		base.Update(eu);

		if (this.room.game.devToolsActive && Input.GetKey("b")) {
			this.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		this.lastRotation = this.rotation;
		if (this.grabbedBy.Count > 0) {
			this.rotation = Custom.PerpendicularVector(Custom.DirVec(this.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
			this.rotation.y = Mathf.Abs(this.rotation.y);
		}
		if (this.firstChunk.ContactPoint.y < 0) {
			this.rotation = (this.rotation - Custom.PerpendicularVector(this.rotation) * 0.1f * this.firstChunk.vel.x).normalized;
			this.firstChunk.vel.x *= 0.8f;
		}

		if (this.stuck) {
			if (this.grabbedBy.Count > 0) {
				this.stuck = false;
			}
			else {
				this.firstChunk.vel = Vector2.zero;
				this.firstChunk.pos = this.stuckPos;
				this.rotation = this.stuckRot;
				this.lastRotation = this.stuckRot;
			}
		}
		this.firstChunk.collideWithObjects = !this.stuck;
	}

	public override void PlaceInRoom(Room placeRoom) {
		base.PlaceInRoom(placeRoom);

		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
		this.rotation = Custom.RNV();
		this.lastRotation = this.rotation;
	}

	public void Stuck(Vector2 stuckPos, Vector2 stuckRot) {
		this.stuck = true;
		this.stuckPos = stuckPos;
		this.stuckRot = stuckRot;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
			rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[i]);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].color = palette.blackColor;
		}
		this.color = Color.Lerp(new Color(1.0f, 0.0f, 0.57f), palette.blackColor, this.darkness);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A", true);
		sLeaser.sprites[1] = new FSprite("DangleFruit0B", true);
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		Vector2 position = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 rotation = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
		this.lastDarkness = this.darkness;
		this.darkness = rCam.room.Darkness(position) * (1f - rCam.room.LightSourceExposure(position));
		if (this.darkness != this.lastDarkness) {
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		for (int i = 0; i < 2; i++) {
			sLeaser.sprites[i].x = position.x - camPos.x;
			sLeaser.sprites[i].y = position.y - camPos.y;
			sLeaser.sprites[i].rotation = Custom.VecToDeg(rotation);
			sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("DangleFruit" + (2 - this.bites) + (i == 0 ? "A" : "B"));
		}

		if (this.blink > 0 && UnityEngine.Random.value < 0.5f) {
			sLeaser.sprites[0].color = base.blinkColor;
		}
		else {
			sLeaser.sprites[0].color = this.color;
		}
		if (this.blink > 0 && UnityEngine.Random.value < 0.5f) {
			sLeaser.sprites[1].color = base.blinkColor;
		}
		else {
			sLeaser.sprites[1].color = Color.Lerp(this.color, Color.black, 0.5f);
		}

		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu) {
		this.bites--;
		this.room.PlaySound(this.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, base.firstChunk);

		if (this.bites == 0) {
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			this.Destroy();
		}
	}

	public void ThrowByPlayer() {
	}

	public class AbstractCactusFruit : AbstractPhysicalObject {
		public AbstractCactusFruit(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, Enums.CactusFruit, realizedObject, pos, ID) {
		}
	}
}