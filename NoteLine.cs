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
        

        public bool isActive = false;

        private bool _isMiss = false;
        public bool isMiss { get { return _isMiss; } }

        private int _instanceCalls = 0;
        public int InstanceCalls {  get { return _instanceCalls; } }

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
        
        public void Animate(Form form, int newY, int endY)
        {
            if (!isActive && newY >= endY - 50 && _instanceCalls < 1)
            {
                isActive = true;
                _instanceCalls++;
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
                        if (!isActive && note.Line.BackColor != Color.Gray)
                            note.Line.BackColor = Color.Gray;
                            
                        note.Line.Size = new Size(10, note.Line.Height - (int)GameForm.DeltaTime);
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
                if (GameForm.KeysPressed[note.KeyBind].isDown)
                    trueCount++;
            }
            isActive = false;
            if (trueCount == noteList.Count)
                return true;
            _isMiss = true;
            return false;
        }

        public bool ButtonHold()
        {
            int trueCount = 0;
            foreach (Note note in noteList)
                if (GameForm.KeysPressed[note.KeyBind].isHolding)
                    trueCount++;
            //Miss Input Validator
            if (_instanceCalls == 1 && (trueCount != noteList.Count || GameForm.holdElpsd >= _holdDuration))
                _isMiss = true;
            else if (trueCount == noteList.Count && GameForm.holdElpsd <= _holdDuration)
            {
                _instanceCalls++;
                return true;
            }

            foreach (Note note in noteList)
            {
                GameForm.KeysPressed[note.KeyBind].isInvalidated = true;
                GameForm.KeysPressed[note.KeyBind].isHolding = false;
            }

            isActive = false;
            return false;
        }
    }
}

