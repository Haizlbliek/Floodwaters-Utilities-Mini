namespace Floodwaters.Objects;

public class CustomVineSystem : UpdatableAndDeletable, IDrawable {
	public Vector2 lastPObjPos;

	public float gravity = 0.9f;
	public float pushApart = 0.15f;
	public float conRad = 10f;

	public readonly List<VinePoint> leaves = [];
	public readonly List<Point> points = [];
	public readonly List<Connection> connections = [];
	public readonly List<Point[]> vinePoints = [];
	public readonly List<CustomVineClimbable> vines = [];

	public bool refreshApplyPalette = true;

	public int TotalSprites => this.vinePObjs.Count + this.leaves.Count;

	public CustomVineSystem(Room room) {
		this.room = room;
		if (this.room.climbableVines == null) {
			this.room.climbableVines = new ClimbableVinesSystem();
			this.room.AddObject(this.room.climbableVines);
		}
	}

	public readonly List<PlacedObject> vinePObjs = [];
	public readonly List<PlacedObject> connectorPObjs = [];

	public void AddVine(PlacedObject pObj) {
		this.vinePObjs.Add(pObj);
		this.Reset();
	}

	public void AddConnector(PlacedObject pObj) {
		this.connectorPObjs.Add(pObj);
		this.Reset();
	}

	public Point MergePoints(Point a, Point b) {
		if (b.forcePosition != null && a.forcePosition == null) {
			a.forcePosition = b.forcePosition;
		}
		this.points.Remove(b);
		foreach (Point[] vine in this.vinePoints) {
			for (int i = 0; i < vine.Length; i++) {
				if (vine[i] == b) {
					vine[i] = a;
				}
			}
		}
		for (int i = this.connections.Count - 1; i >= 0; i--) {
			Connection connection = this.connections[i];

			if (connection.pointA == b) {
				connection.pointA = a;
			}
			if (connection.pointB == b) {
				connection.pointB = a;
			}

			if (connection.pointA == connection.pointB) {
				this.connections.RemoveAt(i);
			}
		}

		return a;
	}

	public void Reset() {
		this.points.Clear();
		this.connections.Clear();
		this.leaves.Clear();
		this.vinePoints.Clear();

		foreach (PlacedObject pObj in this.vinePObjs) {
			CustomVineData data = pObj.data as CustomVineData;
			List<Point> points = [];
			int offset = this.points.Count;
			int pointCount = Mathf.RoundToInt(data.handlePos.magnitude / (this.conRad / Mathf.Max(data.density, 0.001f)));
			for (int i = 0; i < pointCount; i++) {
				float t = i / (pointCount - 1f);
				this.points.Add(new Point(pObj.pos + data.handlePos * t) {
					vel = Custom.RNV() * Random.value,
					vineIndex = this.vinePoints.Count
				});
				points.Add(this.points[this.points.Count - 1]);
				if (i != 0) {
					this.connections.Add(new Connection(this.points[i - 1 + offset], this.points[i + offset]));
				}
			}
			if (!data.dropLeft) this.points[offset].forcePosition = this.points[offset].pos;
			if (!data.dropRight) this.points[offset + pointCount - 1].forcePosition = this.points[offset + pointCount - 1].pos;
			this.vinePoints.Add([.. points]);
		}

		foreach (PlacedObject pObj in this.connectorPObjs) {
			CustomVineConnectorData data = pObj.data as CustomVineConnectorData;
			float mag = data.handlePos.sqrMagnitude;
			Point[] points = [.. this.points.Where(x => (x.pos - pObj.pos).sqrMagnitude <= mag).OrderBy(x => (x.pos - pObj.pos).sqrMagnitude)];
			HashSet<int> alreadyUsedLines = [];
			Point x = null;
			foreach (Point point in points) {
				if (data.onePerVine) {
					if (alreadyUsedLines.Contains(point.vineIndex)) {
						continue;
					}

					alreadyUsedLines.Add(point.vineIndex);
				}

				if (x == null) {
					x = point;
					continue;
				}

				x = this.MergePoints(x, point);
			}
		}

		foreach (CustomVineClimbable vine in this.vines) {
			this.room.RemoveObject(vine);
			vine.Destroy();
		}

		this.vines.Clear();
		for (int i = 0; i < this.vinePObjs.Count; i++) {
			this.vines.Add(new CustomVineClimbable(this, i, (this.vinePObjs[i].data as CustomVineData).presetName));

			foreach (CustomVinePreset.Child child in this.vines[i].preset.children) {
				float leafT = (float) child.placementDistance.Evaluate(new ParserContext().SetVariable("progress", 0f));
				float leafEnd = (float) child.placementDistance.Evaluate(new ParserContext().SetVariable("progress", 1f));
				while (leafT <= this.vinePoints[i].Length - leafEnd) {
					this.leaves.Add(new VinePoint(child, leafT / this.vinePoints[i].Length, i));
					leafT += (float) child.placementDistance.Evaluate(new ParserContext().SetVariable("progress", leafT / this.vinePoints[i].Length));
				}
			}
		}
		this.room.climbableVines.vines.AddRange(this.vines);
	}

