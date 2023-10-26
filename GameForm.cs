using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Diagnostics;

using System.Media;

namespace RhythmGame
{
    internal class GameForm : Form
    {
        //Temporary: chart for the level
        private string[,] Chart = new string[,]
        {{"3000", "g", null, null}, {"3200", "g", null, null}, {"3400", null, "r", null}, {"3600", null, "r", null}, {"3800", null, null, "y"},
        {"4000", "r", null, null},{"4200","r", null, null }, {"4400", "y", null, null}, {"4600", "y", null, null}, {"5000", "b", null, null},
        {"5200", "g", null, null},{"5400", "g", null, null}, {"5600", null, "r", null}, {"5800", null, "r", null}, {"6100",null, "y", null},
        {"6300", "r", null, null},{"6500", "r", null, null}, {"6700", null, "y", null}, {"6900", null, "y", null}, {"7100:200","b", null, null}};


        

        //Soundplayer object
        private SoundPlayer player;

        //Main Timer Event. For game's frame updating.
        Timer gameTimer;

        //List of file paths of all the button's image.
        List<string> imageLoc = new List<string>();

        //------------------STATIC------------------------
        //Dictionary of all the Button, values are are accessed using "Keys Object".
        public static Dictionary<Keys?, GtrButton> keysPressed = new Dictionary<Keys?, GtrButton>();

        //Stopwatch object. Used for measuring program runtime, mainly used for song timing and synchronization.
        private Stopwatch program_stpWtch = new Stopwatch();

        //------------------STATIC------------------------
        //Delta time is used for calculating frame dependent U.I objects.
        public static int deltaTime = 0;

        private int gameRunTime;
        private int previous_frameTime;
        

        //Stopwatch object. Used for measuring player's button holding.
        Stopwatch holdButton_stpWtch = new Stopwatch();
        public static long holdElpsd;

        //------------------STATIC------------------------
        // list of notes (one line horizontally) to be inserted to the u.i
        // removes itself from the list if the line is already in the u.i
        public static List<NoteLine> highwayList = new List<NoteLine>();

        // the list of notes (one line horizontally) that are already in ui
        // remove itself from the ui when the line reach the end position
        private List<NoteLine> noteCollection = new List<NoteLine>();


        bool songPlay = false;
        //Int temp score tracker
        Label scoreText = new Label();
        int score;
        Label streakText = new Label();
        int streak;


        public GameForm()
        {
            //Initializes Form's Configuration
            this.Text = "Project Melody";
            this.Size = new Size(600, 700);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            //Initalizes Buttons
            string buttonDir = $"{Directory.GetCurrentDirectory()}\\assets\\button";
            imageLoc = Directory.GetFiles(buttonDir, "*.png").ToList();
            foreach (var imageLoc in imageLoc)
            {
                GtrButton newBtn = new GtrButton(imageLoc);
                keysPressed.Add(GtrButton.GetKey(newBtn), newBtn);
                this.Controls.Add(newBtn);
            }

            //Initializes Extra Userinterface
            scoreText.AutoSize = true;
            scoreText.Font = new Font("Algerian", 21, FontStyle.Bold);
            scoreText.Text = "SCORE: ";
            this.Controls.Add(scoreText);

            streakText.AutoSize = true;
            streakText.Font = new Font("Algerian", 21, FontStyle.Bold);
            streakText.Location = new Point(0, scoreText.Height);
            this.Controls.Add(streakText);

            //Initializes Core Game Events
            gameTimer = new Timer();
            gameTimer.Interval = 30;
            gameTimer.Tick += new EventHandler(GameTimerEvent);
            this.KeyDown += new KeyEventHandler(ButtonDownEvent);
            this.KeyUp += new KeyEventHandler(ButtonUpEvent);
            this.Load += new EventHandler(GameOnLoad);

            
            
        }

