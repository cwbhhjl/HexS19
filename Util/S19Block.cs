using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MiUtil.HexS19
{
    public class S19Block
    {
        public ReadOnlyCollection<byte> Address { get; }

        public ReadOnlyCollection<byte> Length { get; }

        public ReadOnlyCollection<byte> Data { get; }

        public S19Block(IEnumerable<byte> address, IEnumerable<byte> data, IEnumerable<byte> dataLength)
        {
            Address = address.ToList().AsReadOnly();
            Data = data.ToList().AsReadOnly();
            Length = dataLength.ToList().AsReadOnly();
        }
    }
}
