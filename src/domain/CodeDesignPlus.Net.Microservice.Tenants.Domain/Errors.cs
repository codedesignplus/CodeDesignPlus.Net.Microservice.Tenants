using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("100", "UnknownError");

    public static readonly Error LicenseIdIsEmpty = new("101", "The license id is invalid.");
    public static readonly Error LicenseNameIsEmpty = new("102", "The license name is invalid.");
    public static readonly Error LicenseStartDateGreaterThanEndDate = new("103", "The license start date is greater than the end date.");
    public static readonly Error LicenseMetadataIsNull = new("105", "The license metadata is null.");

    public static readonly Error LocalityNameIsEmpty = new("106", "The locality name is invalid.");
    public static readonly Error LocalityIdIsEmpty = new("107", "The locality id is invalid.");

    public static readonly Error NeighborhoodNameIsEmpty = new("108", "The neighborhood name is invalid.");
    public static readonly Error NeighborhoodIdIsEmpty = new("109", "The neighborhood id is invalid.");

    public static readonly Error CityNameIsEmpty = new("110", "The city name is invalid.");
    public static readonly Error CityIdIsEmpty = new("111", "The city id is invalid.");
    public static readonly Error CityTimezoneIsEmpty = new("112", "The city timezone is invalid.");

    public static readonly Error StateNameIsEmpty = new("113", "The state name is invalid.");
    public static readonly Error StateIdIsEmpty = new("114", "The state id is invalid.");
    public static readonly Error StateCodeIsEmpty = new("115", "The state code is invalid.");

    public static readonly Error CountryNameIsEmpty = new("116", "The country name is invalid.");
    public static readonly Error CountryIdIsEmpty = new("117", "The country id is invalid.");
    public static readonly Error CountryCodeIsInvalid = new("118", "The country code is invalid.");
    public static readonly Error CountryTimezoneIsEmpty = new("119", "The country timezone is invalid.");

    public static readonly Error CurrencyIdIsEmpty = new("120", "The currency id is invalid.");
    public static readonly Error CurrencyNameIsEmpty = new("121", "The currency name is invalid.");
    public static readonly Error CurrencyTimezoneIsEmpty = new("122", "The currency code is invalid.");
    public static readonly Error CurrencySymbolIsEmpty = new("123", "The currency symbol is invalid.");

    public static readonly Error IdTenantIsInvalid = new("124", "The tenant id is invalid."); 
    public static readonly Error NameTenantIsInvalid = new("125", "The tenant name is invalid."); 
    public static readonly Error DomainTenantIsInvalid = new("126", "The tenant domain is invalid."); 
    public static readonly Error TenantIsInvalid = new("127", "The tenant is invalid."); 
    public static readonly Error CreatedByIsInvalid = new("128", "The created by is invalid.");
    public static readonly Error LicenseNameIsInvalid = new("129", "The license name is invalid.");

    public static readonly Error CurrencyCodeIsEmpty = new("130", "The currency code is invalid.");

    public static readonly Error CountryIsNull = new("131", "The country is null.");
    public static readonly Error StateIsNull = new("132", "The state is null."); 
    public static readonly Error CityIsNull = new("133", "The city is null."); 
    public static readonly Error LocalityIsNull = new("134", "The locality is null."); 
    public static readonly Error NeighborhoodIsNull = new("135", "The neighborhood is null.");

    public static readonly Error CodeTypeDocumentIsInvalid = new("136", "The code type document is invalid.");
    public static readonly Error NameTypeDocumentIsInvalid = new("137", "The name type document is invalid.");
    public static readonly Error CodeTypeDocumentCannotBeNullOrEmpty = new("138", "The code type document cannot be null or empty.");

    public static readonly Error TypeDocumentIsInvalid = new("139", "The type document is invalid.");

    public static readonly Error PhoneTenantIsInvalid = new("140", "The phone tenant is invalid.");
    public static readonly Error NumberDocumentTenantIsInvalid = new("141", "The number document tenant is invalid.");

    public static readonly Error AddressIsNullOrEmpty = new("142", "The address is null or empty.");
    public static readonly Error PostalCodeIsNullOrEmpty = new("143", "The postal code is null or empty.");

    public static readonly Error EmailTenantIsInvalid = new("144", "The email tenant is invalid.");

    public static readonly Error DeletedByIsInvalid = new("145", "The deleted by is invalid.");
    public static readonly Error UpdatedByIsInvalid = new("146", "The updated by is invalid.");
}
