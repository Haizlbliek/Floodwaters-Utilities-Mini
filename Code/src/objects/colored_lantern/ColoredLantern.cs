namespace Floodwaters.Objects;

public class ColoredLantern : PlayerCarryableItem, IDrawable, IProvideWarmth {
	float IProvideWarmth.warmth {
		get {
			return RainWorldGame.DefaultHeatSourceWarmth;
		}
	}

	Room IProvideWarmth.loadedRoom {
		get {
			return this.room;
		}
	}

	Vector2 IProvideWarmth.Position() {
		return base.firstChunk.pos;
	}

	float IProvideWarmth.range {
		get {
			return 350f;
		}
	}

	public AbstractColoredLantern Abstr => this.abstractPhysicalObject as AbstractColoredLantern;
	public PlacedObject PO => this.Abstr.po;
	public ColoredLanternObjectData Data => this.Abstr.data;
	public ColoredLanternStickObjectData StickData => this.Data as ColoredLanternStickObjectData;

	public Vector2 rotation;
	public Vector2 lastRotation;
	public Vector2? setRotation;
	public LightSource lightSource;
	public float[,] flicker;

	public bool hasStick = false;
	public Vector2[] stickPositions;

	public Color color1;
	public Color color2;
	public bool dead;

	public ColoredLantern(AbstractColoredLantern abstractPhysicalObject) : base(abstractPhysicalObject) {
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.2f);
		this.bodyChunkConnections = [];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		this.bounce = 0.4f;
		this.surfaceFriction = 0.8f;
		this.collisionLayer = 1;
		base.waterFriction = 0.95f;
		base.buoyancy = 0.8f;
		this.hasStick = abstractPhysicalObject.data is ColoredLanternStickObjectData;

		this.flicker = new float[2, 3];
		for (int i = 0; i < this.flicker.GetLength(0); i++) {
			this.flicker[i, 0] = 1f;
			this.flicker[i, 1] = 1f;
			this.flicker[i, 2] = 1f;
		}

