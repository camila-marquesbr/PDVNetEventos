using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Data;

namespace PDVNetEventos.Services
{
    public class RelatoriosService
    {
        public async Task<List<AgendaLinha>> ObterAgendaParticipanteAsync(int participanteId)
        {
            using var db = new AppDbContext();
            return await db.EventosParticipantes
                .Where(ep => ep.ParticipanteId == participanteId)
                .Select(ep => new AgendaLinha
                {
                    EventoId = ep.EventoId,
                    Evento = ep.Evento!.Nome,
                    DataInicio = ep.Evento.DataInicio,
                    DataFim = ep.Evento.DataFim
                })
                .OrderBy(x => x.DataInicio)
                .ToListAsync();
        }

        public async Task<List<TopFornecedorLinha>> ObterFornecedoresMaisUsadosAsync()
        {
            using var db = new AppDbContext();
            return await db.EventosFornecedores
                .GroupBy(x => x.Fornecedor!.NomeServico)
                .Select(g => new TopFornecedorLinha
                {
                    Servico = g.Key,
                    Quantidade = g.Count(),
                    Total = g.Sum(x => x.ValorAcordado)
                })
                .OrderByDescending(x => x.Quantidade)
                .ToListAsync();
        }

        public async Task<List<TiposParticipanteLinha>> ObterDistribuicaoTiposParticipanteAsync()
        {
            using var db = new AppDbContext();
            return await db.Participantes
                .GroupBy(p => p.Tipo)
                .Select(g => new TiposParticipanteLinha
                {
                    Tipo = g.Key.ToString(),
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToListAsync();
        }

        public async Task<List<SaldoEventoLinha>> ObterSaldoEventosAsync()
        {
            using var db = new AppDbContext();
            var baseQry = await db.Eventos
                .Select(e => new
                {
                    e.Id,
                    e.Nome,
                    e.OrcamentoMaximo,
                    Gasto = db.EventosFornecedores
                                .Where(v => v.EventoId == e.Id)
                                .Sum(v => (decimal?)v.ValorAcordado) ?? 0m
                })
                .ToListAsync();

            return baseQry
                .Select(x => new SaldoEventoLinha
                {
                    EventoId = x.Id,
                    Evento = x.Nome,
                    Orcamento = x.OrcamentoMaximo,
                    Gasto = x.Gasto,
                    Saldo = x.OrcamentoMaximo - x.Gasto
                })
                .OrderBy(e => e.Evento)
                .ToList();
        }
    }

    // DTOs dos relatórios
    public record AgendaLinha
    {
        public int EventoId { get; init; }
        public string Evento { get; init; } = "";
        public DateTime DataInicio { get; init; }
        public DateTime DataFim { get; init; }
    }

    public record TopFornecedorLinha
    {
        public string Servico { get; init; } = "";
        public int Quantidade { get; init; }
        public decimal Total { get; init; }
    }

    public record TiposParticipanteLinha
    {
        public string Tipo { get; init; } = "";
        public int Quantidade { get; init; }
    }

    public record SaldoEventoLinha
    {
        public int EventoId { get; init; }
        public string Evento { get; init; } = "";
        public decimal Orcamento { get; init; }
        public decimal Gasto { get; init; }
        public decimal Saldo { get; init; }
    }
}