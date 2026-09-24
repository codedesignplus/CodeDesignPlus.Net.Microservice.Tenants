using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("100");

    public static readonly Error LicenseIdIsEmpty = new("101");
    public static readonly Error LicenseNameIsEmpty = new("102");
    public static readonly Error LicenseStartDateGreaterThanEndDate = new("103");
    public static readonly Error LicenseMetadataIsNull = new("105");

    public static readonly Error LocalityNameIsEmpty = new("106");
    public static readonly Error LocalityIdIsEmpty = new("107");

    public static readonly Error NeighborhoodNameIsEmpty = new("108");
    public static readonly Error NeighborhoodIdIsEmpty = new("109");

    public static readonly Error CityNameIsEmpty = new("110");
    public static readonly Error CityIdIsEmpty = new("111");
    public static readonly Error CityTimezoneIsEmpty = new("112");

    public static readonly Error StateNameIsEmpty = new("113");
    public static readonly Error StateIdIsEmpty = new("114");
    public static readonly Error StateCodeIsEmpty = new("115");

    public static readonly Error CountryNameIsEmpty = new("116");
    public static readonly Error CountryIdIsEmpty = new("117");
    public static readonly Error CountryCodeIsInvalid = new("118");
    public static readonly Error CountryTimezoneIsEmpty = new("119");

    public static readonly Error CurrencyIdIsEmpty = new("120");
    public static readonly Error CurrencyNameIsEmpty = new("121");
    public static readonly Error CurrencyTimezoneIsEmpty = new("122");
    public static readonly Error CurrencySymbolIsEmpty = new("123");

    public static readonly Error IdTenantIsInvalid = new("124"); 
    public static readonly Error NameTenantIsInvalid = new("125"); 
    public static readonly Error DomainTenantIsInvalid = new("126"); 
    public static readonly Error TenantIsInvalid = new("127"); 
    public static readonly Error CreatedByIsInvalid = new("128");
    public static readonly Error LicenseNameIsInvalid = new("129");

    public static readonly Error CurrencyCodeIsEmpty = new("130");

    public static readonly Error CountryIsNull = new("131");
    public static readonly Error StateIsNull = new("132"); 
    public static readonly Error CityIsNull = new("133"); 
    public static readonly Error LocalityIsNull = new("134"); 
    public static readonly Error NeighborhoodIsNull = new("135");

    public static readonly Error CodeTypeDocumentIsInvalid = new("136");
    public static readonly Error NameTypeDocumentIsInvalid = new("137");
    public static readonly Error CodeTypeDocumentCannotBeNullOrEmpty = new("138");

    public static readonly Error TypeDocumentIsInvalid = new("139");

    public static readonly Error PhoneTenantIsInvalid = new("140");
    public static readonly Error NumberDocumentTenantIsInvalid = new("141");

    public static readonly Error AddressIsNullOrEmpty = new("142");
    public static readonly Error PostalCodeIsNullOrEmpty = new("143");

    public static readonly Error EmailTenantIsInvalid = new("144");

    public static readonly Error DeletedByIsInvalid = new("145");
    public static readonly Error UpdatedByIsInvalid = new("146");
}
