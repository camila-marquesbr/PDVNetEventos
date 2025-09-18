using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Data;
using PDVNetEventos.Data.Entities;

namespace PDVNetEventos.Services
{
    public class EventService
    {
        /// <summary>
        /// Valida conflito de agenda: impede salvar um evento com datas que
        /// se sobrepõem às de outro evento já existente.
        /// </summary>
        public async Task ValidateEventDatesAsync(Evento novo)
        {
            using var db = new AppDbContext();

            bool conflita = await db.Eventos
                .AnyAsync(e =>
                    e.Id != novo.Id &&
                    // sobreposição: (ini <= fimOutro) && (fim >= iniOutro)
                    novo.DataInicio <= e.DataFim &&
                    novo.DataFim >= e.DataInicio);

            if (conflita)
                throw new InvalidOperationException(
                    "Conflito de agenda: já existe um evento que se sobrepõe a estas datas.");
        }

        /// <summary>
        /// Adiciona um participante a um evento respeitando a capacidade máxima.
        /// </summary>
        public async Task AddParticipantToEventAsync(int eventoId, int participanteId)
        {
            using var db = new AppDbContext();

            var ev = await db.Eventos
                .Include(x => x.Participantes)
                .FirstOrDefaultAsync(x => x.Id == eventoId)
                ?? throw new InvalidOperationException("Evento não encontrado.");

            var participante = await db.Participantes.FindAsync(participanteId)
                ?? throw new InvalidOperationException("Participante não encontrado.");

            int inscritos = await db.EventosParticipantes.CountAsync(x => x.EventoId == eventoId);
            if (inscritos >= ev.CapacidadeMaxima)
                throw new InvalidOperationException("Capacidade do evento atingida.");

            bool jaInscrito = await db.EventosParticipantes
                .AnyAsync(x => x.EventoId == eventoId && x.ParticipanteId == participanteId);
            if (jaInscrito)
                throw new InvalidOperationException("Participante já inscrito neste evento.");

            db.EventosParticipantes.Add(new EventoParticipante
            {
                EventoId = eventoId,
                ParticipanteId = participanteId
            });

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Adiciona um fornecedor ao evento respeitando o orçamento máximo.
        /// </summary>
        public async Task AddSupplierToEventAsync(int eventoId, int fornecedorId, decimal valorAcordado)
        {
            using var db = new AppDbContext();

            var ev = await db.Eventos
                .Include(x => x.Fornecedores)
                .FirstOrDefaultAsync(x => x.Id == eventoId)
                ?? throw new InvalidOperationException("Evento não encontrado.");

            var fornecedor = await db.Fornecedores.FindAsync(fornecedorId)
                ?? throw new InvalidOperationException("Fornecedor não encontrado.");

            decimal usado = await db.EventosFornecedores
                .Where(x => x.EventoId == eventoId)
                .SumAsync(x => (decimal?)x.ValorAcordado ?? 0m);

            if (usado + valorAcordado > ev.OrcamentoMaximo)
                throw new InvalidOperationException(
                    $"Orçamento excedido. Restante: {ev.OrcamentoMaximo - usado:C}.");

            bool jaTem = await db.EventosFornecedores
                .AnyAsync(x => x.EventoId == eventoId && x.FornecedorId == fornecedorId);
            if (jaTem)
                throw new InvalidOperationException("Fornecedor já está vinculado a este evento.");

            db.EventosFornecedores.Add(new EventoFornecedor
            {
                EventoId = eventoId,
                FornecedorId = fornecedorId,
                ValorAcordado = valorAcordado
            });

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Cria um novo participante e devolve o Id.
        /// </summary>
        public async Task<int> CreateParticipantAsync(string nome, string cpf, string? telefone, TipoParticipante tipo)
        {
            using var db = new AppDbContext();

            bool cpfExiste = await db.Participantes.AnyAsync(x => x.CPF == cpf);
            if (cpfExiste)
                throw new InvalidOperationException("Já existe participante com este CPF.");

            var p = new Participante
            {
                NomeCompleto = nome,
                CPF = cpf,
                Telefone = telefone,
                Tipo = tipo
            };
            db.Participantes.Add(p);
            await db.SaveChangesAsync();
            return p.Id;
        }

        /// <summary>
        /// Cria um novo fornecedor e devolve o Id.
        /// </summary>
        public async Task<int> CreateSupplierAsync(string nomeServico, string cnpj, decimal? precoPadrao)
        {
            using var db = new AppDbContext();

            bool cnpjExiste = await db.Fornecedores.AnyAsync(x => x.CNPJ == cnpj);
            if (cnpjExiste)
                throw new InvalidOperationException("Já existe fornecedor com este CNPJ.");

            var f = new Fornecedor
            {
                NomeServico = nomeServico,
                CNPJ = cnpj,
                PrecoPadrao = precoPadrao
            };
            db.Fornecedores.Add(f);
            await db.SaveChangesAsync();
            return f.Id;
        }
    }
}