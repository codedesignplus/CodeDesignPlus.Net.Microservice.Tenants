using System.Collections.Generic;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test;

/// <summary>
/// El documento de una copropiedad de un tipo que lleva dígito de verificación se escribe con guion y con el dígito que le
/// corresponde, y el rechazo dice cuál es (pendings/744). Lo que no escribe el documento no lo valida: una copropiedad
/// guardada antes de la regla se sigue pudiendo eliminar, restaurar, purgar y renovar.
/// </summary>
public class TenantDocumentCheckDigitTest
{
    private const string WithCheckDigit = "31";

    private static readonly DocumentCheckDigitRules Rules = new([WithCheckDigit], [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71]);

    private static readonly TypeDocument TaxId = TypeDocument.Create(WithCheckDigit, "Tax identification number");

    private static readonly Instant Now = SystemClock.Instance.GetCurrentInstant();

    private static TenantAggregate Create(TypeDocument typeDocument, string numberDocument, DocumentCheckDigitRules rules)
    {
        return TenantAggregate.Create(Guid.NewGuid(), "Conjunto", typeDocument, numberDocument, null, "3107845123", "admin@example.com", Utils.Location, Utils.License, true, Guid.NewGuid(), rules);
    }

    /// <summary>
    /// Una copropiedad como las guardadas antes de la regla: con el documento sin guion ni dígito.
    /// </summary>
    private static TenantAggregate StoredBeforeTheRule()
    {
        var tenant = Create(TaxId, "900765432", DocumentCheckDigitRules.None);

        tenant.GetAndClearEvents();

        return tenant;
    }

    private static TenantAggregate DeletedBeforeTheRule()
    {
        var tenant = StoredBeforeTheRule();

        tenant.Delete(Guid.NewGuid(), Duration.FromDays(30));
        tenant.GetAndClearEvents();

        return tenant;
    }

    [Theory]
    [InlineData("901888777", 3)]
    [InlineData("901777333", 0)]
    [InlineData("900765432", 6)]
    [InlineData("800197268", 4)]
    [InlineData("79456123", 2)]
    [InlineData("123456", 3)]
    [InlineData("1023456781", 6)]
    public void Compute_Number_ReturnsWeightedModulusElevenDigit(string number, int expected)
    {
        // Act
        var digit = Rules.Compute(number);

        // Assert
        Assert.Equal(expected, digit);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("90188877A")]
    [InlineData("1234567890123456")]
    public void Compute_NumberThatCannotBeWeighted_ReturnsNull(string? number)
    {
        // Act
        var digit = Rules.Compute(number);

        // Assert
        Assert.Null(digit);
    }

    /// <summary>
    /// Sin longitud fija: el de una persona jurídica tiene nueve dígitos antes del guion y el de una persona natural es su
    /// cédula, de seis a diez.
    /// </summary>
    [Theory]
    [InlineData("901777333-0")]
    [InlineData("901888777-3")]
    [InlineData("79456123-2")]
    [InlineData("123456-3")]
    [InlineData("1023456781-6")]
    public void Create_TaxIdWithItsCheckDigit_KeepsTheNumberAsWritten(string numberDocument)
    {
        // Act
        var tenant = Create(TaxId, numberDocument, Rules);

        // Assert
        Assert.Equal(numberDocument, tenant.NumberDocument);
    }

    [Fact]
    public void Create_TaxIdWithoutDash_ThrowsMissingWithTheDigit()
    {
        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => Create(TaxId, "900765432", Rules));

