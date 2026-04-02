namespace Floodwaters.Objects;

public static class Objects {
	public static void Initialize() {
		On.AbstractPhysicalObject.Realize += On_AbstractPhysicalObject_Realize;
		On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += On_ObjectsPage_DevObjectGetCategoryFromPlacedType;
		On.DevInterface.ObjectsPage.CreateObjRep += On_ObjectsPage_CreateObjRep;
		On.PlacedObject.GenerateEmptyData += On_PlacedObject_GenerateEmptyData;
		On.Room.Loaded += On_Room_Loaded;
		On.SlugcatStats.NourishmentOfObjectEaten += On_SlugcatStats_NourishmentOfObjectEaten;
		On.Player.Grabability += On_Player_Grabability;
		On.Player.CanBeSwallowed += On_Player_CanBeSwallowed;
		On.SaveState.AbstractPhysicalObjectFromString += On_SaveState_AbstractPhysicalObjectFromString;

		On.CoralBrain.CoralNeuronSystem.AIMapReady += On_CoralNeuronSystem_AIMapReady;
		On.Player.Regurgitate += On_Player_Regurgitate;
		On.CoralBrain.CoralNeuron.DrawSprites += On_CoralNeuron_DrawSprites;
		IL.Player.UpdateAnimation += IL_Player_UpdateAnimation;
		On.Player.UpdateAnimation += On_Player_UpdateAnimation;
		On.Player.MovementUpdate += On_Player_MovementUpdate;
		On.RainWorldGame.RestartGame += On_RainWorldGame_RestartGame;
		On.LightSource.InitiateSprites += On_LightSource_InitiateSprites;
		On.LightSource.DrawSprites += On_LightSource_DrawSprites;

		VerticalGateManager.Initialize();

		Futile.atlasManager.LoadAtlas("atlases/Floodwaters-Mini");
	}

	public static void Cleanup() {
		On.AbstractPhysicalObject.Realize -= On_AbstractPhysicalObject_Realize;
		On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType -= On_ObjectsPage_DevObjectGetCategoryFromPlacedType;
		On.DevInterface.ObjectsPage.CreateObjRep -= On_ObjectsPage_CreateObjRep;
		On.PlacedObject.GenerateEmptyData -= On_PlacedObject_GenerateEmptyData;
		On.Room.Loaded -= On_Room_Loaded;
		On.SlugcatStats.NourishmentOfObjectEaten -= On_SlugcatStats_NourishmentOfObjectEaten;
		On.Player.Grabability -= On_Player_Grabability;
		On.Player.CanBeSwallowed -= On_Player_CanBeSwallowed;

		On.CoralBrain.CoralNeuronSystem.AIMapReady -= On_CoralNeuronSystem_AIMapReady;
		On.Player.Regurgitate -= On_Player_Regurgitate;
		On.CoralBrain.CoralNeuron.DrawSprites -= On_CoralNeuron_DrawSprites;
		IL.Player.UpdateAnimation -= IL_Player_UpdateAnimation;
		On.Player.UpdateAnimation -= On_Player_UpdateAnimation;
		On.Player.MovementUpdate -= On_Player_MovementUpdate;
		On.RainWorldGame.RestartGame -= On_RainWorldGame_RestartGame;
		On.LightSource.InitiateSprites -= On_LightSource_InitiateSprites;
		On.LightSource.DrawSprites -= On_LightSource_DrawSprites;

		VerticalGateManager.Cleanup();

		Futile.atlasManager.UnloadAtlas("atlases/Floodwaters-Mini");
	}

