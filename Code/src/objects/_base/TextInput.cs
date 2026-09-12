namespace Floodwaters.Objects;

public class TextInput : MouseOverSwitchColorLabel, IDevUIEditable {
	private readonly int cursorSprite;
	private readonly int textLabel;

	private float lastCursorX;
	private int flash;

	public bool Editing { get; set; }

	public TextInput(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string startText) : base(owner, IDstring, parentNode, pos, width, startText) {
		this.textLabel = this.fLabels.Count - 1;
		this.cursorSprite = this.fSprites.Count;
		this.fSprites.Add(new FSprite("Futile_White") { scaleX = 1f / 16f, scaleY = 7f / 8f, isVisible = false, anchorY = 0.5f, });
		if (owner != null) {
			Futile.stage.AddChild(this.fSprites[this.cursorSprite]);
		}
	}

	public override void Update() {
		base.Update();
		this.flash++;

		if (this.owner != null && this.owner.mouseClick && base.MouseOver) {
			FWDevUI.ToggleEdit(this);
		}

		FSprite cursor = this.fSprites[this.cursorSprite];
		cursor.isVisible = FWDevUI.EditingDevUINode == this && (this.flash % 40 < 22);
		float cursorX = this.fLabels[this.textLabel].textRect.width + this.fLabels[this.textLabel].textRect.x;
		if (this.lastCursorX != cursorX) {
			this.lastCursorX = cursorX;
			this.Refresh();
		}
	}

	public override void ClearSprites() {
		base.ClearSprites();
		FWDevUI.NodeRemoved(this);
	}

	public override void Refresh() {
		base.Refresh();
		this.MoveSprite(this.cursorSprite, this.absPos + new Vector2(this.lastCursorX + 2f, 9f));
	}
}