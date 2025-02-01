namespace CodeDesignPlus.Net.Microservice.Tenants.Application;

public class Errors : IErrorCodes
{
    public const string UnknownError = "200 : UnknownError";
    public const string InvalidRequest = "201 : The request is invalid.";
    public const string TenantAlreadyExists = "202 : The tenant already exists.";
    public const string TenantNotFound = "203 : The tenant was not found.";
}
