using System;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using FluentValidation.TestHelper;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Commands.UpdateTenant;

public class UpdateTenantCommandTest
{
    private readonly Validator validator;

    public UpdateTenantCommandTest()
    {
        validator = new Validator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new UpdateTenantCommand(Guid.Empty, "TenantName", Utils.TypeDocument, "12345678", new Uri("http://example.com"), "3105682451", "fake@fake.com", Utils.Location, Utils.License, true);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), null!, Utils.TypeDocument, "12345678", new Uri("http://example.com"), "3105682451", "fake@fake.com", Utils.Location, Utils.License, true);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), new string('a', 129), Utils.TypeDocument, "12345678", new Uri("http://example.com"), "3105682451", "fake@fake.com", Utils.Location, Utils.License, true);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Domain_Is_Null()
    {
        // El dominio es opcional, igual que al crear: exigirlo impedia actualizar un tenant que se
        // creo sin el.
        var command = new UpdateTenantCommand(Guid.NewGuid(), "TenantName", Utils.TypeDocument, "12345678", null, "3105682451", "fake@fake.com", Utils.Location, Utils.License, true);
        var result = validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Domain);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "TenantName", Utils.TypeDocument, "12345678", new Uri("http://example.com"), "3105682451", null!, Utils.Location, Utils.License, true);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Not_A_Valid_Address()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "TenantName", Utils.TypeDocument, "12345678", new Uri("http://example.com"), "3105682451", "not-an-email", Utils.Location, Utils.License, true);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "TenantName", Utils.TypeDocument, "12345678", new Uri("http://example.com"), "3105682451", "fake@fake.com", Utils.Location, Utils.License, true);
        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
