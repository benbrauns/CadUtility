using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CadUtility.GuiElementClasses;

namespace CadUtility
{
    public struct Filter
    {
        public List<string> JobBox(List<JobBox> allBoxes,List<string> preFilter, string criteria)
        {
            if (criteria.Contains("*") || criteria.Contains("?"))
            {
                return allBoxes.Where(v => preFilter.Contains(v.fileName) && Regex.IsMatch(v.boxNum, WildCardToRegular(criteria)) && v.fileName != null).Select(v => v.fileName).ToList();
            }
            else
            {
                return allBoxes.Where(v => preFilter.Contains(v.fileName) && v.boxNum.Contains(criteria) && v.fileName != null).Select(v => v.fileName).ToList();
            }
                
        }

        public List<string> CusPart(List<CustomerPart> allCusParts, List<string> preFilter, string criteria)
        {
            if (criteria.Contains("*") || criteria.Contains("?"))
            {
                return allCusParts.Where(v => preFilter.Contains(v.fileName) && v.customerPartNum != null && v.fileName != null && Regex.IsMatch(v.customerPartNum, WildCardToRegular(criteria))).Select(v => v.fileName).ToList();
            }
            else
            {
                return allCusParts.Where(v => preFilter.Contains(v.fileName) && v.customerPartNum != null && v.fileName != null && v.customerPartNum.Contains(criteria)).Select(v => v.fileName).ToList();
            }
                
        }


        // If you want to implement both "*" and "?"
        private static string WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }



        public List<string> SpcPart(List<Part> allParts, List<string> preFilter, string criteria)
        {
            if (criteria.Contains("*") || criteria.Contains("?"))
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.spcNumber != null && Regex.IsMatch(v.spcNumber, WildCardToRegular(criteria))).Select(v => v.fileName).ToList();
            }
            else
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.spcNumber != null && v.spcNumber.Contains(criteria)).Select(v => v.fileName).ToList();
            }
            
        }

        public List<string> Desc(List<Part> allParts, List<string> preFilter, string criteria)
        {
            criteria = criteria.Replace(" ", "");
            if (criteria.Contains("*") || criteria.Contains("?"))
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.description != null && Regex.IsMatch(v.description.Replace(" ", ""), WildCardToRegular(criteria))).Select(v => v.fileName).ToList();
            }
            else
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.description != null && v.description.Replace(" ", "").Contains(criteria)).Select(v => v.fileName).ToList();
            }
        }





        public List<string> Draw(List<Part> allParts, List<string> preFilter, string criteria)
        {
            if (criteria.Contains("*") || criteria.Contains("?"))
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.drawSize != null && Regex.IsMatch(v.drawSize, WildCardToRegular(criteria))).Select(v => v.fileName).ToList();
            }
            else
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.drawSize != null && v.drawSize.Contains(criteria)).Select(v => v.fileName).ToList();
            }
        }
        public List<string> Material(List<Part> allParts, List<string> preFilter, string criteria)
        {
            if (criteria.Contains("*") || criteria.Contains("?"))
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.material != null && Regex.IsMatch(v.material,criteria)).Select(v => v.fileName).ToList();
            }
            else
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.material != null && v.material.Contains(criteria)).Select(v => v.fileName).ToList();
            }
                
        }

        public List<string> Customer(List<Part> allParts, List<string> preFilter, string criteria)
        {
            if (criteria.Contains("*") || criteria.Contains("?"))
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.customerNumber != null && Regex.IsMatch(v.customerNumber,criteria)).Select(v => v.fileName).ToList();
            }
            else
            {
                return allParts.Where(v => preFilter.Contains(v.fileName) && v.fileName != null && v.customerNumber != null && v.customerNumber.Contains(criteria)).Select(v => v.fileName).ToList();
            }
        }

        public List<string> Attribute(List<PartAttribute> allAttributes, List<string> preFilter, string attValue)
        {
            if (attValue.Contains("*") || attValue.Contains("?"))
            {
                return allAttributes.Where(v => v.fileName != null && preFilter.Contains(v.fileName) && Regex.IsMatch(v.value,attValue.ToUpper())).Select(v => v.fileName).ToList();
            }
            else
            {
                return allAttributes.Where(v => v.fileName != null && preFilter.Contains(v.fileName) && v.value.Contains(attValue.ToUpper())).Select(v => v.fileName).ToList();
            }
                
        }


        




        public List<string> FilterFileNames(List<string> preFilter, string criteria)
        {
            return preFilter;
        }

    }
}
