using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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

        private Label timerLabel;
        private Label questionLabel;
        private RadioButton[] options;
        private Button nextButton;
        private Button previousButton;
        private Button finishButton;

        private List<Question> currentQuestions;
        private readonly List<int> answers = new List<int>();
        private int currentIndex;
        private string currentSubject;
        private int remainingSeconds;
        private readonly Timer testTimer;

        private Label resultTitle;
        private Label resultStats;
        private Label resultLast;
        private Label resultBest;
        private DataGridView progressGrid;

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
            var title = CreateLabel("Добро пожаловать в NIS Prep", 28, FontStyle.Bold, new Point(30, 60));
            var subtitle = CreateLabel("Тренируйтесь по предметам и отслеживайте прогресс", 14, FontStyle.Regular, new Point(34, 120));
            var startButton = CreateButton("Начать подготовку", new Rectangle(34, 200, 280, 58), false);
            startButton.Click += (s, e) => ShowScreen(subjectPanel);

            welcomePanel.Controls.Add(title);
            welcomePanel.Controls.Add(subtitle);
            welcomePanel.Controls.Add(startButton);
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
            openProgress.Click += (s, e) => { RefreshProgressGrid(); ShowScreen(progressPanel); };
            subjectPanel.Controls.Add(openProgress);

            screenContainer.Controls.Add(subjectPanel);
        }

        private void BuildTestScreen()
        {
            testPanel = CreateCardPanel();
            timerLabel = CreateLabel("Осталось: 15:00", 14, FontStyle.Bold, new Point(30, 24));
            questionLabel = CreateLabel("", 18, FontStyle.Bold, new Point(30, 70));
            questionLabel.MaximumSize = new Size(860, 0);
            testPanel.Controls.Add(timerLabel);
            testPanel.Controls.Add(questionLabel);

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
            var toHome = CreateButton("К предметам", new Rectangle(510, 390, 170, 46), true);

            previousButton.Click += (s, e) => PreviousQuestion();
            nextButton.Click += (s, e) => NextQuestion();
            finishButton.Click += (s, e) => FinishTest();
            toHome.Click += (s, e) => { testTimer.Stop(); ShowScreen(subjectPanel); };

            testPanel.Controls.Add(previousButton);
            testPanel.Controls.Add(nextButton);
            testPanel.Controls.Add(finishButton);
            testPanel.Controls.Add(toHome);
            screenContainer.Controls.Add(testPanel);
        }

        private void BuildResultScreen()
        {
            resultPanel = CreateCardPanel();
            resultTitle = CreateLabel("Результат", 26, FontStyle.Bold, new Point(30, 24));
            resultStats = CreateLabel("", 14, FontStyle.Regular, new Point(30, 90));
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
            var back = CreateButton("Назад", new Rectangle(30, 460, 140, 44), true);
            var clear = CreateButton("Очистить прогресс", new Rectangle(180, 460, 200, 44), true);
            back.Click += (s, e) => ShowScreen(subjectPanel);
            clear.Click += (s, e) => ClearProgress();
            progressPanel.Controls.Add(back);
            progressPanel.Controls.Add(clear);
            screenContainer.Controls.Add(progressPanel);
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
            SaveCurrentAnswer();
            if (currentIndex < currentQuestions.Count - 1)
            {
                currentIndex++;
                LoadQuestion();
            }
        }

        private void PreviousQuestion()
        {
            SaveCurrentAnswer();
            if (currentIndex > 0)
            {
                currentIndex--;
                LoadQuestion();
            }
        }

        private void SaveCurrentAnswer()
        {
            var selected = Array.FindIndex(options, x => x.Checked);
            answers[currentIndex] = selected;
        }

        private void FinishTest()
        {
            SaveCurrentAnswer();
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
            resultLast.Text = last == null ? "Последний результат: нет данных" : $"Последний: {last.Percentage}% ({last.Date:g})";
            resultBest.Text = best == null ? "Лучший результат: нет данных" : $"Лучший: {best.Percentage}% ({best.Date:g})";

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
    }
}
