namespace Floodwaters.Objects;

public class MagmaArea : UpdatableAndDeletable {
	public readonly PlacedObject pObj;
	public LightSource light1;
	public LightSource light2;
	public StaticSoundLoop steam;
	public FireSmoke smoke;
	public bool burntLast;
	public float heat;
	public float heatWait;

	MagmaAreaData Data => this.pObj.data as MagmaAreaData;
	float Rad => this.Data.Rad;

	public MagmaArea(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
		this.steam = new StaticSoundLoop(SoundID.Gate_Water_Steam_LOOP, this.pObj.pos, this.room, 1f, 1f);
		this.smoke = new FireSmoke(this.room);
	}

	public void Burn(Creature creature, BodyChunk chunk, bool skipHeat) {
		if (!skipHeat) {
			this.heat += (this.burntLast ? 0.04f : 0.2f) / this.Data.burnTime;
			this.heat = Mathf.Min(this.heat, 1.0f);
		}

		if (this.heat >= 0.95f) {
			creature.Violence(null, null, chunk, null, Enums.BurnDamageType, 1.0f, 0.0f);
		}
		if (Random.value < this.heat * 4.0f) {
			this.smoke.EmitSmoke(chunk.pos, Custom.DegToVec(Random.Range(45f, 135f)), this.room.game.cameras[0].currentPalette.blackColor, 20);
		}
	}

	public override void Update(bool eu) {
		bool heated = false;

		foreach (PhysicalObject physicalObject in this.room.physicalObjects[1]) {
			if (physicalObject is not Creature creature) continue;
			bool alreadyBurnt = false;

			foreach (BodyChunk chunk in creature.bodyChunks) {
				if ((chunk.pos - this.pObj.pos).sqrMagnitude > this.Rad * this.Rad) continue;

				if (creature is Player player) {
					player.aerobicLevel = Mathf.Min(player.aerobicLevel + this.heat * 2f, 1f);
					player.exhausted = player.aerobicLevel >= 1f;
				}

				if (chunk.contactPoint.x == 0 && chunk.contactPoint.y == 0) continue;

				this.Burn(creature, chunk, alreadyBurnt);
				this.heatWait = 20f;
				heated = true;
				alreadyBurnt = true;
			}
		}

		this.burntLast = heated;

		if (!heated) {
			this.heatWait--;
			if (this.heatWait < 0f) {
				this.heat -= 0.03f / this.Data.burnTime;
				this.heat = Mathf.Max(this.heat, 0.0f);
			}
		}

		if (this.light1 == null && !this.slatedForDeletetion) {
			this.light1 = new LightSource(this.pObj.pos, true, new Color(1f, 0.2f, 0f), null);
			this.room.AddObject(this.light1);
			this.light2 = new LightSource(this.pObj.pos, true, new Color(0.8f, 0.6f, 0f), null);
			this.room.AddObject(this.light2);
		}

		if (this.light1 != null && this.slatedForDeletetion) {
			this.light1.room.RemoveObject(this.light1);
			this.light1.Destroy();
			this.light1 = null;
			this.light2.room.RemoveObject(this.light2);
			this.light2.Destroy();
			this.light2 = null;
		}

		if (this.light1 != null) {
			this.light1.setPos = this.pObj.pos;
			this.light1.setRad = Mathf.Lerp(1.25f, 2f, this.heat) * this.Rad;
			this.light1.setAlpha = this.heat * 1.5f;
			this.light1.color = this.Data.colorA;

			this.light2.setPos = this.pObj.pos + Vector2.up * 2f;
			this.light2.setRad = Mathf.Lerp(1f, 1.75f, this.heat) * this.Rad;
			this.light2.setAlpha = this.heat * 1.75f;
			this.light2.color = this.Data.colorB;
		}

		this.steam.volume = this.heat * 0.7f;
		this.steam.Update();
	}
}