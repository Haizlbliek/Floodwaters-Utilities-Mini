namespace Floodwaters.Effects;

public class WaterEffects {
	public static void Initialize() {
		On.Water.InitiateSprites += On_Water_InitiateSprites;
	}

	public static void Cleanup() {
		On.Water.InitiateSprites -= On_Water_InitiateSprites;
	}

	private static void On_Water_InitiateSprites(On.Water.orig_InitiateSprites orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		orig(self, sLeaser, rCam);

		FShader customSurfaceShader = null;
		FShader customDeepShader = null;

		if (self.room.roomSettings.GetEffect(Enums.WaterSlush) != null) {
			customSurfaceShader = Custom.rainWorld.Shaders["FWWaterSlush"];
		}
		else if (self.room.roomSettings.GetEffect(Enums.WaterSludge) != null) {
			customSurfaceShader = Custom.rainWorld.Shaders["FWWaterSludge"];
		}

		if (self.room.roomSettings.GetEffect(Enums.StraightDeepWater) != null) {
			customDeepShader = Custom.rainWorld.Shaders["FWStraightDeepWater"];
		}

		if (customSurfaceShader != null) {
			sLeaser.sprites[0].shader = customSurfaceShader;
			for (int k = 1; k < self.surfaces.Length; k++) {
				sLeaser.sprites[k * 2].shader = customSurfaceShader;
			}
		}

		if (customDeepShader != null) {
			sLeaser.sprites[1].shader = customDeepShader;
			for (int k = 1; k < self.surfaces.Length; k++) {
				sLeaser.sprites[k * 2 + 1].shader = customDeepShader;
			}
		}
	}
}