        //Loads and generate game objects from Charts
        private async void GameOnLoad(object sender, EventArgs e)
        {
            player = new SoundPlayer($"{Directory.GetCurrentDirectory()}\\assets\\blink-182 - Dammit.wav");

            await Task.Run(() => LoadLevel());
            gameTimer.Start();
            program_stpWtch.Start();
        }

        private void GameTimerEvent(object sender, EventArgs e)
        {
            holdElpsd = holdButton_stpWtch.ElapsedMilliseconds;
            gameRunTime = Convert.ToInt32(program_stpWtch.ElapsedMilliseconds);
            deltaTime = gameRunTime - previous_frameTime;

            if (!songPlay && gameRunTime > 3000)
            {
                player.Play();
                songPlay = true;
            }

            foreach (NoteLine noteLine in noteCollection.ToArray())
            {
                if (gameRunTime + 3000 >= noteLine.songPosition)
                {
                    noteLine.InsertToControl(this);

                    highwayList.Add(noteLine);
                    noteCollection.Remove(noteLine);
                }
            }

            foreach (NoteLine noteLine in highwayList.ToArray())
            {
                int moveDuration = 500;
                int input_offset = 10;
                int endY = 550 - input_offset;

                //Synchronization of the note movement based on current song position.
                float progress = (float)(noteLine.songPosition - gameRunTime) / moveDuration;
                int noteY = (int)Math.Round(0 + ((0 - endY) * progress));
                //Input and Scoring Evaluation.

                //NORMAL NOTE CHECK INPUT
                if (noteLine.isActive && !noteLine.isHoldType && noteLine.ButtonDown())
                {
                    score++;
                    streak++;
                }
                
                //HOLD NOTE INPUT CHECK
                else if (noteLine.isActive && noteLine.isHoldType && noteLine.ButtonHold())
                {
                    score += Convert.ToInt32(holdElpsd / 100);
                }

                if (noteLine.isMiss)
                    streak = 0;


                noteLine.Animate(this, noteY, endY);
            }

            foreach (GtrButton btn in keysPressed.Values)
            {
                if (holdButton_stpWtch.ElapsedMilliseconds >= 200 && btn.isDown)
                    btn.isDown = false;
            }
            previous_frameTime = gameRunTime;
            scoreText.Text = $"Score: {score}";
            streakText.Text = $"Streak: {streak}";
        }
        
        private void ButtonDownEvent(object sender, KeyEventArgs e)
        {
            if (keysPressed.ContainsKey(e.KeyData))
                GtrButton.KeyDownEvaluate(keysPressed[e.KeyData], holdButton_stpWtch.ElapsedMilliseconds);
            holdButton_stpWtch.Start();
        }

        private void ButtonUpEvent(object sender, KeyEventArgs e)
        {
            if (keysPressed.ContainsKey(e.KeyData))
                GtrButton.KeyUpEvaluate(keysPressed[e.KeyData]);    
            holdButton_stpWtch.Reset();
        }

        private void LoadLevel()
        {
            for (int r = 0; r < Chart.GetLength(0); r++)
            {
                List<Note> noteList = new List<Note>();
                NoteLine noteLine;
                string timeStamp = Chart[r, 0];

                for (int c = 1; c < Chart.GetLength(1); c++)
                {
                    Note newNote;
                    if (Chart[r, c] != null)
                    {
                        char color = char.Parse(Chart[r, c]);
                        newNote = new Note(color);
                        noteList.Add(newNote);
                    }
                }

                if (!timeStamp.Contains(":"))
                    noteLine = new NoteLine(Convert.ToInt32(timeStamp), noteList);
                else
                {
                    int[] data = timeStamp.Split(':').Select(s => int.Parse(s)).ToArray();
                    noteLine = new NoteLine(data[0], noteList, data[1]);
                }
                noteCollection.Add(noteLine);
            }
            Array.Clear(Chart, 0, Chart.GetLength(0));
        }
    }
}
