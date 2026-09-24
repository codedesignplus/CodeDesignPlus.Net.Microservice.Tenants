using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("200");
    public static readonly Error InvalidRequest = new("201");
    public static readonly Error TenantAlreadyExists = new("202");
    public static readonly Error TenantNotFound = new("203");
    public static readonly Error DuplicateDocument = new("204");
}
