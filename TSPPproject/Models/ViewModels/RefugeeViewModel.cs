using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSPPproject.Models;

namespace TSPPproject.Models.ViewModels
{
    public class RefugeeViewModel
    {
        public IEnumerable<Refugee> Volunteers { get; set; }
        public IEnumerable<What_helps> WhatHelpses { get; set; }
    }
}
