namespace Floodwaters;

public class RestrictedEnums : EnumRegistry<Enums> {
	public static Enum<RoomSettings.RoomEffect.Type> SKLightningSaturation;

	public static new void Initialize() {
		if (ModManager.Watcher) {
			SKLightningSaturation = new(nameof(SKLightningSaturation));
		}

		EnumRegistry<Enums>.Initialize();
	}
}