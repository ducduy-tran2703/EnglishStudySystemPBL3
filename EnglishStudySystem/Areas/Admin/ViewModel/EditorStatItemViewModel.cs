using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class EditorStatItemViewModel
    {
        public string EditorName { get; set; }
        public int TotalCategoriesCreated { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}