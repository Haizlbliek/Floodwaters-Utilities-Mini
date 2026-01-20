namespace Floodwaters.Objects;

public class HeatSource : UpdatableAndDeletable, IProvideWarmth {
	public PlacedObject pObj;

	public HeatSource(Room room, PlacedObject pObj) {
		this.room = room;
		this.pObj = pObj;
	}

	public Room loadedRoom => this.room;

	public float warmth => RainWorldGame.DefaultHeatSourceWarmth;

	public float range => (this.pObj.data as PlacedObject.ResizableObjectData).Rad;

	public Vector2 Position() {
		return this.pObj.pos;
	}
}