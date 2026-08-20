namespace Floodwaters;

public interface IEnumWrapper<T> where T : ExtEnum<T> {
	T Value { get; }
}
