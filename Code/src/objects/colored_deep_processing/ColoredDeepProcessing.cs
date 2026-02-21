using System.Security.Cryptography;

namespace Floodwaters.Objects;

public class ColoredDeepProcessing : UpdatableAndDeletable, IDrawable {
	private readonly Vector2[] quad;
	private Vector2[] verts;
	public bool meshDirty;
	public int gridDiv = 1;
	private float power = 1f;

	public readonly PlacedObject pObj;
	public ColoredDeepProcessingData Data => this.pObj.data as ColoredDeepProcessingData;

	public ColoredDeepProcessing(PlacedObject placedObject, Room room) {
		this.pObj = placedObject;
		this.room = room;
		this.quad = new Vector2[4];
		this.quad[0] = this.pObj.pos;
		this.quad[1] = this.pObj.pos + this.Data.handles[0];
		this.quad[2] = this.pObj.pos + this.Data.handles[1];
		this.quad[3] = this.pObj.pos + this.Data.handles[2];
		this.gridDiv = this.GetIdealGridDiv();
		this.meshDirty = true;
	}

	public int GetIdealGridDiv() {
		float maxDist = 0f;
		for (int i = 0; i < 3; i++) {
			float dist = Vector2.Distance(this.quad[i], this.quad[i + 1]);
			if (dist > maxDist) {
				maxDist = dist;
			}
		}
		float lastDist = Vector2.Distance(this.quad[0], this.quad[3]);
		if (lastDist > maxDist) {
			maxDist = lastDist;
		}
		return Mathf.Clamp(Mathf.RoundToInt(maxDist / 150f), 1, 20);
	}

	public override void Update(bool eu) {
		base.Update(eu);
		Vector2[] newQuad = new Vector2[4];
		for (int i = 0; i < 4; i++) {
			newQuad[i] = this.pObj.pos + (i == 0 ? Vector2.zero : this.Data.handles[i - 1]);
			if (this.quad[i] != newQuad[i]) {
				this.quad[i] = newQuad[i];
				this.meshDirty = true;
			}
		}

		if (UnityEngine.Random.value < 0.071428575f) {
			if (this.power > this.room.ElectricPower) {
				this.power = Mathf.Max((UnityEngine.Random.value < 0.2f) ? 0f : this.room.ElectricPower, this.power - 1f / Mathf.Lerp(1f, 4f, UnityEngine.Random.value));
				return;
			}
			if (this.power < this.room.ElectricPower) {
				this.power = Mathf.Min((UnityEngine.Random.value < 0.2f) ? 1f : this.room.ElectricPower, this.power + 1f / Mathf.Lerp(1f, 4f, UnityEngine.Random.value));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1];
		TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh("Futile_White", this.gridDiv);
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FWColoredDeepProcessing"];
		this.verts = new Vector2[triangleMesh.vertices.Length];
		this.AddToContainer(sLeaser, rCam, null);
		this.meshDirty = true;
	}

	private void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		int idealGridDiv = this.GetIdealGridDiv();
		if (idealGridDiv != this.gridDiv) {
			this.gridDiv = idealGridDiv;
			sLeaser.sprites[0].RemoveFromContainer();
			this.InitiateSprites(sLeaser, rCam);
		}

		for (int i = 0; i <= this.gridDiv; i++) {
			for (int j = 0; j <= this.gridDiv; j++) {
				Vector2 a = Vector2.Lerp(this.quad[0], this.quad[1], j / (float) this.gridDiv);
				Vector2 b = Vector2.Lerp(this.quad[1], this.quad[2], i / (float) this.gridDiv);
				Vector2 b2 = Vector2.Lerp(this.quad[3], this.quad[2], j / (float) this.gridDiv);
				Vector2 a2 = Vector2.Lerp(this.quad[0], this.quad[3], i / (float) this.gridDiv);
				this.verts[j * (this.gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
			}
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (base.slatedForDeletetion || this.room != rCam.room) {
			sLeaser.CleanSpritesAndRemove();
			return;
		}

		if (this.meshDirty) {
			this.UpdateVerts(sLeaser, rCam);
			this.meshDirty = false;
		}

		for (int i = 0; i < this.verts.Length; i++) {
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, this.verts[i] - camPos);
		}

		if (sLeaser.sprites[0]._renderLayer != null) {
			sLeaser.sprites[0]._renderLayer._material.SetColor("_Color", this.Data.color);
			sLeaser.sprites[0]._renderLayer._material.SetFloat("_FromDepth", this.Data.fromDepth / 30f);
			sLeaser.sprites[0]._renderLayer._material.SetFloat("_ToDepth", this.Data.toDepth / 30f);
			sLeaser.sprites[0]._renderLayer._material.SetFloat("_Power", this.power);
			sLeaser.sprites[0]._renderLayer._material.SetFloat("_Intensity", this.Data.intensity);
		}
		else {
			this.meshDirty = true;
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Water");

		foreach (FSprite sprite in sLeaser.sprites) {
			sprite.RemoveFromContainer();
			newContatiner.AddChild(sprite);
		}
	}

	public class ColoredDeepProcessingData : PlacedObject.QuadObjectData {
		public Vector2 panelPos;

		public int fromDepth = 0;
		public int toDepth = 30;
		public float intensity = 0.5f;
		public Color color = Color.blue;

		public ColoredDeepProcessingData(PlacedObject owner) : base(owner) {
		}

		public override void FromString(string s) {
			base.FromString(s);

			try {
				string[] array = Regex.Split(s, "~");
				this.panelPos.x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.fromDepth = int.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.toDepth = int.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.intensity = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color.r = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color.g = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.color.b = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 14);
			}
			catch (Exception) { }
		}

		public override string ToString() {
			string baseString = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}",
				this.panelPos.x,
				this.panelPos.y,
				this.fromDepth,
				this.toDepth,
				this.intensity,
				this.color.r,
				this.color.g,
				this.color.b
			);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", this.unrecognizedAttributes);
		}
	}

