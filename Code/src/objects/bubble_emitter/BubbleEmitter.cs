namespace Floodwaters.Objects;

public class BubbleEmitter : UpdatableAndDeletable {
	public PlacedObject po;

	public BubbleEmitterData Data => this.po.data as BubbleEmitterData;

	public float delay;

	public BubbleEmitter(Room room, PlacedObject po) {
		this.room = room;
		this.po = po;
	}

	public override void Update(bool eu) {
		base.Update(eu);

		this.delay -= 1f;

		if (this.delay <= 0f) {
			this.room.AddObject(new Bubble(this.po.pos + Custom.RNV() * 4f, Custom.RNV() * Mathf.Lerp(6f, 16f, Random.value) * 0.25f, false, false) {
				age = (int) (600f - Mathf.Lerp(1f, 10f, this.Data.age) * Random.Range(20f, Random.Range(30f, 80f))),
			});
			this.delay = Random.value * (1f - this.Data.intensity) * 40f;
		}
	}
}