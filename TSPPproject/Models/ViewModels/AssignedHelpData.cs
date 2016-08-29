using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPPproject.Models.ViewModels
{
    public class AssignedHelpData
    {
        public int Id_What_helps { get; set; }
        public string Name { get; set; }
        public string Type_of_help { get; set; }
        public bool Assigned { get; set; }

    }
}
