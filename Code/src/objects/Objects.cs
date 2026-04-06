using Floodwaters.Registry;

namespace Floodwaters.Objects;

public static class Objects {
	public static void Initialize() {
		RegisterPlaceableObjects();

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
		ColoredFlameJet.Initialize();

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
		ColoredFlameJet.Cleanup();

		Futile.atlasManager.UnloadAtlas("atlases/Floodwaters-Mini");
	}

	private static void RegisterPlaceableObjects() {
		ObjectRegistry.Register(
			new PlaceableDefinition<Cactus>(
				Enums.CactusPO,
				pObj => new CactusData(pObj),
				(owner, idString, parentNode, pObj, name) => new CactusRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new Cactus(self, pObj)
			) {
				FirstTimeOnly = true,
			}
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<SandDrip>(
				Enums.SandDripPO,
				pObj => new SandDripData(pObj),
				(owner, idString, parentNode, pObj, name) => new SandDripRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new SandDrip(self, pObj)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<DeerSkull>(
				Enums.DeerSkullPO,
				pObj => new DeerSkull.DeerSkullData(pObj),
				(owner, idString, parentNode, pObj, name) => new DeerSkull.DeerSkullRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new DeerSkull(self, pObj)
			)
		);

		ObjectRegistry.Register(
			new AbstractPlaceableDefinition<Cattail, Cattail.AbstractCattail>(
				Enums.CattailPO,
				Enums.Cattail,
				pObj => new Cattail.CattailData(pObj),
				(owner, idString, parentNode, pObj, name) => new ResizeableObjectRepresentation(owner, idString, parentNode, pObj, name, false),
				(pObj, self) => new Cattail.AbstractCattail(self.world, Enums.Cattail, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), Color.black),
				self => new Cattail(self)
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					if (!firstTimeRealized) return;

					AbstractPhysicalObject abstr = def.CreateAbstractObject(pObj, self);
					self.abstractRoom.entities.Add(abstr);
					abstr.Realize();
					CattailStick cattailStick = new CattailStick(self, pObj, abstr.realizedObject as Cattail);
					self.AddObject(cattailStick);
				}
			}
		);

