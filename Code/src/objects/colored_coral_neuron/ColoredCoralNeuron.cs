namespace Floodwaters.Objects;

public class ColoredCoralNeuron : CoralNeuron {
	public readonly PlacedObject pObj;

	public ColoredCoralNeuronData Data => this.pObj.data as ColoredCoralNeuronData;

	public ColoredCoralNeuron(CoralNeuronSystem system, Room room, float length, Vector2? posA, Vector2? posB, PlacedObject pObj) : base(system, room, length, posA, posB) {
		this.pObj = pObj;
	}

	public Color MeshColor(float f, Color from) {
		if (this.Data == null) {
			return Color.black;
		}

		f = Mathf.Abs(f - 0.5f) * 2f;
		Vector3 offset = Custom.RGB2HSL(from);
	
		float hue = (offset.x + Custom.Decimal(Mathf.Lerp(1.025f, 0.9638889f, 0.5f + 0.5f * Mathf.Pow(f, 3f)))) % 1f;
		float sat = offset.y * Custom.LerpMap(f, 0.8f, 1f, 1f, 0.5f);
		float light = Mathf.Clamp01(offset.z * 2f * Custom.LerpMap(f, 0.7f, 1f, 0.5f, 0.15f));
	
		return Custom.HSL2RGB(hue, sat, light);
	}

	public void UpdateMyceliaColor(Mycelium self, Color startColor, Color midColor, Color endColor, float gradientStart, TriangleMesh triangle) {
		self.color = startColor;
		for (int i = 0; i < triangle.verticeColors.Length; i++) {
			float value = i / (float) (triangle.verticeColors.Length - 1);
			triangle.verticeColors[i] = Color.Lerp(startColor, midColor, Mathf.InverseLerp(gradientStart, 1f, value));
		}
		for (int j = 1; j < 3; j++) {
			triangle.verticeColors[triangle.verticeColors.Length - j] = endColor;
		}
	}

	public void Refresh(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; i++) {
			float f = i / (float)((sLeaser.sprites[0] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = this.MeshColor(f, this.Data.mainColor);
		}

		for (int i = 0; i < this.bumps.Length; i++) {
			int idx = 2 + this.mycelia.GetLength(0) * 2 + 3 * i;
			sLeaser.sprites[idx].color = Color.Lerp(this.MeshColor(this.bumps[i].y, this.Data.mainColor), this.Data.mainColor * 0.2f, 0.25f);
			Vector3 offset = Custom.RGB2HSL(this.Data.mainColor);
			sLeaser.sprites[idx + 2].color = Custom.HSL2RGB(offset.x, offset.y, Mathf.Clamp01(offset.z * 2f * (0.1f + 0.9f * Mathf.Lerp(this.bumpPings[i, 1], this.bumpPings[i, 0], timeStacker))));
		}

		int myceliaSpriteIndex = 2;
		for (int i = 0; i < this.mycelia.GetLength(0); i++) {
			for (int j = 0; j < 2; j++) {
				this.UpdateMyceliaColor(
					this.mycelia[i, j],
					this.MeshColor(Mathf.InverseLerp(0f, this.segments.GetLength(0) - 1, this.SegmentOfMycelium(i * 2 + j)), this.Data.tendrilStartColor),
					this.MeshColor(Mathf.InverseLerp(0f, this.segments.GetLength(0) - 1, this.SegmentOfMycelium(i * 2 + j)), this.Data.tendrilMidColor),
					this.MeshColor(Mathf.InverseLerp(0f, this.segments.GetLength(0) - 1, this.SegmentOfMycelium(i * 2 + j)), this.Data.tendrilEndColor),
					0f, sLeaser.sprites[myceliaSpriteIndex] as TriangleMesh
				);
				myceliaSpriteIndex++;
			}
		}
	}
}