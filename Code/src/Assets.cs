namespace Floodwaters;

public static class Assets {
	public static readonly string[] shaders = [
		"FWFog", "FWChromatic", "FWStatic",
		"FWCustomDepthVectorCircle", "FWCustomDepthGradient",
		"FW3dLightSource",
		"FWColoredDeepProcessing",
		"FWColoredFlameJet", "FWColoredFlameJetGlow",
		"FWWaterSlush", "FWWaterSludge", "FWStraightDeepWater",
		"FWEffectColor", "FWEffectColorCircle"
	];

	public static readonly int ShadPropFWBloomType = Shader.PropertyToID("_FWBloomType");

	public static void Initialize() {
		string bundlePath = AssetManager.ResolveFilePath("assetsbundles/floodwaters");
		if (!File.Exists(bundlePath)) return;

		AssetBundle assetBundle = null;

		try {
			assetBundle = AssetBundle.LoadFromFile(bundlePath);
			foreach (string shader in shaders) {
				Custom.rainWorld.Shaders.Add(shader, FShader.CreateShader(shader, assetBundle.LoadAsset<Shader>("Assets/Shaders/floodwaters/" + shader + ".shader")));
			}
		} catch (Exception ex) {
			Plugin.Log(ex);
		} finally {
			assetBundle?.Unload(false);
		}
	}

	public static void Cleanup() {
		foreach (string shader in shaders) {
			Custom.rainWorld.Shaders.Remove(shader);
		}
	}
}