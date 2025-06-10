namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure;

public class Errors: IErrorCodes
{    
    public const string UnknownError = "300 : UnknownError";
    public const string TenantIdIsInvalid = "301 : TenantIdIsInvalid";
}
