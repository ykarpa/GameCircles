﻿using System;
using System.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using Timer = System.Windows.Forms.Timer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace GameCircles
{
    public partial class GameForm : Form
    {
        private Timer removeTimer;

        private Random random = new Random();
        private int score = 0;
        private int maxScore = 0;
        private int missedBalls = 0;
        private bool isPaused = false;

        //private DateTime startTime;

        public GameForm()
        {
            InitializeComponent();
            this.Load += GameForm_Load;
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            shapeTimer.Start();
        }

        private void ShapeTimer_Tick(object sender, EventArgs e)
        {
            //Shape shape = GenerateCircle();

            Shape circle = new Circle();
            Shape star = new Star();
            Shape[] shapes = { circle, star };

            Shape shape = shapes[random.Next(0, shapes.Length)];

            shape.Size = (shape is Star) ? new Size(65, 65) : new Size(50, 50);
            shape.BackColor = GetRandomColor();
            shape.Location = GetRandomLocation();
            shape.Click += Shape_Click;

            gamePanel.Controls.Add(shape);

            removeTimer = new Timer();
            removeTimer.Interval = 1500;
            removeTimer.Tick += (sender, e) => Remove(sender, shape);
            removeTimer.Start();
        }

        private Shape GenerateCircle(int size = 0)
        {
            Shape circle = new Circle();
            circle.BackColor = GetRandomColor();
            circle.Location = GetRandomLocation();
            circle.Size = (size == 0) ? (new Size(50, 50)) : GetRandomSize();
            circle.Click += Shape_Click;

            return circle;
        }

        private void GenerateStar(int size = 0)
        {
            Shape star = new Star();
            star.BackColor = GetRandomColor();
            star.Location = GetRandomLocation();
            star.Size = (size == 0) ? (new Size(65, 65)) : GetRandomSize();
            star.Click += Shape_Click;

            gamePanel.Controls.Add(star);
        }

        private void Remove(object sender,  Shape shape)
        {
            (sender as Timer).Stop();
            (sender as Timer).Dispose();

            if (gamePanel.Controls.Contains(shape)) {
                if (shape.BackColor != Color.Green)
                {
                    missedBalls++;
                    UpdateMissedBallsLabel();
                }

                if (missedBalls == 5)
                {
                    shapeTimer.Enabled = false;
                    shapeTimer.Stop();
                    shapeTimer.Dispose();
                    EndGame();
                }

                gamePanel.Controls.Remove(shape);
            }
        }

        private Color GetRandomColor()
        {
            Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Yellow};
            return colors[random.Next(colors.Length)];
        }

        private Point GetRandomLocation()
        {
            int x = random.Next(0, gamePanel.ClientSize.Width - 50);
            int y = random.Next(0, gamePanel.ClientSize.Height - 50);
            return new Point(x, y);
        }

        private Size GetRandomSize()
        {
            int width, height;
            if (this is Circle)
            {
                width = height = random.Next(35, 55);
            }
            else //if (this is Star)
            {
                width = height = random.Next(50, 70);
            }
            
            return new Size(width, height);
        }

        private void Shape_Click(object sender, EventArgs e)
        {
            string soundFilePath = "../../../sounds/ball_hit.wav";
            SoundPlayer soundPlayer = new SoundPlayer(soundFilePath);
            soundPlayer.Play();

            Shape shape = (Shape)sender;
            gamePanel.Controls.Remove(shape);
            shapeTimer.Interval = (shapeTimer.Interval <= 400) ? 400 : shapeTimer.Interval - 10;

            Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Yellow };
            int[] points = { 10, -10, 0, 5 };

            score += points[Array.IndexOf(colors, shape.BackColor)];

            MaxScore(score);
            UpdateScoreLabel();

            if (score < 0) EndGame();
        }

        private void MaxScore(int score)
        {
            if (score > maxScore)
                maxScore = score;
            UpdateMaxScoreLabel();
        }

        private void UpdateScoreLabel()
        {
            scoreLabel.Text = $"Кількість очків: {score}";
        }

        private void UpdateMissedBallsLabel()
        {
            missedLabel.Text = $"Пропущені: {missedBalls}";
        }

        private void UpdateMaxScoreLabel()
        {
            recordLabel.Text = $"Найкращий результат: {maxScore}";
        }

        private void EndGame()
        {
            removeTimer?.Stop();
            shapeTimer.Stop();
            shapeTimer.Dispose();
            removeTimer?.Dispose();
            shapeTimer.Interval = 1000;
            pausedButton.Enabled = false;
            endButton.Enabled = false;
            gamePanel.Enabled = false;

            ShowResultDialog();
        }

        private void RestartGame()
        {
            shapeTimer.Stop();
            shapeTimer.Dispose();
            removeTimer?.Dispose();
            gamePanel.Controls.Clear();

            shapeTimer.Start();
            removeTimer?.Start();

            pausedButton.Enabled = endButton.Enabled = gamePanel.Enabled = true;
            score = maxScore = missedBalls = 0;

            UpdateScoreLabel();
            UpdateMaxScoreLabel();
            UpdateMissedBallsLabel();
        }

        private void ShowResultDialog()
        {
            Form resultForm = new Form();
            resultForm.Text = "Результат гри";
            resultForm.Size = new Size(500, 400);
            resultForm.BackColor = Color.AntiqueWhite;
            resultForm.MinimizeBox = false;
            resultForm.MaximizeBox = false;
            resultForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            resultForm.StartPosition = FormStartPosition.CenterScreen;

            Label resultLabel = new Label();
            resultLabel.Text = "Ваш найкращий результат:\n" + maxScore.ToString();
            resultLabel.TextAlign = ContentAlignment.MiddleCenter;
            resultLabel.AutoSize = true;
            resultLabel.Font = new Font(resultLabel.Font.FontFamily, 12, FontStyle.Bold);
            resultLabel.Location = new Point((resultForm.Width - 3 * resultLabel.Width) / 2, 20);
            resultForm.Controls.Add(resultLabel);

            Label nameLabel = new Label();
            nameLabel.Text = "Введіть своє ім'я:";
            nameLabel.AutoSize = true;
            nameLabel.Font = new Font(nameLabel.Font.FontFamily, 10);
            nameLabel.Location = new Point(20, 100 + resultLabel.Height);
            resultForm.Controls.Add(nameLabel);

            TextBox nameTextBox = new TextBox();
            nameTextBox.Location = new Point(50 + nameLabel.Width, 98 + resultLabel.Height);
            nameTextBox.Width = 200;
            nameTextBox.BackColor = Color.LightBlue;
            resultForm.Controls.Add(nameTextBox);

            Button mainMenuButton = new Button();
            mainMenuButton.Text = "Головне меню";
            mainMenuButton.Size = new Size(250, 50);
            mainMenuButton.BackColor = Color.SkyBlue;
            mainMenuButton.Location = new Point(-1, 305);
            mainMenuButton.Click += (sender, e) =>
            {
                if (nameTextBox.Text != String.Empty)
                {
                    Player player = new Player(nameTextBox.Text, maxScore);
                    SavePlayerData(player);
                }

                resultForm.Close();
                this.Close();
            };
            resultForm.Controls.Add(mainMenuButton);

            Button restartButton = new Button();
            restartButton.Text = "Почати заново";
            restartButton.Size = new Size(250, 50);
            restartButton.BackColor = Color.SkyBlue;
            restartButton.Location = new Point(247, 305);
            restartButton.Click += (sender, e) =>
            {
                if (nameTextBox.Text != String.Empty)
                {
                    Player player = new Player(nameTextBox.Text, maxScore);
                    SavePlayerData(player);
                }

                resultForm.Close();
                RestartGame();

            };
            resultForm.Controls.Add(restartButton);

            resultForm.ShowDialog();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                isPaused = true;
                pausedButton.Text = "Продовжити";
                shapeTimer.Stop();
                removeTimer?.Stop();
                gamePanel.Enabled = false;
            }
            else
            {
                isPaused = false;
                pausedButton.Text = "Пауза";
                shapeTimer.Start();
                removeTimer?.Start();
                gamePanel.Enabled = true;
            }
        }

        private void EndButton_Click(object sender, EventArgs e)
        {
            endButton.Enabled = false;
            EndGame();
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            RestartGame();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            shapeTimer.Stop();
            removeTimer?.Stop();
            gamePanel.Controls.Clear();

            this.Close();
        }

        private void SavePlayerData(Player player)
        {
            string fileName = "player_data.txt";

            string data = player + ", " + DateTime.Now.ToString();

            try
            {
                using (StreamWriter writer = File.AppendText(fileName))
                {
                    writer.WriteLine(data);
                }

                Console.WriteLine("Дані успішно записані у файл.");
            }
            catch (IOException e)
            {
                Console.WriteLine("Помилка при записі даних у файл: " + e.Message);
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            shapeTimer.Stop();
            removeTimer?.Stop();
        }
    }
}
