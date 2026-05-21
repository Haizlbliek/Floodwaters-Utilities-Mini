namespace Floodwaters.Objects;

public class Cattail : Weapon {
	public SporesSmoke smoke;
	public float beingEaten;
	public bool lastModeThrown;
	public float swallowed;
	public CattailStick stick;
	public Vector2[,] rag;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public AbstractCattail AbstrCattail => this.abstractPhysicalObject as AbstractCattail;

	private Color SporeColor => Color.Lerp(this.AbstrCattail.HasDye ? this.AbstrCattail.dyeColor : new Color(0.8f, 0.75f, 0.6f), this.color, Random.Range(0.0f, 0.25f));

	public Cattail(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject, abstractPhysicalObject.world) {
		this.bodyChunks = new BodyChunk[1];
		this.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 7f, 0.11f);
		this.bodyChunkConnections = [];

		this.airFriction = 0.98f;
		this.gravity = 0.86f;
		this.bounce = 0.2f;
		this.surfaceFriction = 0.3f;
		this.collisionLayer = 2;
		this.waterFriction = 0.98f;
		this.buoyancy = 1.8f;
		this.tailPos = this.firstChunk.pos;
		this.exitThrownModeSpeed = 15f;

		this.rag = new Vector2[UnityEngine.Random.Range(4, UnityEngine.Random.Range(4, 10)), 6];
	}

	private Vector2 RagAttachPos(float timeStacker) {
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
	}

	public void ResetRag() {
		Vector2 vector = this.RagAttachPos(1f);
		for (int i = 0; i < this.rag.GetLength(0); i++) {
			this.rag[i, 0] = vector;
			this.rag[i, 1] = vector;
			this.rag[i, 2] *= 0f;
		}
	}

	public override void PlaceInRoom(Room placeRoom) {
		while (this.abstractPhysicalObject.pos.y >= 0 && !placeRoom.GetTile(this.abstractPhysicalObject.pos.Tile + new IntVector2(0, -1)).Solid) {
			AbstractPhysicalObject abstractPhysicalObject = this.abstractPhysicalObject;
			abstractPhysicalObject.pos.y--;
		}
		base.PlaceInRoom(placeRoom);
		this.rotation = Custom.DegToVec(Mathf.Lerp(-45f, 45f, this.abstractPhysicalObject.world.game.SeededRandom(this.abstractPhysicalObject.ID.RandomSeed)));
		
		this.stick?.SetCattailPos();

		this.ResetRag();
	}

	public override void NewRoom(Room newRoom) {
		base.NewRoom(newRoom);

		this.ResetRag();
	}
	
	public void UpdateRag() {
		for (int i = 0; i < this.rag.GetLength(0); i++) {
			float t = (float) i / (float) (this.rag.GetLength(0) - 1);
			this.rag[i, 1] = this.rag[i, 0];
			this.rag[i, 0] += this.rag[i, 2];
			this.rag[i, 2] -= this.rotation * Mathf.InverseLerp(1f, 0f, (float) i) * 0.8f;
			this.rag[i, 4] = this.rag[i, 3];
			this.rag[i, 3] = (this.rag[i, 3] + this.rag[i, 5] * Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
			this.rag[i, 5] = (this.rag[i, 5] + Custom.RNV() * UnityEngine.Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(this.rag[i, 0], this.rag[i, 1])), 0.3f)).normalized;
			if (this.room.PointSubmerged(this.rag[i, 0])) {
				this.rag[i, 2] *= Custom.LerpMap(this.rag[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
				this.rag[i, 2].y += 0.05f;
				this.rag[i, 2] += Custom.RNV() * 0.1f;
			}
			else {
				this.rag[i, 2] *= Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
				this.rag[i, 2].y -= this.room.gravity * Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 6f, 0.6f, 0f);
				if (i % 3 == 2 || i == this.rag.GetLength(0) - 1) {
					SharedPhysics.TerrainCollisionData terrainCollisionData = this.scratchTerrainCollisionData.Set(this.rag[i, 0], this.rag[i, 1], this.rag[i, 2], 1f, new IntVector2(0, 0), false);
					terrainCollisionData = SharedPhysics.HorizontalCollision(this.room, terrainCollisionData);
					terrainCollisionData = SharedPhysics.VerticalCollision(this.room, terrainCollisionData);
					terrainCollisionData = SharedPhysics.SlopesVertically(this.room, terrainCollisionData);
					this.rag[i, 0] = terrainCollisionData.pos;
					this.rag[i, 2] = terrainCollisionData.vel;
					if (terrainCollisionData.contactPoint.x != 0) {
						this.rag[i, 2].y *= 0.6f;
					}
					if (terrainCollisionData.contactPoint.y != 0) {
						this.rag[i, 2].x *= 0.6f;
					}
				}
			}
		}

		for (int j = 0; j < this.rag.GetLength(0); j++) {
			if (j > 0) {
				Vector2 normalized = (this.rag[j, 0] - this.rag[j - 1, 0]).normalized;
				float num = Vector2.Distance(this.rag[j, 0], this.rag[j - 1, 0]);
				float d = (num > 7f) ? 0.5f : 0.25f;
				this.rag[j, 0] += normalized * (7f - num) * d;
				this.rag[j, 2] += normalized * (7f - num) * d;
				this.rag[j - 1, 0] -= normalized * (7f - num) * d;
				this.rag[j - 1, 2] -= normalized * (7f - num) * d;
				if (j > 1) {
					normalized = (this.rag[j, 0] - this.rag[j - 2, 0]).normalized;
					this.rag[j, 2] += normalized * 0.2f;
					this.rag[j - 2, 2] -= normalized * 0.2f;
				}
				if (j < this.rag.GetLength(0) - 1) {
					this.rag[j, 3] = Vector3.Slerp(this.rag[j, 3], (this.rag[j - 1, 3] * 2f + this.rag[j + 1, 3]) / 3f, 0.1f);
					this.rag[j, 5] = Vector3.Slerp(this.rag[j, 5], (this.rag[j - 1, 5] * 2f + this.rag[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(this.rag[j, 1], this.rag[j, 0]), 1f, 8f, 0.05f, 0.5f));
				}
			}
			else {
				this.rag[j, 0] = this.RagAttachPos(1f);
				this.rag[j, 2] *= 0f;
			}
		}
	}

	public override void Update(bool eu) {
		this.UpdateRag();

		if (this.stick != null) {
			base.Update(eu);
			this.stick.SetCattailPos();
			
			return;
		}
	
		if (this.beingEaten > 0f) {
			this.beingEaten += 0.1f;
			if (this.beingEaten > 1f) {
				this.Destroy();
			}
		}
		if (this.lastModeThrown && (this.firstChunk.ContactPoint.x != 0 || this.firstChunk.ContactPoint.y != 0)) {
			this.Explode();
		}
		this.lastModeThrown = this.mode == Mode.Thrown;
		if (this.firstChunk.ContactPoint.y != 0) {
			this.rotationSpeed = (this.rotationSpeed * 2f + this.firstChunk.vel.x * 5f) / 3f;
		}
		if (this.smoke != null) {
			if (this.room.ViewedByAnyCamera(this.firstChunk.pos, 300f)) {
				this.smoke.EmitSmoke(this.firstChunk.pos + this.rotation * 2f, this.rotation * 0.5f, this.SporeColor);
			}
			if (this.smoke.slatedForDeletetion || this.smoke.room != this.room) {
				this.smoke = null;
			}
		}
		else {
			this.smoke = new SporesSmoke(this.room);
			this.room.AddObject(this.smoke);
		}
		bool swallowing = false;
		if (this.mode == Mode.Carried && this.grabbedBy.Count > 0 && this.grabbedBy[0].grabber is Player && (this.grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (this.grabbedBy[0].grabber as Player).objectInStomach == null && (this.grabbedBy[0].grabber as Player).input[0].pckp) {
			int num2 = -1;
			for (int l = 0; l < 2; l++) {
				if ((this.grabbedBy[0].grabber as Player).grasps[l] != null && (this.grabbedBy[0].grabber as Player).CanBeSwallowed((this.grabbedBy[0].grabber as Player).grasps[l].grabbed)) {
					num2 = l;
					break;
				}
			}
			if (num2 > -1 && (this.grabbedBy[0].grabber as Player).grasps[num2] != null && (this.grabbedBy[0].grabber as Player).grasps[num2].grabbed == this) {
				swallowing = true;
			}
		}
		this.swallowed = Custom.LerpAndTick(this.swallowed, swallowing ? 1f : 0f, 0.05f, 0.05f);
		base.Update(eu);
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu) {
		if (result.chunk == null) {
			return false;
		}
		if (result.chunk.owner.abstractPhysicalObject.rippleLayer != this.abstractPhysicalObject.rippleLayer && !result.chunk.owner.abstractPhysicalObject.rippleBothSides && !this.abstractPhysicalObject.rippleBothSides) {
			return false;
		}
		result.chunk.vel += this.firstChunk.vel * 0.1f / result.chunk.mass;
		base.HitSomething(result, eu);
		this.Explode();
		return true;
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu) {
		if (this.stick != null) {
			this.ChangeMode(Mode.Free);
			return;
		}

		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);

		this.room?.AddObject(new SporeCloud(this.firstChunk.pos, Custom.RNV() * Random.value + throwDir.ToVector2() * 10f, this.SporeColor, 0.5f, null, -1, null, this.abstractPhysicalObject.rippleLayer));
		this.room?.PlaySound(SoundID.Slugcat_Throw_Puffball, this.firstChunk);
	}

	public override void PickedUp(Creature upPicker) {
		this.room.PlaySound(SoundID.Slugcat_Pick_Up_Puffball, this.firstChunk);
		if (ModManager.MMF && this.room.game.IsStorySession && this.room.game.cameras[0].hud != null && !this.room.game.rainWorld.progression.miscProgressionData.sporePuffTutorialShown && this.room.world.region != null && this.room.world.region.name == "LF" && MMF.cfgExtraTutorials.Value) {
			this.room.game.cameras[0].hud.textPrompt.AddMessage(this.room.game.manager.rainWorld.inGameTranslator.Translate("The use of some objects may not be obvious at first glance, experiment with everything you find!"), 60, 250, true, true);
			this.room.game.rainWorld.progression.miscProgressionData.sporePuffTutorialShown = true;
		}
	}

	public override void HitWall() {
		this.Explode();
		this.SetRandomSpin();
		this.ChangeMode(Mode.Free);

		this.forbiddenToPlayer = 10;
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk) {
		base.HitByExplosion(hitFac, explosion, hitChunk);
		this.Explode();
	}

	public override void HitByWeapon(Weapon weapon) {
		base.HitByWeapon(weapon);
		this.Explode();
	}

	public void Explode() {
		if (this.slatedForDeletetion) {
			return;
		}
		InsectCoordinator smallInsects = null;
		for (int i = 0; i < this.room.updateList.Count; i++) {
			if (this.room.updateList[i] is InsectCoordinator) {
				smallInsects = this.room.updateList[i] as InsectCoordinator;
				break;
			}
		}
		for (int j = 0; j < 70; j++) {
			this.room.AddObject(new SporeCloud(this.firstChunk.pos, Custom.RNV() * Random.value * 10f, this.SporeColor, 2f, this.thrownBy?.abstractCreature, j % 20, smallInsects, this.abstractPhysicalObject.rippleLayer));
		}
		this.room.AddObject(new SporePuffVisionObscurer(this.firstChunk.pos, this.abstractPhysicalObject.rippleLayer));
		for (int k = 0; k < 7; k++) {
			this.room.AddObject(new PuffBallSkin(this.firstChunk.pos, Custom.RNV() * Random.value * 16f, this.color, Color.Lerp(this.color, this.SporeColor, 0.5f)));
		}
		this.room.PlaySound(SoundID.Puffball_Eplode, this.firstChunk.pos);
		this.Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[this.AbstrCattail.HasDye ? 3 : 1];
		sLeaser.sprites[0] = new FSprite("BodyA", true);
		if (this.AbstrCattail.HasDye) {
			sLeaser.sprites[1] = new FSprite("Futile_White", true) {
				color = this.AbstrCattail.dyeColor,
			};
			sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(this.rag.GetLength(0), false, false);
			sLeaser.sprites[2].color = this.AbstrCattail.dyeColor;
			sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
		}
		this.AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		Vector2 position = Vector2.Lerp(this.firstChunk.lastPos, this.firstChunk.pos, timeStacker);
		Vector2 rotation = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
		float rotationDegrees = Custom.VecToDeg(rotation);
		if (this.vibrate > 0) {
			position += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
		}
		sLeaser.sprites[0].x = position.x - camPos.x;
		sLeaser.sprites[0].y = position.y - camPos.y;
		sLeaser.sprites[0].rotation = rotationDegrees;

		float size = 1f;
		if (this.beingEaten > 0f || this.swallowed > 0f) {
			size = 1f - Mathf.Max(this.beingEaten, this.swallowed * 0.5f);
		}
		size *= 0.6f;
		sLeaser.sprites[0].scaleX = size;
		sLeaser.sprites[0].scaleY = 1.8f * size;

		if (this.blink > 0) {
			sLeaser.sprites[0].color = (this.blink > 1 && Random.value < 0.5f) ? this.blinkColor : this.color;
		}
		else if (sLeaser.sprites[0].color != this.color) {
			sLeaser.sprites[0].color = this.color;
		}

		if (this.AbstrCattail.HasDye) {
			sLeaser.sprites[1].x = position.x - camPos.x;
			sLeaser.sprites[1].y = position.y - camPos.y;
			sLeaser.sprites[1].rotation = rotationDegrees;
			sLeaser.sprites[1].scaleX = size;
			sLeaser.sprites[1].scaleY = size * 0.5f;

			float num = 0f;
			Vector2 a = this.RagAttachPos(timeStacker);
			for (int i = 0; i < this.rag.GetLength(0); i++) {
				float f = i / (this.rag.GetLength(0) - 1f);
				Vector2 vector = Vector2.Lerp(this.rag[i, 1], this.rag[i, 0], timeStacker);
				float num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * 3.1415927f)) * Vector3.Slerp(this.rag[i, 4], this.rag[i, 3], timeStacker).x;
				Vector2 normalized = (a - vector).normalized;
				Vector2 a2 = Custom.PerpendicularVector(normalized);
				float d = Vector2.Distance(a, vector) / 5f;
				(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4, a - normalized * d - a2 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 1, a - normalized * d + a2 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, vector + normalized * d - a2 * num2 - camPos);
				(sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 3, vector + normalized * d + a2 * num2 - camPos);
				a = vector;
				num = num2;
			}
		}

		if (this.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		this.color = Color.Lerp(new Color(0.33f, 0.25f, 0.15f), palette.blackColor, palette.darkness * 0.8f + 0.1f);
		sLeaser.sprites[0].color = this.color;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Items");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public class AbstractCattail : AbstractPhysicalObject {
		public Color dyeColor;

		public bool HasDye => this.dyeColor.r > 0f || this.dyeColor.g > 0f || this.dyeColor.b > 0f;

		public AbstractCattail(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, Color? dyeColor = null) : base(world, type, realizedObject, pos, ID) {
			this.dyeColor = dyeColor ?? Color.black;
		}

		public override string ToString() {
			string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", [
				base.IDAndRippleLayerString,
				this.type.ToString(),
				this.pos.SaveToString(),
				this.dyeColor.r,
				this.dyeColor.g,
				this.dyeColor.b
			]);
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", this.unrecognizedAttributes);
		}
	}

	public class CattailData : PlacedObject.ResizableObjectData {
		public Vector2 panelPos;
		public float hue;

		public CattailData(PlacedObject owner) : base(owner) {
			this.panelPos = new Vector2(0f, 0f);
			this.hue = Random.value;
		}

		public override void FromString(string s) {
			base.FromString(s);

			string[] array = Regex.Split(s, "~");
			try {
				this.panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.hue = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			} catch (Exception) {
			}

			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		protected new string BaseSaveString() {
			return base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}",
				this.panelPos.x,
				this.panelPos.y,
				this.hue
			);
		}

		public override string ToString() {
			string text = this.BaseSaveString();
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}
	}

	public class ColoredCattailRepresentation : ResizeableObjectRepresentation {
		private CattailData Data => this.pObj.data as CattailData;

		public ColoredCattailRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject po, string name) : base(owner, IDstring, parentNode, po, name, false) {
			this.controlPanel = new CattailControlPanel(owner, "Cattail_Panel", this, new Vector2(0f, 100f));
			this.subNodes.Add(this.controlPanel);
			this.controlPanel.pos = this.Data.panelPos;
			this.fSprites.Add(new FSprite("pixel", true));
			this.lineSprite = this.fSprites.Count - 1;
			owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
			this.fSprites[this.lineSprite].anchorY = 0f;
		}

		public override void Refresh() {
			base.Refresh();
			base.MoveSprite(this.lineSprite, this.absPos);
			this.fSprites[this.lineSprite].scaleY = this.controlPanel.pos.magnitude;
			this.fSprites[this.lineSprite].rotation = Custom.VecToDeg(this.controlPanel.pos);
			this.Data.panelPos = this.controlPanel.pos;
			if (this.pObj.pos != this.lastPos) {
				this.lastPos = this.pObj.pos;
			}
		}

		private readonly CattailControlPanel controlPanel;

		private readonly int lineSprite;

		private Vector2 lastPos;

		public class CattailControlPanel : Panel {
			public CattailControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 25f), "Cattail") {
				this.subNodes.Add(new CattailSlider(owner, "Hue_Slider", this, new Vector2(5f, 5f), "Hue: "));
			}

			public class CattailSlider : Slider {
				private ColoredCattailRepresentation Rep => this.parentNode.parentNode as ColoredCattailRepresentation;

				private CattailData Data => this.Rep.pObj.data as CattailData;

				public CattailSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 60f) {
				}

				public override void Refresh() {
					base.Refresh();
					float num = 0f;
					switch (this.IDstring) {
						case "Hue_Slider":
							num = this.Data.hue;
							base.NumberText = Mathf.RoundToInt(num * 360f).ToString();
							break;
					}

					base.RefreshNubPos(num);
				}

				public override void NubDragged(float nubPos) {
					switch (this.IDstring) {
						case "Hue_Slider":
							this.Data.hue = nubPos;
							break;
					}

					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
			}
		}
	}
}