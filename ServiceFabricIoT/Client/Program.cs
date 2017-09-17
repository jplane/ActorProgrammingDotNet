using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using DeviceActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Client
{
    class Program
    {
        static HashSet<string> _devices = new HashSet<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to IoT Command Central. Enter a command or type 'help' for more info.");
            Console.WriteLine();

            while (true)
            {
                var cmd = Console.ReadLine();

                Func<string[]> getParts = () => cmd.Split(' ').Skip(1).ToArray();

                try
                {
                    if (cmd.ToLowerInvariant() == "help")
                    {
                        Help();
                    }
                    else if (cmd.ToLowerInvariant() == "exit")
                    {
                        break;
                    }
                    else if (cmd.ToLowerInvariant() == "list-devices")
                    {
                        ListDevices();
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("new-device"))
                    {
                        NewDevice(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("kill-device"))
                    {
                        KillDevice(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("start-device"))
                    {
                        StartDevice(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("stop-device"))
                    {
                        StopDevice(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("pause-device"))
                    {
                        PauseDevice(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("resume-device"))
                    {
                        ResumeDevice(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("device-status"))
                    {
                        DeviceStatus(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("set-flux-capacitance"))
                    {
                        SetFluxCapacitance(getParts());
                    }
                    else if (cmd.ToLowerInvariant().StartsWith("set-gravitational-integrity"))
                    {
                        SetGravitationalIntegrity(getParts());
                    }
                    else
                    {
                        Console.Error.WriteLine("Unknown command or device.");
                    }
                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Console.WriteLine();
            }
        }

        private static void SetGravitationalIntegrity(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.Error.WriteLine("Specify a device id and units.");
                return;
            }

            var id = parts[0];

            var units = double.Parse(parts[1]);

            var device = GetDevice(id);

            device.SetGravitationalIntegrity(units).Wait();
        }

        private static void SetFluxCapacitance(string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.Error.WriteLine("Specify a device id and farad count.");
                return;
            }

            var id = parts[0];

            var farads = int.Parse(parts[1]);

            var device = GetDevice(id);

            device.SetFluxCapacitance(farads).Wait();
        }

        private static void DeviceStatus(string[] parts)
        {
            Func<string, Task> reportStatus = async id =>
            {
                var device = GetDevice(id);

                dynamic status = JObject.Parse(await device.ReportStatus());

                Console.WriteLine(
                    $"Device id = {status.id}, state = {status.state}, uptime = {status.uptime}, flux capacitance = {status.fluxCapacitance}, grav. integrity = {status.gravitationalIntegrity}");
            };

            if (parts.Length > 0)
            {
                var id = parts[0];
                reportStatus(id).Wait();
            }
            else
            {
                foreach (var id in _devices)
                {
                    reportStatus(id).Wait();
                }
            }
        }

        private static void ResumeDevice(string[] parts)
        {
            if (parts.Length < 1)
            {
                Console.Error.WriteLine("Specify a device id.");
                return;
            }

            var id = parts[0];

            var device = GetDevice(id);

            device.Resume().Wait();
        }

        private static void PauseDevice(string[] parts)
        {
            if (parts.Length < 1)
            {
                Console.Error.WriteLine("Specify a device id.");
                return;
            }

            var id = parts[0];

            var device = GetDevice(id);

            device.Pause().Wait();
        }

        private static void StopDevice(string[] parts)
        {
            if (parts.Length < 1)
            {
                Console.Error.WriteLine("Specify a device id.");
                return;
            }

            var id = parts[0];

            var device = GetDevice(id);

            device.Stop().Wait();
        }

        private static void StartDevice(string[] parts)
        {
            if (parts.Length < 1)
            {
                Console.Error.WriteLine("Specify a device id.");
            }

            var id = parts[0];

            var device = GetDevice(id);

            device.Start().Wait();
        }

        private static void KillDevice(string[] parts)
        {
            Console.WriteLine("Not supported. SF does not support explicit lifetime management operations for actor instances.");
        }

        private static IDeviceActor GetDevice(string id)
        {
            var svcUri = ConfigurationManager.AppSettings["svcUri"];

            var device = ActorProxy.Create<IDeviceActor>(new ActorId(id), new Uri(svcUri));

            _devices.Add(id);

            return device;
        }

        private static void NewDevice(string[] parts)
        {
            Console.WriteLine("Not needed. SF creates actor instances as needed to satisfy client requests.");
        }

        private static void ListDevices()
        {
            Console.WriteLine($"{_devices.Count} total devices:");

            foreach (var device in _devices)
            {
                Console.WriteLine(device);
            }
        }

        private static void Help()
        {
            var text = @"IoT Command Central
===================

- help
- exit
- list-devices
- new-device <id>
- kill-device [id]
- start-device [id]
- stop-device [id]
- pause-device [id]
- resume-device [id]
- device-status <id>
- set-flux-capacitance [id] [farads]
- set-gravitational-integrity [id] [units]";

            Console.WriteLine(text);
        }
    }
}
