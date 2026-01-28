
namespace Floodwaters.Effects;

public static class EoCFanSpeed {
	public static void Initialize() {
		On.SpinningFan.Update += On_SpinningFan_Update;
	}

	public static void Cleanup() {
		On.SpinningFan.Update -= On_SpinningFan_Update;
	}

	private static void On_SpinningFan_Update(On.SpinningFan.orig_Update orig, SpinningFan self, bool eu) {
		float speed = self.speed;
		RoomSettings.RoomEffect effect = self.room.roomSettings.GetEffect(Enums.EoCFanSpeedEffect);
		if (effect != null) {
			RainCycle cycle = self.room.world.rainCycle;
			speed = 1f + 25f * effect.amount * Mathf.Clamp01(0.75f - cycle.AmountLeft);
		}
		orig(self, eu);
		self.speed = speed;
	}
}