        // Assert
        Assert.Equal(Errors.DocumentCheckDigitIsMissing.GetCode(), exception.Code);
        Assert.Equal("Este tipo de documento se escribe con guion y dígito de verificación. Si 900765432 es el número sin el dígito, escríbelo así: 900765432-6.", exception.Error!.Value.GetMessage("es"));
    }

    [Fact]
    public void Create_TaxIdWithTheDigitAttached_GivesBothReadings()
    {
        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => Create(TaxId, "9018887773", Rules));

        // Assert
        Assert.Equal(Errors.DocumentCheckDigitMayBeAttached.GetCode(), exception.Code);
        Assert.Equal("Este tipo de documento se escribe con guion y dígito de verificación. Si 9018887773 ya incluye el dígito, escríbelo así: 901888777-3. Si no lo incluye, así: 9018887773-5.", exception.Error!.Value.GetMessage("es"));
    }

    [Fact]
    public void Create_TaxIdWithWrongCheckDigit_ThrowsDoesNotMatchWithTheDigit()
    {
        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => Create(TaxId, "901888777-5", Rules));

        // Assert
        Assert.Equal(Errors.DocumentCheckDigitDoesNotMatch.GetCode(), exception.Code);
        Assert.Equal("El dígito de verificación no corresponde: el de 901888777 es 3, no 5. Escríbelo así: 901888777-3.", exception.Error!.Value.GetMessage("es"));
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("-3")]
    [InlineData("901888777-")]
    [InlineData("901888777-33")]
    [InlineData("901888777-A")]
    [InlineData("901.888.777-3")]
    [InlineData("901 888 777-3")]
    [InlineData("1234567890123456-1")]
    public void Create_TaxIdThatIsNotDigitsDashDigit_ThrowsFormatIsInvalid(string numberDocument)
    {
        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => Create(TaxId, numberDocument, Rules));

        // Assert
        Assert.Equal(Errors.DocumentNumberFormatIsInvalid.GetCode(), exception.Code);
    }

    [Fact]
    public void Create_TypeWithoutCheckDigit_AcceptsAnyNumber()
    {
        // Act
        var tenant = Create(Utils.TypeDocument, "900765432", Rules);

        // Assert
        Assert.Equal("900765432", tenant.NumberDocument);
    }

    [Fact]
    public void Create_NoTypesConfigured_AcceptsAnyNumber()
    {
        // Act
        var tenant = Create(TaxId, "900765432", DocumentCheckDigitRules.None);

        // Assert
        Assert.Equal("900765432", tenant.NumberDocument);
    }

    [Fact]
    public void Create_WithoutRules_Throws()
    {
        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => Create(TaxId, "901888777-3", null!));

        // Assert
        Assert.Equal(Errors.DocumentRulesRequired.GetCode(), exception.Code);
    }

    [Fact]
    public void Update_TaxIdWithItsCheckDigit_ReplacesTheDocument()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();

        // Act
        tenant.Update("Conjunto", TaxId, "900765432-6", null, "3107845123", "admin@example.com", true, Guid.NewGuid(), Rules);

        // Assert
        Assert.Equal("900765432-6", tenant.NumberDocument);
    }

    [Fact]
    public void Update_TaxIdWithoutDash_ThrowsAndKeepsTheDocument()
    {
        // Arrange
        var tenant = Create(TaxId, "901888777-3", Rules);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Update("Otro nombre", TaxId, "900765432", null, "3107845123", "admin@example.com", true, Guid.NewGuid(), Rules));

        // Assert
        Assert.Equal(Errors.DocumentCheckDigitIsMissing.GetCode(), exception.Code);
        Assert.Equal("901888777-3", tenant.NumberDocument);
        Assert.Equal("Conjunto", tenant.Name);
    }

    [Fact]
    public void Update_TaxIdWithWrongCheckDigit_ThrowsAndKeepsTheDocument()
    {
        // Arrange
        var tenant = Create(TaxId, "901888777-3", Rules);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Update("Conjunto", TaxId, "901888777-5", null, "3107845123", "admin@example.com", true, Guid.NewGuid(), Rules));

        // Assert
        Assert.Equal(Errors.DocumentCheckDigitDoesNotMatch.GetCode(), exception.Code);
        Assert.Equal("901888777-3", tenant.NumberDocument);
    }

    /// <summary>
    /// Editar una copropiedad guardada antes de la regla pide corregir el documento: es cuando se escribe.
    /// </summary>
    [Fact]
    public void Update_StoredBeforeTheRuleKeepingItsDocument_Throws()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Update("Otro nombre", tenant.TypeDocument, tenant.NumberDocument, null, "3107845123", "admin@example.com", true, Guid.NewGuid(), Rules));

        // Assert
        Assert.Equal(Errors.DocumentCheckDigitIsMissing.GetCode(), exception.Code);
    }

    [Fact]
    public void Update_WithoutRules_Throws()
    {
        // Arrange
        var tenant = Create(TaxId, "901888777-3", Rules);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Update("Conjunto", TaxId, "901888777-3", null, "3107845123", "admin@example.com", true, Guid.NewGuid(), null!));

        // Assert
        Assert.Equal(Errors.DocumentRulesRequired.GetCode(), exception.Code);
    }

    [Fact]
    public void Delete_StoredBeforeTheRule_DeletesIt()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();

        // Act
        tenant.Delete(Guid.NewGuid(), Duration.FromDays(30));

        // Assert
        Assert.True(tenant.IsDeleted);
    }

    [Fact]
    public void Restore_DeletedBeforeTheRule_RestoresItWithItsDocument()
    {
        // Arrange
        var tenant = DeletedBeforeTheRule();

        // Act
        tenant.Restore(Guid.NewGuid(), Now);

        // Assert
        Assert.False(tenant.IsDeleted);
        Assert.Equal("900765432", tenant.NumberDocument);
    }

    [Fact]
    public void Purge_DeletedBeforeTheRule_AnnouncesThePurge()
    {
        // Arrange
        var tenant = DeletedBeforeTheRule();

        // Act
        tenant.Purge(Now.Plus(Duration.FromDays(31)));

        // Assert
        Assert.Single(tenant.GetAndClearEvents(), x => x is TenantPurgedDomainEvent);
    }

    [Fact]
    public void UpdateLocation_StoredBeforeTheRule_DoesNotThrow()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();

        // Act
        var exception = Record.Exception(() => tenant.UpdateLocation(Utils.Location, Guid.NewGuid()));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void UpdateLicense_StoredBeforeTheRule_DoesNotThrow()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();

        // Act
        var exception = Record.Exception(() => tenant.UpdateLicense(Utils.License, Guid.NewGuid()));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void RenewLicense_StoredBeforeTheRule_AppliesTheRenewal()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();
        var renewed = License.Create(Utils.License.Id, Utils.License.Name, Utils.License.EndDate, Utils.License.EndDate.Plus(Duration.FromDays(365)), [], new Dictionary<string, string>(Utils.License.Metadata));

        // Act
        var applied = tenant.RenewLicense(renewed, Guid.NewGuid());

        // Assert
        Assert.True(applied);
    }

    [Fact]
    public void ChangeLicense_StoredBeforeTheRule_AppliesTheChange()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();
        var changed = License.Create(Guid.NewGuid(), "Élite", Now, Now.Plus(Duration.FromDays(365)), [], new Dictionary<string, string>());

        // Act
        var applied = tenant.ChangeLicense(changed, Guid.NewGuid());

        // Assert
        Assert.True(applied);
    }

    [Fact]
    public void ApplyPackages_StoredBeforeTheRule_AddsThem()
    {
        // Arrange
        var tenant = StoredBeforeTheRule();
        var package = new TenantPackage(Guid.NewGuid(), Guid.NewGuid(), "Virtual Básico", Now, Now.Plus(Duration.FromDays(20)), null, [new TenantPackageAttribute("assemblies_number_meetings", "3", "Sum")]);

        // Act
        var applied = tenant.ApplyPackages([package], Now, Guid.NewGuid());

        // Assert
        Assert.True(applied);
    }

    [Theory]
    [InlineData("es")]
    [InlineData("en")]
    [InlineData("fr")]
    [InlineData("pt")]
    public void Errors_EveryLanguage_SaysTheDigit(string language)
    {
        // Act
        var missing = Errors.DocumentCheckDigitIsMissing.With("901888777", 3).GetMessage(language);
        var wrong = Errors.DocumentCheckDigitDoesNotMatch.With("901888777", 3, '5').GetMessage(language);
        var attached = Errors.DocumentCheckDigitMayBeAttached.With("9018887773", "901888777", '3', 5).GetMessage(language);
        var format = Errors.DocumentNumberFormatIsInvalid.GetMessage(language);
        var rules = Errors.DocumentRulesRequired.GetMessage(language);

        // Assert
        Assert.Contains("901888777-3", missing);
        Assert.Contains("901888777-3", wrong);
        Assert.Contains("901888777-3", attached);
        Assert.Contains("9018887773-5", attached);
        Assert.True(attached.IndexOf("901888777-3", StringComparison.Ordinal) < attached.IndexOf("9018887773-5", StringComparison.Ordinal));
        Assert.DoesNotContain("{", missing + wrong + attached);
        Assert.NotEqual(Errors.DocumentNumberFormatIsInvalid.GetCode(), format);
        Assert.NotEqual(Errors.DocumentRulesRequired.GetCode(), rules);
        Assert.Equal(language == "en", missing == Errors.DocumentCheckDigitIsMissing.With("901888777", 3).ToEnglish());
    }
}
