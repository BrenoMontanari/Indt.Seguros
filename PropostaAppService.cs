using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using PropostaService.Application.Dtos;
using PropostaService.Application.Repositories;
using PropostaService.Application.Services;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using Xunit;

namespace IndtSeguros.Tests.Application
{
    public class PropostaAppServiceTests
    {
        private readonly Mock<IPropostaRepository> _repoMock;
        private readonly PropostaAppService _service;

        public PropostaAppServiceTests()
        {
            _repoMock = new Mock<IPropostaRepository>();
            _service = new PropostaAppService(_repoMock.Object);
        }

        [Fact]
        public async Task CriarAsync_Deve_Criar_Proposta_Com_Dados_Certos_E_Retornar_Id()
        {
            // Arrange
            var request = new CriarPropostaRequest
            {
                NomeCliente = "João da Silva",
                Produto = "Seguro Vida",
                Premio = 150.75m
            };

            var esperadoId = 42;

            _repoMock
                .Setup(r => r.InserirAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(esperadoId);

            // Act
            var id = await _service.CriarAsync(request);

            // Assert
            Assert.Equal(esperadoId, id);

            _repoMock.Verify(r =>
                r.InserirAsync(It.Is<Proposta>(p =>
                    p.NomeCliente == request.NomeCliente &&
                    p.Produto == request.Produto &&
                    p.Premio == request.Premio &&
                    p.Status == StatusProposta.EmAnalise
                )),
                Times.Once);
        }

        [Fact]
        public async Task ListarAsync_Deve_Mapear_Entidades_Para_Dtos()
        {
            // Arrange
            var agora = DateTime.UtcNow;

            var entidades = new List<Proposta>
            {
                new Proposta("Cliente 1", "Produto 1", 100m)
                {
                    Id = 1,
                    Status = StatusProposta.Aprovada,
                    DataCriacao = agora.AddMinutes(-10)
                },
                new Proposta("Cliente 2", "Produto 2", 200m)
                {
                    Id = 2,
                    Status = StatusProposta.Rejeitada,
                    DataCriacao = agora.AddMinutes(-5)
                }
            };

            _repoMock
                .Setup(r => r.ListarAsync())
                .ReturnsAsync(entidades);

            // Act
            var resultado = (await _service.ListarAsync()).ToList();

            // Assert
            Assert.Equal(2, resultado.Count);

            var primeiro = resultado[0];
            Assert.Equal(1, primeiro.Id);
            Assert.Equal("Cliente 1", primeiro.NomeCliente);
            Assert.Equal("Produto 1", primeiro.Produto);
            Assert.Equal(100m, primeiro.Premio);
            Assert.Equal(StatusProposta.Aprovada.ToString(), primeiro.Status);

            var segundo = resultado[1];
            Assert.Equal(2, segundo.Id);
            Assert.Equal("Cliente 2", segundo.NomeCliente);
            Assert.Equal("Produto 2", segundo.Produto);
            Assert.Equal(200m, segundo.Premio);
            Assert.Equal(StatusProposta.Rejeitada.ToString(), segundo.Status);
        }

        [Fact]
        public async Task ObterPorIdAsync_Quando_Proposta_Nao_Existe_Deve_Retornar_Null()
        {
            // Arrange
            _repoMock
                .Setup(r => r.ObterPorIdAsync(999))
                .ReturnsAsync((Proposta?)null);

            // Act
            var resultado = await _service.ObterPorIdAsync(999);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_Quando_Proposta_Existe_Deve_Mapear_Para_Dto()
        {
            // Arrange
            var entidade = new Proposta("Maria", "Seguro Auto", 300m)
            {
                Id = 10,
                Status = StatusProposta.EmAnalise,
                DataCriacao = DateTime.UtcNow.AddHours(-1)
            };

            _repoMock
                .Setup(r => r.ObterPorIdAsync(entidade.Id))
                .ReturnsAsync(entidade);

            // Act
            var dto = await _service.ObterPorIdAsync(entidade.Id);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entidade.Id, dto!.Id);
            Assert.Equal(entidade.NomeCliente, dto.NomeCliente);
            Assert.Equal(entidade.Produto, dto.Produto);
            Assert.Equal(entidade.Premio, dto.Premio);
            Assert.Equal(entidade.Status.ToString(), dto.Status);
        }
    }
}
