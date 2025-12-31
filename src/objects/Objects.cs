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

		On.Player.Regurgitate += On_Player_Regurgitate;

		Futile.atlasManager.LoadAtlas("atlases/cactus");
		Futile.atlasManager.LoadAtlas("atlases/lillypad");
		Futile.atlasManager.LoadAtlas("atlases/lillypad_details");
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

		On.Player.Regurgitate -= On_Player_Regurgitate;

		Futile.atlasManager.UnloadAtlas("atlases/cactus");
		Futile.atlasManager.UnloadAtlas("atlases/lillypad");
		Futile.atlasManager.UnloadAtlas("atlases/lillypad_details");
	}

	private static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj) {
		if (obj is CactusFruit)
			return Player.ObjectGrabability.OneHand;

		if (obj is CactusSpear)
			return Player.ObjectGrabability.OneHand;

		if (obj is Cattail cattail)
			return cattail.stick != null ? Player.ObjectGrabability.TwoHands : Player.ObjectGrabability.OneHand;

		if (obj is ColoredLantern lantern && !lantern.hasStick)
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

	private static void On_ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj) {
		orig(self, tp, pObj);
		pObj ??= self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];

		string name = tp?.ToString();
		string repName = name + "_Rep";

		PlacedObjectRepresentation placedObjectRepresentation = null;

		if (tp == Enums.CactusPO) {
			placedObjectRepresentation = new Cactus.CactusRepresentation(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.SandDripPO) {
			placedObjectRepresentation = new SandDrip.SandDripRepresentation(self.owner, repName, self, pObj, name);
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
			placedObjectRepresentation = new ColoredLantern.ColoredLanternRepresentaion(self.owner, repName, self, pObj, name);
		}

		else if (tp == Enums.ColoredLanternStickPO) {
			placedObjectRepresentation = new ColoredLantern.ColoredLanternStickRepresentaion(self.owner, repName, self, pObj, name);
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
				EntityID newId = self.game.GetNewID();
				ColoredLantern.AbstractColoredLantern abstractPhysicalObject = new ColoredLantern.AbstractColoredLantern(self.world, Enums.ColoredLantern, null, self.GetWorldCoordinate(pObj.pos), newId, self.abstractRoom.index, poIndex, pObj.data as ColoredLantern.ColoredLanternObjectData, pObj) {
					isConsumed = false
				};
				self.abstractRoom.entities.Add(abstractPhysicalObject);
				abstractPhysicalObject.placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(self.abstractRoom.name, poIndex);
			}
			else if (pObj.type == Enums.ColoredLanternPO && (!(self.game.session as StoryGameSession)?.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, poIndex) ?? true)) {
				EntityID newId = self.game.GetNewID();
				ColoredLantern.AbstractColoredLantern abstractPhysicalObject = new ColoredLantern.AbstractColoredLantern(self.world, Enums.ColoredLantern, null, self.GetWorldCoordinate(pObj.pos), newId, self.abstractRoom.index, poIndex, pObj.data as ColoredLantern.ColoredLanternObjectData, pObj) {
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
		}
	}

	private static void On_PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self) {
		orig(self);

		if (self.type == Enums.CactusPO) {
			self.data = new Cactus.CactusData(self);
			return;
		}

		if (self.type == Enums.SandDripPO) {
			self.data = new SandDrip.SandDripData(self);
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
			self.data = new ColoredLantern.ColoredLanternObjectData(self);
			return;
		}

		if (self.type == Enums.ColoredLanternStickPO) {
			self.data = new ColoredLantern.ColoredLanternStickObjectData(self);
			return;
		}

		if (self.type == Enums.LillypadPO) {
			self.data = new Lillypad.LillypadData(self);
		}

		if (self.type == Enums.WaterDripsPO) {
			self.data = new GooDripSource.GooDripsData(self);
		}

		if (self.type == Enums.MagmaAreaPO) {
			self.data = new MagmaArea.MagmaAreaData(self);
		}

		if (self.type == Enums.HeatSourcePO) {
			self.data = new PlacedObject.ResizableObjectData(self);
		}
	}

	private static ObjectsPage.DevObjectCategories On_ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type) {
		if (type == Enums.CactusPO || type == Enums.SandDripPO || type == Enums.DeerSkullPO || type == Enums.CattailPO || type == Enums.ColoredCattailPO || type == Enums.BubbleEmitterPO || type == Enums.BambooPO || type == Enums.ColoredLanternPO || type == Enums.ColoredLanternStickPO || type == Enums.LillypadPO || type == Enums.WaterDripsPO || type == Enums.MagmaAreaPO || type == Enums.HeatSourcePO)
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
			self.realizedObject = new ColoredLantern(self as ColoredLantern.AbstractColoredLantern);
		}

		if (self.type == Enums.Lillypad) {
			self.realizedObject = new Lillypad(self as Lillypad.AbstractLillypad);
		}
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
}