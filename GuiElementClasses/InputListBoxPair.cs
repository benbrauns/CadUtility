using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImGuiNET;
using System.Numerics;
using Raylib_cs;
using System.Text.RegularExpressions;



namespace CadUtility.GuiElementClasses
{
    internal class InputListBoxPair
    {

        public Input input;
        public ListBox list;
        private List<string> allVals;
        private List<string> allFileNames;
        public string selVal;

        private float width, leftSpacing;
        private int listItemCount;
        private string inputLabel, listLabel;
        private bool showListLabel = false;
        //private bool listFocused = false;

        private PopupListManager manager;
        private Guid id;

        public InputListBoxPair(int inputLength, List<string> AllVals, float Width, float LeftSpacing, int ListItemCount, string InputLabel, string ListLabel, PopupListManager Manager, bool ShowListLabel=false)
        {
            manager = Manager;
            id = new Guid();
            allVals = AllVals;
            input = new Input(inputLength);
            list = new ListBox(new List<string>());
            width = Width;
            leftSpacing = LeftSpacing;
            listItemCount = ListItemCount;
            inputLabel = InputLabel;
            listLabel = ListLabel;
            showListLabel = ShowListLabel;
        }

        public void Update()
        {
            if (list.clicked && list.i != list.b || (list.clicked && list.list.Count == 1))
            {
                for (int i = 0; i < input.arr.Length; i++)
                {
                    if (i < list.list[list.i].Length)
                    {
                        input.arr[i] = (byte)list.list[list.i].ToCharArray()[i];
                    }
                    else
                    {
                        input.arr[i] = (byte)"\0".ToCharArray()[0];
                    }
                }
                list.clicked = false;
                list.wasLastClick = true;   
            }
            else if (list.list.Count != allVals.Count && input.i == "")
            {
                
                list.list = allVals;
            }
            var temp = Encoding.ASCII.GetString(input.arr);
            input.Update();
            list.Update();

            
            if (input.changed)
            {
                FilterList();
            }
            selVal = GetSelectedVal();
            
        }

        private string GetSelectedVal()
        {
            list.i = Math.Clamp(list.i, 0, Math.Clamp(list.list.Count - 1, 0, 100_000));
            
            if (list.list.Count != 0)
            {
                return list.list[list.i];
            }
            else
            {
                return "";
            }
            
        }

        public void SetAllVals(List<string> AllVals)
        {
            allVals = AllVals;
        }


        private static string WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private void FilterList()
        {
            if (input.i.Contains("*") || input.i.Contains("?"))
            {
                var temp = WildCardToRegular(input.i);
                list.list = allVals.Where(v => Regex.IsMatch(v, temp)).ToList();
            }
            else
            {
                list.list = allVals.Where(v => v.Contains(input.i)).ToList();
            }
            
        }
        public void DrawPopupList(Vector2 pos)
        {
            //styling
            var style = ImGui.GetStyle();
            var colors = style.Colors;
            var padding = style.WindowPadding;
            var borderSize = ImGui.GetStyle().WindowBorderSize;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

            //actually drawing the popup
            
            ImGui.SetCursorPos(pos);
            //ImGui.SetNextWindowBgAlpha(255f);
            
            if (ImGui.BeginChild($"{id}", new Vector2(0,0), false))
            {
                ImGui.PushItemWidth(Settings.ITEM_WIDTH);
                var currentCol = colors[(int)ImGuiCol.FrameBg];

               

                //the RGB values are percentages of 255

                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.29f, 0.29f, 0.29f, 1f));
                ImGui.ListBox($"##{listLabel} - {id}", ref list.i, list.list.ToArray(), list.list.Count, Math.Clamp(list.list.Count, 0, 5));
                
                //TODO: This logic is absolutely horrendus but it mostly works
                if (ImGui.IsItemClicked())
                {
                    list.clicked = true;
                    list.wasLastClick = true;
                    input.clicked = false;
                }
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !list.clicked && !input.clicked)
                {
                    input.wasLastClick = false;
                    list.wasLastClick = false;
                }
                ImGui.PushStyleColor(ImGuiCol.FrameBg, currentCol);
                ImGui.PopItemWidth();
                ImGui.EndChild();
            }
            

            //styling
            

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, padding);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, borderSize);

        }




        public unsafe bool Draw(bool focusInput=false)
        {
            bool nextFoc = false;
            ImGuiInputTextCallback callback = (data) =>
            {
                int* p_cursor_pos = (int*)data->UserData;

                if (ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                    *p_cursor_pos = data->CursorPos;
                return 0;
            };


            var padding = ImGui.GetStyle().WindowPadding;
            var borderSize = ImGui.GetStyle().WindowBorderSize;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);


            ImGui.PushItemWidth(Settings.ITEM_WIDTH);
            ImGui.Text(inputLabel);
            ImGui.SameLine(leftSpacing);
            if (input.Draw(inputLabel, focused: focusInput))
            {
                input.active = false;
                input.wasLastClick = false;
                nextFoc = true;
            }
            else if (ImGui.IsItemActive())
            {
                nextFoc = false;
                if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_TAB))
                {
                    input.active = false;
                    input.wasLastClick = false;
                }
                else
                {
                    input.active = true;
                }
            }
            else
            {
                nextFoc = false;
                input.active = false;
            }


            if (ImGui.IsItemClicked())
            {
                input.clicked = true;
                input.wasLastClick = true;
                list.wasLastClick = false;
            }
            else
            {
                input.clicked = false;
            }

            //checking for escape input
            if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_ESCAPE))
            {
                input.active = false;
                input.wasLastClick = false;
            }
            /*if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_ENTER) && input.active)
            {
                input.active = false;
                input.wasLastClick = false;
                nextFoc = true;
            }*/

            if ((input.wasLastClick | list.clicked | input.active | list.wasLastClick | input.clicked))
            {
                if (showListLabel)
                {
                    ImGui.Text(listLabel);
                    ImGui.SameLine(leftSpacing);
                }
                else
                {
                    //ImGui.NewLine();
                }
                var currentAlpha = ImGui.GetStyle().Alpha;
                var linePadding = (ImGui.GetTextLineHeightWithSpacing() - ImGui.GetTextLineHeight());
                var currentPos = ImGui.GetCursorPos();
                var newPos = new Vector2(currentPos.X + leftSpacing, currentPos.Y - linePadding);

                manager.popups.Add(Tuple.Create(this, newPos));


                /*ImGui.SetCursorPos(newPos);


                ImGui.ListBox($"##{listLabel} - {id}", ref list.i, list.list.ToArray(), list.list.Count, Math.Clamp(list.list.Count, 0, 5));

                ImGui.SetCursorPos(currentPos);
                ImGui.NewLine();*/
            }
            ImGui.PopItemWidth();

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, padding);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, borderSize);

            return nextFoc;
        }
    }
}
