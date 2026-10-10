using System.Collections.Concurrent;

namespace Floodwaters.Objects;

public class Lanternfruit : PlayerCarryableItem, IDrawable, IPlayerEdible, IProvideWarmth {
	public PlacedObject pObj;
	public Vector2 rotation;
	public Vector2 lastRotation;

	public float darkness;
	public float lastDarkness;

	//Light settings
	public LightSource lightSource;
	public float[,] flicker;
	private const float LightRadius = 100f;
	private const float LightAlpha = 0.3f;
	private const float DeadLightRadius = 50f;
	private const float DeadLightAlpha = 0.15f;

	//Color classes
	public LanternfruitData.ColorType generatedColorType;
	public Color generatedColor;


	//warmth settings
	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth;
	Room IProvideWarmth.loadedRoom => this.room;
	float IProvideWarmth.range => 75;
	Vector2 IProvideWarmth.Position() => base.firstChunk.pos;


	//Stuck settings for generation on Dodder
	private bool stuck;
	private Vector2 stuckPos;
	private Vector2 stuckRot;

	//fruit configs
	public bool Edible => true;
	public int BitesLeft => this.bites;
	public int bites = 2;
	public int FoodPoints => 1;
	
	public bool AutomaticPickUp => true;

	//color configuration
	public Color Color => this.pObj?.data is LanternfruitData data ? data.color : this.generatedColor;
	private LanternfruitData.ColorType ColorType => this.pObj?.data is LanternfruitData data ? data.colorType : this.generatedColorType;
	private Color LightColor => this.ColorType == LanternfruitData.ColorType.Custom || this.ColorType == LanternfruitData.ColorType.Dead ? this.Color : this.color;


	public Lanternfruit(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject) {
		AbstractLanternfruit abstractFruit = abstractPhysicalObject as AbstractLanternfruit;
		this.pObj = abstractFruit?.pObj;
		this.generatedColorType = abstractFruit?.colorType ?? LanternfruitData.ColorType.EffectColorA;
		this.generatedColor = abstractFruit?.color ?? Color.white;
		base.bodyChunks = [
			new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.15f)
		];
		this.bodyChunkConnections = [];

		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		this.bounce = 0.3f;
		this.surfaceFriction = 0.8f;
		this.collisionLayer = 1;
		base.waterFriction = 0.8f;
		base.buoyancy = 0.8f;

