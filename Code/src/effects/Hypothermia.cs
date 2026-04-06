

namespace Floodwaters.Effects;

public class Hypothermia {
	public static void Initialize() {
		On.Creature.HypothermiaUpdate += On_Creature_HypothermiaUpdate;
		On.MoreSlugcats.HypothermiaMeter.Update += On_HypothermiaMeter_Update;
	}

	public static void Cleanup() {
		On.Creature.HypothermiaUpdate -= On_Creature_HypothermiaUpdate;
		On.MoreSlugcats.HypothermiaMeter.Update -= On_HypothermiaMeter_Update;
	}

	private static void On_Creature_HypothermiaUpdate(On.Creature.orig_HypothermiaUpdate orig, Creature self) {
		// LATER: Control strength
		if (!ModManager.HypothermiaModule || self.room.roomSettings.GetEffect(Enums.Hypothermia) == null) {
			orig(self);
			return;
		}

		float strength = self.room.roomSettings.GetEffectAmount(Enums.Hypothermia);

		self.HypothermiaGain = 0f;
		if (self.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer) {
			self.HypothermiaExposure = 0f;
			self.Hypothermia = 0f;
			return;
		}
		if (ModManager.HypothermiaModule && self.room.world.rainCycle.CycleProgression > 0f) {
			foreach (IProvideWarmth blizzardHeatSource in self.room.blizzardHeatSources) {
				float num;
				num = Vector2.Distance(self.firstChunk.pos, blizzardHeatSource.Position());
				if (self.abstractCreature.Hypothermia > 0.001f && blizzardHeatSource.loadedRoom == self.room && num < blizzardHeatSource.range) {
					float num2;
					num2 = Mathf.InverseLerp(blizzardHeatSource.range, blizzardHeatSource.range * 0.2f, num);
					self.abstractCreature.Hypothermia -= Mathf.Lerp(blizzardHeatSource.warmth * num2, 0f, self.HypothermiaExposure);
					if (self.abstractCreature.Hypothermia < 0f) {
						self.abstractCreature.Hypothermia = 0f;
					}
				}
			}
			if (!self.dead) {
				self.HypothermiaGain = Mathf.Lerp(0f, RainWorldGame.DefaultHeatSourceWarmth * 0.1f, Mathf.InverseLerp(0.1f, 0.95f, self.room.world.rainCycle.CycleProgression));
				if (!self.abstractCreature.HypothermiaImmune) {
					float num3 = self.room.world.rainCycle.cycleLength + (float) RainWorldGame.BlizzardHardEndTimer(self.room.game.IsStorySession);
					self.HypothermiaGain += Mathf.Lerp(0f, RainWorldGame.BlizzardMaxColdness, Mathf.InverseLerp(0f, num3, self.room.world.rainCycle.timer));
					self.HypothermiaGain += Mathf.Lerp(0f, 50f, Mathf.InverseLerp(num3, num3 * 5f, self.room.world.rainCycle.timer));
				}
				Color blizzardPixel = new Color(strength, strength, strength);
				self.HypothermiaGain += blizzardPixel.g / Mathf.Lerp(9100f, 5350f, Mathf.InverseLerp(0f, self.room.world.rainCycle.cycleLength + 4300f, self.room.world.rainCycle.timer));
				self.HypothermiaGain += blizzardPixel.b / 8200f;
				self.HypothermiaExposure = blizzardPixel.g;
				if (self.Submersion >= 0.1f) {
					self.HypothermiaExposure = 1f;
				}
				self.HypothermiaGain += self.Submersion / 7000f;
				self.HypothermiaGain = Mathf.Lerp(0f, self.HypothermiaGain, Mathf.InverseLerp(-0.5f, self.room.game.IsStorySession ? 1f : 3.6f, self.room.world.rainCycle.CycleProgression));
				self.HypothermiaGain *= Mathf.InverseLerp(50f, -10f, self.TotalMass);
			}
			else {
				self.HypothermiaExposure = 1f;
				self.HypothermiaGain = Mathf.Lerp(0f, 4E-05f, Mathf.InverseLerp(0.8f, 1f, self.room.world.rainCycle.CycleProgression));
				self.HypothermiaGain += self.Submersion / 6000f;
				self.HypothermiaGain += Mathf.InverseLerp(50f, -10f, self.TotalMass) / 1000f;
			}
			if (self.Hypothermia > 1.5f) {
				self.HypothermiaGain *= 2.3f;
			}
			else if (self.Hypothermia > 0.8f) {
				self.HypothermiaGain *= 0.5f;
			}
			if (self.abstractCreature.HypothermiaImmune) {
				self.HypothermiaGain /= 80f;
			}
			self.HypothermiaGain = Mathf.Clamp(self.HypothermiaGain, -1f, 0.0055f);
			if (ModManager.Watcher && self.room.game.IsStorySession && self.abstractCreature.rippleLayer != 0) {
				self.HypothermiaGain = 0f;
			}
			self.Hypothermia += self.HypothermiaGain;
			if (self.Hypothermia >= 0.8f && self.Consious && self.room != null && !self.room.abstractRoom.shelter) {
				if (self.HypothermiaGain > 0.0003f) {
					if (self.HypothermiaStunDelayCounter < 0) {
						int st;
						st = (int) Mathf.Lerp(5f, 60f, Mathf.Pow(self.Hypothermia / 2f, 8f));
						self.HypothermiaStunDelayCounter = (int) UnityEngine.Random.Range(300f - self.Hypothermia * 120f, 500f - self.Hypothermia * 100f);
						self.Stun(st);
					}
				}
				else {
					self.HypothermiaStunDelayCounter = UnityEngine.Random.Range(200, 500);
				}
			}
			if (self.Hypothermia >= 1f && self.stun > 50f && !self.dead) {
				self.Die();
				return;
			}
		}
		else {
			if (self.Hypothermia > 2f) {
				self.Hypothermia = 2f;
			}
			self.Hypothermia = Mathf.Lerp(self.Hypothermia, 0f, 0.001f);
			self.HypothermiaExposure = 0f;
		}
		if (self.room != null && !self.room.abstractRoom.shelter) {
			self.HypothermiaStunDelayCounter--;
		}
	}

	private static void On_HypothermiaMeter_Update(On.MoreSlugcats.HypothermiaMeter.orig_Update orig, HypothermiaMeter self) {
		Room room = (self.hud.rainWorld.processManager.currentMainLoop as RainWorldGame)?.cameras[0].room;
		if (room == null || !ModManager.HypothermiaModule || room.roomSettings.GetEffect(Enums.Hypothermia) == null) {
			orig(self);
			return;
		}

		RoomRain.DangerType pre = room.roomSettings.dType;
		room.roomSettings.dType = DLCSharedEnums.RoomRainDangerType.Blizzard;
		orig(self);
		room.roomSettings.dType = pre;
	}
}