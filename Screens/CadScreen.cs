using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CadUtility.GuiElementClasses;


namespace CadUtility
{
    class CadScreen
    {
        private const string FONT_PATH = "C:/Windows/Fonts/arial.ttf";
        public Database _db = new Database();
        public ImFontPtr font1 = new ImFontPtr();
        //public ImFontPtr newFont;
        private static bool _parsingCad = false;
        private static bool lastEnter = true;
        public bool exit = false;

        //private static bool spcInpSelected = true;

        enum Tab
        {
            General,
            Create,
            Query,
            Parsing
        }

        private Tab currTab = Tab.General;

        private static int fontSize = Settings.FONT_SIZE;
        private ImGuiIOPtr io;

        //styling variables
        private float leftSpacing = 150;
        private int buttonSize = 100;
        private float listHeight = fontSize * 4;




        public Vector3 clearColor = new Vector3(0.45f, 0.55f, 0.6f);

        public Color GetClearColor()
        {
            return new Color((byte)(clearColor.X * 255), (byte)(clearColor.Y * 255), (byte)(clearColor.Z * 255), (byte)255);
        }

        public CadScreen()
        {
            _db.Update();


            io = Settings.IO;

        }

        
        private void CheckInputs()
        {
            string mods = Settings.IO.KeyMods.ToString();

            if (currTab == Tab.General || currTab == Tab.Query)
            {
                if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_D) && mods == "Ctrl")
                {
                    _db.OpenPrint(true);

                }
                if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_T) && mods == "Ctrl")
                {
                    _db.OpenPrint(false);

                }
                //update the part
                if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_U) && mods == "Ctrl")
                {
                    bool UpdatePart(int result)
                    {
                        if (result == 1)
                        {
                            _db.UpdatePartForm();
                        }
                        return true;
                    }
                    _db._popups.Add(new Popup(_db, "Update Part?", PopupType.YesNo, YesNoFunc: UpdatePart));
                }
                if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_S) && mods == "Ctrl")
                {
                    if (_db.solidworksExists)
                    {
                        _db.OpenSld();
                    } 
                }
                if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_P) && mods == "Ctrl")
                {
                    bool written;

                    BackgroundWorker ad = new BackgroundWorker();
                    ad.WorkerReportsProgress = true;
                    //what to do in the background thread
                    //reference this question on stack overflow
                    //https://stackoverflow.com/questions/363377/how-do-i-run-a-simple-bit-of-code-in-a-new-thread
                    ad.DoWork += new DoWorkEventHandler(
                        delegate (object o, DoWorkEventArgs args)
                        {
                            BackgroundWorker b = o as BackgroundWorker;
                            var result = _db.OutputAutoDraw(_db._fileNameSel);
                            args.Result = result;
                        }
                    );

                    ad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                        delegate (object o, RunWorkerCompletedEventArgs args)
                        {
                            written = (bool)args.Result;
                            Console.WriteLine(written);
                            Console.WriteLine(Environment.CurrentDirectory);
                        }
                    );

                    ad.RunWorkerAsync();
                }
                if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_I) && mods == "Ctrl")
                {
                    bool UpdatePart(int result)
                    {
                        if (result == 1)
                        {
                            _db.UpdateSolidworksPart();
                        }
                        return true;
                    }
                    _db._popups.Add(new Popup(_db, "Update Solidworks Part?", PopupType.YesNo, YesNoFunc: UpdatePart));
                }
                
            }
            //we don't want to do this anymore
            /*if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_P))
            {
                spcInpSelected = true;
            }*/

            if (ImGui.IsKeyPressed((int)KeyboardKey.KEY_Q) && mods == "Ctrl")
            {
                if (_db.solidworksExists)
                {
                    bool CloseSolidworks(int result)
                    {
                        exit = true;
                        if (result == 1)
                        {
                            _db.sw.closeOnExit = true;
                        }
                        return true;
                    }

                    _db._popups.Add(new Popup(_db, "Close Solidworks", PopupType.YesNo, YesNoFunc: CloseSolidworks));
                }
                else
                {
                    exit = true;
                }
            }

        }

        public void SetTooltip(string tip)
        {
            if (ImGui.IsItemHovered())
            {   
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 15);
                ImGui.Text(tip);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        //this shows the entire right column of the general screeen "selected" 
        //it's nice having this as a function so we can call it elsewhere
        private void DisplaySelected()
        {
            var spacingHolder = leftSpacing;
            var widthHolder = Settings.ITEM_WIDTH;
            var changeBy = 25;
            leftSpacing -= changeBy;
            Settings.ITEM_WIDTH += changeBy;
            if (ImGui.BeginChild("##selected"))
            {
                //ImGui.PushItemWidth(Settings.ITEM_WIDTH);
                ImGui.Text("SPC Number: ");
                ImGui.SameLine(leftSpacing);
                if (ImGui.BeginChild("##spcSel", new Vector2(Settings.ITEM_WIDTH, (float)(fontSize * 1.75)), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_db._spcSel != null)
                    {
                        ImGui.Text(_db._spcSel);
                    }
                    ImGui.EndChild();
                }

                //ImGui.InputText("##readSPC", _db._spcSel, (uint)_db._spcSel.Length,ImGuiInputTextFlags.ReadOnly);


                ImGui.Text("Customer Part: ");
                ImGui.SameLine(leftSpacing);
                if (ImGui.BeginChild("##cusSel", new Vector2(Settings.ITEM_WIDTH, (float)(fontSize * 1.75)), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_db._cusSel != null)
                    {
                        ImGui.Text(_db._cusSel);
                    }
                    ImGui.EndChild();
                }
                //----
                var smallBoxWidth = Settings.ITEM_WIDTH / 4;
                var smallBoxPadding = 5;

                //----
                
                ImGui.Text("Rev # : ");
                ImGui.SameLine(leftSpacing);
                var textWidth = ImGui.GetItemRectSize().X;
                ImGui.SameLine(leftSpacing);

                //==
                if (ImGui.BeginChild("##revSel", new Vector2(smallBoxWidth * 2 + smallBoxPadding * 2, (float)(fontSize * 1.75)), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_db._revSel != null)
                    {
                        ImGui.Text(_db._revSel);
                    }
                    ImGui.EndChild();
                }
                //----
                //so that these are in the smae line
                ImGui.SameLine(leftSpacing+Settings.ITEM_WIDTH - smallBoxWidth - smallBoxPadding - textWidth);
                ImGui.Text("Cus #: ");
                ImGui.SameLine(leftSpacing + Settings.ITEM_WIDTH - smallBoxWidth);
                //==

                if (ImGui.BeginChild("##cusNumSel", new Vector2(smallBoxWidth, (float)(fontSize * 1.75)), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_db._cusNumSel != null)
                    {
                        ImGui.Text(_db._cusNumSel);
                    }
                    ImGui.EndChild();
                }
                //----

                ImGui.Text("Material: ");
                ImGui.SameLine(leftSpacing);
                if (ImGui.BeginChild("##matSel", new Vector2(smallBoxWidth * 2 + smallBoxPadding * 2, (float)(fontSize * 1.75)), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_db._matSel != null)
                    {
                        ImGui.Text(_db._matSel);
                    }
                    
                    ImGui.EndChild();
                }
                //----
                ImGui.SameLine(leftSpacing + Settings.ITEM_WIDTH - smallBoxWidth - smallBoxPadding - textWidth);
                ImGui.Text("Draw: ");
                ImGui.SameLine(leftSpacing + Settings.ITEM_WIDTH - smallBoxWidth);
                if (ImGui.BeginChild("##drawSel", new Vector2(smallBoxWidth, (float)(fontSize * 1.75)), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_db._drawSel != null)
                    {
                        ImGui.Text(_db._drawSel);
                    }
                    ImGui.EndChild();
                }
                //----




                ImGui.Text("Description: ");
                ImGui.SameLine(leftSpacing);
                if (ImGui.BeginChild("##Description", new Vector2(Settings.ITEM_WIDTH, fontSize * 4), true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (_db._descSel != null)
                    {
                        ImGui.TextWrapped(_db._descSel);
                    }
                    ImGui.EndChild();
                }
                //----
                //ImGui.PopItemWidth();

                ImGui.Text("Job Boxes:");
                ImGui.SameLine(leftSpacing);
                if (ImGui.BeginChild("#boxesList", new Vector2(Settings.ITEM_WIDTH, listHeight), true))
                {
                    foreach (var box in _db._jobBoxes)
                    {
                        ImGui.Text(box);
                    }
                    ImGui.EndChild();
                }

                ImGui.Text("Shares With:");
                ImGui.SameLine(leftSpacing);
                if (ImGui.BeginChild("##toolList", new Vector2(Settings.ITEM_WIDTH, listHeight), true))
                {
                    foreach (var tool in _db._sharesTooling)
                    {
                        ImGui.Text(tool);
                    }
                    ImGui.EndChild();
                }

                ImGui.Text("Attributes:");
                ImGui.SameLine(leftSpacing);
                if (ImGui.BeginChild("##attrList", new Vector2(Settings.ITEM_WIDTH, listHeight), true))
                {
                    foreach (var att in _db._attributes)
                    {
                        ImGui.Text(att);
                    }
                    ImGui.EndChild();
                }


                /*if (_db._prodPrintErr)
                {
                    
                    _db._prodPrintErr = Elements.PopupMessage(_db._printErr);
                }*/
                ImGui.EndChild();
            }

            Settings.ITEM_WIDTH = widthHolder;
            leftSpacing = spacingHolder;
        }

        public void Dispose()
        {
            if (_db.solidworksExists)
            {
                _db.sw.Dispose();
            }
            
        }

        public unsafe void CloseScreen()
        {
            var flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar;
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT));
            if (ImGui.Begin("Solidworks Utility", flags))
            {
                for (int i = _db._popups.Count - 1; i >= 0; i--)
                {
                    _db._popups[i].Draw();
                }
                ImGui.End();
            }
        }

        public unsafe void Update()
        {
            
           

            var flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize |ImGuiWindowFlags.NoTitleBar;
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT));
            //TODO: Figure out how to hide the tabs if the user does not have admin
            /*if (_db._accessLevel != "Test")
            {
                ImGuiWindowClass window = new ImGuiWindowClass();
                window.DockNodeFlagsOverrideSet = ImGuiDockNodeFlags.AutoHideTabBar;
               
                ImGuiWindowClassPtr windowptr = new ImGuiWindowClassPtr(&window);
                ImGui.SetNextWindowClass(windowptr);
            }*/
            if (ImGui.Begin("Solidworks Utility", flags))
            {
                //-----Update Elements-----
                CheckInputs();
                _db.Update();

                //-----Menu-----
                //I'm commenting this out for now because I don't like the way it looks
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Exit"))
                        {
                            Console.WriteLine("TODO");   
                        }
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Tools"))
                    {
                        if (ImGui.BeginMenu("ImGui"))
                        {
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                //-----Body-----
                //screen tab section
                


                if (ImGui.BeginTabBar("##Tab_Section", ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.FittingPolicyResizeDown)) {
                    //General part searching tab
                    if (ImGui.BeginTabItem("Search"))
                    {
                        currTab = Tab.General;
                        
                        ImGui.Columns(3, "##cols", false);
                        ImGui.SetColumnWidth(0, Settings.SCREEN_WIDTH / 2 - Settings.COLUMN_GAP);
                        ImGui.SetColumnWidth(1, 6);
                        ImGui.SetColumnWidth(2, Settings.SCREEN_WIDTH / 2 - Settings.COLUMN_GAP);
                        ImGui.SetColumnOffset(0, 0);
                        //-----left side of page-----
                        if (ImGui.BeginChild("##GenLeftSide", new Vector2(0,0),false))
                        {
                            /*if (spcInpSelected)
                            {
                                ImGui.SetKeyboardFocusHere();
                                spcInpSelected = false;
                            }*/
                            bool enter;
                            enter = _db.cus.Draw(focusInput: lastEnter);
                            enter = _db.spc.Draw(focusInput: enter);
                            enter = _db.box.Draw(focusInput: enter);
                            enter = _db.mat.Draw(focusInput: enter);
                            enter = _db.draw.Draw(focusInput: enter);
                            enter = _db.desc.Draw(focusInput: enter);
                            lastEnter = _db.attr.Draw(focusInput: enter);
                            //_db.customers.Draw();
                            //SetTooltip("Since the program only searches through prints, it will only show the customer number on the print");


                            ImGui.Text("Part Numbers: ");
                            ImGui.SameLine(leftSpacing);
                            _db._cusPartsFound.Draw();
                            ImGui.NewLine();
                            ImGui.SameLine(leftSpacing);
                            ImGui.Text($"Count: {_db._cusPartsFound.list.Count}");

                            /*if (ImGui.BeginChild("#boxesList", new Vector2(Settings.ITEM_WIDTH, listHeight * 3), true))
                            {
                                foreach (var file in _db._fileNames.list)
                                {
                                    ImGui.Text(file);
                                }
                                ImGui.EndChild();
                            }*/

                            //ImGui.ListBox("##Part", ref _db._fileNames.i, _db._fileNames.list.ToArray(), _db._fileNames.list.Count, 5);

                            _db.pLManager.Draw();


                            ImGui.PopItemWidth();
                            ImGui.EndChild();
                        }
                        
                        //-----Middle Spacer-----
                        ImGui.NextColumn();

                        //-----right side of page-----
                        ImGui.NextColumn();
                        DisplaySelected();


                        ImGui.Columns();
                        for (int i = _db._updateWindows.Count - 1; i >= 0; i--)
                        {
                            _db._updateWindows[i].Update();
                            ImGui.SetNextWindowFocus();
                            _db._updateWindows[i].Draw();
                        }


                        ImGui.EndTabItem();

                    }

                    if (ImGui.BeginTabItem(("Solidworks")))
                    {
                        if (ImGui.Button("Run"))
                        {
                            _db.sw.UpdateAnnotationFont();
                        }
                        ImGui.EndTabItem();
                    }


                    //Cad Parsing Tab
                    if (_db._accessLevel == "Admin")
                    {
                        if (ImGui.BeginTabItem("Parse Cad"))
                        {
                            currTab = Tab.Parsing;
                            //check boxes, just putting this in if so I can collapse
                            if (true)
                            {
                                ImGui.Text("Parse All Parts: ");
                                ImGui.SameLine(200f);
                                ImGui.Checkbox("##parseAll", ref _db._parseAll);
                                SetTooltip("If this is not checked then the program will only parse parts that have been updated since the last parse");

                                ImGui.Text("Find Shared Tools: ");
                                ImGui.SameLine(200f);
                                ImGui.Checkbox("##findShared", ref _db._findShared);

                                ImGui.Text("Find Job Boxes: ");
                                ImGui.SameLine(200f);
                                ImGui.Checkbox("##findJobBoxes", ref _db._findJobBoxes);

                                ImGui.Text("Find Attributes: ");
                                ImGui.SameLine(200f);
                                ImGui.Checkbox("##findAttributes", ref _db._findAttributes);


                                //all of the attributes
                                if (_db._findAttributes && ImGui.BeginChild("##attributes", new Vector2(Settings.ITEM_WIDTH, fontSize * 6), true))
                                {

                                    ImGui.Text("Find Thread: ");
                                    ImGui.SameLine(200f);
                                    ImGui.Checkbox("##findThread", ref _db._findThread);

                                    ImGui.Text("Find Hardness: ");
                                    ImGui.SameLine(200f);
                                    ImGui.Checkbox("##findHardness", ref _db._findHardness);

                                    ImGui.Text("Parse Description: ");
                                    ImGui.SameLine(200f);
                                    ImGui.Checkbox("##parseDescription", ref _db._parseDescription);
                                    SetTooltip("This is useful for finding part attributes like thread size");
                                    ImGui.EndChild();
                                }


                                ImGui.Text("Delete Database: ");
                                ImGui.SameLine(200f);
                                ImGui.Checkbox("##deleteDatabase", ref _db._deleteDatabase);
                            }

                            // this is just spacing because it looks better
                            ImGui.NewLine();
                            ImGui.NewLine();
                            ImGui.NewLine();



                            //lines up the button with the check boxes
                            ImGui.SameLine((float)(buttonSize + fontSize + (Settings.STYLE.FramePadding.Y * 2)));

                            //ImGui.PushStyleColor()
                            if (ImGui.Button("Run Parse", new Vector2(buttonSize, 30)) && !_parsingCad)
                            {
                                _parsingCad = true;
                                _db.startTime = DateTime.Now;
                                if (_db._deleteDatabase)
                                {
                                    _db.DeleteDatabase();
                                }


                                BackgroundWorker bw = new BackgroundWorker();
                                bw.WorkerReportsProgress = true;
                                //what to do in the background thread
                                //reference this question on stack overflow
                                //https://stackoverflow.com/questions/363377/how-do-i-run-a-simple-bit-of-code-in-a-new-thread
                                bw.DoWork += new DoWorkEventHandler(
                                    delegate (object o, DoWorkEventArgs args)
                                    {
                                        BackgroundWorker b = o as BackgroundWorker;
                                        _db.ParseParts(bw);

                                    }
                                );

                                bw.ProgressChanged += new ProgressChangedEventHandler(
                                    delegate (object o, ProgressChangedEventArgs args)
                                    {
                                        _db._parsingProgress = (float)args.UserState;
                                    }
                                );

                                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                                    delegate (object o, RunWorkerCompletedEventArgs args)
                                    {
                                        _parsingCad = false;
                                        _db._currentParseFile = "";
                                    }
                                );

                                bw.RunWorkerAsync();

                            }


                            ImGui.ProgressBar(_db._parsingProgress, new Vector2((int)((buttonSize * 2) + fontSize + (Settings.STYLE.FramePadding.Y) - Settings.STYLE.FramePadding.X), 20));
                            if (_parsingCad && _db._parsingProgress > 0.001)
                            {
                                var timeElapsed = (DateTime.Now - _db.startTime).Duration();
                                var timeLeft = (1/_db._parsingProgress) * timeElapsed;
                                ImGui.Text($"Current File: {_db._currentParseFile}");
                                //TODO: this is bugged and needs to be fixed
                                ImGui.Text($"Est. Time Remaining: {Math.Round(timeLeft.TotalMinutes, 0)} Minutes");
                            }
                            
                            ImGui.EndTabItem();
                        }
                    }
                    
                    
                    ImGui.EndTabBar();
                }
                
                for (int i = _db._popups.Count - 1; i >= 0; i--)
                {
                    _db._popups[i].Draw();
                }

                /*float framerate = ImGui.GetIO().Framerate;
                ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");*/

            }

                
            
        }



    }
}