	public class ColoredDeepProcessingRepresentation : QuadObjectRepresentation {
		public ColoredDeepProcessing DP;
		private readonly ColoredDeepProcessingPanel controlPanel;
		private readonly int lineSprite;

		public ColoredDeepProcessingRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
			this.controlPanel = new ColoredDeepProcessingPanel(owner, "Deep_Processing_Panel", this, new Vector2(0f, 100f));
			this.subNodes.Add(this.controlPanel);
			this.controlPanel.pos = (pObj.data as ColoredDeepProcessingData).panelPos;
			this.fSprites.Add(new FSprite("pixel", true));
			this.lineSprite = this.fSprites.Count - 1;
			owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
			this.fSprites[this.lineSprite].anchorY = 0f;

			this.DP = owner.room.updateList.OfType<ColoredDeepProcessing>().FirstOrDefault(x => x.pObj == this.pObj);
			if (this.DP == null) {
				this.DP = new ColoredDeepProcessing(pObj, this.owner.room);
				owner.room.AddObject(this.DP);
			}
		}

		public override void Refresh() {
			base.Refresh();
			base.MoveSprite(this.lineSprite, this.absPos);
			this.fSprites[this.lineSprite].scaleY = this.controlPanel.pos.magnitude;
			this.fSprites[this.lineSprite].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.controlPanel.absPos);
			(this.pObj.data as ColoredDeepProcessingData).panelPos = this.controlPanel.pos;
		}


		public class ColoredDeepProcessingPanel : Panel {
			public ColoredDeepProcessingPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 125f), "Colored Deep Processing") {
				this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Color_R_Slider", this, new Vector2(5f, 105f), "Red: "));
				this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Color_G_Slider", this, new Vector2(5f, 85f), "Green: "));
				this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Color_B_Slider", this, new Vector2(5f, 65f), "Blue: "));
				this.subNodes.Add(new ColoredDeepProcessSlider(owner, "From_Depth_Slider", this, new Vector2(5f, 45f), "From Depth: "));
				this.subNodes.Add(new ColoredDeepProcessSlider(owner, "To_Depth_Slider", this, new Vector2(5f, 25f), "To Depth: "));
				this.subNodes.Add(new ColoredDeepProcessSlider(owner, "Intensity_Slider", this, new Vector2(5f, 5f), "Intensity: "));
			}

			public class ColoredDeepProcessSlider : Slider {
				public ColoredDeepProcessSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
				}

				public override void Refresh() {
					base.Refresh();
					ColoredDeepProcessingData data = (this.parentNode.parentNode as ColoredDeepProcessingRepresentation).pObj.data as ColoredDeepProcessingData;

					switch (this.IDstring) {
						case "From_Depth_Slider": {
							base.NumberText = data.fromDepth.ToString();
							base.RefreshNubPos(data.fromDepth / 30f);
							break;
						}

						case "To_Depth_Slider": {
							base.NumberText = data.toDepth.ToString();
							base.RefreshNubPos(data.toDepth / 30f);
							break;
						}

						case "Intensity_Slider": {
							base.NumberText = Mathf.FloorToInt(data.intensity * 100f).ToString();
							base.RefreshNubPos(data.intensity);
							break;
						}

						case "Color_R_Slider": {
							base.NumberText = Mathf.FloorToInt(data.color.r * 255f).ToString();
							base.RefreshNubPos(data.color.r);
							break;
						}

						case "Color_G_Slider": {
							base.NumberText = Mathf.FloorToInt(data.color.g * 255f).ToString();
							base.RefreshNubPos(data.color.g);
							break;
						}

						case "Color_B_Slider": {
							base.NumberText = Mathf.FloorToInt(data.color.b * 255f).ToString();
							base.RefreshNubPos(data.color.b);
							break;
						}
					}
				}

				public override void NubDragged(float nubPos) {
					ColoredDeepProcessingData data = (this.parentNode.parentNode as ColoredDeepProcessingRepresentation).pObj.data as ColoredDeepProcessingData;

					switch (this.IDstring) {
						case "From_Depth_Slider": {
							data.fromDepth = Mathf.Min(Mathf.RoundToInt(nubPos * 30), data.toDepth);
							break;
						}

						case "To_Depth_Slider": {
							data.toDepth = Mathf.Max(Mathf.RoundToInt(nubPos * 30), data.fromDepth);
							break;
						}

						case "Intensity_Slider": {
							data.intensity = nubPos;
							break;
						}

						case "Color_R_Slider": {
							data.color.r = nubPos;
							break;
						}

						case "Color_G_Slider": {
							data.color.g = nubPos;
							break;
						}

						case "Color_B_Slider": {
							data.color.b = nubPos;
							break;
						}
					}

					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
			}
		}
	}
}