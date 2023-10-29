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
using System.Text.RegularExpressions;

namespace RhythmGame
{

    enum NoteType
    {
        standard,
        twox
    }
    internal class GameForm : Form
    {

        

        //Soundplayer object
        private SoundPlayer player;

        //Main Timer Event. For game's frame updating.
        private Timer gameTimer;

        //List of file paths of all the button's image.
        private List<string> imageLoc = new List<string>();

        //------------------STATIC------------------------
        //Dictionary of all the Button, values are are accessed using "Keys Object".
        public static Dictionary<Keys?, GtrButton> KeysPressed = new Dictionary<Keys?, GtrButton>();

        //Stopwatch object. Used for measuring program runtime, mainly used for song timing and synchronization.
        private Stopwatch program_stpWtch = new Stopwatch();
        private long gameRunTime;

        //------------------STATIC------------------------
        //Delta time is used for calculating frame dependent U.I objects.
        public static long DeltaTime = 0;
        private long previous_frameTime;
        
        //Stopwatch object. Used for measuring player's button holding.
        private Stopwatch holdButton_stpWtch = new Stopwatch();
        public static long holdElpsd;

        //------------------STATIC------------------------
        // list of notes (one line horizontally) to be inserted to the u.i
        // removes itself from the list if the line is already in the u.i
        public static List<NoteLine> highwayList = new List<NoteLine>();

        // the list of notes (one line horizontally) that are already in ui
        // remove itself from the ui when the line reach the end position
        private List<NoteLine> noteCollection = new List<NoteLine>();


        private List<List<NoteLine>> specialNoteLine = new List<List<NoteLine>>();
        bool songPlay = false;

        Label scoreLbl = new Label();
        int score;
        Label streakLbl = new Label();
        int streak;
        Label debugLbl = new Label();

        private int multiplier
        {
            get
            {
                if (streak >= 10 && streak < 20) return 2;
                else if (streak >= 20 && streak < 30) return 4;
                else if (streak >= 30 && streak < 40) return 6;
                else if (streak >= 40 && streak < 50) return 8;
                else if (streak >= 50) return 10;
                else return 1;
            }
        }

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
                KeysPressed.Add(GtrButton.GetKey(newBtn), newBtn);
                this.Controls.Add(newBtn);
            }

            //Initializes Extra Userinterface
            scoreLbl.AutoSize = true;
            scoreLbl.Font = new Font("Algerian", 21, FontStyle.Bold);
            scoreLbl.Text = "SCORE: ";
            this.Controls.Add(scoreLbl);

            streakLbl.AutoSize = true;
            streakLbl.Font = new Font("Algerian", 21, FontStyle.Bold);
            streakLbl.Location = new Point(0, scoreLbl.Height);
            this.Controls.Add(streakLbl);

            debugLbl.AutoSize = true;
            debugLbl.Font = new Font("Algerian", 21, FontStyle.Bold);
            debugLbl.Location = new Point(0, scoreLbl.Height+streakLbl.Height);
            this.Controls.Add(debugLbl);

            //Initializes Core Game Events
            gameTimer = new Timer();
            gameTimer.Interval = 30;
            gameTimer.Tick += new EventHandler(GameTimerEvent);
            this.KeyDown += new KeyEventHandler(ButtonDownEvent);
            this.KeyUp += new KeyEventHandler(ButtonUpEvent);
            this.Load += new EventHandler(GameOnLoad);

        }

        private async void GameOnLoad(object sender, EventArgs e)
        {
            player = new SoundPlayer($"{Directory.GetCurrentDirectory()}\\levels\\blink-182 - Dammit\\blink-182 - Dammit.wav");

            await Task.Run(() => LoadLevel());
            gameTimer.Start();
            program_stpWtch.Start();
        }

        //Game Time Event, all of game logic comes from this event.
        private void GameTimerEvent(object sender, EventArgs e)
        {
            //Variables used for calculations
            holdElpsd = holdButton_stpWtch.ElapsedMilliseconds;
            gameRunTime = program_stpWtch.ElapsedMilliseconds;
            DeltaTime = gameRunTime - previous_frameTime;

            if (!songPlay && gameRunTime > 3000)
            {
                player.Play();
                songPlay = true;
            }

            //For Inserting Notes in the U.I.
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

                //---INPUT AND SCORING EVALUATION---

                //Single Note Input Check
                if (noteLine.isActive && !noteLine.isHoldType && noteLine.ButtonDown())
                {
                    streak++;
                    score += noteLine.Points * multiplier;
                }
                
                //Hold Note Input Check
                else if (noteLine.isActive && noteLine.isHoldType && noteLine.ButtonHold())
                {
                    //Computation for how long the player should hold the note
                    double noteHoldElpsd = (double)(holdElpsd + DeltaTime) / 100;
                    double noteHoldTime = Math.Ceiling((double)holdElpsd / 100);

                    //Increase streak on the very first time collision is detected
                    if (noteLine.InstanceCalls < 3)
                        streak++;
                    //Increases score by 1 for every 100th milliseconds
                    if (noteHoldElpsd > noteHoldTime)
                        score += noteLine.Points * multiplier;
                }

                if (noteLine.isMiss)
                {
                    streak = 0;
                    if (noteLine.Type != NoteType.standard)
                    {
                        foreach (NoteLine specialNL in specialNoteLine.ElementAt(0))
                            specialNL.UpdateLine(NoteType.standard);
                        specialNoteLine.RemoveAt(0);
                    }
                }
                noteLine.Animate(this, noteY, endY);
            }

            //Resets the button press to false if the player tries to hold it down.
            foreach (GtrButton btn in KeysPressed.Values)
                if (btn.isDown && holdButton_stpWtch.ElapsedMilliseconds >= 200)
                    btn.isDown = false;
            
            previous_frameTime = gameRunTime;

            scoreLbl.Text = $"Score: {score}";
            streakLbl.Text = $"Streak: {streak}";
        }
        
        //Key Down Event
        private void ButtonDownEvent(object sender, KeyEventArgs e)
        {
            if (KeysPressed.ContainsKey(e.KeyData))
                GtrButton.KeyDownEvaluate(KeysPressed[e.KeyData]);
            holdButton_stpWtch.Start();
        }

        //Key Up Event
        private void ButtonUpEvent(object sender, KeyEventArgs e)
        {
            if (KeysPressed.ContainsKey(e.KeyData))
                GtrButton.KeyUpEvaluate(KeysPressed[e.KeyData]);    
            holdButton_stpWtch.Reset();
        }

        //Loads and generate game objects from Charts
        private void LoadLevel()
        {
            NoteType noteType = NoteType.standard;
            foreach (var line in File.ReadLines($"{Directory.GetCurrentDirectory()}\\levels\\blink-182 - Dammit\\Expert.txt"))
            {
                if (line.Contains("start") && Enum.TryParse(line.Remove(line.IndexOf("|")).ToLower(), out noteType))
                {
                    if (noteType != NoteType.standard)
                        specialNoteLine.Add(new List<NoteLine>());
                    continue;
                }

                NoteLine noteLine = new NoteLine(line, noteType);
                if (noteType != NoteType.standard)
                    specialNoteLine.ElementAt(specialNoteLine.Count - 1).Add(noteLine);
                noteCollection.Add(noteLine);
            }
        }
    }
}
