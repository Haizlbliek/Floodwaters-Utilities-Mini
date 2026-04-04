namespace Floodwaters.Objects;

public class ColoredFlameJet : FlameJet {
	public static void Initialize() {
		On.Watcher.FlameJet.InitiateSprites += InitiateSprites;
		On.Watcher.FlameJet.AddToContainer += AddToContainer;
		On.Watcher.FlameJet.DrawSprites += DrawSprites;
	}

	public static void Cleanup() {
		On.Watcher.FlameJet.InitiateSprites -= InitiateSprites;
		On.Watcher.FlameJet.AddToContainer -= AddToContainer;
		On.Watcher.FlameJet.DrawSprites -= DrawSprites;
	}

	public PropertyMesh jetMesh;
	public PropertyMesh glowMesh;
	public PropertyCircularMesh light0;
	public PropertyCircularMesh light1;
	public PropertyCircularMesh light2;

	public ColoredFlameJetData data;

	public ColoredFlameJet(Room room, ColoredFlameJetData data) : base(room, data) {
		this.data = data;
	}


	public static void InitiateSprites(On.Watcher.FlameJet.orig_InitiateSprites orig, FlameJet self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		if (self is not ColoredFlameJet jet) {
			orig(self, sLeaser, rCam);
			return;
		}

		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White") {
			shader = Custom.rainWorld.Shaders["HeatDistortion"],
			anchorY = 0.2f
		};

		jet.light0 = new PropertyCircularMesh("FW3dLightSource", "Futile_White", mpb => jet.SetMPBLight(mpb, false));
		jet.light1 = new PropertyCircularMesh("FW3dLightSource", "Futile_White", mpb => jet.SetMPBLight(mpb, true));
		jet.light2 = new PropertyCircularMesh("FW3dLightSource", "Futile_White", mpb => jet.SetMPBLight(mpb, true));

		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[jet.nJetPos * 4];
		for (int i = 0; i < jet.nJetPos - 1; i++) {
			int num = i * 3;
			int num2 = i * 4;
			tris[num2] = new TriangleMesh.Triangle(num, num + 1, num + 3);
			tris[num2 + 1] = new TriangleMesh.Triangle(num + 3, num + 1, num + 4);
			tris[num2 + 2] = new TriangleMesh.Triangle(num + 1, num + 2, num + 4);
			tris[num2 + 3] = new TriangleMesh.Triangle(num + 4, num + 2, num + 5);
		}
		(Vector2[] vertices, int[] indices) = Utils.TrianglesToPoints(tris);
		Vector2[] uvs1 = new Vector2[vertices.Length];
		Vector2[] uvs2 = new Vector2[vertices.Length];
		for (int j = 0; j < jet.nJetPos; j++) {
			int num3 = j * 3;
			uvs1[num3] = Vector2.Lerp(Vector2.zero, Vector2.right, j / (jet.nJetPos - 1f));
			uvs1[num3 + 1] = new Vector2(uvs1[num3].x, 0.5f);
			uvs1[num3 + 2] = new Vector2(uvs1[num3].x, 1f);
			uvs2[num3] = uvs1[num3];
			uvs2[num3 + 1] = uvs1[num3 + 1];
			uvs2[num3 + 2] = uvs1[num3 + 2];
		}

		jet.jetMesh = new PropertyMesh("FWColoredFlameJet", jet.SetMPBColors) { Vertices = vertices, Indices = indices, UVs = uvs1, VertexColors = new Color[vertices.Length] };
		jet.glowMesh = new PropertyMesh("FWColoredFlameJetGlow", jet.SetMPBColors) { Vertices = [..vertices], Indices = indices, UVs = uvs2, VertexColors = new Color[vertices.Length] };

		sLeaser.containers = [ jet.jetMesh, jet.light0, jet.light1, jet.light2, jet.glowMesh ];

