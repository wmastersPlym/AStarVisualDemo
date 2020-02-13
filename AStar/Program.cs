using System;

namespace AStarMonoGame._4
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new AStarExample())
                game.Run();
        }
    }
#endif
}


// ORIGINAL: https://github.com/dreasgrech/AStarXNA