using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teatteri
{
    public class YearList
    {
        public List<string> Years { get; set; }
        public string Year { get; set; }
        
        public YearList()
        {
            DateTime date = DateTime.Today;
            int year = date.Year;
            Year = year.ToString();
            List<string> years = new List<string>();
            
            for (int i = 1900; i <= year; i++)
            {
                years.Add(i.ToString());
            }
            Years = years;
        }
    }
}
