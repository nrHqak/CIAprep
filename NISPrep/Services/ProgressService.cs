using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NISPrep.Models;

namespace NISPrep.Services
{
    public class ProgressService
    {
        private readonly List<ProgressEntry> _entries;
        private readonly string _filePath;

        public ProgressService()
        {
            _entries = new List<ProgressEntry>();
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "progress.txt");
            Load();
        }

        public IReadOnlyList<ProgressEntry> GetAll() => _entries.OrderByDescending(e => e.Date).ToList();

        public ProgressEntry GetBest(string subject)
        {
            return _entries
                .Where(e => e.Subject == subject)
                .OrderByDescending(e => e.Percentage)
                .ThenByDescending(e => e.Date)
                .FirstOrDefault();
        }

        public ProgressEntry GetLast(string subject)
        {
            return _entries
                .Where(e => e.Subject == subject)
                .OrderByDescending(e => e.Date)
                .FirstOrDefault();
        }

        public Dictionary<string, int> GetSubjectAverages()
        {
            return _entries
                .GroupBy(e => e.Subject)
                .ToDictionary(g => g.Key, g => (int)Math.Round(g.Average(x => x.Percentage)));
        }

        public void Add(ProgressEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            _entries.Add(entry);
            Save();
        }

        public void ClearAll()
        {
            _entries.Clear();
            Save();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                foreach (var line in File.ReadAllLines(_filePath))
                {
                    var parts = line.Split('|');
                    if (parts.Length != 6) continue;
                    _entries.Add(new ProgressEntry
                    {
                        Date = DateTime.Parse(parts[0]),
                        Subject = parts[1],
                        CorrectAnswers = int.Parse(parts[2]),
                        TotalQuestions = int.Parse(parts[3]),
                        Percentage = int.Parse(parts[4]),
                        Level = parts[5]
                    });
                }
            }
            catch
            {
                _entries.Clear();
            }
        }

        private void Save()
        {
            var lines = _entries.Select(e =>
                $"{e.Date:o}|{e.Subject}|{e.CorrectAnswers}|{e.TotalQuestions}|{e.Percentage}|{e.Level}");
            File.WriteAllLines(_filePath, lines);
        }
    }
}
