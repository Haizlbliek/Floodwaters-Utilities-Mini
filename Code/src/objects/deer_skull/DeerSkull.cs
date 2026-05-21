namespace Floodwaters.Objects;

public class DeerSkull : UpdatableAndDeletable, IDrawable {
	public PlacedObject pObj;

	public DeerGraphics.Antlers antlers;

	public Vector2 antlerPos;

	public float antlerFlip;

	public Color blackColor;

	public Color boneColor;

	public Color paintColor;

	public DeerSkullData Data => this.pObj.data as DeerSkullData;

	public float Rad => this.Data.Rad;

	public int PoleSprite => 0;

	private int FirstAntlerSprite => 1;

	private int LastAntlerSprite => this.FirstAntlerSprite + this.antlers.SpritesClaimed - 1;

	private int FirstAntlerDetailSprite => this.LastAntlerSprite + 1;

	private int LastAntlerDetailSprite => this.FirstAntlerDetailSprite + this.antlers.SpritesClaimed - 1;

	private int SkullSprite(int part) {
		return this.LastAntlerDetailSprite + 1 + part;
	}

	public DeerSkull(Room room, PlacedObject pObj) {
		this.pObj = pObj;
		this.room = room;
		this.antlerFlip = Mathf.Lerp(-0.8f, 0.8f, this.Data.handlePos.normalized.x);

		Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.Data.skullSeed);
		this.antlers = new DeerGraphics.Antlers(70f, 0.7f);
		this.antlerPos = this.pObj.pos + this.Data.handlePos + this.Data.handlePos.normalized * this.antlers.rad;
		UnityEngine.Random.state = state;

		if (!Futile.atlasManager.DoesContainAtlas("outpostSkulls")) {
			Futile.atlasManager.LoadAtlas("Atlases/outPostSkulls");
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1 + this.antlers.SpritesClaimed * 2 + 3];
		sLeaser.sprites[this.PoleSprite] = new FSprite("Futile_White", true) {
			scaleX = 0.5f,
			scaleY = this.Data.handlePos.magnitude * 2f / 16f,
			shader = rCam.game.rainWorld.Shaders["JaggedSquare"],
			alpha = 0f,
			rotation = Custom.VecToDeg(this.Data.handlePos.normalized)
		};

		int num = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(this.antlerFlip) * 4f) + 1, 1, 4);
		for (int i = 0; i < 3; i++) {
			sLeaser.sprites[this.SkullSprite(i)] = new FSprite("skull" + num.ToString() + "_" + (((this.Data.hasPaint && i == 2) ? 2 : 1) + i).ToString(), true) {
				anchorY = 0.85f,
				rotation = Custom.VecToDeg(this.Data.handlePos.normalized) + num * 2.5f * Mathf.Sign(this.antlerFlip),
				scaleX = -Mathf.Sign(this.antlerFlip)
			};
		}

		this.antlers.InitiateSprites(this.FirstAntlerSprite, sLeaser, rCam);
		this.antlers.InitiateSprites(this.FirstAntlerDetailSprite, sLeaser, rCam);
		for (int j = this.FirstAntlerDetailSprite; j <= this.LastAntlerDetailSprite; j++) {
			sLeaser.sprites[j].shader = rCam.game.rainWorld.Shaders["OutPostAntler"];
		}
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		sLeaser.sprites[this.PoleSprite].x = this.pObj.pos.x - camPos.x;
		sLeaser.sprites[this.PoleSprite].y = this.pObj.pos.y - camPos.y;
		Vector2 vector = this.pObj.pos + this.Data.handlePos;
		for (int i = 0; i < 3; i++) {
			sLeaser.sprites[this.SkullSprite(i)].x = vector.x - camPos.x;
			sLeaser.sprites[this.SkullSprite(i)].y = vector.y - camPos.y;
		}
		this.antlers.DrawSprites(this.FirstAntlerSprite, sLeaser, rCam, timeStacker, camPos, this.pObj.pos, this.antlerPos, this.antlerFlip, this.boneColor, this.boneColor);
		this.antlers.DrawSprites(this.FirstAntlerDetailSprite, sLeaser, rCam, timeStacker, camPos, this.pObj.pos, this.antlerPos, this.antlerFlip, this.paintColor, this.paintColor);

		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		Color paintFullColor = Custom.HSL2RGB(this.Data.paintHue, 1f, 0.5f);

		this.blackColor = palette.blackColor;
		this.boneColor = Color.Lerp(palette.blackColor, new Color(0.9f, 0.9f, 0.8f), Mathf.Lerp(0.9f, 0.2f, rCam.room.Darkness(this.pObj.pos)));
		this.paintColor = Color.Lerp(palette.blackColor, Color.Lerp(this.boneColor, paintFullColor, 0.9f), Mathf.Lerp(0.5f, 0.15f, rCam.room.Darkness(this.pObj.pos)));

		if (!this.Data.hasPaint)
			this.paintColor = this.boneColor;

		sLeaser.sprites[0].color = palette.blackColor;
		sLeaser.sprites[this.SkullSprite(0)].color = Color.Lerp(Color.Lerp(this.boneColor, new Color(0.6f, 0.5f, 0.1f), 0.3f), this.blackColor, Mathf.Lerp(0.6f, 1f, rCam.room.Darkness(this.pObj.pos)));
		sLeaser.sprites[this.SkullSprite(1)].color = this.boneColor;
		sLeaser.sprites[this.SkullSprite(2)].color = this.paintColor;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Midground");
		FSprite[] sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++) {
			sprites[i].RemoveFromContainer();
		}
		for (int j = this.FirstAntlerSprite; j < this.FirstAntlerSprite + this.antlers.parts.Length; j++) {
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		for (int k = this.FirstAntlerDetailSprite; k < this.FirstAntlerDetailSprite + this.antlers.parts.Length; k++) {
			newContatiner.AddChild(sLeaser.sprites[k]);
		}
		newContatiner.AddChild(sLeaser.sprites[this.PoleSprite]);
		for (int l = this.FirstAntlerSprite + this.antlers.parts.Length; l <= this.LastAntlerSprite; l++) {
			newContatiner.AddChild(sLeaser.sprites[l]);
		}
		for (int m = this.FirstAntlerDetailSprite + this.antlers.parts.Length; m <= this.LastAntlerDetailSprite; m++) {
			newContatiner.AddChild(sLeaser.sprites[m]);
		}
		for (int n = 0; n < 3; n++) {
			newContatiner.AddChild(sLeaser.sprites[this.SkullSprite(n)]);
		}
	}
}