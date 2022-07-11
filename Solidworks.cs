using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using CadUtility;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace CadUtility
{
    internal class Solidworks
    {
        private static SldWorks swApp;
        private string PARTS_PATH = "F:/DC/Solidworks/Parts/";
        private string TEMPLATE_PATH = "F:/DC/Solidworks/Templates/";
        private string PROD_TEMP_NAME = "Production_Print.SLDDRW";
        private string SLD_PATH = "C:/Program Files/SOLIDWORKS Corp/SOLIDWORKS/SLDWORKS.exe";
        public bool closeOnExit = false;   
        Process sld;
        public Solidworks()
        {
            BackgroundWorker ga = new BackgroundWorker();
            ga.WorkerReportsProgress = true;
            //what to do in the background thread
            //reference this question on stack overflow
            //https://stackoverflow.com/questions/363377/how-do-i-run-a-simple-bit-of-code-in-a-new-thread
            ga.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;
                    var app = GetApplication();
                    args.Result = app;
                }
            );

            ga.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object o, RunWorkerCompletedEventArgs args)
                {
                    swApp = (SldWorks)args.Result;
                    Console.WriteLine("Solidworks loaded");
                }
            );

            ga.RunWorkerAsync();

        }

        private static SldWorks GetApplication()
        {
            SldWorks _swApp = new SldWorks();

            _swApp = Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application")) as SldWorks;
            _swApp.Visible = true;
            return _swApp;
        }

        public void UpdatePart(PartInfo info)
        {
            swApp.Visible = false;
            string extension = Path.GetExtension(info.fileName);
            string folderName = info.fileName.Replace(extension, "");
            string sldProdPath = PARTS_PATH + folderName + "/Production/" + folderName + ".SLDDRW";
            string sldToolPath = PARTS_PATH + folderName + "/Tooling/" + folderName + ".SLDDRW";

            if (!Directory.Exists(PARTS_PATH + folderName))
            {
                Directory.CreateDirectory(PARTS_PATH + folderName);
                Directory.CreateDirectory(PARTS_PATH + folderName + "/Production");
                Directory.CreateDirectory(PARTS_PATH + folderName + "/Tooling");
                
            }
            if (!File.Exists(sldProdPath))
            {
                File.Copy(TEMPLATE_PATH + PROD_TEMP_NAME, sldProdPath);
            }


            
            int warning = 0;
            int error = 0;

            ModelDoc2 swModel = swApp.OpenDoc6(sldProdPath, (int)swDocumentTypes_e.swDocDRAWING,  (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", ref error, ref warning);
            CustomPropertyManager manager = swModel.Extension.CustomPropertyManager[""];
            foreach (var prop in info.properties.Keys)
            {
                UpdateCustomProperty(manager, prop, info.properties[prop]);
            }
            swModel.SaveSilent();
            swApp.QuitDoc(swModel.GetTitle());
            swApp.Visible = true;
        }

        public void UpdateAnnotationFont()
        {
            string filePath = "F:/DC/Solidworks/Templates/00010701.SLDDRW";
            int warnings = 0;
            int errors = 0;
            ModelDoc2 swModel = swApp.OpenDoc6(filePath, 3, 0, "", ref errors, ref warnings);
            DrawingDoc swDraw = (DrawingDoc)swApp.GetFirstDocument();
            Console.WriteLine(swDraw.GetFirstView());
            View swView = (View)swDraw.GetFirstView();
            Console.WriteLine(swView.GetAnnotations());
            Console.WriteLine(swView.GetAnnotationCount());

            //TextFormat format = new TextFormat();
            //Console.WriteLine()




            object[] annotations = (object[])swView.GetAnnotations();
            foreach (var annotation in annotations)
            {
                var ann = annotation as Annotation;
                Paragraphs par = ann.GetParagraphs() as Paragraphs;
                TextFormat format = ann.GetTextFormat(0) as TextFormat;
                Console.WriteLine("----------");
                Console.WriteLine(format.LineLength);
                format.TypeFaceName = "Arial";
                Console.WriteLine("-");
                ann.SetTextFormat(0, false, format);
                format = ann.GetTextFormat(0) as TextFormat;
                Console.WriteLine(format.LineLength);
                //Console.WriteLine(format.TypeFaceName);
            }

            //swApp.QuitDoc(swModel.GetTitle());

        }
        public Tuple<bool,string> OpenPrint(string fileName, bool production=true)
        {
            string extension = Path.GetExtension(fileName);
            string folderName = fileName.Replace(extension, "");
            string sldProdPath = PARTS_PATH + folderName + "/Production/" + folderName + ".SLDDRW";
            string sldToolPath = PARTS_PATH + folderName + "/Tooling/" + folderName + ".SLDDRW";
            bool err = false;
            string mess = "";
            if (production)
            {
                if (!File.Exists(sldProdPath))
                {
                    err = true;
                    mess = "Print Does Not Exist";
                }
                else
                {
                    int warnings = 0;
                    int errors = 0;
                    swApp.OpenDoc6(sldProdPath, 3, 0, "",  ref errors, ref warnings);


                }
            }



            return new Tuple<bool,string>(err, mess);
        }






        public void Dispose()
        {
            if (swApp != null && closeOnExit)
            {
                swApp.ExitApp();
                swApp = null;
            }
        }

        internal static void UpdateCustomProperty(CustomPropertyManager customPropertyManager, string propertyToUpdate, string newValue)
        {
            int ret = customPropertyManager.Set(propertyToUpdate, newValue);
        }






    }
}
