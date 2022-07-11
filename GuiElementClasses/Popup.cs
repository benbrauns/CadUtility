using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;
using System.Numerics;

namespace CadUtility.GuiElementClasses
{
    enum PopupType
    {
        Warning,
        YesNo,
    }


    internal unsafe class Popup
    {
        private int yesNoRes;
        private string message;
        private PopupType type;
        private bool firstFrame = true;
        private Database _db;
        private Func<int, bool> yesNoFunc;
        private Func<bool> warnFunc;
 


        public Popup(Database DB, string Message, PopupType Type, Func<int, bool> YesNoFunc=default(Func<int, bool>), Func<bool> WarnFunc = default(Func<bool>))
        {
            _db = DB;
            message = Message;
            type = Type;
            yesNoFunc = YesNoFunc;
            warnFunc = WarnFunc;
        }


        public void Draw()
        {
            if (type == PopupType.YesNo)
            {
                DrawYesNo();
            }
            else if (type == PopupType.Warning)
            {
                DrawWarning();
            }
            if (firstFrame)
            {
                firstFrame = false;
            }
        }


        private void DrawYesNo()
        {
            bool temp = true;
            if (!firstFrame)
            {
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Escape)) || ImGui.IsKeyPressed((int)KeyboardKey.KEY_N))
                {
                    yesNoRes = 0;
                    Exit();
                }
                else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Enter)) || ImGui.IsKeyPressed((int)KeyboardKey.KEY_Y))
                {
                    yesNoRes = 1;
                    Exit();
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
            yesNoRes = -1;
        }

        private void DrawWarning()
        {
            bool temp = true;
            if (!firstFrame)
            {
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Enter)) || ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Escape)) || ImGui.IsKeyPressed((int)KeyboardKey.KEY_KP_ENTER))
                {
                    Exit();
                }
            }
            ImGui.SetNextWindowFocus();
            Vector2 size = new Vector2(Settings.FONT_SIZE * 15, Settings.FONT_SIZE * 3);
            ImGui.SetNextWindowPos(new Vector2(Settings.SCREEN_WIDTH / 2 - size.X / 2, Settings.SCREEN_HEIGHT / 2 - size.Y / 2));
            ImGui.SetNextWindowSize(size);
            if (ImGui.Begin($"##{message}", ref temp, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal))
            {
                ImGui.TextWrapped(message);
                ImGui.End();
            }
        }

        private void Exit()
        {
            if (yesNoFunc != default(Func<int, bool>) && type == PopupType.YesNo)
            {
                var _ = yesNoFunc(yesNoRes);
            }
            else if (warnFunc != default(Func<bool>) && type == PopupType.Warning)
            {
                var _ = warnFunc();
            }
            _db._popups.Remove(this);
        }
    }
}
