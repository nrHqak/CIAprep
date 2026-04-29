using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using NISPrep.Models;
using NISPrep.Services;

namespace NISPrep
{
    public class Form1 : Form
    {
        private readonly Color bgTop = ColorTranslator.FromHtml("#7ABCE8");
        private readonly Color active = ColorTranslator.FromHtml("#81C0E9");
        private readonly Color darkText = ColorTranslator.FromHtml("#4A4A4A");
        private readonly Color border = ColorTranslator.FromHtml("#333F48");
        private readonly Color white = Color.White;
        private readonly Color secondary = ColorTranslator.FromHtml("#E9EEF2");

        private readonly TestService _testService = new TestService();
        private readonly ProgressService _progressService = new ProgressService();

        private Panel screenContainer;
        private Panel subjectPanel;
        private Panel welcomePanel;
        private Panel testPanel;
        private Panel resultPanel;
        private Panel progressPanel;
        private Panel materialsPanel;

        private Label timerLabel;
        private Label questionLabel;
        private RadioButton[] options;
        private Button nextButton;
        private Button previousButton;
        private Button finishButton;
        private Button pauseButton;

        private List<Question> currentQuestions;
        private readonly List<int> answers = new List<int>();
        private int currentIndex;
        private string currentSubject;
        private int remainingSeconds;
        private readonly Timer testTimer;
        private bool isPaused;
        private bool isBlitzMode;
        private int streak;
        private Label streakLabel;

        private Label resultTitle;
        private Label resultStats;
        private Label resultLast;
        private Label resultBest;
        private DataGridView progressGrid;
        private DataGridView reviewGrid;
        private FlowLayoutPanel subjectProgressPanel;
        private TextBox materialSearchBox;
        private ComboBox materialSubjectFilter;
        private ListView materialsListView;
        private readonly List<StudyMaterial> materials = new List<StudyMaterial>();

        public Form1()
        {
            Text = "NIS Prep";
            MinimumSize = new Size(1000, 680);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            testTimer = new Timer { Interval = 1000 };
            testTimer.Tick += TestTimer_Tick;

            InitializeLayout();
            BuildSubjectScreen();
            BuildWelcomeScreen();
            BuildTestScreen();
            BuildResultScreen();
            BuildProgressScreen();
            BuildMaterialsScreen();
            LoadMaterials();
            ShowScreen(welcomePanel);
        }

        private void InitializeLayout()
        {
            screenContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(32), BackColor = Color.Transparent };
            Controls.Add(screenContainer);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(ClientRectangle, bgTop, white, 90f))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        private void BuildWelcomeScreen()
        {
            welcomePanel = CreateCardPanel();
            var title = CreateLabel("NIS Prep", 44, FontStyle.Bold, new Point(32, 30));
            var subtitle = CreateLabel("Подготовка к тестам по основным предметам", 15, FontStyle.Regular, new Point(36, 95));
            var tip = CreateLabel("Выберите формат: начать подготовку или сразу открыть прогресс.", 11, FontStyle.Regular, new Point(36, 125));
            tip.ForeColor = ColorTranslator.FromHtml("#6B7280");

            var previewCard = new Panel
            {
                BackColor = Color.FromArgb(245, 250, 254),
                Location = new Point(36, 175),
                Size = new Size(860, 230),
                BorderStyle = BorderStyle.FixedSingle
            };
            var previewTitle = CreateLabel("Доступные предметы", 18, FontStyle.Bold, new Point(20, 16));
            previewCard.Controls.Add(previewTitle);
            var previewSubjects = new[] { "Математика", "Физика", "Информатика", "Химия", "Биология", "Казахский язык", "Русский язык", "История Казахстана" };
            for (int i = 0; i < previewSubjects.Length; i++)
            {
                var tag = new Label
                {
                    Text = previewSubjects[i],
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = darkText,
                    BackColor = ColorTranslator.FromHtml("#E9EEF2"),
                    Location = new Point(20 + (i % 4) * 205, 60 + (i / 4) * 70),
                    Size = new Size(185, 44),
                    BorderStyle = BorderStyle.FixedSingle
                };
                previewCard.Controls.Add(tag);
            }

            var startButton = CreateButton("Начать подготовку", new Rectangle(36, 430, 280, 58), false);
            var progressButton = CreateButton("Открыть прогресс", new Rectangle(328, 430, 220, 58), true);
            var materialButton = CreateButton("Материалы", new Rectangle(560, 430, 180, 58), true);
            startButton.Click += (s, e) => ShowScreen(subjectPanel);
            progressButton.Click += (s, e) => { RefreshProgressGrid(); ShowScreen(progressPanel); };
            materialButton.Click += (s, e) => { ApplyMaterialFilters(false); ShowScreen(materialsPanel); };

            welcomePanel.Controls.Add(title);
            welcomePanel.Controls.Add(subtitle);
            welcomePanel.Controls.Add(tip);
            welcomePanel.Controls.Add(previewCard);
            welcomePanel.Controls.Add(startButton);
            welcomePanel.Controls.Add(progressButton);
            welcomePanel.Controls.Add(materialButton);
            screenContainer.Controls.Add(welcomePanel);
        }