	private static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj) {
		if (obj is CactusFruit)
			return Player.ObjectGrabability.OneHand;

		if (obj is CactusSpear)
			return Player.ObjectGrabability.OneHand;

		if (obj is Cattail cattail)
			return cattail.stick != null ? Player.ObjectGrabability.TwoHands : Player.ObjectGrabability.OneHand;

		if (obj is ColoredLantern lantern && lantern.stick == null)
			return Player.ObjectGrabability.OneHand;

		return orig(self, obj);
	}

	private static bool On_Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj) {
		if (testObj is Cattail cattail)
			return cattail.stick == null;

		return orig(self, testObj);
	}

	private static int On_SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject) {
		if (eatenobject is CactusFruit) {
			return 2;
		}

		return orig(slugcatIndex, eatenobject);
	}

	private static void On_PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self) {
		orig(self);

		if (self.type == Enums.CactusPO) {
			self.data = new CactusData(self);
			return;
		}

		if (self.type == Enums.SandDripPO) {
			self.data = new SandDripData(self);
			return;
		}

		if (self.type == Enums.DeerSkullPO) {
			self.data = new DeerSkull.DeerSkullData(self);
			return;
		}

		if (self.type == Enums.CattailPO) {
			self.data = new Cattail.CattailData(self);
			return;
		}

		if (self.type == Enums.ColoredCattailPO) {
			self.data = new Cattail.CattailData(self);
			return;
		}

		if (self.type == Enums.BubbleEmitterPO) {
			self.data = new BubbleEmitter.BubbleEmitterData(self);
			return;
		}

		if (self.type == Enums.BambooPO) {
			self.data = new PlacedObject.ResizableObjectData(self);
			return;
		}

		if (self.type == Enums.ColoredLanternPO) {
			self.data = new ColoredLanternData(self, false);
			return;
		}

		if (self.type == Enums.ColoredLanternStickPO) {
			self.data = new ColoredLanternData(self, true);
			return;
		}

		if (self.type == Enums.LillypadPO) {
			self.data = new Lillypad.LillypadData(self);
			return;
		}

		if (self.type == Enums.WaterDripsPO) {
			self.data = new GooDripSource.GooDripsData(self);
			return;
		}

		if (self.type == Enums.MagmaAreaPO) {
			self.data = new MagmaArea.MagmaAreaData(self);
			return;
		}

		if (self.type == Enums.HeatSourcePO) {
			self.data = new PlacedObject.ResizableObjectData(self);
			return;
		}

		if (self.type == Enums.ColoredCoralNeuronPO) {
			self.data = new ColoredCoralNeuron.ColoredCoralNeuronData(self);
			return;
		}

		if (self.type == Enums.ColoredDeepProcessingPO) {
			self.data = new ColoredDeepProcessingData(self);
			return;
		}

		if (self.type == Enums.CustomVinePO) {
			self.data = new CustomVineSystem.CustomVineData(self);
			return;
		}

		if (self.type == Enums.CustomVineConnectorPO) {
			self.data = new CustomVineConnectorData(self);
			return;
		}

		if (self.type == Enums.CustomLightRodPO) {
			self.data = new CustomLightRod.CustomLightRodData(self);
			return;
		}

		if (self.type == Enums.CustomLightArcPO) {
			self.data = new CustomLightArc.CustomLightArcData(self);
			return;
		}

		if (self.type == Enums.IceCubePO) {
			self.data = new PlacedObject.ResizableObjectData(self);
			return;
		}

		if (self.type == Enums.LittleIceCubesPO) {
			self.data = new PlacedObject.ResizableObjectData(self);
			return;
		}

		if (self.type == Enums.ColoredSparksPO) {
			self.data = new ColoredSparksData(self);
			return;
		}

		if (self.type == Enums.LightSource3dPO) {
			self.data = new LightSource3dData(self);
			return;
		}

		if (self.type == Enums.ColoredLightSource3dPO) {
			self.data = new ColoredLightSource3dData(self);
			return;
		}

		if (self.type == Enums.VerticalGatePositionPO) {
			self.data = new PlacedObject.ResizableObjectData(self);
			return;
		}
	}

	private static void On_ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj) {
		orig(self, tp, pObj);
		pObj ??= self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];

		string name = tp?.ToString();
		string repName = name + "_Rep";

		PlacedObjectRepresentation placedObjectRepresentation = null;

		if (tp == Enums.CactusPO) {
			placedObjectRepresentation = new CactusRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.SandDripPO) {
			placedObjectRepresentation = new SandDripRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.DeerSkullPO) {
			placedObjectRepresentation = new DeerSkull.DeerSkullRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.CattailPO) {
			placedObjectRepresentation = new ResizeableObjectRepresentation(self.owner, repName, self, pObj, name, false);
		}

		else if (tp == Enums.ColoredCattailPO) {
			placedObjectRepresentation = new Cattail.ColoredCattailRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.BubbleEmitterPO) {
			placedObjectRepresentation = new BubbleEmitter.BubbleEmitterRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.BambooPO) {
			placedObjectRepresentation = new Bamboo.BambooRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.ColoredLanternPO) {
			placedObjectRepresentation = new ColoredLanternRepresentaion(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.ColoredLanternStickPO) {
			placedObjectRepresentation = new ColoredLanternRepresentaion(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.LillypadPO) {
			placedObjectRepresentation = new Lillypad.LillypadRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.WaterDripsPO) {
			placedObjectRepresentation = new WaterDripSource.WaterDripsRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.MagmaAreaPO) {
			placedObjectRepresentation = new MagmaArea.MagmaAreaRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.HeatSourcePO) {
			placedObjectRepresentation = new ResizeableObjectRepresentation(self.owner, repName, self, pObj, name, true);
		}

		else if (tp == Enums.ColoredCoralNeuronPO) {
			placedObjectRepresentation = new ColoredCoralNeuron.ColoredCoralNeuronRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.ColoredDeepProcessingPO) {
			placedObjectRepresentation = new ColoredDeepProcessingRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.CustomVinePO) {
			placedObjectRepresentation = new CustomVineSystem.CustomVineRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.CustomVineConnectorPO) {
			placedObjectRepresentation = new CustomVineConnectorRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.CustomLightRodPO) {
			placedObjectRepresentation = new CustomLightRod.CustomLightRodRepresentation(self.owner, repName, self, pObj);
		}

		else if (tp == Enums.CustomLightArcPO) {
			placedObjectRepresentation = new CustomLightArc.CustomLightArcRepresentation(self.owner, repName, self, pObj);
		}

		else if (tp == Enums.IceCubePO) {
			placedObjectRepresentation = new ResizeableObjectRepresentation(self.owner, repName, self, pObj, name, true);
		}

		else if (tp == Enums.LittleIceCubesPO) {
			placedObjectRepresentation = new ResizeableObjectRepresentation(self.owner, repName, self, pObj, name, true);
		}

		else if (tp == Enums.ColoredSparksPO) {
			placedObjectRepresentation = new ColoredSparksRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.LightSource3dPO) {
			placedObjectRepresentation = new LightSource3dRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.ColoredLightSource3dPO) {
			placedObjectRepresentation = new ColoredLightSource3dRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.VerticalGatePositionPO) {
			placedObjectRepresentation = new VerticalGateRepresentation(self.owner, repName, self, pObj, name);
		}

		if (placedObjectRepresentation != null) {
			self.tempNodes.Add(placedObjectRepresentation);
			self.subNodes.Add(placedObjectRepresentation);
		}
	}

	private static void On_Room_Loaded(On.Room.orig_Loaded orig, Room self) {
		bool firstTimeRealized = self.abstractRoom.firstTimeRealized;

		orig(self);

		if (self.game == null)
			return;

		bool hasCoralNeuronSystem = self.updateList.Any(x => x is CoralNeuronSystem);
		CustomVineSystem customVineSystem = self.updateList.OfType<CustomVineSystem>().FirstOrDefault();

		for (int poIndex = 0; poIndex < self.roomSettings.placedObjects.Count; poIndex++) {
			PlacedObject pObj = self.roomSettings.placedObjects[poIndex];

			if (pObj.type == Enums.CactusPO && firstTimeRealized) {
				self.AddObject(new Cactus(self, pObj));
			}
			else if (pObj.type == Enums.SandDripPO) {
				self.AddObject(new SandDrip(self, pObj));
			}
			else if (pObj.type == Enums.DeerSkullPO) {
				self.AddObject(new DeerSkull(self, pObj));
			}
			else if (pObj.type == Enums.CattailPO) {
				Cattail.AbstractCattail abstractPhysicalObject = new Cattail.AbstractCattail(self.world, Enums.Cattail, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), Color.black);
				self.abstractRoom.entities.Add(abstractPhysicalObject);
				abstractPhysicalObject.Realize();

				CattailStick cattailStick = new CattailStick(self, pObj, abstractPhysicalObject.realizedObject as Cattail);
				self.AddObject(cattailStick);
			}
			else if (pObj.type == Enums.ColoredCattailPO) {
				// (UnityEngine.Random.value > 0.5f) ? Mathf.Lerp(0.45f, 0.55f, UnityEngine.Random.value) : Mathf.Lerp(0.05f, 0.1f, UnityEngine.Random.value),
				float hue = (pObj.data as Cattail.CattailData).hue;
				Color color = Custom.HSL2RGB(
					hue >= 1f ? 0f : hue,
					Mathf.Lerp(0.25f, 0.75f, UnityEngine.Random.value),
					Mathf.Lerp(0.5f, 0.75f, UnityEngine.Random.value)
				);
				Cattail.AbstractCattail abstractPhysicalObject = new Cattail.AbstractCattail(self.world, Enums.Cattail, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), color);
				self.abstractRoom.entities.Add(abstractPhysicalObject);
				abstractPhysicalObject.Realize();
			}
			else if (pObj.type == Enums.BubbleEmitterPO) {
				BubbleEmitter bubbleEmitter = new BubbleEmitter(self, pObj);
				self.AddObject(bubbleEmitter);
			}
			else if (pObj.type == Enums.BambooPO) {
				Bamboo bamboo = new Bamboo(self, pObj);
				self.AddObject(bamboo);
			}
			else if (pObj.type == Enums.ColoredLanternStickPO) {
				self.AddObject(new ColoredLanternStick(self, pObj, self.abstractRoom.index, poIndex));
			}
			else if (pObj.type == Enums.ColoredLanternPO && (!(self.game.session as StoryGameSession)?.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, poIndex) ?? true)) {
				EntityID newId = self.game.GetNewID();
				ColoredLanternData data = pObj.data as ColoredLanternData;
				AbstractColoredLantern abstractPhysicalObject = new AbstractColoredLantern(self.world, null, self.GetWorldCoordinate(pObj.pos), newId, self.abstractRoom.index, poIndex, data.color1, data.color2, data.dead, data) {
					isConsumed = false
				};
				self.abstractRoom.entities.Add(abstractPhysicalObject);
				abstractPhysicalObject.placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(self.abstractRoom.name, poIndex);
			}
			else if (pObj.type == Enums.LillypadPO && firstTimeRealized) {
				Lillypad.AbstractLillypad abstractLillypad = new Lillypad.AbstractLillypad(self.world, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), pObj);
				self.abstractRoom.entities.Add(abstractLillypad);
			}
			else if (pObj.type == Enums.WaterDripsPO) {
				self.AddObject(new WaterDripSource(pObj));
			}
			else if (pObj.type == Enums.MagmaAreaPO) {
				self.AddObject(new MagmaArea(self, pObj));
			}
			else if (pObj.type == Enums.HeatSourcePO) {
				self.AddObject(new HeatSource(self, pObj));
			}
			else if (pObj.type == Enums.ColoredCoralNeuronPO) {
				if (!hasCoralNeuronSystem) {
					self.AddObject(new CoralNeuronSystem());
					hasCoralNeuronSystem = true;
				}
				self.waitToEnterAfterFullyLoaded = Mathf.Max(self.waitToEnterAfterFullyLoaded, 80);
			}
			else if (pObj.type == Enums.ColoredDeepProcessingPO) {
				self.AddObject(new ColoredDeepProcessing(pObj, self));
			}
			else if (pObj.type == Enums.CustomVinePO) {
				if (customVineSystem == null) {
					customVineSystem = new CustomVineSystem(self);
					self.AddObject(customVineSystem);
				}

				customVineSystem.AddVine(pObj);
			}
			else if (pObj.type == Enums.CustomVineConnectorPO) {
				if (customVineSystem == null) {
					customVineSystem = new CustomVineSystem(self);
					self.AddObject(customVineSystem);
				}

				customVineSystem.AddConnector(pObj);
			}
			else if (pObj.type == Enums.CustomLightRodPO) {
				self.AddObject(new CustomLightRod(pObj, self));
			}
			else if (pObj.type == Enums.CustomLightArcPO) {
				self.AddObject(new CustomLightArc(pObj, self));
			}
			else if (pObj.type == Enums.IceCubePO && firstTimeRealized) {
				self.abstractRoom.AddEntity(new IceCube.AbstractIceCube(self.world, Enums.IceCube, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), pObj));
			}
			else if (pObj.type == Enums.LittleIceCubesPO) {
				self.AddObject(new LittleIceCubes(self, pObj));
			}
			else if (pObj.type == Enums.ColoredSparksPO) {
				self.AddObject(new ColoredSparks(self, pObj));
			}
			else if (pObj.type == Enums.LightSource3dPO) {
				LightSource3dData data = pObj.data as LightSource3dData;
				LightSource3d lightSource = new LightSource3d(pObj.pos, true, Color.white, null) {
					depth = data.depth,
					depthRange = data.depthRange,
				};
				self.AddObject(lightSource);
				lightSource.setRad = data.Rad;
				lightSource.setAlpha = data.strength;
				lightSource.fadeWithSun = data.fadeWithSun;
				lightSource.colorFromEnvironment = data.colorType == PlacedObject.LightSourceData.ColorType.Environment;
				lightSource.flat = data.flat;
				lightSource.effectColor = Math.Max(-1, (int) data.colorType - 2);
				self.SetLightSourceBlink(lightSource, poIndex);
			}
			else if (pObj.type == Enums.ColoredLightSource3dPO) {
				ColoredLightSource3dData data = pObj.data as ColoredLightSource3dData;
				ColoredLightSource3d light = new ColoredLightSource3d(pObj.pos, true, data.color, null, pObj) {
					depth = data.depth,
					depthRange = data.depthRange,
				};
				self.AddObject(light);
				light.setRad = data.Rad;
				light.setAlpha = data.strength;
				light.fadeWithSun = data.fadeWithSun;
				light.colorFromEnvironment = data.colorType == PlacedObject.LightSourceData.ColorType.Environment;
				light.flat = data.flat;
				light.effectColor = Math.Max(-1, (int) data.colorType - 2);
				self.SetLightSourceBlink(light, poIndex);
			}
		}
	}

	private static ObjectsPage.DevObjectCategories On_ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type) {
		if (Enums.Has(type))
			return Enums.FloodwatersCategory;

		return orig(self, type);
	}

	private static void On_AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self) {
		orig(self);

		if (self.realizedObject != null)
			return;

		if (self.type == Enums.CactusFruit) {
			self.realizedObject = new CactusFruit(self);
		}

		if (self.type == Enums.CactusSpear) {
			self.realizedObject = new CactusSpear(self);
		}

		if (self.type == Enums.Cattail) {
			self.realizedObject = new Cattail(self);
		}

		if (self.type == Enums.ColoredLantern) {
			self.realizedObject = new ColoredLantern(self as AbstractColoredLantern);
		}

		if (self.type == Enums.Lillypad) {
			self.realizedObject = new Lillypad(self as Lillypad.AbstractLillypad);
		}

		if (self.type == Enums.IceCube) {
			self.realizedObject = new IceCube(self);
		}
	}

	private static void On_CoralNeuronSystem_AIMapReady(On.CoralBrain.CoralNeuronSystem.orig_AIMapReady orig, CoralNeuronSystem self) {
		orig(self);

		foreach (PlacedObject pObj in self.room.roomSettings.placedObjects) {
			if (!pObj.active) continue;

			if (pObj.type == Enums.ColoredCoralNeuronPO) {
				ColoredCoralNeuron.ColoredCoralNeuronData data = pObj.data as ColoredCoralNeuron.ColoredCoralNeuronData;
				ColoredCoralNeuron neuron = new ColoredCoralNeuron(self, self.room, data.handlePos.magnitude, pObj.pos, pObj.pos + data.handlePos, pObj);
				self.room.AddObject(neuron);
				self.neurons.Add(neuron);
			}
		}
	}

	private static AbstractPhysicalObject On_SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString) {
		AbstractPhysicalObject apo = orig(world, objString);

		try {
			string[] array = Regex.Split(objString, "<oA>");
			if (apo.type == Enums.ColoredLantern) {
				return new AbstractColoredLantern(
					world, null, apo.pos, apo.ID,
					int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture),
					int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture),
					Custom.hexToColor(array[5]),
					Custom.hexToColor(array[6]),
					array[7] == "dead",
					null
				) {
					unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8)
				};
			}
		} catch (Exception ex) {
			Plugin.Log($"Failed to parse: {ex}");
		}

		return apo;
	}

	private static void On_Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self) {
		AbstractPhysicalObject apo = self.objectInStomach;

		orig(self);

		if (apo?.type == Enums.Cattail) {
			(apo.realizedObject as Cattail).Explode();
			self.Stun(80);
			return;
		}
	}

	private static void On_CoralNeuron_DrawSprites(On.CoralBrain.CoralNeuron.orig_DrawSprites orig, CoralNeuron self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		orig(self, sLeaser, rCam, timeStacker, camPos);

		if (self is ColoredCoralNeuron neuron) {
			neuron.Refresh(sLeaser, rCam, timeStacker, camPos);
		}
	}

	private static void IL_Player_UpdateAnimation(ILContext il) {
		ILCursor ilcursor = new ILCursor(il);
		MoveType moveType = MoveType.After;
		if (ilcursor.TryGotoNext(moveType, [
			x => x.MatchLdarg(0),
			x => x.MatchLdfld<UpdatableAndDeletable>("room"),
			x => x.MatchLdfld<Room>("climbableVines"),
			x => x.MatchLdarg(0),
			x => x.MatchLdfld<Player>("vinePos"),
			x => x.MatchLdarg(0),
			x => x.MatchCallOrCallvirt<ClimbableVinesSystem>("VineBeingClimbedOn"),
		])) {
			ilcursor.Emit(OpCodes.Ldarg_0);
			ilcursor.EmitDelegate(delegate (Player self) {
				Room room = self.room;
				ClimbableVinesSystem climbableVinesSystem = room?.climbableVines;
				if (climbableVinesSystem != null) {
					ClimbableVinesSystem.VinePosition vinePos = self.vinePos;
					if (vinePos != null && climbableVinesSystem.vines.Count > 0) {
						if (climbableVinesSystem.GetVineObject(vinePos) is CustomVineSystem.CustomVineClimbable customVine && customVine.JumpAllowed()) {
							self.canJump = 5;
						}
					}
				}
			});
		}
		else {
			Plugin.Log("Failed to IL hook player UpdateAnimation");
		}
	}

	private static void On_Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self) {
		if (self.animation == Player.AnimationIndex.VineGrab) {
			if (self.SwimDir(true).magnitude > 0f) {
				IClimbableVine vine = self.room.climbableVines.GetVineObject(self.vinePos);

				if (vine != null && vine is CustomVineSystem.CustomVineClimbable customVine && Random.value < 0.05f) {
					self.room.PlaySound(customVine.preset.climbSound, self.mainBodyChunk, false, 0.5f, 0.5f + Random.value * 1.5f);
				}
			}
		}

		orig(self);
	}

	private static void On_Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu) {
		bool wasGrabbingVine = self.animation == Player.AnimationIndex.VineGrab;
		orig(self, eu);
		if (self.animation == Player.AnimationIndex.VineGrab && !wasGrabbingVine) {
			IClimbableVine vine = self.room.climbableVines?.GetVineObject(self.vinePos);
			if (vine != null && vine is CustomVineSystem.CustomVineClimbable customVine) {
				self.room.PlaySound(customVine.preset.grabSound, self.mainBodyChunk, false, 1f, 0.75f + UnityEngine.Random.value * 0.5f);
			}
		}
	}

	private static void On_RainWorldGame_RestartGame(On.RainWorldGame.orig_RestartGame orig, RainWorldGame self) {
		orig(self);

		CustomVinePreset.presets.Clear();
	}

	private static void On_LightSource_InitiateSprites(On.LightSource.orig_InitiateSprites orig, LightSource self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
		if (self is LightSource3d light) {
			LightSource3d.InitiateSprites(light, sLeaser, rCam);
			return;
		}

		orig(self, sLeaser, rCam);
	}

	private static void On_LightSource_DrawSprites(On.LightSource.orig_DrawSprites orig, LightSource self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
		float darken = rCam.room.darkenLightsFactor;
		float flickerChance = 0.99f - self.blinkRate * 0.98f;
		if (self.blinkType == Enums.Flicker && UnityEngine.Random.value < flickerChance) {
			rCam.room.darkenLightsFactor = 1f;
		}

		if (self is LightSource3d light) {
			LightSource3d.DrawSprites(light, sLeaser, rCam, timeStacker, camPos);
			rCam.room.darkenLightsFactor = darken;
			return;
		}

		orig(self, sLeaser, rCam, timeStacker, camPos);
		rCam.room.darkenLightsFactor = darken;
	}
}