using DevInterface;
using RWCustom;
using Smoke;
using UnityEngine;

namespace Floodwaters.Objects;

public class MagmaArea : UpdatableAndDeletable {
	public readonly PlacedObject pObj;
	public LightSource light1;
	public LightSource light2;
	public StaticSoundLoop steam;
	public FireSmoke smoke;
	public bool burntLast;
	public float heat;
	public float heatWait;

	MagmaAreaData Data => this.pObj.data as MagmaAreaData;
	float Rad => this.Data.Rad;

	public MagmaArea(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
		this.steam = new StaticSoundLoop(SoundID.Gate_Water_Steam_LOOP, this.pObj.pos, this.room, 1f, 1f);
		this.smoke = new FireSmoke(this.room);
	}

	public void Burn(Creature creature, BodyChunk chunk, bool skipHeat) {
		if (!skipHeat) {
			this.heat += (this.burntLast ? 0.04f : 0.2f) / this.Data.burnTime;
			this.heat = Mathf.Min(this.heat, 1.0f);
		}

		if (this.heat >= 0.95f) {
			creature.Violence(null, null, chunk, null, Enums.BurnDamageType, 1.0f, 0.0f);
		}
		if (Random.value < this.heat * 4.0f) {
			this.smoke.EmitSmoke(chunk.pos, Custom.DegToVec(Random.Range(45f, 135f)), this.room.game.cameras[0].currentPalette.blackColor, 20);
		}
	}

	public override void Update(bool eu) {
		bool heated = false;

		foreach (PhysicalObject physicalObject in this.room.physicalObjects[1]) {
			if (physicalObject is not Creature creature) continue;
			bool alreadyBurnt = false;

			foreach (BodyChunk chunk in creature.bodyChunks) {
				if ((chunk.pos - this.pObj.pos).sqrMagnitude > this.Rad * this.Rad) continue;

				if (creature is Player player) {
					player.aerobicLevel = Mathf.Min(player.aerobicLevel + this.heat * 2f, 1f);
					player.exhausted = player.aerobicLevel >= 1f;
				}

				if (chunk.contactPoint.x == 0 && chunk.contactPoint.y == 0) continue;

				this.Burn(creature, chunk, alreadyBurnt);
				this.heatWait = 20f;
				heated = true;
				alreadyBurnt = true;
			}
		}

		this.burntLast = heated;

		if (!heated) {
			this.heatWait--;
			if (this.heatWait < 0f) {
				this.heat -= 0.03f / this.Data.burnTime;
				this.heat = Mathf.Max(this.heat, 0.0f);
			}
		}

		if (this.light1 == null && !this.slatedForDeletetion) {
			this.light1 = new LightSource(this.pObj.pos, true, new Color(1f, 0.2f, 0f), null);
			this.room.AddObject(this.light1);
			this.light2 = new LightSource(this.pObj.pos, true, new Color(0.8f, 0.6f, 0f), null);
			this.room.AddObject(this.light2);
		}

		if (this.light1 != null && this.slatedForDeletetion) {
			this.light1.room.RemoveObject(this.light1);
			this.light1.Destroy();
			this.light1 = null;
			this.light2.room.RemoveObject(this.light2);
			this.light2.Destroy();
			this.light2 = null;
		}

		if (this.light1 != null) {
			this.light1.setPos = this.pObj.pos;
			this.light1.setRad = Mathf.Lerp(1.25f, 2f, this.heat) * this.Rad;
			this.light1.setAlpha = this.heat * 1.5f;
			this.light1.color = this.Data.colorA;

			this.light2.setPos = this.pObj.pos + Vector2.up * 2f;
			this.light2.setRad = Mathf.Lerp(1f, 1.75f, this.heat) * this.Rad;
			this.light2.setAlpha = this.heat * 1.75f;
			this.light2.color = this.Data.colorB;
		}

		this.steam.volume = this.heat * 0.7f;
		this.steam.Update();
	}


	public class MagmaAreaData : PlacedObject.ResizableObjectData {
		public float burnTime = 16f;
		public Vector2 panelPos;
		public Color colorA = new Color(1f, 0.2f, 0f);
		public Color colorB = new Color(0.8f, 0.6f, 0f);

		public MagmaAreaData(PlacedObject owner) : base(owner) {
		}

		public override void FromString(string s) {
			base.FromString(s);

			try {
				string[] array = Regex.Split(s, "~");
				this.burnTime = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.x = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.y = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.colorA.r = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.colorA.g = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.colorA.b = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.colorB.r = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.colorB.g = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.colorB.b = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 11);
			} catch (Exception) {}
		}

