namespace Floodwaters;

public static class Enums {
	public static AbstractPhysicalObject.AbstractObjectType CactusFruit;
	public static AbstractPhysicalObject.AbstractObjectType CactusSpear;
	public static AbstractPhysicalObject.AbstractObjectType Cattail;
	public static AbstractPhysicalObject.AbstractObjectType ColoredLantern;
	public static AbstractPhysicalObject.AbstractObjectType Lillypad;

	public static PlacedObject.Type CactusPO;
	public static PlacedObject.Type SandDripPO;
	public static PlacedObject.Type DeerSkullPO;
	public static PlacedObject.Type CattailPO;
	public static PlacedObject.Type ColoredCattailPO;
	public static PlacedObject.Type BubbleEmitterPO;
	public static PlacedObject.Type BambooPO;
	public static PlacedObject.Type ColoredLanternPO;
	public static PlacedObject.Type ColoredLanternStickPO;
	public static PlacedObject.Type LillypadPO;
	public static PlacedObject.Type WaterDripsPO;
	public static PlacedObject.Type MagmaAreaPO;
	public static PlacedObject.Type HeatSourcePO;

	public static ObjectsPage.DevObjectCategories FloodwatersCategory;
	public static RoomSettingsPage.DevEffectsCategories FloodwatersEffectCategory;

	public static RoomSettings.RoomEffect.Type FWFogEffect;
	public static RoomSettings.RoomEffect.Type FWChromaticEffect;
	public static RoomSettings.RoomEffect.Type FWNoiseEffect;

	public static RoomRain.DangerType HeatDanger;

	public static Creature.DamageType BurnDamageType; public static bool deleteBurnDamageType;

	public static SoundID HeatDangerLoop;

	public static void Initialize() {
		CactusFruit = new AbstractPhysicalObject.AbstractObjectType("CactusFruit", register: true);
		CactusSpear = new AbstractPhysicalObject.AbstractObjectType("CactusSpear", register: true);
		Cattail = new AbstractPhysicalObject.AbstractObjectType("Cattail", register: true);
		ColoredLantern = new AbstractPhysicalObject.AbstractObjectType("ColoredLantern", register: true);
		Lillypad = new AbstractPhysicalObject.AbstractObjectType("Lillypad", register: true);

		CactusPO = new PlacedObject.Type("Cactus", register: true);
		SandDripPO = new PlacedObject.Type("SandDrip", register: true);
		DeerSkullPO = new PlacedObject.Type("DeerSkull", register: true);
		CattailPO = new PlacedObject.Type("Cattail", register: true);
		ColoredCattailPO = new PlacedObject.Type("ColoredCattail", register: true);
		BubbleEmitterPO = new PlacedObject.Type("BubbleEmitter", register: true);
		BambooPO = new PlacedObject.Type("Bamboo", register: true);
		ColoredLanternPO = new PlacedObject.Type("ColoredLantern", register: true);
		ColoredLanternStickPO = new PlacedObject.Type("ColoredLanternStick", register: true);
		LillypadPO = new PlacedObject.Type("Lillypad", register: true);
		WaterDripsPO = new PlacedObject.Type("WaterDrips", register: true);
		MagmaAreaPO = new PlacedObject.Type("MagmaArea", register: true);
		HeatSourcePO = new PlacedObject.Type("HeatSource", register: true);

		FloodwatersCategory = new ObjectsPage.DevObjectCategories("Floodwaters", register: true);
		FloodwatersEffectCategory = new RoomSettingsPage.DevEffectsCategories("Floodwaters", register: true);

		FWFogEffect = new RoomSettings.RoomEffect.Type("FWFog", register: true);
		FWChromaticEffect = new RoomSettings.RoomEffect.Type("Chromatic", register: true);
		FWNoiseEffect = new RoomSettings.RoomEffect.Type("Noise", register: true);

		HeatDanger = new RoomRain.DangerType("Heat", register: true);

		BurnDamageType = new Creature.DamageType("Burn", register: false);
		deleteBurnDamageType = BurnDamageType == null;
		BurnDamageType = new Creature.DamageType("Burn", register: true);

		HeatDangerLoop = new SoundID("FW_HeatDangerLoop", register: true);
	}

	public static void Cleanup() {
		CactusFruit?.Unregister(); CactusFruit = null;
		CactusSpear?.Unregister(); CactusSpear = null;
		Cattail?.Unregister(); Cattail = null;
		ColoredLantern?.Unregister(); ColoredLantern = null;
		Lillypad?.Unregister(); Lillypad = null;

		CactusPO?.Unregister(); CactusPO = null;
		SandDripPO?.Unregister(); SandDripPO = null;
		DeerSkullPO?.Unregister(); DeerSkullPO = null;
		CattailPO?.Unregister(); CattailPO = null;
		ColoredCattailPO?.Unregister(); ColoredCattailPO = null;
		BubbleEmitterPO?.Unregister(); BubbleEmitterPO = null;
		BambooPO?.Unregister(); BambooPO = null;
		ColoredLanternPO?.Unregister(); ColoredLanternPO = null;
		ColoredLanternStickPO?.Unregister(); ColoredLanternStickPO = null;
		LillypadPO?.Unregister(); LillypadPO = null;
		WaterDripsPO?.Unregister(); WaterDripsPO = null;
		MagmaAreaPO?.Unregister(); MagmaAreaPO = null;
		HeatSourcePO?.Unregister(); HeatSourcePO = null;

		FloodwatersCategory?.Unregister(); FloodwatersCategory = null;
		FloodwatersEffectCategory?.Unregister(); FloodwatersEffectCategory = null;

		FWFogEffect?.Unregister(); FWFogEffect = null;
		FWChromaticEffect?.Unregister(); FWChromaticEffect = null;
		FWNoiseEffect?.Unregister(); FWNoiseEffect = null;

		HeatDanger?.Unregister(); HeatDanger = null;

		if (deleteBurnDamageType) BurnDamageType?.Unregister(); BurnDamageType = null;

		HeatDangerLoop?.Unregister(); HeatDangerLoop = null;
	}
}