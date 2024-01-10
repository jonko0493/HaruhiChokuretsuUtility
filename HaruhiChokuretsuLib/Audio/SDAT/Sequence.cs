﻿// These files are taken from Nitro Studio 2
// https://github.com/Gota7/NitroStudio2
// They have been modified to work with .NET 6.0
// Nitro Studio 2 is created by Gota 7
// It is not explicitly licensed, but given that its
// components are licensed under GPLv3, we can assume
// it is also GPLv3 compatible
using GotaSequenceLib;
using GotaSoundIO.IO;
using System.Collections.Generic;
using System.Linq;

namespace HaruhiChokuretsuLib.Audio.SDAT
{
    /// <summary>
    /// Sequence file.
    /// </summary>
    public class Sequence : SequenceFile
    {
        /// <summary>
        /// Read the file.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(FileReader r)
        {

            //Read the sequence.
            r.OpenFile<NHeader>(out FileHeader header);

            //Data block.
            uint dataSize;
            r.OpenBlock(0, out _, out _);
            uint off = r.ReadUInt32();
            dataSize = (uint)(header.FileSize - off);
            r.Jump(off, true);
            var data = r.ReadBytes((int)dataSize).ToList();

            //Remove padding.
            for (int i = data.Count - 1; i >= 0; i--)
            {
                if (data[i] == 0)
                {
                    data.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            //Set raw data.
            RawData = [.. data];
        }

        /// <summary>
        /// Write the file.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(FileWriter w)
        {
            //Simple.
            w.InitFile<NHeader>("SSEQ", ByteOrder.LittleEndian, null, 1);
            w.InitBlock("DATA");
            w.Write((uint)0x1C);
            w.Write(RawData);
            w.Pad(4);
            w.CloseBlock();
            w.CloseFile();
        }

        /// <summary>
        /// Convert the file to text.
        /// </summary>
        /// <returns>The file as text.</returns>
        public new string[] ToText()
        {
            //Command list.
            List<string> l = new List<string>();

            //Add header.
            l.Add(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            l.Add(";");
            l.Add("; " + Name + ".smft");
            l.Add(";     Generated By Nitro Studio 2");
            l.Add(";");
            l.Add(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            l.Add("");

            //For each command. Last one isn't counted.
            for (int i = 0; i < Commands.Count - 1; i++)
            {

                //Add labels.
                bool labelAdded = false;
                var labels = PublicLabels.Where(x => x.Value == i).Select(x => x.Key);
                bool label0Added = false;
                foreach (var label in labels)
                {
                    if (i != 0 && !labelAdded && Commands[i - 1].CommandType == SequenceCommands.Fin)
                    {
                        l.Add(" ");
                    }
                    if (i == 0)
                    {
                        label0Added = true;
                    }
                    l.Add(label + ":");
                    labelAdded = true;
                }
                if (OtherLabels.Contains(i))
                {
                    if (i != 0 && !labelAdded && Commands[i - 1].CommandType == SequenceCommands.Fin)
                    {
                        l.Add(" ");
                    }
                    if (i != 0) { l.Add("_command_" + i + ":"); }
                    //else if (Commands[i - 1].CommandType == SequenceCommands.Fin) { l.Add("Command_" + i + ":"); }
                    labelAdded = true;
                }
                if (i == 0 && !label0Added)
                {
                    l.Add("Sequence_Start:");
                }

                //Add command.
                l.Add("\t" + Commands[i].ToString());

            }

            //Return the list.
            return [.. l];

        }

        /// <summary>
        /// Sequence platform.
        /// </summary>
        /// <returns>The sequence platform.</returns>
        public override SequencePlatform Platform() => new Nitro();
    }
}
