﻿using System.IO;
using WwiseParserLib.Structures.Sections;

namespace WwiseParserLib.Structures
{
    public class InMemorySoundBank : SoundBank
    {
        private readonly byte[] _blob = null;

        public InMemorySoundBank(byte[] blob) : base()
        {
            _blob = blob;
        }

        public override byte[] ReadSection(SoundBankSectionName name)
        {
            using (var reader = new BinaryReader(new MemoryStream(_blob)))
            {
                while (reader.PeekChar() > -1)
                {
                    var sectionName = reader.ReadUInt32();
                    var sectionLength = reader.ReadUInt32();

                    if (sectionName == (uint)name)
                    {
                        // Section found
                        return reader.ReadBytes((int)sectionLength);
                    }
                    else
                    {
                        // Not the section we're looking for
                        reader.BaseStream.Seek(sectionLength, SeekOrigin.Current);
                    }
                }

                // Section does not exist
                return null;
            }
        }
    }
}
