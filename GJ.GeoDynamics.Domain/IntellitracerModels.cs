using System.ServiceModel;
using System.ServiceModel.Channels;

namespace GJ.GeoDynamics.Domain;

[ServiceContract(Namespace = "http://tempuri.org/")]
public interface IntegratorWebserviceSoap
{
    [OperationContract(Action = "http://tempuri.org/User_GetAll", ReplyAction = "*")]
    Task<UserGetAllResponse> User_GetAllAsync(CallerEntity caller);

    [OperationContract(Action = "http://tempuri.org/Vehicle_GetAll", ReplyAction = "*")]
    Task<VehicleGetAllResponse> Vehicle_GetAllAsync(CallerEntity caller);

    [OperationContract(Action = "http://tempuri.org/Poi_GetAll", ReplyAction = "*")]
    Task<PoiGetAllResponse> Poi_GetAllAsync(CallerEntity caller);

    [OperationContract(Action = "http://tempuri.org/Clocking_GetByDateRangeUtc", ReplyAction = "*")]
    Task<ClockingGetByDateRangeUtcResponse> Clocking_GetByDateRangeUtcAsync(CallerEntity caller, DateTime start, DateTime end);

    [OperationContract(Action = "http://tempuri.org/Location_GetByVehicleIdDateRange", ReplyAction = "*")]
    Task<LocationGetByVehicleIdDateRangeResponse> Location_GetByVehicleIdDateRangeAsync(CallerEntity caller, string vehicleId, DateTime start, DateTime end);

    [OperationContract(Action = "http://tempuri.org/TimeSheet_Overview_GetByUserIdListDateRange", ReplyAction = "*")]
    Task<TimeSheetOverviewGetByUserIdListDateRangeResponse> TimeSheet_Overview_GetByUserIdListDateRangeAsync(CallerEntity caller, ArrayOfGuid userGuids, DateTime start, DateTime end, TimesheetOverviewOptions options);
}

public class IntegratorWebserviceSoapClient : ClientBase<IntegratorWebserviceSoap>, IntegratorWebserviceSoap
{
    public IntegratorWebserviceSoapClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
    {
    }

    public Task<UserGetAllResponse> User_GetAllAsync(CallerEntity caller)
    {
        return Channel.User_GetAllAsync(caller);
    }

    public Task<VehicleGetAllResponse> Vehicle_GetAllAsync(CallerEntity caller)
    {
        return Channel.Vehicle_GetAllAsync(caller);
    }

    public Task<PoiGetAllResponse> Poi_GetAllAsync(CallerEntity caller)
    {
        return Channel.Poi_GetAllAsync(caller);
    }

    public Task<ClockingGetByDateRangeUtcResponse> Clocking_GetByDateRangeUtcAsync(CallerEntity caller, DateTime start, DateTime end)
    {
        return Channel.Clocking_GetByDateRangeUtcAsync(caller, start, end);
    }

    public Task<LocationGetByVehicleIdDateRangeResponse> Location_GetByVehicleIdDateRangeAsync(CallerEntity caller, string vehicleId, DateTime start, DateTime end)
    {
        return Channel.Location_GetByVehicleIdDateRangeAsync(caller, vehicleId, start, end);
    }

    public Task<TimeSheetOverviewGetByUserIdListDateRangeResponse> TimeSheet_Overview_GetByUserIdListDateRangeAsync(CallerEntity caller, ArrayOfGuid userGuids, DateTime start, DateTime end, TimesheetOverviewOptions options)
    {
        return Channel.TimeSheet_Overview_GetByUserIdListDateRangeAsync(caller, userGuids, start, end, options);
    }
}

public partial class UserGetAllResponse
{
    public User_GetAllResponseBody Body { get; set; }
}

public class User_GetAllResponseBody
{
    public UserEntity[] User_GetAllResult { get; set; }
}

public partial class VehicleGetAllResponse
{
    public Vehicle_GetAllResponseBody Body { get; set; }
}

public class Vehicle_GetAllResponseBody
{
    public VehicleEntity[] Vehicle_GetAllResult { get; set; }
}

public class LastPositionEntity
{
    public AddressEntity AddressEntity { get; set; }
}

public class ClockingGetByDateRangeUtcResponse
{
    public ClockingGetByDateRangeUtcResponseBody Body { get; set; }
}

public class ClockingGetByDateRangeUtcResponseBody
{
    public ClockingEntity[] Clocking_GetByDateRangeUtcResult { get; set; }
}

public class LocationGetByVehicleIdDateRangeResponse
{
    public LocationGetByVehicleIdDateRangeResponseBody Body { get; set; }
}

public class LocationGetByVehicleIdDateRangeResponseBody
{
    public LocationEntity[] LocationGetByVehicleIdDateRangeResult { get; set; }
}

public class TimeSheetOverviewGetByUserIdListDateRangeResponse
{
    public TimeSheetOverviewGetByUserIdListDateRangeResponseBody Body { get; set; }
}

public class TimeSheetOverviewGetByUserIdListDateRangeResponseBody
{
    public TimeSheetDayEntity[] TimeSheetOverviewGetByUserIdListDateRangeResult { get; set; }
}

public class ArrayOfGuid : List<string>
{
}