		public override string ToString() {
			string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}",
				this.burnTime,
				this.panelPos.x,
				this.panelPos.y,
				this.colorA.r,
				this.colorA.g,
				this.colorA.b,
				this.colorB.r,
				this.colorB.g,
				this.colorB.b
			);
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}
	}

	public class MagmaAreaRepresentation : ResizeableObjectRepresentation {
		public MagmaAreaRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name, true) {
			this.subNodes.Add(new MagmaAreaPanel(owner, "Lillypad_Panel", this, (pObj.data as MagmaAreaData).panelPos));
			this.fSprites.Add(new FSprite("pixel", true) {
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(this.fSprites[this.fSprites.Count - 1]);
		}

		public override void Refresh() {
			base.Refresh();

			MagmaAreaData data = this.pObj.data as MagmaAreaData;
			MagmaAreaPanel panel = this.subNodes[this.subNodes.Count - 1] as MagmaAreaPanel;
			base.MoveSprite(this.fSprites.Count - 1, this.absPos);
			this.fSprites[this.fSprites.Count - 1].scaleY = panel.pos.magnitude;
			this.fSprites[this.fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(this.absPos, panel.nonCollapsedAbsPos);
			data.panelPos = panel.pos;
		}

		public class MagmaAreaPanel : Panel {
			public MagmaAreaPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 145f), "Magma Area") {
				this.subNodes.Add(new MagmaAreaSlider(owner, "Burn_Time_Slider", this, new Vector2(5f, 125f), "Burn Time: "));
				this.subNodes.Add(new MagmaAreaSlider(owner, "ColorA_R_Slider", this, new Vector2(5f, 105f), "Color A R: "));
				this.subNodes.Add(new MagmaAreaSlider(owner, "ColorA_G_Slider", this, new Vector2(5f, 85f), "Color A G: "));
				this.subNodes.Add(new MagmaAreaSlider(owner, "ColorA_B_Slider", this, new Vector2(5f, 65f), "Color A B: "));
				this.subNodes.Add(new MagmaAreaSlider(owner, "ColorB_R_Slider", this, new Vector2(5f, 45f), "Color B R: "));
				this.subNodes.Add(new MagmaAreaSlider(owner, "ColorB_G_Slider", this, new Vector2(5f, 25f), "Color B G: "));
				this.subNodes.Add(new MagmaAreaSlider(owner, "ColorB_B_Slider", this, new Vector2(5f, 5f), "Color B B: "));
			}

			public class MagmaAreaSlider : Slider {
				public MagmaAreaSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
				}

				public override void Refresh() {
					base.Refresh();
					MagmaAreaData data = (this.parentNode.parentNode as MagmaAreaRepresentation).pObj.data as MagmaAreaData;

					switch (this.IDstring) {
						case "Burn_Time_Slider": {
							base.NumberText = data.burnTime.ToString();
							base.RefreshNubPos(Mathf.InverseLerp(0.1f, 32f, data.burnTime));
							break;
						}

						case "ColorA_R_Slider": {
							base.NumberText = Mathf.FloorToInt(data.colorA.r * 255f).ToString();
							base.RefreshNubPos(data.colorA.r);
							break;
						}

						case "ColorA_G_Slider": {
							base.NumberText = Mathf.FloorToInt(data.colorA.g * 255f).ToString();
							base.RefreshNubPos(data.colorA.g);
							break;
						}

						case "ColorA_B_Slider": {
							base.NumberText = Mathf.FloorToInt(data.colorA.b * 255f).ToString();
							base.RefreshNubPos(data.colorA.b);
							break;
						}

						case "ColorB_R_Slider": {
							base.NumberText = Mathf.FloorToInt(data.colorB.r * 255f).ToString();
							base.RefreshNubPos(data.colorB.r);
							break;
						}

						case "ColorB_G_Slider": {
							base.NumberText = Mathf.FloorToInt(data.colorB.g * 255f).ToString();
							base.RefreshNubPos(data.colorB.g);
							break;
						}

						case "ColorB_B_Slider": {
							base.NumberText = Mathf.FloorToInt(data.colorB.b * 255f).ToString();
							base.RefreshNubPos(data.colorB.b);
							break;
						}

						default: {
							base.NumberText = "0";
							base.RefreshNubPos(0f);
							break;
						}
					}
				}

				public override void NubDragged(float nubPos) {
					MagmaAreaData data = (this.parentNode.parentNode as MagmaAreaRepresentation).pObj.data as MagmaAreaData;

					switch (this.IDstring) {
						case "Burn_Time_Slider": {
							data.burnTime = Mathf.Lerp(0.1f, 32f, nubPos);
							break;
						}

						case "ColorA_R_Slider": {
							data.colorA.r = nubPos;
							break;
						}

						case "ColorA_G_Slider": {
							data.colorA.g = nubPos;
							break;
						}

						case "ColorA_B_Slider": {
							data.colorA.b = nubPos;
							break;
						}

						case "ColorB_R_Slider": {
							data.colorB.r = nubPos;
							break;
						}

						case "ColorB_G_Slider": {
							data.colorB.g = nubPos;
							break;
						}

						case "ColorB_B_Slider": {
							data.colorB.b = nubPos;
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