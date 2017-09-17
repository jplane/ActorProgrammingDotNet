using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaIoT.Messages;

namespace AkkaIoT
{
    public class DeviceActor : ReceiveActor
    {
        private readonly string _id;

        private string _state = "stopped";
        private DateTimeOffset? _started;
        private int _fluxCapacitance = 0;
        private double _gravitationalIntegrity = 0;

        public DeviceActor(string id)
        {
            _id = id;
            CanStart();
        }

        private void CanStart()
        {
            ReceiveAsync<ReportStatusMessage>(ReportStatus);
            ReceiveAsync<StartMessage>(Start);

            Receive<StopMessage>(_ => Console.Error.WriteLine("Device is already stopped."));
            Receive<PauseMessage>(_ => Console.Error.WriteLine("Device is not running."));
            Receive<ResumeMessage>(_ => Console.Error.WriteLine("Device is not paused."));
            Receive<SetFluxCapacitanceMessage>(_ => Console.Error.WriteLine("Cannot change device state while it is stopped."));
            Receive<SetGravitationalIntegrityMessage>(_ => Console.Error.WriteLine("Cannot change device state while it is stopped."));
        }

        private void CanStopOrPause()
        {
            ReceiveAsync<ReportStatusMessage>(ReportStatus);
            ReceiveAsync<StopMessage>(Stop);
            ReceiveAsync<PauseMessage>(Pause);
            ReceiveAsync<SetFluxCapacitanceMessage>(SetFluxCapacitance);
            ReceiveAsync<SetGravitationalIntegrityMessage>(SetGravitationalIntegrity);

            Receive<StartMessage>(_ => Console.Error.WriteLine("Device is already started."));
            Receive<ResumeMessage>(_ => Console.Error.WriteLine("Device is not paused."));
        }

        private void CanStopOrResume()
        {
            ReceiveAsync<ReportStatusMessage>(ReportStatus);
            ReceiveAsync<StopMessage>(Stop);
            ReceiveAsync<ResumeMessage>(Resume);

            Receive<StartMessage>(_ => Console.Error.WriteLine("Device is already started."));
            Receive<PauseMessage>(_ => Console.Error.WriteLine("Device is already paused."));
            Receive<SetFluxCapacitanceMessage>(_ => Console.Error.WriteLine("Cannot change device state while it is paused."));
            Receive<SetGravitationalIntegrityMessage>(_ => Console.Error.WriteLine("Cannot change device state while it is paused."));
        }

        private Task ReportStatus(ReportStatusMessage msg)
        {
            var uptime = _started == null ? TimeSpan.Zero : DateTimeOffset.UtcNow.Subtract(_started.Value);

            Console.WriteLine(
                $"Device id = {_id}, state = {_state}, uptime = {uptime}, flux capacitance = {_fluxCapacitance}, grav. integrity = {_gravitationalIntegrity}");

            return Task.CompletedTask;
        }

        private Task Start(StartMessage msg)
        {
            _started = DateTimeOffset.UtcNow;

            _state = "running";

            _fluxCapacitance = 1;

            _gravitationalIntegrity = 1;

            Become(CanStopOrPause);

            Console.WriteLine($"Device '{_id}' started.");

            return Task.CompletedTask;
        }

        private Task Stop(StopMessage msg)
        {
            _started = null;

            _state = "stopped";

            Become(CanStart);

            Console.WriteLine($"Device '{_id}' stopped.");

            return Task.CompletedTask;
        }

        private Task Pause(PauseMessage msg)
        {
            _state = "paused";

            Become(CanStopOrResume);

            Console.WriteLine($"Device '{_id}' paused.");

            return Task.CompletedTask;
        }

        private Task Resume(ResumeMessage msg)
        {
            _state = "running";

            Become(CanStopOrPause);

            Console.WriteLine($"Device '{_id}' resumed.");

            return Task.CompletedTask;
        }

        private Task SetFluxCapacitance(SetFluxCapacitanceMessage msg)
        {
            _fluxCapacitance = msg.Farads;

            Console.WriteLine($"Flux capacitance set to {msg.Farads} for device '{_id}'.");

            return Task.CompletedTask;
        }

        private Task SetGravitationalIntegrity(SetGravitationalIntegrityMessage msg)
        {
            _gravitationalIntegrity = msg.Units;

            Console.WriteLine($"Gravitational integrity set to {msg.Units} for device '{_id}'.");

            return Task.CompletedTask;
        }
    }
}
