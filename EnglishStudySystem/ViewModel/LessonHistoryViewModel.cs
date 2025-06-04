using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnglishStudySystem.ViewModel
{
    public class LessonHistoryViewModel
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; }
        public string LessonDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ViewDate { get; set; }
        public string CourseName { get; set; }
        public int? CourseId { get; set; }
    }
}