namespace Floodwaters.Objects;

using UnityEngine;

public class PropertySquareMesh : PropertyMesh {
	public PropertySquareMesh(string shaderName, string spriteName, Action<MaterialPropertyBlock> setCustomProperties)
		: base(shaderName, setCustomProperties) {
		this.Vertices = [
			new Vector2(-0.5f, -0.5f),
			new Vector2( 0.5f, -0.5f),
			new Vector2( 0.5f,  0.5f),
			new Vector2(-0.5f,  0.5f)
		];

		this.Indices = [0, 2, 1, 0, 3, 2];

		FAtlasElement element = Futile.atlasManager.GetElementWithName(spriteName);
		Rect uv = element.uvRect;

		this.UVs = [
			new Vector2(uv.xMin, uv.yMin),
			new Vector2(uv.xMax, uv.yMin),
			new Vector2(uv.xMax, uv.yMax),
			new Vector2(uv.xMin, uv.yMax)
		];

		this.SetCustomProperties += (mpb) => {
			mpb.SetTexture("_MainTex", element.atlas.texture);
		};
	}
}
