namespace Floodwaters.Objects;

public static class VerticalGateManager {
	public class VerticalGateData {
		public IntVector2 gateTile;
		public Vector2 gatePosition;
		public List<Vector2> origDoorPositions = [];
	}

	public static readonly ConditionalWeakTable<RegionGate, VerticalGateData> verticalGates = new ConditionalWeakTable<RegionGate, VerticalGateData>();

	public static void Initialize() {
		On.Room.Loaded += On_Room_Loaded;
		On.RegionGateGraphics.Update += On_RegionGateGraphics_Update;
		On.RegionGateGraphics.DrawSprites += On_RegionGateGraphics_DrawSprites;
		On.GateKarmaGlyph.InitiateSprites += On_GateKarmaGlyph_InitiateSprites;
		On.ElectricGate.Update += On_ElectricGate_Update;
		On.RegionGate.DetectZone += On_RegionGate_DetectZone;
		On.RegionGate.AllPlayersThroughToOtherSide += On_RegionGate_AllPlayersThroughToOtherSide;
		On.RegionGate.ChangeDoorStatus += On_RegionGate_ChangeDoorStatus;
		On.RegionGateGraphics.DoorGraphic.PansarPush += On_RegionGateGraphics_DoorGraphic_PansarPush;
	}

	public static void Cleanup() {
		On.Room.Loaded -= On_Room_Loaded;
		On.RegionGateGraphics.Update -= On_RegionGateGraphics_Update;
		On.RegionGateGraphics.DrawSprites -= On_RegionGateGraphics_DrawSprites;
		On.GateKarmaGlyph.InitiateSprites -= On_GateKarmaGlyph_InitiateSprites;
		On.ElectricGate.Update -= On_ElectricGate_Update;
		On.RegionGate.DetectZone -= On_RegionGate_DetectZone;
		On.RegionGate.AllPlayersThroughToOtherSide -= On_RegionGate_AllPlayersThroughToOtherSide;
		On.RegionGate.ChangeDoorStatus -= On_RegionGate_ChangeDoorStatus;
		On.RegionGateGraphics.DoorGraphic.PansarPush -= On_RegionGateGraphics_DoorGraphic_PansarPush;
	}

	private static void On_Room_Loaded(On.Room.orig_Loaded orig, Room self) {
		orig(self);

		if (!self.IsGateRoom())
			return;

		Plugin.Log("Looking for gate file");
		string filePath = AssetManager.ResolveFilePath(string.Concat([
			"world",
			Path.DirectorySeparatorChar.ToString(),
			"gates",
			Path.DirectorySeparatorChar.ToString(),
			"vgates.txt"
		]));
		if (!File.Exists(filePath))
			return;

		string[] array = File.ReadAllLines(filePath);
		if (!array.Contains(self.abstractRoom.name))
			return;

		Plugin.Log("<" + self.abstractRoom.name + "> I'm a vertical gate!");

		RegionGate gate = null;
		foreach (UpdatableAndDeletable obj in self.updateList) {
			if (obj is RegionGate regionGate) {
				gate = regionGate;
				break;
			}
		}

		if (gate == null) {
			Plugin.Log("<" + self.abstractRoom.name + "> But I can't find the gate object... :-(");
			return;
		}
		
		Plugin.Log("<" + self.abstractRoom.name + "> I found the gate!");

		VerticalGateData data = new VerticalGateData();
		
		foreach (PlacedObject po in self.roomSettings.placedObjects) {
			if (po.type == Enums.VerticalGatePositionPO) {
				Plugin.Log("<" + self.abstractRoom.name + "> Gate position SET!");
				data.gatePosition = self.MiddleOfTile(po.pos) + Vector2.up * 10f;
				data.gateTile = self.GetTilePosition(po.pos);
				break;
			}
		}

		foreach (GateKarmaGlyph glyph in gate.karmaGlyphs) {
			glyph.pos = data.gatePosition;
			glyph.pos.x -= 40f;
			glyph.pos.y += (glyph.side ? 4.5f : -4.5f) * 20f;
			glyph.lastPos = glyph.pos;
		}

		verticalGates.Add(gate, data);
	}

