namespace Floodwaters;

public static class CustomSaveData {
	public static readonly ConditionalWeakTable<PlayerProgression.MiscProgressionData, CustomProgressionData> customProgressionData = new ConditionalWeakTable<PlayerProgression.MiscProgressionData, CustomProgressionData>();

	public static void Initialize() {
		On.PlayerProgression.MiscProgressionData.ToString += On_MiscProgressionData_ToString;
		On.PlayerProgression.MiscProgressionData.FromString += On_MiscProgressionData_FromString;
	}

	public static void Cleanup() {
		On.PlayerProgression.MiscProgressionData.ToString -= On_MiscProgressionData_ToString;
		On.PlayerProgression.MiscProgressionData.FromString -= On_MiscProgressionData_FromString;
	}

	private static string On_MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self) {
		string text = orig(self);

		if (customProgressionData.TryGetValue(self, out CustomProgressionData data)) {
			text += "FWREGIONEXITKEYS<mpdB>" + string.Join(",", data.regionExitKeys) + "<mpdA>";
		}

		return text;
	}

	private static void On_MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s) {
		orig(self, s);

		CustomProgressionData data = customProgressionData.GetOrCreateValue(self);
		data.regionExitKeys.Clear();

		for (int i = self.unrecognizedSaveStrings.Count - 1; i >= 0; i--) {
			string[] array2 = Regex.Split(self.unrecognizedSaveStrings[i], "<mpdB>");
			switch (array2[0]) {
				case "FWREGIONEXITKEYS": {
					self.unrecognizedSaveStrings.RemoveAt(i);
					data.regionExitKeys = [.. array2[1].Split(',')];
					break;
				}
			}
		}
	}

	public class CustomProgressionData {
		public HashSet<string> regionExitKeys = [];
	}

	public static bool HasRegionExitKey(this PlayerProgression.MiscProgressionData self, string regionAcronym, string key) {
		return customProgressionData.TryGetValue(self, out CustomProgressionData data) && data.regionExitKeys.Contains(regionAcronym.ToLowerInvariant() + "-" + key);
	}

	public static void AddRegionExitKey(this PlayerProgression.MiscProgressionData self, string regionAcronym, string key) {
		customProgressionData.GetOrCreateValue(self).regionExitKeys.Add(regionAcronym.ToLowerInvariant() + "-" + key);
	}
}