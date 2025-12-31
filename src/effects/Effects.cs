
namespace Floodwaters.Effects;

public static class Effects {
	public static void Initialize() {
		On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += On_RoomSettingsPage_DevEffectGetCategoryFromEffectType;
		On.RoomCamera.ApplyPalette += On_RoomCamera_ApplyPalette;
		On.Room.Loaded += On_Room_Loaded;
		On.RoomPreprocessor.DecompressStringToAImaps += a;
	}

	public static void Cleanup() {
		On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType -= On_RoomSettingsPage_DevEffectGetCategoryFromEffectType;
		On.RoomCamera.ApplyPalette -= On_RoomCamera_ApplyPalette;
		On.Room.Loaded -= On_Room_Loaded;
		On.RoomPreprocessor.DecompressStringToAImaps -= a;
	}

	private static CreatureSpecificAImap[] a(On.RoomPreprocessor.orig_DecompressStringToAImaps orig, string s, AImap aimap) {
		return orig(null, aimap);
	}

	private static RoomSettingsPage.DevEffectsCategories On_RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type) {
		if (type == Enums.FWFogEffect || type == Enums.FWChromaticEffect || type == Enums.FWNoiseEffect)
			return Enums.FloodwatersEffectCategory;

		return orig(self, type);
	}

	private static void On_RoomCamera_ApplyPalette(On.RoomCamera.orig_ApplyPalette orig, RoomCamera self) {
		orig(self);

		if (self.room?.roomSettings.GetEffect(Enums.FWFogEffect) != null) {
			self.SetUpFullScreenEffect("Bloom");
			self.fullScreenEffect.shader = Custom.rainWorld.Shaders["FWFog"];
			self.lightBloomAlphaEffect = Enums.FWFogEffect;
			self.lightBloomAlpha = self.room.roomSettings.GetEffectAmount(Enums.FWFogEffect);
			self.fullScreenEffect.color = new Color(
				self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Bloom),
				Mathf.Max(self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LightBurn), self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom)),
				Mathf.Max(self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyBloom), self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom)),
				1.0f
			);
			self.fullScreenEffect.alpha = self.lightBloomAlpha;
			return;
		}

		if (self.room?.roomSettings.GetEffectAmount(Enums.FWChromaticEffect) > 0f) {
			self.SetUpFullScreenEffect("Bloom");
			self.fullScreenEffect.shader = Custom.rainWorld.Shaders["FWChromatic"];
			self.lightBloomAlphaEffect = Enums.FWChromaticEffect;
			self.lightBloomAlpha = self.room.roomSettings.GetEffectAmount(Enums.FWChromaticEffect);
			return;
		}

		if (self.room?.roomSettings.GetEffectAmount(Enums.FWNoiseEffect) > 0f) {
			self.SetUpFullScreenEffect("Bloom");
			self.fullScreenEffect.shader = Custom.rainWorld.Shaders["FWStatic"];
			self.lightBloomAlphaEffect = Enums.FWNoiseEffect;
			self.lightBloomAlpha = self.room.roomSettings.GetEffectAmount(Enums.FWNoiseEffect);
			return;
		}
	}

	private static void On_Room_Loaded(On.Room.orig_Loaded orig, Room self) {
		orig(self);

		if (self.game == null)
			return;

		if (self.roomSettings.DangerType == Enums.HeatDanger) {
			self.AddObject(new HeatDanger(self));
		}
	}
}