	private static void On_RegionGateGraphics_Update(On.RegionGateGraphics.orig_Update orig, RegionGateGraphics self) {
		if (!verticalGates.TryGetValue(self.gate, out VerticalGateData data)) {
			orig(self);
			return;
		}
		
		for (int i = 0; i < 3; i++) {
			RegionGateGraphics.DoorGraphic door = self.doorGraphs[i];
			if (data.origDoorPositions.Count > i) door.posZ = data.origDoorPositions[i];
			door.Update();
		}
	}

	private static void Door_DrawSprites(RegionGateGraphics.DoorGraphic door, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 origPosZ) {
		camPos.x += 0.25f;
		camPos.y += 0.25f;

		float globalOffsetX = 90f;

		for (int side = 0; side < 2; side++) {
			float centerTrackSlide = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(door.tracks[1, 1], door.tracks[1, 0], timeStacker)), 0.6f);

			sLeaser.sprites[door.CenterTrackSprite(side)].x = door.posZ.x + ((side == 0) ? (centerTrackSlide * 65f) : (-180f - centerTrackSlide * 130f)) - camPos.x + globalOffsetX;
			sLeaser.sprites[door.CenterTrackSprite(side)].y = door.posZ.y - camPos.y;
			sLeaser.sprites[door.CenterTrackSprite(side)].rotation = 90f;
			sLeaser.sprites[door.CenterTrackSprite(side)].alpha = 1f - Mathf.Lerp(1.5f, 2.5f, centerTrackSlide) / 30f;

