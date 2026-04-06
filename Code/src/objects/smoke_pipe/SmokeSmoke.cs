namespace Floodwaters.Objects;

public class SmokeSmoke : SmokeSystem {
	public class SmokeParticle : SpriteSmoke {
		public float upForce;
		public float moveDir;
		public Vector2 forceDir;
		public FloatRect confines;

		public float intensity;

		public override float ToMidSpeed => 0.4f;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime) {
			this.forceDir = vel;
			base.Reset(newOwner, pos, vel, lifeTime);
			this.upForce = UnityEngine.Random.value * 100f / lifeTime;
			this.moveDir = UnityEngine.Random.value * 360f;
		}

		public override void Update(bool eu) {
			base.Update(eu);
			if (!this.resting) {
				this.moveDir += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 50f;
				this.vel *= 0.8f;
				this.vel += this.forceDir;
				this.vel += Custom.DegToVec(this.moveDir) * 0.1f * (1.8f * this.intensity * this.life);
				this.vel.y += 2.8f * this.intensity * this.upForce * 0.1f;
				if (this.room.PointSubmerged(this.pos)) {
					this.pos.y = this.room.FloatWaterLevel(this.pos);
				}

				if (this.pos.x < this.confines.left) {
					this.pos.x = this.confines.left;
				}
				else if (this.pos.x > this.confines.right) {
					this.pos.x = this.confines.right;
				}

				if (this.pos.y < this.confines.bottom) {
					this.pos.y = this.confines.bottom;
				}
				else if (this.pos.y > this.confines.top) {
					this.pos.y = this.confines.top;
				}
			}
		}

		public override float Rad(int type, float useLife, float useStretched, float timeStacker) {
			float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(Mathf.Sin(useLife * (float) Math.PI), 1f - useLife, 0.7f)), 0.8f);
			return type switch {
				0 => Mathf.Lerp(4f, this.rad, num + useStretched),
				1 => 1.5f * Mathf.Lerp(2f, this.rad, num),
				_ => Mathf.Lerp(4f, this.rad, num),
			} * 0.5f;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
			base.InitiateSprites(sLeaser, rCam);
			for (int i = 0; i < 2; i++) {
				sLeaser.sprites[i].shader = this.room.game.rainWorld.Shaders["Steam"];
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!this.resting) {
				for (int i = 0; i < 2; i++) {
					sLeaser.sprites[i].alpha = this.life;
				}
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
			base.ApplyPalette(sLeaser, rCam, palette);
			for (int i = 0; i < 2; i++) {
				sLeaser.sprites[i].color = palette.blackColor;//Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), Mathf.Lerp(0.03f, 0.35f, palette.texture.GetPixel(30, 7).r));
			}
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
			newContatiner = rCam.ReturnFContainer("Water");
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public SmokeSmoke(Room room) : base(SmokeType.Steam, room, 2, 0f) {
	}

	public override SmokeSystemParticle CreateParticle() {
		return new SmokeParticle();
	}

	public void EmitSmoke(Vector2 pos, Vector2 vel, FloatRect confines, float intensity) {
		if (this.AddParticle(pos, vel, Mathf.Lerp(60f, 180f, UnityEngine.Random.value * intensity)) is SmokeParticle smokeParticle) {
			smokeParticle.confines = confines;
			smokeParticle.intensity = intensity;
			smokeParticle.rad = Mathf.Lerp(108f, 286f, UnityEngine.Random.value) * Mathf.Lerp(0.5f, 1f, intensity);
		}
	}
}