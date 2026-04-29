using System.Collections.Generic;

namespace NISPrep.Models
{
    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectOptionIndex { get; set; }
    }
}
