using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmGame
{
    internal class NoteLine
    {
        public int songPosition;
        private int? _holdDuration;
        public int? lineHeight;
        private List<Note> noteList;

        private bool _isLong = false;
        public bool isActive = true;

        public int? HoldDuration
        {
            get { return _holdDuration; }
            set
            {
                if (value == null)
                {
                    _holdDuration = null;
                    _isLong = false;
                }
                else
                {
                    _holdDuration = int.Parse(value.ToString());
                    lineHeight = 100 * (_holdDuration * 2 / 200);
                    _isLong = true;
                }
            }
        }
        public bool IsLong
        {
            get { return _isLong; }
        }

        //FIRST CONSTRUCTER, USED WHEN NOTE IS NOT TYPE OF HOLD NOTE
        public NoteLine(int timeStamp, List<Note> noteListArg)
        {
            songPosition = timeStamp;
            noteList = noteListArg;
        }
        //SECOND CONSTRUCTER, USED WHEN NOTE IS TYPE OF HOLD NOTE
        public NoteLine(int timeStamp, List<Note> noteListArg, int hldDrtn)
        {
            songPosition = timeStamp;
            noteList = noteListArg;
            HoldDuration = hldDrtn;
        }

        public void InsertToControl(Form form)
        {
            foreach (Note note in noteList)
            {
                if (_isLong)
                    form.Controls.Add(note.Line = new LongNote(_holdDuration));
                form.Controls.Add(note);
            }
        }
        public void DisposeFromControl(Form form)
        {
            foreach (Note note in noteList)
            {
                form.Controls.Remove(note); //DISPOSES AND REMOVES THE NOTE FROM THE CONTROL AFTER REACHING ENDPOINT 
                note.Dispose();             //ONLY EXECUTES WHEN NOTELINE IS OF TYPE LONG NOTE.
                if (_isLong)
                {
                    if (!isActive)
                        note.Line.BackColor = Color.Gray;
                    note.Line.AnimateDispose(form);
                }
            }
        }
        public void AnimateLine(int newY, int endY)
        {
            foreach (Note note in noteList)
            {
                note.Location = new Point(note.Location.X, newY);
                if (_isLong && newY <= endY)
                    note.Line.Location = new Point(note.Line.Location.X, note.Location.Y - note.Line.Size.Height);
            }
        }

        public bool ButtonDown()
        {
            int trueCount = 0;
            foreach (Note note in noteList)
            {
                if (note.Button.isDown)
                    trueCount++;
            }
            if (trueCount == noteList.Count)
                return true;
            return false;
        }

        public bool ButtonHold()
        {
            int trueCount = 0;
            foreach (Note note in noteList)
            {
                if (note.Button.isHolding)
                    trueCount++;
            }
            if (trueCount == noteList.Count)
                return true;
            return false;
        }
    }
}

