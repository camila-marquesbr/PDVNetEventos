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
        // ========= EVENTO =========

        public async Task ValidarDatasEventoAsync(Evento novo)
        {
            using var db = new AppDbContext();

            bool conflita = await db.Eventos
                .AnyAsync(e =>
                    e.Id != novo.Id &&
                    novo.DataInicio <= e.DataFim &&
                    novo.DataFim >= e.DataInicio);

            if (conflita)
                throw new InvalidOperationException(
                    "Conflito de agenda: já existe um evento que se sobrepõe a estas datas.");
        }

        public async Task<Evento> ObterEventoAsync(int id)
        {
            using var db = new AppDbContext();
            return await db.Eventos.FirstOrDefaultAsync(e => e.Id == id)
                   ?? throw new InvalidOperationException("Evento não encontrado.");
        }

        public async Task AtualizarEventoAsync(Evento e)
        {
            using var db = new AppDbContext();

            // valida regra de sobreposição
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
            using var db = new AppDbContext();

            var ev = await db.Eventos.FirstOrDefaultAsync(x => x.Id == eventoId)
                     ?? throw new InvalidOperationException("Evento não encontrado.");

            _ = await db.Participantes.FindAsync(participanteId)
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

        public async Task RemoverParticipanteAsync(int eventoId, int participanteId)
        {
            using var db = new AppDbContext();

            var vinc = await db.EventosParticipantes
                .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.ParticipanteId == participanteId)
                ?? throw new InvalidOperationException("Vínculo não encontrado.");

            db.EventosParticipantes.Remove(vinc);
            await db.SaveChangesAsync();
        }

        // ========= VÍNCULO Fornecedor ↔ Evento =========

        public async Task AdicionarFornecedorAsync(int eventoId, int fornecedorId, decimal valorAcordado)
        {
            using var db = new AppDbContext();

            var ev = await db.Eventos.FirstOrDefaultAsync(x => x.Id == eventoId)
                     ?? throw new InvalidOperationException("Evento não encontrado.");

            _ = await db.Fornecedores.FindAsync(fornecedorId)
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

        public async Task RemoverFornecedorAsync(int eventoId, int fornecedorId)
        {
            using var db = new AppDbContext();

            var vinc = await db.EventosFornecedores
                .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.FornecedorId == fornecedorId)
                ?? throw new InvalidOperationException("Vínculo não encontrado.");

            db.EventosFornecedores.Remove(vinc);
            await db.SaveChangesAsync();
        }

        // ========= CRUD Participante =========
        public async Task<int> CriarParticipanteAsync(string nome, string cpf, string? telefone, TipoParticipante tipo)
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

        public async Task<Participante> ObterParticipanteAsync(int id)
        {
            using var db = new AppDbContext();
            return await db.Participantes.FirstOrDefaultAsync(x => x.Id == id)
                   ?? throw new InvalidOperationException("Participante não encontrado.");
        }

        public async Task AtualizarParticipanteAsync(Participante p)
        {
            using var db = new AppDbContext();

            bool cpfEmUso = await db.Participantes
                .AnyAsync(x => x.Id != p.Id && x.CPF == p.CPF);
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

        public async Task<Fornecedor> ObterFornecedorAsync(int id)
        {
            using var db = new AppDbContext();
            return await db.Fornecedores.FirstOrDefaultAsync(x => x.Id == id)
                   ?? throw new InvalidOperationException("Fornecedor não encontrado.");
        }

        public async Task AtualizarFornecedorAsync(Fornecedor f)
        {
            using var db = new AppDbContext();

            bool cnpjEmUso = await db.Fornecedores
                .AnyAsync(x => x.Id != f.Id && x.CNPJ == f.CNPJ);
            if (cnpjEmUso) throw new InvalidOperationException("Já existe fornecedor com este CNPJ.");

            var atual = await db.Fornecedores.FirstOrDefaultAsync(x => x.Id == f.Id)
                ?? throw new InvalidOperationException("Fornecedor não encontrado.");

            atual.NomeServico = f.NomeServico;
            atual.CNPJ = f.CNPJ;
            atual.PrecoPadrao = f.PrecoPadrao;

            await db.SaveChangesAsync();
        }
    }
}
