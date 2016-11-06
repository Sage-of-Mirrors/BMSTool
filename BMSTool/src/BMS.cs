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
    /// Container for track and event data imported from a BMS file.
    /// </summary>
    class BMS
    {
        List<Track> Tracks;

        public BMS(EndianBinaryReader reader)
        {
            Tracks = new List<Track>();
            ReadBMS(reader);
        }

        private void ReadBMS(EndianBinaryReader reader)
        {
            // Get the tracks that are defined at the start of the file
            while (reader.PeekReadByte() == 0xC1)
            {
                reader.SkipInt16(); // Skip open track opcode and track number

                int trackOffset = (int)reader.ReadBits(24); //& 0x00FFFFFF; // Get 24 bits that hold the offset
                long curPos = reader.BaseStream.Position; // Save position, since we're jumping to trackOffset
                reader.BaseStream.Seek(trackOffset, System.IO.SeekOrigin.Begin); // Jump to beginning of track data

                Track track = new Track(reader, FileTypes.BMS);
                Tracks.Add(track);

                reader.BaseStream.Position = curPos; // Restore position to continue reading the file
            }

            // MIDI format 1, which this program will output, uses the first track in the file to set global things
            // like tempo and volume. We'll create a track out of the BMS's header here and insert it at the top
            // of the track list.
            Track headerTrack = new Track(reader, FileTypes.BMS);
            Tracks.Insert(0, headerTrack);
        }

        public void WriteMIDIFile(string fileName)
        {
            using (FileStream stream = new FileStream(string.Format("{0}.mid", fileName), FileMode.Create, FileAccess.Write))
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                writer.Write("MThd".ToCharArray()); // Header magic
                writer.Write((int)6); // Header size, always 6
                writer.Write((short)1); // Format, this program outputs format 1
                writer.Write((short)Tracks.Count); // Number of tracks
                writer.Write((short)0xF0); // This is for timing and divisions. It's overwritten (hopefully) by the SetTimeBase event.

                foreach (Track track in Tracks)
                    track.WriteTrack(writer, FileTypes.MIDI);
            }
        }
    }
}
