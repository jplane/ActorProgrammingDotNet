using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using DeviceActor.Interfaces;
using Newtonsoft.Json;

namespace DeviceActor
{
    [StatePersistence(StatePersistence.Volatile)]
    internal class DeviceActor : Actor, IDeviceActor
    {
        private enum State
        {
            Stopped,
            Running,
            Paused
        }

        public DeviceActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            var task1 = this.StateManager.SetStateAsync("state", State.Stopped);
            var task2 = this.StateManager.SetStateAsync("started", (DateTimeOffset?) null);
            var task3 = this.StateManager.SetStateAsync("fluxCapacitance", 0);
            var task4 = this.StateManager.SetStateAsync("gravitationalIntegrity", 0d);

            return Task.WhenAll(task1, task2, task3, task4);
        }

        async Task IDeviceActor.Pause()
        {
            var state = await this.StateManager.GetStateAsync<State>("state");

            if (state != State.Running)
            {
                throw new InvalidOperationException("Device not running.");
            }

            await this.StateManager.SetStateAsync("state", State.Paused);

            await this.StateManager.SaveStateAsync();

            Console.WriteLine($"Device {this.Id} paused.");
        }

        async Task<string> IDeviceActor.ReportStatus()
        {
            var started = await this.StateManager.GetStateAsync<DateTimeOffset?>("started");

            var uptime = started == null ? TimeSpan.Zero : DateTimeOffset.UtcNow.Subtract(started.Value);

            var status = new
            {
                id = this.Id.ToString(),
                state = (await this.StateManager.GetStateAsync<State>("state")).ToString(),
                uptime = uptime,
                fluxCapacitance = await this.StateManager.GetStateAsync<int>("fluxCapacitance"),
                gravitationalIntegrity = await this.StateManager.GetStateAsync<double>("gravitationalIntegrity")
            };

            return JsonConvert.SerializeObject(status);
        }

        async Task IDeviceActor.Resume()
        {
            var state = await this.StateManager.GetStateAsync<State>("state");

            if (state != State.Paused)
            {
                throw new InvalidOperationException("Device not paused.");
            }

            await this.StateManager.SetStateAsync("state", State.Running);

            await this.StateManager.SaveStateAsync();

            Console.WriteLine($"Device {this.Id} resumed.");
        }

        async Task IDeviceActor.SetFluxCapacitance(int farads)
        {
            var state = await this.StateManager.GetStateAsync<State>("state");

            if (state != State.Running)
            {
                throw new InvalidOperationException("Cannot change state; device is not running.");
            }

            await this.StateManager.SetStateAsync("fluxCapacitance", farads);

            await this.StateManager.SaveStateAsync();

            Console.WriteLine($"Device {this.Id} flux capacitance set to {farads}.");
        }

        async Task IDeviceActor.SetGravitationalIntegrity(double units)
        {
            var state = await this.StateManager.GetStateAsync<State>("state");

            if (state != State.Running)
            {
                throw new InvalidOperationException("Cannot change state; device is not running.");
            }

            await this.StateManager.SetStateAsync("gravitationalIntegrity", units);

            await this.StateManager.SaveStateAsync();

            Console.WriteLine($"Device {this.Id} gravitational integrity set to {units}.");
        }

        async Task IDeviceActor.Start()
        {
            var state = await this.StateManager.GetStateAsync<State>("state");

            if (state != State.Stopped)
            {
                throw new InvalidOperationException("Device not stopped.");
            }

            await this.StateManager.SetStateAsync("state", State.Running);
            await this.StateManager.SetStateAsync("started", DateTimeOffset.UtcNow);
            await this.StateManager.SetStateAsync("fluxCapacitance", 1);
            await this.StateManager.SetStateAsync("gravitationalIntegrity", 1d);

            await this.StateManager.SaveStateAsync();

            Console.WriteLine($"Device {this.Id} started.");
        }

        async Task IDeviceActor.Stop()
        {
            var state = await this.StateManager.GetStateAsync<State>("state");

            if (state == State.Stopped)
            {
                throw new InvalidOperationException("Device is already stopped.");
            }

            await this.StateManager.SetStateAsync("state", State.Stopped);
            await this.StateManager.SetStateAsync("started", (DateTimeOffset?) null);

            await this.StateManager.SaveStateAsync();

            Console.WriteLine($"Device {this.Id} stopped.");
        }
    }
}
