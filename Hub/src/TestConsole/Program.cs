using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Core.Shared.CAN;
using Infrastructure;

namespace TestConsole
{
    internal class Program
    {
        record TestDevice(uint uniqueId, byte deviceType, byte deviceId);

        private static ICanService canService = null!;
        private static Dictionary<uint, TestDevice> devices = new Dictionary<uint, TestDevice>();
        static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddInfrastructure()
                .BuildServiceProvider();

            canService = services.GetRequiredService<ICanService>();
            canService.CanMessageReceived += CanService_CanMessageReceived;

            Console.WriteLine("Hello, World!");
            Console.WriteLine();

            canService.Start();

            string? cmd = Console.ReadLine() ?? "";
            
            while(!cmd.Equals("/exit"))
            {
                try
                {
                    if(cmd.Equals("list"))
                    {
                        foreach(var device in devices.Keys)
                        {
                            Console.WriteLine($"Device {device} {devices[device]}");
                        }
                        Console.WriteLine();
                    }
                    else if (cmd.Equals("ping"))
                    {
                        CanMessage reqData = new CanMessage(CanMessageType.Ping, 0, Array.Empty<byte>());
                        canService.SendCanMessage(reqData);
                    }
                    else if (cmd.Contains(':') && cmd.Split(":")[0].Equals("cmd"))
                    {
                        //ex: cmd:127:0 23
                        byte deviceId = byte.Parse(cmd.Split(":")[1]);
                        byte cmdType = byte.Parse(cmd.Split(":")[2].Split(" ")[0]);
                        byte cmdData = byte.Parse(cmd.Split(":")[2].Split(" ")[1]);
                        byte[] cmdArr = new byte[2];
                        cmdArr[0] = cmdType;
                        cmdArr[1] = cmdData;
                        CanMessage reqData = new CanMessage(CanMessageType.Command, deviceId, cmdArr);
                        canService.SendCanMessage(reqData);
                    }
                    else
                    {
                        //data:51 example
                        byte deviceId = byte.Parse(cmd.Split(":")[1]);
                        byte dataSource = 0;
                        if(cmd.Split(":").Length > 2)
                        {
                            dataSource = byte.Parse(cmd.Split(":")[2]);
                        }
                        CanMessage reqData = new CanMessage(CanMessageType.Data, deviceId, [dataSource]);
                        canService.SendCanMessage(reqData);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error in comand: {e}");
                }
                
                cmd = Console.ReadLine() ?? "";
            }

            canService.Stop();

        }

        private static void CanService_CanMessageReceived(object? sender, CanMessage e)
        {
            Console.WriteLine(e);

            if(e.MessageType == CanMessageType.RegisterRequest)
            {
                byte deviceId = (byte)devices.Count;
                deviceId += 50;
                var devieType = e.Data.Last();
                byte[] paddedArr = new byte[4];
                for (int i = 0; i < e.Data.Length-1; i++)
                {
                    paddedArr[i] = e.Data[i];
                }
                var uniqueId = BitConverter.ToUInt32(paddedArr);
                Console.WriteLine($"Device type: {devieType}, uinqueId: {uniqueId}");

                byte[] respArr = new byte[5];
                for(int i = 0; i < e.Data.Length-1; i++)
                {
                    respArr[i] = e.Data[i];
                }
                respArr[4] = deviceId;
                Task.Delay(200);
                CanMessage response = new CanMessage(CanMessageType.RegisterResponse, 0, respArr);
                canService.SendCanMessage(response);
                if(!devices.Any(d => d.Value.uniqueId == uniqueId))
                {
                    devices.Add(deviceId, new(uniqueId, devieType, deviceId));
                }
            }

            if (e.MessageType == CanMessageType.Ping)
            {
                var devieType = e.Data.Last();
                byte[] paddedArr = new byte[4];
                paddedArr[0] = e.Data[0];
                paddedArr[1] = e.Data[1];
                var uniqueId = BitConverter.ToUInt32(paddedArr);
                
                Console.WriteLine($"Device type: {devieType}, canId: {e.DeviceID}, uinqueId: {uniqueId}");
                
                if(!devices.ContainsKey(e.DeviceID))
                {
                    devices.Add(e.DeviceID, new(uniqueId, devieType, e.DeviceID));
                }
            }

            if (e.MessageType == CanMessageType.Data)
            {
                byte[] value = new byte[4];
                Array.Copy(e.Data, value, 2);
                int val = BitConverter.ToInt32(value);
                Console.WriteLine($"data int: {val}");
                float dec = ((float)val) / 100f;
                Console.WriteLine($"data float: {dec}");
            }
        }
    }
}
