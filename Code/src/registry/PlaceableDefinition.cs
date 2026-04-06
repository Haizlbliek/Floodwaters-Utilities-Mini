namespace Floodwaters.Registry;

public interface IPlaceableDefinition {
	PlacedObject.Type PlacedType { get; }

	PlacedObject.Data CreateData(PlacedObject pObj);
	UpdatableAndDeletable CreateObject(PlacedObject pObj, Room room);
	PlacedObjectRepresentation CreateRepresentation(DevUI owner, string idString, DevUINode parentNode, PlacedObject pObj, string name);
	void OnRoomLoaded(Room room, PlacedObject pObj, bool firstTimeRealized);
}

public interface IAbstractPlaceableDefinition : IPlaceableDefinition {
	AbstractPhysicalObject.AbstractObjectType AbstractObjectType { get; }

	AbstractPhysicalObject CreateAbstractObject(PlacedObject pObj, Room room);
	PhysicalObject CreateRealizedObject(AbstractPhysicalObject self);
}

public class PlaceableDefinition<TObject> : IPlaceableDefinition where TObject : UpdatableAndDeletable {
	public Enums.Enum<PlacedObject.Type> PlacedTypeEnum { get; }
	public PlacedObject.Type PlacedType => this.PlacedTypeEnum;

	public Func<PlacedObject, PlacedObject.Data> CreateDataFunc { get; }
	public Func<PlacedObject, Room, TObject> CreateObjectFunc { get; }
	public Func<DevUI, string, DevUINode, PlacedObject, string, PlacedObjectRepresentation> CreateRepresentationFunc { get; }

	public Action<PlaceableDefinition<TObject>, Room, PlacedObject, bool> OnRoomLoadedAction { get; set; }
	public Action<TObject, Room> OnRealize { get; set; }
	public Func<AbstractPhysicalObject.AbstractObjectType, bool> IsConsumable { get; set; }

	public bool FirstTimeOnly = false;

	public PlaceableDefinition(
		Enums.Enum<PlacedObject.Type> placedType,
		Func<PlacedObject, PlacedObject.Data> createDataFunc,
		Func<DevUI, string, DevUINode, PlacedObject, string, PlacedObjectRepresentation> createRepresentationFunc,
		Func<PlacedObject, Room, TObject> createObjectFunc
	) {
		this.PlacedTypeEnum = placedType;
		this.CreateDataFunc = createDataFunc;
		this.CreateObjectFunc = createObjectFunc;
		this.CreateRepresentationFunc = createRepresentationFunc;
	}

	public PlacedObject.Data CreateData(PlacedObject pObj) {
		return this.CreateDataFunc?.Invoke(pObj);
	}

	public UpdatableAndDeletable CreateObject(PlacedObject pObj, Room room) {
		return this.CreateObjectFunc?.Invoke(pObj, room);
	}

	public PlacedObjectRepresentation CreateRepresentation(DevUI owner, string idString, DevUINode parentNode, PlacedObject pObj, string name) {
		return this.CreateRepresentationFunc?.Invoke(owner, idString, parentNode, pObj, name);
	}

	public virtual void OnRoomLoaded(Room room, PlacedObject pObj, bool firstTimeRealized) {
		if (this.FirstTimeOnly && !firstTimeRealized) return;

		if (this.OnRoomLoadedAction != null) {
			this.OnRoomLoadedAction(this, room, pObj, firstTimeRealized);
			return;
		}

		if (this.CreateObjectFunc != null) {
			UpdatableAndDeletable obj = this.CreateObject(pObj, room);
			if (obj != null) {
				room.AddObject(obj);
			}
		}
	}
}


public class AbstractPlaceableDefinition<TObject, TAbstractObject> : PlaceableDefinition<TObject>, IAbstractPlaceableDefinition where TObject : PhysicalObject where TAbstractObject : AbstractPhysicalObject {
	public Enums.Enum<AbstractPhysicalObject.AbstractObjectType> AbstractTypeEnum { get; }
	public AbstractPhysicalObject.AbstractObjectType AbstractObjectType => this.AbstractTypeEnum;

	public Func<PlacedObject, Room, TAbstractObject> CreateAbstractObjectFunc { get; }
	public Func<TAbstractObject, TObject> CreateRealizedObjectFunc { get; }

	public new Action<AbstractPlaceableDefinition<TObject, TAbstractObject>, Room, PlacedObject, bool> OnRoomLoadedAction { get; set; }

	public AbstractPlaceableDefinition(
		Enums.Enum<PlacedObject.Type> placedType, Enums.Enum<AbstractPhysicalObject.AbstractObjectType> abstractType,
		Func<PlacedObject, PlacedObject.Data> createDataFunc,
		Func<DevUI, string, DevUINode, PlacedObject, string, PlacedObjectRepresentation> createRepresentationFunc,
		Func<PlacedObject, Room, TAbstractObject> createAbstractObjectFunc,
		Func<TAbstractObject, TObject> createRealizedObjectFunc
	) : base(placedType, createDataFunc, createRepresentationFunc, null) {
		this.AbstractTypeEnum = abstractType;
		this.CreateAbstractObjectFunc = createAbstractObjectFunc;
		this.CreateRealizedObjectFunc = createRealizedObjectFunc;
		this.FirstTimeOnly = true;
	}

	public AbstractPhysicalObject CreateAbstractObject(PlacedObject pObj, Room room) {
		return this.CreateAbstractObjectFunc?.Invoke(pObj, room);
	}

	public override void OnRoomLoaded(Room room, PlacedObject pObj, bool firstTimeRealized) {
		if (this.FirstTimeOnly && !firstTimeRealized) return;

		if (this.OnRoomLoadedAction != null) {
			this.OnRoomLoadedAction(this, room, pObj, firstTimeRealized);
			return;
		}

		if (this.CreateAbstractObjectFunc != null) {
			AbstractPhysicalObject obj = this.CreateAbstractObject(pObj, room);
			if (obj != null) {
				room.abstractRoom.entities.Add(obj);
				obj.Realize();
			}
		}
	}

	public PhysicalObject CreateRealizedObject(AbstractPhysicalObject self) {
		return this.CreateRealizedObjectFunc?.Invoke(self as TAbstractObject);
	}
}