        private void BuildSubjectScreen()
        {
            subjectPanel = CreateCardPanel();
            var title = CreateLabel("Выберите предмет", 26, FontStyle.Bold, new Point(30, 24));
            subjectPanel.Controls.Add(title);

            var subjects = new[] { "Математика", "Физика", "Информатика", "Химия", "Биология", "Казахский язык", "Русский язык", "История Казахстана" };
            for (int i = 0; i < subjects.Length; i++)
            {
                var index = i;
                var btn = CreateButton(subjects[i], new Rectangle(30 + (i % 2) * 360, 90 + (i / 2) * 80, 320, 56), false);
                btn.Click += (s, e) => StartTest(subjects[index]);
                subjectPanel.Controls.Add(btn);
            }

            var openProgress = CreateButton("Открыть прогресс", new Rectangle(30, 440, 200, 48), true);
            var blitzButton = CreateButton("Быстрый тест", new Rectangle(250, 440, 200, 48), false);
            var materialsButton = CreateButton("Материалы", new Rectangle(470, 440, 200, 48), true);
            openProgress.Click += (s, e) => { RefreshProgressGrid(); ShowScreen(progressPanel); };
            blitzButton.Click += (s, e) => StartBlitzMode();
            materialsButton.Click += (s, e) => { ApplyMaterialFilters(false); ShowScreen(materialsPanel); };
            subjectPanel.Controls.Add(openProgress);
            subjectPanel.Controls.Add(blitzButton);
            subjectPanel.Controls.Add(materialsButton);

            screenContainer.Controls.Add(subjectPanel);
        }

