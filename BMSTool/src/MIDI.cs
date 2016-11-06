using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src
{
    /// <summary>
    /// Container for track and event data imported from a MIDI file.
    /// </summary>
    class MIDI
    {
        List<Track> Tracks;
        short TimeBase;

        public MIDI(EndianBinaryReader reader)
        {
            Tracks = new List<Track>();

            ReadMIDI(reader);
        }

        private void ReadMIDI(EndianBinaryReader reader)
        {
            reader.SkipInt64(); // Skips "MThd" magic and size, which is always 6

            // There's type 0, type 1, and type 2
            // Type 1 is the easiest to turn into BMS.
            // Type 0 puts individual notes into the different channels. Slightly harder but still possible.
            // Type 2 is meant for tracks that are to be played invidivdually. Not supported for BMS output.
            short midiType = reader.ReadInt16();

            if (midiType == 0)
            {
                //ReadType0Midi(reader);
            }
            else if (midiType == 1)
            {
                ReadType1Midi(reader);
            }
            else if (midiType == 2)
            {
                throw new FormatException("MIDI type 2 is unsupported!");
            }
            else
            {
                throw new FormatException("MIDI type unrecognized!");
            }
        }

        private void ReadType1Midi(EndianBinaryReader reader)
        {
            short numTracks = reader.ReadInt16();
            TimeBase = reader.ReadInt16();

            for (int i = 0; i < numTracks; i++)
            {
                Track track = new Track(reader, FileTypes.MIDI);
                Tracks.Add(track);
            }
        }

        public void WriteBMS(string fileName)
        {
            using (FileStream stream = new FileStream(string.Format("D:\\{0}.bms", fileName), FileMode.Create, FileAccess.Write))
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);

                // - 1 because MIDI type 1's first track is global info like tempo
                for (int i = 0; i < Tracks.Count - 1; i++)
                {
                    writer.Write((byte)0xC1); // Open track command
                    writer.Write((byte)i); // Track number
                    writer.Write((byte)0);
                    writer.Write((byte)0); // Placeholder for track offset. It's only 24 bits, hence the 3 bytes
                    writer.Write((byte)0);
                }

                foreach (Event ev in Tracks[0].Events)
                    ev.WriteBMS(writer);
                writer.Write((byte)0xFD);
                writer.Write(TimeBase);
                Wait highestWait = new Wait(CalcHighestWait());
                highestWait.WriteBMS(writer);
                writer.Write((byte)0xFF);
                Tracks.RemoveAt(0);

                foreach (Track track in Tracks)
                    track.WriteTrack(writer, FileTypes.BMS);
            }
        }

        private uint CalcHighestWait()
        {
            uint highestTime = uint.MinValue;

            foreach (Track track in Tracks)
            {
                uint localHigh = 0;

                foreach (Event ev in track.Events)
                {
                    if (ev.GetType() == typeof(Wait))
                    {
                        Wait wait = ev as Wait;
                        localHigh += wait.WaitTime;
                    }
                }

                if (localHigh > highestTime)
                    highestTime = localHigh;
            }

            return highestTime;
        }
    }
}
