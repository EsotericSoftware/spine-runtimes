using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SpineSkeletalAnimation
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using (App game = new App())
            {
                game.Run();
            }
        }
    }
}

