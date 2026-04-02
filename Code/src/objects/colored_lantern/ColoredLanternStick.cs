namespace Floodwaters.Objects;

public class ColoredLanternStick : UpdatableAndDeletable, IDrawable, IProvideWarmth {
	public PlacedObject po;
	public ColoredLantern lantern;
	public Vector2[] stickPositions;

	float IProvideWarmth.warmth => ((IProvideWarmth) this.lantern).warmth;
	Room IProvideWarmth.loadedRoom => base.room;
	float IProvideWarmth.range => ((IProvideWarmth) this.lantern).range;
	Vector2 IProvideWarmth.Position() => this.lantern.firstChunk.pos;

	public ColoredLanternData Data => this.po.data as ColoredLanternData;

	public ColoredLanternStick(Room room, PlacedObject po, int originRoom, int placedObjectIndex) {
		base.room = room;
		this.po = po;
		ColoredLanternData data = po.data as ColoredLanternData;
		this.lantern = new ColoredLantern(new AbstractColoredLantern(room.world, null, room.GetWorldCoordinate(po.pos), room.game.GetNewID(), originRoom, placedObjectIndex, data.color1, data.color2, data.dead, data)) {
			room = room,
			stick = this
		};
		this.lantern.firstChunk.HardSetPosition(po.pos);
		this.stickPositions = new Vector2[(int) Mathf.Clamp(data.stickEnd.Value.magnitude / 11f, 3f, 30f)];
		Random.State state;
		state = Random.state;
		Random.InitState((int) po.pos.x);
		for (int i = 0; i < this.stickPositions.Length; i++) {
			this.stickPositions[i] = Custom.RNV() * Random.value;
		}
		Random.state = state;
	}

	public void Refresh() {
		this.lantern.Abstr.color1 = this.Data.color1;
		this.lantern.Abstr.color2 = this.Data.color2;
		this.lantern.Abstr.dead = this.Data.dead;
		this.lantern.Abstr.HandleDead();
	}

	public override void Update(bool eu) {
		base.Update(eu);
		this.lantern.Update(eu);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		this.lantern.InitiateSprites(sLeaser, rCam);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		this.lantern.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		this.lantern.ApplyPalette(sLeaser, rCam, palette);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		this.lantern.AddToContainer(sLeaser, rCam, newContatiner);
	}
}