	public override void Update(bool eu) {
		foreach (Connection connection in this.connections) {
			Vector2 a = Custom.DirVec(connection.pointA.pos, connection.pointB.pos);
			connection.pointA.vel -= a * this.pushApart;
			connection.pointB.vel += a * this.pushApart;
		}

		for (int i = 0; i < this.points.Count; i++) {
			Point point = this.points[i];
			point.vel.y -= this.gravity * this.room.gravity;
			point.lastPos = point.pos;
			point.pos += point.vel;
			point.vel *= 0.999f;

			if (i > 2 && i < this.points.Count - 3 && this.room.readyForAI && this.room.aimap.getTerrainProximity(point.pos) < 3) {
				SharedPhysics.TerrainCollisionData terrainCollisionData = new SharedPhysics.TerrainCollisionData(point.pos, point.lastPos, point.vel, 2f, new IntVector2(0, 0), true);
				terrainCollisionData = SharedPhysics.VerticalCollision(this.room, terrainCollisionData);
				terrainCollisionData = SharedPhysics.HorizontalCollision(this.room, terrainCollisionData);
				point.pos = terrainCollisionData.pos;
				point.vel = terrainCollisionData.vel;
				if (terrainCollisionData.contactPoint.x != 0) {
					point.vel.y *= 0.6f;
				}
				if (terrainCollisionData.contactPoint.y != 0) {
					point.vel.x *= 0.6f;
				}
			}
		}

		this.ConnectToWalls();
		for (int i = 0; i < this.connections.Count; i++) {
			this.Connect(this.connections[i]);
		}
		this.ConnectToWalls();
		for (int i = this.connections.Count - 1; i >= 0; i--) {
			this.Connect(this.connections[i]);
		}
		this.ConnectToWalls();
	}

	public void ConnectToWalls() {
		foreach (Point point in this.points) {
			if (point.forcePosition != null) {
				point.pos = (Vector2) point.forcePosition;
				point.vel *= 0f;
			}
		}
	}

