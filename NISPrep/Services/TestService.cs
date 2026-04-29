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
            return new Dictionary<string, List<Question>>
            {
                ["Математика"] = new List<Question>
                {
                    new Question { Text = "Сколько будет 15 + 27?", Options = new List<string> {"42", "41", "39", "45"}, CorrectOptionIndex = 0 },
                    new Question { Text = "Корень из 81 равен:", Options = new List<string> {"7", "8", "9", "10"}, CorrectOptionIndex = 2 },
                    new Question { Text = "2x = 18. Чему равен x?", Options = new List<string> {"7", "8", "9", "10"}, CorrectOptionIndex = 2 },
                    new Question { Text = "Площадь квадрата со стороной 6:", Options = new List<string> {"12", "24", "30", "36"}, CorrectOptionIndex = 3 },
                    new Question { Text = "Сколько градусов в прямом угле?", Options = new List<string> {"45", "90", "120", "180"}, CorrectOptionIndex = 1 }
                },
                ["Физика"] = new List<Question>
                {
                    new Question { Text = "Единица силы в СИ:", Options = new List<string> {"Паскаль", "Ньютон", "Ватт", "Джоуль"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Скорость = ?", Options = new List<string> {"путь * время", "путь / время", "время / путь", "масса / объем"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Прибор для измерения температуры:", Options = new List<string> {"Барометр", "Термометр", "Амперметр", "Манометр"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Состояния вещества:", Options = new List<string> {"2", "3", "4", "5"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Единица мощности:", Options = new List<string> {"Вольт", "Ом", "Ватт", "Ньютон"}, CorrectOptionIndex = 2 }
                },
                ["Информатика"] = new List<Question>
                {
                    new Question { Text = "Что такое CPU?", Options = new List<string> {"Оперативная память", "Процессор", "Жесткий диск", "Монитор"}, CorrectOptionIndex = 1 },
                    new Question { Text = "C# — это:", Options = new List<string> {"Язык программирования", "ОС", "Браузер", "Антивирус"}, CorrectOptionIndex = 0 },
                    new Question { Text = "Двоичная система использует цифры:", Options = new List<string> {"0 и 1", "1 и 2", "0-9", "A-F"}, CorrectOptionIndex = 0 },
                    new Question { Text = "HTML используется для:", Options = new List<string> {"СУБД", "Разметки веб-страниц", "Монтажа видео", "Шифрования"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Что хранит временные данные программы?", Options = new List<string> {"RAM", "SSD", "GPU", "BIOS"}, CorrectOptionIndex = 0 }
                },
                ["Химия"] = new List<Question>
                {
                    new Question { Text = "Химический символ воды:", Options = new List<string> {"CO2", "H2O", "NaCl", "O2"}, CorrectOptionIndex = 1 },
                    new Question { Text = "pH = 7 означает:", Options = new List<string> {"Кислая среда", "Щелочная среда", "Нейтральная среда", "Нет среды"}, CorrectOptionIndex = 2 },
                    new Question { Text = "Поваренная соль — это:", Options = new List<string> {"NaCl", "KCl", "CaCO3", "HCl"}, CorrectOptionIndex = 0 },
                    new Question { Text = "Самый легкий элемент:", Options = new List<string> {"Кислород", "Гелий", "Водород", "Азот"}, CorrectOptionIndex = 2 },
                    new Question { Text = "Горение требует:", Options = new List<string> {"Кислород", "Соль", "Сахар", "Азот"}, CorrectOptionIndex = 0 }
                },
                ["Биология"] = new List<Question>
                {
                    new Question { Text = "Основная единица живого организма:", Options = new List<string> {"Ткань", "Клетка", "Орган", "Система"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Орган дыхания человека:", Options = new List<string> {"Печень", "Сердце", "Легкие", "Почки"}, CorrectOptionIndex = 2 },
                    new Question { Text = "Зеленый пигмент растений:", Options = new List<string> {"Меланин", "Хлорофилл", "Кератин", "Гемоглобин"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Кровь по организму перекачивает:", Options = new List<string> {"Мозг", "Сердце", "Желудок", "Легкие"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Наследственная информация содержится в:", Options = new List<string> {"ДНК", "Белках", "Жирах", "Воде"}, CorrectOptionIndex = 0 }
                },
                ["Казахский язык"] = new List<Question>
                {
                    new Question { Text = "Сколько букв в современном казахском алфавите (кириллица)?", Options = new List<string> {"40", "41", "42", "43"}, CorrectOptionIndex = 2 },
                    new Question { Text = "Выберите местоимение:", Options = new List<string> {"кітап", "жүгіру", "мен", "үлкен"}, CorrectOptionIndex = 2 },
                    new Question { Text = "Антоним к слову «жақсы»:", Options = new List<string> {"жаман", "әдемі", "биік", "терең"}, CorrectOptionIndex = 0 },
                    new Question { Text = "Как переводится «мектеп»?", Options = new List<string> {"Университет", "Школа", "Библиотека", "Дом"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Сөйлем деген не?", Options = new List<string> {"Слово", "Буква", "Законченная мысль", "Звук"}, CorrectOptionIndex = 2 }
                },
                ["Русский язык"] = new List<Question>
                {
                    new Question { Text = "Сколько падежей в русском языке?", Options = new List<string> {"5", "6", "7", "8"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Выберите существительное:", Options = new List<string> {"бежать", "красивый", "школа", "быстро"}, CorrectOptionIndex = 2 },
                    new Question { Text = "Антоним к слову «высокий»:", Options = new List<string> {"длинный", "низкий", "широкий", "глубокий"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Что обозначает глагол?", Options = new List<string> {"Предмет", "Признак", "Действие", "Количество"}, CorrectOptionIndex = 2 },
                    new Question { Text = "В слове «молоко» ударение падает на:", Options = new List<string> {"1-й слог", "2-й слог", "3-й слог", "нет ударения"}, CorrectOptionIndex = 2 }
                },
                ["История Казахстана"] = new List<Question>
                {
                    new Question { Text = "В каком году Казахстан стал независимым?", Options = new List<string> {"1986", "1991", "1995", "2001"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Первый Президент РК:", Options = new List<string> {"К. Токаев", "Н. Назарбаев", "Д. Кунаев", "А. Букейханов"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Столица Казахстана:", Options = new List<string> {"Алматы", "Астана", "Тараз", "Костанай"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Кто такие саки?", Options = new List<string> {"Средневековый народ", "Древние племена", "Современная нация", "Городское сословие"}, CorrectOptionIndex = 1 },
                    new Question { Text = "Как назывался Великий шелковый путь?", Options = new List<string> {"Торговый путь", "Военный путь", "Морской путь", "Степной путь"}, CorrectOptionIndex = 0 }
                }
            };
        }
    }
}
