namespace Floodwaters.Objects;

public class ParserContext {
	public RoomCamera rCam;
	public Dictionary<string, object> variables = [];

	public ParserContext SetVariable(string id, object value) {
		this.variables[id] = value;

		return this;
	}
}

public class Parser {
	public Parser() {}

	public virtual object Evaluate(ParserContext context) {
		return null;
	}

	public static Parser Parse(string value) {
		value = value.Trim();
		Plugin.Log("ParseAny:" + value);

		if (value.IndexOf('(') == -1) {
			return ParseValue(value);
		}

		if (value.IndexOf('(') > value.IndexOf(')') || value.IndexOf('(') > value.IndexOf(',')) {
			int idx = Mathf.Min(value.IndexOf(')'), value.IndexOf(','));

			return ParseValue(value.Substring(0, idx));
		}

		string func = value.Substring(0, value.IndexOf('(')).Trim();
		string currentArg = "";
		List<string> args = [];
		int index = value.IndexOf('(') + 1;
		int inside = 0;
		for (; index < value.Length; index++) {
			if (value[index] == ')') {
				inside--;
			}
			else if (value[index] == '(') {
				inside++;
			}
			if (inside < 0) break;

			if (value[index] == ',' && inside == 0) {
				args.Add(currentArg.Trim());
				currentArg = "";
				continue;
			}
			currentArg += value[index];
		}
		if (currentArg.Length > 0) {
			args.Add(currentArg.Trim());
		}

		return ParseFunc(func, [.. args]);
	}

	protected static Parser ParseValue(string value) {
		Plugin.Log("ParseValue:" + value);

		if (value.StartsWith("\"") && value.EndsWith("\"")) {
			return new ParserValue(value.Substring(1, value.Length - 2));
		}

		switch (value) {
			case "false": {
				return new ParserValue(false);
			}
			case "true": {
				return new ParserValue(true);
			}
		}

		string numberValue = value;
		if (numberValue.ToLowerInvariant().EndsWith("f")) {
			numberValue = numberValue.Substring(0, numberValue.Length - 1);
		}
		if (float.TryParse(numberValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float valueF)) {
			return new ParserValue(valueF);
		}

		return new ParserVariable(value.ToLowerInvariant());
	}

	protected static Parser ParseFunc(string func, string[] args) {
		Plugin.Log("Parsing func: " + func + "(" + string.Join(", ", args) + ")");

		switch (func.ToLowerInvariant()) {
			case "": {
				if (args.Length != 1) {
					Plugin.Log("Parentheses parser must have one arg, not " + args.Length);
					return null;
				}

				return Parse(args[0]);
			}

			case "col":
			case "color":
			case "rgb":
			case "rgba": {
				if (args.Length < 3 || args.Length > 4) {
					Plugin.Log("Incorrect color arg amount: " + args.Length);
					return null;
				}

				return new ParserValue(Parse(args[0]), Parse(args[1]), Parse(args[2]), args.Length == 4 ? Parse(args[3]) : new ParserValue(1f));
			}

			case "vec2":
			case "vector2": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect vector2 arg amount: " + args.Length);
					return null;
				}

				return new ParserValue(Parse(args[0]), Parse(args[1]));
			}

			case "vec3":
			case "vector3": {
				if (args.Length != 3) {
					Plugin.Log("Incorrect vector3 arg amount: " + args.Length);
					return null;
				}

				return new ParserValue(Parse(args[0]), Parse(args[1]), Parse(args[2]));
			}

			case "+":
			case "add": {
				return new ParserMulti(ParserMulti.Type.Add, [.. args.Select(Parse).OfType<Parser>()]);
			}

			case "-":
			case "sub":
			case "subtract": {
				return new ParserMulti(ParserMulti.Type.Subtract, [.. args.Select(Parse).OfType<Parser>()]);
			}

			case "*":
			case "mul":
			case "multiply": {
				return new ParserMulti(ParserMulti.Type.Multiply, [.. args.Select(Parse).OfType<Parser>()]);
			}

			case "/":
			case "div":
			case "divide": {
				return new ParserMulti(ParserMulti.Type.Divide, [.. args.Select(Parse).OfType<Parser>()]);
			}

