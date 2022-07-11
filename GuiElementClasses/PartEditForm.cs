using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using CadUtility.GuiElementClasses;
using System.Text.RegularExpressions;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace CadUtility.GuiElementClasses
{
    internal class PartEditForm
    {
        public bool open = true;
        private bool formSubmitted = false;
        private bool prevEnter = true;
        private static float leftSpacing = Settings.LEFT_SPACING;
        private string windowName;
        public Input spc = new Input(40);
        public Input cus = new Input(40);
        public Input box = new Input(40);
        public Input mat = new Input(40);
        public Input rev = new Input(40);
        public Input desc = new Input(240);
        public Input customer = new Input(40);
        public Input draw = new Input(40);
        private bool updateExisting = false;
        Database _db;

        bool inputErr = false;
        string errMess = "";

        public PartEditForm(Database DB, string SPC="", string CUS="", string BOX="", string MAT="", string REV="", string DESC="", string CUSTOMER="", string DRAW="", bool EXISTING=false)
        {
            _db = DB;
            spc.SetInput(SPC);
            cus.SetInput(CUS);
            box.SetInput(BOX);
            mat.SetInput(MAT);
            rev.SetInput(REV);
            desc.SetInput(DESC);
            customer.SetInput(CUSTOMER);
            draw.SetInput(DRAW);
            updateExisting = EXISTING;
            if (updateExisting)
            {
                windowName = $"Upate Part: {SPC}";
            }
            else
            {
                windowName = "Create New Part";
            }
        }

        public void Update()
        {
            spc.Update();
            cus.Update();
            box.Update();
            mat.Update();
            rev.Update();
            desc.Update();
            customer.Update();
            draw.Update();
        }

        private static string WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }


        public void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(Settings.SCREEN_WIDTH / 2, Settings.SCREEN_HEIGHT / 2));
            if (ImGui.Begin(windowName, ref open, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
            {
                ImGui.Text("Update Existing:");
                ImGui.SameLine(leftSpacing);
                //ImGui.Checkbox("##updateExisting", ref updateExisting);
                ImGuiInputTextFlags flags = ImGuiInputTextFlags.None;
                Vector4 spcCol = default(Vector4);
                if (updateExisting)
                {
                    flags = ImGuiInputTextFlags.ReadOnly;
                    spcCol = new Vector4(0.29f, 0.29f, 0.29f, 1f);
                }

                bool enter = spc.Draw("SPC Number: ", drawLabelText: true, flags: flags, bgColor: spcCol, focused: prevEnter);
                
                enter = cus.Draw("Customer Part: ", drawLabelText: true, focused: enter);
                enter = box.Draw("Box Number: ", drawLabelText: true, focused: enter);
                enter = mat.Draw("Material: ", drawLabelText: true, focused: enter);
                enter = rev.Draw("Revision:", drawLabelText: true, focused: enter);
                enter = desc.Draw("Description:", drawLabelText: true, focused: enter);
                prevEnter = customer.Draw("Customer Num:", drawLabelText: true, focused: enter);
                ImGui.NewLine();
                ImGui.SameLine(leftSpacing);
                ImGui.Text("Ctrl + Enter to submit");
                string mods = ImGui.GetIO().KeyMods.ToString();
                if ((ImGui.IsKeyPressed((int)KeyboardKey.KEY_ENTER) || ImGui.IsKeyPressed((int)KeyboardKey.KEY_KP_ENTER)) && (mods == "Ctrl"))
                {
                    ParseInputs();
                }
                if (inputErr)
                {
                    inputErr = false;
                    _db._popups.Add(new Popup(_db, errMess, PopupType.Warning, WarnFunc: ClearInputErr));
                }
                ImGui.End();
            }

            if (formSubmitted)
            {
                formSubmitted = false;
                _db._popups.Add(new Popup(_db, "Are you sure? This can't be undone.", PopupType.YesNo, YesNoFunc: CreatePart));
            }

            if ((!open || ImGui.IsKeyPressed((int)KeyboardKey.KEY_ESCAPE)) && !formSubmitted)
            {
                Exit();
            }

        }

        public bool ClearInputErr()
        {
            //inputErr = false;
            errMess = "";
            return true;
        }


        public bool CreatePart(int result)
        {
            if (result == 1 && !updateExisting)
            {
                _db.CreatePart(this);
            }
            else if (result == 1 && updateExisting)
            {
                _db.UpdatePart(this);
            }
            return true;
        }

        private void Exit()
        {
            _db._updateWindows.Remove(this);
        }



        private void ParseInputs()
        {
            if (!Regex.IsMatch(spc.i, WildCardToRegular("????-??????-????")))
            {
                inputErr = true;
                errMess = "Incorrect SPC Number Format";
            }
            var fileName = _db.PRODUCTION_PATH + spc.i.Replace("-","").Substring(6) + ".dc";
            if (File.Exists(fileName) && !updateExisting)
            {
                inputErr = true;
                errMess = "Part already exists with that spc Number!";
            }

            if (!inputErr)
            {
                formSubmitted = true;
            }
        }



    }
}
