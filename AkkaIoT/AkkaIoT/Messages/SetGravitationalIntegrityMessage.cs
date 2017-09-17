using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaIoT.Messages
{
    public class SetGravitationalIntegrityMessage
    {
        public SetGravitationalIntegrityMessage(double units)
        {
            this.Units = units;
        }

        public double Units { get; }
    }
}
