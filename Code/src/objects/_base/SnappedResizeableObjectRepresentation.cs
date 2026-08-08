public class SnappedResizeableObjectRepresentation : PlacedObjectRepresentation {
	public bool showRing;

	public SnappedResizeableObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name, bool showRing)
		: base(owner, IDstring, parentNode, pObj, name) {
		this.showRing = showRing;
		this.subNodes.Add(new SnappedHandle(owner, "Rad_Handle", this, (pObj.data as PlacedObject.ResizableObjectData).handlePos));
		if (showRing) {
			this.fSprites.Add(new FSprite("Futile_White"));
			owner.placedObjectsContainer.AddChild(this.fSprites[1]);
			this.fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		}

		this.fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(this.fSprites[(!showRing) ? 1 : 2]);
		this.fSprites[(!showRing) ? 1 : 2].anchorY = 0f;
	}

	public override void Refresh() {
		base.Refresh();
		this.MoveSprite(1, this.absPos);
		if (this.showRing) {
			this.fSprites[1].scale = (this.subNodes[0] as Handle).pos.magnitude / 8f;
			this.fSprites[1].alpha = 2f / (this.subNodes[0] as Handle).pos.magnitude;
		}

		this.MoveSprite(2, this.absPos);
		this.fSprites[(!this.showRing) ? 1 : 2].scaleY = (this.subNodes[0] as Handle).pos.magnitude;
		this.fSprites[(!this.showRing) ? 1 : 2].rotation = Custom.AimFromOneVectorToAnother(this.absPos, (this.subNodes[0] as Handle).absPos);
		(this.pObj.data as PlacedObject.ResizableObjectData).handlePos = (this.subNodes[0] as Handle).pos;
	}

	public class SnappedHandle : Handle {
		public SnappedHandle(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos) {
		}

		public override void Move(Vector2 newPos) {
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				newPos = Snap45(newPos);
			}

			base.Move(newPos);
		}

		private static Vector2 Snap45(Vector2 v) {
			float magnitude = v.magnitude;

			if (magnitude == 0f) {
				return Vector2.zero;
			}

			float angle = Mathf.Atan2(v.y, v.x);
			float snappedAngle = Mathf.Round(angle / (Mathf.PI / 4f)) * (Mathf.PI / 4f);

			return new Vector2(
				Mathf.Cos(snappedAngle),
				Mathf.Sin(snappedAngle)
			) * magnitude;
		}
	}
}