using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace MiUtil.HexS19
{
    public static class HexS19
    {
        private static readonly Dictionary<string, byte> AddressTypeLength = new Dictionary<string, byte>
        {
            {"S0", 0 },{"S1", 2 },{"S2", 3 },{"S3", 4 },{"S7", 0 },{"S8", 0 },{"S9", 0 }
        };

        public static List<S19Block> ConvertS19(string path)
        {
            var lines = ConvertFileToLines(path).Select(line => ConvertDataStrToS19Line(line)).ToList();
            lines.RemoveAll(i => i == null);

            List<S19Block> s19Blocks = new List<S19Block>();
            List<byte> s19BlockData = new List<byte>();

            int currentBlockLength = 0;
            byte[] currentBlockLengthByte = new byte[4];
            S19Line currentBlockFirstLine = lines[0];

            for (int lineNum = 0; lineNum < lines.Count; lineNum++)
            {
                if (lines[lineNum] == null) continue;

                s19BlockData.AddRange(lines[lineNum].Data);

                checked { currentBlockLength += lines[lineNum].Data.Count; }

                if (lineNum + 1 == lines.Count || lines[lineNum].AddressUInt + lines[lineNum].Data.Count != lines[lineNum + 1].AddressUInt)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        currentBlockLengthByte[3 - i] = (byte)((currentBlockLength & (0xFF * (uint)Math.Pow(0x100, i))) >> 8 * i);
                    }

                    s19Blocks.Add(new S19Block(currentBlockFirstLine.Address, s19BlockData.AsReadOnly(), Array.AsReadOnly(currentBlockLengthByte)));

                    if (lineNum + 1 == lines.Count)
                    {
                        break;
                    }
                    currentBlockFirstLine = lines[lineNum + 1];
                    s19BlockData.Clear();
                    currentBlockLength = 0;
                }
            }

            return s19Blocks;
        }

        private static List<string> ConvertFileToLines(string path)
        {
            List<string> strLines = new List<string>();

            using (FileStream fs = File.OpenRead(path))
            {
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                while (!sr.EndOfStream)
                {
                    strLines.Add(sr.ReadLine());
                }
            }

            return strLines;
        }

        private static S19Line ConvertDataStrToS19Line(string line)
        {
            var list = Regex.Matches(line, @"\w\w").Cast<Match>().Select(m => m.Value).ToList();
            byte addressLength = AddressTypeLength[list[0]];

            if (addressLength == 0) return null;

            list.RemoveAt(0);
            var data = list.Select(c => Convert.ToByte(c, 16)).ToList();
            byte rawChecksum = data.Last();
            data.RemoveAt(data.Count - 1);

            if ((0xFF - (byte)data.Sum(d => d)) != rawChecksum) throw new BadChecksumException();

            data.RemoveAt(0);
            return new S19Line(data.GetRange(0, addressLength).ToArray(), data.GetRange(addressLength, data.Count - addressLength).ToArray());
        }
    }

    internal class S19Line
    {
        public ReadOnlyCollection<byte> Address { get; }

        public ReadOnlyCollection<byte> Data { get; }

        public uint AddressUInt { get; private set; }

        internal S19Line(IEnumerable<byte> address, IEnumerable<byte> data)
        {
            Address = address.ToList().AsReadOnly();
            Data = data.ToList().AsReadOnly();
            int i = 0;
            address.Reverse().ToList().ForEach(n => { AddressUInt += n * (uint)Math.Pow(0x100, i); i++; });
        }
    }
}