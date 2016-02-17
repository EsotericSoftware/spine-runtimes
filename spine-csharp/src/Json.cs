using System.IO;

namespace Spine {
	public static class Json {
		public static object Deserialize (TextReader text) {
			var parser = new SharpJson.JsonDecoder();
			parser.parseNumbersAsFloat = true;
			return parser.Decode(text.ReadToEnd());
		}
	}
}