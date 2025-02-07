using System;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using FluentValidation.TestHelper;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Commands.CreateTenant;

public class CreateTenantCommandTest
{
    private readonly Validator validator;

    public CreateTenantCommandTest()
    {
        validator = new Validator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new CreateTenantCommand(Guid.Empty, "Test Tenant", new Uri("http://example.com"), Utils.License, Utils.Location);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateTenantCommand(Guid.NewGuid(), string.Empty, new Uri("http://example.com"), Utils.License, Utils.Location);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var command = new CreateTenantCommand(Guid.NewGuid(), new string('a', 129), new Uri("http://example.com"), Utils.License, Utils.Location);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Domain_Is_Null()
    {
        var command = new CreateTenantCommand(Guid.NewGuid(), "Test Tenant", null, Utils.License, Utils.Location);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Domain);
    }

    [Fact]
    public void Should_Have_Error_When_License_Is_Null()
    {
        var command = new CreateTenantCommand(Guid.NewGuid(), "Test Tenant", new Uri("http://example.com"), null!, Utils.Location);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.License);
    }

    [Fact]
    public void Should_Have_Error_When_Location_Is_Null()
    {
        var command = new CreateTenantCommand(Guid.NewGuid(), "Test Tenant", new Uri("http://example.com"), Utils.License, null!);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateTenantCommand(Guid.NewGuid(), "Test Tenant", new Uri("http://example.com"), Utils.License, Utils.Location);
        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