		jet.AddToContainer(sLeaser, rCam, null);
	}

	public void SetMPBColors(MaterialPropertyBlock mpb) {
		mpb.SetInt("_PaletteSmoke", this.data.paletteSmoke ? 1 : 0);
		mpb.SetColor("_SmokeColor", this.data.smokeColor);
		mpb.SetColor("_DarkColor", this.data.darkColor);
		mpb.SetColor("_MidColor", this.data.midColor);
		mpb.SetColor("_LightColor", this.data.lightColor);
		mpb.SetColor("_BrightColor", this.data.brightColor);
		mpb.SetInt("_Depth", this.data.depth);
	}

	public void SetMPBLight(MaterialPropertyBlock mpb, bool small) {
		float size = this.target.magnitude * 0.12f * (0.5f + this.finalIntensity[this.nJetPos / 2] * 0.5f);

		mpb.SetInt("_Depth", this.data.depth);
		mpb.SetInt("_DepthRange", Mathf.RoundToInt((small ? 0.15f : 3f) * size));
	}

	public static void AddToContainer(On.Watcher.FlameJet.orig_AddToContainer orig, FlameJet self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		if (self is not ColoredFlameJet jet) {
			orig(self, sLeaser, rCam, newContatiner);
			return;
		}

		rCam.ReturnFContainer("Midground").AddChild(jet.jetMesh);
		rCam.ReturnFContainer("Bloom").AddChild(jet.glowMesh);
		FContainer foregroundLights = rCam.ReturnFContainer("ForegroundLights");
		foregroundLights.AddChild(jet.light0);
		foregroundLights.AddChild(jet.light1);
		foregroundLights.AddChild(jet.light2);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
	}

	private static void DrawSprites(On.Watcher.FlameJet.orig_DrawSprites orig, FlameJet self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (self is not ColoredFlameJet jet) {
			orig(self, sLeaser, rCam, timeStacker, camPos);
			return;
		}

		Vector2 vector = Vector2.Lerp(jet.lastPos, jet.pos, timeStacker) - camPos;
		int num = jet.nJetPos / 2;
		jet.jetMesh.SetPosition(vector);
		jet.glowMesh.SetPosition(vector);
		Vector2 vector2 = Vector2.Lerp(jet.lightPos[1], jet.lightPos[0], timeStacker) * jet.target.magnitude / 500f;
		Vector2 position = vector + Vector2.Lerp(jet.lastJetPos[num], jet.jetPos[num], timeStacker) + vector2 * (Mathf.Lerp(jet.lastJetScale[num], jet.jetScale[num], timeStacker) * 0.1f);
		Color color = new Color(jet.data.midColor.r, jet.data.midColor.g, jet.data.midColor.b, Mathf.Min(jet.finalTemperature[num], Mathf.Max(0f, jet.finalTemperature[num] - 0.3f) * 5f)); // Orange
		color *= Mathf.Lerp(jet.lastRippleMult, jet.rippleMult, timeStacker);
		float num2 = jet.target.magnitude * 0.12f * (0.5f + jet.finalIntensity[num] * 0.5f);
		jet.light0.SetPosition(position);
		jet.light0.Color = color;
		jet.light0.scale = num2;
		jet.light1.SetPosition(position);
		jet.light1.Color = color;
		jet.light1.scale = num2;
		jet.light2.SetPosition(position);
		jet.light2.Color = new Color(jet.data.lightColor.r, jet.data.lightColor.g, jet.data.lightColor.b, color.a);
		jet.light2.scale = num2 * 0.5f;
		sLeaser.sprites[0].SetPosition(vector + Vector2.Lerp(jet.lastJetPos[num], jet.jetPos[num], timeStacker));
		sLeaser.sprites[0].color = new Color(0f, 0f, 0f, color.a * 0.5f);
		sLeaser.sprites[0].scale = num2 * 0.5f;

		for (int i = 0; i < jet.nJetPos; i++) {
			int num3 = i * 3;
			Vector2 vector3 = Vector2.Lerp(jet.lastJetPerp[i] * jet.lastJetScale[i], jet.jetPerp[i] * jet.jetScale[i], timeStacker);
			Vector2 vector4 = Vector2.Lerp(jet.lastJetPos[i], jet.jetPos[i], timeStacker);
			jet.jetMesh.MoveVertex(num3, vector4 + vector3);
			jet.jetMesh.MoveVertex(num3 + 1, vector4);
			jet.jetMesh.MoveVertex(num3 + 2, vector4 - vector3);
			jet.glowMesh.MoveVertex(num3, vector4 + vector3 * 1.4f);
			jet.glowMesh.MoveVertex(num3 + 1, vector4);
			jet.glowMesh.MoveVertex(num3 + 2, vector4 - vector3 * 1.4f);
			Color color2 = new Color(jet.finalIntensity[i], jet.finalTemperature[i], jet.individualOffset, 1f);
			jet.jetMesh.SetVertexColor(num3, color2);
			jet.jetMesh.SetVertexColor(num3 + 1, color2);
			jet.jetMesh.SetVertexColor(num3 + 2, color2);
			jet.glowMesh.SetVertexColor(num3, color2);
			jet.glowMesh.SetVertexColor(num3 + 1, color2);
			jet.glowMesh.SetVertexColor(num3 + 2, color2);
		}

		if (jet.slatedForDeletetion || jet.room != rCam.room) {
			sLeaser.CleanSpritesAndRemove();
		}

		if (jet.debug && jet.debugInit) {
			for (int k = 0; k < jet.nJetPos; k++) {
				Vector2 position2 = Vector2.Lerp(jet.lastPos + jet.lastJetPos[k], jet.pos + jet.jetPos[k], timeStacker) - camPos;
				jet.jetPosDebug[k].sprite.SetPosition(position2);
				jet.jetNormalDebug[k].sprite.SetPosition(position2);
				jet.jetNormalDebug[k].sprite.rotation = Custom.VecToDeg(jet.jetPerp[k]);
				jet.jetNormalDebug[k].sprite.scaleY = jet.jetScale[k];
			}
		}
	}
}