			case "%":
			case "modf":
			case "mod":
			case "fmod": {
				return new ParserMulti(ParserMulti.Type.Modulo, [.. args.Select(Parse).OfType<Parser>()]);
			}

			case "pal": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect pal arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.Palette, Parse(args[0]), Parse(args[1]));
			}

			case "lerp":
			case "interpolate": {
				if (args.Length != 3) {
					Plugin.Log("Incorrect lerp arg amount: " + args.Length);
					return null;
				}

				return new ParserTriple(ParserTriple.Type.Lerp, Parse(args[0]), Parse(args[1]), Parse(args[2]));
			}

			case "abs": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect abs arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Abs, Parse(args[0]));
			}

			case "round": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect round arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Round, Parse(args[0]));
			}

			case "ceil": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect ceil arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Ceil, Parse(args[0]));
			}

			case "floor": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect floor arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Floor, Parse(args[0]));
			}

			case "randf":
			case "randfloat":
			case "randomfloat": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect RandomFloat arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.RandomFloat, Parse(args[0]), Parse(args[1]));
			}

			case "randi":
			case "randint":
			case "randomint": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect RandomInt arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.RandomInt, Parse(args[0]), Parse(args[1]));
			}

			case "rand":
			case "randchoice":
			case "random":
			case "randomchoice":
			case "pick":
			case "pickrand":
			case "pickrandom": {
				return new ParserMulti(ParserMulti.Type.RandomChoice, [.. args.Select(Parse).OfType<Parser>()]);
			}

			case "if":
			case "ifelse":
			case "?:": {
				if (args.Length != 3) {
					Plugin.Log("Incorrect IF arg amount: " + args.Length);
					return null;
				}

				return new ParserTriple(ParserTriple.Type.IfElse, Parse(args[0]), Parse(args[1]), Parse(args[2]));
			}

			case "less":
			case "lessthan":
			case "<": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect LessThan arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.LessThan, Parse(args[0]), Parse(args[1]));
			}

			case "greater":
			case "greaterthan":
			case ">": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect GreaterThan arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.GreaterThan, Parse(args[0]), Parse(args[1]));
			}

			case "lessequal":
			case "lessthanequal":
			case "<=": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect LessThanEqual arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.LessThanEqual, Parse(args[0]), Parse(args[1]));
			}

			case "greaterequal":
			case "greaterthanequal":
			case ">=": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect GreaterThanEqual arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.GreaterThanEqual, Parse(args[0]), Parse(args[1]));
			}

			case "eq":
			case "equal":
			case "=":
			case "==": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect Equal arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.Equal, Parse(args[0]), Parse(args[1]));
			}

			case "neq":
			case "nequal":
			case "notequal":
			case "!=": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect NotEqual arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.NotEqual, Parse(args[0]), Parse(args[1]));
			}

			case "cosine":
			case "cos": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect Cosine arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Cos, Parse(args[0]));
			}

			case "sine":
			case "sin": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect Sine arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Sin, Parse(args[0]));
			}

			case "tangent":
			case "tan": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect Tangent arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Tan, Parse(args[0]));
			}

			case "clamp01": {
				if (args.Length != 1) {
					Plugin.Log("Incorrect Clamp01 arg amount: " + args.Length);
					return null;
				}

				return new ParserSingle(ParserSingle.Type.Clamp01, Parse(args[0]));
			}

			case "power":
			case "pow": {
				if (args.Length != 2) {
					Plugin.Log("Incorrect Power arg amount: " + args.Length);
					return null;
				}

				return new ParserDouble(ParserDouble.Type.Pow, Parse(args[0]), Parse(args[1]));
			}

			case "clamp": {
				if (args.Length != 3) {
					Plugin.Log("Incorrect Clamp arg amount: " + args.Length);
					return null;
				}

				return new ParserTriple(ParserTriple.Type.Clamp, Parse(args[0]), Parse(args[1]), Parse(args[2]));
			}
		}

		Plugin.Log("Failed to parse func: " + func + "(" + string.Join(", ", args) + ")");
		return null;
	}
}

