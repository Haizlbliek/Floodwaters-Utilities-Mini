namespace Floodwaters.Objects;

public class ExitLock : UpdatableAndDeletable {
	private readonly PlacedObject pObj;
	private ExitLockData Data => this.pObj.data as ExitLockData;

	private IntVector2 lastPos;

	private bool lastLocked = true;
	public bool Locked { get; private set; } = true;

	public ExitLock(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
		this.lastPos = this.room.GetTilePosition(this.pObj.pos);
		this.Refresh();
	}

	public void Refresh() {
		this.Locked = true;
		if (!this.Data.ignoreRegionSpecific && this.room.game.rainWorld.progression.miscProgressionData.HasRegionExitKey(this.room.world.name, this.Data.key))
			this.Locked = false;

		if (!this.Data.ignoreSaveSpecific && this.room.game.rainWorld.progression.miscProgressionData.HasRegionExitKey("", this.Data.key))
			this.Locked = false;
	}

	public override void Update(bool eu) {
		IntVector2 pos = this.room.GetTilePosition(this.pObj.pos);

		if (this.Locked) {
			foreach (Player player in this.room.PlayersInRoom) {
				if (player == null)
					continue;

				if (player.enteringShortCut.HasValue && player.enteringShortCut.Value == pos) {
					player.firstChunk.vel = this.room.ShorcutEntranceHoleDirection(player.enteringShortCut.Value).ToVector2() * 4f;
					player.enteringShortCut = null;
				}
			}
		}

		if (pos != this.lastPos || this.Locked != this.lastLocked) {
			this.lastPos = pos;
			this.lastLocked = this.Locked;
			this.room.game.cameras[0].shortcutGraphics.NewRoom();
		}
	}
}