using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test.Helpers;

/// <summary>
/// Levanta un Mongo efimero para las pruebas de repositorio y expone su puerto.
/// </summary>
/// <remarks>
/// El contenedor se declara aqui y no se toma del SDK: <c>CodeDesignPlus.Net.xUnit</c> es de uso interno del
/// SDK y no debe referenciarse desde un microservicio. Se usa <c>Ductus.FluentDocker</c>, que ya es la
/// dependencia con la que este repositorio maneja contenedores en pruebas.
/// <para>
/// El puerto del anfitrion lo asigna Docker —se expone el 0— para que dos suites en paralelo, o un Mongo local
/// ya escuchando en el 27017, no choquen entre si.
/// </para>
/// <para>
/// <b>Requiere Docker.</b> Sin el, estas pruebas no corren; conviene tenerlo en cuenta al montarlas en CI.
/// </para>
/// </remarks>
public sealed class MongoContainerFixture : IDisposable
{
    public const string Collection = "Mongo Tenants Collection";

    private readonly IContainerService container;

    /// <summary>Puerto del anfitrion donde responde el Mongo de las pruebas.</summary>
    public int Port { get; }

    public MongoContainerFixture()
    {
        this.container = new Builder()
            .UseContainer()
            .UseImage("mongo:8.0")
            .ExposePort(0, 27017)
            // Se espera por el log y no por el puerto: en Docker Desktop sobre Windows, WaitForPort sondea la
            // IP interna del contenedor —inalcanzable desde el anfitrion— y agota el tiempo aunque Mongo este
            // listo. El mensaje del log es la senal fiable.
            .WaitForMessageInLog("Waiting for connections", TimeSpan.FromSeconds(90))
            .Build()
            .Start();

        this.Port = this.container.ToHostExposedEndpoint("27017/tcp").Port;
    }

    public string ConnectionString => $"mongodb://localhost:{this.Port}";

    public void Dispose()
    {
        this.container.Dispose();
    }
}
