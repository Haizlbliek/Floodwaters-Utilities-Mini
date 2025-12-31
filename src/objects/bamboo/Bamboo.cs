namespace Floodwaters.Objects;

public class Bamboo : UpdatableAndDeletable, IClimbableVine, IDrawable {
	private readonly PlacedObject pObj;

	public PlacedObject.ResizableObjectData Data => this.pObj.data as PlacedObject.ResizableObjectData;

	public Segment[] stalk;
	public float bend = 0f;
	public float bendVelocity = 0f;
	public int climbing = 0;

	public Bamboo(Room room, PlacedObject pObj) {
		this.pObj = pObj;
		this.room = room;

		if (this.room.climbableVines == null) {
			this.room.AddObject(this.room.climbableVines = new ClimbableVinesSystem());
		}
		this.room.climbableVines.vines.Add(this);

		this.SetStalk();
	}

	public void SetStalk() {
		this.stalk = new Segment[Mathf.RoundToInt(this.Data.handlePos.magnitude / 20f)];
		Vector2 pos = this.pObj.pos;
		for (int i = 0; i < this.stalk.Length; i++) {
			this.stalk[i] = new Segment(pos, 0f);
			pos = this.stalk[i].End;
		}
	}

	public override void Update(bool eu) {
		base.Update(eu);

		this.bend = Mathf.Lerp(this.bend, 0.0f, Mathf.Clamp(Mathf.Abs(this.bend) / -0.5f, 0.1f, 0.95f));
		this.climbing--;
		if (this.climbing <= 0) {
			this.bendVelocity += -this.bend * 0.3f;
			this.bendVelocity *= 0.9f;
			this.bend += this.bendVelocity;
		}

		for (int i = 0; i < this.stalk.Length; i++) {
			Segment segment = this.stalk[i];
			segment.lastPos = segment.pos;
			segment.lastRot = segment.rot;

			if (i == 0) {
				segment.pos = this.pObj.pos;
				segment.rot = this.bend * i / this.stalk.Length;
			} else {
				segment.pos = this.stalk[i - 1].End;
				segment.rot = this.bend * i / this.stalk.Length;
			}
		}
	}

	public void BeingClimbedOn(Creature crit) {
		this.bend += (crit.mainBodyChunk.pos.x - this.pObj.pos.x) * 0.25f * Mathf.Clamp01((crit.mainBodyChunk.pos.y - this.pObj.pos.y) / 800f);
		this.bend *= 0.95f;
		this.bendVelocity = 0f;
		this.climbing = 2;
	}

	public bool CurrentlyClimbable() {
		return true;
	}

	public float Mass(int index) {
		return 1000f;
	}

	public Vector2 Pos(int index) {
		if (index == this.stalk.Length)
			return this.stalk[this.stalk.Length - 1].End;

		return this.stalk[index].pos;
	}

	public void Push(int index, Vector2 movement) {
	}

	public float Rad(int index) {
		return 2f;
	}

	public int TotalPositions() {
		return this.stalk.Length + 1;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(this.stalk.Length + 1, false, true);

		// for (int i = 0; i < sLeaser.sprites.Length; i++) {
		// 	sLeaser.sprites[i] = new FSprite("Futile_White", true);
		// }

		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Midground");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		TriangleMesh mesh = sLeaser.sprites[0] as TriangleMesh;
		Color dark = palette.texture.GetPixel(0, 4);
		Color light = palette.texture.GetPixel(5, 2);

		Random.State state = Random.state;
		Random.InitState((int) this.pObj.pos.x);
		for (int i = 0; i < mesh.verticeColors.Length; i++) {
			// mesh.verticeColors[i] = new Color(
			// 	(i % 2 >= 1) ? 1f : 0f,
			// 	(i % 4 >= 2) ? 1f : 0f,
			// 	(i % 8 >= 4) ? 1f : 0f
			// );
			float pos = (float) i / mesh.verticeColors.Length;
			Color color = Color.Lerp(dark, light, Mathf.Sqrt(pos));
			Vector3 hsl = Custom.RGB2HSL(color);
			mesh.verticeColors[i] = Custom.HSL2RGB(hsl.x + (Random.value - 0.5f) * 0.1f, hsl.y + (Random.value - 0.5f) * 0.2f, hsl.z);
			// mesh.verticeColors[i] = Custom.HSL2RGB(Mathf.Lerp(0.18f, 0.25f, Random.value), Mathf.Lerp(0.195f, 0.425f, Random.value), Mathf.Lerp(0.2f, 0.457f, Mathf.Pow(pos, 0.6f)));
		}
		Random.state = state;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		TriangleMesh mesh = sLeaser.sprites[0] as TriangleMesh;

		Random.State state = Random.state;
		Random.InitState((int) this.pObj.pos.x);
		for (int i = 0; i < mesh.vertices.Length; i += 2) {
			int stalkIndex = i / 4;
			bool middlePos = i % 4 >= 2;
			bool endPos = false;
			if (stalkIndex >= this.stalk.Length) {
				stalkIndex = this.stalk.Length - 1;
				endPos = true;
			}
			Segment segment = this.stalk[stalkIndex];
			float pos = (float) i / mesh.vertices.Length;
			float width = Mathf.Lerp(20f, Mathf.Lerp(4f, 1f, pos), Mathf.Pow(i / 15f, 0.33f));
			float xOff = (Random.value * 2f - 1f) * Mathf.Lerp(0f, Mathf.Lerp(4f, 2f, pos), i / 10f);
			mesh.vertices[i] = (middlePos ? segment.Center : endPos ? segment.End : segment.pos) - camPos - segment.Side * width + segment.Side * xOff;
			mesh.vertices[i + 1] = (middlePos ? segment.Center : endPos ? segment.End : segment.pos) - camPos + segment.Side * width + segment.Side * xOff;
		}
		mesh._isMeshDirty = true;
		Random.state = state;
	}

	public class Segment {
		public Vector2 lastPos;
		public Vector2 pos;
		public float lastRot;
		public float rot;

		public Vector2 vel = Vector2.zero;

		public Vector2 LastEnd => this.lastPos + Custom.DegToVec(this.lastRot) * 20f;
		public Vector2 End => this.pos + Custom.DegToVec(this.rot) * 20f;
		public Vector2 Center => this.pos + Custom.DegToVec(this.rot) * 10f;

		public Vector2 Side {
			get {
				return Custom.DegToVec(this.rot + 90f);
			}
		}

		public Segment(Vector2 pos, float rot) {
			this.lastPos = pos;
			this.pos = pos;
			this.lastRot = rot;
			this.rot = rot;
		}

		public Vector2 Middle(float timeStacker) {
			return Vector2.Lerp(this.lastPos, this.pos, timeStacker) + Custom.DegToVec(Mathf.Lerp(this.lastRot, this.rot, timeStacker)) * 10f;
		}
	}

	public class BambooRepresentation : ResizeableObjectRepresentation {
		public BambooRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, false) {
		}
	}
}