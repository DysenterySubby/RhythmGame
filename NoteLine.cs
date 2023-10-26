using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmGame
{
    internal class NoteLine
    {
        private List<Note> noteList;
        public int songPosition;
        
        public int? lineHeight;

        public bool isActive = false;
        private bool _isMiss = false;
        public bool isMiss { get { return _isMiss; } }


        private int? _holdDuration;
        public int? HoldDuration
        {
            get { return _holdDuration; }
            set
            {
                if (value == null)
                {
                    _holdDuration = null;
                    _isHoldType = false;
                }
                else
                {
                    _holdDuration = int.Parse(value.ToString());
                    lineHeight = 100 * (_holdDuration * 2 / 200);
                    _isHoldType = true;
                }
            }
        }
        private bool _isHoldType = false;
        public bool isHoldType
        {
            get { return _isHoldType; }
        }



        //FIRST CONSTRUCTER, USED WHEN NOTE IS NOT TYPE OF HOLD NOTE
        public NoteLine(int timeStamp, List<Note> noteListArg)
        {
            songPosition = timeStamp;
            noteList = noteListArg;
            HoldDuration = null;
        }
        //SECOND CONSTRUCTER, USED WHEN NOTE IS TYPE OF HOLD NOTE
        public NoteLine(int timeStamp, List<Note> noteListArg, int hldDrtn)
        {
            songPosition = timeStamp;
            noteList = noteListArg;
            HoldDuration = hldDrtn;
        }

        //INSERTS THE NOTES TO THE FORM
        public void InsertToControl(Form form)
        {
            foreach (Note note in noteList)
            {
                if (_isHoldType)
                    form.Controls.Add(note.Line = new HoldLine(Convert.ToInt32(_holdDuration)));
                form.Controls.Add(note);
            }
        }

        //
        int instanceActive = 0;
        public void Animate(Form form, int newY, int endY)
        {
            if (!isActive && newY >= endY - 50 && instanceActive < 1)
            {
                isActive = true;
                instanceActive++;
            }
            
            foreach (Note note in noteList)
            {
                note.Location = new Point(note.Location.X, newY);
                if (_isHoldType && newY <= endY)
                    note.Line.Location = new Point(note.Line.Location.X, note.Location.Y - note.Line.Size.Height);

                //WILL DISPOSE THE NOTE FROM THE FORM AFTER REACHING THE END Y LOCATION
                if (newY >= endY) 
                {
                    if (!_isHoldType)
                        GameForm.highwayList.Remove(this);
                    //DISPOSES AND REMOVES THE NOTE FROM THE FORM AFTER REACHING ENDPOINT 
                    form.Controls.Remove(note); 
                    note.Dispose();

                    //ANIMATES THE LINE OF THE HOLD NOTE AFTER COMPLETELY DISPOSING IT
                    if (_isHoldType)
                    {
                        if (!isActive && instanceActive < 2)
                        {
                            note.Line.BackColor = Color.Gray;
                            instanceActive++;
                        }
                            
                        note.Line.Size = new Size(10, note.Line.Height - GameForm.deltaTime);
                        note.Line.Location = new Point(note.Line.Location.X, 500 - note.Line.Size.Height);
                        if (note.Line.Size.Height < 10)
                        {
                            form.Controls.Remove(note.Line);
                            note.Line.Dispose();
                            GameForm.highwayList.Remove(this);
                        }
                    }
                }
            }
        }

        public bool ButtonDown()
        {
            int trueCount = 0;
            foreach (Note note in noteList)
            {
                if (GameForm.keysPressed[note.KeyBind].isDown)
                    trueCount++;
            }
            isActive = false;
            if (trueCount == noteList.Count)
            {
                return true;
            }
            _isMiss = true;
            return false;
        }

        public bool ButtonHold()
        {
            int trueCount = 0;
            foreach (Note note in noteList)
            {
                if (GameForm.keysPressed[note.KeyBind].isHolding)
                    trueCount++;
            }
            if (trueCount == noteList.Count && GameForm.holdElpsd <= _holdDuration)
                return true;

            Console.WriteLine("False!");
            foreach (GtrButton btn in GameForm.keysPressed.Values)
                btn.isHolding = false;

            _isMiss = true;
            isActive = false;
            return false;
        }
    }
}

