using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.ComponentModel;
using ImGuiNET;
using System.Diagnostics;
using CadUtility.GuiElementClasses;

namespace CadUtility
{
    [Table("Parts")]
    public class Part
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }
        [Unique]
        [Column("File_Name")]
        [DefaultValue("")]
        public string fileName { get; set; }
        [DefaultValue("")]
        public string spcNumber { get; set; }
        [DefaultValue("")]
        public string description { get; set; }
        [DefaultValue("")]
        public string revision { get; set; }
        [DefaultValue("")]
        public string material { get; set; }
        [DefaultValue("")]
        public string drawSize { get; set; }
        [DefaultValue("")]
        public string customerNumber { get; set; }
        public DateTime lastUpdated { get; set; }
    }
    [Table("Customer_Parts")]
    public class CustomerPart
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }

        [Column("Customer_Part_Number")]
        [DefaultValue("")]
        public string customerPartNum { get; set; }

        [ForeignKey(typeof(Part))]
        [DefaultValue("")]
        [Column("File_Name")]
        public string fileName { get; set; }
    }
    [Table("Job_Boxes")]
    public class JobBox
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }
        [Column("Box_Number")]
        [DefaultValue("")]
        public string boxNum { get; set; }
        [ForeignKey(typeof(Part))]
        [Column("File_Name")]
        [DefaultValue("")]
        public string fileName { get; set; }
    }
    [Table("Shared Tooling")]
    public class SharedTool
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }
        [Column("File Name")]
        [DefaultValue("")]
        public string fileName { get; set; }
        [Column("Shared Customer Part Number")]
        [DefaultValue("")]
        public string sharedCusNum { get; set; }
    }

    [Table("Part_Attributes")]
    public class PartAttribute
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }
        [Column("Type")]
        [DefaultValue("")]
        public string type { get; set; }
        [Column("Value")]
        [DefaultValue("")]
        public string value { get; set; }

        [ForeignKey(typeof(Part))]
        [Column("File Name")]
        [DefaultValue("")]
        public string fileName { get; set; }
    }
    public class Information
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }
        [Column("user")]
        [DefaultValue("")]
        public string user { get; set; }
        [Column("Start Date/Time")]
        public DateTime startTime { get; set; }
        [Column("End Date/Time")]
        public DateTime endTime { get; set; }
    }
    [Table("Update_History")]
    public class UpdateHistory
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }
        [Column("user")]
        [DefaultValue("")]
        public string user { get; set; }
        [Column("Start_Date-Time")]
        public DateTime startTime { get; set; }
        [Column("End_Date-Time")]
        public DateTime endTime { get; set; }
        [Column("Files_Updated")]
        public int updatedCount { get; set; }
    }
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int id { get; set; }
        [Column("Username")]
        [DefaultValue("")]
        public string username { get; set; }
        [Column("Access Level")]
        [DefaultValue("User")]
        public string accessLevel { get; set; }
        [Column("Access Count")]
        public int accessCount { get; set; }
        [Column("Last Used")]
        public DateTime lastUsed { get; set; }
    }

    internal class Database
    {
        



        //the locations of the files
        public string PRODUCTION_PATH = "F:/DC/SPC/";
        private string TOOLING_PATH = "F:DC/TOOLING/";
        private string TOOLING2_PATH = "F:DC/TOOLING2/";
        private string TOOLING3_PATH = "F:DC/TOOLING3/";
        private string[] TOOL_PATHS;
        private string POSSIBLE_BOXES = "F:/DC/Find/ParsingSupport/job_boxes.json";
        private string CAD_PATH = "C:/Program Files (x86)/DesignCAD 3000/Dcad3000.exe";
        private string SLD_PATH = "C:/Program Files/SOLIDWORKS Corp/SOLIDWORKS/SLDWORKS.exe";

        public SQLiteConnection _db;
        private string LOCATION = "F:/DC/Find/ParsingSupport/database/parts.db";
        //this is the float used to updated the progress bar on the parse page
        public float _parsingProgress;

        //bools to handle what we will search in parsing
        public bool _findShared, _parseAll, _findJobBoxes = false;
        public bool _deleteDatabase = false;
        public bool _findAttributes = false;
        public bool _parseDescription = true;
        public bool _findHardness = true;
        public bool _findThread = true;
        public bool _findScrewType = true;

        public bool _prodPrintErr = false;
        public string _printErr;
        public bool _sldPrintErr = false;
        public string _sldErr;
        public bool solidworksExists = false;





        public byte[] _boxSearch;
        public string _descSel = "";
        public string _cusSel = "";
        public string _spcSel = "";
        public string _revSel = "";
        public string _cusNumSel = "";
        public string _matSel = "";
        public string _drawSel = "";
        public string _accessLevel;

        public string _fileNameSel;

        public List<string> _attributes = new List<string>();
        public List<string> _jobBoxes = new List<string>();
        public List<string> _sharesTooling = new List<string>();
        public byte[] test = new byte[40];

        //database lists
        private List<CustomerPart> _allCusParts = new List<CustomerPart>();
        private List<Part> _allSpcParts = new List<Part>();
        private List<SharedTool> _allTools = new List<SharedTool>();
        private List<PartAttribute> _allAttributes = new List<PartAttribute>();
        public List<JobBox> _allBoxes = new List<JobBox>();
        public string _currentParseFile = "";

        public Dictionary<string, List<string>> _attrDict = new Dictionary<string, List<string>>();

        public Solidworks sw;


        public DateTime startTime = new DateTime();

        private bool _queryChanged = false;

        public ListBox _boxQuery = new ListBox(new List<string>());
        //public ListBox _spcNums = new ListBox(new List<string>());
        //public ListBox _cusNums = new ListBox(new List<string>());
        //public ListBox _boxNums = new ListBox(new List<string>());
        public ListBox _cusPartsFound = new ListBox(new List<string>());
        
        public List<PartEditForm> _updateWindows = new List<PartEditForm>();
        public List<Popup> _popups = new List<Popup>();

        public Dictionary<string, Input> create = new Dictionary<string, Input>();
        public Button createButton = new Button("Create");

        private bool refresh = false;
        public PopupListManager pLManager = new PopupListManager();
        public InputListBoxPair spc;
        public InputListBoxPair cus;
        public InputListBoxPair box;
        public InputListBoxPair mat;
        public InputListBoxPair customers;
        public InputListBoxPair draw;
        public InputListBoxPair attr;
        public InputListBoxPair desc;



        //public List<string> _boxNums = new List<string>();
        public List<string> _matchingParts = new List<string>(); 
        private Filter _filter;
        public Database()
        {
            //making sure that solidworks is present on this computer before loading it. if it is not, then it disables all solidworks functions
            if (File.Exists(SLD_PATH))
            {
                solidworksExists = true;
                sw = new Solidworks();
            }



            CreateDatabase();
            _boxSearch = new byte[40];
            spc = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "SPC Number: ", "Results", pLManager);
            cus = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "Customer Part: ", "Results: ", pLManager);
            box = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "Job Boxes: ", "Results: ", pLManager);
            mat = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "Raw Material: ", "Results: ", pLManager);
            customers = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "Customer Num: ", "Results: ", pLManager);
            draw = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "Draw Size: ", "Results: ", pLManager);
            attr = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "Attributes: ", "Results: ", pLManager);
            desc = new InputListBoxPair(40, new List<string>(), 300f, 150f, 5, "Description: ", "Results: ", pLManager);
            

            GetLists();

            SetPaths();

            _accessLevel = StartUser();
        }


        //this gets all the records and stores them in lists so I don't have to call the database
        //so much, hopefully this brings down the network traffic and later will allow me to only
        //open the database when absolutely needed
        private void GetLists()
        {
            refresh = true;
            _allCusParts = _db.Table<CustomerPart>().ToList();
            //_allCusParts.Sort();
            cus.SetAllVals(_allCusParts.Where(v => v.customerPartNum != null).Select(v => v.customerPartNum).ToList());

            _allSpcParts = _db.Table<Part>().ToList();
            //_allSpcParts.Sort();
            spc.SetAllVals(_allSpcParts.Where(v => v.spcNumber != null).Select(v => v.spcNumber).ToList());
            var temp = _allSpcParts.Where(v => v.material != null).Select(v => v.material).Where(v => v.Length == 16).Distinct().ToList();
            temp.Sort();
            mat.SetAllVals(temp);
            temp = _allSpcParts.Where(v => v.customerNumber != null).Select(v => v.customerNumber).Where(v => v.Length > 3).Distinct().ToList();
            temp.Sort();
            customers.SetAllVals(temp);
            temp = _allSpcParts.Where(v => v.drawSize != null && v.drawSize.Contains(".")).Select(v => v.drawSize.Substring(v.drawSize.IndexOf(".")).Replace(" ", "")).Where(v => v.Length > 3).Distinct().ToList();
            temp.Sort();
            draw.SetAllVals(temp);
            temp = _allSpcParts.Where(v => v.description != null).Select(v => v.description).Where(v => v.Length > 3).Distinct().ToList();
            desc.SetAllVals(temp);

            _allTools = _db.Table<SharedTool>().ToList();

            _allAttributes = _db.Table<PartAttribute>().ToList();
            temp = _allAttributes.Where(v => v.value != null).Select(v => v.value).Distinct().ToList();
            temp.Sort();
            attr.SetAllVals(temp);

            _allBoxes = _db.Table<JobBox>().ToList();
            temp = _allBoxes.Where(v => v.boxNum != null).Select(v => v.boxNum).Distinct().ToList();
            temp.Sort();
            box.SetAllVals(temp);

            _attrDict.Clear();
            foreach (var attr in _allAttributes.Select(v => v.type).Distinct())
            {
                var values = _allAttributes.Where(v => v.type == attr).Select(v => v.value).Distinct().ToList();
                _attrDict.Add(attr, values);
            }

            return;
        }


        //this sets the paths of files depending on whether the program is running from the
        //network or from a person's computer
        private void SetPaths()
        {
            if (!Directory.GetCurrentDirectory().Contains("C:"))
            {
                PRODUCTION_PATH = "../../SPC/";
                TOOLING_PATH = "../../TOOLING/";
                TOOLING2_PATH = "../../TOOLING2/";
                TOOLING3_PATH = "../../TOOLING3/";
                POSSIBLE_BOXES = "../ParsingSupport/job_boxes.json";
                LOCATION = "../ParsingSupport/database/parts.db";
            }
            TOOL_PATHS = new string[] { TOOLING_PATH, TOOLING2_PATH, TOOLING3_PATH };
        }


        private void GetQueryResults()
        {

            if (_queryChanged)
            {

            }

        }

        //storing part attributes like thread sizes
        //TODO: add hex sizes, torx sizes, and think of other things to add
        public struct Attributes
        {
            public static string[] Threads = new string[]
            {
                //standard
                "0-80",
                "1-64",
                "1-72",
                "2-56",
                "2-64",
                "3-48",
                "3-56",
                "4-40",
                "4-48",
                "5-40",
                "5-44",
                "6-32",
                "6-40",
                "8-32",
                "8-36",
                "10-24",
                "10-32",
                "12-24",
                "12-28",
                "1/4-20",
                "1/4-28",
                "5/16-18",
                "5/16-24",
                "3/8-16",
                "3/8-24",
                "7/16-14",
                "7/16-20",
                "1/2-13",
                "1/2-20",
                "9/16-12",
                "9/16-18",
                "5/8-11",
                "5/8-18",
                "3/4-10",
                "3/4-16",
                "7/8-9",
                "7/8-14",
                //"1-8",
                //"1-12",
        
                //metric
                "M1.6x0.35",
                "M2X0.4",
                "M2.5X0.45",
                "M3X0.5",
                "M3.5X0.6",
                "M4X0.7",
                "M5X0.8",
                "M6X1.0",
                "M7X1.0",
                "M8X1.25",
                "M8X1.0",
                "M10X1.5",
                "M10X1.25",
                "M121.75",
                "M12X1.25",
                "M14X2.0",
                "M14X1.5",
                "M16X2.0",
                "M16X1.5",
                "M18X2.5",
                "M20X2.5",
                "M22X2.5",
                "M24X3.0",
                "M27X3.0",
            };
        }


        //this is the function that will handle parsing all of the files from within the program
        public void ParseParts(BackgroundWorker bw)
        {
            _db = new SQLiteConnection(LOCATION);
            DateTime start = DateTime.Now;
            
            //this is the function we call to update all the parts
            void UpdateParts()
            {
                _db = new SQLiteConnection(LOCATION);
                //make sure we have the most up to date lists when we check against them
                GetLists();
                string[] files = Directory.GetFiles(PRODUCTION_PATH);
                Dictionary<string, List<string>> possibleBoxes = GetPossibleNumbers(files);
                int i = 0;
                int updatedCount = 0;
                foreach (string file in files)
                {
                    /*if (i > 500)
                    {
                        break;
                    }*/
                    i++;
                    string fileName = file[(file.LastIndexOf("/") + 1)..].ToUpper();
                    _currentParseFile = fileName;
                    //this reports back the progress of the parsing so that I can get the progress
                    //bar
                    bw.ReportProgress(0, ((float)i / (float)files.Length));

                    //eliminating parts that are't worth checking
                    if (!file.Contains('.'))
                    {
                        continue;
                    }
                    FileInfo info = new FileInfo(file);
                    //makes sure its a .dc file
                    if (IsFileinUse(info) || !file.ToLower().Contains(".dc"))
                    {
                        continue;
                    }
                    
                    
                    Part part = new Part();
                    //var parts = _allSpcParts.Where(v => v.fileName == fileName);

                    //checks to see if the record exists and if not it creates it
                    if (_allSpcParts.Where(v => v.fileName == fileName).Count() == 0)
                    {
                        var newPart = new Part
                        {
                            fileName = fileName,
                            lastUpdated = info.LastWriteTime
                        };
                        var test = _db.Insert(newPart);
                        part = _db.Table<Part>().Where(v => v.fileName == fileName).FirstOrDefault();
                    }
                    //checks to see if the record has changed since the last time we created it
                    else
                    {
                        part = _allSpcParts.Where(v => v.fileName == fileName).FirstOrDefault();
                        if (part.lastUpdated >= info.LastWriteTime && !_parseAll)
                        {
                            //if it has not changed than we can just skip it which saves a lot of computing time
                            continue;
                        }
                        else
                        {
                            UpdatePartTime(part, info);
                        }
                    }

                    updatedCount++;

                    //getting all the lines of the file
                    IEnumerable<string> lines = File.ReadAllLines(file).Where(v => v.Contains("Arial"));
                    string all = string.Join("", lines);
                    //setting up needed variables for parsing
                    string customerPart = "";
                    string spcNumber = "";
                    string cusNumber = "";
                    string revNumber = "";
                    string matNumber = "";
                    string drawSize = "";
                    bool custPartFound = false;
                    bool spcFound = false;
                    bool desc1Found = false;
                    bool desc2Found = false;
                    bool cusFound = false;
                    bool revFound = false;
                    bool matFound = false;
                    bool drawFound = false;

                    string[] description = new string[2];
                    


                    //finding names within the file
                    foreach (string line in lines)
                    {
                        //all text lines include Arial so we can skip any that dont
                        /*if (!line.Contains("Arial"))
                        {
                            continue;
                        }*/


                        //customer part number
                        if (!custPartFound)
                        {
                            if (line.Contains("g\0h\0\u0010\u000e\u0004\0������H@�G�z\u0014�I@"))
                            {
                                custPartFound = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int length = 50;
                                customerPart = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();

                                if (_allCusParts.Where(v => v.fileName == fileName).Where(v => v.customerPartNum == customerPart).Count() == 0)
                                {
                                    var newPart = new CustomerPart
                                    {
                                        customerPartNum = customerPart,
                                        fileName = fileName,
                                    };
                                    _db.Insert(newPart);
                                }
                            }
                        }

                        //spcNumber
                        if (!spcFound)
                        {
                            if (line.Contains("������C@������J@") || line.Contains("��h@���,��p@"))
                            {
                                spcFound = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int length = 50;
                                spcNumber = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();
                                UpdateSPCNum(part, spcNumber);
                            }
                        }

                        //looking for customer number
                        if (!cusFound)
                        {
                            if (line.Contains("g\0h\0\u0010\u000e\u0004\0\0\0\0\0\0\0D@�G�z\u0014�I@"))
                            {
                                cusFound = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int length = 50;
                                cusNumber = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();
                                //UpdateCusNum(part, cusNumber);
                                part.customerNumber = cusNumber;
                            }
                        }

                        if (!drawFound)
                        {
                            if (line.Contains("\0h\0\u0010\u000e\u0004\0ffffffR@������H@"))
                            {
                                drawFound = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int length = 50;
                                drawSize = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();
                                part.drawSize = drawSize;
                            }
                        }


                        //looking for revision
                        if (!revFound)
                        {
                            if (line.Contains("g\0h\0\u0010\u000e\u0004\0333333P@{\u0014�G�zI@"))
                            {
                                revFound = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int length = 50;
                                revNumber = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();
                                //UpdateRevNum(part, revNumber);
                                part.revision = revNumber;
                            }
                        }

                        //looking for material
                        if (!matFound)
                        {
                            if (line.Contains("\0h\0\u0010\u000e\u0004\0������D@������H@"))
                            {
                                matFound = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int length = 50;
                                matNumber = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();
                                part.material = matNumber;
                            }
                        }

                        //first line of the description
                        if (!desc1Found)
                        {
                            if (line.Contains("������C@ffffffL@"))
                            {
                                desc1Found = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int secondSpot = line.IndexOf("\u0014400");
                                int length = secondSpot - firstSpot;
                                description[0] = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();
                            }
                        }
                        //second line of the description
                        if (!desc2Found)
                        {
                            if (line.Contains("������C@ףp="))
                            {
                                desc2Found = true;
                                int firstSpot = line.IndexOf("I\0") + 2;
                                int secondSpot = line.IndexOf("\u0014400");
                                int length = secondSpot - firstSpot;
                                description[1] = line.Substring(firstSpot, length).Replace("\u0014", "").Trim();
                            }
                        }

                        if (desc1Found && desc2Found)
                        {
                            if (description[0] != null)
                            {
                                UpdateDescription(part, description);
                            }
                        }

                        if (custPartFound && spcFound && desc1Found && desc2Found && cusFound && revFound && matFound && drawFound)
                        {
                            break;
                        }
                    }

                    //updating the part in the database with what was found
                    var results = _db.Update(part);

                    //where the program parses for attributes
                    if (_findAttributes)
                    {
                        string fullDesc = (description[0] + description[1]).ToUpper();
                        if (_findScrewType)
                        {
                            if (fullDesc.Contains("SHLD") || fullDesc.Contains("SHOULDER"))
                            {
                                AddAttribute(part, "Type", "Shoulder Screw");
                            }
                            else if (fullDesc.Contains("CAP"))
                            {
                                AddAttribute(part, "Type", "Cap Screw");
                            }
                        }

                        //finds the thread information in the print
                        if (_findThread)
                        {
                            List<string> foundThreads = new List<string>();
                            foreach (string thread in Attributes.Threads)
                            {
                                string raw = File.ReadAllText(file).Replace(" ", "");
                                if (all.Contains(thread) && !foundThreads.Contains(thread))
                                {
                                    foundThreads.Add(thread);
                                }
                            }
                            if (foundThreads.Count > 0)
                            {
                                UpdateThread(part, foundThreads);
                            }
                        }

                        //Parses the description for part attributes
                        if (_parseDescription && description[0] != null && part.fileName != null)
                        {
                            
                            //Hex Flange
                            if (fullDesc.Contains("HEX FLANGE"))
                            {
                                AddAttribute(part, "Head", "Hex Flange");
                            }
                        }
                        //parses the entire file for the hardness
                        if (_findHardness)
                        {
                            var allRep = all.Replace(" ", "");
                            string type = "Hardness";
                            if (allRep.Contains("GRADE8.8"))
                            {
                                AddAttribute(part, type, "Grade 8.8");
                            }
                            else if (allRep.Contains("GRADE8"))
                            {
                                AddAttribute(part, type, "Grade 8");
                            }
                            else if (allRep.Contains("GRADE10.9"))
                            {
                                AddAttribute(part, type, "GRADE 10.9");
                            }
                            else if (allRep.Contains("GRADE5"))
                            {
                                AddAttribute(part, type, "GRADE 5");
                            }
                            else if (allRep.Contains("GRADE12.9"))
                            {
                                AddAttribute(part, type, "GRADE 12.9");
                            }
                            else if (allRep.Contains("GRADE2"))
                            {
                                AddAttribute(part, type, "GRADE 2");
                            }
                        }
                    }

                    //finds shared tooling
                    if (_findShared && fileName != null)
                    {
                        FindSharedTooling(fileName);
                    }


                    //im passing the cusotmer part number because some customer part numbers have box numbers in them
                    List<string> boxes;
                    if (_findJobBoxes)
                    {
                        if (custPartFound)
                        {
                            boxes = FindJobBoxes(file, lines, customerPart);
                        }
                        else
                        {
                            boxes = FindJobBoxes(file, lines);
                        }
                        UpdateJobBoxes(part, boxes);
                    }
                    
                    //gets the job boxes found in this part
                }
                LogUser(updatedCount);
                //Console.WriteLine($"{updatedCount} file(s) were updated");
                _parsingProgress = 0;
                //since we have parsed new values we need to update the lists to reflect that
                GetLists();
            }

           void FindSharedTooling(string fileName)
            {
                List<string> shared = new List<string>();
                var selfRecs = _allCusParts.Where(v => v.fileName == fileName).ToList();
                if (selfRecs.Count == 0)
                {
                    return;
                }
                var self = selfRecs.Select(v => v.customerPartNum).ToList().FirstOrDefault();


                if (!File.Exists(TOOLING_PATH+fileName))
                {
                    return;
                }

                string all = File.ReadAllText(TOOLING_PATH+fileName);
                var custPartNumbers = _allCusParts.Where(v => v.customerPartNum != "").ToList().Select(v => v.customerPartNum);

                var currRecs = _allTools.Where(v => v.fileName == fileName).ToList().Select(v => v.sharedCusNum).ToList();

                //find all the part numbers 
                foreach (var part in custPartNumbers)
                {
                    //this makes sure we don't get duplicate entries
                    if (currRecs.Contains(part) || part == self || part.Length < 5)
                    {
                        continue;
                    }
                    if (all.Contains(part))
                    {
                        shared.Add(part);
                    }
                }

                //add the new records to the database
                foreach (var part in shared)
                {
                    SharedTool newShare = new SharedTool()
                    {
                        fileName = fileName,
                        sharedCusNum = part,
                    };
                    var results = _db.Insert(newShare);
                }
                
                //this removes all the values we haave already found


            }


            //this gets all possible job box numbers as generated by the python script.
            //TODO: make the job box number generation apart of this program
            Dictionary<string, List<string>> GetPossibleNumbers(string[] files)
            {
                Dictionary<string, List<string>> numbers = new Dictionary<string, List<string>>();
                string raw = File.ReadAllText(POSSIBLE_BOXES);

                numbers = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(raw);
                return numbers;
            }

            //updates the threads found for the part
            void UpdateThread(Part part, List<string> threads)
            {
                foreach (string thread in threads)
                {
                    var found = _allAttributes.Where(v => v.fileName == part.fileName).Where(v => v.value == thread);
                    if (found.Count() == 0)
                    {
                        PartAttribute newAt = new PartAttribute
                        {
                            fileName = part.fileName,
                            type = "Thread",
                            value = thread
                        };
                        var results = _db.Insert(newAt);
                    }
                }

            }

            //a general function for creating attributes
            void AddAttribute(Part part, string type, string value)
            {
                var found = _allAttributes.Where(v => v.fileName == part.fileName).Where(v => v.value == value && v.type == type);
                if (found.Count() == 0)
                {
                    PartAttribute newAt = new PartAttribute
                    {
                        fileName = part.fileName,
                        type = type,
                        value = value,
                    };
                    var results = _db.Insert(newAt);
                }
            }

            //updates the SPC number for a given part
            void UpdateSPCNum(Part part, string spcNum)
            {
                part.spcNumber = spcNum;
                //Part updated = new Part { id = part.id, spcNumber = spcNum };
                var results = _db.Update(part);
                //_db.Execute($"UPDATE Parts SET spcNumber = {spcNum} WHERE id = {part.id}");
            }

            void UpdateCusNum(Part part, string cusNum)
            {
                part.customerNumber = cusNum;
                var results = _db.Update(part);
            }

            void UpdateRevNum(Part part, string revNum)
            {
                part.revision = revNum;
                var result = _db.Update(part);
            }

            void UpdateMatNum(Part part, string matNum)
            {
                part.material = matNum;
                var results = _db.Update(part);
            }

            //updates the Description for a given part
            void UpdateDescription(Part part, string[] description)
            {
                string joined = String.Join(" ", description);
                part.description = joined;
                var results = _db.Update(part);
            }
            
            void UpdatePartTime(Part part, FileInfo info)
            {
                part.lastUpdated = info.LastWriteTime;
                var results = _db.Update(part);
            }

            //logs when a user parses the files
            void LogUser(int count)
            {
                var newLog = new UpdateHistory
                {
                    user = Environment.UserName,
                    startTime = start,
                    endTime = DateTime.Now,
                    updatedCount = count,
                };
                var results = _db.Insert(newLog);
            }


            //finds the job boxes for a given file path
            List<string> FindJobBoxes(string file, IEnumerable<string> lines, string customerPartNum = "")
            {
                List<string> boxes = new List<string>();
                string rawFile = string.Join("", lines);
                Dictionary<string, List<string>> possibleBoxes = GetPossibleBoxNums();

                //this will go through and find all the boxes but we could get duplicates from partial
                //box numbers (example: ML-12 and ML-120 with ML-120 being the actual box we want)
                foreach (KeyValuePair<string, List<string>> category in possibleBoxes)
                {
                    if (rawFile.Contains(category.Key))
                    {
                        foreach (string box in category.Value)
                        {
                            if (customerPartNum != "" && customerPartNum.Contains(box))
                            {
                                continue;
                            }
                            if (rawFile.Contains(box))
                            {
                                if (box == "01-1005")
                                {
                                    var temp = 1;
                                }
                                boxes.Add(box);
                            }
                        }
                    }
                }

                //this will sort out the partials
                List<string> boxesCopy = new List<string>(boxes);
                foreach (string box in boxesCopy)
                {
                    foreach (string box2 in boxesCopy)
                    {
                        if (box2.Contains(box) && box2 != box)
                        {
                            boxes.Remove(box);
                        }
                    }
                    // if a customer part number was passed then we want to make sure the box isn't in that
                    if (customerPartNum != "")
                    {
                        if (customerPartNum.Contains(box))
                        {
                            boxes.Remove(box);
                        }
                    }
                }

                return boxes;
            }

            //gets the dictionary of possible box numbers taht we generated with python.
            //TODO incorporate this to c# instead of loading from JSON. I don't want this to need
            //any external support files and it's not a very time expensive task to do. 
            Dictionary<string, List<string>> GetPossibleBoxNums()
            {
                Dictionary<string, List<string>> numbers = new Dictionary<string, List<string>>();
                string raw = File.ReadAllText(POSSIBLE_BOXES);

                numbers = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(raw);
                return numbers;
            }


            //Checks whether or not someone is using a file currently
            

            UpdateParts();
            
        }
        void UpdateJobBoxes(Part part, List<string> jobBoxes, bool deleteMissing=false)
        {
            var currentBoxes = _allBoxes.Where(v => v.fileName == part.fileName).Select(v => v.boxNum).ToList();


            if (deleteMissing)
            {
                bool deleted = false;
                foreach (var box in currentBoxes)
                {
                    if (!jobBoxes.Contains(box))
                    {
                        var boxRec = _allBoxes.Where(v => v.boxNum == box && v.fileName == part.fileName).FirstOrDefault();
                        _db.Delete(boxRec);
                        deleted = true;
                    }
                }
                if (deleted)
                {
                    var temp = _db.Table<JobBox>().Where(v => v.fileName == part.fileName);
                    currentBoxes = temp.Select(v => v.boxNum).ToList();
                } 
            }



            //adds the new records
            foreach (string box in jobBoxes)
            {
                if (currentBoxes.Contains(box))
                {
                    continue;
                }
                var newBox = new JobBox
                {
                    boxNum = box,
                    fileName = part.fileName
                };
                var results = _db.Insert(newBox);
            }
        }
        public void OpenPrint(bool production=true)
        {
            if (production)
            {
                if (File.Exists(PRODUCTION_PATH + _fileNameSel))
                {
                    if (!IsFileinUse(new FileInfo(PRODUCTION_PATH + _fileNameSel)))
                    {
                        Process.Start(CAD_PATH, PRODUCTION_PATH + _fileNameSel);
                    }
                    else
                    {
                        _prodPrintErr = true;
                        _printErr = "File is in use";
                    }
                }
                else
                {
                    _prodPrintErr = true;
                    _printErr = "Production print does not exist";
                    return;
                }
            }
            else
            {
                List<string> files = new List<string>();
                foreach (var path in TOOL_PATHS)
                {
                    if (File.Exists(path + _fileNameSel))
                    {
                        files.Add(path + _fileNameSel);
                    }
                }

                if (files.Count == 0)
                {
                    _prodPrintErr = true;
                    _printErr = "Tooling print does not exist";
                    return;
                }
                _printErr = "";
                foreach (var file in files)
                {
                    
                    if (IsFileinUse(new FileInfo(file)))
                    {
                        _prodPrintErr = true;
                        _printErr = string.Concat(_printErr, $"{file.Substring(5,file.IndexOf("/",5) - 5)} is in use \n");
                    }
                    else
                    {
                        Process.Start(CAD_PATH, file);
                    }
                }

            }


            if (_prodPrintErr)
            {
                _popups.Add(new Popup(this, _printErr, PopupType.Warning));
            }


        }
        public void OpenSld(bool production=true)
        {
            if (_fileNameSel != "")
            {
                Tuple<bool,string> result = sw.OpenPrint(_fileNameSel);
                _sldPrintErr = result.Item1;
                _sldErr = result.Item2;
            }


            if (_sldPrintErr)
            {
                _popups.Add(new Popup(this, _sldErr, PopupType.Warning));
            }

        }
        public bool ClearSldWarning()
        {
            _sldPrintErr = false;
            _sldErr = "";
            return true;
        }
        public bool ClearPrintWarning()
        {
            _prodPrintErr = false;
            _printErr = "";
            return true;
        }
        //checks whether or not someone has a print open or not
        private bool IsFileinUse(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
        private string StartUser()
        {
            string username = Environment.UserName;
            var recs = _db.Table<User>().Where(V => V.username == username).ToList();
            if (recs.Count == 0)
            {
                User newUser = new User
                {
                    username = username,
                    accessLevel = "User",
                    accessCount = 1,
                    lastUsed = DateTime.Now,
                };
                var results = _db.Insert(newUser);
                return newUser.accessLevel;
            }
            else
            {
                var count = recs.Select(v => v.accessCount).ToList().FirstOrDefault();
                User updated = new User
                {
                    id = recs.Select(v => v.id).FirstOrDefault(),
                    username = username,
                    accessLevel = recs.Select(v => v.accessLevel).FirstOrDefault(),
                    accessCount = count + 1,
                    lastUsed = DateTime.Now,
                };
                var results = _db.Update(updated);
                return recs.Select(v => v.accessLevel).ToList().FirstOrDefault();
            }
        }
        private void CreateDatabase()
        {
            _db = new SQLiteConnection(LOCATION);
            _db.CreateTable<Part>();
            _db.CreateTable<CustomerPart>();
            _db.CreateTable<JobBox>();
            _db.CreateTable<PartAttribute>();
            _db.CreateTable<UpdateHistory>();
            _db.CreateTable<SharedTool>();
            _db.CreateTable<User>();
        }
        public void UpdateSolidworksPart()
        {
            if (_fileNameSel != "")
            {
                BackgroundWorker su = new BackgroundWorker();
                su.WorkerReportsProgress = true;
                //what to do in the background thread
                //reference this question on stack overflow
                //https://stackoverflow.com/questions/363377/how-do-i-run-a-simple-bit-of-code-in-a-new-thread
                su.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        BackgroundWorker b = o as BackgroundWorker;
                        sw.UpdatePart(new PartInfo(this, _fileNameSel));
                    }
                );

                su.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                    delegate (object o, RunWorkerCompletedEventArgs args)
                    {
                        Console.WriteLine("Done");
                    }
                );
                su.RunWorkerAsync();
            } 
        }
        public bool UpdatePartForm()
        {
            if (_fileNameSel != "")
            {
                _updateWindows.Add(new PartEditForm(this, SPC: _spcSel, CUS: _cusSel, MAT: _matSel, REV: _revSel, BOX: String.Join(",", _jobBoxes), DESC: _descSel, CUSTOMER: _cusNumSel, DRAW: _drawSel, EXISTING: true));
            }
            return true;
        }
        public void UpdatePart(PartEditForm form)
        {
            string fileName = form.spc.i.Substring(7).Replace("-", "") + ".DC";
            int currId = _allSpcParts.Where(v => v.fileName == fileName).Select(v => v.id).FirstOrDefault();
            _updateWindows.Remove(form);
            Part newPart = new Part
            {
                id = currId,
                fileName = fileName,
                spcNumber = form.spc.i,
                description = form.desc.i,
                revision = form.rev.i,
                material = form.mat.i,
                drawSize = form.draw.i,
                customerNumber = form.customer.i,
                lastUpdated = DateTime.Now,
            };
            var results = _db.Update(newPart);
            var boxes = form.box.i.Split(",").ToList().Select(v => v.Replace(" ", "")).ToList();
            UpdateJobBoxes(newPart, boxes, true);
            GetLists();
        }
        public void CreatePart(PartEditForm form)
        {
            _updateWindows.Remove(form);
            Part newPart = new Part
            {
                fileName = form.spc.i.Substring(7).Replace("-", "") + ".DC",
                spcNumber = form.spc.i,
                description = form.desc.i,
                material = form.mat.i,
                drawSize = form.draw.i,
                customerNumber = form.customer.i,
                lastUpdated = DateTime.Now,
            };
            var results = _db.Insert(newPart);
        }
        public bool CreatePartForm()
        {
            _updateWindows.Add(new PartEditForm(this));
            return true;
        }
        public void Update()
        {
            _cusPartsFound.Update();
            if (_cusPartsFound.changed)
            {
                _fileNameSel = GetFileName(_cusPartsFound.numClicked, false);
                //cus.input.SetInput(_cusPartsFound.numClicked);
                refresh = true;
                _cusPartsFound.changed = false;
            }
            spc.Update();
            cus.Update();
            box.Update();
            mat.Update();
            customers.Update();
            draw.Update();
            attr.Update();
            desc.Update();


            if (spc.input.changed || cus.input.changed || box.input.changed || mat.input.changed || customers.input.changed || draw.input.changed || attr.input.changed || desc.input.changed || refresh)
            {
                refresh = false;
                var allSpcNums = _allSpcParts.Select(v => v.fileName).ToList();

                List<string> spcFileNames;
                if (spc.input.i != "")
                {
                    spcFileNames = _filter.SpcPart(_allSpcParts, allSpcNums, spc.input.i);
                }
                else
                {
                    spcFileNames = allSpcNums;
                }

                List<string> cusChecked;
                if (cus.input.i != "")
                {
                    cusChecked = _filter.CusPart(_allCusParts, spcFileNames, cus.input.i);
                }
                else
                {
                    cusChecked = spcFileNames;
                }

                List<string> matChecked;
                if (mat.input.i != "")
                {
                    matChecked = _filter.Material(_allSpcParts, cusChecked, mat.input.i);
                }
                else
                {
                    matChecked = cusChecked;
                }

                List<string> customersChecked;
                if (customers.input.i != "")
                {
                    customersChecked = _filter.Customer(_allSpcParts, matChecked, customers.input.i);
                }
                else
                {
                    customersChecked = matChecked;
                }

                List<string> drawChecked;
                if (draw.input.i != "")
                {
                    drawChecked = _filter.Draw(_allSpcParts, customersChecked, draw.input.i);
                }
                else
                {
                    drawChecked = customersChecked;
                }

                List<string> attrChecked;
                if (attr.input.i != "")
                {
                    attrChecked = _filter.Attribute(_allAttributes, drawChecked, attr.input.i);
                }
                else
                {
                    attrChecked = drawChecked;
                }

                List<string> descChecked;
                if (desc.input.i != "")
                {
                    descChecked = _filter.Desc(_allSpcParts, attrChecked, desc.input.i);
                }
                else
                {
                    descChecked = attrChecked;
                }

                List<string> crossChecked;
                if (box.input.i != "")
                {
                    crossChecked = _filter.JobBox(_allBoxes, descChecked, box.input.i);
                }
                else
                {
                    crossChecked = descChecked;
                }


                _cusPartsFound.list = _allCusParts.Where(v => crossChecked.Contains(v.fileName)).Select(v => v.customerPartNum).Distinct().ToList();
                _cusPartsFound.Update();
                _cusPartsFound.Update();
                if (_cusPartsFound.list.Count != 0)
                {
                    var fileName = _allCusParts.Where(v => v.customerPartNum == _cusPartsFound.list[_cusPartsFound.i]).Select(v => v.fileName).Distinct().FirstOrDefault();
                    GetSelectedInfo(fileName);
                }
                else
                {
                    SetSelectedBlank();
                }

            }
        }

        //TODO: Change this to return this all as one value so that I can use it for more things
        //Maybe even consider turning this into a class "PartInfo" where we pass a creation
        //variable then we can reference it as needed
        //that way I can put all the Get(value) functions into it and clean up this code
        public void GetSelectedInfo(string fileName)
        {
            _fileNameSel = fileName;
            _spcSel = GetSpcFromFileName(fileName);
            _cusSel = GetCusFromFileName(fileName);
            _descSel = GetDescription(fileName);
            _jobBoxes = GetJobBoxes(fileName);
            _sharesTooling = GetSharedTooling(fileName);
            _attributes = GetAttributes(fileName);

            _drawSel = GetDraw(fileName);
            _cusNumSel = GetCustomerNumber(fileName);
            _matSel = GetMaterial(fileName);
            _revSel = GetRevision(fileName);
        }
        public void SetSelectedBlank()
        {
            _fileNameSel = "";
            _spcSel = "";
            _cusSel = "";
            _descSel = "";
            _jobBoxes.Clear();
            _sharesTooling.Clear();
            _attributes.Clear();
            _drawSel = "";
            _cusNumSel = "";
            _matSel = "";
            _revSel = "";
        }
        public string GetCustomerNumber(string fileName)
        {
            return _allSpcParts.Where(v => v.fileName == fileName).Select(v => v.customerNumber).Distinct().FirstOrDefault();
        }
        public string GetRevision(string fileName)
        {
            return _allSpcParts.Where(v => v.fileName == fileName && v.revision != null).Select(v => v.revision).Distinct().FirstOrDefault();
        }
        public string GetDraw(string fileName)
        {
            var temp = _allSpcParts.Where(v => v.fileName == fileName && v.drawSize != null).Where(v => v.drawSize.Contains("."));
            return temp.Select(v => v.drawSize.Substring(v.drawSize.IndexOf("."))).Distinct().FirstOrDefault();
        }
        public string GetMaterial(string fileName)
        {
            return _allSpcParts.Where(v => v.fileName == fileName).Select(v => v.material).Distinct().FirstOrDefault();
        }
        public string GetDescription(string fileName)
        {
            return _allSpcParts.Where(v => v.fileName == fileName).ToList().Select(v => v.description).FirstOrDefault();
        }
        public List<string> GetAttributes(string fileName)
        {
            List<string> attributes = new List<string>();
            var recs = _allAttributes.Where(v => v.fileName == fileName).ToList();

            foreach (var attr in recs)
            {
                attributes.Add(String.Concat(attr.type, " - ", attr.value));
            }
            return attributes;
        }
        public List<string> GetJobBoxes(string fileName)
        {
            return _allBoxes.Where(v => v.fileName == fileName).ToList().Select(v => v.boxNum).ToList();
        }
        private List<string> GetSharedTooling(string fileName)
        {
            return _allTools.Where(v => v.fileName == fileName).ToList().Select(v => v.sharedCusNum).ToList();
        }
        public string GetSpc(string cusPart)
        {
            string fileName = GetFileName(cusPart, false);
            return _allSpcParts.Where(v => v.fileName == fileName).ToList().Select(v => v.spcNumber).FirstOrDefault();
        }
        public string GetDescFromFileName(string fileName)
        {
            return _allSpcParts.Where(v => v.fileName == fileName).Select(v => v.description).FirstOrDefault();
        }
        public string GetCusFromFileName(string fileName)
        {
            return _allCusParts.Where(v => v.fileName == fileName).Select(v => v.customerPartNum).FirstOrDefault();
        }
        public string GetSpcFromFileName(string fileName)
        {
            return _allSpcParts.Where(v => v.fileName == fileName).Select(v => v.spcNumber).FirstOrDefault();
        }
        public string GetCus(string spcNum)
        {
            string fileName = GetFileName(spcNum, true);
            var results = _allCusParts.Where(v => v.fileName == fileName).ToList();
            if (results.Count > 0)
            {
                return results.Select(v => v.customerPartNum).FirstOrDefault();
            }
            else
            {
                return "\0";
            }
        }
        public bool OutputAutoDraw(string fileName)
        {
            List<string> codes = new List<string>() { "SPC", "DESC", "DESC1", "CUST", "BWGT", "FWGT", "REV", "PT", "RAW", "RDESC", "DRAW", "OPCD1", "INST1", "VEND1", "OPCD2", "INST2", "VEND2", "OPCD3", "INST3", "VEND3", "OPCD4", "INST4", "VEND4", "OPCD5", "INST5", "VEND5", "OPCD6", "INST6", "VEND6", "ROLD1", "ROLM1", "ROLR1", "ROLD2", "ROLM2", "ROLR2", "ROLD3", "ROLM3", "ROLR3", "HEAD1", "HEAM1", "HEAR1", "HEAD2", "HEAM2", "HEAR2", "HEAD3", "HEAM3", "HEAR3" };
            bool written = false;

            var part = _allSpcParts.Where(v => v.fileName == fileName);
            string spcNumber = part.Select(v => v.spcNumber).FirstOrDefault();
            string description = part.Select(v => v.description).FirstOrDefault();
            string cusNum = part.Select(v => v.customerNumber).FirstOrDefault();
            string revision = part.Select(v => v.revision).FirstOrDefault();
            string material = part.Select(v => v.material).FirstOrDefault();
            string drawSize = part.Select(v => v.drawSize).FirstOrDefault();
            string cusPartNum = _allCusParts.Where(v => v.fileName == fileName).Select(v => v.customerPartNum).FirstOrDefault();
            if (spcNumber == null || cusPartNum == null)
            {
                return false;
            }
            else
            {
                var temp = 1;
            }
            foreach (var code in codes)
            {
                File.AppendAllText("out.txt", code);
                int spacingLength;
                spacingLength = 7 - code.Length;
                for (int i = 0; i < spacingLength; i++)
                {
                    File.AppendAllText("out.txt", " ");
                }
                string lineOutput = "";
                if (code == "SPC")
                {
                    lineOutput = spcNumber;
                }
                else if (code == "DESC")
                {
                    lineOutput = description;
                }
                else if (code == "CUST")
                {
                    lineOutput = cusNum;
                }
                else if (code == "REV")
                {
                    lineOutput = revision;
                }
                else if (code == "PT")
                {
                    lineOutput = cusPartNum;
                }
                else if (code == "RAW")
                {
                    lineOutput = material;
                }
                else if (code == "DRAW")
                {
                    lineOutput = drawSize;
                }

                //error checking for the lineOutput
                if (lineOutput == null)
                {
                    lineOutput = "";
                }

                //writing the lineOutput
                File.AppendAllText("out.txt", lineOutput);
                spacingLength = 65 - lineOutput.Length;

                //writing the whitespace before the X
                for (int i = 0; i < spacingLength; i++)
                {
                    File.AppendAllText("out.txt", " ");
                }
                //creating a newline
                File.AppendAllText("out.txt", "X\n");
            }
            return true;
        }

        public string GetFileName(string val, bool useSpcNum = true)
        {
            string fileName;
            if (useSpcNum)
            {
                fileName = _allSpcParts.Where(v => v.spcNumber == val).ToList().Select(v => v.fileName).FirstOrDefault();
            }
            else
            {
                fileName = _allCusParts.Where(v => v.customerPartNum == val).ToList().Select(v => v.fileName).FirstOrDefault();
            }
            return fileName;
        }

        public void DeleteDatabase()
        {
            _db.Close();
            File.Delete(LOCATION);
            CreateDatabase();
        }

    }

}
