
namespace Floodwaters.Objects;

public class WaterDripSource : UpdatableAndDeletable {
	public PlacedObject pObj;
	public List<IntVector2> dripTiles;
	public GooDripSource.GooDripsData Data => this.pObj.data as GooDripSource.GooDripsData;

	public WaterDripSource(PlacedObject pObj) {
		this.pObj = pObj;
	}

	public override void Update(bool eu) {
		base.Update(eu);
		if (this.dripTiles == null) {
			this.RefreshCeilingTiles();
		}

		for (int i = 0; i < this.dripTiles.Count; i++) {
			if (UnityEngine.Random.value < this.Data.frequency * 0.1f) {
				IntVector2 pos = this.dripTiles[UnityEngine.Random.Range(0, this.dripTiles.Count)];
				Vector2 ceilingPoint = WaterDripSource.GetCeilingPoint(this.room, pos, UnityEngine.Random.value);
				this.room.AddObject(new WaterDrip(ceilingPoint, Vector2.zero, false));
			}
		}
	}

	private static Vector2 GetCeilingPoint(Room room, IntVector2 pos, float value) {
		return room.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-10f, 10f, value), 6f);
	}

	public void RefreshCeilingTiles() {
		if (this.room != null) {
			this.dripTiles = WaterDripSource.FindCeilingTiles(this.room, this.pObj.pos, this.Data.Rad);
			return;
		}

		this.dripTiles = null;
	}

	public static List<IntVector2> FindCeilingTiles(Room room, Vector2 pos, float radius) {
		List<IntVector2> list = [];
		IntVector2 tilePosition = room.GetTilePosition(pos);
		int num = tilePosition.x - (int)radius;
		int num2 = tilePosition.x + (int)radius;
		int num3 = tilePosition.y - (int)radius;
		int num4 = tilePosition.y + (int)radius;
		for (int i = num3; i <= num4; i++) {
			for (int j = num; j <= num2; j++) {
				Room.Tile tile = room.GetTile(j, i);
				if (Custom.DistLess(pos, room.MiddleOfTile(j, i), radius) && (tile.Terrain == Room.Tile.TerrainType.Air || tile.Terrain == Room.Tile.TerrainType.Slope) && room.HasAnySolid(j, i + 1)) {
					list.Add(new IntVector2(j, i));
				}
			}
		}

		return list;
	}




	public class WaterDripsRepresentation : ResizeableObjectRepresentation {
		private GooDripSource.GooDripsData Data {
			get {
				return this.pObj.data as GooDripSource.GooDripsData;
			}
		}

		public WaterDripsRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, true) {
			this.controlPanel = new WaterDripsControlPanel(owner, "Goo_Drips_Panel", this, new Vector2(0f, 100f));
			this.subNodes.Add(this.controlPanel);
			this.controlPanel.pos = this.Data.panelPos;
			this.fSprites.Add(new FSprite("pixel", true));
			this.lineSprite = this.fSprites.Count - 1;
			owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
			this.fSprites[this.lineSprite].anchorY = 0f;
			this.drips = owner.room.updateList.FirstOrDefault(delegate(UpdatableAndDeletable obj) {
				return obj is WaterDripSource waterDripSource && waterDripSource.pObj == pObj;
			}) as WaterDripSource;

			if (this.drips == null) {
				this.drips = new WaterDripSource(pObj);
				owner.room.AddObject(this.drips);
			}
		}

		public override void Refresh() {
			base.Refresh();
			base.MoveSprite(this.lineSprite, this.absPos);
			this.fSprites[this.lineSprite].scaleY = this.controlPanel.pos.magnitude;
			this.fSprites[this.lineSprite].rotation = Custom.VecToDeg(this.controlPanel.pos);
			this.Data.panelPos = this.controlPanel.pos;
			if (this.pObj.pos != this.lastPos) {
				this.lastPos = this.pObj.pos;
				this.drips?.RefreshCeilingTiles();
			}
		}

		private WaterDripSource drips;

		private WaterDripsControlPanel controlPanel;

		private int lineSprite;

		private Vector2 lastPos;

		public class WaterDripsControlPanel : Panel {
			public WaterDripsControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "Water Drips") {
				this.subNodes.Add(new WaterDripsSlider(owner, "Frequency_Slider", this, new Vector2(5f, 5f), "Frequency: "));
			}

			public class WaterDripsSlider : Slider {
				private WaterDripsRepresentation Rep {
					get {
						return this.parentNode.parentNode as WaterDripsRepresentation;
					}
				}

				private GooDripSource.GooDripsData Data {
					get {
						return this.Rep.pObj.data as GooDripSource.GooDripsData;
					}
				}

				public WaterDripsSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
				}

				public override void Refresh() {
					base.Refresh();
					float num = 0f;
					string idstring = this.IDstring;
					if (idstring != null && idstring == "Frequency_Slider") {
						num = this.Data.frequency;
						base.NumberText = Mathf.RoundToInt(num * 100f).ToString() + "%";
					}
					base.RefreshNubPos(num);
				}

				public override void NubDragged(float nubPos) {
					string idstring = this.IDstring;
					if (idstring != null && idstring == "Frequency_Slider") {
						this.Data.frequency = nubPos;
					}
					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
			}
		}
	}
}