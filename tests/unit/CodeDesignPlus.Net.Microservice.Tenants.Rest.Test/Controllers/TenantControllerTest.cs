using CodeDesignPlus.Microservice.Api.Dtos;
using System;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Tenants.Rest.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MediatR;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Rest.Test.Controllers
{
    public class TenantControllerTest
    {
        private readonly static Currency currency = Currency.Create(Guid.NewGuid(), "COP", "Colombian Peso", "COP");
        private readonly static Country country = Country.Create(Guid.NewGuid(), "Colombia", 102, "America/Bogota", currency);
        private readonly static State state = State.Create(Guid.NewGuid(), "Bogota", "DC");
        private readonly static City city = City.Create(Guid.NewGuid(), "Bogota", "America/Bogota");
        private readonly static Locality locality = Locality.Create(Guid.NewGuid(), "Punta Aranda");
        private readonly static Neighborhood neighborhood = Neighborhood.Create(Guid.NewGuid(), "Galán");
        private readonly static Location location = Location.Create(country, state, city, locality, neighborhood);
        private readonly static License license = License.Create(Guid.NewGuid(), "License Test", SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(30)), new Dictionary<string, string>{
            { "User", "10" },
            { "Admin", "1" },
            { "Invoice", "1" }
        });

        private readonly Mock<IMediator> mediatorMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly TenantController controller;

        public TenantControllerTest()
        {
            mediatorMock = new Mock<IMediator>();
            mapperMock = new Mock<IMapper>();
            controller = new TenantController(mediatorMock.Object, mapperMock.Object);
        }

        [Fact]
        public async Task GetTenants_ReturnsOkResult()
        {
            // Arrange
            var criteria = new C.Criteria();
            var cancellationToken = new CancellationToken();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllTenantQuery>(), cancellationToken))
                .ReturnsAsync(new List<TenantDto>());

            // Act
            var result = await controller.GetTenants(criteria, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<TenantDto>>(okResult.Value);

            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllTenantQuery>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetTenantById_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cancellationToken = new CancellationToken();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetTenantByIdQuery>(), cancellationToken))
                .ReturnsAsync(new TenantDto()
                {
                    Id = id,
                    Name = "Tenant",
                    Domain = new Uri("http://localhost"),
                    License = license
                });

            // Act
            var result = await controller.GetTenantById(id, cancellationToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TenantDto>(okResult.Value);

            mediatorMock.Verify(m => m.Send(It.IsAny<GetTenantByIdQuery>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task CreateTenant_ReturnsNoContentResult()
        {
            // Arrange
            var data = new CreateTenantDto();
            var cancellationToken = new CancellationToken();
            mapperMock.Setup(m => m.Map<CreateTenantCommand>(data)).Returns(new CreateTenantCommand(Guid.NewGuid(), "Tenant", new Uri("http://localhost"), license, location));

            // Act
            var result = await controller.CreateTenant(data, cancellationToken);

            // Assert
            Assert.IsType<NoContentResult>(result);

            mediatorMock.Verify(m => m.Send(It.IsAny<CreateTenantCommand>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task UpdateTenant_ReturnsNoContentResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var data = new UpdateTenantDto();
            var cancellationToken = new CancellationToken();
            mapperMock.Setup(m => m.Map<UpdateTenantCommand>(data)).Returns(new UpdateTenantCommand(id, "Tenant", new Uri("http://localhost"), license, location, true));

            // Act
            var result = await controller.UpdateTenant(id, data, cancellationToken);

            // Assert
            Assert.IsType<NoContentResult>(result);

            mediatorMock.Verify(m => m.Send(It.IsAny<UpdateTenantCommand>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task DeleteTenant_ReturnsNoContentResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await controller.DeleteTenant(id, cancellationToken);

            // Assert
            Assert.IsType<NoContentResult>(result);

            mediatorMock.Verify(m => m.Send(It.IsAny<DeleteTenantCommand>(), cancellationToken), Times.Once);
        }
    }
}
