
namespace Floodwaters.Effects;

public static class RainRotation {
	private class RoomData {
		public float lastDirection;
		public List<SplashPoint> splashPoints = [];

		public class SplashPoint {
			public IntVector2 tilePos;
			public Direction direction;

			public enum Direction {
				Left,
				Right,
				Up,
				Down,
			}
		}
	}

	private static readonly ConditionalWeakTable<Room, RoomData> roomData = new ConditionalWeakTable<Room, RoomData>();

	public static void Initialize() {
		On.Room.Update += On_Room_Update;
		On.RoomRain.DrawSprites += On_RoomRain_DrawSprites;
		On.RoomRain.GenerateShelterTex += On_RoomRain_GenerateShelterTex;
	}

	public static void Cleanup() {
		On.Room.Update -= On_Room_Update;
		On.RoomRain.DrawSprites -= On_RoomRain_DrawSprites;
		On.RoomRain.GenerateShelterTex -= On_RoomRain_GenerateShelterTex;
	}

	private static void On_Room_Update(On.Room.orig_Update orig, Room self) {
		orig(self);

		RoomSettings.RoomEffect effect = self.roomSettings.GetEffect(Enums.RainRotation);
		if (effect == null)
			return;

		RoomData data = roomData.GetOrCreateValue(self);
		if (data.lastDirection != effect.amount) {
			data.lastDirection = effect.amount;
			RoomRain.GenerateShelterTex(self.roomRain, self);
		}
	}

	private static void On_RoomRain_DrawSprites(On.RoomRain.orig_DrawSprites orig, RoomRain self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		RoomSettings.RoomEffect effect = self.room.roomSettings.GetEffect(Enums.RainRotation);
		RoomData data = roomData.GetOrCreateValue(self.room);

		if (effect != null) {
			Shader.SetGlobalFloat(Assets.ShadPropFWRainRotation, effect.amount * Mathf.PI * 2f);

			if (sLeaser.sprites.Length != 1 + self.splashes) {
				sLeaser.RemoveAllSpritesFromContainer();
				self.InitiateSprites(sLeaser, rCam);
				self.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}

			if (self.shelterTex != null && sLeaser.sprites[0].element.atlas.texture != self.shelterTex) {
				sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("RainMask_" + self.room.abstractRoom.name);
			}

			int splashes = self.splashes;
			self.splashes = 0;
			orig(self, sLeaser, rCam, timeStacker, camPos);
			self.splashes = splashes;

			for (int j = 1; j < self.splashes; j++) {
				FSprite sprite = sLeaser.sprites[j];
				sprite.isVisible = false;
				sprite.rotation = 0f;
				if (data.splashPoints.Count > 0) {
					for (int k = 0; k < 5; k++) {
						RoomData.SplashPoint splashPoint = data.splashPoints[Random.Range(0, data.splashPoints.Count)];
						Vector2 vector = self.room.MiddleOfTile(splashPoint.tilePos);
						if (vector.y > self.room.FloatWaterLevel(vector) && rCam.IsVisibleAtCameraPosition(rCam.currentCameraPosition, vector)) {
							sprite.x = vector.x - camPos.x;
							sprite.y = vector.y - camPos.y;
							switch (splashPoint.direction) {
								case RoomData.SplashPoint.Direction.Down:
									sprite.rotation = 0f;
									sprite.x += Mathf.Lerp(-10f, 10f, Random.value);
									sprite.y += 10f;
									break;
								case RoomData.SplashPoint.Direction.Up:
									sprite.rotation = Mathf.PI;
									sprite.x += Mathf.Lerp(-10f, 10f, Random.value);
									sprite.y -= 10f;
									break;
								case RoomData.SplashPoint.Direction.Left:
									sprite.rotation = Mathf.PI / 2f;
									sprite.x -= 10f;
									sprite.y += Mathf.Lerp(-10f, 10f, Random.value);
									break;
								case RoomData.SplashPoint.Direction.Right:
									sprite.rotation = Mathf.PI / 2f * 3f;
									sprite.x += 10f;
									sprite.y += Mathf.Lerp(-10f, 10f, Random.value);
									break;
							}
							sprite.isVisible = true;
							break;
						}
					}
				}
				sprite.scaleY = self.SplashSize;
				if (j < self.splashes / 2) {
					sprite.rotation += Mathf.Lerp(-45f, 45f, Random.value);
					sprite.scaleX = ((Random.value < 0.5f) ? -1f : 1f) * self.SplashSize;
				}
				else {
					sprite.rotation += Mathf.Lerp(-25f, 25f, Random.value);
					sprite.scaleX = Mathf.Lerp(-1f, 1f, Random.value) * self.SplashSize;
				}
				sprite.color = Color.Lerp(self.pal.fogColor, new Color(1f, 1f, 1f), Random.value * self.intensity);
			}
		}
		else {
			orig(self, sLeaser, rCam, timeStacker, camPos);
		}

		if (sLeaser.sprites[0].shader.name == "DeathRain" && effect != null) {
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["FWDeathRain"];
		}
		else if (sLeaser.sprites[0].shader.name == "FWDeathRain" && effect == null) {
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["DeathRain"];
		}
	}

