namespace Floodwaters;

public static class Merge {
	public static void ApplyMerge(ModManager.ModMerger merger) {
		try {
			foreach (KeyValuePair<string, List<ModManager.ModMerger.PendingApply>> pair in merger.moddedFiles) {
				if (pair.Value.Any(x => x.filePath.Contains("string.txt")))
					continue;
	
				string resolved = AssetManager.ResolveFilePath(pair.Key.Substring(1));
				Plugin.Log("pair.Key = " + pair.Key);
				Plugin.Log("resolved = " + resolved);
				if (!File.Exists(resolved) || string.IsNullOrEmpty(resolved)) {
					resolved = "";
				}
	
				string outPath = (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "mergedmods" + pair.Key).ToLowerInvariant();
				Directory.CreateDirectory(Path.GetDirectoryName(outPath));
	
				if (resolved == "" || resolved.Equals(outPath, StringComparison.InvariantCultureIgnoreCase)) {
					File.WriteAllText(outPath, "");
				}
				else {
					File.Copy(resolved, outPath, true);
				}
	
				List<ModManager.ModMerger.PendingApply> merges = [];
				List<ModManager.ModMerger.PendingApply> mods = [];
	
				for (int i = 0; i < pair.Value.Count; i++) {
					if (pair.Value[i].mergeLines != null) {
						merges.Add(pair.Value[i]);
					}
	
					if (pair.Value[i].isModification) {
						mods.Add(pair.Value[i]);
					}
				}
	
				foreach (ModManager.ModMerger.PendingApply apply in merges) {
					apply.ApplyMerges(apply.modApplyFrom, merger, outPath);
				}
	
				foreach (ModManager.ModMerger.PendingApply apply in mods) {
					apply.ApplyModifications(outPath);
				}
			}
		}
		catch (Exception ex) {
			Plugin.Log(ex);
			throw;
		}
	}

	public static void MergeCustomFiles() {
		Plugin.Log("Apply merges!");
	
		string path = Path.Combine(Custom.RootFolderDirectory(), "mergedmods", "gates", "vgates.txt");
		if (!File.Exists(path)) {
			Plugin.Log("Merging vgates");
			string filePath = Path.Combine("world", "gates", "vgates.txt");
			ModManager.ModMerger merger = new ModManager.ModMerger();
			foreach (ModManager.Mod mod in ModManager.ActiveMods.OrderBy(o => o.loadOrder)) {
				string text = Path.Combine(mod.path, Path.Combine("modify", filePath));
				if (File.Exists(text)) {
					string basePath = Path.DirectorySeparatorChar.ToString() + filePath;
					merger.AddPendingApply(mod, basePath, text, true);
				}
			}
	
			ApplyMerge(merger);
		}
	}
}