namespace Floodwaters.Objects;

public class CustomLightRod : UpdatableAndDeletable, IDrawable {
	public PlacedObject pObj;
	public readonly List<LightVessel> lights = [];
	public Vector2 lastHandlePos;

	public CustomLightRodData Data => this.pObj.data as CustomLightRodData;

	public CustomLightRod(PlacedObject pObj, Room room) {
		this.room = room;
		this.pObj = pObj;
		this.UpdateLightAmount();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("pixel", true) {
			anchorX = 0f,
			scaleY = 4f,
			shader = rCam.game.rainWorld.Shaders["CustomDepth"]
		};
		sLeaser.sprites[1] = new FSprite("Futile_White", true) {
			anchorY = 0f,
			scaleX = 0.25f,
			shader = rCam.game.rainWorld.Shaders["FWCustomDepthGradient"]
		};
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("GrabShaders");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			FSprite fsprite = sLeaser.sprites[i];
			fsprite.RemoveFromContainer();
			newContatiner.AddChildAtIndex(fsprite, i);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		sLeaser.sprites[0].x = this.pObj.pos.x - camPos.x;
		sLeaser.sprites[0].y = this.pObj.pos.y - camPos.y;
		sLeaser.sprites[0].alpha = 1f - this.Data.depth;
		sLeaser.sprites[0].scaleX = this.Data.Rad;
		sLeaser.sprites[0].rotation = this.Data.handlePos.GetAngle();

		sLeaser.sprites[1].x = this.pObj.pos.x - camPos.x;
		sLeaser.sprites[1].y = this.pObj.pos.y - camPos.y;
		sLeaser.sprites[1].alpha = 1f - this.Data.depth;
		sLeaser.sprites[1].scaleY = this.Data.Rad / 16f;
		sLeaser.sprites[1].rotation = this.Data.handlePos.GetAngle() + 90f;

		float t = Mathf.Lerp(rCam.currentPalette.fogAmount / 4f, rCam.currentPalette.fogAmount, this.Data.depth / 1.1f);
		sLeaser.sprites[0].color = Color.Lerp(this.Data.color1, Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, t), (1f - rCam.room.ElectricPower) * 0.9f);
		sLeaser.sprites[1].color = Color.Lerp(this.Data.color2, Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, t), (1f - rCam.room.ElectricPower) * 0.9f);

		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
			foreach (LightVessel light in this.lights) {
				light.light.RemoveFromRoom();
			}
		}
	}

	public override void Update(bool eu) {
		base.Update(eu);

		foreach (LightVessel light in this.lights) {
			light.Update();
			light.light.color = Color.Lerp(this.Data.color1, this.Data.color2, light.progression);
		}

		if (this.Data.handlePos != this.lastHandlePos) {
			this.UpdateLightAmount();
		}
	}

	private void UpdateLightAmount() {
		int num = Custom.IntClamp((int) (this.Data.Rad / 45f), 2, 30);
		if (num == this.lights.Count) {
			return;
		}
		for (int i = 0; i < this.lights.Count; i++) {
			this.lights[i].light.Destroy();
		}
		this.lights.Clear();
		for (int j = 0; j < num; j++) {
			this.lights.Add(new LightVessel(this));
		}
		this.lastHandlePos = this.Data.handlePos;
	}

	public class LightVessel {
		public readonly CustomLightRod rod;

		public LightSource light;
		public float progression;
		public float size;
		public float speed;
		public float strength;
		public float visible;

		public PlacedObject PObj => this.rod.pObj;

		public CustomLightRodData Data => this.PObj.data as CustomLightRodData;

		public LightVessel(CustomLightRod rod) {
			this.rod = rod;
			this.Reset();
			this.progression = UnityEngine.Random.value;
			this.light = new LightSource(this.PObj.pos, false, Color.Lerp(this.rod.Data.color1, this.rod.Data.color2, this.progression), rod);
			this.rod.room.AddObject(this.light);
		}

		private void Reset() {
			this.progression = 0f;
			this.speed = Mathf.Lerp(0.5f, 2f, UnityEngine.Random.value);
			this.size = UnityEngine.Random.value;
		}

		public void Update() {
			this.strength = Mathf.InverseLerp(0.1f, 1f, Mathf.Pow(Mathf.Sin(this.progression * 3.1415927f), 0.5f));
			Vector2 vector = this.PObj.pos + this.Data.handlePos * this.progression;
			float num = 0.7f;
			if (this.rod.room.ViewedByAnyCamera(vector, 100f)) {
				num = 0f;
				for (int i = -3; i < 4; i++) {
					Vector2 coord = vector + this.Data.handlePos.normalized * (2f * i);
					if (this.rod.room.game.cameras[0].DepthAtCoordinate(coord) >= this.Data.depth) {
						num += 1f;
					}
				}
				num /= 7f;
			}
			this.visible = Mathf.Lerp(this.visible, num, Mathf.Lerp(0.2f, 0.05f, this.Data.brightness));
			this.strength *= this.visible;
			this.light.setAlpha = Mathf.Lerp(1f, 0.5f, this.size) * this.strength * this.rod.room.ElectricPower;
			this.light.setPos = vector;
			this.light.setRad = Mathf.Lerp(100f, 400f, this.size) * this.visible * Mathf.Lerp(0.2f, 1.5f, this.Data.brightness);
			this.progression = Mathf.Min(1f, this.progression + this.speed / this.Data.Rad);
			if (this.progression >= 1f) {
				this.Reset();
			}
		}
	}
}