namespace Floodwaters;

public class EnumRegistry<R> where R : EnumRegistry<R> {
	private static readonly List<Action> CoreRegister = [];
	private static readonly List<Action> CoreUnregister = [];

	public static void Initialize() {
		System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(R).TypeHandle);

		CoreRegister.ForEach(a => a());
	}

	public static void Cleanup() {
		CoreUnregister.ForEach(a => a());
	}

	public static bool Has<T>(T t) where T : ExtEnum<T> {
		return Enum<T>.Enums.Contains(t);
	}

	public class Enum<T> : IEnumWrapper<T> where T : ExtEnum<T> {
		public static readonly HashSet<T> Enums = [];

		public T Value { get; private set; }
		private bool existedBefore;
		private readonly string _id;

#pragma warning disable IDE1006
		public int index => this.Value.index;
#pragma warning restore IDE1006

		public Enum(string id) {
			this._id = id;

			CoreRegister.Add(() => {
				T t = (T) Activator.CreateInstance(typeof(T), this._id, false);
				this.existedBefore = t.index != -1;
				this.Value = this.existedBefore ? t : (T) Activator.CreateInstance(typeof(T), this._id, true);

				Enums.Add(this.Value);
			});

			CoreUnregister.Add(() => {
				if (!this.existedBefore)
					this.Value?.Unregister();

				Enums.Remove(this.Value);
				this.existedBefore = false;
				this.Value = null;
			});
		}

		public static implicit operator T(Enum<T> e) => e?.Value;
	}
}