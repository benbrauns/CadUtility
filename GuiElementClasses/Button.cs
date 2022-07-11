using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace CadUtility.GuiElementClasses
{
    internal class Button
    {
        string label;
        public Button(string Label)
        {
            label = Label;
        }

        public bool Draw()
        {
            if (ImGui.Button(label))
            {
                return true;
            }
            return false;
        }

    }
}
