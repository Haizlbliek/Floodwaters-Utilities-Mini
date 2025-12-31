namespace Floodwaters;

public static class Assets {
	public static readonly string[] shaders = [ "FWFog", "FWChromatic", "FWStatic", "CustomDepthVectorCircle", "FWColoredDeepProcessing", "CustomDepthGradient" ];

	public static readonly int ShadPropFWBloomType = Shader.PropertyToID("_FWBloomType");

	public static void Initialize() {
		try {
			AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetsbundles/floodwaters"));
			foreach (string shader in shaders) {
				Custom.rainWorld.Shaders.Add(shader, FShader.CreateShader(shader, assetBundle.LoadAsset<Shader>("Assets/Shaders/" + shader + ".shader")));
			}
		} catch (Exception ex) {
			Plugin.Log(ex);
		}
	}

	public static void Cleanup() {
		foreach (string shader in shaders) {
			Custom.rainWorld.Shaders.Remove(shader);
		}
	}
}