		this.flicker = new float[2, 3];
		for (int i = 0; i < this.flicker.GetLength(0); i++) {
			this.flicker[i, 0] = 0.3f;
			this.flicker[i, 1] = 1f;
			this.flicker[i, 2] = 0.2f;
		}
	}


	
	public override void Update(bool eu) {
		base.Update(eu);
		if (this.pObj != null && this.grabbedBy.Count > 0 && this.abstractPhysicalObject is AbstractLanternfruit abstractFruit && !abstractFruit.isConsumed) {
			abstractFruit.Consume();
		}

		//flicker
		for (int i = 0; i < this.flicker.GetLength(0); i++) {
			this.flicker[i, 1] = this.flicker[i, 0];
			this.flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
			this.flicker[i, 0] = Custom.LerpAndTick(this.flicker[i, 0], this.flicker[i, 2], 0.05f, 0.033333335f);
			if (UnityEngine.Random.value < 0.2f) {
				this.flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
			}
			this.flicker[i, 2] = Mathf.Lerp(this.flicker[i, 2], 1f, 0.01f);
			}
		//flicker
	
		//Lightsource addition
		bool dead = this.ColorType == LanternfruitData.ColorType.Dead;
		bool custom = this.ColorType == LanternfruitData.ColorType.Custom;
		if (this.lightSource == null) {
			this.lightSource = new LightSource(base.firstChunk.pos, false, this.LightColor, this) {
				affectedByPaletteDarkness = 0.5f,
				setRad = (dead ? DeadLightRadius : LightRadius) * this.flicker[0, 0],
				setAlpha = dead ? DeadLightAlpha : LightAlpha
			};
			this.room.AddObject(this.lightSource);
		}
		else if (this.lightSource != null) {
			this.lightSource.setPos = new Vector2?(base.firstChunk.pos);
			this.lightSource.setRad = new float?((dead ? DeadLightRadius : LightRadius) * this.flicker[0, 0]);
			this.lightSource.setAlpha = new float?(dead ? DeadLightAlpha : LightAlpha);
			this.lightSource.affectedByPaletteDarkness = custom ? 0f : 0.5f;
			this.lightSource.color = this.LightColor;
			if (this.lightSource.slatedForDeletetion || this.lightSource.room != this.room) {
				this.lightSource = null;
			}
		}
		//Lightsource addition


		if (this.room.game.devToolsActive && Input.GetKey("b")) {
			this.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		

		this.lastRotation = this.rotation;
		if (this.grabbedBy.Count > 0) {
			this.rotation = Custom.PerpendicularVector(Custom.DirVec(this.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
			this.rotation.y = Mathf.Abs(this.rotation.y);
		}
		if (this.firstChunk.ContactPoint.y < 0) {
			this.rotation = (this.rotation - Custom.PerpendicularVector(this.rotation) * 0.1f * this.firstChunk.vel.x).normalized;
			this.firstChunk.vel.x *= 0.8f;
		}

		if (this.stuck) {
			if (this.grabbedBy.Count > 0) {
				this.stuck = false;
			}
			else {
				this.firstChunk.vel = Vector2.zero;
				this.firstChunk.pos = this.stuckPos;
				this.rotation = this.stuckRot;
				this.lastRotation = this.stuckRot;
			}
		}
		this.firstChunk.collideWithObjects = !this.stuck;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) {
		base.TerrainImpact(chunk, direction, speed, firstContact);

		if (speed > 5f && firstContact) {
			Vector2 pos = base.bodyChunks[chunk].pos + direction.ToVector2() * base.bodyChunks[chunk].rad * 0.9f;

			for (int i = 0; i < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)); i++) {
				this.room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * UnityEngine.Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), Color.Lerp(this.color, new Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
			}
		}
	}

	public override void PlaceInRoom(Room placeRoom) {
		base.PlaceInRoom(placeRoom);

		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
		this.rotation = Custom.RNV();
		this.lastRotation = this.rotation;
	}

	public void Stuck(Vector2 stuckPos, Vector2 stuckRot) {
		this.stuck = true;
	}

	public void UpdateStuckPosition(Vector2 stuckPos, Vector2 stuckRot) {
		if (!this.stuck)
			return;

		this.stuckPos = stuckPos;
		this.stuckRot = stuckRot;
		this.firstChunk.lastPos = stuckPos;
		this.firstChunk.pos = stuckPos;
		this.rotation = stuckRot;
		this.lastRotation = stuckRot;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Items");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
		}

		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[1]);
	}
	


	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		LanternfruitData data = this.pObj?.data as LanternfruitData;
		Color baseColor = this.Color;
		LanternfruitData.ColorType colorType = data?.colorType ?? this.generatedColorType;
		if (colorType == LanternfruitData.ColorType.Dead) {
			int x = Random.Range(0, 10);
			baseColor = palette.texture.GetPixel(x, 3);
			
		}
		else if (colorType != LanternfruitData.ColorType.Custom) {
			int effectColor = colorType == LanternfruitData.ColorType.EffectColorB ? 1 : 0;
			baseColor = Color.Lerp(
				palette.texture.GetPixel(30, 4 - 2 * effectColor),
				palette.texture.GetPixel(30, 5 - 2 * effectColor),
				0.5f
			);
		}

		this.color = colorType == LanternfruitData.ColorType.Custom
			? baseColor
			: Color.Lerp(baseColor, palette.blackColor, this.darkness);
		sLeaser.sprites[0].color = this.color;
		sLeaser.sprites[1].color = Color.Lerp(this.color, Color.black, 0.5f);
	}



	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A", true);
		sLeaser.sprites[1] = new FSprite("DangleFruit0B", true);
		for (int i = 0; i < 2; i++) {
			sLeaser.sprites[i].scaleX = 0.7f;
			sLeaser.sprites[i].scaleY = 0.6f;
		}
		this.AddToContainer(sLeaser, rCam, null);
	}


	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		Vector2 position = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 rotation = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
		this.lastDarkness = this.darkness;
		this.darkness = rCam.room.Darkness(position) * (1f - rCam.room.LightSourceExposure(position));
		this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);

		for (int i = 0; i < 2; i++) {
			sLeaser.sprites[i].x = position.x - camPos.x;
			sLeaser.sprites[i].y = position.y - camPos.y;
			sLeaser.sprites[i].rotation = Custom.VecToDeg(rotation);
			sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("DangleFruit" + (2 - this.bites) + (i == 0 ? "A" : "B"));
		}

		if (this.blink > 0 && UnityEngine.Random.value < 0.5f) {
			sLeaser.sprites[0].color = base.blinkColor;
		}
		else {
			sLeaser.sprites[0].color = this.color;
		}
		if (this.blink > 0 && UnityEngine.Random.value < 0.5f) {
			sLeaser.sprites[1].color = base.blinkColor;
		}
		else {
			sLeaser.sprites[1].color = Color.Lerp(this.color, Color.black, 0.5f);
		}

		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
		}
	}


	public void BitByPlayer(Creature.Grasp grasp, bool eu) {
		this.bites--;
		this.room.PlaySound(this.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, base.firstChunk);

		if (this.bites == 0) {
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			this.Destroy();
		}
	}


	public void ThrowByPlayer() {
	}

	public class AbstractLanternfruit : AbstractConsumable {
		public PlacedObject pObj;
		public LanternfruitData.ColorType colorType;
		public Color color;

		public AbstractLanternfruit(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, PlacedObject pObj = null, int? originRoom = null, int placedObjectIndex = 0, LanternfruitData.ColorType colorType = null, Color? color = null) : base(world, Enums.Lanternfruit, realizedObject, pos, ID, originRoom ?? pos.room, placedObjectIndex, pObj?.data as PlacedObject.ConsumableObjectData) {
			this.pObj = pObj;
			LanternfruitData data = pObj?.data as LanternfruitData;
			this.colorType = colorType ?? data?.colorType ?? LanternfruitData.ColorType.EffectColorA;
			this.color = color ?? data?.color ?? Color.white;
		}

		public override string ToString() {
			string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}<oA>{6}",
				this.ID.ToString(),
				this.type.ToString(),
				this.pos.SaveToString(),
				this.originRoom,
				this.placedObjectIndex,
				this.colorType.ToString(),
				Custom.colorToHex(this.color)
			);
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", this.unrecognizedAttributes);
		}
	}
}