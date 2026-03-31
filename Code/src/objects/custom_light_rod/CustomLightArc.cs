namespace Floodwaters.Objects;

public class CustomLightArc : UpdatableAndDeletable, IDrawable {
	public PlacedObject pObj;
	public readonly List<LightVessel> lights = [];
	public Vector2 lastHandlePos;
	public Vector2 lastHandleAPos;
	public Vector2 lastHandleBPos;

	public float length;
	public bool nodeCountChanged;
	public bool nodesUpdated;
	public int nodeCount;
	public Vector2[] nodes;

	public CustomLightArcData Data => this.pObj.data as CustomLightArcData;

	public CustomLightArc(PlacedObject pObj, Room room) {
		this.room = room;
		this.pObj = pObj;
		this.UpdateNodes();
		this.UpdateLightAmount();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMeshAtlased(this.nodeCount, false, true);
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
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
		float t = Mathf.Lerp(rCam.currentPalette.fogAmount / 4f, rCam.currentPalette.fogAmount, this.Data.depth / 1.1f);
		Color colorA = Color.Lerp(this.Data.color1, Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, t), (1f - rCam.room.ElectricPower) * 0.9f);;
		Color colorB = Color.Lerp(this.Data.color2, Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, t), (1f - rCam.room.ElectricPower) * 0.9f);;

		TriangleMesh mesh = sLeaser.sprites[0] as TriangleMesh;
		for (int i = 0; i < this.nodeCount; i++) {
			float progress = i / (float)(this.nodeCount - 1);
			mesh.verticeColors[i * 4 + 0] = Color.Lerp(colorA, colorB, progress);
			mesh.verticeColors[i * 4 + 1] = Color.Lerp(colorA, colorB, progress);
			mesh.verticeColors[i * 4 + 2] = Color.Lerp(colorA, colorB, progress + (0.5f / (this.nodeCount - 1)));
			mesh.verticeColors[i * 4 + 3] = Color.Lerp(colorA, colorB, progress + (0.5f / (this.nodeCount - 1)));
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (this.nodeCountChanged) {
			foreach (FSprite sprite in sLeaser.sprites) {
				sprite.RemoveFromContainer();
			}
			this.InitiateSprites(sLeaser, rCam);
			this.nodeCountChanged = false;
		}

		TriangleMesh mesh = sLeaser.sprites[0] as TriangleMesh;
		if (this.nodesUpdated) {
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);

			Vector2 a = this.nodes[0];
			float d = 2f;
			for (int i = 0; i < this.nodeCount; i++) {
				Vector2 vector = this.nodes[i];
				Vector2 a2 = Custom.PerpendicularVector((a - vector).normalized);
				mesh.MoveVertice(i * 4 + 0, a - a2 * d);
				mesh.MoveVertice(i * 4 + 1, a + a2 * d);
				mesh.MoveVertice(i * 4 + 2, vector - a2 * d);
				mesh.MoveVertice(i * 4 + 3, vector + a2 * d);
				a = vector;
			}
			this.nodesUpdated = false;
		}

		mesh.alpha = 1f - this.Data.depth;
		mesh.SetPosition(this.pObj.pos - camPos);

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

		if (this.Data.handlePos != this.lastHandlePos || this.Data.handleA != this.lastHandleAPos || this.Data.handleB != this.lastHandleBPos) {
			this.UpdateNodes();
			this.UpdateLightAmount();
		}
	}

	private void UpdateNodes() {
		this.length = this.Data.handleA.magnitude;
		this.length += (this.Data.handleB + this.Data.handlePos - this.Data.handleA).magnitude;
		this.length += this.Data.handleB.magnitude;
		int newNodeCount = 2 + Mathf.CeilToInt(this.length / 10f);
		if (Mathf.Abs((newNodeCount - this.nodeCount) / (float) this.nodeCount) > 0.2f) {
			this.nodeCountChanged = true;
			this.nodeCount = newNodeCount;
			this.nodes = new Vector2[this.nodeCount];
		}
		Vector2 a2 = this.Data.handleA;
		Vector2 a3 = this.Data.handlePos + this.Data.handleB;
		Vector2 a4 = this.Data.handlePos;

		for (int i = 0; i < this.nodeCount; i++) {
			float num4 = (float) (1f / (this.nodeCount - 1)) * i;
			float num5 = 1f - num4;
			Vector2 vector = 3f * num5 * num5 * num4 * a2 + 3f * num5 * num4 * num4 * a3 + num4 * num4 * num4 * a4;
			this.nodes[i] = vector;
		}

		this.nodesUpdated = true;
	}

	public Vector2 PointOnArc(float t) {
		float inverseT = 1f - t;
		return inverseT * inverseT * inverseT * this.pObj.pos + 3f * inverseT * inverseT * t * (this.pObj.pos + this.Data.handleA) + 3f * inverseT * t * t * (this.pObj.pos + this.Data.handlePos + this.Data.handleB) + t * t * t * (this.pObj.pos + this.Data.handlePos);
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
		this.lastHandleAPos = this.Data.handleA;
		this.lastHandleBPos = this.Data.handleB;
	}

	public class LightVessel {
		public readonly CustomLightArc arc;

		public LightSource light;
		public float progression;
		public float size;
		public float speed;
		public float strength;
		public float visible;

		public PlacedObject PObj => this.arc.pObj;

		public CustomLightArcData Data => this.PObj.data as CustomLightArcData;

		public LightVessel(CustomLightArc arc) {
			this.arc = arc;
			this.Reset();
			this.progression = UnityEngine.Random.value;
			this.light = new LightSource(this.PObj.pos, false, Color.Lerp(this.arc.Data.color1, this.arc.Data.color2, this.progression), arc);
			this.arc.room.AddObject(this.light);
		}

		private void Reset() {
			this.progression = 0f;
			this.speed = Mathf.Lerp(0.5f, 2f, UnityEngine.Random.value);
			this.size = UnityEngine.Random.value;
		}

		public void Update() {
			this.strength = Mathf.InverseLerp(0.1f, 1f, Mathf.Pow(Mathf.Sin(this.progression * 3.1415927f), 0.5f));
			Vector2 pos = this.arc.PointOnArc(this.progression);
			float num = 0.7f;
			if (this.arc.room.ViewedByAnyCamera(pos, 100f)) {
				num = 0f;
				for (int i = -3; i < 4; i++) {
					Vector2 coord = pos + this.Data.handlePos.normalized * (2f * i);
					if (this.arc.room.game.cameras[0].DepthAtCoordinate(coord) >= this.Data.depth) {
						num += 1f;
					}
				}
				num /= 7f;
			}
			this.visible = Mathf.Lerp(this.visible, num, Mathf.Lerp(0.2f, 0.05f, this.Data.brightness));
			this.strength *= this.visible;
			this.light.setAlpha = Mathf.Lerp(1f, 0.5f, this.size) * this.strength * this.arc.room.ElectricPower;
			this.light.setPos = pos;
			this.light.setRad = Mathf.Lerp(100f, 400f, this.size) * this.visible * Mathf.Lerp(0.2f, 1.5f, this.Data.brightness);
			this.progression = Mathf.Min(1f, this.progression + this.speed / this.arc.length);
			if (this.progression >= 1f) {
				this.Reset();
			}
		}
	}

	public class CustomLightArcData : CustomLightRod.CustomLightRodData {
		public Vector2 handleA = new Vector2(50f, 0f);
		public Vector2 handleB = new Vector2(-50f, 0f);

		public CustomLightArcData(PlacedObject owner) : base(owner) {
			this.handlePos = new Vector2(100f, 100f);
		}

		public override void FromString(string s) {
			base.FromString(s);
			try {
				string[] array = Regex.Split(s, "~");
				this.handleA.x = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.handleA.y = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.handleB.x = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.handleB.y = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 16);
			} catch (Exception) {}
		}

		public override string ToString() {
			return base.ToString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}",
				this.handleA.x,
				this.handleA.y,
				this.handleB.x,
				this.handleB.y
			);
		}
	}

	public class CustomLightArcRepresentation : CustomLightRod.CustomLightRodRepresentation {
		public new CustomLightArcData Data => this.pObj.data as CustomLightArcData;

		public Handle handleA;
		public Handle handleB;
		public int handleALine;
		public int handleBLine;

		public CustomLightArcRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, bool createCustomArc = true) : base(owner, IDstring, parentNode, pObj, false) {
			this.fLabels[0].text = "CustomLightArc";
			(this.subNodes[this.subNodes.Count - 1] as Panel).Title = "Custom Light Arc";

			CustomLightArc customArc = this.owner.room.updateList.OfType<CustomLightArc>().FirstOrDefault(x => x.pObj == pObj);

			if (customArc == null && createCustomArc) {
				customArc = new CustomLightArc(pObj, owner.room);
				owner.room.AddObject(customArc);
			}

			this.subNodes.Add(this.handleA = new Handle(owner, "HandleA", this, this.Data.handleA));
			this.subNodes.Add(this.handleB = new Handle(owner, "HandleB", this.subNodes[0], this.Data.handleB));

			this.handleALine = this.fSprites.Count;
			this.fSprites.Add(new FSprite("pixel", true) { anchorY = 0f });
			owner.placedObjectsContainer.AddChild(this.fSprites[this.handleALine]);

			this.handleBLine = this.fSprites.Count;
			this.fSprites.Add(new FSprite("pixel", true) { anchorY = 0f });
			owner.placedObjectsContainer.AddChild(this.fSprites[this.handleBLine]);
		}

		public override void Refresh() {
			base.Refresh();

			this.Data.handleA = this.handleA.pos;
			this.Data.handleB = this.handleB.pos;

			this.MoveSprite(this.handleALine, this.absPos);
			this.fSprites[this.handleALine].scaleY = this.handleA.pos.magnitude;
			this.fSprites[this.handleALine].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.handleA.absPos);

			this.MoveSprite(this.handleBLine, (this.subNodes[0] as Handle).absPos);
			this.fSprites[this.handleBLine].scaleY = this.handleB.pos.magnitude;
			this.fSprites[this.handleBLine].rotation = Custom.AimFromOneVectorToAnother((this.subNodes[0] as Handle).absPos, this.handleB.absPos);
		}
	}
}