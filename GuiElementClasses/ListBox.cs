using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace CadUtility
{
    internal class ListBox
    {
        public List<string> list = new List<string>();
        //the index
        public int i = 0;
        //the index buffer
        public int b;
        public bool clicked = false;
        public bool wasLastClick = false;
        public string numClicked = string.Empty;
        public bool changed;


        Guid id;
        public ListBox(List<string> listIndexing)
        {
            id = Guid.NewGuid();
            list = listIndexing;
        }
        public void Update()
        {
            i = Math.Clamp(i, 0, Math.Clamp(list.Count - 1, 0, 100_000));
            changed = Changed();
            if (clicked && (changed || list.Count == 1))
            {
                clicked = false;
                numClicked = list[i];
            }
            
        }

        public void Draw()
        {
            ImGui.ListBox($"##{id}", ref i, list.ToArray(), list.Count);
            if (ImGui.IsItemClicked() && !clicked)
            {
                clicked = true;
            }
        }

        public bool Changed()
        {
            var beforeClamp = i;
            i = Math.Clamp(i, 0, Math.Clamp(list.Count - 1,0,100_000));

            if (beforeClamp != i)
            {
                return true;
            }

            if (list.Count == 0)
            {
                return false;
            }

            if (i != b)
            {
                b = i;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
