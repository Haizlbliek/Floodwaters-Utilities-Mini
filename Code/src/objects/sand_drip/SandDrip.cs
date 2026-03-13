namespace Floodwaters.Objects;

public class SandDrip : CosmeticSprite {
	public readonly PlacedObject pObj;
	private float height;
	private float lastTime;
	private float time;

	protected SandDripData Data => this.pObj.data as SandDripData;

	public SandDrip(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
		Futile.atlasManager.GetElementWithName("Circle20").atlas.texture.wrapMode = TextureWrapMode.Repeat;
	}

	public override void Update(bool eu) {
		base.Update(eu);
		this.pos = this.pObj.pos;
		this.lastTime = this.time;
		if (this.height != 0.0f) this.time += 10.0f / this.height;

		if (this.pos != this.lastPos) {
			this.lastPos = this.pos;
			this.Recalculate();
		}
	}
	
	private void Recalculate() {
		IntVector2 tile = this.room.GetTilePosition(this.pos);
		while (tile.y >= 0) {
			if (this.room.GetTile(tile.x, tile.y).Solid) {
				break;
			}

			tile.y--;
		}

		this.height = this.pos.y - tile.y * 20f - 20f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new TriangleMesh("Futile_White", [ new TriangleMesh.Triangle(0, 1, 2) ], false, false) {
			color = Color.blue
		};
		
		sLeaser.sprites[1] = new TriangleMesh("Circle20", [ new TriangleMesh.Triangle(0, 1, 2), new TriangleMesh.Triangle(1, 2, 3) ], false) {
			color = Color.green
		};
		this.AddToContainer(sLeaser, rCam, null);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		float cTime = Mathf.Lerp(this.lastTime, this.time, timeStacker);

		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float uy = this.height / 200f;
		Vector2 endPos = new Vector2(this.pos.x, this.pos.y - this.height);

		TriangleMesh trickle = sLeaser.sprites[1] as TriangleMesh;
		trickle.color = this.Data.sandColor;
		trickle.MoveVertice(0, this.pos + Vector2.left - camPos);
		trickle.MoveVertice(1, this.pos + Vector2.right - camPos);
		trickle.MoveVertice(2, this.pos + Vector2.left + Vector2.down * this.height - camPos);
		trickle.MoveVertice(3, this.pos + Vector2.right + Vector2.down * this.height - camPos);
		trickle.UVvertices[0] = new Vector2(0f, 0f - cTime);
		trickle.UVvertices[1] = new Vector2(uy, 0f - cTime);
		trickle.UVvertices[2] = new Vector2(0f, 1f - cTime);
		trickle.UVvertices[3] = new Vector2(uy, 1f - cTime);

		TriangleMesh pile = sLeaser.sprites[0] as TriangleMesh;
		pile.color = this.Data.sandColor;
		pile.MoveVertice(0, endPos + Vector2.left * this.Data.pileSize - camPos);
		pile.MoveVertice(1, endPos + Vector2.right * this.Data.pileSize - camPos);
		pile.MoveVertice(2, endPos + Vector2.up * this.Data.pileSize - camPos);
	}
}