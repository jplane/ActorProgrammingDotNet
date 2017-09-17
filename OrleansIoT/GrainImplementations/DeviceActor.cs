using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrainInterfaces;
using Orleans;
using Orleans.Providers;

namespace GrainImplementations
{
    public enum DeviceStatus
    {
        Stopped,
        Running,
        Paused
    }

    public class DeviceState
    {
        public DeviceState()
        {
        }

        public DeviceStatus State { get; set; }
        public DateTimeOffset? Started { get; set; }
        public int FluxCapacitance { get; set; }
        public double GravitationalIntegrity { get; set; }
    }

    [StorageProvider(ProviderName = "VolatileStore")]
    public class DeviceActor : Orleans.Grain<DeviceState>, IDeviceActor
    {
        public DeviceActor()
        {
        }

        public Task Start()
        {
            if (this.State.State != DeviceStatus.Stopped)
            {
                throw new InvalidOperationException("Device not stopped.");
            }

            this.State.Started = DateTimeOffset.UtcNow;

            this.State.State = DeviceStatus.Running;

            this.State.FluxCapacitance = 1;

            this.State.GravitationalIntegrity = 1;

            Console.WriteLine($"Device {this.GetPrimaryKeyString()} started.");

            return base.WriteStateAsync();
        }

        public Task Stop()
        {
            if (this.State.State == DeviceStatus.Stopped)
            {
                throw new InvalidOperationException("Device is already stopped.");
            }

            this.State.Started = null;

            this.State.State = DeviceStatus.Stopped;

            Console.WriteLine($"Device {this.GetPrimaryKeyString()} stopped.");

            return base.WriteStateAsync();
        }

        public Task Pause()
        {
            if (this.State.State != DeviceStatus.Running)
            {
                throw new InvalidOperationException("Device not running.");
            }

            this.State.State = DeviceStatus.Paused;

            Console.WriteLine($"Device {this.GetPrimaryKeyString()} paused.");

            return base.WriteStateAsync();
        }

        public Task Resume()
        {
            if (this.State.State != DeviceStatus.Paused)
            {
                throw new InvalidOperationException("Device not paused.");
            }

            this.State.State = DeviceStatus.Running;

            Console.WriteLine($"Device {this.GetPrimaryKeyString()} resumed.");

            return base.WriteStateAsync();
        }

        public Task SetFluxCapacitance(int farads)
        {
            if (this.State.State != DeviceStatus.Running)
            {
                throw new InvalidOperationException("Cannot change state; device is not running.");
            }

            this.State.FluxCapacitance = farads;

            Console.WriteLine($"Device {this.GetPrimaryKeyString()} flux capacitance set to {farads}.");

            return base.WriteStateAsync();
        }

        public Task SetGravitationalIntegrity(double units)
        {
            if (this.State.State != DeviceStatus.Running)
            {
                throw new InvalidOperationException("Cannot change state; device is not running.");
            }

            this.State.GravitationalIntegrity = units;

            Console.WriteLine($"Device {this.GetPrimaryKeyString()} gravitational integrity set to {units}.");

            return base.WriteStateAsync();
        }

        public Task ReportStatus()
        {
            var uptime = this.State.Started == null ? TimeSpan.Zero : DateTimeOffset.UtcNow.Subtract(this.State.Started.Value);

            Console.WriteLine(
                $"Device id = {this.GetPrimaryKeyString()}, state = {this.State.State}, uptime = {uptime}, flux capacitance = {this.State.FluxCapacitance}, grav. integrity = {this.State.GravitationalIntegrity}");

            return Task.CompletedTask;
        }
    }
}
