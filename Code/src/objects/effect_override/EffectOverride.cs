namespace Floodwaters.Objects;

// TODO - Upload proper colors
public class EffectOverride : UpdatableAndDeletable, IDrawable {
	public readonly PlacedObject pObj;
	public EffectOverrideData Data => this.pObj.data as EffectOverrideData;

	public PropertySquareMesh mesh;

	public Color[] overrideA = new Color[4];
	public Color[] overrideB = new Color[4];

	public bool needsRefresh = true;

	public EffectOverride(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = [];

		this.mesh = new PropertySquareMesh(this.pObj.type == Enums.EffectOverrideRectPO ? "FWEffectColor" : "FWEffectColorCircle", "Futile_White", mpb => {
			mpb.SetColor("_OverrideA0", this.overrideA[0]);
			mpb.SetColor("_OverrideA1", this.overrideA[1]);
			mpb.SetColor("_OverrideA2", this.overrideA[2]);
			mpb.SetColor("_OverrideA3", this.overrideA[3]);
			mpb.SetInt("_OverrideA", this.Data.colorA);

			mpb.SetColor("_OverrideB0", this.overrideB[0]);
			mpb.SetColor("_OverrideB1", this.overrideB[1]);
			mpb.SetColor("_OverrideB2", this.overrideB[2]);
			mpb.SetColor("_OverrideB3", this.overrideB[3]);
			mpb.SetInt("_OverrideB", this.Data.colorB);

			mpb.SetInt("_FromDepth", this.Data.fromDepth);
			mpb.SetInt("_ToDepth", this.Data.toDepth);

			mpb.SetFloat("_Gradient", this.Data.gradient);
		});

		if (this.pObj.type == Enums.EffectOverrideRectPO) {
			this.mesh.Vertices = [ new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) ];
		}

		sLeaser.containers = [ this.mesh ];

		this.AddToContainer(sLeaser, rCam, null);
	}

	public void Refresh() {
		try {
			if (this.Data.colorA != -1)
				this.overrideA = this.ModifyColors(RoomCamera.allEffectColorsTexture.GetPixels(this.Data.colorA * 2, 2, 2, 2, 0), this.Data.modifyA.x, this.Data.modifyA.y * 2f, this.Data.modifyA.z * 2f);

			if (this.Data.colorB != -1)
				this.overrideB = this.ModifyColors(RoomCamera.allEffectColorsTexture.GetPixels(this.Data.colorB * 2, 2, 2, 2, 0), this.Data.modifyB.x, this.Data.modifyB.y * 2f, this.Data.modifyB.z * 2f);

			this.needsRefresh = false;
		}
		catch (Exception) {}
	}

	public Color[] ModifyColors(Color[] colors, float h, float s, float l) {
		for (int i = 0; i < colors.Length; i++) {
			Vector3 vector;
			vector = Custom.RGB2HSL(colors[i]);
			vector.x = ((vector.x + h) % 1f + 1f) % 1f;
			vector.y = Mathf.Clamp01(vector.y * s);
			vector.z = Mathf.Clamp01(vector.z * l);
			colors[i] = Custom.HSL2RGB(vector.x, vector.y, vector.z);
		}
		return colors;
	}

	public override void Update(bool eu) {
		if (this.needsRefresh) {
			this.Refresh();
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Foreground");
		sLeaser.containers[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.containers[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		sLeaser.containers[0].SetPosition(this.pObj.pos - camPos);
		if (this.pObj.type == Enums.EffectOverrideRectPO) {
			sLeaser.containers[0].scaleX = this.Data.handlePos.x;
			sLeaser.containers[0].scaleY = this.Data.handlePos.y;
		}
		else {
			sLeaser.containers[0].scale = this.Data.handlePos.magnitude * 2f;
		}

		if (this.slatedForDeletetion || sLeaser.deleteMeNextFrame) {
			sLeaser.CleanSpritesAndRemove();
		}
	}
}