public class ParserSingle : Parser {
	public Type type;
	public Parser a;

	public override object Evaluate(ParserContext context) {
		object a = this.a.Evaluate(context);

		if (a is float af) {
			if (this.type == Type.Abs) return Mathf.Abs(af);
			if (this.type == Type.Round) return Mathf.Round(af);
			if (this.type == Type.Floor) return Mathf.Floor(af);
			if (this.type == Type.Ceil) return Mathf.Ceil(af);
			if (this.type == Type.Cos) return Mathf.Cos(af);
			if (this.type == Type.Sin) return Mathf.Sin(af);
			if (this.type == Type.Tan) return Mathf.Tan(af);
			if (this.type == Type.Clamp01) return Mathf.Clamp01(af);
		} else if (a is bool ab) {
			if (this.type == Type.Not) return !ab;
		}

		Plugin.Log("Invalid single type or values");
		return null;
	}

	public ParserSingle(Type type, Parser a) {
		this.type = type;
		this.a = a;
	}

	public class Type : ExtEnum<Type> {
		public Type(string value, bool register = false) : base(value, register) {
		}

		public static readonly Type Abs = new Type("Abs", true);
		public static readonly Type Round = new Type("Round", true);
		public static readonly Type Ceil = new Type("Ceil", true);
		public static readonly Type Floor = new Type("Floor", true);
		public static readonly Type Not = new Type("Not", true);
		public static readonly Type Cos = new Type("Cos", true);
		public static readonly Type Sin = new Type("Sin", true);
		public static readonly Type Tan = new Type("Tan", true);
		public static readonly Type Clamp01 = new Type("Clamp01", true);
	}
}

public class ParserDouble : Parser {
	public Type type;
	public Parser a;
	public Parser b;

	public override object Evaluate(ParserContext context) {
		object a = this.a.Evaluate(context);
		object b = this.b.Evaluate(context);

		if (this.type == Type.Palette) {
			if (a is float af && b is float bf) {
				return context.rCam.currentPalette.texture.GetPixel(Mathf.RoundToInt(af), Mathf.RoundToInt(bf));
			}
		}
		else if (this.type == Type.RandomFloat) {
			if (a is float af && b is float bf) {
				return Random.Range(af, bf);
			}
		}
		else if (this.type == Type.RandomInt) {
			if (a is float af && b is float bf) {
				return (float) Random.Range((int) af, (int) bf);
			}
		}
		else if (this.type == Type.Equal) {
			return a.Equals(b);
		}
		else if (this.type == Type.NotEqual) {
			return !a.Equals(b);
		}
		else if (this.type == Type.GreaterThan) {
			if (a is float af && b is float bf) {
				return af > bf;
			}
		}
		else if (this.type == Type.LessThan) {
			if (a is float af && b is float bf) {
				return af < bf;
			}
		}
		else if (this.type == Type.GreaterThanEqual) {
			if (a is float af && b is float bf) {
				return af >= bf;
			}
		}
		else if (this.type == Type.LessThanEqual) {
			if (a is float af && b is float bf) {
				return af <= bf;
			}
		}
		else if (this.type == Type.Pow) {
			if (a is float af && b is float bf) {
				return Mathf.Pow(af, bf);
			}
		}

		Plugin.Log("Invalid double type or values: " + a.GetType().Name + ", " + b.GetType().Name);
		return null;
	}

	public ParserDouble(Type type, Parser a, Parser b) {
		this.type = type;
		this.a = a;
		this.b = b;
	}

	public class Type : ExtEnum<Type> {
		public Type(string value, bool register = false) : base(value, register) {
		}

		public static readonly Type RandomFloat = new Type("RandomFloat", true);
		public static readonly Type RandomInt = new Type("RandomInt", true);
		public static readonly Type Palette = new Type("Palette", true);
		public static readonly Type Equal = new Type("Equal", true);
		public static readonly Type NotEqual = new Type("NotEqual", true);
		public static readonly Type LessThan = new Type("LessThan", true);
		public static readonly Type GreaterThan = new Type("GreaterThan", true);
		public static readonly Type LessThanEqual = new Type("LessThanEqual", true);
		public static readonly Type GreaterThanEqual = new Type("GreaterThanEqual", true);
		public static readonly Type Pow = new Type("Pow", true);
	}
}