	private static void On_RoomRain_GenerateShelterTex(On.RoomRain.orig_GenerateShelterTex orig, RoomRain rain, Room rm) {
		RoomSettings.RoomEffect effect = rm.roomSettings.GetEffect(Enums.RainRotation);
		if (effect == null) {
			orig(rain, rm);
			return;
		}

		RoomData data = roomData.GetOrCreateValue(rm);

		rain.splashTiles.Clear();
		data.splashPoints.Clear();

		float angle = effect.amount * Mathf.PI * 2f;
		Vector2 rainDir = new Vector2(-Mathf.Sin(angle), -Mathf.Cos(angle)).normalized;

		Texture2D texture2D = new Texture2D(rm.TileWidth, rm.TileHeight);
		bool[,] isAir = new bool[rm.TileWidth, rm.TileHeight];

		if (rain != null) {
			for (int i = 0; i < rm.TileWidth; i++) {
				rain.rainReach[i] = -1;
			}
		}

		if (rainDir.y < 0) {
			for (int x = 0; x < rm.TileWidth; x++) {
				TraceRainRay(rm, rain, data, ref isAir, new IntVector2(x, rm.TileHeight - 1), rainDir);
			}
		}
		else if (rainDir.y > 0) {
			for (int x = 0; x < rm.TileWidth; x++) {
				TraceRainRay(rm, rain, data, ref isAir, new IntVector2(x, 0), rainDir);
			}
		}
		if (rainDir.x < 0) {
			for (int y = 0; y < rm.TileHeight; y++) {
				TraceRainRay(rm, rain, data, ref isAir, new IntVector2(rm.TileWidth - 1, y), rainDir);
			}
		}
		else if (rainDir.x > 0) {
			for (int y = 0; y < rm.TileHeight; y++) {
				TraceRainRay(rm, rain, data, ref isAir, new IntVector2(0, y), rainDir);
			}
		}

		for (int x = 0; x < rm.TileWidth; x++) {
			for (int y = 0; y < rm.TileHeight; y++) {
				texture2D.SetPixel(x, y, isAir[x, y] ? new Color(1f, 0f, 0f) : new Color(0f, 0f, 0f));
			}
		}

		if (rm.water) {
			for (int j = 0; j < rm.TileWidth; j++) {
				if (!rm.GetTile(j, rm.defaultWaterLevel).Solid) {
					texture2D.SetPixel(j, rm.defaultWaterLevel, (texture2D.GetPixel(j, rm.defaultWaterLevel).r > 0.5f) ? new Color(1f, 0f, 1f) : new Color(0f, 0f, 1f));
					for (int k = rm.defaultWaterLevel; k < rm.TileHeight && k < rm.defaultWaterLevel + 20f && !rm.HasAnySolid(j, k); k++) {
						texture2D.SetPixel(j, k + 1, (texture2D.GetPixel(j, k + 1).r > 0.5f) ? new Color(1f, 0f, 1f) : new Color(0f, 0f, 1f));
					}
				}
			}
		}
		else {
			for (int l = 0; l < rm.TileWidth; l++) {
				if (rm.HasAnySolid(l, 0)) {
					texture2D.SetPixel(l, 0, (texture2D.GetPixel(l, 0).r > 0.5f) ? new Color(1f, 0f, 1f) : new Color(0f, 0f, 1f));
				}
			}
		}

		texture2D.wrapMode = TextureWrapMode.Clamp;
		string atlasName = "RainMask_" + rm.abstractRoom.name;
		texture2D.Apply();
		FAtlas atlas = Futile.atlasManager.GetAtlasWithName(atlasName);
		if (atlas != null) {
			UnityEngine.Object.Destroy(atlas.texture);
		}
		// atlas._texture = texture2D;
		// atlas._textureSize = new Vector2(texture2D.width, texture2D.height);
		HeavyTexturesCache.UnloadAtlasTexture(atlasName, textureFromAsset: false);
		HeavyTexturesCache.LoadAndCacheAtlasFromTexture(atlasName, texture2D, textureFromAsset: false);
		rain?.shelterTex = texture2D;

		rain.splashes = data.splashPoints.Count * 2 / rm.cameraPositions.Length;
	}

