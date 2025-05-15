using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class LessonStatsViewModel
    {
        public int TotalCategories { get; set; }
        public CategorySummaryViewModel MostPopularCategory { get; set; }
        public CategorySummaryViewModel MostProfitableCategory { get; set; }
    }

    public class CategorySummaryViewModel
    {
        public string Name { get; set; }
        public int? TotalPurchases { get; set; }
        public decimal Revenue { get; set; }
    }
}