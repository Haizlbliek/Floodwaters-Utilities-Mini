namespace Floodwaters;

public static class AcronymFix {
	public static void Initialize() {
		IL.Menu.FastTravelScreen.ctor += IL_Menu_FastTravelScreen_ctor;
		IL.SaveState.SaveToString += IL_SaveState_SaveToString;
	}

	public static void Cleanup() {
		IL.Menu.FastTravelScreen.ctor -= IL_Menu_FastTravelScreen_ctor;
		IL.SaveState.SaveToString -= IL_SaveState_SaveToString;
	}

	private static string SubstringRegion(string room) {
		if (room.IndexOf("_") == -1) {
			return room;
		}

		return room.Substring(0, room.IndexOf("_"));
	}

	private static void IL_SaveState_SaveToString(ILContext il) {
		ILCursor c = new ILCursor(il);
		while (c.TryGotoNext(
			i => i.MatchLdcI4(0),
			i => i.MatchLdcI4(2),
			i => i.MatchCallOrCallvirt(typeof(string).GetMethod(nameof(string.Substring), [ typeof(int), typeof(int) ]))
		)) {
			c.RemoveRange(3);
			c.EmitDelegate(SubstringRegion);
		}
	}

	private static void IL_Menu_FastTravelScreen_ctor(ILContext il) {
		ILCursor c = new ILCursor(il);
		while (c.TryGotoNext(
			i => i.MatchLdcI4(0),
			i => i.MatchLdcI4(2),
			i => i.MatchCallOrCallvirt(typeof(string).GetMethod(nameof(string.Substring), [ typeof(int), typeof(int) ]))
		)) {
			c.RemoveRange(3);
			c.EmitDelegate((Menu.FastTravelScreen self, string shelterName) => {
				return SubstringRegion(shelterName);
			});
		}
	}
}