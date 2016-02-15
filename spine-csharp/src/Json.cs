using System.IO;

namespace Spine {
	public static class Json {
		
		static readonly SharpJson.JsonDecoder parser;

		static Json () {
			parser = new SharpJson.JsonDecoder();
			parser.parseNumbersAsFloat = true;
		}

		public static object Deserialize (TextReader text) {
			return parser.Decode(text.ReadToEnd());
		}
	}
}