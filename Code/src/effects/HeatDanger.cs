namespace Floodwaters.Effects;

public class HeatDanger : UpdatableAndDeletable {
	public FireSmoke smoke;
	public DisembodiedDynamicSoundLoop soundPlayer;

	public HeatDanger(Room room) {
		this.room = room;
		this.smoke = new FireSmoke(this.room) {
			connectParticlesTime = -1
		};
		this.room.AddObject(this.smoke);
		this.soundPlayer = new DisembodiedDynamicSoundLoop(this) {
			sound = Enums.HeatDangerLoop,
			VolumeGroup = 3
		};
	}

	public override void Update(bool eu) {
		if (this.room.roomSettings.DangerType != Enums.HeatDanger) return;

		RainCycle rainCycle = this.room.world.rainCycle;
		if (rainCycle.TimeUntilRain < 2400f) {
			float heat = 1f - (rainCycle.TimeUntilRain / 2400f);

			foreach (PhysicalObject po in this.room.physicalObjects[1]) {
				if (po is not Creature creature) continue;
				if (creature.Template.damageRestistances[Enums.BurnDamageType.index, 0] > 0f) {
					continue;
				}

				if (heat > 0.5f) {
					creature.Violence(null, null, null, null, Enums.BurnDamageType, heat, Random.value < heat * 0.125f ? -15f : -1000f);
				}

				foreach (BodyChunk chunk in po.bodyChunks) {
					if (Random.value < heat * 2f) {
						this.smoke.EmitSmoke(chunk.pos, Custom.DegToVec(Random.Range(45f, 135f)), this.room.game.cameras[0].currentPalette.blackColor, 20);
					}
				}
			}

			heat = Mathf.Clamp01(heat);
			this.soundPlayer.Volume = Mathf.Lerp(0.0f, 0.3f, heat);
			this.soundPlayer.Pitch = Mathf.Lerp(0.1f, 2.5f, heat);
		} else {
			this.soundPlayer.Volume = 0.0f;
		}
		this.soundPlayer.Update();
	}
}