	public void Connect(Connection connection) {
		float conRad = this.conRad / Mathf.Max((this.vinePObjs[connection.pointB.vineIndex].data as CustomVineData).density, 0.001f);
		Vector2 norm = (connection.pointA.pos - connection.pointB.pos).normalized;
		float dist = Vector2.Distance(connection.pointA.pos, connection.pointB.pos);
		float d = Mathf.InverseLerp(0f, conRad, dist);
		float massA = (float) this.vines[connection.pointA.vineIndex].preset.mass.Evaluate(new ParserContext().SetVariable("index", (float) connection.pointA.pointIndex).SetVariable("length", (float) this.vinePoints[connection.pointA.vineIndex].Length));
		float massB = (float) this.vines[connection.pointB.vineIndex].preset.mass.Evaluate(new ParserContext().SetVariable("index", (float) connection.pointB.pointIndex).SetVariable("length", (float) this.vinePoints[connection.pointA.vineIndex].Length));
		float dA = d * (massA / (massA + massB));
		float dB = d * (massB / (massA + massB));
		connection.pointA.pos += norm * (conRad - dist) * dA;
		connection.pointA.vel += norm * (conRad - dist) * dA;
		connection.pointB.pos -= norm * (conRad - dist) * dB;
		connection.pointB.vel -= norm * (conRad - dist) * dB;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[this.TotalSprites];

		for (int i = 0; i < this.vinePObjs.Count; i++) {
			sLeaser.sprites[i] = TriangleMesh.MakeLongMeshAtlased(this.vinePoints[i].Length, false, true);
			string shader = this.vines[i].preset.shader;
			if (shader != "") sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders[shader];
		}

		for (int i = 0; i < this.leaves.Count; i++) {
			CustomVinePreset.ChildLeaf leaf = this.leaves[i].child as CustomVinePreset.ChildLeaf;
			ParserContext context = new ParserContext().SetVariable("progress", this.leaves[i].pos).SetVariable("length", (float) this.vinePoints[this.leaves[i].vineIndex].Length).SetVariable("index", (float) i);
			string sprite = (string) leaf.sprite.Evaluate(context);
			context.SetVariable("sprite", sprite);
			Vector2 anchor = (Vector2) leaf.spriteAnchor.Evaluate(context);
			object scaleValue = leaf.spriteScale.Evaluate(context);
			Vector2 scale = (scaleValue is Vector2 vector) ? vector : new Vector2((float) scaleValue, (float) scaleValue);
			sLeaser.sprites[i + this.vinePObjs.Count] = new FSprite(CustomVinePreset.LoadElement(sprite), false) {
				anchorX = anchor.x,
				anchorY = anchor.y,
				scaleX = scale.x,
				scaleY = scale.y
			};
			this.leaves[i].rotation = (float) leaf.spriteRotation.Evaluate(context);
			this.leaves[i].offsetPosition = (Vector2) leaf.spriteOffset.Evaluate(context);
		}

		this.AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
		newContatiner ??= rCam.ReturnFContainer("Midground");

		foreach (FSprite sprite in sLeaser.sprites) {
			sprite.RemoveFromContainer();
			newContatiner.AddChild(sprite);
		}
	}