        private void BuildTestScreen()
        {
            testPanel = CreateCardPanel();
            timerLabel = CreateLabel("Осталось: 15:00", 14, FontStyle.Bold, new Point(30, 24));
            questionLabel = CreateLabel("", 18, FontStyle.Bold, new Point(30, 70));
            questionLabel.MaximumSize = new Size(860, 0);
            streakLabel = CreateLabel("🔥 Серия: 0", 12, FontStyle.Bold, new Point(760, 24));
            testPanel.Controls.Add(timerLabel);
            testPanel.Controls.Add(questionLabel);
            testPanel.Controls.Add(streakLabel);

            options = new RadioButton[4];
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new RadioButton
                {
                    Location = new Point(40, 140 + i * 52),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12f),
                    ForeColor = darkText
                };
                testPanel.Controls.Add(options[i]);
            }

            previousButton = CreateButton("Назад", new Rectangle(30, 390, 130, 46), true);
            nextButton = CreateButton("Далее", new Rectangle(170, 390, 130, 46), false);
            finishButton = CreateButton("Завершить тест", new Rectangle(310, 390, 190, 46), true);
            pauseButton = CreateButton("Пауза", new Rectangle(700, 390, 130, 46), true);
            var toHome = CreateButton("К предметам", new Rectangle(510, 390, 170, 46), true);

            previousButton.Click += (s, e) => PreviousQuestion();
            nextButton.Click += (s, e) => NextQuestion();
            finishButton.Click += (s, e) => FinishTest();
            pauseButton.Click += (s, e) => TogglePause();
            toHome.Click += (s, e) => { testTimer.Stop(); ShowScreen(subjectPanel); };

            testPanel.Controls.Add(previousButton);
            testPanel.Controls.Add(nextButton);
            testPanel.Controls.Add(finishButton);
            testPanel.Controls.Add(toHome);
            testPanel.Controls.Add(pauseButton);
            screenContainer.Controls.Add(testPanel);
        }

        private void BuildResultScreen()
        {
            resultPanel = CreateCardPanel();
            resultTitle = CreateLabel("Результат", 26, FontStyle.Bold, new Point(30, 24));
            resultStats = CreateLabel("", 14, FontStyle.Regular, new Point(30, 90));
            resultStats.MaximumSize = new Size(860, 0);
            resultLast = CreateLabel("", 12, FontStyle.Regular, new Point(30, 180));
            resultBest = CreateLabel("", 12, FontStyle.Regular, new Point(30, 220));
            var retry = CreateButton("Пройти заново", new Rectangle(30, 320, 200, 48), false);
            var home = CreateButton("К предметам", new Rectangle(240, 320, 180, 48), true);
            var viewProgress = CreateButton("Смотреть прогресс", new Rectangle(430, 320, 200, 48), true);
            retry.Click += (s, e) => StartTest(currentSubject);
            home.Click += (s, e) => ShowScreen(subjectPanel);
            viewProgress.Click += (s, e) => { RefreshProgressGrid(); ShowScreen(progressPanel); };

            resultPanel.Controls.Add(resultTitle);
            resultPanel.Controls.Add(resultStats);
            resultPanel.Controls.Add(resultLast);
            resultPanel.Controls.Add(resultBest);
            reviewGrid = new DataGridView
            {
                Location = new Point(30, 270),
                Size = new Size(860, 180),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                BackgroundColor = white,
                BorderStyle = BorderStyle.FixedSingle
            };
            reviewGrid.RowPrePaint += ReviewGrid_RowPrePaint;
            resultPanel.Controls.Add(reviewGrid);
            resultPanel.Controls.Add(retry);
            resultPanel.Controls.Add(home);
            resultPanel.Controls.Add(viewProgress);
            screenContainer.Controls.Add(resultPanel);
        }

        private void BuildProgressScreen()
        {
            progressPanel = CreateCardPanel();
            progressPanel.Controls.Add(CreateLabel("Прогресс", 26, FontStyle.Bold, new Point(30, 24)));
            progressGrid = new DataGridView
            {
                Location = new Point(30, 90),
                Size = new Size(860, 350),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                BackgroundColor = white,
                BorderStyle = BorderStyle.FixedSingle
            };
            progressPanel.Controls.Add(progressGrid);
            subjectProgressPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 450),
                Size = new Size(860, 120),
                AutoScroll = true
            };
            progressPanel.Controls.Add(subjectProgressPanel);
            var back = CreateButton("Назад", new Rectangle(30, 580, 140, 44), true);
            var clear = CreateButton("Очистить прогресс", new Rectangle(180, 580, 200, 44), true);
            back.Click += (s, e) => ShowScreen(subjectPanel);
            clear.Click += (s, e) => ClearProgress();
            progressPanel.Controls.Add(back);
            progressPanel.Controls.Add(clear);
            screenContainer.Controls.Add(progressPanel);
        }

        private void BuildMaterialsScreen()
        {
            materialsPanel = CreateCardPanel();
            materialsPanel.Controls.Add(CreateLabel("Материалы", 26, FontStyle.Bold, new Point(30, 24)));

            materialSearchBox = new TextBox { Location = new Point(30, 90), Width = 360, Font = new Font("Segoe UI", 11f) };
            materialSubjectFilter = new ComboBox { Location = new Point(410, 90), Width = 230, Font = new Font("Segoe UI", 11f), DropDownStyle = ComboBoxStyle.DropDownList };
            materialSubjectFilter.Items.AddRange(new object[] { "Все", "Математика", "Физика", "Информатика", "Химия", "Биология", "Казахский язык", "Русский язык", "История Казахстана" });
            materialSubjectFilter.SelectedIndex = 0;
            var searchButton = CreateButton("Поиск", new Rectangle(660, 88, 120, 38), false);
            var openButton = CreateButton("Открыть", new Rectangle(790, 88, 120, 38), true);
            var back = CreateButton("Назад", new Rectangle(30, 540, 140, 44), true);

            materialsListView = new ListView
            {
                Location = new Point(30, 140),
                Size = new Size(880, 380),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            materialsListView.Columns.Add("Название файла", 430);
            materialsListView.Columns.Add("Предмет", 220);
            materialsListView.Columns.Add("Путь", 220);

            materialSearchBox.TextChanged += (s, e) => ApplyMaterialFilters(false);
            materialSubjectFilter.SelectedIndexChanged += (s, e) => ApplyMaterialFilters(false);
            searchButton.Click += (s, e) => ApplyMaterialFilters(true);
            openButton.Click += (s, e) => OpenSelectedMaterial();
            back.Click += (s, e) => ShowScreen(subjectPanel);

            materialsPanel.Controls.Add(materialSearchBox);
            materialsPanel.Controls.Add(materialSubjectFilter);
            materialsPanel.Controls.Add(searchButton);
            materialsPanel.Controls.Add(openButton);
            materialsPanel.Controls.Add(materialsListView);
            materialsPanel.Controls.Add(back);
            screenContainer.Controls.Add(materialsPanel);
        }

        private void StartTest(string subject)
        {
            try
            {
                currentSubject = subject;
                currentQuestions = _testService.GetQuestionsBySubject(subject);
                answers.Clear();
                for (int i = 0; i < currentQuestions.Count; i++) answers.Add(-1);
                currentIndex = 0;
                remainingSeconds = 15 * 60;
                streak = 0;
                streakLabel.Text = "🔥 Серия: 0";
                isBlitzMode = false;
                isPaused = false;
                pauseButton.Text = "Пауза";
                testTimer.Start();
                UpdateTimerLabel();
                LoadQuestion();
                ShowScreen(testPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка запуска теста: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadQuestion()
        {
            var q = currentQuestions[currentIndex];
            questionLabel.Text = $"{currentIndex + 1}. {q.Text}";
            for (int i = 0; i < options.Length; i++)
            {
                options[i].Text = q.Options[i];
                options[i].Checked = answers[currentIndex] == i;
            }
            previousButton.Enabled = currentIndex > 0;
            nextButton.Enabled = currentIndex < currentQuestions.Count - 1;
        }

        private void NextQuestion()
        {
            SaveCurrentAnswerAndUpdateStreak();
            if (currentIndex < currentQuestions.Count - 1)
            {
                currentIndex++;
                LoadQuestion();
            }
        }

        private void PreviousQuestion()
        {
            SaveCurrentAnswerAndUpdateStreak();
            if (currentIndex > 0)
            {
                currentIndex--;
                LoadQuestion();
            }
        }

        private void SaveCurrentAnswerAndUpdateStreak()
        {
            var selected = Array.FindIndex(options, x => x.Checked);
            answers[currentIndex] = selected;
            if (selected == -1) return;
            if (selected == currentQuestions[currentIndex].CorrectOptionIndex) streak++;
            else streak = 0;
            streakLabel.Text = $"🔥 Серия: {streak}";
        }

        private void FinishTest()
        {
            SaveCurrentAnswerAndUpdateStreak();
            testTimer.Stop();

            int correct = 0;
            for (int i = 0; i < currentQuestions.Count; i++)
            {
                if (answers[i] == currentQuestions[i].CorrectOptionIndex) correct++;
            }

            int percentage = (int)Math.Round((double)correct / currentQuestions.Count * 100);
            string level = percentage < 50 ? "❌ Низкий" : percentage < 80 ? "⚠️ Средний" : "✅ Высокий";

            var entry = new ProgressEntry
            {
                Date = DateTime.Now,
                Subject = currentSubject,
                CorrectAnswers = correct,
                TotalQuestions = currentQuestions.Count,
                Percentage = percentage,
                Level = level
            };
            _progressService.Add(entry);

            resultStats.Text = $"Предмет: {currentSubject}\nПравильных ответов: {correct}/{currentQuestions.Count}\nПроцент: {percentage}%\nУровень: {level}";
            var last = _progressService.GetLast(currentSubject);
            var best = _progressService.GetBest(currentSubject);
            resultLast.Location = new Point(30, resultStats.Bottom + 12);
            resultBest.Location = new Point(30, resultLast.Bottom + 10);
            resultLast.Text = last == null ? "Последний результат: нет данных" : $"Последний: {last.Percentage}% ({last.Date:g})";
            resultBest.Text = best == null ? "Лучший результат: нет данных" : $"Лучший: {best.Percentage}% ({best.Date:g})";
            reviewGrid.DataSource = currentQuestions.Select((q, idx) => new
            {
                Вопрос = q.Text,
                Ваш_ответ = answers[idx] >= 0 ? q.Options[answers[idx]] : "Не отвечено",
                Правильный = q.Options[q.CorrectOptionIndex],
                Верно = answers[idx] == q.CorrectOptionIndex
            }).ToList();

            ShowScreen(resultPanel);
        }

        private void RefreshProgressGrid()
        {
            progressGrid.DataSource = _progressService.GetAll().Select(x => new
            {
                Дата = x.Date,
                Предмет = x.Subject,
                Верно = $"{x.CorrectAnswers}/{x.TotalQuestions}",
                Процент = x.Percentage,
                Уровень = x.Level
            }).ToList();
            RefreshSubjectProgress();
        }

        private void TestTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            UpdateTimerLabel();
            if (remainingSeconds <= 0)
            {
                FinishTest();
            }
        }

        private void UpdateTimerLabel()
        {
            var ts = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
            timerLabel.Text = $"Осталось: {ts:mm\\:ss}";
        }

        private void StartBlitzMode()
        {
            currentSubject = "Blitz Mode";
            currentQuestions = _testService.GetRandomQuestions(12);
            answers.Clear();
            for (int i = 0; i < currentQuestions.Count; i++) answers.Add(-1);
            currentIndex = 0;
            remainingSeconds = 60;
            streak = 0;
            streakLabel.Text = "🔥 Серия: 0";
            isBlitzMode = true;
            isPaused = false;
            pauseButton.Text = "Пауза";
            testTimer.Start();
            UpdateTimerLabel();
            LoadQuestion();
            ShowScreen(testPanel);
        }

        private void TogglePause()
        {
            if (isPaused)
            {
                testTimer.Start();
                pauseButton.Text = "Пауза";
            }
            else
            {
                testTimer.Stop();
                pauseButton.Text = "Продолжить";
            }
            isPaused = !isPaused;
        }

        private void ReviewGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = reviewGrid.Rows[e.RowIndex];
            var isCorrect = row.Cells["Верно"].Value is bool value && value;
            row.DefaultCellStyle.BackColor = isCorrect ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 226, 226);
        }

        private void RefreshSubjectProgress()
        {
            subjectProgressPanel.Controls.Clear();
            var subjectAverages = _progressService.GetSubjectAverages();
            foreach (var item in subjectAverages.OrderBy(x => x.Key))
            {
                var row = new Panel { Width = 400, Height = 36, Margin = new Padding(8), BackColor = white };
                var label = new Label { Text = $"{item.Key}: {item.Value}%", Width = 180, Location = new Point(0, 10), ForeColor = darkText };
                var bar = new ProgressBar { Minimum = 0, Maximum = 100, Value = Math.Max(0, Math.Min(100, item.Value)), Width = 200, Height = 18, Location = new Point(190, 9) };
                row.Controls.Add(label);
                row.Controls.Add(bar);
                subjectProgressPanel.Controls.Add(row);
            }
        }

        private Panel CreateCardPanel()
        {
            return new Panel { Dock = DockStyle.Fill, BackColor = white, Padding = new Padding(12) };
        }

        private Label CreateLabel(string text, float size, FontStyle style, Point location)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = darkText, AutoSize = true, Location = location };
        }

        private Button CreateButton(string text, Rectangle bounds, bool secondaryButton)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                ForeColor = darkText,
                BackColor = secondaryButton ? secondary : active,
                Bounds = bounds,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = border;
            btn.FlatAppearance.BorderSize = 1;

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(btn.BackColor);
            btn.MouseLeave += (s, e) => btn.BackColor = secondaryButton ? secondary : active;
            btn.Resize += (s, e) =>
            {
                var path = new GraphicsPath();
                int radius = 24;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                btn.Region = new Region(path);
            };

            return btn;
        }

        private void ShowScreen(Control screen)
        {
            subjectPanel.Visible = false;
            welcomePanel.Visible = false;
            testPanel.Visible = false;
            resultPanel.Visible = false;
            progressPanel.Visible = false;
            materialsPanel.Visible = false;
            screen.Visible = true;
        }

        private void ClearProgress()
        {
            var result = MessageBox.Show(
                "Удалить все результаты тестов?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            _progressService.ClearAll();
            RefreshProgressGrid();
        }

        private void LoadMaterials()
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Materials");
            materials.Clear();
            materials.Add(new StudyMaterial { Title = "Алгебра_Базовый.pdf", Subject = "Математика", FilePath = Path.Combine(basePath, "Алгебра_Базовый.pdf") });
            materials.Add(new StudyMaterial { Title = "Механика_Основы.pdf", Subject = "Физика", FilePath = Path.Combine(basePath, "Механика_Основы.pdf") });
            materials.Add(new StudyMaterial { Title = "CSharp_Введение.pdf", Subject = "Информатика", FilePath = Path.Combine(basePath, "CSharp_Введение.pdf") });
            materials.Add(new StudyMaterial { Title = "Органика_Кратко.pdf", Subject = "Химия", FilePath = Path.Combine(basePath, "Органика_Кратко.pdf") });
            materials.Add(new StudyMaterial { Title = "Анатомия_Человека.pdf", Subject = "Биология", FilePath = Path.Combine(basePath, "Анатомия_Человека.pdf") });
            materials.Add(new StudyMaterial { Title = "Казахский_Грамматика.pdf", Subject = "Казахский язык", FilePath = Path.Combine(basePath, "Казахский_Грамматика.pdf") });
            materials.Add(new StudyMaterial { Title = "Русский_Орфография.pdf", Subject = "Русский язык", FilePath = Path.Combine(basePath, "Русский_Орфография.pdf") });
            materials.Add(new StudyMaterial { Title = "История_Казахстана_Хронология.pdf", Subject = "История Казахстана", FilePath = Path.Combine(basePath, "История_Казахстана_Хронология.pdf") });
            ApplyMaterialFilters(false);
        }

        private void ApplyMaterialFilters(bool strictValidation)
        {
            var search = materialSearchBox.Text.Trim();
            if (strictValidation && string.IsNullOrWhiteSpace(search))
            {
                MessageBox.Show("Введите текст для поиска", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (strictValidation && search.Any(ch => !(char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch) || ch == '_' || ch == '-')))
            {
                MessageBox.Show("Некорректный ввод", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedSubject = materialSubjectFilter.SelectedItem?.ToString() ?? "Все";
            var filtered = materials.Where(m =>
                (selectedSubject == "Все" || m.Subject == selectedSubject) &&
                (string.IsNullOrWhiteSpace(search) ||
                 m.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 m.Subject.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            materialsListView.Items.Clear();
            foreach (var material in filtered)
            {
                var item = new ListViewItem(material.Title);
                item.SubItems.Add(material.Subject);
                item.SubItems.Add(material.FilePath);
                item.Tag = material;
                materialsListView.Items.Add(item);
            }

            if (strictValidation && filtered.Count == 0)
            {
                MessageBox.Show("Материалы не найдены", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenSelectedMaterial()
        {
            if (materialsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите материал из списка", "Материалы", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var material = materialsListView.SelectedItems[0].Tag as StudyMaterial;
            try
            {
                if (material == null || !File.Exists(material.FilePath))
                {
                    MessageBox.Show("Ошибка открытия файла", "Материалы", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Process.Start(new ProcessStartInfo(material.FilePath) { UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла", "Материалы", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
