using Infrastructure.CAN;
using Core.Devices;
using Infrastructure.Devices;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Queries;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Core.Devices.Shared.Queries;
using Infrastructure.Commands;
using Core.Shared.Queries;
using Core.Shared.Commands;
using Core.Sensors;
using Core.Shared.CAN;
using Infrastructure.BackgroundServices;
using Core.Shared;
using Infrastructure.Shared;
using Core.Telemetry;
using Core.ControlDevices;

namespace Infrastructure
{
    public static class InfrastructureCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
#if DEBUG
            services.AddSingleton<ICanService, SimulatedCanService>();
#else
            services.AddSingleton<ICanService, CanService>();
#endif
            services.AddSingleton<CanHandler, CanHandlerImplmentation>();
            services.AddSingleton<ISensorRepository, SensorRepository>();
            services.AddSingleton<IControlDeviceRepository, ControlDeviceRepository>();
            services.AddSingleton<ISystemRepository, SystemRepository>();
            services.AddSingleton<ITelemetryService, TelemetryService>();
            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<FetchSensorDataService>();
            services.AddHostedService<SystemTelemetryService>();
            services.AddHostedService<SystemSchedulingService>();
#if !DEBUG
            services.AddHostedService<LcdScreenService>();
            services.AddHostedService<CoolingFanControlService>();
#endif

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IQueryDispatcher, QueryDispatcher>();
            services.TryAddSingleton<ICommandDispatcher, CommandDispatcher>();

            //services.AddScoped<IQueryHandler<GetAllDevicesQuery, List<Device>>, GetAllDevicesQueryHandler>();

            services.Scan(selector =>
            {
                selector.FromCallingAssembly()
                    .AddClasses(filter =>
                    {
                        filter.AssignableTo(typeof(IQueryHandler<,>));
                    })
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime();
            });

            services.Scan(selector =>
            {
                selector.FromAssemblyOf<ICommand>()
                    .AddClasses(filter =>
                    {
                        filter.AssignableTo(typeof(ICommandHandler<,>));
                    })
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime();
            });

            return services;
        }
    }
}
