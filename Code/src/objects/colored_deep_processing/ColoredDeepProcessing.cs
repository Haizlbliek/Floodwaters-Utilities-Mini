namespace Floodwaters.Objects;

public class ColoredDeepProcessing : UpdatableAndDeletable, IDrawable {
	private float power = 1f;

	public readonly PlacedObject pObj;
	public ColoredDeepProcessingData Data => this.pObj.data as ColoredDeepProcessingData;

	public ColoredDeepProcessing(PlacedObject placedObject, Room room) {
		this.pObj = placedObject;
		this.room = room;
	}

	public override void Update(bool eu) {
		base.Update(eu);

		if (UnityEngine.Random.value < 1f / 14f) {
			if (this.power > this.room.ElectricPower) {
				this.power = Mathf.Max((UnityEngine.Random.value < 0.2f) ? 0f : this.room.ElectricPower, this.power - 1f / Mathf.Lerp(1f, 4f, UnityEngine.Random.value));
			}
			else if (this.power < this.room.ElectricPower) {
				this.power = Mathf.Min((UnityEngine.Random.value < 0.2f) ? 1f : this.room.ElectricPower, this.power + 1f / Mathf.Lerp(1f, 4f, UnityEngine.Random.value));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = [];
		sLeaser.containers = [
			new PropertyMesh("FWColoredDeepProcessing", (mpb) => {
				mpb.SetColor("_Color", this.Data.color);
				mpb.SetFloat("_FromDepth", this.Data.fromDepth / 30f);
				mpb.SetFloat("_ToDepth", this.Data.toDepth / 30f);
				mpb.SetFloat("_Power", this.power);
				mpb.SetFloat("_Intensity", this.Data.intensity);
			}) {
				Vertices = new Vector2[4],
				Indices = [0, 1, 2, 0, 2, 3],
				UVs = [new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f)]
			},
		];
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("ForegroundLights");

		foreach (FContainer container in sLeaser.containers) {
			container.RemoveFromContainer();
			newContatiner.AddChild(container);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		ColoredDeepProcessingData data = this.pObj.data as ColoredDeepProcessingData;
		PropertyMesh mesh = sLeaser.containers[0] as PropertyMesh;

		mesh.MoveVertex(0, Vector2.zero);
		mesh.MoveVertex(1, data.handles[0]);
		mesh.MoveVertex(2, data.handles[1]);
		mesh.MoveVertex(3, data.handles[2]);

		mesh.SetPosition(this.pObj.pos - camPos);

		if (base.slatedForDeletetion) {
			mesh.Destroy();
			mesh.RemoveFromContainer();
			sLeaser.CleanSpritesAndRemove();
		}
	}
}