using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaIoT.Messages
{
    public class SetFluxCapacitanceMessage
    {
        public SetFluxCapacitanceMessage(int farads)
        {
            this.Farads = farads;
        }

        public int Farads { get; }
    }
}
