using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadUtility
{
    internal class PartInfo
    {
        public string fileName;
        private Database _db;
        public string spc, cus, rev, customer, mat, draw, desc, box;
        public Dictionary<string, string> properties = new Dictionary<string, string>();
        public PartInfo(Database DB, string FileName)
        {
            _db = DB;
            GetAll(FileName);
        }

        public void GetAll(string FileName)
        {
            fileName = FileName;
            spc = _db.GetSpcFromFileName(fileName);
            cus = _db.GetCusFromFileName(fileName);
            rev = _db.GetRevision(fileName);
            customer = _db.GetCustomerNumber(fileName);
            mat = _db.GetMaterial(fileName);
            draw = _db.GetDraw(fileName);
            desc = _db.GetDescFromFileName(fileName);
            box = string.Join(",", _db.GetJobBoxes(fileName));
            properties.Clear();
            properties.Add("spc", spc);
            properties.Add("cus", cus);
            properties.Add("rev", rev);
            properties.Add("customer", customer);
            properties.Add("mat", mat);
            properties.Add("draw", draw);
            properties.Add("desc", desc);
            properties.Add("box", box);
        }        
    }
}
