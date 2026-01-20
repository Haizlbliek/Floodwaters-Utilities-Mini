namespace Floodwaters.Objects;

public class CactusSpear : Spear {
	Color blackColor = Color.red;

	private bool stuck;
	private Vector2 stuckPos;
	private Vector2 stuckRot;

	public CactusSpear(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject, abstractPhysicalObject.world) {
	}

	public override void Update(bool eu) {
		base.Update(eu);

		base.firstChunk.collideWithTerrain = true;

		if (this.stuck) {
			if (this.grabbedBy.Count > 0) {
				this.stuck = false;
			}
			else {
				base.firstChunk.vel = Vector2.zero;
				base.firstChunk.pos = this.stuckPos;
				this.rotation = this.stuckRot;
				this.lastRotation = this.stuckRot;
			}
		}
		base.firstChunk.collideWithObjects = !this.stuck;
	}

	public void Stuck(Vector2 stuckPos, Vector2 stuckRot) {
		this.stuck = true;
		this.stuckPos = stuckPos;
		this.stuckRot = stuckRot;
	}

	public override void Grabbed(Creature.Grasp grasp) {
		this.stuck = false;
		if (grasp.grabber is Player) {
			for (int i = 0; i < this.room.game.cameras.Length; i++) {
				this.room.game.cameras[i].MoveObjectToContainer(base.graphicsModule, null);
			}
		}

		base.Grabbed(grasp);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("SmallSpear", true) {
			scaleY = 0.5f
		};

		this.AddToContainer(sLeaser, rCam, this.stuck ? rCam.ReturnFContainer("Background") : null);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
		base.ApplyPalette(sLeaser, rCam, palette);

		this.blackColor = palette.blackColor;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

		if (this.blink > 0 && Random.value < 0.5f) {
			sLeaser.sprites[0].color = this.blackColor;
		}
		else {
			sLeaser.sprites[0].color = Color.white;
		}
	}

	public override void ChangeMode(Mode newMode) {
		if (newMode == Mode.StuckInCreature || (newMode == Mode.StuckInWall && Random.value < 0.5f)) {
			this.room?.PlaySound(newMode == Mode.StuckInCreature ? SoundID.Spear_Stick_In_Creature : SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk.pos);
			for (int n = 40; n > 0; n--) {
				this.room.AddObject(new Spark(base.firstChunk.pos, Custom.RNV(), Color.white, null, 20, 50));
			}
			this.Destroy();
			return;
		}

		if (newMode == Mode.StuckInWall) {
			this.room?.PlaySound(SoundID.Spear_Bounce_Off_Wall, base.firstChunk.pos);
			this.vibrate = 20;
			this.SetRandomSpin();
			base.ChangeMode(Mode.Free);
			for (int n = 17; n > 0; n--) {
				this.room.AddObject(new Spark(base.firstChunk.pos, Custom.RNV(), Color.white, null, 10, 20));
			}
			return;
		}

		base.ChangeMode(newMode);
	}

	public class AbstractCactusSpear : AbstractSpear {
		public AbstractCactusSpear(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID) : base(world, realizedObject, pos, ID, false) {
			this.type = Enums.CactusSpear;
		}
	}
}