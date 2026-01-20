using System.Security.Cryptography;

namespace Floodwaters.Objects;

public class ColoredDeepProcessing : UpdatableAndDeletable, IDrawable {
	private readonly Vector2[] quad;

	private float power = 1f;

	private FContainer rendererNodeContainer;
	private FGameObjectNode rendererNode;
	private GameObject renderer;
	private Mesh mesh;

	private static MaterialPropertyBlock mpb;
	private static Material material;

	public readonly PlacedObject pObj;
	public ColoredDeepProcessingData Data => this.pObj.data as ColoredDeepProcessingData;

	public ColoredDeepProcessing(PlacedObject placedObject) {
		this.pObj = placedObject;
		this.quad = new Vector2[4];
		this.quad[0] = placedObject.pos;
		this.quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		this.quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		this.quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];

		mpb ??= new MaterialPropertyBlock();
	}

	public override void Update(bool eu) {
		base.Update(eu);
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

	private GameObject CreateMeshObject() {
		this.DisposeMesh();
		this.renderer = new GameObject("Colored Deep Processing Renderer", [ typeof(MeshFilter), typeof(MeshRenderer) ]);
		MeshFilter filter = this.renderer.GetComponent<MeshFilter>();
		MeshRenderer renderer = this.renderer.GetComponent<MeshRenderer>();
		this.mesh = new Mesh {
			name = "Colored Deep Processing Mesh",
			bounds = new Bounds(Vector3.zero, new Vector3(100_000f, 100_000f, 100_000f))
		};
		this.mesh.SetVertices(new Vector3[]{ Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero });
		this.mesh.SetIndices([ 0, 1, 2, 0, 2, 3 ], MeshTopology.Triangles, 0);
		this.mesh.SetUVs(0, new Vector2[]{ new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) });
		filter.sharedMesh = this.mesh;
		mpb.Clear();
		mpb.SetColor("_Color", this.Data.color);
		mpb.SetFloat("_FromDepth", this.Data.fromDepth / 30f);
		mpb.SetFloat("_ToDepth", this.Data.toDepth / 30f);
		mpb.SetFloat("_Power", this.power);
		mpb.SetFloat("_Intensity", this.Data.intensity);
		renderer.sharedMaterial = material;
		renderer.SetPropertyBlock(mpb);
		return this.renderer;
	}

	private void DisposeMesh() {
		this.DisposeRendererNode();
		if (this.renderer != null) {
			UnityEngine.Object.Destroy(this.renderer);
			this.renderer = null;
		}
		if (this.mesh != null) {
			UnityEngine.Object.Destroy(this.mesh);
			this.mesh = null;
		}
	}

	private void DisposeRendererNode() {
		if (this.rendererNode == null) return;

		this.rendererNodeContainer.RemoveChild(this.rendererNode);
		this.rendererNode = null;
		this.rendererNodeContainer = null;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		material ??= new Material(rCam.game.rainWorld.Shaders["FWColoredDeepProcessing"].shader);

		sLeaser.sprites = [];
		sLeaser.containers = [ new FContainer() ];
		this.DisposeRendererNode();
		this.rendererNode = new FGameObjectNode(this.CreateMeshObject(), true, true, true) {
			shouldDestroyOnRemoveFromStage = true,
		};
		this.rendererNodeContainer = sLeaser.containers[0];
		this.rendererNodeContainer.AddChild(this.rendererNode);
		this.AddToContainer(sLeaser, rCam, null);
	}

	private void UpdateVerts() {
		this.quad[0] = this.pObj.pos;
		this.quad[1] = this.pObj.pos + (this.pObj.data as PlacedObject.QuadObjectData).handles[0];
		this.quad[2] = this.pObj.pos + (this.pObj.data as PlacedObject.QuadObjectData).handles[1];
		this.quad[3] = this.pObj.pos + (this.pObj.data as PlacedObject.QuadObjectData).handles[2];
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (base.slatedForDeletetion || this.room != rCam.room) {
			sLeaser.CleanSpritesAndRemove();
			return;
		}

		FGameObjectNode obj = sLeaser.containers[0].GetChildAt(0) as FGameObjectNode;
		obj.SetPosition(-camPos);
		if (obj.gameObject) {
			MeshRenderer renderer = obj.gameObject.GetComponent<MeshRenderer>();
			renderer.GetPropertyBlock(mpb);
			mpb.SetColor("_Color", this.Data.color);
			mpb.SetFloat("_FromDepth", this.Data.fromDepth / 30f);
			mpb.SetFloat("_ToDepth", this.Data.toDepth / 30f);
			mpb.SetFloat("_Power", this.power);
			mpb.SetFloat("_Intensity", this.Data.intensity);
			renderer.SetPropertyBlock(mpb);
		}
		this.UpdateVerts();

		Vector3[] vertices = [
			new Vector3(this.quad[0].x, this.quad[0].y, 0f),
			new Vector3(this.quad[1].x, this.quad[1].y, 0f),
			new Vector3(this.quad[2].x, this.quad[2].y, 0f),
			new Vector3(this.quad[3].x, this.quad[3].y, 0f)
		];
		this.mesh.SetVertices(vertices);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Foreground");

		foreach (FContainer container in sLeaser.containers) {
			container.RemoveFromContainer();
			newContatiner.AddChild(container);
		}
	}

	public override void Destroy() {
		base.Destroy();
		this.DisposeMesh();
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
				this.DP = new ColoredDeepProcessing(pObj);
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