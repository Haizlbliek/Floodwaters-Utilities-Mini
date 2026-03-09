namespace Floodwaters.Objects;

using UnityEngine;
using System;

public class PropertyCircularMesh : PropertyMesh {
	public int segmentCount;
	public FAtlasElement Element { get; private set; }

	public PropertyCircularMesh(string shaderName, string spriteName, Action<MaterialPropertyBlock> setCustomProperties, int segments = 64) : base(shaderName, setCustomProperties) {
		this.segmentCount = segments;
		this.Element = Futile.atlasManager.GetElementWithName(spriteName);

		this.GenerateCircularGeometry();

		this.SetCustomProperties += (mpb) => {
			mpb.SetTexture("_MainTex", this.Element.atlas.texture);
		};
	}

	private void GenerateCircularGeometry() {
		int vertCount = this.segmentCount + 1;
		Vector2[] newVerts = new Vector2[vertCount];
		Vector2[] newUVs = new Vector2[vertCount];
		int[] newIndices = new int[this.segmentCount * 3];

		Rect uvRect = this.Element.uvRect;
		float w = this.Element.sourceSize.x;
		float h = this.Element.sourceSize.y;

		newVerts[0] = Vector2.zero;
		newUVs[0] = uvRect.center;

		for (int i = 0; i < this.segmentCount; i++) {
			float angle = (float) i / this.segmentCount * Mathf.PI * 2f;
			float cos = Mathf.Cos(angle);
			float sin = Mathf.Sin(angle);

			newVerts[i + 1] = new Vector2(cos * (w / 2f), sin * (h / 2f));

			Vector2 normalizedUV = new Vector2(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);
			newUVs[i + 1] = new Vector2(
				Mathf.Lerp(uvRect.xMin, uvRect.xMax, normalizedUV.x),
				Mathf.Lerp(uvRect.yMin, uvRect.yMax, normalizedUV.y)
			);

			int triIndex = i * 3;
			newIndices[triIndex] = 0;
			newIndices[triIndex + 1] = i + 1;
			newIndices[triIndex + 2] = (i + 1 >= this.segmentCount) ? 1 : i + 2;
		}

		this.Vertices = newVerts;
		this.UVs = newUVs;
		this.Indices = newIndices;
	}
}
