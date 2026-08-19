namespace Floodwaters;

public class Enums : EnumRegistry<Enums> {
	public static Enum<AbstractPhysicalObject.AbstractObjectType> CactusFruit = new("CactusFruit");
	public static Enum<AbstractPhysicalObject.AbstractObjectType> CactusSpear = new("CactusSpear");
	public static Enum<AbstractPhysicalObject.AbstractObjectType> Cattail = new("Cattail");
	public static Enum<AbstractPhysicalObject.AbstractObjectType> ColoredLantern = new("ColoredLantern");
	public static Enum<AbstractPhysicalObject.AbstractObjectType> Lillypad = new("Lillypad");
	public static Enum<AbstractPhysicalObject.AbstractObjectType> IceCube = new("IceCube");

	public static Enum<PlacedObject.Type> CactusPO = new("Cactus");
	public static Enum<PlacedObject.Type> SandDripPO = new("SandDrip");
	public static Enum<PlacedObject.Type> DeerSkullPO = new("DeerSkull");
	public static Enum<PlacedObject.Type> CattailPO = new("Cattail");
	public static Enum<PlacedObject.Type> ColoredCattailPO = new("ColoredCattail");
	public static Enum<PlacedObject.Type> BubbleEmitterPO = new("BubbleEmitter");
	public static Enum<PlacedObject.Type> BambooPO = new("Bamboo");
	public static Enum<PlacedObject.Type> ColoredLanternPO = new("ColoredLantern");
	public static Enum<PlacedObject.Type> ColoredLanternStickPO = new("ColoredLanternStick");
	public static Enum<PlacedObject.Type> LillypadPO = new("Lillypad");
	public static Enum<PlacedObject.Type> WaterDripsPO = new("WaterDrips");
	public static Enum<PlacedObject.Type> MagmaAreaPO = new("MagmaArea");
	public static Enum<PlacedObject.Type> HeatSourcePO = new("HeatSource");
	public static Enum<PlacedObject.Type> ColoredCoralNeuronPO = new("ColoredCoralNeuron");
	public static Enum<PlacedObject.Type> ColoredDeepProcessingPO = new("ColoredDeepProcessing");
	public static Enum<PlacedObject.Type> CustomVinePO = new("CustomVine");
	public static Enum<PlacedObject.Type> CustomVineConnectorPO = new("CustomVineConnector");
	public static Enum<PlacedObject.Type> CustomLightRodPO = new("CustomLightRod");
	public static Enum<PlacedObject.Type> CustomLightArcPO = new("CustomLightArc");
	public static Enum<PlacedObject.Type> IceCubePO = new("IceCube");
	public static Enum<PlacedObject.Type> LittleIceCubesPO = new("LittleIceCubes");
	public static Enum<PlacedObject.Type> ColoredSparksPO = new("ColoredSparks");
	public static Enum<PlacedObject.Type> LightSource3dPO = new("3dLightSource");
	public static Enum<PlacedObject.Type> ColoredLightSource3dPO = new("Colored3dLightSource");
	public static Enum<PlacedObject.Type> VerticalGatePositionPO = new("VerticalGatePosition");
	public static Enum<PlacedObject.Type> ColoredFlameJetPO = new("ColoredFlameJet");
	public static Enum<PlacedObject.Type> EffectOverrideRectPO = new("EffectOverrideRect");
	public static Enum<PlacedObject.Type> EffectOverrideCirclePO = new("EffectOverrideCircle");
	public static Enum<PlacedObject.Type> SmokePipe = new("SmokePipe");

	public static Enum<ObjectsPage.DevObjectCategories> FloodwatersCategory = new("Floodwaters");
	public static Enum<RoomSettingsPage.DevEffectsCategories> FloodwatersEffectCategory = new("Floodwaters");

	public static Enum<RoomSettings.RoomEffect.Type> FWFogEffect = new("FWFog");
	public static Enum<RoomSettings.RoomEffect.Type> FWChromaticEffect = new("Chromatic");
	public static Enum<RoomSettings.RoomEffect.Type> FWNoiseEffect = new("Noise");
	public static Enum<RoomSettings.RoomEffect.Type> EoCFanSpeedEffect = new("EoCFanSpeed");
	public static Enum<RoomSettings.RoomEffect.Type> WaterSlush = new("WaterSlush");
	public static Enum<RoomSettings.RoomEffect.Type> WaterSludge = new("WaterSludge");
	public static Enum<RoomSettings.RoomEffect.Type> StraightDeepWater = new("StraightDeepWater");
	public static Enum<RoomSettings.RoomEffect.Type> Hypothermia = new("Hypothermia");
	public static Enum<RoomSettings.RoomEffect.Type> NoDeathFallGradient = new("NoDeathFallGradient");

	public static Enum<RoomRain.DangerType> HeatDanger = new("Heat");
	public static Enum<Creature.DamageType> BurnDamageType = new("Burn");

	public static Enum<SoundID> HeatDangerLoop = new("FW_HeatDangerLoop");

	public static Enum<PlacedObject.LightSourceData.BlinkType> Flicker = new("Flicker");
}