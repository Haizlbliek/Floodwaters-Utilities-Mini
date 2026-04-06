namespace Floodwaters.Objects;

public class SmokePipe : UpdatableAndDeletable {
	public PlacedObject pObj;
	public SmokePipeData Data => this.pObj.data as SmokePipeData;

	private float intensity;
	private SmokeSmoke smoke;
	private int steamCounter;
	private float burst;
	private float timeBurst;

	public SmokePipe(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
	}

	public override void Update(bool eu) {
		Vector2 direction = this.Data.handlePos.normalized;
		this.intensity = Mathf.Clamp01(this.Data.Rad / 250f);

		if (this.smoke == null) {
			this.smoke = new SmokeSmoke(base.room);
			base.room.AddObject(this.smoke);
		}
		float num = Mathf.Clamp(this.intensity, 0.01f, 1f);
		if (this.Data.eoc) {
			num *= 1f - Mathf.Clamp(this.room.game.world.rainCycle.TimeUntilRain / 1000f, 0f, 1f);
		}
		if (this.steamCounter > 0) {
			this.steamCounter += 2;
			if (this.steamCounter > (int) ((200f + this.intensity * 800f) * this.timeBurst)) {
				this.steamCounter = 0;
			}
			return;
		}
		if (this.intensity < 0.25f) {
			this.burst = Random.Range(0f, 0.1f);
		}
		else if (this.intensity < 0.5f) {
			this.burst = Random.Range(0.1f, 0.2f);
		}
		else {
			this.burst = Random.Range(0.2f, 0.35f);
		}
		this.timeBurst = Random.Range(0.2f, 0.35f);
		if (num > 0f) {
			if (base.room.PointSubmerged(this.pObj.pos)) {
				for (int i = 0; i < Random.Range(10f, 30f); i++) {
					base.room.AddObject(new Bubble(this.pObj.pos, direction, Random.Range(0f, 1f) < 0.2f, fakeWaterBubble: false));
				}
			}
			else if (base.room.game.cameras[0].room == base.room && Vector2.Distance(base.room.cameraPositions[base.room.game.cameras[0].currentCameraPosition], this.pObj.pos) < 1500f) {
				if (Random.Range(0f, 1f) < 0.5f) {
					base.room.PlaySound(SoundID.Gate_Electric_Steam_Puff, this.pObj.pos, num / 3f + this.burst, Random.Range(0.45f, 1.4f));
				}
				else {
					base.room.PlaySound(SoundID.Gate_Water_Steam_Puff, this.pObj.pos, num / 3f + this.burst, Random.Range(0.45f, 1.4f));
				}
				this.smoke.EmitSmoke(this.pObj.pos, direction * this.intensity, new FloatRect(this.pObj.pos.x - 150f, this.pObj.pos.y - 150f, this.pObj.pos.x + 150f, this.pObj.pos.y + 150f), num + this.burst);
			}
		}
		this.steamCounter = 1;
	}
}