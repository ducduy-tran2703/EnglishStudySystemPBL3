using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class RevenueStatsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public List<MonthlyRevenueItemViewModel> RevenueByMonth { get; set; }

        public RevenueStatsViewModel()
        {
            RevenueByMonth = new List<MonthlyRevenueItemViewModel>();
        }
    }

    public class MonthlyRevenueItemViewModel
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }
}