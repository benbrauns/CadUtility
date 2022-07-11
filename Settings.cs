using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace CadUtility
{
    internal struct Settings
    {
        public const string FONT_PATH = "C:/Windows/Fonts/arial.ttf";
        public static int FONT_SIZE = 20;

        private static int fontSizeBuffer = 0;
        public static bool fontSizeChanged = true;
        private static bool changedBuffer = true;
        public static int SCREEN_WIDTH = 960;
        public static int SCREEN_HEIGHT = 540;
        public static float LEFT_SPACING = 150;
        public static float COLUMN_GAP = 2;
        public static float ITEM_WIDTH = 300;
        public static ImGuiStylePtr STYLE;
        public static ImFontPtr FONT;
        public static ImGuiIOPtr IO;

        public static void Initialize()
        {
            STYLE = ImGui.GetStyle();
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 255f);
            IO = ImGui.GetIO();
            SetFont();
            //ImGui.PushFont(FONT);
            /*FONT = IO.Fonts.AddFontFromFileTTF(FONT_PATH, FONT_SIZE);
            ImGui.PushFont(FONT);*/
        }

        public static void Update()
        {
            IO = ImGui.GetIO();
            LEFT_SPACING = (int)Math.Round(SCREEN_WIDTH * .15625);
            ITEM_WIDTH = (float)Math.Round(SCREEN_WIDTH * .3125);
            FONT_SIZE = (int)Math.Round((SCREEN_HEIGHT * SCREEN_WIDTH) * 0.00003858);

        }

        
        private static void SetFont()
        {
            fontSizeBuffer = FONT_SIZE;
            fontSizeChanged = false;
            IO.Fonts.Clear();
            FONT = IO.Fonts.AddFontFromFileTTF(FONT_PATH, FONT_SIZE);
        }






    }
}