	private static void TraceRainRay(Room rm, RoomRain rain, RoomData data, ref bool[,] isAir, IntVector2 startPos, Vector2 dir) {
		int tileX = startPos.x;
		int tileY = startPos.y;
		if (rm.HasAnySolid(tileX, tileY) || tileX < 0 || tileX >= rm.TileWidth || tileY < 0 || tileY >= rm.TileHeight) {
			return;
		}

		int stepX = dir.x >= 0 ? 1 : -1;
		int stepY = dir.y >= 0 ? 1 : -1;
		float deltaX = (dir.x == 0) ? float.MaxValue : Mathf.Abs(1f / dir.x);
		float deltaY = (dir.y == 0) ? float.MaxValue : Mathf.Abs(1f / dir.y);
		float sideDistX = (dir.x >= 0)
			? (Mathf.Floor(startPos.x) + 1f - startPos.x) * deltaX
			: (startPos.x - Mathf.Floor(startPos.x)) * deltaX;
		float sideDistY = (dir.y >= 0)
			? (Mathf.Floor(startPos.y) + 1f - startPos.y) * deltaY
			: (startPos.y - Mathf.Floor(startPos.y)) * deltaY;
		int lastTileX = tileX;
		int lastTileY = tileY;

		while (tileX >= 0 && tileX < rm.TileWidth && tileY >= 0 && tileY < rm.TileHeight) {
			if (rm.HasAnySolid(tileX, tileY)) {
				if (rain != null) {
					int splashX = Mathf.Clamp(lastTileX, 0, rm.TileWidth - 1);
					int splashY = Mathf.Clamp(lastTileY, 0, rm.TileHeight - 1);

					data.splashPoints.Add(new RoomData.SplashPoint() {
						tilePos = new IntVector2(splashX, splashY),
						direction = lastTileX == tileX ? (lastTileY < tileY ? RoomData.SplashPoint.Direction.Down : RoomData.SplashPoint.Direction.Up) : (lastTileX < tileX ? RoomData.SplashPoint.Direction.Right : RoomData.SplashPoint.Direction.Left)
					});
				}
				return;
			}

			isAir[tileX, tileY] = true;
			lastTileX = tileX;
			lastTileY = tileY;

			if (sideDistX < sideDistY) {
				sideDistX += deltaX;
				tileX += stepX;
			}
			else {
				sideDistY += deltaY;
				tileY += stepY;
			}
		}
	}
}