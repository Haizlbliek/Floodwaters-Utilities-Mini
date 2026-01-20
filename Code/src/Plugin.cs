using System.Security.Permissions;
using BepInEx;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Floodwaters;

[BepInPlugin(MOD_ID, "Floodwaters Utilities Mini", "1.0.6")]
public class Plugin : BaseUnityPlugin {
	public const string MOD_ID = "floodwaters_utilities_mini";

	public static bool HasInitialized { get; private set; } = false;
	
	public static Plugin instance;
	
	public static void Log(object data) {
		instance.SelfLog(data);
	}
	
	public void SelfLog(object data) {
		Debug.Log("[Floodwaters] " + data);
		this.Logger.LogInfo(data);
	}

	public void OnEnable() {
		instance = this;

		On.RainWorld.OnModsInit += this.OnModsInit;
		On.RainWorld.OnModsDisabled += this.OnModsDisabled;

		Enums.Initialize();
	}
	
	private void Initialize() {
		if (HasInitialized)
			return;

		Objects.Objects.Initialize();
		Effects.Effects.Initialize();
		Creatures.Creatures.Initialize();
		Assets.Initialize();

		HasInitialized = true;
	}
	
	private void Cleanup() {
		Enums.Cleanup();
		Objects.Objects.Cleanup();
		Effects.Effects.Cleanup();
		Creatures.Creatures.Cleanup();
		Assets.Cleanup();

		HasInitialized = false;
	}

	private void OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods) {
		orig(self, newlyDisabledMods);

		if (newlyDisabledMods.Any(x => x.id == MOD_ID))
			this.Cleanup();
	}

	private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
		orig(self);

		this.Initialize();
	}

	public void OnDisable() {
		Enums.Cleanup();
	}
}