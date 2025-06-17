namespace CodeDesignPlus.Net.Microservice.Tenants.Domain;

public class Errors : IErrorCodes
{
    public const string UnknownError = "100 : UnknownError";

    public const string LicenseIdIsEmpty = "101 : The license id is invalid.";
    public const string LicenseNameIsEmpty = "102 : The license name is invalid.";
    public const string LicenseStartDateGreaterThanEndDate = "103 : The license start date is greater than the end date.";
    public const string LicenseMetadataIsNull = "105 : The license metadata is null.";

    public const string LocalityNameIsEmpty = "106 : The locality name is invalid.";
    public const string LocalityIdIsEmpty = "107 : The locality id is invalid.";

    public const string NeighborhoodNameIsEmpty = "108 : The neighborhood name is invalid.";
    public const string NeighborhoodIdIsEmpty = "109 : The neighborhood id is invalid.";

    public const string CityNameIsEmpty = "110 : The city name is invalid.";
    public const string CityIdIsEmpty = "111 : The city id is invalid.";
    public const string CityTimezoneIsEmpty = "112 : The city timezone is invalid.";

    public const string StateNameIsEmpty = "113 : The state name is invalid.";
    public const string StateIdIsEmpty = "114 : The state id is invalid.";
    public const string StateCodeIsEmpty = "115 : The state code is invalid.";

    public const string CountryNameIsEmpty = "116 : The country name is invalid.";
    public const string CountryIdIsEmpty = "117 : The country id is invalid.";
    public const string CountryCodeIsInvalid = "118 : The country code is invalid.";
    public const string CountryTimezoneIsEmpty = "119 : The country timezone is invalid.";

    public const string CurrencyIdIsEmpty = "120 : The currency id is invalid.";
    public const string CurrencyNameIsEmpty = "121 : The currency name is invalid.";
    public const string CurrencyTimezoneIsEmpty = "122 : The currency code is invalid.";
    public const string CurrencySymbolIsEmpty = "123 : The currency symbol is invalid.";

    public const string IdTenantIsInvalid = "124 : The tenant id is invalid."; 
    public const string NameTenantIsInvalid = "125 : The tenant name is invalid."; 
    public const string DomainTenantIsInvalid = "126 : The tenant domain is invalid."; 
    public const string TenantIsInvalid = "127 : The tenant is invalid."; 
    public const string CreatedByIsInvalid = "128 : The created by is invalid.";
    public const string LicenseNameIsInvalid = "129 : The license name is invalid.";

    public const string CurrencyCodeIsEmpty = "130 : The currency code is invalid.";

    public const string CountryIsNull = "131 : The country is null.";
    public const string StateIsNull = "132 : The state is null."; 
    public const string CityIsNull = "133 : The city is null."; 
    public const string LocalityIsNull = "134 : The locality is null."; 
    public const string NeighborhoodIsNull = "135 : The neighborhood is null.";

    public const string CodeTypeDocumentIsInvalid = "136 : The code type document is invalid.";
    public const string NameTypeDocumentIsInvalid = "137 : The name type document is invalid.";
    public const string CodeTypeDocumentCannotBeNullOrEmpty = "138 : The code type document cannot be null or empty.";

    public const string TypeDocumentIsInvalid = "139 : The type document is invalid.";

    public const string PhoneTenantIsInvalid = "140 : The phone tenant is invalid.";
    public const string NumberDocumentTenantIsInvalid = "141 : The number document tenant is invalid.";

    public const string AddressIsNullOrEmpty = "142 : The address is null or empty.";
    public const string PostalCodeIsNullOrEmpty = "143 : The postal code is null or empty.";
}