			Vector2 centerOffset = new Vector2(door.posZ.x + ((side != 0) ? 40f : -220f), door.posZ.y);
			for (int track = 0; track < 2; track++) {
				centerTrackSlide = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(door.tracks[track * 2, 1], door.tracks[track * 2, 0], timeStacker)), 1.4f);
				sLeaser.sprites[door.TrackSprite(side, track)].rotation = 90f;
				sLeaser.sprites[door.TrackSprite(side, track)].x = door.posZ.x + ((side == 0) ? (centerTrackSlide * 130f) : (-180f - centerTrackSlide * 65f)) - camPos.x + globalOffsetX;
				sLeaser.sprites[door.TrackSprite(side, track)].y = door.posZ.y + ((track == 0) ? -9f : 9f) - camPos.y;
				sLeaser.sprites[door.TrackSprite(side, track)].alpha = 1f - Mathf.Lerp(1.5f, 2.5f, centerTrackSlide) / 30f;

				float blockLerp = Mathf.Lerp(door.blocks[side, track, 1], door.blocks[side, track, 0], timeStacker);
				Vector2 blockPos = door.posZ;
				Vector2 blockOffset = Custom.DegToVec(30f + blockLerp * 150f);
				blockOffset.y += 1f;
				blockOffset.x *= 20f * (-1 + 2 * track) * Mathf.Lerp(blockLerp, 1f, 0.5f);
				blockOffset.y *= 30f * (-1 + 2 * side);
				blockOffset.y += 60f * Mathf.InverseLerp(0.2f, 0f, blockLerp) * (-1 + 2 * side);
				blockPos += blockOffset;
				if (side == 0) {
					blockPos.y -= 90f;
				}
				float blockRot = Mathf.Pow(Mathf.Sin(3.1415927f * blockLerp), 3f) * -2f * (-1 + 2 * track) * (-1 + 2 * side);

				sLeaser.sprites[door.BlockSprite(side, track)].x = door.posZ.x + (blockPos.y - door.posZ.y) - camPos.x + globalOffsetX;
				sLeaser.sprites[door.BlockSprite(side, track)].y = door.posZ.y - (blockPos.x - door.posZ.x) - camPos.y;
				sLeaser.sprites[door.BlockSprite(side, track)].alpha = 1f - Mathf.Lerp(3f, 2f, Mathf.Pow(blockLerp, 7f)) / 30f;
				sLeaser.sprites[door.BlockSprite(side, track)].rotation = blockRot + 90f;

				float armLift = Mathf.Lerp(0f, 100f * ((side == 0) ? -1f : 1f), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(door.arms[side, track, 1], door.arms[side, track, 0], timeStacker)), 0.8f));
				float handYOffset = Mathf.Lerp(-10f, -80f, (side == 0) ? 0f : 1f);
				Vector2 handPos = blockPos + Custom.RotateAroundOrigo(new Vector2(22f * (-1 + 2 * track), handYOffset), blockRot);
				handPos.y += armLift;
				Vector2 armEndPos = door.posZ + new Vector2(
					(-1 + 2 * track) * Mathf.Lerp(30f, -35f, Mathf.Pow(0.5f * blockLerp + 0.5f * Mathf.Sin(blockLerp * 3.1415927f), 1f + 4f * blockLerp)),
					(side == 1) ? 60f : -240f
				);
				armEndPos.y += armLift;
				float collisionAdjust = Custom.CirclesCollisionTime(handPos.x, handPos.y, centerOffset.x, centerOffset.y, armEndPos.x - handPos.x, armEndPos.y - handPos.y, 1f, 28f);
				if (collisionAdjust > 0f && collisionAdjust < 1f) {
					armEndPos = Vector2.Lerp(handPos, armEndPos, collisionAdjust);
				}
				handPos = new Vector2(
					door.posZ.x + (handPos.y - door.posZ.y),
					door.posZ.y + (handPos.x - door.posZ.x)
				);
				armEndPos = new Vector2(
					door.posZ.x + (armEndPos.y - door.posZ.y),
					door.posZ.y + (armEndPos.x - door.posZ.x)
				);
				sLeaser.sprites[door.ArmSprite(side, track)].x = handPos.x - camPos.x + globalOffsetX;
				sLeaser.sprites[door.ArmSprite(side, track)].y = handPos.y - camPos.y;
				sLeaser.sprites[door.ArmSprite(side, track)].rotation = Custom.AimFromOneVectorToAnother(handPos, armEndPos);
				sLeaser.sprites[door.ArmSprite(side, track)].scaleY = Vector2.Distance(handPos, armEndPos);
				sLeaser.sprites[door.HandSprite(side, track)].x = handPos.x - camPos.x + globalOffsetX;
				sLeaser.sprites[door.HandSprite(side, track)].y = handPos.y - camPos.y;
				sLeaser.sprites[door.HandSprite(side, track)].rotation = blockRot - 90f;
			}
			
			// Clamps
			for (int clamp = 0; clamp < 9; clamp++) {
				Vector2 clampPos = new Vector2(
					Mathf.Lerp(door.clamps[side, clamp].lastPos.x, door.clamps[side, clamp].pos.x, timeStacker),
					Mathf.Lerp(door.clamps[side, clamp].lastPos.y, door.clamps[side, clamp].pos.y, timeStacker)
				);
				sLeaser.sprites[door.ClampSprite(side, clamp)].x = door.posZ.x + (clampPos.y - origPosZ.y) - camPos.x + globalOffsetX;
				sLeaser.sprites[door.ClampSprite(side, clamp)].y = door.posZ.y + (clampPos.x - origPosZ.x) - camPos.y;
				sLeaser.sprites[door.ClampSprite(side, clamp)].rotation = 90f - Mathf.Lerp(door.clamps[side, clamp].lastRotat, door.clamps[side, clamp].rotat, timeStacker);
				sLeaser.sprites[door.ClampSprite(side, clamp)].alpha = 1f - door.clamps[side, clamp].depth;
			}

			// Pasnars
			float pansarProgress = Mathf.Lerp(door.lastPC, door.PC, timeStacker);
			Vector2 pansarPos = door.posZ;
			pansarPos.y += ((side != 0) ? -1 : 1f) * 7f * pansarProgress;
			pansarPos.y += Mathf.Sin(pansarProgress * 3.1415927f) * 25f * ((side != 0) ? -1f : 1f);
			pansarPos.x -= 90f;
			sLeaser.sprites[door.PansarSprite(side)].x = pansarPos.x - camPos.x + globalOffsetX;
			sLeaser.sprites[door.PansarSprite(side)].y = pansarPos.y - camPos.y;
			sLeaser.sprites[door.PansarSprite(side)].rotation = 90f;
			sLeaser.sprites[door.PansarSprite(side)].isVisible = pansarProgress > 0.5f;
			sLeaser.sprites[door.PansarSprite(side)].alpha = Mathf.Lerp(0.7f, 1f, pansarProgress);
			sLeaser.sprites[door.BehindPansarSprite(side)].x = pansarPos.x - camPos.x + globalOffsetX;
			sLeaser.sprites[door.BehindPansarSprite(side)].y = pansarPos.y - camPos.y;
			sLeaser.sprites[door.BehindPansarSprite(side)].rotation = 90f;
			sLeaser.sprites[door.BehindPansarSprite(side)].alpha = Mathf.Lerp(0.7f, 1f, pansarProgress);
			sLeaser.sprites[door.BigScrewSprite(side)].x = centerOffset.x - camPos.x + globalOffsetX;
			sLeaser.sprites[door.BigScrewSprite(side)].y = centerOffset.y - camPos.y;
			sLeaser.sprites[door.BigScrewSprite(side)].rotation = Mathf.Lerp(door.bigScrews[side, 1], door.bigScrews[side, 0], timeStacker) + 90f;

			for (int cogSide = 0; cogSide < 2; cogSide++) {
				for (int cogIndex = 0; cogIndex < 2; cogIndex++) {
					Vector2 cogPos = new Vector2(
						((cogSide == 0) ? -1f : 1f) * ((cogIndex == 0) ? 40f : 50f) * ((side == 0) ? 1f : 0.8f),
						-(90f + ((side == 0) ? -1f : 1f) * ((cogIndex == 0) ? 150f : 175f) * ((side == 0) ? 1f : 1.2f))
					);
					sLeaser.sprites[door.CogSprite(side, cogSide, cogIndex)].x = door.posZ.x + cogPos.y - camPos.x + globalOffsetX;
					sLeaser.sprites[door.CogSprite(side, cogSide, cogIndex)].y = door.posZ.y + cogPos.x - camPos.y;
					sLeaser.sprites[door.CogSprite(side, cogSide, cogIndex)].rotation = ((cogSide == 0) ? -1f : 1f) * ((side == 0) ? 1f : -1f) * (door.GearsTurned * 0.5f + 0.5f * Mathf.Sin(door.GearsTurned * 3.1415927f)) * ((cogIndex == 0) ? 90f : 210f);
				}
			}
		}

		for (int pansarSegment = 0; pansarSegment < 9; pansarSegment++) {
			sLeaser.sprites[door.PansarSegmentSprite(pansarSegment)].x = door.posZ.x - 180f * Mathf.Lerp(door.pansarLocks[pansarSegment, 1], door.pansarLocks[pansarSegment, 0], timeStacker) - camPos.x + globalOffsetX;
			sLeaser.sprites[door.PansarSegmentSprite(pansarSegment)].y = door.posZ.y - camPos.y;
			sLeaser.sprites[door.PansarSegmentSprite(pansarSegment)].rotation = 90f;
			sLeaser.sprites[door.PansarSegmentSprite(pansarSegment)].alpha = Mathf.Lerp(1f, 0.8f, Mathf.InverseLerp(0.3f, -0.2f, Mathf.Lerp(door.pansarLocks[pansarSegment, 1], door.pansarLocks[pansarSegment, 0], timeStacker)));

			if (pansarSegment % 2 == 1) {
				sLeaser.sprites[door.PansarSegmentSprite(pansarSegment)].rotation = 90f * Mathf.Lerp(door.pansarLocks[pansarSegment, 3], door.pansarLocks[pansarSegment, 2], timeStacker) * ((pansarSegment / 2 % 2 == 0 == door.flip) ? -1f : 1f) + 90f;
			}
		}

		for (int pole = 0; pole < 4; pole++) {
			if (door.Closed == 1f || (door.poles[pole, 1] == 1f && door.poles[pole, 0] == 1f)) {
				sLeaser.sprites[door.PoleSprite(pole)].isVisible = false;
			}
			else {
				sLeaser.sprites[door.PoleSprite(pole)].isVisible = true;
				float poleDirection = (pole % 2 == 0 == door.flip) ? -1f : 1f;
				Vector2 poleOffset = new Vector2(
					(((pole > 0 && pole < 3) ? 14f : 11f) + ((pole < 2) ? 1f : 0f)) * (pole - 1.5f),
					-90f + poleDirection * 200f * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(door.poles[pole, 1], door.poles[pole, 0], timeStacker)), 1.4f)
				);
				sLeaser.sprites[door.PoleSprite(pole)].x = door.posZ.x + poleOffset.y - camPos.x + globalOffsetX;
				sLeaser.sprites[door.PoleSprite(pole)].y = door.posZ.y + poleOffset.x - camPos.y;
				sLeaser.sprites[door.PoleSprite(pole)].rotation = 90f;
			}
		}

		for (int bolt = 0; bolt < 4; bolt++) {
			sLeaser.sprites[door.BoltSprite(bolt)].x = door.posZ.x - 30f - 40f * bolt - camPos.x + globalOffsetX;
			sLeaser.sprites[door.BoltSprite(bolt)].y = door.posZ.y - camPos.y;
			sLeaser.sprites[door.BoltSprite(bolt)].rotation = 90f;
			sLeaser.sprites[door.BoltSprite(bolt)].isVisible = door.boltsBolted[bolt];
		}
	}

	private static void On_RegionGateGraphics_DrawSprites(On.RegionGateGraphics.orig_DrawSprites orig, RegionGateGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		if (!verticalGates.TryGetValue(self.gate, out VerticalGateData data)) {
			orig(self, sLeaser, rCam, timeStacker, camPos);
			return;
		}

		for (int i = 0; i < 3; i++) {
			RegionGateGraphics.DoorGraphic door = self.doorGraphs[i];
			if (data.origDoorPositions.Count <= i) data.origDoorPositions.Add(door.posZ);

			door.posZ = new Vector2(data.gatePosition.x, data.gatePosition.y + (-9f + 9f * door.door.number) * 20f);
			Door_DrawSprites(door, sLeaser, rCam, timeStacker, camPos, data.origDoorPositions[i]);
		}
	}

	private static void On_GateKarmaGlyph_InitiateSprites(On.GateKarmaGlyph.orig_InitiateSprites orig, GateKarmaGlyph self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		if (!verticalGates.TryGetValue(self.gate, out VerticalGateData data)) {
			orig(self, sLeaser, rCam);
			return;
		}

		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White", true) {
			shader = rCam.game.rainWorld.Shaders["LightSource"]
		};
		sLeaser.sprites[1] = new FSprite("pixel", true) {
			shader = rCam.game.rainWorld.Shaders["GateHologram"],
			anchorY = 0.75f,
			rotation = -90f
		};
		self.symbolDirty = true;
		self.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	private static void On_ElectricGate_Update(On.ElectricGate.orig_Update orig, ElectricGate self, bool eu) {
		orig(self, eu);
		if (!verticalGates.TryGetValue(self, out VerticalGateData data)) return;

		for (int i = 0; i < 4; i++) {
			bool topHalf = i < 2;
			bool leftSide = (i % 2 == 0) ^ topHalf;
	
			Vector2 position = new Vector2(
				10f + (topHalf ? -3f : 3f) * 20f + data.gatePosition.x,
				10f + (leftSide ? -4.5f : 4.5f) * 20f + data.gatePosition.y
			);
	
			for (int j = 0; j < 2; j++) {
				self.lamps[i, j].HardSetPos(position);
			}
		}
	}

	private static int On_RegionGate_DetectZone(On.RegionGate.orig_DetectZone orig, RegionGate self, AbstractCreature crit) {
		if (!verticalGates.TryGetValue(self, out VerticalGateData data)) return orig(self, crit);

		if (crit.pos.room != self.room.abstractRoom.index) {
			return -1;
		}

		if (crit.pos.y < data.gateTile.y - 8) {
			return 0;
		}
		if (crit.pos.y < data.gateTile.y) {
			return 1;
		}
		if (crit.pos.y < data.gateTile.y + 8) {
			return 2;
		}
		return 3;
	}

	private static bool On_RegionGate_AllPlayersThroughToOtherSide(On.RegionGate.orig_AllPlayersThroughToOtherSide orig, RegionGate self) {
		if (!verticalGates.TryGetValue(self, out VerticalGateData data)) return orig(self);

		foreach (AbstractCreature player in self.room.game.Players) {
			if (player.pos.room != self.room.abstractRoom.index) continue;

			if (self.letThroughDir) {
				if (player.pos.y < data.gateTile.y + 3) {
					return false;
				}
			}
			else {
				if (player.pos.y > data.gateTile.y - 3) {
					return false;
				}
			}
		}
		return true;
	}

	private static void On_RegionGate_ChangeDoorStatus(On.RegionGate.orig_ChangeDoorStatus orig, RegionGate self, int door, bool open) {
		if (!verticalGates.TryGetValue(self, out VerticalGateData data)) {
			orig(self, door, open);
			return;
		}

		int left = data.gateTile.x - 4;
		int bottom = data.gateTile.y + (door - 1) * 9;
		for (int i = 0; i < 2; i++) {
			for (int j = left; j <= left + 8; j++) {
				self.room.GetTile(j, bottom + i).Terrain = open ? Room.Tile.TerrainType.Air : Room.Tile.TerrainType.Solid;
			}
		}
	}

	private static void On_RegionGateGraphics_DoorGraphic_PansarPush(On.RegionGateGraphics.DoorGraphic.orig_PansarPush orig, RegionGateGraphics.DoorGraphic self) {
		if (!verticalGates.TryGetValue(self.door.gate, out VerticalGateData data)) {
			orig(self);
			return;
		}

		self.posZ = new Vector2(data.gatePosition.x, data.gatePosition.y + (-9f + 9f * self.door.number) * 20f);

		float width = 21f;
		width += 7f * self.PC;
		width += Mathf.Sin(self.PC * 3.1415927f) * 25f;
		Room room = self.rgGraphics.gate.room;

		for (int i = 0; i < room.physicalObjects.Length; i++) {
			for (int j = 0; j < room.physicalObjects[i].Count; j++) {
				for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++) {
					BodyChunk chunk = room.physicalObjects[i][j].bodyChunks[k];

					if (chunk.pos.y > self.posZ.y - width - chunk.rad && chunk.pos.y < self.posZ.y) {
						chunk.pos.y = self.posZ.y - width - chunk.rad;
						chunk.vel.y *= 0.2f;
					}
					else if (chunk.pos.y < self.posZ.y + width + chunk.rad && chunk.pos.y > self.posZ.y) {
						chunk.pos.y = self.posZ.y + width + chunk.rad;
					}
				}
			}
		}
	}
}