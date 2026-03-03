using GeoDynamics.Functions;
using GJ.GeoDynamics.Domain;
using GJ.GeoDynamics.Infra;
using GJ.GeoDynamics.Infra.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["SqlConnectionString"];

        services.AddSingleton<IDatabaseRepository>(sp => new DatabaseRepository(sp.GetRequiredService<ILogger<DatabaseRepository>>(), connectionString ?? throw new InvalidOperationException("SqlConnectionString is missing")));
        services.AddHttpClient<ISoapClient, SoapClient>();
        
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleQueryRepository, VehicleQueryRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();

        services.AddScoped<IPoiRepository, PoiRepository>();
        services.AddScoped<IClockingRepository, ClockingRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();

        services.AddScoped<ITripOverviewRepository, TripOverviewRepository>();

        services.AddScoped<IGeodynamicsTransferService, GeodynamicsTransferService>();



        services.AddScoped<IGeodynamicsTransferService, GeodynamicsTransferService>();
    })
    .Build();

host.Run();