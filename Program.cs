using Raylib_cs;
using ImGuiNET;
using System;
using CadUtility.GuiElementClasses;



namespace CadUtility
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            const int screenWidth = 960;
            const int screenHeight = 540;
            
            // Initialization
            //--------------------------------------------------------------------------------------
            Raylib.SetTraceLogCallback(&Logging.LogConsole);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT, "Dumbware");
            Raylib.SetExitKey(0);
            Raylib.SetTargetFPS(60);
            Raylib.InitAudioDevice();

            ImguiController controller = new ImguiController();
            EditorScreen editor = new EditorScreen();
            Settings.Initialize();
            CadScreen cad = new CadScreen();
            
            controller.Load(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);
            editor.Load();
            //--------------------------------------------------------------------------------------

            // Main game loop
            while (!Raylib.WindowShouldClose() && !cad.exit)
            {
                // Update
                //----------------------------------------------------------------------------------
                float dt = Raylib.GetFrameTime();
                // Feed the input events to our ImGui controller, which passes them through to ImGui.
                

                //editor.Update(dt);
                controller.Update(dt);
                Settings.Update();
                cad.Update();
                
                //----------------------------------------------------------------------------------

                // Draw
                //----------------------------------------------------------------------------------
                Raylib.BeginDrawing();
                Raylib.ClearBackground(cad.GetClearColor());

                /*Raylib.DrawText("Hello, world!", 12, 12, 20, Color.BLACK);*/

                controller.Draw();
                Raylib.EndDrawing();
                //----------------------------------------------------------------------------------
            }
            if (!cad.exit && cad._db.solidworksExists)
            {
                bool CloseSolidworks(int result)
                {
                    cad.exit = true;
                    if (result == 1)
                    {
                        cad._db.sw.closeOnExit = true;
                    }
                    return true;
                }

                cad._db._popups.Add(new Popup(cad._db, "Close Solidworks", PopupType.YesNo, YesNoFunc: CloseSolidworks));

                while (!cad.exit)
                {
                    float dt = Raylib.GetFrameTime();
                    controller.Update(dt);
                    Settings.Update();
                    cad.CloseScreen();



                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(cad.GetClearColor());
                    controller.Draw();
                    Raylib.EndDrawing();
                }
            }










            editor.Unload();
            controller.Dispose();
            cad.Dispose();
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
            //--------------------------------------------------------------------------------------
        }
    }
}
