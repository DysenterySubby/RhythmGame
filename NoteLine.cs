using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmGame
{
    internal class NoteLine
    {
        private List<Note> noteList = new List<Note>();
        public long songPosition;
        public bool isActive = false;
        private bool _isMiss = false;
        public bool isMiss { get { return _isMiss; } }
        private int _instanceCalls = 0;
        public int InstanceCalls {  get { return _instanceCalls; } }
        private int _holdDuration;
        public int HoldDuration
        {
            get { return _holdDuration; }
            private set
            {
                _holdDuration = value;
                _isHoldType = true;
            }
        }
        private bool _isHoldType = false;
        public bool isHoldType { get { return _isHoldType; } }
        public int Points { get { return noteList.Count * _specialMultiplier; } }
        private int _specialMultiplier = 1;
        private NoteType _type;
        public NoteType Type
        {
            get { return _type; }
            private set
            {
                _type = value;
                if (value == NoteType.twox)
                    _specialMultiplier = 2;
                else
                    _specialMultiplier = 1;
            }
        }

        //CONSTRUCTOR
        public NoteLine(string lineInfo, NoteType noteType)
        {
            Type = noteType;
            ParseLineInfo(lineInfo, noteType);
        } 

        private void ParseLineInfo(string lineInfo, NoteType noteType)
        {
            var dataResult = lineInfo.Split(';');

            if (!long.TryParse(dataResult[0], out songPosition))
            {
                Console.WriteLine(dataResult[0]);
                songPosition = Convert.ToInt64(dataResult[0].Split(':')[0]);
                HoldDuration = Convert.ToInt32(dataResult[0].Split(':')[1]);
            }

            var notesTemp =
                from note in dataResult[1]
                where Regex.IsMatch(Convert.ToString(note), @"[grybo]")
                select note;

            foreach (var color in notesTemp)
            {
                Note newNote = new Note(color, noteType);
                if (_isHoldType)
                    newNote.Line = new HoldLine((int)_holdDuration);
                noteList.Add(newNote);
            }
        }

        //INSERTS THE NOTES TO THE WINDOWS FORM
        public void InsertToControl(Form form)
        {
            foreach (Note note in noteList)
            {
                if (_isHoldType)
                    form.Controls.Add(note.Line);
                form.Controls.Add(note);
            }
        }
        
        //ANIMATES A ROW OF NOTE"S MOVEMENT FROM TOP TO BOTTOM
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


        //Updates the type of note
        public void UpdateLine(NoteType newType)
        {
            Type = newType;
            foreach (Note note in noteList)
                note.UpdateNote(newType);
        }


        //HIT OR MISS NOTE CHECKER - FOR NORMAL TAP NOTES
        public bool ButtonDown()
        {
            int trueCount = 0;
            foreach (Note note in noteList)
                if (GameForm.KeysPressed[note.KeyBind].isDown)
                    trueCount++;
            isActive = false;
            if (trueCount == noteList.Count)
                return true;
            _isMiss = true;
            return false;
        }

        //HIT OR MISS NOTE CHECKER - FOR HOLD NOTES
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

            //Clears Button State
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

