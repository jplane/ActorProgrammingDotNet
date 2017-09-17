using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace DeviceActor.Interfaces
{
    public interface IDeviceActor : IActor
    {
        Task Start();
        Task Stop();
        Task Pause();
        Task Resume();
        Task SetFluxCapacitance(int farads);
        Task SetGravitationalIntegrity(double units);
        Task<string> ReportStatus();
    }
}
