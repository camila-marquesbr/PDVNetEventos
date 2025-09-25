using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Data;
using PDVNetEventos.Data.Entities;
using PDVNetEventos.Services;
using Xunit;

namespace Testes
{
    public class EventServiceTests
    {
        /// <summary>
        /// Cria uma fábrica que devolve sempre um AppDbContext InMemory
        /// com o mesmo databaseName (para compartilhar dados entre chamadas).
        /// </summary>
        private static (Func<AppDbContext> factory, AppDbContext db) NewDbFactory(string databaseName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            // Fábrica usada pelo EventService
            Func<AppDbContext> factory = () => new AppDbContext(options);

            // Um contexto “mestre” para semear dados no Arrange
            var db = new AppDbContext(options);

            return (factory, db);
        }

        [Fact]
        public async Task Nao_Deve_Permitir_Acima_Do_Orcamento()
        {
            // Arrange: banco InMemory + serviço usando a fábrica
            var (factory, db) = NewDbFactory(nameof(Nao_Deve_Permitir_Acima_Do_Orcamento));
            var svc = new EventService(factory);

            var evento = new Evento
            {
                Nome = "E1",
                DataInicio = DateTime.Today,
                DataFim = DateTime.Today,
                CapacidadeMaxima = 10,
                OrcamentoMaximo = 1000m,
                TipoEventoId = 1
            };
            var f1 = new Fornecedor { NomeServico = "Som", CNPJ = "00.000.000/0000-00" };
            var f2 = new Fornecedor { NomeServico = "Luz", CNPJ = "11.111.111/1111-11" };

            db.Eventos.Add(evento);
            db.Fornecedores.AddRange(f1, f2);
            await db.SaveChangesAsync();

            // Act: primeiro vínculo ok
            await svc.AdicionarFornecedorAsync(evento.Id, f1.Id, 800m);

            // Tenta exceder orçamento
            Func<Task> act = () => svc.AdicionarFornecedorAsync(evento.Id, f2.Id, 300m);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*Orçamento excedido*");
        }
    }
}