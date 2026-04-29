using System;

namespace NISPrep.Models
{
    public class ProgressEntry
    {
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int Percentage { get; set; }
        public string Level { get; set; }
    }
}
