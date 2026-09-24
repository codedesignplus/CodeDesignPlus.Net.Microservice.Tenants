using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("200", "UnknownError");
    public static readonly Error InvalidRequest = new("201", "The request is invalid.");
    public static readonly Error TenantAlreadyExists = new("202", "The tenant already exists.");
    public static readonly Error TenantNotFound = new("203", "The tenant was not found.");
    public static readonly Error DuplicateDocument = new("204", "A tenant with the same document type and number already exists.");
}
