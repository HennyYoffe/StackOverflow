using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HW60_Stackoverflow_May2.Models
{
    public class ViewQuestionViewModel
    {
        public Question Question { get; set; } 
        public User User { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Answer> Answers { get; set; }
        public bool CanLikeQuestion { get; set; }  
        public int Likes { get; set; }
    }
}
