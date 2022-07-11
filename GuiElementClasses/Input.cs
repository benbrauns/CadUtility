using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Numerics;
using Raylib_cs;



namespace CadUtility
{
    internal class Input
    {
        private Guid id;
        //the byte array for the search box
        public byte[] arr;
        //the input as a string
        public string i;
        //the input string buffer
        public string b;
        //per the previous called Updated did the input change
        public bool changed = false;
        public bool clicked = false;
        public bool wasLastClick = false;
        public bool active = false;
        public int length;
        public int cursorPos = 0;
        private int cursorPosBuf = 0;



        public Input(int Length, bool Multiline=false)
        {
            id = Guid.NewGuid();
            length = Length;
            arr = new byte[Length];
        }

        public void Update()
        {
            if (cursorPos != cursorPosBuf)
            {
                ClearArr(cursorPos);
                cursorPosBuf = cursorPos;  
            }
            i = GetString();
            if (i != b)
            {
                changed = true; b = i;
            }
            else
            {
                changed = false;
            }
            
        }


        public unsafe bool Draw(string label, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, bool drawLabelText = false, Vector4 bgColor = default(Vector4), bool focused=false)
        {
            bool result;
            Vector4 currentCol = default(Vector4);
            if (drawLabelText)
            {
                ImGui.Text(label);
                ImGui.SameLine(Settings.LEFT_SPACING);
            }
            if (bgColor != default(Vector4))
            {
                currentCol = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
                ImGui.PushStyleColor(ImGuiCol.FrameBg, bgColor);
            }


            ImGuiInputTextCallback callback = (data) =>
            {
                int* p_cursor_pos = (int*)data->UserData;

                if (ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                    *p_cursor_pos = data->CursorPos;
                return 0;
            };
            int cursPos = -1;
            if (focused)
            {
                ImGui.SetKeyboardFocusHere();
            }
            result = ImGui.InputText($"##{label} - {id}", arr, (uint)arr.Length, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways | flags, callback, (IntPtr)(&cursPos));
            bool active = ImGui.IsItemActive();
            if (bgColor != default(Vector4))
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, currentCol);
            }


            if (cursPos > -1)
            {
                cursorPos = cursPos;
            }
            if (focused)
            {
                return false;
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_ENTER) && active)
            {
                if (Raylib.GetKeyPressed() == 335)
                {
                    return true;
                }

            }



            
            return result;
        }


        public void ClearArr(int startIndex=0)
        {
            for (int i = startIndex; i < length; i++)
            {
                arr[i] = Encoding.ASCII.GetBytes("\0").ToArray()[0];
            }
        }

        public void SetInput(string value)
        {
            if (value == null)
            {
                value = "";
            }
            for (int i = 0;i < length; i++)
            {
                if (i < value.Length)
                {
                    arr[i] = (byte)value[i];
                }
                else
                {
                    arr[i] = Encoding.ASCII.GetBytes("\0").ToArray()[0];
                }
            }
        }

        private string GetString()
        {
            return Encoding.ASCII.GetString(arr).Replace("\0", "").ToUpper();
        }

    }
}
