namespace Floodwaters.Objects;

public class CustomVinePreset {
	public static FAtlasElement LoadElement(string value) {
		if (!value.StartsWith("/")) {
			if (Futile.atlasManager.DoesContainElementWithName(value)) {
				return Futile.atlasManager.GetElementWithName(value);
			}
		}
		string name = "CustomVine-" + value;

		if (Futile.atlasManager.DoesContainElementWithName(name)) {
			return Futile.atlasManager.GetElementWithName(name);
		}
		FAtlas atlas = Futile.atlasManager.ActuallyLoadAtlasOrImage(name, "vinepresets" + Path.DirectorySeparatorChar + value, "");
		atlas.texture.wrapMode = TextureWrapMode.Repeat;
		return atlas?.elements[0];
	}

	public static Dictionary<string, CustomVinePreset> presets = [];

	public static CustomVinePreset GetPreset(string id) {
		if (presets.TryGetValue(id, out CustomVinePreset existing)) {
			return existing;
		}

		CustomVinePreset preset = new CustomVinePreset(id);
		presets.Add(id, preset);
		return preset;
	}

	public LoadingMode loadingMode = LoadingMode.Main;

	public readonly string id;
	public SoundID grabSound = SoundID.Leaves;
	public SoundID climbSound = SoundID.Leaves;
	public Parser color = new ParserValue(1f, 1f, 1f, 1f);
	public Parser radius = new ParserValue(2f);
	public Parser mass = new ParserValue(0.25f);
	public FAtlasElement texture;
	public float textureLoopVertexCount = 2f;
	public bool climbable = true;
	public bool jumpable = true;
	public string shader = "";
	public List<Child> children = [];

	public CustomVinePreset(string id) {
		this.id = id;

		string[] properties = GetPresetProperties(this.id);

		foreach (string line in properties) {
			if (line.Trim() == "") {
				continue;
			}

			string key = line.Substring(0, line.IndexOf('=')).ToLowerInvariant().Replace(" ", "").Replace("_", "");
			string value = line.Substring(line.IndexOf('=') + 1);

			Plugin.Log("Loading '" + key + "=" + value + "'  Mode: " + this.loadingMode.value);

			if (this.loadingMode != LoadingMode.Main && key[0] != '\t') {
				this.loadingMode = LoadingMode.Main;
			}

			if (this.loadingMode == LoadingMode.Main) {
				this.ParseDefault(key, value);
			}
			else if (this.loadingMode == LoadingMode.Child) {
				this.ParseChild(key.Substring(1), value);
			}
			else {
				this.ParseCustom(key, value);
			}
		}

		foreach (Child child in this.children) {
			Plugin.Log("CHILD! " + child.GetType().Name);
		}
	}

	public static float ParseFloat(string value) {
		if (value.EndsWith("f")) {
			value = value.Substring(0, value.Length - 1);
		}

		return float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
	}

	public void ParseDefault(string key, string value) {
		switch (key) {
			case "grabsound": {
				this.grabSound = new SoundID(value, false);
				break;
			}
			case "climbsound": {
				this.climbSound = new SoundID(value, false);
				break;
			}
			case "color": {
				this.color = Parser.Parse(value);
				break;
			}
			case "texture": {
				this.texture = LoadElement(value);
				break;
			}
			case "textureloopvertexcount": {
				this.textureLoopVertexCount = ParseFloat(value);
				break;
			}
			case "climbable": {
				this.climbable = bool.Parse(value);
				break;
			}
			case "jumpable": {
				this.jumpable = bool.Parse(value);
				break;
			}
			case "rad":
			case "width":
			case "radius": {
				this.radius = Parser.Parse(value);
				break;
			}
			case "mass": {
				this.mass = Parser.Parse(value);
				break;
			}
			case "shader": {
				this.shader = value;
				break;
			}
			case "childleaf": {
				this.children.Add(new ChildLeaf());
				this.loadingMode = LoadingMode.Child;
				break;
			}
		}
	}

	public void ParseChild(string key, string value) {
		Child child = this.children[this.children.Count - 1];

		switch (key) {
			case "dist":
			case "distance":
			case "placedist":
			case "placedistance":
			case "placementdistance":
			case "placementdist": {
				child.placementDistance = Parser.Parse(value);
				break;
			}

			case "color": {
				if (child is ChildLeaf leaf) {
					leaf.color = Parser.Parse(value);
				}
				break;
			}
			case "sprite": {
				if (child is ChildLeaf leaf) {
					leaf.sprite = Parser.Parse(value);
				}
				break;
			}
			case "spriteanchor": {
				if (child is ChildLeaf leaf) {
					leaf.spriteAnchor = Parser.Parse(value);
				}
				break;
			}
			case "spritescale": {
				if (child is ChildLeaf leaf) {
					leaf.spriteScale = Parser.Parse(value);
				}
				break;
			}
			case "spriterotation": {
				if (child is ChildLeaf leaf) {
					leaf.spriteRotation = Parser.Parse(value);
				}
				break;
			}
			case "spriteoffset": {
				if (child is ChildLeaf leaf) {
					leaf.spriteOffset = Parser.Parse(value);
				}
				break;
			}
		}
	}

	public void ParseCustom(string key, string value) {}


	public static string[] GetPresetProperties(string id) {
		string path = AssetManager.ResolveFilePath("vinepresets" + Path.DirectorySeparatorChar + id + ".txt");
		if (!File.Exists(path)) {
			return [];
		}
		string[] array = File.ReadAllText(path).Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
		string[] result = new string[array.Length];
		for (int i = 0; i < array.Length; i++) {
			string x = array[i];
			result[i] = Regex.Replace(x, "//.*", "");
		}
		return result;
	}

	public class LoadingMode : ExtEnum<LoadingMode> {
		public LoadingMode(string value, bool register = false) : base(value, register) {
		}

		public static readonly LoadingMode Main = new LoadingMode("Main", true);
		public static readonly LoadingMode Child = new LoadingMode("Child", true);
	}


	public abstract class Child {
		public Parser placementDistance = new ParserDouble(ParserDouble.Type.RandomFloat, new ParserValue(0.65f), new ParserValue(3.5f));
	}

	public class ChildLeaf : Child {
		public Parser color = new ParserValue(1.0f, 1.0f, 1.0f);
		public Parser sprite = new ParserValue("Leaf0");
		public Parser spriteAnchor = new ParserValue(0.5f, 0.9f);
		public Parser spriteScale = new ParserValue(1.0f);
		public Parser spriteRotation = new ParserValue(0.0f);
		public Parser spriteOffset = new ParserValue(0.0f, 0.0f);
	}
}