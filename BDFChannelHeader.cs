using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    class BDFChannelHeader
    {
        public string Label { get; set; }
        public string TransuderType { get; set; }
        public string Dimension { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int DigitalMin { get; set; }
        public int DigitalMax { get; set; }
        public string Prefiltered { get; set; }
        public int SamplesPerDataRecord { get; set; }

        public bool Equals(BDFChannelHeader header)
        {
            if (header == null)
                return false;
            if (!this.Label.Equals(header.Label))
                return false;
            if (!this.TransuderType.Equals(header.TransuderType))
                return false;
            if (!this.Dimension.Equals(header.Dimension))
                return false;
            if (this.MinValue != header.MinValue)
                return false;
            if (this.MaxValue != header.MaxValue)
                return false;
            if (this.DigitalMin != header.DigitalMin)
                return false;
            if (this.DigitalMax != header.DigitalMax)
                return false;
            if (!this.Prefiltered.Equals(header.Prefiltered))
                return false;
            if (this.SamplesPerDataRecord != header.SamplesPerDataRecord)
                return false;
            return true;
        }
    }
}
