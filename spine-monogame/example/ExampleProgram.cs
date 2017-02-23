
using System;

namespace Spine {
    static class ExampleProgram {
        static void Main (string[] args) {
            using (Example game = new Example()) {
                game.Run();
            }
        }
    }
}