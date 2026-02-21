namespace Floodwaters;

public static class Enums {
	private static readonly List<Action> CoreRegister = [];
	private static readonly List<Action> CoreUnregister = [];

	public class Enum<T> where T : ExtEnum<T> {
		public static readonly HashSet<T> Enums = [];

		public T Value;
		private bool existedBefore;
		private readonly string _id;

#pragma warning disable IDE1006
		public int index => this.Value.index;
#pragma warning restore IDE1006

		public Enum(string id, bool isFloodwaters = false) {
			this._id = id;

			CoreRegister.Add(() => {
				T t = (T) Activator.CreateInstance(typeof(T), this._id, false);
				this.existedBefore = t.index != -1;
				this.Value = this.existedBefore ? t : (T) Activator.CreateInstance(typeof(T), this._id, true);

				Enums.Add(this.Value);
			});
			CoreUnregister.Add(() => {
				if (!this.existedBefore)
					this.Value?.Unregister();

				Enums.Remove(this.Value);
				this.existedBefore = false;
				this.Value = null;
			});
		}

		public static implicit operator T(Enum<T> e) => e?.Value;
	}

	public static bool Has<T>(T t) where T : ExtEnum<T> {
		return Enum<T>.Enums.Contains(t);
	}

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
	public static Enum<PlacedObject.Type> BambooPO = new("Bamboo");	public static Enum<PlacedObject.Type> ColoredLanternPO = new("ColoredLantern");
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

	public static Enum<ObjectsPage.DevObjectCategories> FloodwatersCategory = new("Floodwaters");
	public static Enum<RoomSettingsPage.DevEffectsCategories> FloodwatersEffectCategory = new("Floodwaters");

	public static Enum<RoomSettings.RoomEffect.Type> FWFogEffect = new("FWFog");
	public static Enum<RoomSettings.RoomEffect.Type> FWChromaticEffect = new("Chromatic");
	public static Enum<RoomSettings.RoomEffect.Type> FWNoiseEffect = new("Noise");
	public static Enum<RoomSettings.RoomEffect.Type> EoCFanSpeedEffect = new("EoCFanSpeed");

	public static Enum<RoomRain.DangerType> HeatDanger = new("Heat");
	public static Enum<Creature.DamageType> BurnDamageType = new("Burn");

	public static Enum<SoundID> HeatDangerLoop = new("FW_HeatDangerLoop");

	public static void Initialize() {
		CoreRegister.ForEach(a => a());
	}

	public static void Cleanup() {
		CoreUnregister.ForEach(a => a());
	}
}