		ObjectRegistry.Register(
			new AbstractPlaceableDefinition<Cattail, Cattail.AbstractCattail>(
				Enums.ColoredCattailPO,
				Enums.Cattail,
				pObj => new Cattail.CattailData(pObj),
				(owner, idString, parentNode, pObj, name) => new Cattail.ColoredCattailRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => {
					float hue = (pObj.data as Cattail.CattailData).hue;
					Color color = Custom.HSL2RGB(
						hue % 1f,
						Mathf.Lerp(0.25f, 0.75f, UnityEngine.Random.value),
						Mathf.Lerp(0.5f, 0.75f, UnityEngine.Random.value)
					);
					return new Cattail.AbstractCattail(self.world, Enums.Cattail, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), color);
				},
				self => new Cattail(self)
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					if (!firstTimeRealized) return;

					AbstractPhysicalObject abstr = def.CreateAbstractObject(pObj, self);
					self.abstractRoom.entities.Add(abstr);
					abstr.Realize();
					CattailStick cattailStick = new CattailStick(self, pObj, abstr.realizedObject as Cattail);
					self.AddObject(cattailStick);
				}
			}
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<BubbleEmitter>(
				Enums.BubbleEmitterPO,
				po => new BubbleEmitter.BubbleEmitterData(po),
				(owner, idString, parentNode, pObj, name) => new BubbleEmitter.BubbleEmitterRepresentation(owner, idString, parentNode, pObj, name),
				(po, self) => new BubbleEmitter(self, po)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<Bamboo>(
				Enums.BambooPO,
				pObj => new PlacedObject.ResizableObjectData(pObj),
				(owner, idString, parentNode, pObj, name) => new Bamboo.BambooRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new Bamboo(self, pObj)
			)
		);

		// TODO: Colored Lanterns

		ObjectRegistry.Register(
			new AbstractPlaceableDefinition<Lillypad, Lillypad.AbstractLillypad>(
				Enums.LillypadPO,
				Enums.Lillypad,
				pObj => new Lillypad.LillypadData(pObj),
				(owner, idString, parentNode, pObj, name) => new Lillypad.LillypadRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new Lillypad.AbstractLillypad(self.world, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), pObj),
				self => new Lillypad(self)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<WaterDripSource>(
				Enums.WaterDripsPO,
				pObj => new GooDripSource.GooDripsData(pObj),
				(owner, idString, parentNode, pObj, name) => new WaterDripSource.WaterDripsRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new WaterDripSource(pObj)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<MagmaArea>(
				Enums.MagmaAreaPO,
				pObj => new MagmaArea.MagmaAreaData(pObj),
				(owner, idString, parentNode, pObj, name) => new MagmaArea.MagmaAreaRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new MagmaArea(self, pObj)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<HeatSource>(
				Enums.HeatSourcePO,
				pObj => new PlacedObject.ResizableObjectData(pObj),
				(owner, idString, parentNode, pObj, name) => new ResizeableObjectRepresentation(owner, idString, parentNode, pObj, name, true),
				(pObj, self) => new HeatSource(self, pObj)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<ColoredCoralNeuron>(
				Enums.ColoredCoralNeuronPO,
				pObj => new ColoredCoralNeuron.ColoredCoralNeuronData(pObj),
				(owner, idString, parentNode, pObj, name) => new ColoredCoralNeuron.ColoredCoralNeuronRepresentation(owner, idString, parentNode, pObj, name),
				null
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					if (!self.updateList.OfType<CoralNeuronSystem>().Any()) {
						self.AddObject(new CoralNeuronSystem());
					}
					self.waitToEnterAfterFullyLoaded = Mathf.Max(self.waitToEnterAfterFullyLoaded, 80);
				}
			}
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<ColoredDeepProcessing>(
				Enums.ColoredDeepProcessingPO,
				po => new ColoredDeepProcessingData(po),
				(owner, idString, parentNode, pObj, name) => new ColoredDeepProcessingRepresentation(owner, idString, parentNode, pObj, name),
				(po, self) => new ColoredDeepProcessing(po, self)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<UpdatableAndDeletable>(
				Enums.CustomVinePO,
				po => new CustomVineSystem.CustomVineData(po),
				(owner, idString, parentNode, pObj, name) => new CustomVineSystem.CustomVineRepresentation(owner, idString, parentNode, pObj, name),
				null
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					CustomVineSystem customVineSystem = self.updateList.OfType<CustomVineSystem>().FirstOrDefault();
					if (customVineSystem == null) {
						customVineSystem = new CustomVineSystem(self);
						self.AddObject(customVineSystem);
					}

					customVineSystem.AddVine(pObj);
				}
			}
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<UpdatableAndDeletable>(
				Enums.CustomVineConnectorPO,
				po => new CustomVineConnectorData(po),
				(owner, idString, parentNode, pObj, name) => new CustomVineConnectorRepresentation(owner, idString, parentNode, pObj, name),
				null
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					CustomVineSystem customVineSystem = self.updateList.OfType<CustomVineSystem>().FirstOrDefault();
					if (customVineSystem == null) {
						customVineSystem = new CustomVineSystem(self);
						self.AddObject(customVineSystem);
					}

					customVineSystem.AddConnector(pObj);
				}
			}
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<CustomLightRod>(
				Enums.CustomLightRodPO,
				po => new CustomLightRod.CustomLightRodData(po),
				(owner, idString, parentNode, pObj, name) => new CustomLightRod.CustomLightRodRepresentation(owner, idString, parentNode, pObj),
				(po, self) => new CustomLightRod(po, self)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<CustomLightArc>(
				Enums.CustomLightArcPO,
				po => new CustomLightArc.CustomLightArcData(po),
				(owner, idString, parentNode, pObj, name) => new CustomLightArc.CustomLightArcRepresentation(owner, idString, parentNode, pObj),
				(po, self) => new CustomLightArc(po, self)
			)
		);

		ObjectRegistry.Register(
			new AbstractPlaceableDefinition<IceCube, IceCube.AbstractIceCube>(
				Enums.IceCubePO,
				Enums.IceCube,
				po => new PlacedObject.ResizableObjectData(po),
				(owner, idString, parentNode, pObj, name) => new ResizeableObjectRepresentation(owner, idString, parentNode, pObj, name, true),
				(po, self) => new IceCube.AbstractIceCube(self.world, null, self.GetWorldCoordinate(po.pos), self.game.GetNewID(), po),
				self => new IceCube(self)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<LittleIceCubes>(
				Enums.LittleIceCubesPO,
				po => new PlacedObject.ResizableObjectData(po),
				(owner, idString, parentNode, pObj, name) => new ResizeableObjectRepresentation(owner, idString, parentNode, pObj, name, true),
				(po, self) => new LittleIceCubes(self, po)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<ColoredSparks>(
				Enums.ColoredSparksPO,
				pObj => new ColoredSparksData(pObj),
				(owner, idString, parentNode, pObj, name) => new ColoredSparksRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new ColoredSparks(self, pObj)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<LightSource3d>(
				Enums.LightSource3dPO,
				pObj => new LightSource3dData(pObj),
				(owner, idString, parentNode, pObj, name) => new LightSource3dRepresentation(owner, idString, parentNode, pObj, name),
				null
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					int poIndex = self.roomSettings.placedObjects.IndexOf(pObj);
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
			}
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<ColoredLightSource3d>(
				Enums.ColoredLightSource3dPO,
				pObj => new ColoredLightSource3dData(pObj),
				(owner, idString, parentNode, pObj, name) => new ColoredLightSource3dRepresentation(owner, idString, parentNode, pObj, name),
				null
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					int poIndex = self.roomSettings.placedObjects.IndexOf(pObj);
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
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<UpdatableAndDeletable>(
				Enums.VerticalGatePositionPO,
				pObj => new PlacedObject.ResizableObjectData(pObj),
				(owner, idString, parentNode, pObj, name) => new VerticalGateRepresentation(owner, idString, parentNode, pObj, name),
				null
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<ColoredFlameJet>(
				Enums.ColoredFlameJetPO,
				pObj => new ColoredFlameJetData(pObj),
				(owner, idString, parentNode, pObj, name) => new ColoredFlameJetRepresentation(owner, idString, parentNode, pObj),
				(pObj, self) => new ColoredFlameJet(self, pObj.data as ColoredFlameJetData)
			) {
				OnRoomLoadedAction = (def, self, pObj, firstTimeRealized) => {
					ColoredFlameJet jet = def.CreateObject(pObj, self) as ColoredFlameJet;
					self.AddObject(jet);
					(pObj.data as ColoredFlameJetData).obj = jet;
				}
			}
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<EffectOverride>(
				Enums.EffectOverrideRectPO,
				pObj => new EffectOverrideData(pObj),
				(owner, idString, parentNode, pObj, name) => new EffectOverrideRectRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new EffectOverride(self, pObj)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<EffectOverride>(
				Enums.EffectOverrideCirclePO,
				pObj => new EffectOverrideData(pObj),
				(owner, idString, parentNode, pObj, name) => new EffectOverrideCircleRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new EffectOverride(self, pObj)
			)
		);

		ObjectRegistry.Register(
			new PlaceableDefinition<SmokePipe>(
				Enums.SmokePipe,
				pObj => new SmokePipeData(pObj),
				(owner, idString, parentNode, pObj, name) => new SmokePipeRepresentation(owner, idString, parentNode, pObj, name),
				(pObj, self) => new SmokePipe(self, pObj)
			)
		);
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

		if (ObjectRegistry.TryGetDefinition(self.type, out var def)) {
			PlacedObject.Data data = def.CreateData(self);
			if (data != null) {
				self.data = data;
				return;
			}
		}

		if (self.type == Enums.ColoredLanternPO) {
			self.data = new ColoredLanternData(self, false);
		}
		else if (self.type == Enums.ColoredLanternStickPO) {
			self.data = new ColoredLanternData(self, true);
		}
	}

	private static void On_ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj) {
		orig(self, tp, pObj);
		pObj ??= self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];

		string name = tp?.ToString();
		string repName = name + "_Rep";

		PlacedObjectRepresentation placedObjectRepresentation = null;

		if (ObjectRegistry.TryGetDefinition(tp, out var regDef)) {
			placedObjectRepresentation = regDef.CreateRepresentation(self.owner, repName, self, pObj, name);
			if (placedObjectRepresentation != null) {
				self.tempNodes.Add(placedObjectRepresentation);
				self.subNodes.Add(placedObjectRepresentation);
				return;
			}
		}

		if (tp == Enums.ColoredLanternPO) {
			placedObjectRepresentation = new ColoredLanternRepresentaion(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.ColoredLanternStickPO) {
			placedObjectRepresentation = new ColoredLanternRepresentaion(self.owner, repName, self, pObj, name);
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

		for (int poIndex = 0; poIndex < self.roomSettings.placedObjects.Count; poIndex++) {
			PlacedObject pObj = self.roomSettings.placedObjects[poIndex];

			if (ObjectRegistry.TryGetDefinition(pObj.type, out var regDef)) {
				regDef.OnRoomLoaded(self, pObj, firstTimeRealized);
				continue;
			}

			if (pObj.type == Enums.ColoredLanternStickPO) {
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

		if (ObjectRegistry.TryGetDefinition(self.type, out IAbstractPlaceableDefinition regDef)) {
			self.realizedObject = regDef.CreateRealizedObject(self);
			return;
		}

		if (self.type == Enums.CactusFruit) {
			self.realizedObject = new CactusFruit(self);
		}

		if (self.type == Enums.CactusSpear) {
			self.realizedObject = new CactusSpear(self);
		}

		if (self.type == Enums.ColoredLantern) {
			self.realizedObject = new ColoredLantern(self as AbstractColoredLantern);
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