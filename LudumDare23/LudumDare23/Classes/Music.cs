using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace LudumDare23.Classes
{
    class MusicNote
    {
        public Note Note;
        public NoteModifier NoteModifier;
        public SoundEffect[] Sound;
        public int Octave;

        public MusicNote(SoundEffect[] pSound, Note pNote, NoteModifier pNoteModifier, int pOctave)
        {
            Note = pNote;
            Sound = pSound;
            NoteModifier = pNoteModifier;
            Octave = pOctave;
        }
    }
    class Music
    {
        public List<MusicNote>[] Notes;
        int BeatsPerMinute, SinceLastQuarterBeat, CurrentQuarterBeat;
        public bool Playing = false;
        public Music(int pQuarterBeats, int pBeatsPerMinute)
        {
            BeatsPerMinute = pBeatsPerMinute;
            Notes = new List<MusicNote>[pQuarterBeats];
            for (int index = 0; index < pQuarterBeats; index++)
                Notes[index] = new List<MusicNote>();
        }
        public void Play()
        {
            Playing = true;
            CurrentQuarterBeat = 0;
            SinceLastQuarterBeat = 0;
        }
        public void Stop()
        {
            Playing = false;
        }
        public void Update()
        {
            if (!Playing)
                return;
            if (SinceLastQuarterBeat > 0)
                SinceLastQuarterBeat--;
            else
            {
                SinceLastQuarterBeat = 3600 / BeatsPerMinute;
                foreach (MusicNote note in Notes[CurrentQuarterBeat])
                    Methods.PlayNote(note.Sound, note.Octave, note.Note, note.NoteModifier);
                if (CurrentQuarterBeat < Notes.Length - 1)
                    CurrentQuarterBeat++;
                else
                    CurrentQuarterBeat = 0;
            }

        }
    }
}
