namespace Floodwaters.Objects;

public class LittleIceCubes : UpdatableAndDeletable, IDrawable {
	public PlacedObject pObj;

	public Vector2 lastHandlePos;
	public float[] littleIslands;
	public Color snowColor;

	public PlacedObject.ResizableObjectData Data => this.pObj.data as PlacedObject.ResizableObjectData;

	public LittleIceCubes(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
	}


	public Color GetColor(float lightness, RoomCamera rCam) {
		return Color.Lerp(
			Color.Lerp(rCam.currentPalette.waterSurfaceColor1, rCam.currentPalette.waterSurfaceColor2, lightness * 2f),
			this.snowColor,
			lightness * 2f - 1.0f
		);
	}

	public FSprite CreateIsland(float rad) {
		TriangleMesh mesh = TriangleMesh.MakeLongMesh(Mathf.Max(Mathf.RoundToInt(rad / 6f), 2), false, true);
		float wasOffset = 0f;
		float spikeRand = Random.Range(0.8f, 1.5f);
		for (int i = 0; i < mesh.triangles.Length / 2; i += 2) {
			float left = i / (float) mesh.triangles.Length * 2f;
			float right = (i + 1) / (float) mesh.triangles.Length * 2f;
			Vector2 xL = new Vector2(Mathf.Lerp(-1f, 1f, left), 0f);
			Vector2 xR = new Vector2(Mathf.Lerp(-1f, 1f, right), 0f);
			wasOffset = Mathf.Lerp(wasOffset, Random.Range(-0.025f, 0.1f), 0.5f);
			spikeRand = Mathf.Lerp(spikeRand, Random.Range(0.8f, 1.5f), 0.33f);
			mesh.MoveVertice(i * 2 + 0, xL + Vector2.up * wasOffset * Mathf.Sin(Mathf.Acos(left * 2f - 1f)));
			mesh.MoveVertice(i * 2 + 1, xL + Vector2.down * (-0.5f + Mathf.Sin(Mathf.Acos(left * 1.5f - 0.75f))) * spikeRand);
			wasOffset = Mathf.Lerp(wasOffset, Random.Range(-0.025f, 0.1f), 0.5f);
			spikeRand = Mathf.Lerp(spikeRand, Random.Range(0.8f, 1.5f), 0.33f);
			mesh.MoveVertice(i * 2 + 2, xR + Vector2.up * wasOffset * Mathf.Sin(Mathf.Acos(right * 2f - 1f)));
			mesh.MoveVertice(i * 2 + 3, xR + Vector2.down * (-0.5f + Mathf.Sin(Mathf.Acos(right * 1.5f - 0.75f))) * spikeRand);
		}
		mesh._isAlphaDirty = true;
		return mesh;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		int littleIslandCount = Mathf.Max(Mathf.RoundToInt(this.Data.Rad / 20f), 0);
		sLeaser.sprites = new FSprite[littleIslandCount];

		this.littleIslands = new float[littleIslandCount];
		for (int i = 0; i < this.littleIslands.Length; i++) {
			this.littleIslands[i] = Random.Range(6f, 24f);
			sLeaser.sprites[i] = this.CreateIsland(this.littleIslands[i]);
		}

		this.lastHandlePos = this.Data.handlePos;

		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Midground");

		foreach (FSprite sprite in sLeaser.sprites) {
			sprite.RemoveFromContainer();
			newContatiner.AddChild(sprite);
		}
	}

