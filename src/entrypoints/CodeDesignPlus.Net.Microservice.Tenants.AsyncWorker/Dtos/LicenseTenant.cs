using System;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos;

public class LicenseTenant
{

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Instant StartDate { get; set; }
    public Instant EndDate { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];

}
