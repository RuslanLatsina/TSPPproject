using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPPproject.Models.ViewModels
{
    public class RefugeeCountGroup
    {
        public IQueryable<IGrouping<int, Refugee>> Refugees { get; set; }
        public  Location Location { get; set; }
        public string Name { get; set; }
        public int RefugeeCount { get; set; }
        public int Id_Location { get; set; }
    }
}
