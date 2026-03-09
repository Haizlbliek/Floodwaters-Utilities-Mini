using System.CodeDom;

namespace Floodwaters.Objects;

public class LightSource3d : LightSource {
	public int depth;
	public int depthRange;
	protected PropertyCircularMesh mesh;

	public LightSource3d(Vector2 initPos, bool environmentalLight, Color color, UpdatableAndDeletable tiedToObject) : base(initPos, environmentalLight, color, tiedToObject) {
	}

	public static void InitiateSprites(LightSource3d self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		self.mesh = new PropertyCircularMesh("FW3dLightSource", self.ElementName, mpb => {
			mpb.SetInt("_Depth", self.depth);
			mpb.SetInt("_DepthRange", self.depthRange);
		}) {
			Color = self.color,
		};
		sLeaser.containers = [ self.mesh ];

		sLeaser.sprites = new FSprite[rCam.room.water ? 2 : 1];
		sLeaser.sprites[0] = new CircularSprite(self.ElementName) {
			isVisible = false,
		};

		if (rCam.room.water) {
			sLeaser.sprites[0] = new FSprite("Futile_White") {
				shader = rCam.room.game.rainWorld.Shaders["UnderWaterLight"],
				color = self.color
			};
		}

		self.AddToContainer(sLeaser, rCam, null);
		sLeaser.containers[0].RemoveFromContainer();
		rCam.ReturnFContainer(self.LayerName).AddChild(sLeaser.containers[0]);
	}

	public static void DrawSprites(LightSource3d self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		self.flat = false;
		self.shaderDirty = false;

		float num = 1f;
		float num2 = (1.01f - self.blinkRate) * 1000f;
		if (self.blinkType == PlacedObject.LightSourceData.BlinkType.Flash) {
			num2 /= 4f;
		}
		if (self.blinkType == PlacedObject.LightSourceData.BlinkType.Flash && self.blinkTicker % (num2 * 2f) <= num2) {
			num = 0f;
		}
		else if (self.blinkType == PlacedObject.LightSourceData.BlinkType.Fade) {
			num = (Mathf.Sin(self.blinkTicker % num2 / num2 * (float) Math.PI * 2f) + 1f) / 2f;
		}
		num *= 1f - rCam.room.darkenLightsFactor;
		if (sLeaser.sprites.Length > 1) {
			float num3 = Mathf.Lerp(self.lastPos.y, self.pos.y, timeStacker);
			float num4 = Mathf.InverseLerp(self.waterSurfaceLevel - self.rad * 0.25f, self.waterSurfaceLevel + self.rad * 0.25f, num3);
			float num5 = Mathf.Lerp(self.lastRad, self.rad, timeStacker) * 0.5f * Mathf.Pow(1f - num4, 0.5f);
			sLeaser.sprites[1].x = Mathf.Floor(Mathf.Lerp(self.lastPos.x, self.pos.x, timeStacker) - camPos.x) + 0.5f;
			sLeaser.sprites[1].y = Mathf.Floor(Mathf.Min(num3, Mathf.Lerp(num3, self.waterSurfaceLevel - num5 * 0.5f, 0.5f)) - camPos.y) + 0.5f;
			sLeaser.sprites[1].color = self.color;
			sLeaser.sprites[1].alpha = Mathf.Lerp(self.lastAlpha, self.Alpha, timeStacker) * Mathf.Lerp(1f, rCam.room.Darkness(self.pos), self.affectedByPaletteDarkness) * Mathf.Pow(1f - num4, 0.5f) * self.colorAlpha * num;
			if (ModManager.DLCShared && rCam.room.waterInverted) {
				num4 = 1f - Mathf.InverseLerp(self.waterSurfaceLevel - self.rad * 0.25f, self.waterSurfaceLevel + self.rad * 0.25f, num3);
				num5 = Mathf.Lerp(self.lastRad, self.rad, timeStacker) * 0.5f * Mathf.Pow(1f - num4, 0.5f);
				sLeaser.sprites[1].y = Mathf.Floor(Mathf.Min(num3, Mathf.Lerp(num3, num5 - self.waterSurfaceLevel * 0.5f, 0.5f)) - camPos.y) + 0.5f;
			}
			sLeaser.sprites[1].scale = num5 / 8f;
		}
		sLeaser.sprites[0].isVisible = false;

		self.mesh.x = Mathf.Floor(Mathf.Lerp(self.lastPos.x, self.pos.x, timeStacker) - camPos.x) + 0.5f;
		self.mesh.y = Mathf.Floor(Mathf.Lerp(self.lastPos.y, self.pos.y, timeStacker) - camPos.y) + 0.5f;
		self.mesh.Color = self.color;
		self.mesh.Alpha = Mathf.Lerp(self.lastAlpha, self.Alpha, timeStacker) * Mathf.Lerp(1f, rCam.room.Darkness(self.pos), self.affectedByPaletteDarkness) * self.colorAlpha * num;
		self.mesh.scale = Mathf.Lerp(self.lastRad, self.rad, timeStacker) / 8f;

		if (self.slatedForDeletetion || self.room != rCam.room) {
			sLeaser.CleanSpritesAndRemove();
		}
	}
}