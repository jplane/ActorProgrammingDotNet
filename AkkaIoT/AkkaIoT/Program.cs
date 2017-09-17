using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaIoT.Messages;

namespace AkkaIoT
{
    class Program
    {
        private static readonly Dictionary<string, IActorRef> _devices = new Dictionary<string, IActorRef>();
        private static readonly ActorSystem _system = ActorSystem.Create("iot-system");

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to IoT Command Central. Enter a command or type 'help' for more info.");
            Console.WriteLine();

            while (true)
            {
                var cmd = Console.ReadLine();

                Func<string[]> getParts = () => cmd.Split(' ').Skip(1).ToArray();

                if (cmd.ToLowerInvariant() == "help")
                {
                    Help();
                }
                else if (cmd.ToLowerInvariant() == "exit")
                {
                    return;
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

                Thread.Sleep(250);

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

            if (device == null)
            {
                Console.Error.WriteLine($"Device id {id} not found.");
            }
            else
            {
                device.Tell(new SetGravitationalIntegrityMessage(units));
            }
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

            if (device == null)
            {
                Console.Error.WriteLine($"Device id {id} not found.");
            }
            else
            {
                device.Tell(new SetFluxCapacitanceMessage(farads));
            }
        }

        private static void DeviceStatus(string[] parts)
        {
            Action<string> reportStatus = id =>
            {
                var device = GetDevice(id);

                if (device == null)
                {
                    Console.Error.WriteLine($"Device id {id} not found.");
                }
                else
                {
                    device.Tell(new ReportStatusMessage());
                }
            };

            if (parts.Length > 0)
            {
                var id = parts[0];
                reportStatus(id);
            }
            else
            {
                foreach (var id in _devices.Keys)
                {
                    reportStatus(id);
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

            if (device == null)
            {
                Console.Error.WriteLine($"Device id {id} not found.");
            }
            else
            {
                device.Tell(new ResumeMessage());
            }
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

            if (device == null)
            {
                Console.Error.WriteLine($"Device id {id} not found.");
            }
            else
            {
                device.Tell(new PauseMessage());
            }
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

            if (device == null)
            {
                Console.Error.WriteLine($"Device id {id} not found.");
            }
            else
            {
                device.Tell(new StopMessage());
            }
        }

        private static void StartDevice(string[] parts)
        {
            if (parts.Length < 1)
            {
                Console.Error.WriteLine("Specify a device id.");
                return;
            }

            var id = parts[0];

            var device = GetDevice(id);

            if (device == null)
            {
                Console.Error.WriteLine($"Device id {id} not found.");
            }
            else
            {
                device.Tell(new StartMessage());
            }
        }

        private static void KillDevice(string[] parts)
        {
            if (parts.Length < 1)
            {
                Console.Error.WriteLine("Specify a device id.");
                return;
            }

            var id = parts[0];

            var device = GetDevice(id);

            if (device == null)
            {
                Console.Error.WriteLine($"Device id {id} not found.");
            }
            else
            {
                device.GracefulStop(TimeSpan.FromSeconds(5)).Wait();

                _devices.Remove(id);

                Console.WriteLine($"Device '{id}' removed.");
            }
        }

        private static IActorRef GetDevice(string id)
        {
            IActorRef device = null;

            _devices.TryGetValue(id, out device);

            return device;
        }

        private static void NewDevice(string[] parts)
        {
            var id = Guid.NewGuid().ToString();

            if (parts.Length > 0)
            {
                id = parts[0];
            }

            var device = _system.ActorOf(Props.Create<DeviceActor>(id), id);

            _devices.Add(id, device);

            Console.WriteLine($"Device '{id}' created.");
        }

        private static void ListDevices()
        {
            Console.WriteLine($"{_devices.Count} total devices:");

            foreach (var device in _devices.Keys)
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
