namespace Floodwaters.Objects;

public class ColoredDeepProcessing : UpdatableAndDeletable, IDrawable {
	private float power = 1f;
	protected PropertyMesh mesh;

	public readonly PlacedObject pObj;
	public ColoredDeepProcessingData Data => this.pObj.data as ColoredDeepProcessingData;

	public ColoredDeepProcessing(PlacedObject placedObject, Room room) {
		this.pObj = placedObject;
		this.room = room;
		this.mesh = new PropertyMesh("FWColoredDeepProcessing", (mpb) => {
			mpb.SetColor("_Color", this.Data.color);
			mpb.SetFloat("_FromDepth", this.Data.fromDepth / 30f);
			mpb.SetFloat("_ToDepth", this.Data.toDepth / 30f);
			mpb.SetFloat("_Power", this.power);
			mpb.SetFloat("_Intensity", this.Data.intensity);
		}) {
			Vertices = new Vector2[4],
			Indices = [0, 1, 2, 0, 2, 3],
			UVs = [new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f)]
		};
	}

	public override void Update(bool eu) {
		base.Update(eu);
		if (UnityEngine.Random.value < 0.071428575f) {
			if (this.power > this.room.ElectricPower) {
				this.power = Mathf.Max((UnityEngine.Random.value < 0.2f) ? 0f : this.room.ElectricPower, this.power - 1f / Mathf.Lerp(1f, 4f, UnityEngine.Random.value));
				return;
			}
			if (this.power < this.room.ElectricPower) {
				this.power = Mathf.Min((UnityEngine.Random.value < 0.2f) ? 1f : this.room.ElectricPower, this.power + 1f / Mathf.Lerp(1f, 4f, UnityEngine.Random.value));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = [];
		sLeaser.containers = [ this.mesh ];
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Foreground");

		foreach (FContainer container in sLeaser.containers) {
			newContatiner.AddChild(container);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
			return;
		}

		ColoredDeepProcessingData data = this.pObj.data as ColoredDeepProcessingData;
		this.mesh.MoveVertex(0, Vector2.zero);
		this.mesh.MoveVertex(1, data.handles[0]);
		this.mesh.MoveVertex(2, data.handles[1]);
		this.mesh.MoveVertex(3, data.handles[2]);

		this.mesh.SetPosition(this.pObj.pos - camPos);
	}
}