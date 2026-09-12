namespace Floodwaters.Objects;

public static class FWDevUI {
	public static IDevUIEditable EditingDevUINode = null;

	public static bool ShouldBlockKeyboardInput => EditingDevUINode is TextInput;

	private static List<Hook> hooks = [];

	public static void Initialize() {
		Type inputType = typeof(Input);
		Type[] stringArg = [typeof(string)];
		Type[] keyCodeArg = [typeof(KeyCode)];
		hooks = [
			new Hook(inputType.GetMethod("GetKey", stringArg), InputGetKey_string),
			new Hook(inputType.GetMethod("GetKey", keyCodeArg), InputGetKey_keyCode),
			new Hook(inputType.GetMethod("GetKeyUp", stringArg), InputGetKey_string),
			new Hook(inputType.GetMethod("GetKeyUp", keyCodeArg), InputGetKey_keyCode),
			new Hook(inputType.GetMethod("GetKeyDown", stringArg), InputGetKey_string),
			new Hook(inputType.GetMethod("GetKeyDown", keyCodeArg), InputGetKey_keyCode),
		];

		On.RainWorldGame.RawUpdate += On_RainWorldGame_RawUpdate;
	}

	public static void Cleanup() {
		foreach (Hook hook in hooks) {
			hook.Free();
		}
		hooks.Clear();

		On.RainWorldGame.RawUpdate -= On_RainWorldGame_RawUpdate;
	}

	private static bool InputGetKey_string(Func<string, bool> orig, string name) {
		bool res = orig(name);

		if (ShouldBlockKeyboardInput) {
			res = false;
		}
		return res;
	}

	private static bool InputGetKey_keyCode(Func<KeyCode, bool> orig, KeyCode key) {
		bool res = orig(key);
		if (ShouldBlockKeyboardInput) {
			res = false;
		}
		return res;
	}


	private static void On_RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt) {
		if (self.devUI == null) {
			StopEdit();
			orig(self, dt);
			return;
		}

		if (EditingDevUINode is not TextInput textInput) {
			orig(self, dt);
			return;
		}

		string input = Input.inputString;

		foreach (char c in input) {
			if (c == '\b') {
				textInput.Text = textInput.Text.Substring(0, Math.Max(textInput.Text.Length - 1, 0));
			}
			else if (c == '\n' || c == '\r') {
				StopEdit();
			}
			else {
				textInput.Text += c;
			}
		}

		orig(self, dt);
	}

	public static void Edit(IDevUIEditable editable) {
		EditingDevUINode?.Editing = false;
		EditingDevUINode = editable;
		EditingDevUINode?.Editing = true;
	}

	public static void StopEdit() {
		if (EditingDevUINode is TextInput textInput) {
			DevUINode devUINode = textInput;
			while (devUINode != null) {
				devUINode = devUINode.parentNode;
				if (devUINode is IDevUISignals) {
					(devUINode as IDevUISignals).Signal(Enums.Edited, textInput, textInput.Text);
					break;
				}
			}
		}

		EditingDevUINode?.Editing = false;
		EditingDevUINode = null;
	}

	public static void NodeRemoved(IDevUIEditable editable) {
		if (EditingDevUINode == editable) {
			StopEdit();
		}
	}

	public static void ToggleEdit(IDevUIEditable editable) {
		if (EditingDevUINode == editable) {
			StopEdit();
		}
		else {
			Edit(editable);
		}
	}
}