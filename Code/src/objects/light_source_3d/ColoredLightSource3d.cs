namespace Floodwaters.Objects;

public class ColoredLightSource3d : LightSource3d {
	protected PlacedObject pObj;
	protected ColoredLightSource3dData Data => this.pObj.data as ColoredLightSource3dData;

	public ColoredLightSource3d(Vector2 initPos, bool environmentalLight, Color color, UpdatableAndDeletable tiedToObject, PlacedObject pObj) : base(initPos, environmentalLight, color, tiedToObject) {
		this.pObj = pObj;
	}

	public override void Update(bool eu) {
		base.Update(eu);
		this.c = this.Data.color;
	}
}