using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure;

public class Errors: IErrorCodes
{    
    public static readonly Error UnknownError = new("300", "UnknownError");
    public static readonly Error TenantIdIsInvalid = new("301", "TenantIdIsInvalid");
}
