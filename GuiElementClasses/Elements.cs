using System;
using ImGuiNET;
using System.Numerics;
using Raylib_cs;

namespace CadUtility.GuiElementClasses
{
    public struct Elements
    {
        
        public unsafe static bool PopupMessage(string message, bool firstFrame=false)
        {
            bool temp = true;
            if (!firstFrame)
            {
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Enter)) || ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Escape)) || ImGui.IsKeyPressed((int)KeyboardKey.KEY_KP_ENTER))
                {
                    return false;
                }
            }
            ImGui.SetNextWindowFocus();
            Vector2 size = new Vector2(Settings.FONT_SIZE * 15, Settings.FONT_SIZE * 3);
            ImGui.SetNextWindowPos(new Vector2( Settings.SCREEN_WIDTH / 2 - size.X / 2, Settings.SCREEN_HEIGHT / 2 - size.Y / 2));
            ImGui.SetNextWindowSize(size);
            if (ImGui.Begin($"##{message}", ref temp, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal))
            {
                ImGui.TextWrapped(message);
                ImGui.End();
            }
            return true;
        }


        //returns -1 when nothing has been entered, 0 when no, and 1 when yes
        public unsafe static int PopupMessageYesNo(string message, bool firstFrame=false, Func<bool> functionToCall= default(Func<bool>))
        {
            bool temp = true;
            if (!firstFrame)
            {
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Escape)) || ImGui.IsKeyPressed((int)KeyboardKey.KEY_N))
                {
                    return 0;
                }
                else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Enter)) || ImGui.IsKeyPressed((int)KeyboardKey.KEY_Y))
                {
                    var foo = functionToCall();
                    return 1;
                }
            }

            ImGui.SetNextWindowFocus();
            Vector2 size = new Vector2(Settings.FONT_SIZE * 15, Settings.FONT_SIZE * 5);
            ImGui.SetNextWindowPos(new Vector2(Settings.SCREEN_WIDTH / 2 - size.X / 2, Settings.SCREEN_HEIGHT / 2 - size.Y / 2));
            ImGui.SetNextWindowSize(size);
            if (ImGui.Begin($"##{message}", ref temp, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal))
            {
                ImGui.TextWrapped(message);
                ImGui.NewLine();
                ImGui.Text("(Y)es / (N)o");
                ImGui.End();
            }


            return -1;
        }
    }
}
