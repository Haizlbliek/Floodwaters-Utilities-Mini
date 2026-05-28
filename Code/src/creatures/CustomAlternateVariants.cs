namespace Floodwaters.Creatures;

public static class CustomAlternateVariants {
	public static void Initialize() {
		On.VultureGraphics.ctor += On_VultureGraphics_ctor;
		On.VultureFeather.CurrentColor += On_VultureFeather_CurrentColor;
	}

	public static void Cleanup() {
		On.VultureGraphics.ctor -= On_VultureGraphics_ctor;
		On.VultureFeather.CurrentColor -= On_VultureFeather_CurrentColor;
	}

	private static void On_VultureGraphics_ctor(On.VultureGraphics.orig_ctor orig, VultureGraphics self, Vulture ow) {
		orig(self, ow);
		if (self.IsMiros && self.vulture.abstractCreature.superSizeMe || (ModManager.MSC && UnityEngine.Random.value < 0.001f)) {
			self.eyeCol = Custom.HSL2RGB(UnityEngine.Random.Range(0.55f, 0.60f), 1f, 0.5f);
			
			float baseHue = UnityEngine.Random.Range(0.45f, 0.53f);
			self.ColorA = new HSLColor(baseHue, 0.9f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
			
			float secondaryHue = baseHue + UnityEngine.Random.Range(0.05f, 0.12f);
			self.ColorB = new HSLColor(secondaryHue, Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
		}
	}

	private static Color On_VultureFeather_CurrentColor(On.VultureFeather.orig_CurrentColor orig, VultureFeather self) {
		if ((self.kGraphics.vulture.IsMiros && self.kGraphics.vulture.abstractCreature.superSizeMe) || self.kGraphics.eyeCol != Color.red) {
			Color rgb = HSLColor.Lerp(new HSLColor(self.kGraphics.ColorB.hue, Mathf.Lerp(self.kGraphics.ColorB.saturation, 1f, self.saturationBonus), Mathf.Lerp(self.kGraphics.ColorB.lightness, 1f, self.lightnessBonus)), self.kGraphics.ColorA, Mathf.Cos(Mathf.Pow(self.wingPosition, 0.75f) * (float) Math.PI)).rgb;
			rgb.a = Mathf.Max(0.4f, self.forcedAlpha, Mathf.Lerp(0.4f, 0.8f, Mathf.Cos(Mathf.Pow(self.wingPosition, 1.7f) * (float)Math.PI))) * (self.extendedFac + self.wing.flyingMode) * 0.5f * (1f - self.brokenColor);
			if (self.kGraphics.vulture.isLaserActive()) {
				rgb.a = UnityEngine.Random.value;
			}
			return rgb;
		}

		return orig(self);
	}
}
