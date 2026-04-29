using System;
using System.Collections.Generic;
using NISPrep.Models;

namespace NISPrep.Services
{
    public class TestService
    {
        private readonly Dictionary<string, List<Question>> _subjectQuestions;

        public TestService()
        {
            _subjectQuestions = BuildData();
        }

        public List<Question> GetQuestionsBySubject(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException("Предмет не может быть пустым.");
            }

            if (_subjectQuestions.ContainsKey(subject))
            {
                return _subjectQuestions[subject];
            }

            throw new InvalidOperationException("Для выбранного предмета нет вопросов.");
        }

        private Dictionary<string, List<Question>> BuildData()
        {
            var common = new List<Question>
            {
                new Question { Text = "Сколько будет 12 + 8?", Options = new List<string> {"18", "20", "22", "24"}, CorrectOptionIndex = 1 },
                new Question { Text = "Сколько минут в 2 часах?", Options = new List<string> {"60", "90", "120", "140"}, CorrectOptionIndex = 2 },
                new Question { Text = "Что является столицей Казахстана?", Options = new List<string> {"Алматы", "Астана", "Шымкент", "Караганда"}, CorrectOptionIndex = 1 },
                new Question { Text = "Какой язык программирования относится к ООП?", Options = new List<string> {"C#", "HTML", "SQL", "CSS"}, CorrectOptionIndex = 0 },
                new Question { Text = "Сколько континентов на Земле?", Options = new List<string> {"5", "6", "7", "8"}, CorrectOptionIndex = 2 }
            };

            var data = new Dictionary<string, List<Question>>();
            var subjects = new[]
            {
                "Математика", "Физика", "Информатика", "Химия", "Биология", "Казахский язык", "Русский язык", "История Казахстана"
            };

            foreach (var subject in subjects)
            {
                data[subject] = new List<Question>(common);
            }

            return data;
        }
    }
}
