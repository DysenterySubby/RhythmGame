using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace RhythmGame
{
    internal class GameForm : Form
    {
        Timer gameTimer;
        List<string> imageLoc = new List<string>();

        //Button Dictionary
        Dictionary<Keys?, GtrButton> keysPressed = new Dictionary<Keys?, GtrButton>();

        //
        private Stopwatch holdButton_stpWtch = new Stopwatch();

        //Temporary: chart for the level
        private string[,] Chart = new string[,]
        {{"3000", "g", "r"}, {"3200", "g", null}, {"3400", null, "r"}, {"3600", null, "r"}, {"3800", null, "r"},
        {"4000", "g", null},{"4200", "g", null}, {"4400", null, "r"}, {"4600", null, "r"}, {"5000",null, "r"},
        {"5200", "g", null},{"5400", "g", null}, {"5600", null, "r"}, {"5800", null, "r"}, {"6100",null, "r"},
        {"6300", "g", null},{"6500", "g", null}, {"6700", null, "r"}, {"6900", null, "r"}, {"7100:200","g", null}};

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

            //Initializes Core Game Events
            gameTimer = new Timer();
            gameTimer.Interval = 30;
            gameTimer.Tick += new EventHandler(GameTimerEvent);
            this.KeyDown += new KeyEventHandler(ButtonDownEvent);
            this.KeyUp += new KeyEventHandler(ButtonUpEvent);
        }

        private void LoadLevel()
        {
            for (int r = 0; r < Chart.GetLength(0); r++)
            {
                List<Note> noteList = new List<Note>();
                NoteLine noteLine;
                string timeStamp = Chart[r, 0];
                for (int c = 1; c < 3; c++)
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
        }

        private void GameTimerEvent(object sender, EventArgs e)
        {
            foreach (GtrButton btn in keysPressed.Values)
            {
                if (holdButton_stpWtch.ElapsedMilliseconds >= 100 && btn.isDown)
                    btn.isDown = false;
            }
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

    }
}
