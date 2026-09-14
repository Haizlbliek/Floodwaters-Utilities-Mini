namespace Floodwaters.Effects;

public static class SKLightningSaturation {
	public class LightningData {
		public float saturation;
	}

	private static readonly ConditionalWeakTable<LightningMaker, LightningData> saturations = new ConditionalWeakTable<LightningMaker, LightningData>();

	public static void Initialize() {
		On.Watcher.LightningMaker.SetHue += On_LightningMaker_SetHue;
		On.Watcher.LightningMaker.Update += On_LightningMaker_Update;
	}

	public static void Cleanup() {
		On.Watcher.LightningMaker.SetHue -= On_LightningMaker_SetHue;
		On.Watcher.LightningMaker.Update -= On_LightningMaker_Update;
	}

	private static void On_LightningMaker_SetHue(On.Watcher.LightningMaker.orig_SetHue orig, LightningMaker self, float hue) {
		orig(self, hue);
		LightningData data = saturations.GetOrCreateValue(self);
		data.saturation = self.room.roomSettings.GetEffect(RestrictedEnums.SKLightningSaturation)?.amount ?? 1f;
		self.mainColor = Custom.HSL2RGB(hue, data.saturation, 0.5f);
	}

	private static void On_LightningMaker_Update(On.Watcher.LightningMaker.orig_Update orig, LightningMaker self, bool eu) {
		orig(self, eu);

		LightningData data = saturations.GetOrCreateValue(self);
		RoomSettings.RoomEffect hueEffect = self.room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.SKLightningHue);
		RoomSettings.RoomEffect satEffect = self.room.roomSettings.GetEffect(RestrictedEnums.SKLightningSaturation);
		if ((satEffect == null ? data.saturation != 1f : data.saturation != satEffect.amount) || (hueEffect == null && self.hue != 0.535f)) {
			self.SetHue(hueEffect?.amount ?? 0.535f);
			Shader.SetGlobalColor("_SKLightningColor", self.mainColor);
		}
	}
}