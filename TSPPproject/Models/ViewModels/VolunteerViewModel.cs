using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPPproject.Models.ViewModels
{
    public class VolunteerViewModel
    {
        public IEnumerable<Volunteer> Volunteers { get; set; }
        public IEnumerable<What_helps> WhatHelpses { get; set; }
        public IEnumerable<Refugee> Refugees { get; set; }
    }
}

