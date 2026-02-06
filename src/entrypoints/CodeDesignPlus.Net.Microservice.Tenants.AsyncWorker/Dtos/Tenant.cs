using System;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public required TypeDocument TypeDocument { get; set; }
    public string NumberDocument { get; set; } = string.Empty;
    public string? Web { get; set; } = string.Empty;
    public required Location Location { get; set; } 
}
