
namespace Floodwaters.Effects;

public static class Effects {
	public static void Initialize() {
		On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += On_RoomSettingsPage_DevEffectGetCategoryFromEffectType;
		On.RoomCamera.ApplyPalette += On_RoomCamera_ApplyPalette;
		On.Room.Loaded += On_Room_Loaded;
		On.RoomPreprocessor.DecompressStringToAImaps += a;
		EoCFanSpeed.Initialize();
	}

	public static void Cleanup() {
		On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType -= On_RoomSettingsPage_DevEffectGetCategoryFromEffectType;
		On.RoomCamera.ApplyPalette -= On_RoomCamera_ApplyPalette;
		On.Room.Loaded -= On_Room_Loaded;
		On.RoomPreprocessor.DecompressStringToAImaps -= a;
		EoCFanSpeed.Cleanup();
	}

	private static CreatureSpecificAImap[] a(On.RoomPreprocessor.orig_DecompressStringToAImaps orig, string s, AImap aimap) {
		return orig(null, aimap);
	}

	private static RoomSettingsPage.DevEffectsCategories On_RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type) {
		if (type == Enums.FWFogEffect || type == Enums.FWChromaticEffect || type == Enums.FWNoiseEffect || type == Enums.EoCFanSpeedEffect)
			return Enums.FloodwatersEffectCategory;

		return orig(self, type);
	}

	private static void On_RoomCamera_ApplyPalette(On.RoomCamera.orig_ApplyPalette orig, RoomCamera self) {
		orig(self);

		if (self.room?.roomSettings == null) {
			return;
		}

		float bloom = self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.Bloom);
		float lightBurn = self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LightBurn);
		float skyAndLightBloom = self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom);
		float skyBloom = self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyBloom);
		float bloomValue = 0f;

		if (bloom > 0f) {
			Shader.SetGlobalInt(Assets.ShadPropFWBloomType, 0);
			bloomValue = bloom;
		} else if (skyAndLightBloom > 0f) {
			Shader.SetGlobalInt(Assets.ShadPropFWBloomType, 1);
			bloomValue = skyAndLightBloom;
		} else if (lightBurn > 0f) {
			Shader.SetGlobalInt(Assets.ShadPropFWBloomType, 2);
			bloomValue = lightBurn;
		} else if (skyBloom > 0f) {
			Shader.SetGlobalInt(Assets.ShadPropFWBloomType, 3);
			bloomValue = skyBloom;
		} else {
			Shader.SetGlobalInt(Assets.ShadPropFWBloomType, -1);
		}

		if (self.room?.roomSettings.GetEffect(Enums.FWFogEffect) != null) {
			self.SetUpFullScreenEffect("Bloom");
			self.fullScreenEffect.shader = Custom.rainWorld.Shaders["FWFog"];
			self.lightBloomAlphaEffect = Enums.FWFogEffect;
			self.lightBloomAlpha = self.room.roomSettings.GetEffectAmount(Enums.FWFogEffect);
			self.fullScreenEffect.color = new Color(
				bloomValue,
				1f,
				1f,
				1f
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