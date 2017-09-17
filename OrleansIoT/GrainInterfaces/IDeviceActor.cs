using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface IDeviceActor : Orleans.IGrainWithStringKey
    {
        Task Start();
        Task Stop();
        Task Pause();
        Task Resume();
        Task SetFluxCapacitance(int farads);
        Task SetGravitationalIntegrity(double units);
        Task ReportStatus();
    }
}
