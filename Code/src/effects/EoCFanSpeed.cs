
namespace Floodwaters.Effects;

public static class EoCFanSpeed {
	public static void Initialize() {
		On.SpinningFan.Update += On_SpinningFan_Update;
	}

	public static void Cleanup() {
		On.SpinningFan.Update -= On_SpinningFan_Update;
	}

	private static void On_SpinningFan_Update(On.SpinningFan.orig_Update orig, SpinningFan self, bool eu) {
		orig(self, eu);
		RoomSettings.RoomEffect effect = self.room.roomSettings.GetEffect(Enums.EoCFanSpeedEffect);
		if (effect != null) {
			RainCycle cycle = self.room.world.rainCycle;
			self.FanElement.rotation += self.speed * 0.1f * 25f * effect.amount * Mathf.Clamp01((0.25f - cycle.AmountLeft) / 0.25f);
		}
	}
}