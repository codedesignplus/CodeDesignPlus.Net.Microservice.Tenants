using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

/// <summary>
/// El tipo de documento de la copropiedad.
/// </summary>
/// <remarks>
/// El constructor solo hidrata: lo usan Mongo, la caché y la deserialización de las peticiones, y leer nunca valida
/// (pendings/737). Lo que lo escribe lo valida con <see cref="Create"/> o <see cref="EnsureValid"/>.
/// </remarks>
public sealed partial class TypeDocument
{
    public string Code { get; private set; }

    public string Name { get; private set; }

    [JsonConstructor]
    public TypeDocument(string code, string name)
    {
        this.Code = code;
        this.Name = name;
    }

    /// <summary>
    /// Crea un tipo de documento válido.
    /// </summary>
    /// <param name="code">El código del tipo.</param>
    /// <param name="name">Su nombre.</param>
    /// <returns>El tipo de documento.</returns>
    public static TypeDocument Create(string code, string name)
    {
        var typeDocument = new TypeDocument(code, name);

        typeDocument.EnsureValid();

        return typeDocument;
    }

    /// <summary>
    /// Comprueba lo que un tipo de documento tiene que cumplir para guardarse. La llaman la fábrica y el agregado en cada
    /// escritura.
    /// </summary>
    public void EnsureValid()
    {
        DomainGuard.IsNullOrEmpty(Code, Errors.CodeTypeDocumentCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(Name, Errors.NameTypeDocumentIsInvalid);
        DomainGuard.IsGreaterThan(Code.Length, 3, Errors.CodeTypeDocumentIsInvalid);
    }
}
