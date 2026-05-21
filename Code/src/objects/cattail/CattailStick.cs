namespace Floodwaters.Objects;

public class CattailStick : UpdatableAndDeletable, IDrawable {
	private readonly PlacedObject po;
	private Vector2 lastPoPos;
	private Vector2 lastPoHandle;
	
	private Vector2 baseEndPosition;
	private Vector2 lastEndPosition;
	private Vector2 endPosition;
	private Vector2 rootPosition;
	private float time;

	private readonly Counter timeToDisconnect = new Counter(50, 0, true);
	private Cattail cattail;

	private StaticSoundLoop pullingSoundLoop;

	private PlacedObject.ResizableObjectData Data => this.po.data as PlacedObject.ResizableObjectData;

	public CattailStick(Room room, PlacedObject po, Cattail cattail) {
		this.room = room;
		this.po = po;
		this.cattail = cattail;
		this.cattail.stick = this;
		this.rootPosition = po.pos + this.Data.handlePos;
		this.baseEndPosition = new Vector2(
			-Mathf.Deg2Rad * this.Data.handlePos.GetAngle(),
			-this.Data.handlePos.magnitude
		);
		this.lastEndPosition = this.baseEndPosition;
		this.endPosition = this.baseEndPosition;
		this.SetCattailPos();

		this.lastPoPos = po.pos;
		this.lastPoHandle = this.Data.handlePos;
		this.time = Random.Range(0f, Mathf.PI * 2f);
	}
	
	private Vector2 EndPositionToChunkPosition(Vector2 endPos) {
		return new Vector2(
			Mathf.Cos(endPos.x) * endPos.y,
			Mathf.Sin(endPos.x) * endPos.y
		) + this.rootPosition;
	}
	
	public void SetCattailPos() {
		this.cattail.firstChunk.lastLastPos = this.EndPositionToChunkPosition(this.lastEndPosition);
		this.cattail.firstChunk.lastPos = this.EndPositionToChunkPosition(this.lastEndPosition);
		this.cattail.firstChunk.pos = this.EndPositionToChunkPosition(this.endPosition);
		this.cattail.rotation = this.Data.handlePos.normalized;
		this.cattail.setRotation = this.cattail.rotation;
		this.cattail.lastRotation = this.cattail.rotation;
		this.cattail.rotationSpeed = 0f;
		this.cattail.CollideWithTerrain = false;
	}

	public override void Update(bool eu) {
		base.Update(eu);

		this.pullingSoundLoop ??= new StaticSoundLoop(SoundID.Slugcat_Slide_Down_Vertical_Beam_LOOP, this.cattail.firstChunk.pos, this.room, 0f, 1f);
		this.pullingSoundLoop.pos = this.EndPositionToChunkPosition(this.endPosition);
		this.pullingSoundLoop.Update();

		if (this.lastPoPos != this.po.pos || this.lastPoHandle != this.Data.handlePos) {
			this.rootPosition = this.po.pos + this.Data.handlePos;
			this.baseEndPosition = new Vector2(
				-Mathf.Deg2Rad * this.Data.handlePos.GetAngle(),
				-this.Data.handlePos.magnitude
			);
			this.lastEndPosition = this.baseEndPosition;
			this.endPosition = this.baseEndPosition;
			this.timeToDisconnect.Reset();

			this.lastPoPos = this.po.pos;
			this.lastPoHandle = this.Data.handlePos;
		} else {
			this.time += 0.005f;
			this.lastEndPosition = this.endPosition;
			this.endPosition.x = Mathf.Lerp(this.endPosition.x, this.baseEndPosition.x + Mathf.Cos(this.time) * (this.cattail == null ? 0.01f : 0.04f), 0.2f);
		}

		if (this.cattail == null)
			return;

		this.ManageDisconnect();
	}
	
	public void ManageDisconnect() {
		if (this.timeToDisconnect.isFinished) {
			this.room.PlaySound(SoundID.Spear_Dislodged_From_Creature, this.cattail.firstChunk);
			this.cattail.CollideWithTerrain = true;
			this.cattail.stick = null;
			this.cattail = null;
			this.pullingSoundLoop.volume = 0f;
			return;
		}
		
		if (this.cattail.grabbedBy.Count == 0 && !this.timeToDisconnect.isFinished) {
			this.cattail.firstChunk.vel = Vector2.zero;
			this.timeToDisconnect.Reset();
		}

		float dst = Mathf.Abs(this.cattail.firstChunk.vel.x);
		if (this.cattail.grabbedBy.Count > 0 && dst > 3f) {
			this.timeToDisconnect.Tick();
			this.pullingSoundLoop.volume = Mathf.Lerp(this.pullingSoundLoop.volume, Mathf.Clamp01(dst / 12f) * 2f, 0.2f);
			this.endPosition.x = Mathf.Lerp(this.endPosition.x, this.baseEndPosition.x + Mathf.Clamp(this.cattail.firstChunk.vel.x * -0.04f, -0.25f, 0.25f), 0.25f);
		} else {
			this.pullingSoundLoop.volume = Mathf.Lerp(this.pullingSoundLoop.volume, 0f, 0.02f);
		}

		this.SetCattailPos();
		this.cattail.firstChunk.vel = Vector2.zero;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("SmallSpear", true);
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		Vector2 end = this.EndPositionToChunkPosition(new Vector2(
			Mathf.Lerp(this.lastEndPosition.x, this.endPosition.x, timeStacker),
			Mathf.Lerp(this.lastEndPosition.y, this.endPosition.y, timeStacker)
		)) - this.rootPosition;
		Vector2 center = this.rootPosition + end * 0.5f;

		sLeaser.sprites[0].x = center.x - camPos.x;
		sLeaser.sprites[0].y = center.y - camPos.y;
		sLeaser.sprites[0].rotation = end.GetAngle() + 90f;
		sLeaser.sprites[0].scaleX = 0.75f;
		sLeaser.sprites[0].scaleY = (end.magnitude + 8f) / 44f;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Background");
		sLeaser.sprites[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.sprites[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		sLeaser.sprites[0].color = palette.blackColor;
	}
}