namespace Floodwaters.Objects;

public class PaletteOverride : UpdatableAndDeletable, IDrawable {
	public readonly PlacedObject pObj;
	public PaletteOverrideData Data => this.pObj.data as PaletteOverrideData;

	private Texture2D paletteTexture;
	private Texture2D tempTexture;

	public PaletteOverride(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
		this.paletteTexture = new Texture2D(32, 8, TextureFormat.ARGB32, mipChain: false) {
			anisoLevel = 0,
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp
		};
		this.Refresh();
	}

	public void Refresh() {
		this.LoadPalette(this.Data.palette, ref this.paletteTexture);
		foreach ((int palette, float strength) in this.Data.fadePalettes) {
			this.LoadPalette(palette, ref this.tempTexture);
			this.FadeOntoMain(strength);
		}
		this.paletteTexture.Apply();
	}

	private void LoadPalette(int palette, ref Texture2D tex) {
		if (this.tempTexture != null) {
			UnityEngine.Object.Destroy(this.tempTexture);
		}
		this.tempTexture = new Texture2D(32, 16, TextureFormat.ARGB32, mipChain: false);
		string text = AssetManager.ResolveFilePath("palettes" + Path.DirectorySeparatorChar + "palette" + palette.ToString(CultureInfo.InvariantCulture) + ".png");
		try {
			AssetManager.SafeWWWLoadTexture(ref this.tempTexture, "file:///" + text, clampWrapMode: false, crispPixels: true);
		}
		catch (FileLoadException) {
			text = AssetManager.ResolveFilePath("palettes" + Path.DirectorySeparatorChar + "palette-1.png");
			AssetManager.SafeWWWLoadTexture(ref this.tempTexture, "file:///" + text, clampWrapMode: false, crispPixels: true);
		}
		this.tempTexture.Apply(updateMipmaps: false);

		for (int i = 0; i < 32; i++) {
			for (int j = 0; j < 8; j++) {
				tex.SetPixel(i, j, Color.Lerp(this.tempTexture.GetPixel(i, j + 8), this.tempTexture.GetPixel(i, j), this.room.game.cameras[0].DarkPalette));
			}
		}
	}

	private void FadeOntoMain(float strength) {
		for (int i = 0; i < 32; i++) {
			for (int j = 0; j < 8; j++) {
				this.paletteTexture.SetPixel(i, j, Color.Lerp(this.paletteTexture.GetPixel(i, j), this.tempTexture.GetPixel(i, j), strength));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = [];

		PropertyMesh mesh = new PropertySquareMesh("FWPaletteOverride", "Futile_White", mpb => {
			GradientStyle gradientStyle = this.Data.gradientStyle;
			float minOpacity = this.Data.minOpacity;
			float maxOpacity = this.Data.maxOpacity;

			if (gradientStyle == GradientStyle.Edges) {
				gradientStyle = GradientStyle.Center;
				(minOpacity, maxOpacity) = (maxOpacity, minOpacity);
			}
			else if (gradientStyle == GradientStyle.EdgesDepth) {
				gradientStyle = GradientStyle.CenterDepth;
				(minOpacity, maxOpacity) = (maxOpacity, minOpacity);
			}

			rCam.levelTexture.filterMode = FilterMode.Point;
			mpb.SetTexture("_CameraTexture", rCam.levelTexture);
			mpb.SetTexture("Palette", this.paletteTexture);
			mpb.SetFloat("Hue", 360f * this.Data.hue);
			mpb.SetFloat("Saturation", this.Data.saturation);
			mpb.SetFloat("Brightness", this.Data.brightness);
			mpb.SetFloat("Darkness", 1f - this.Data.darkness);
			mpb.SetFloat("Contrast", 1f + 2f * this.Data.contrast);
			mpb.SetFloat("Grime", this.Data.grime);
			mpb.SetFloat("WetTerrain", this.Data.wetTerrain ? 1f : 0f);
			mpb.SetInt("FromDepth", this.Data.fromDepth);
			mpb.SetInt("ToDepth", this.Data.toDepth);
			mpb.SetFloat("AffectSky", this.Data.affectSky ? 1f : 0f);
			mpb.SetFloat("MinOpacity", minOpacity);
			mpb.SetFloat("MaxOpacity", maxOpacity);
			mpb.SetFloat("UseCircle", this.Data.shape == PaletteOverrideData.Shape.Circle ? 1f : 0f);
			mpb.SetInt("GradientType", GradientStyle.GradientIndices[gradientStyle]);
			mpb.SetFloat("GradientValue", this.Data.gradientValue);

			float num4 = 1f;
			if (this.room.roomSettings.DangerType != RoomRain.DangerType.None) {
				num4 = this.room.world.rainCycle.ShaderLight;
			}
			if (this.room.lightning != null) {
				if (!this.room.lightning.bkgOnly) {
					num4 = this.room.lightning.CurrentLightLevel(0f); // TODO: timeStacker instead of 0f
				}
			}
			float num5 = rCam.spinningTopGhostMode ? 1f : rCam.ghostMode;
			if (this.Data.clouds == 0f) {
				mpb.SetFloat("CloudLight", 1f);
			}
			else {
				mpb.SetFloat("CloudLight", Mathf.Lerp(Mathf.Lerp(num4, -1f, this.Data.clouds), -0.4f, rCam.ghostMode));
			}
		}) {
			Vertices = [ new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) ],
			VertexColors = [ new Color(0f, 0f, 0f), new Color(1f, 0f, 0f), new Color(1f, 1f, 0f), new Color(0f, 1f, 0f) ],
		};

		sLeaser.containers = [ mesh ];
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Foreground");
		sLeaser.containers[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.containers[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		PropertyMesh mesh = sLeaser.containers[0] as PropertyMesh;
		if (this.Data.shape == PaletteOverrideData.Shape.Rectangle) {
			mesh.scaleX = this.Data.handlePos.x;
			mesh.scaleY = this.Data.handlePos.y;
			mesh.SetPosition(this.pObj.pos - camPos);
		}
		else {
			mesh.scale = this.Data.handlePos.magnitude * 2f;
			mesh.SetPosition(this.pObj.pos - camPos - new Vector2(this.Data.handlePos.magnitude, this.Data.handlePos.magnitude));
		}
		Vector2[] uvs = new Vector2[4];
		float left = (mesh.x - rCam.levelGraphic.x) / rCam.levelGraphic.width;
		float right = (mesh.x + mesh.scaleX - 1 - rCam.levelGraphic.x) / rCam.levelGraphic.width;
		float top = (mesh.y - rCam.levelGraphic.y) / rCam.levelGraphic.height;
		float bottom = (mesh.y + mesh.scaleY - rCam.levelGraphic.y) / rCam.levelGraphic.height;
		float ox = 1f / rCam.levelGraphic.width;
		float oy = -2f / rCam.levelGraphic.height;
		left += ox;
		right += ox;
		top += oy;
		bottom += oy;
		uvs[0] = new Vector2(left, top);
		uvs[1] = new Vector2(right, top);
		uvs[2] = new Vector2(right, bottom);
		uvs[3] = new Vector2(left, bottom);
		mesh.UVs = uvs;

		if (this.slatedForDeletetion) {
			mesh.Destroy();
			mesh.RemoveFromContainer();
			sLeaser.CleanSpritesAndRemove();
			UnityEngine.Object.Destroy(this.paletteTexture);
		}
	}
}