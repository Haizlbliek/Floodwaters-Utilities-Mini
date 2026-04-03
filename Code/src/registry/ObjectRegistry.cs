namespace Floodwaters.Registry;

public static class ObjectRegistry {
	private static readonly List<IPlaceableDefinition> definitions = [];
	private static readonly Dictionary<PlacedObject.Type, IPlaceableDefinition> definitionsByType = [];

	public static IEnumerable<IPlaceableDefinition> Definitions => definitions;

	public static void Register<TObject>(PlaceableDefinition<TObject> def) where TObject : UpdatableAndDeletable{
		if (def == null)
			return;

		if (definitionsByType.ContainsKey(def.PlacedType))
			throw new InvalidOperationException($"Object type {def.PlacedType} already registered.");

		definitions.Add(def);
		definitionsByType[def.PlacedType] = def;
	}

	public static bool TryGetDefinition(PlacedObject.Type type, out IPlaceableDefinition definition) {
		return definitionsByType.TryGetValue(type, out definition);
	}

	public static bool TryGetDefinition(AbstractPhysicalObject.AbstractObjectType type, out IAbstractPlaceableDefinition definition) {
		definition = definitions.OfType<IAbstractPlaceableDefinition>().FirstOrDefault(x => x.AbstractObjectType == type);
		return definition != null;
	}
}
