using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Data;
using PDVNetEventos.Data.Entities;

namespace PDVNetEventos.Services
{
    public class EventService
    {
        private readonly Func<AppDbContext> _db;

        // Construtor padrão (app WPF): usa SQL Server via OnConfiguring
        public EventService() : this(() => new AppDbContext()) { }

        // Construtor para testes/DI: você injeta a fábrica do contexto
        public EventService(Func<AppDbContext> dbFactory)
        {
            _db = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        // ========= EVENTO =========

        public async Task ValidarDatasEventoAsync(Evento novo)
        {
            using var db = _db();
            bool conflita = await db.Eventos
                .AnyAsync(e => e.Id != novo.Id &&
                               novo.DataInicio <= e.DataFim &&
                               novo.DataFim >= e.DataInicio);

            if (conflita)
                throw new InvalidOperationException(
                    "Conflito de agenda: já existe um evento que se sobrepõe a estas datas.");
        }

        public async Task<Evento> ObterEventoAsync(int id)
        {
            using var db = _db();
            return await db.Eventos.FirstOrDefaultAsync(e => e.Id == id)
                   ?? throw new InvalidOperationException("Evento não encontrado.");
        }

        public async Task AtualizarEventoAsync(Evento e)
        {
            using var db = _db();
            await ValidarDatasEventoAsync(e);

            var atual = await db.Eventos.FirstOrDefaultAsync(x => x.Id == e.Id)
                        ?? throw new InvalidOperationException("Evento não encontrado.");

            atual.Nome = e.Nome;
            atual.DataInicio = e.DataInicio;
            atual.DataFim = e.DataFim;
            atual.CapacidadeMaxima = e.CapacidadeMaxima;
            atual.OrcamentoMaximo = e.OrcamentoMaximo;
            atual.TipoEventoId = e.TipoEventoId;

            await db.SaveChangesAsync();
        }

        // ========= VÍNCULO Participante ↔ Evento =========
        public async Task AdicionarParticipanteAsync(int eventoId, int participanteId)
        {
            using var db = _db();

            var evento = await db.Eventos
                .AsNoTracking()
                .Where(e => e.Id == eventoId)
                .Select(e => new { e.Id, e.DataInicio, e.DataFim, e.CapacidadeMaxima })
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Evento não encontrado.");

            bool participanteExiste = await db.Participantes.AnyAsync(p => p.Id == participanteId);
            if (!participanteExiste)
                throw new InvalidOperationException("Participante não encontrado.");

            bool jaVinculado = await db.EventosParticipantes
                .AnyAsync(ep => ep.EventoId == eventoId && ep.ParticipanteId == participanteId);
            if (jaVinculado)
                throw new InvalidOperationException("Participante já está vinculado a este evento.");

            bool conflito = await db.EventosParticipantes
                .Where(ep => ep.ParticipanteId == participanteId && ep.EventoId != eventoId)
                .Select(ep => ep.Evento!)
                .AnyAsync(e => !(e.DataFim < evento.DataInicio || e.DataInicio > evento.DataFim));
            if (conflito)
                throw new InvalidOperationException("Participante já está em evento nas mesmas datas.");

            int qtdAtual = await db.EventosParticipantes.CountAsync(ep => ep.EventoId == eventoId);
            if (qtdAtual >= evento.CapacidadeMaxima)
                throw new InvalidOperationException("Capacidade do evento atingida.");

            db.EventosParticipantes.Add(new EventoParticipante
            {
                EventoId = eventoId,
                ParticipanteId = participanteId
            });

            await db.SaveChangesAsync();
        }

        public async Task RemoverParticipanteAsync(int eventoId, int participanteId)
        {
            using var db = _db();
            var vinc = await db.EventosParticipantes
                .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.ParticipanteId == participanteId)
                ?? throw new InvalidOperationException("Vínculo não encontrado.");

            db.EventosParticipantes.Remove(vinc);
            await db.SaveChangesAsync();
        }

        public async Task RemoverFornecedorAsync(int eventoId, int fornecedorId)
        {
            using var db = _db();
            var vinc = await db.EventosFornecedores
                .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.FornecedorId == fornecedorId)
                ?? throw new InvalidOperationException("Vínculo não encontrado.");

            db.EventosFornecedores.Remove(vinc);
            await db.SaveChangesAsync();
        }