public class ParserMulti : Parser {
	public Type type;
	public Parser[] arguments;

	public override object Evaluate(ParserContext context) {
		if (this.arguments == null || this.arguments.Length == 0) return null;

		if (this.type == Type.RandomChoice) {
			return this.arguments[Random.Range(0, this.arguments.Length)];
		}

		object result = this.arguments[0].Evaluate(context);

		for (int i = 1; i < this.arguments.Length; i++) {
			object next = this.arguments[i].Evaluate(context);

			if (this.type == Type.Add) {
				if (result is float rf && next is float nf) result = rf + nf;
				else if (result is Color rc && next is Color nc) result = rc + nc;
				else if (result is string rs && next is string ns) result = rs + ns;
				else if (result is string rs2 && next is float nf2) result = rs2 + nf2;
				else if (result is string rs3 && next is Color nc2) result = rs3 + Utils.StringColor(nc2);
				else if (result is float rf3 && next is string ns4) result = rf3 + ns4;
				else if (result is Color rc3 && next is string ns5) result = Utils.StringColor(rc3) + ns5;
			}
			else if (this.type == Type.Subtract) {
				if (result is float rf && next is float nf) result = rf - nf;
			}
			else if (this.type == Type.Multiply) {
				if (result is float rf && next is float nf) result = rf * nf;
				else if (result is Color rc && next is Color nc) result = rc * nc;
				else if (result is Color rc2 && next is float nf2) result = rc2 * nf2;
			}
			else if (this.type == Type.Divide) {
				if (result is float rf && next is float nf) result = rf / nf;
				else if (result is Color rc2 && next is float nf2) result = rc2 * nf2;
			}
			else if (this.type == Type.Modulo) {
				if (result is float rf && next is float nf) result = rf % nf;
			}
		}

		return result;
	}

	public ParserMulti(Type type, Parser[] args) {
		this.type = type;
		this.arguments = args;
	}

	public class Type : ExtEnum<Type> {
		public Type(string value, bool register = false) : base(value, register) { }
		public static readonly Type Add = new Type("Add", true);
		public static readonly Type Subtract = new Type("Subtract", true);
		public static readonly Type Divide = new Type("Divide", true);
		public static readonly Type Multiply = new Type("Multiply", true);
		public static readonly Type RandomChoice = new Type("RandomChoice", true);
		public static readonly Type Modulo = new Type("Modulo", true);
	}
}

public class ParserTriple : Parser {
	public Type type;
	public Parser a;
	public Parser b;
	public Parser c;

	public override object Evaluate(ParserContext context) {
		object a = this.a.Evaluate(context);
		object b = this.b.Evaluate(context);
		object c = this.c.Evaluate(context);

		if (this.type == Type.Lerp) {
			if (a is float af && b is float bf && c is float cf) {
				return Mathf.Lerp(af, bf, cf);
			}
			if (a is Color ac && b is Color bc) {
				if (c is float cf2) {
					return Color.Lerp(ac, bc, cf2);
				} else if (c is Color cc) {
					return new Color(
						Mathf.Lerp(ac.r, bc.r, cc.r),
						Mathf.Lerp(ac.g, bc.g, cc.g),
						Mathf.Lerp(ac.b, bc.b, cc.b),
						Mathf.Lerp(ac.a, bc.a, cc.a)
					);
				}
			}
		}
		else if (this.type == Type.IfElse) {
			if (a is bool af) {
				return af ? b : c;
			}
		}
		else if (this.type == Type.Clamp) {
			if (a is float af && b is float bf && c is float cf) {
				return Mathf.Clamp(af, bf, cf);
			}
		}

		Plugin.Log("Invalid triple type or values: " + a.GetType().Name + ", " + b.GetType().Name + ", " + c.GetType().Name);
		return null;
	}