	public void ApplyIslandColor(FSprite sprite, RoomCamera rCam, float bottomLightness) {
		TriangleMesh mesh = sprite as TriangleMesh;
		for (int i = 0; i < mesh.triangles.Length / 2; i += 2) {
			float left = i / (float) mesh.triangles.Length * 2f;
			float right = (i + 1) / (float) mesh.triangles.Length * 2f;
			mesh.verticeColors[i * 2 + 0] = this.GetColor(1f, rCam);
			mesh.verticeColors[i * 2 + 1] = this.GetColor(1f - (1f - bottomLightness) * Mathf.Sin(Mathf.Acos(left * 1.5f - 0.75f)), rCam);
			mesh.verticeColors[i * 2 + 2] = this.GetColor(1f, rCam);
			mesh.verticeColors[i * 2 + 3] = this.GetColor(1f - (1f - bottomLightness) * Mathf.Sin(Mathf.Acos(right * 1.5f - 0.75f)), rCam);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		Color snow = palette.texture.GetPixel(6, 1);
		Color snowLight = palette.texture.GetPixel(6, 9);
		this.snowColor = Color.Lerp(Color.Lerp(snow, snowLight, 0.75f), Color.white, 0.75f);

		for (int i = 0; i < this.littleIslands.Length; i++) {
			this.ApplyIslandColor(sLeaser.sprites[i], rCam, 0.25f);
		}
	}

	public Vector2 LittleIslandPosition(int i) {
		int j = (i % 2 == 0) ? i : (i - 1);
		float t = j / (this.littleIslands.Length - 1f) * Mathf.PI * 0.5f + (i % 2 == 0 ? 0f : Mathf.PI * 0.5f);
		return new Vector2(Mathf.Cos(t), Mathf.Sin(t));
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (this.lastHandlePos != this.Data.handlePos) {
			foreach (FSprite sprite in sLeaser.sprites) {
				sprite.RemoveFromContainer();
			}
			this.InitiateSprites(sLeaser, rCam);
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		if (this.room.waterObject is not null) {
			for (int i = 0; i < this.littleIslands.Length; i++) {
				FSprite sprite = sLeaser.sprites[i];
				Vector2 islandData = this.LittleIslandPosition(i);
				Vector2 islandPos = this.pObj.pos + new Vector2(islandData.x * this.Data.Rad * 1.5f, 0f);

				Water.Surface surface = this.room.waterObject.GetSurface(islandPos);
				int idx = Mathf.Clamp(surface.PreviousPoint(islandPos.x), 0, surface.points.GetLength(0) - 2);
				Vector2 a = Custom.ApplyDepthOnVector(surface.points[idx, 0].DrawPos(timeStacker) - camPos + Vector2.down * this.room.waterObject.cosmeticSurfaceDisplace, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.66666667f), -10f);
				Vector2 b = Custom.ApplyDepthOnVector(surface.points[idx, 1].DrawPos(timeStacker) - camPos + Vector2.down * this.room.waterObject.cosmeticSurfaceDisplace, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.66666667f), 30f);
				Vector2 c = Custom.ApplyDepthOnVector(surface.points[idx + 1, 0].DrawPos(timeStacker) - camPos + Vector2.down * this.room.waterObject.cosmeticSurfaceDisplace, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.66666667f), -10f);
				Vector2 d = Custom.ApplyDepthOnVector(surface.points[idx + 1, 1].DrawPos(timeStacker) - camPos + Vector2.down * this.room.waterObject.cosmeticSurfaceDisplace, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.66666667f), 30f);
				float depth = Mathf.Lerp(0.33f, 0.125f, islandData.y);
				Vector2 posLeft = Vector2.Lerp(a, b, depth);
				Vector2 posRight = Vector2.Lerp(c, d, depth);
				float leftRight = Mathf.InverseLerp(surface.points[idx, 0].defaultPos.x, surface.points[idx + 1, 0].defaultPos.x, islandPos.x);
				sprite.SetPosition(Vector2.Lerp(posLeft, posRight, leftRight) + Vector2.up * 3f);
				sprite.scaleX = this.littleIslands[i];
				sprite.scaleY = this.littleIslands[i] * 1.5f;
			}
		}

		if (sLeaser.deleteMeNextFrame || this.slatedForDeletetion || this.room != rCam.room) {
			this.slatedForDeletetion = true;
			sLeaser.CleanSpritesAndRemove();
		}
	}
}