		if (this.hasStick) {
			this.stickPositions = new Vector2[(int) Mathf.Clamp(this.StickData.stickEnd.magnitude / 11.0f, 3f, 30f)];
		}
	}

	public void UpdateColors() {
		this.color1 = this.Data.color1;
		this.color2 = this.Data.color2;
		this.dead = this.Data.dead;

		if (this.dead) {
			float average1 = (this.color1.r + this.color1.g + this.color1.b) / 3f;
			float average2 = (this.color2.r + this.color2.g + this.color2.b) / 3f;
			this.color1 = Color.Lerp(this.color1, new Color(average1, average1, average1, this.color1.a), 0.5f);
			this.color2 = Color.Lerp(this.color2, new Color(average2, average2, average2, this.color2.a), 0.5f);
		}
	}

	public override void Update(bool eu) {
		base.Update(eu);

		this.UpdateColors();

		for (int i = 0; i < this.flicker.GetLength(0); i++) {
			this.flicker[i, 1] = this.flicker[i, 0];
			this.flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
			this.flicker[i, 0] = Custom.LerpAndTick(this.flicker[i, 0], this.flicker[i, 2], 0.05f, 0.033333335f);
			if (UnityEngine.Random.value < 0.2f) {
				this.flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
			}
			this.flicker[i, 2] = Mathf.Lerp(this.flicker[i, 2], 1f, 0.01f);
		}
		if (this.lightSource == null && !this.dead) {
			this.lightSource = new LightSource(base.firstChunk.pos, false, this.color2, this) {
				affectedByPaletteDarkness = 0.5f
			};
			this.room.AddObject(this.lightSource);
		}
		else if (this.lightSource != null) {
			this.lightSource.setPos = new Vector2?(base.firstChunk.pos);
			this.lightSource.setRad = new float?(250f * this.flicker[0, 0]);
			this.lightSource.setAlpha = new float?(1f);
			this.lightSource.color = this.color2;
			if (this.lightSource.slatedForDeletetion || this.lightSource.room != this.room) {
				this.lightSource = null;
			}
		}
		this.lastRotation = this.rotation;
		if (this.hasStick) {
			base.firstChunk.pos = this.PO.pos;
			base.firstChunk.vel *= 0f;
			this.rotation = this.StickData.stickEnd;
			base.firstChunk.collideWithObjects = false;
			base.firstChunk.collideWithTerrain = false;
			this.canBeHitByWeapons = false;
			return;
		}
		else {
			base.firstChunk.collideWithObjects = true;
			this.canBeHitByWeapons = true;
		}
		base.firstChunk.collideWithTerrain = this.grabbedBy.Count == 0;
		if (this.grabbedBy.Count > 0) {
			this.rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
			this.rotation.y = -Mathf.Abs(this.rotation.y);
		}
		if (this.setRotation != null) {
			this.rotation = this.setRotation.Value;
			this.setRotation = null;
		}
		this.rotation = (this.rotation - Custom.PerpendicularVector(this.rotation) * ((base.firstChunk.ContactPoint.y < 0) ? 0.15f : 0.05f) * base.firstChunk.vel.x).normalized;
		if (base.firstChunk.ContactPoint.y < 0) {
			BodyChunk firstChunk = base.firstChunk;
			firstChunk.vel.x *= 0.8f;
		}
	}

	public override void PlaceInRoom(Room placeRoom) {
		base.PlaceInRoom(placeRoom);

		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));

		if (this.hasStick) {
			Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int) this.PO.pos.x);
			for (int i = 0; i < this.stickPositions.Length; i++) {
				this.stickPositions[i] = Custom.RNV() * UnityEngine.Random.value;
			}
			UnityEngine.Random.state = state;
		}

		this.rotation = Custom.RNV();
		this.lastRotation = this.rotation;
	}

	public override void HitByWeapon(Weapon weapon) {
		base.HitByWeapon(weapon);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) {
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 5f && firstContact) {
			Vector2 pos = base.bodyChunks[chunk].pos + direction.ToVector2() * base.bodyChunks[chunk].rad * 0.9f;
			int num = 0;
			while (num < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f))) {
				this.room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * UnityEngine.Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), Color.Lerp(this.color1, new Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
				num++;
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[(this.hasStick) ? 5 : 4];
		sLeaser.sprites[0] = new FSprite("DangleFruit0A", true);
		sLeaser.sprites[1] = new FSprite("DangleFruit0B", true);
		for (int i = 0; i < 2; i++) {
			sLeaser.sprites[i].scaleX = 0.8f;
			sLeaser.sprites[i].scaleY = 0.9f;
		}
		sLeaser.sprites[2] = new FSprite("Futile_White", true) {
			shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"]
		};
		sLeaser.sprites[3] = new FSprite("Futile_White", true) {
			shader = rCam.game.rainWorld.Shaders["LightSource"]
		};
		if (this.hasStick) {
			sLeaser.sprites[4] = TriangleMesh.MakeLongMesh(this.stickPositions.Length, false, false);
		}
		this.AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		this.SetColor(sLeaser);

		Vector2 currentPos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 currentRotation = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
		for (int i = 0; i < 2; i++) {
			sLeaser.sprites[i].x = currentPos.x - camPos.x;
			sLeaser.sprites[i].y = currentPos.y - camPos.y;
			sLeaser.sprites[i].rotation = Custom.VecToDeg(currentRotation);
		}

		if (this.dead) {
			sLeaser.sprites[2].isVisible = false;
			sLeaser.sprites[3].isVisible = false;
		}
		else {
			sLeaser.sprites[2].isVisible = true;
			sLeaser.sprites[3].isVisible = true;
			sLeaser.sprites[2].x = currentPos.x /*- vector2.x * 3f*/ - camPos.x;
			sLeaser.sprites[2].y = currentPos.y /*- vector2.y * 3f*/ - camPos.y;
			sLeaser.sprites[2].scale = Mathf.Lerp(this.flicker[0, 1], this.flicker[0, 0], timeStacker) * 2f;
			sLeaser.sprites[3].x = currentPos.x /*- vector2.x * 3f*/ - camPos.x;
			sLeaser.sprites[3].y = currentPos.y /*- vector2.y * 3f*/ - camPos.y;
			sLeaser.sprites[3].scale = Mathf.Lerp(this.flicker[1, 1], this.flicker[1, 0], timeStacker) * 200f / 8f;
		}

		if (this.hasStick) {
			Vector2 stickStart = this.PO.pos + this.StickData.stickEnd;
			Vector2 stickEnd = currentPos + this.StickData.stickEnd.normalized * -25f;
			Vector2 a = stickStart + Custom.DirVec(stickEnd, stickStart) * 5f;
			float num = 1f;
			for (int j = 0; j < this.stickPositions.Length; j++) {
				float t = j / (float) (this.stickPositions.Length - 1);
				float num2 = Mathf.Lerp(1f + Mathf.Min(this.StickData.stickEnd.magnitude / 190f, 3f), 0.5f, t);
				Vector2 vector5 = Vector2.Lerp(stickStart, stickEnd, t) + this.stickPositions[j] * Mathf.Lerp(num2 * 0.6f, 1f, t);
				Vector2 normalized = (a - vector5).normalized;
				Vector2 a2 = Custom.PerpendicularVector(normalized);
				float d = Vector2.Distance(a, vector5) / 5f;
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4, a - normalized * d - a2 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 1, a - normalized * d + a2 * (num2 + num) * 0.5f - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 2, vector5 + normalized * d - a2 * num2 - camPos);
				(sLeaser.sprites[4] as TriangleMesh).MoveVertice(j * 4 + 3, vector5 + normalized * d + a2 * num2 - camPos);
				a = vector5;
				num = num2;
			}
		}
		if (base.slatedForDeletetion) {
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void SetColor(RoomCamera.SpriteLeaser sLeaser) {
		sLeaser.sprites[0].color = this.color1;
		sLeaser.sprites[2].color = Color.Lerp(this.color1, new Color(1f, 1f, 1f), 0.3f);
		sLeaser.sprites[3].color = Color.Lerp(this.color1, new Color(1.0f, 1.0f, 1.0f) * 0.333f * (this.color1.r + this.color1.g + this.color1.b), 0.3f);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
		if (this.hasStick) {
			sLeaser.sprites[4].color = palette.blackColor;
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Items");

		for (int i = 0; i < sLeaser.sprites.Length; i++) {
			sLeaser.sprites[i].RemoveFromContainer();
		}

		if (this.hasStick) {
			newContatiner.AddChild(sLeaser.sprites[4]);
		}

		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[3]);
	}

	public class ColoredLanternObjectData : PlacedObject.ConsumableObjectData {
		public Color color1;
		public Color color2;
		public bool dead;

		public ColoredLanternObjectData(PlacedObject owner)
		: base(owner) {
			this.panelPos = new Vector2(0, 0);
			this.color1 = new Color(1.0f, 0.2f, 0.0f);
			this.color2 = new Color(1.0f, 0.2f, 0.0f);
			this.dead = false;
		}

		public static Color String2Color(string hex) {
			if (string.IsNullOrEmpty(hex))
				throw new ArgumentException("Hex string is null or empty");

			hex = hex.TrimStart('#');
			if (hex.Length != 6 && hex.Length != 8) {
				throw new ArgumentException("Hex string must be 6 (RGB) or 8 (RGBA) characters long");
			}

			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

			return new Color32(r, g, b, 255);
		}

		public static string Color2String(Color color) {
			byte r = (byte) (color.r * 255);
			byte g = (byte) (color.g * 255);
			byte b = (byte) (color.b * 255);

			return $"#{r:X2}{g:X2}{b:X2}";
		}

		public override void FromString(string s) {
			Debug.Log(s);
			base.FromString(s);

			try {
				string[] array = Regex.Split(s, "~");
				if (array.Length > 6) {
					this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.color1 = String2Color(array[4]);
					this.color2 = String2Color(array[5]);
					this.dead = bool.Parse(array[6]);
					this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
				}
			} catch (Exception) {}
		}

		protected new string BaseSaveString() {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", [
				this.panelPos.x,
				this.panelPos.y,
				this.minRegen,
				this.maxRegen,
				Color2String(this.color1),
				Color2String(this.color2),
				this.dead
			]);
		}

		public override string ToString() {
			string text = this.BaseSaveString();
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}
	}

	public class ColoredLanternStickObjectData : ColoredLanternObjectData {
		public Vector2 stickEnd;

		public ColoredLanternStickObjectData(PlacedObject owner)
		: base(owner) {
			this.stickEnd = new Vector2(0f, 100f);
		}

		public override void FromString(string s) {
			Debug.Log(s);
			base.FromString(s);

			try {
				string[] array = Regex.Split(s, "~");
				if (array.Length > 8) {
					this.stickEnd.x = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.stickEnd.y = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 9);
				}
			} catch (Exception) {}
		}

		protected new string BaseSaveString() {
			return base.BaseSaveString() + string.Format(System.Globalization.CultureInfo.InvariantCulture, "~{0}~{1}", [
				this.stickEnd.x,
				this.stickEnd.y
			]);
		}

		public override string ToString() {
			string text = this.BaseSaveString();
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}
	}

	public class AbstractColoredLantern : AbstractConsumable {
		public ColoredLanternObjectData data;
		public PlacedObject po;

		public AbstractColoredLantern(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, ColoredLanternObjectData data, PlacedObject po)
		: base(world, type, realizedObject, pos, ID, originRoom, placedObjectIndex, data) {
			this.data = data;
			this.po = po;
		}

		public override string ToString() {
			string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}", this.ID.ToString(), this.type.ToString(), this.pos.SaveToString(), this.data.ToString());
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", this.unrecognizedAttributes);
		}
	}

	public class ColoredLanternRepresentaion : PlacedObjectRepresentation {
		private class ColoredLanternControlPanel : Panel, IDevUISignals {
			private class ColorSlider : Slider {
				public ColorSlider(DevUI owner, string IDString, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDString, parentNode, pos, title, false, 110f) {
				}

				public override void Refresh() {
					base.Refresh();

					ColoredLanternObjectData data = (this.parentNode.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternObjectData;

					float from = -1.0f;

					if (this.IDstring == null)
						return;

					switch (this.IDstring) {
						case "color1r":
							from = data.color1.r;
							break;

						case "color1g":
							from = data.color1.g;
							break;

						case "color1b":
							from = data.color1.b;
							break;

						case "color2r":
							from = data.color2.r;
							break;

						case "color2g":
							from = data.color2.g;
							break;

						case "color2b":
							from = data.color2.b;
							break;
					}

					if (from >= 0.0f) {
						base.NumberText = ((int) (from * 255f)).ToString();
						base.RefreshNubPos(from);
					}
				}

				public override void NubDragged(float nubPos) {
					bool set = false;

					ColoredLanternObjectData data = (this.parentNode.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternObjectData;

					if (this.IDstring == null)
						return;

					switch (this.IDstring) {
						case "color1r":
							set = true;
							data.color1.r = nubPos;
							break;

						case "color1g":
							set = true;
							data.color1.g = nubPos;
							break;

						case "color1b":
							set = true;
							data.color1.b = nubPos;
							break;

						case "color2r":
							set = true;
							data.color2.r = nubPos;
							break;

						case "color2g":
							set = true;
							data.color2.g = nubPos;
							break;

						case "color2b":
							set = true;
							data.color2.b = nubPos;
							break;
					}

					if (set) {
						this.parentNode.parentNode.Refresh();
						this.Refresh();
					}
					else {
						base.NubDragged(nubPos);
					}
				}
			}

			private readonly Button typeButton;

			public ColoredLanternControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), name) {
				this.size.y += 20f * 5f;
				this.typeButton = new Button(owner, "type", this, new Vector2(5f, 125f), 240f, "TYPE");

				this.subNodes.Add(this.typeButton);
				this.subNodes.Add(new ColorSlider(owner, "color1r", this, new Vector2(5f, 105f), "Color 1 r: "));
				this.subNodes.Add(new ColorSlider(owner, "color1g", this, new Vector2(5f, 85f), "Color 1 g: "));
				this.subNodes.Add(new ColorSlider(owner, "color1b", this, new Vector2(5f, 65f), "Color 1 b: "));
				this.subNodes.Add(new ColorSlider(owner, "color2r", this, new Vector2(5f, 45f), "Color 2 r: "));
				this.subNodes.Add(new ColorSlider(owner, "color2g", this, new Vector2(5f, 25f), "Color 2 g: "));
				this.subNodes.Add(new ColorSlider(owner, "color2b", this, new Vector2(5f, 5f), "Color 2 b: "));
			}

			public override void Refresh() {
				base.Refresh();

				if (((this.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternObjectData).dead) {
					this.typeButton.Text = "Dead";
				}
				else {
					this.typeButton.Text = "Normal";
				}
			}

			public void Signal(DevUISignalType type, DevUINode sender, string message) {
				ColoredLanternObjectData data = (this.parentNode as ColoredLanternRepresentaion).pObj.data as ColoredLanternObjectData;

				string idstring = sender.IDstring;
				if (idstring != null) {
					if (idstring == "type")
						data.dead = !data.dead;
				}
				this.Refresh();
			}
		}

		public ColoredLanternRepresentaion(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject placedObject, string name)
		: base(owner, IDstring, parentNode, placedObject, name) {
			this.controlPanel = new ColoredLanternControlPanel(owner, "Panel", this, new Vector2(0f, 100f), placedObject.type.ToString());
			this.subNodes.Add(this.controlPanel);
			this.controlPanel.pos = (this.pObj.data as ColoredLanternObjectData).panelPos;
			this.fSprites.Add(new FSprite("pixel", true));
			owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
			this.fSprites[this.fSprites.Count - 1].anchorY = 0f;
		}

		public override void Refresh() {
			base.Refresh();
			base.MoveSprite(this.fSprites.Count - 1, this.absPos);
			this.fSprites[this.fSprites.Count - 1].scaleY = this.controlPanel.pos.magnitude;
			this.fSprites[this.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.controlPanel.absPos);
			(this.pObj.data as ColoredLanternObjectData).panelPos = (this.subNodes[this.subNodes.Count - 1] as Panel).pos;
		}

		private readonly ColoredLanternControlPanel controlPanel;
	}

	public class ColoredLanternStickRepresentaion : PlacedObjectRepresentation {
		private class ColoredLanternStickControlPanel : Panel, IDevUISignals {
			private class ColorSlider : Slider {
				public ColorSlider(DevUI owner, string IDString, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDString, parentNode, pos, title, false, 110f) {
				}

				public override void Refresh() {
					base.Refresh();

					ColoredLanternObjectData data = (this.parentNode.parentNode as ColoredLanternStickRepresentaion).pObj.data as ColoredLanternObjectData;

					float from = -1.0f;

					if (this.IDstring == null)
						return;

					switch (this.IDstring) {
						case "color1r":
							from = data.color1.r;
							break;

						case "color1g":
							from = data.color1.g;
							break;

						case "color1b":
							from = data.color1.b;
							break;

						case "color2r":
							from = data.color2.r;
							break;

						case "color2g":
							from = data.color2.g;
							break;

						case "color2b":
							from = data.color2.b;
							break;
					}

					if (from >= 0.0f) {
						base.NumberText = ((int) (from * 255f)).ToString();
						base.RefreshNubPos(from);
					}
				}

				public override void NubDragged(float nubPos) {
					bool set = false;

					ColoredLanternObjectData data = (this.parentNode.parentNode as ColoredLanternStickRepresentaion).pObj.data as ColoredLanternObjectData;

					if (this.IDstring == null)
						return;

					switch (this.IDstring) {
						case "color1r":
							set = true;
							data.color1.r = nubPos;
							break;

						case "color1g":
							set = true;
							data.color1.g = nubPos;
							break;

						case "color1b":
							set = true;
							data.color1.b = nubPos;
							break;

						case "color2r":
							set = true;
							data.color2.r = nubPos;
							break;

						case "color2g":
							set = true;
							data.color2.g = nubPos;
							break;

						case "color2b":
							set = true;
							data.color2.b = nubPos;
							break;
					}

					if (set) {
						this.parentNode.parentNode.Refresh();
						this.Refresh();
					}
					else {
						base.NubDragged(nubPos);
					}
				}
			}

			private readonly Button typeButton;

			public ColoredLanternStickControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), name) {
				this.size.y += 20f * 5f;
				this.typeButton = new Button(owner, "type", this, new Vector2(5f, 125f), 240f, "TYPE");

				this.subNodes.Add(this.typeButton);
				this.subNodes.Add(new ColorSlider(owner, "color1r", this, new Vector2(5f, 105f), "Color 1 r: "));
				this.subNodes.Add(new ColorSlider(owner, "color1g", this, new Vector2(5f, 85f), "Color 1 g: "));
				this.subNodes.Add(new ColorSlider(owner, "color1b", this, new Vector2(5f, 65f), "Color 1 b: "));
				this.subNodes.Add(new ColorSlider(owner, "color2r", this, new Vector2(5f, 45f), "Color 2 r: "));
				this.subNodes.Add(new ColorSlider(owner, "color2g", this, new Vector2(5f, 25f), "Color 2 g: "));
				this.subNodes.Add(new ColorSlider(owner, "color2b", this, new Vector2(5f, 5f), "Color 2 b: "));
			}

			public override void Refresh() {
				base.Refresh();

				if (((this.parentNode as ColoredLanternStickRepresentaion).pObj.data as ColoredLanternObjectData).dead) {
					this.typeButton.Text = "Dead";
				}
				else {
					this.typeButton.Text = "Normal";
				}
			}

			public void Signal(DevUISignalType type, DevUINode sender, string message) {
				ColoredLanternObjectData data = (this.parentNode as ColoredLanternStickRepresentaion).pObj.data as ColoredLanternObjectData;

				string idstring = sender.IDstring;
				if (idstring != null) {
					if (idstring == "type")
						data.dead = !data.dead;
				}
				this.Refresh();
			}
		}

		private readonly ColoredLanternStickControlPanel controlPanel;

		private readonly Handle stickEndHandle;

		public ColoredLanternStickRepresentaion(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject placedObject, string name)
		: base(owner, IDstring, parentNode, placedObject, name) {
			this.controlPanel = new ColoredLanternStickControlPanel(owner, "Panel", this, new Vector2(0f, 100f), placedObject.type.ToString());
			this.subNodes.Add(this.controlPanel);
			this.controlPanel.pos = (this.pObj.data as ColoredLanternStickObjectData).panelPos;

			this.fSprites.Add(new FSprite("pixel", true));
			owner.placedObjectsContainer.AddChild(this.fSprites[1]);
			this.fSprites[1].anchorY = 0f;

			this.fSprites.Add(new FSprite("pixel", true));
			owner.placedObjectsContainer.AddChild(this.fSprites[2]);
			this.fSprites[2].anchorY = 0f;

			this.stickEndHandle = new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f));
			this.subNodes.Add(this.stickEndHandle);
			this.stickEndHandle.pos = (this.pObj.data as ColoredLanternStickObjectData).stickEnd;
		}

		public override void Refresh() {
			base.Refresh();

			base.MoveSprite(1, this.absPos);
			this.fSprites[1].scaleY = this.controlPanel.pos.magnitude;
			this.fSprites[1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.controlPanel.absPos);

			base.MoveSprite(2, this.absPos);
			this.fSprites[2].scaleY = this.stickEndHandle.pos.magnitude;
			this.fSprites[2].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.stickEndHandle.absPos);

			(this.pObj.data as ColoredLanternStickObjectData).panelPos = this.controlPanel.pos;
			(this.pObj.data as ColoredLanternStickObjectData).stickEnd = this.stickEndHandle.pos;
		}
	}
}