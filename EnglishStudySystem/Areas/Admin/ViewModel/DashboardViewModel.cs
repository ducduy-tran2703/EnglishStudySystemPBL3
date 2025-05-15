using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class DashboardViewModel
    {
        public UserStatsViewModel UserStats { get; set; }
        public RevenueStatsViewModel RevenueStats { get; set; }
        public LessonStatsViewModel LessonStats { get; set; }
        public List<EditorStatItemViewModel> EditorStats { get; set; }

        public DashboardViewModel()
        {
            UserStats = new UserStatsViewModel();
            RevenueStats = new RevenueStatsViewModel();
            LessonStats = new LessonStatsViewModel();
            EditorStats = new List<EditorStatItemViewModel>();
        }
    }
}