	public ParserTriple(Type type, Parser a, Parser b, Parser c) {
		this.type = type;
		this.a = a;
		this.b = b;
		this.c = c;
	}

	public class Type : ExtEnum<Type> {
		public Type(string value, bool register = false) : base(value, register) {
		}

		public static readonly Type Lerp = new Type("Lerp", true);
		public static readonly Type IfElse = new Type("IfElse", true);
		public static readonly Type Clamp = new Type("Clamp", true);
	}
}

public class ParserValue : Parser {
	public Type type;

	public Parser value0;
	public Parser value1;
	public Parser value2;
	public Parser value3;
	public float floatValue;
	public bool boolValue;
	public string stringValue;

	public override object Evaluate(ParserContext context) {
		if (this.type == Type.Color) return new Color((float) (float) this.value0.Evaluate(context), (float) (float) this.value1.Evaluate(context), (float) (float) this.value2.Evaluate(context), (float) (float) this.value3.Evaluate(context));
		else if (this.type == Type.Float) return this.floatValue;
		else if (this.type == Type.Bool) return this.boolValue;
		else if (this.type == Type.String) return this.stringValue;
		else if (this.type == Type.Vector2) return new Vector2((float) this.value0.Evaluate(context), (float) this.value1.Evaluate(context));
		else if (this.type == Type.Vector3) return new Vector3((float) this.value0.Evaluate(context), (float) this.value1.Evaluate(context), (float) this.value2.Evaluate(context));

		return null;
	}

	public ParserValue(float a, float b, float c, float d) : this(new ParserValue(a), new ParserValue(b), new ParserValue(c), new ParserValue(d)) {
	}

	public ParserValue(Parser r, Parser g, Parser b, Parser a) {
		this.type = Type.Color;

		this.value0 = r;
		this.value1 = g;
		this.value2 = b;
		this.value3 = a;
	}

	public ParserValue(Color color) : this(color.r, color.g, color.b, color.a) {
	}

	public ParserValue(float value) {
		this.type = Type.Float;
		this.floatValue = value;
	}

	public ParserValue(bool value) {
		this.type = Type.Bool;
		this.boolValue = value;
	}

	public ParserValue(string value) {
		this.type = Type.String;
		this.stringValue = value;
	}

	public ParserValue(Vector2 value) : this(value.x, value.y) {
	}

	public ParserValue(float a, float b) : this(new ParserValue(a), new ParserValue(b)) {
	}

	public ParserValue(Parser a, Parser b) {
		this.type = Type.Vector2;
		this.value0 = a;
		this.value1 = b;
	}

	public ParserValue(Vector3 value) : this(value.x, value.y, value.z) {
	}

	public ParserValue(float a, float b, float c) : this(new ParserValue(a), new ParserValue(b)) {
	}

	public ParserValue(Parser a, Parser b, Parser c) {
		this.type = Type.Vector3;
		this.value0 = a;
		this.value1 = b;
		this.value3 = c;
	}

	public static implicit operator ParserValue(Color value) {
		return new ParserValue(value);
	}

	public static implicit operator ParserValue(float value) {
		return new ParserValue(value);
	}

	public static implicit operator ParserValue(bool value) {
		return new ParserValue(value);
	}

	public static implicit operator ParserValue(string value) {
		return new ParserValue(value);
	}

	public static implicit operator ParserValue(Vector2 value) {
		return new ParserValue(value);
	}

	public static implicit operator ParserValue(Vector3 value) {
		return new ParserValue(value);
	}

	public class Type : ExtEnum<Type> {
		public Type(string value, bool register = false) : base(value, register) {
		}

		public static readonly Type Color = new Type("Color", true);
		public static readonly Type Float = new Type("Float", true);
		public static readonly Type Bool = new Type("Bool", true);
		public static readonly Type String = new Type("String", true);
		public static readonly Type Vector2 = new Type("Vector2", true);
		public static readonly Type Vector3 = new Type("Vector3", true);
	}
}

public class ParserVariable : Parser {
	public string value;

	public override object Evaluate(ParserContext context) {
		return context.variables[this.value];
	}

	public ParserVariable(string value) {
		this.value = value;
	}
}