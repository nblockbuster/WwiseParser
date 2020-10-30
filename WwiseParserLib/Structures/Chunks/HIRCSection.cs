﻿using WwiseParserLib.Structures.Objects.HIRC;

namespace WwiseParserLib.Structures.Chunks
{
    public class HIRCSection : SoundBankSection
    {
        public HIRCSection(int length) : base(SoundBankSectionName.HIRC, (uint)length)
        {

        }

        /// <summary>
        /// <para>The count of objects in the section.</para>
        /// </summary>
        public uint ObjectCount { get; set; }

        /// <summary>
        /// <para>The objects in the section.</para>
        /// </summary>
        public HIRCObjectBase[] Objects { get; set; }
    }
}