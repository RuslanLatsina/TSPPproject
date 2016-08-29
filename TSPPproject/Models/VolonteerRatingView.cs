using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPPproject.Models.ViewModels
{
    public class VolonteerRatingView
    {
        public int Id_Volunteer { get; set; }
        public int Rating { get; set; }
        public int Number_of_rating { get; set; }
        public double AverageRating { get; set; }
    }
}