        // ========= CRUD Participante =========
        public async Task<int> CriarParticipanteAsync(string nome, string cpf, string? telefone, TipoParticipante tipo)
        {
            using var db = _db();

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

        public async Task<Participante> ObterParticipanteAsync(int id)
        {
            using var db = _db();
            return await db.Participantes.FirstOrDefaultAsync(x => x.Id == id)
                   ?? throw new InvalidOperationException("Participante não encontrado.");
        }

        public async Task AtualizarParticipanteAsync(Participante p)
        {
            using var db = _db();

            bool cpfEmUso = await db.Participantes.AnyAsync(x => x.Id != p.Id && x.CPF == p.CPF);
            if (cpfEmUso) throw new InvalidOperationException("Já existe participante com este CPF.");

            var atual = await db.Participantes.FirstOrDefaultAsync(x => x.Id == p.Id)
                ?? throw new InvalidOperationException("Participante não encontrado.");

            atual.NomeCompleto = p.NomeCompleto;
            atual.CPF = p.CPF;
            atual.Telefone = p.Telefone;
            atual.Tipo = p.Tipo;

            await db.SaveChangesAsync();
        }

        // ========= CRUD Fornecedor =========
        public async Task<int> CriarFornecedorAsync(string nomeServico, string cnpj, decimal? precoPadrao)
        {
            using var db = _db();

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

        public async Task<Fornecedor> ObterFornecedorAsync(int id)
        {
            using var db = _db();
            return await db.Fornecedores.FirstOrDefaultAsync(x => x.Id == id)
                   ?? throw new InvalidOperationException("Fornecedor não encontrado.");
        }

        public async Task AtualizarFornecedorAsync(Fornecedor f)
        {
            using var db = _db();

            bool cnpjEmUso = await db.Fornecedores.AnyAsync(x => x.Id != f.Id && x.CNPJ == f.CNPJ);
            if (cnpjEmUso) throw new InvalidOperationException("Já existe fornecedor com este CNPJ.");

            var atual = await db.Fornecedores.FirstOrDefaultAsync(x => x.Id == f.Id)
                ?? throw new InvalidOperationException("Fornecedor não encontrado.");

            atual.NomeServico = f.NomeServico;
            atual.CNPJ = f.CNPJ;
            atual.PrecoPadrao = f.PrecoPadrao;

            await db.SaveChangesAsync();
        }

        public async Task<decimal> ObterTotalGastosEventoAsync(int eventoId)
        {
            using var db = _db();
            var total = await db.EventosFornecedores
                .Where(x => x.EventoId == eventoId)
                .SumAsync(x => (decimal?)x.ValorAcordado) ?? 0m;
            return total;
        }

        public async Task<decimal> ObterSaldoOrcamentoEventoAsync(int eventoId)
        {
            using var db = _db();
            var dados = await db.Eventos
                .Where(e => e.Id == eventoId)
                .Select(e => new
                {
                    Orcamento = e.OrcamentoMaximo,
                    Gasto = db.EventosFornecedores
                              .Where(v => v.EventoId == e.Id)
                              .Sum(v => (decimal?)v.ValorAcordado) ?? 0m
                })
                .FirstOrDefaultAsync();

            return dados == null ? 0m : dados.Orcamento - dados.Gasto;
        }

        public async Task AdicionarFornecedorAsync(int eventoId, int fornecedorId, decimal valorAcordado)
        {
            using var db = _db();

            if (valorAcordado <= 0)
                throw new InvalidOperationException("O valor acordado deve ser maior que zero.");

            var evento = await db.Eventos.FindAsync(eventoId)
                         ?? throw new InvalidOperationException("Evento não encontrado.");

            bool jaVinculado = await db.EventosFornecedores
                .AnyAsync(x => x.EventoId == eventoId && x.FornecedorId == fornecedorId);
            if (jaVinculado)
                throw new InvalidOperationException("Este fornecedor já está vinculado a este evento.");

            decimal gastoAtual = await db.EventosFornecedores
                .Where(x => x.EventoId == eventoId)
                .SumAsync(x => (decimal?)x.ValorAcordado) ?? 0m;

            if (gastoAtual + valorAcordado > evento.OrcamentoMaximo)
            {
                var excedente = gastoAtual + valorAcordado - evento.OrcamentoMaximo;
                throw new InvalidOperationException(
                    $"Orçamento excedido. Orçamento: R$ {evento.OrcamentoMaximo:N2}, " +
                    $"Gasto atual: R$ {gastoAtual:N2}, Novo valor: R$ {valorAcordado:N2}. " +
                    $"Excederia em R$ {excedente:N2}.");
            }

            db.EventosFornecedores.Add(new EventoFornecedor
            {
                EventoId = eventoId,
                FornecedorId = fornecedorId,
                ValorAcordado = valorAcordado
            });

            await db.SaveChangesAsync();
        }

        public async Task AtualizarValorFornecedorAsync(int eventoId, int fornecedorId, decimal novoValor)
        {
            using var db = _db();

            if (novoValor <= 0)
                throw new InvalidOperationException("O valor acordado deve ser maior que zero.");

            var evento = await db.Eventos.FindAsync(eventoId)
                         ?? throw new InvalidOperationException("Evento não encontrado.");

            var vinculo = await db.EventosFornecedores
                .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.FornecedorId == fornecedorId)
                ?? throw new InvalidOperationException("Vínculo não encontrado.");

            decimal gastoOutros = await db.EventosFornecedores
                .Where(x => x.EventoId == eventoId && x.FornecedorId != fornecedorId)
                .SumAsync(x => (decimal?)x.ValorAcordado) ?? 0m;

            if (gastoOutros + novoValor > evento.OrcamentoMaximo)
            {
                var excedente = (gastoOutros + novoValor) - evento.OrcamentoMaximo;
                throw new InvalidOperationException(
                    $"Orçamento excedido. Orçamento: R$ {evento.OrcamentoMaximo:N2}, " +
                    $"Gasto (outros): R$ {gastoOutros:N2}, Novo valor: R$ {novoValor:N2}. " +
                    $"Excederia em R$ {excedente:N2}.");
            }

            vinculo.ValorAcordado = novoValor;
            await db.SaveChangesAsync();
        }
    }
}