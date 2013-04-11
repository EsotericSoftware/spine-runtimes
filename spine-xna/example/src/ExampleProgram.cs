using System;

namespace Spine {
#if WINDOWS || XBOX
    static class ExampleProgram {
        static void Main (string[] args) {
            using (Example game = new Example()) {
                game.Run();
            }
        }
    }
#endif
}
