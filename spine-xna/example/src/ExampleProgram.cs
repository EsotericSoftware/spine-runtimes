
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
#elif WINDOWS_STOREAPP 
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var factory = new MonoGame.Framework.GameFrameworkViewSource<Example>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
#endif
}