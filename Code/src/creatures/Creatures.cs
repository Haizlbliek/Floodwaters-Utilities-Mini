using Watcher;

namespace Floodwaters.Creatures;

public class Creatures {
	public static bool DisableBodyChunkTerrainCollision = false;

	public static void Initialize() {
		CustomAlternateVariants.Initialize();

		On.BodyChunk.CheckVerticalCollision += On_BodyChunk_CheckVerticalCollision;
		On.StaticWorld.InitCustomTemplates += On_StaticWorld_InitCustomTemplates;
	}

	public static void Cleanup() {
		CustomAlternateVariants.Cleanup();

		On.BodyChunk.CheckVerticalCollision -= On_BodyChunk_CheckVerticalCollision;
		On.StaticWorld.InitCustomTemplates -= On_StaticWorld_InitCustomTemplates;
	}

	private static void On_BodyChunk_CheckVerticalCollision(On.BodyChunk.orig_CheckVerticalCollision orig, BodyChunk self) {
		TerrainManager oldTerrain = self.owner.room.terrain;
		if (DisableBodyChunkTerrainCollision) {
			self.owner.room.terrain = null;
		}

		orig(self);

		self.owner.room.terrain = oldTerrain;
	}

	private static void On_StaticWorld_InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig) {
		orig();

		if (ModManager.Watcher) {
			StaticWorld.creatureTemplates[WatcherEnums.CreatureTemplateType.FireSprite.index].damageRestistances[Enums.BurnDamageType.index, 0] = 10000f;
			StaticWorld.creatureTemplates[WatcherEnums.CreatureTemplateType.BoxWorm.index].damageRestistances[Enums.BurnDamageType.index, 0] = 10000f;
		}
	}
}