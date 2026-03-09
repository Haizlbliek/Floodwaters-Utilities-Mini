namespace Floodwaters.Objects;

public class PropertyMesh : FContainer {
	protected readonly FGameObjectNode rendererNode;
	protected readonly GameObject renderer;
	protected readonly MeshRenderer meshRenderer;
	protected readonly Mesh mesh;
	protected readonly Material material;
	protected static readonly Dictionary<string, Material> CachedMaterials = [];
	protected static readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();


	private Vector3[] vertices = [];
	public Vector2[] Vertices {
		set {
			if (this.vertices.Length != value.Length) {
				this.vertices = new Vector3[value.Length];
			}
			for (int i = 0; i < value.Length; i++) {
				this.vertices[i] = new Vector3(value[i].x, value[i].y, 0f);
			}
			this.meshDirty = true;
		}
	}

	private int[] indices = [];
	public int[] Indices {
		set {
			this.indices = value;
			this.meshDirty = true;
		}
	}

	private Vector2[] uvs = [];
	public Vector2[] UVs {
		set {
			this.uvs = value;
			this.meshDirty = true;
		}
	}

	private Color[] vertexColors = [];
	public Color[] VertexColors {
		set {
			this.vertexColors = value;
			this.meshDirty = true;
		}
	}

	public Color Color {
		set {
			if (this.vertexColors.Length != this.vertices.Length) {
				this.vertexColors = new Color[this.vertices.Length];
				for (int i = 0; i < this.vertexColors.Length; i++) {
					this.vertexColors[i] = Color.white;
				}
			}
			for (int i = 0; i < this.vertexColors.Length; i++) {
				this.vertexColors[i] = value;
			}
			this.meshDirty = true;
		}
	}

	public float Alpha {
		set {
			if (this.vertexColors.Length != this.vertices.Length) {
				this.vertexColors = new Color[this.vertices.Length];
				for (int i = 0; i < this.vertexColors.Length; i++) {
					this.vertexColors[i] = Color.white;
				}
			}
			for (int i = 0; i < this.vertexColors.Length; i++) {
				this.vertexColors[i].a = value;
			}
			this.meshDirty = true;
		}
	}

	protected Action<MaterialPropertyBlock> SetCustomProperties;
	private bool meshDirty;

	public PropertyMesh(string shaderName, Action<MaterialPropertyBlock> setCustomProperties) {
		this.SetCustomProperties = setCustomProperties;
		this.renderer = new GameObject("Dynamic Mesh Renderer", [ typeof(MeshFilter), typeof(MeshRenderer) ]);
		MeshFilter filter = this.renderer.GetComponent<MeshFilter>();
		this.meshRenderer = this.renderer.GetComponent<MeshRenderer>();

		this.mesh = new Mesh { name = "Dynamic Mesh" };
		filter.sharedMesh = this.mesh;

		if (!CachedMaterials.TryGetValue(shaderName, out this.material)) {
			this.material = new Material(Custom.rainWorld.Shaders[shaderName].shader);
			CachedMaterials[shaderName] = this.material;
		}
		this.meshRenderer.sharedMaterial = this.material;

		this.rendererNode = new FGameObjectNode(this.renderer, true, true, true);
		this.AddChild(this.rendererNode);

		this.meshDirty = true;
	}

	public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth) {
		base.Redraw(shouldForceDirty, shouldUpdateDepth);

		this.meshRenderer.GetPropertyBlock(mpb);
		this.SetCustomProperties?.Invoke(mpb);
		this.meshRenderer.SetPropertyBlock(mpb);

		if (this.meshDirty || shouldForceDirty) {
			this.mesh.vertices = this.vertices;
			this.mesh.uv = this.uvs;
			if (this.vertexColors.Length == this.vertices.Length) {
				this.mesh.colors = this.vertexColors;
			}
			this.mesh.SetIndices(this.indices, MeshTopology.Triangles, 0);
			this.mesh.RecalculateBounds();
			this.meshDirty = false;
		}
	}

	public override void HandleRemovedFromStage() {
		base.HandleRemovedFromStage();
		if (this.renderer != null) UnityEngine.Object.Destroy(this.renderer);
		if (this.mesh != null) UnityEngine.Object.Destroy(this.mesh);
	}

	public void MoveVertex(int index, Vector2 newPos) {
		if (index >= 0 && index < this.vertices.Length) {
			this.vertices[index].x = newPos.x;
			this.vertices[index].y = newPos.y;
			this.meshDirty = true;
		}
	}
}