	public Vector2 OneDimensionalVinePos(float floatPos, float timeStacker) {
		int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (this.points.Count - 1)), 0, this.points.Count - 1);
		int num2 = Custom.IntClamp(num + 1, 0, this.points.Count - 1);
		float t = Mathf.InverseLerp(num, num2, floatPos * (this.points.Count - 1));
		return Vector2.Lerp(Vector2.Lerp(this.points[num].lastPos, this.points[num2].lastPos, t), Vector2.Lerp(this.points[num].pos, this.points[num2].pos, t), timeStacker);
	}

	public (Vector2, float) OneDimensionalVinePos(int vineIndex, float floatPos, float timeStacker) {
		Point[] points = this.vinePoints[vineIndex];
		int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (points.Length - 1)), 0, points.Length - 1);
		int num2 = Custom.IntClamp(num + 1, 0, points.Length - 1);
		float t = Mathf.InverseLerp(num, num2, floatPos * (points.Length - 1));
		Vector2 a = Vector2.Lerp(points[num].lastPos, points[num].pos, timeStacker);
		Vector2 b = Vector2.Lerp(points[num2].lastPos, points[num2].pos, timeStacker);
		return (Vector2.Lerp(a, b, t), Custom.AimFromOneVectorToAnother(a, b));
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		RoomPalette currentPalette = this.room.game.cameras[0].currentPalette;
		Random.State state = Random.state;
		int id = 0;
		foreach (PlacedObject p in this.vinePObjs) {
			id += p.pos.GetHashCode();
		}
		foreach (PlacedObject p in this.connectorPObjs) {
			id ^= p.pos.GetHashCode();
		}
		Random.InitState(id);
		Color baseColor = currentPalette.texture.GetPixel(15 + Random.Range(0, 9), 2 + Random.Range(0, 2));
		Random.state = state;

		baseColor += currentPalette.texture.GetPixel(4, 8) / 2f;
		float num = this.room.game.cameras[0].PaletteDarkness() * 0.1f;
		baseColor -= new Color(num, num, num, 1f);
		baseColor.r = Mathf.Clamp(baseColor.r, 0f, 1f);
		baseColor.g = Mathf.Clamp(baseColor.g, 0f, 1f);
		baseColor.b = Mathf.Clamp(baseColor.b, 0f, 1f);
		baseColor.a = 1f;

		ParserContext context = new ParserContext {
			rCam = rCam
		};
		context.variables["darkness"] = this.room.game.cameras[0].PaletteDarkness();
		context.variables["blackcolor"] = this.room.game.cameras[0].currentPalette.blackColor;

		for (int j = 0; j < this.vinePObjs.Count; j++) {
			TriangleMesh mesh = sLeaser.sprites[j] as TriangleMesh;
			CustomVinePreset preset = this.vines[j].preset;
			mesh.element = preset.texture ?? Futile.atlasManager.GetElementWithName("Futile_White");
			for (int i = 0; i < mesh.vertices.Length; i++) {
				context.variables["progress"] = i / 2 / (float) (mesh.vertices.Length / 2);
				Color color = (Color) preset.color.Evaluate(context);
				mesh.verticeColors[i] = color with { a = 1.0f };
				if (i % 4 == 0) {
					int k = i / 4;

					mesh.UVvertices[i + 0].x = k / preset.textureLoopVertexCount;
					mesh.UVvertices[i + 0].y = 0f;
					mesh.UVvertices[i + 1].x = k / preset.textureLoopVertexCount;
					mesh.UVvertices[i + 1].y = 1f;
					mesh.UVvertices[i + 2].x = (k + 1) / preset.textureLoopVertexCount;
					mesh.UVvertices[i + 2].y = 0f;
					mesh.UVvertices[i + 3].x = (k + 1) / preset.textureLoopVertexCount;
					mesh.UVvertices[i + 3].y = 1f;
				}
			}
		}

		for (int i = 0; i < this.leaves.Count; i++) {
			context.variables["progress"] = this.leaves[i].pos;
			sLeaser.sprites[this.vinePObjs.Count + i].color = (Color) (this.leaves[i].child as CustomVinePreset.ChildLeaf).color.Evaluate(context);
		}

		this.refreshApplyPalette = false;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (sLeaser.deleteMeNextFrame) return;

		if (this.TotalSprites != sLeaser.sprites.Length) {
			foreach (FSprite sprite in sLeaser.sprites) {
				sprite.RemoveFromContainer();
			}
			this.InitiateSprites(sLeaser, rCam);
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		if (this.refreshApplyPalette) {
			this.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		for (int j = 0; j < this.vinePObjs.Count; j++) {
			TriangleMesh mesh = sLeaser.sprites[j] as TriangleMesh;

			Point[] points = this.vinePoints[j];

			Vector2 vector = Vector2.Lerp(points[0].lastPos, points[0].pos, timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(this.points[1].lastPos, points[1].pos, timeStacker), vector) * 1f;
			for (int i = 0; i < points.Length; i++) {
				Vector2 vector2 = Vector2.Lerp(points[i].lastPos, points[i].pos, timeStacker);
				if (i < points.Length - 1) {
					Vector2.Lerp(points[i + 1].lastPos, points[i + 1].pos, timeStacker);
				}
				Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
				float width = this.vines[j].Rad(i);
				mesh.MoveVertice(i * 4, vector - a * width - camPos);
				mesh.MoveVertice(i * 4 + 1, vector + a * width - camPos);
				mesh.MoveVertice(i * 4 + 2, vector2 - a * width - camPos);
				mesh.MoveVertice(i * 4 + 3, vector2 + a * width - camPos);
				vector = vector2;
			}
		}

		for (int i = 0; i < this.leaves.Count; i++) {
			FSprite sprite = sLeaser.sprites[i + this.vinePObjs.Count];
			(Vector2, float) pos = this.OneDimensionalVinePos(this.leaves[i].vineIndex, this.leaves[i].pos, timeStacker);
			sprite.SetPosition(pos.Item1 + this.leaves[i].offsetPosition - camPos);
			sprite.rotation = pos.Item2 + this.leaves[i].rotation;
		}
	}

	public void BeingClimbedOn(CustomVineClimbable vine, Creature crit) {
		// TODO: Params
	}


	public int TotalPositions(CustomVineClimbable vine) {
		return this.vinePoints[vine.index].Length;
	}

	public Vector2 Pos(CustomVineClimbable vine, int index) {
		return this.vinePoints[vine.index][index].pos;
	}

	public void Push(CustomVineClimbable vine, int index, Vector2 movement) {
		this.vinePoints[vine.index][index].vel += movement;
	}



	public class Point {
		public Vector2 pos;
		public Vector2 lastPos;
		public Vector2 vel;
		public Vector2 acc;

		public Vector2? forcePosition;

		public int pointIndex = -1;
		public int vineIndex = -1;

		public Point(Vector2 pos) {
			this.pos = pos;
			this.lastPos = pos;
			this.vel = Vector2.zero;
			this.acc = Vector2.zero;
		}
	}

	public class Connection {
		public Point pointA;
		public Point pointB;
		public float dist;

		public Connection(Point a, Point b) {
			this.pointA = a;
			this.pointB = b;
			this.dist = (this.pointA.pos - this.pointB.pos).magnitude;
		}
	}

	public class VinePoint {
		public float pos;
		public int vineIndex;
		public float rotation = 0f;
		public Vector2 offsetPosition = Vector2.zero;
		public CustomVinePreset.Child child;

		public VinePoint(CustomVinePreset.Child child, float pos, int vineIndex) {
			this.pos = pos;
			this.vineIndex = vineIndex;
			this.child = child;
		}
	}

	public class CustomVineClimbable : UpdatableAndDeletable, IClimbableVine {
		public readonly CustomVineSystem owner;

		public int index;

		public CustomVinePreset preset;

		public CustomVineClimbable(CustomVineSystem owner, int index, string presetId) {
			this.owner = owner;
			this.index = index;
			this.preset = CustomVinePreset.GetPreset(presetId);
		}

		public void BeingClimbedOn(Creature crit) {
			this.owner.BeingClimbedOn(this, crit);
		}

		public bool CurrentlyClimbable() {
			return this.preset.climbable;
		}

		public bool JumpAllowed() {
			return this.preset.jumpable;
		}

		public Vector2 Pos(int index) {
			return this.owner.Pos(this, index);
		}

		public void Push(int index, Vector2 movement) {
			this.owner.Push(this, index, movement);
		}

		public int TotalPositions() {
			return this.owner.TotalPositions(this);
		}

		public float Mass(int index) {
			return (float) this.preset.mass.Evaluate(new ParserContext().SetVariable("index", (float) index).SetVariable("length", (float) this.owner.TotalPositions(this)));
		}

		public float Rad(int index) {
			return (float) this.preset.radius.Evaluate(new ParserContext().SetVariable("index", (float) index).SetVariable("length", (float) this.owner.TotalPositions(this)));
		}
	}
}