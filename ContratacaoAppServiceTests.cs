using ContratacaoService.Application.Repositories;
using Moq;
using PropostaService.Application.Dtos;
using PropostaService.Application.Repositories;
using PropostaService.Application.Services;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IndtSeguros.Tests.Application
{
    public class ContratacaoAppServiceTests
    {
        private readonly Mock<IContratacaoRepository> _contratacaoRepoMock;
        private readonly Mock<IPropostaRepository> _propostaRepoMock;
        private readonly ContratacaoAppService _service;

        public ContratacaoAppServiceTests()
        {
            _contratacaoRepoMock = new Mock<IContratacaoRepository>();
            _propostaRepoMock = new Mock<IPropostaRepository>();

            _service = new ContratacaoAppService(
                _contratacaoRepoMock.Object,
                _propostaRepoMock.Object);
        }

        [Fact]
        public async Task ContratarAsync_PropostaNaoEncontrada_DeveLancarInvalidOperationException()
        {
            // Arrange
            var request = new ContratarPropostaRequest { PropostaId = 10 };

            _propostaRepoMock
                .Setup(r => r.ObterPorIdAsync(request.PropostaId))
                .ReturnsAsync((Proposta?)null);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ContratarAsync(request));

            // Assert
            Assert.Equal("Proposta não encontrada.", ex.Message);

            _contratacaoRepoMock.Verify(r => r.InserirAsync(It.IsAny<Contratacao>()), Times.Never);
        }

        [Fact]
        public async Task ContratarAsync_PropostaNaoAprovada_DeveLancarInvalidOperationException()
        {
            // Arrange
            var request = new ContratarPropostaRequest { PropostaId = 20 };

            var proposta = new Proposta("Cliente", "Produto", 100m)
            {
                Id = request.PropostaId,
                Status = StatusProposta.EmAnalise
            };

            _propostaRepoMock
                .Setup(r => r.ObterPorIdAsync(request.PropostaId))
                .ReturnsAsync(proposta);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ContratarAsync(request));

            // Assert
            Assert.Equal("Somente propostas aprovadas podem ser contratadas.", ex.Message);

            _contratacaoRepoMock.Verify(r => r.InserirAsync(It.IsAny<Contratacao>()), Times.Never);
        }

        [Fact]
        public async Task ContratarAsync_PropostaAprovada_DeveCriarContratacaoERetornarId()
        {
            // Arrange
            var request = new ContratarPropostaRequest { PropostaId = 30 };

            var proposta = new Proposta("Cliente", "Produto", 100m)
            {
                Id = request.PropostaId,
                Status = StatusProposta.Aprovada
            };

            _propostaRepoMock
                .Setup(r => r.ObterPorIdAsync(request.PropostaId))
                .ReturnsAsync(proposta);

            var esperadoId = 99;

            _contratacaoRepoMock
                .Setup(r => r.InserirAsync(It.IsAny<Contratacao>()))
                .ReturnsAsync(esperadoId);

            // Act
            var id = await _service.ContratarAsync(request);

            // Assert
            Assert.Equal(esperadoId, id);

            _contratacaoRepoMock.Verify(r =>
                r.InserirAsync(It.Is<Contratacao>(c =>
                    c.PropostaId == request.PropostaId
                )),
                Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoNaoExiste_DeveRetornarNull()
        {
            // Arrange
            _contratacaoRepoMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Contratacao?)null);

            // Act
            var resultado = await _service.ObterPorIdAsync(1);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoExiste_DeveMapearParaDto()
        {
            // Arrange
            var entidade = new Contratacao(123)
            {
                Id = 5,
                DataContratacao = DateTime.UtcNow.AddMinutes(-15)
            };

            _contratacaoRepoMock
                .Setup(r => r.ObterPorIdAsync(entidade.Id))
                .ReturnsAsync(entidade);

            // Act
            var dto = await _service.ObterPorIdAsync(entidade.Id);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entidade.Id, dto!.Id);
            Assert.Equal(entidade.PropostaId, dto.PropostaId);
            Assert.Equal(entidade.DataContratacao, dto.DataContratacao);
        }

        [Fact]
        public async Task ListarAsync_DeveMapearListaDeEntidadesParaDtos()
        {
            // Arrange
            var entidades = new List<Contratacao>
            {
                new Contratacao(1) { Id = 1, DataContratacao = DateTime.UtcNow.AddMinutes(-10) },
                new Contratacao(2) { Id = 2, DataContratacao = DateTime.UtcNow.AddMinutes(-5) }
            };

            _contratacaoRepoMock
                .Setup(r => r.ListarAsync())
                .ReturnsAsync(entidades);

            // Act
            var lista = (await _service.ListarAsync()).ToList();

            // Assert
            Assert.Equal(2, lista.Count);

            Assert.Equal(entidades[0].Id, lista[0].Id);
            Assert.Equal(entidades[0].PropostaId, lista[0].PropostaId);
            Assert.Equal(entidades[0].DataContratacao, lista[0].DataContratacao);

            Assert.Equal(entidades[1].Id, lista[1].Id);
            Assert.Equal(entidades[1].PropostaId, lista[1].PropostaId);
            Assert.Equal(entidades[1].DataContratacao, lista[1].DataContratacao);
        }
    }
}
