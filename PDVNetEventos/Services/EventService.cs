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
        // ======== Regras de negócio / vínculos ========

        /// Valida conflito de agenda entre eventos.
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

        /// Vincula participante ao evento (capacidade + duplicidade).
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

        /// Vincula fornecedor ao evento (controle de orçamento + duplicidade).
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
                    $"Orçamento excedido. Restante: {(ev.OrcamentoMaximo - usado):C}.");

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

        // ======== CRUD: Evento ========

        public async Task<int> CriarEventoAsync(Evento ev)
        {
            await ValidarDatasEventoAsync(ev);
            using var db = new AppDbContext();
            db.Eventos.Add(ev);
            await db.SaveChangesAsync();
            return ev.Id;
        }

        public async Task AtualizarEventoAsync(Evento ev)
        {
            await ValidarDatasEventoAsync(ev);
            using var db = new AppDbContext();
            db.Eventos.Update(ev);
            await db.SaveChangesAsync();
        }

        public async Task RemoverEventoAsync(int eventoId)
        {
            using var db = new AppDbContext();

            var ev = await db.Eventos.FindAsync(eventoId)
                ?? throw new InvalidOperationException("Evento não encontrado.");

            var participantes = db.EventosParticipantes.Where(x => x.EventoId == eventoId);
            var fornecedores = db.EventosFornecedores.Where(x => x.EventoId == eventoId);

            db.EventosParticipantes.RemoveRange(participantes);
            db.EventosFornecedores.RemoveRange(fornecedores);
            db.Eventos.Remove(ev);

            await db.SaveChangesAsync();
        }

        // ======== CRUD: Participante ========

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

        public async Task AtualizarParticipanteAsync(Participante p)
        {
            using var db = new AppDbContext();
            db.Participantes.Update(p);
            await db.SaveChangesAsync();
        }

        public async Task RemoverParticipanteAsync(int participanteId)
        {
            using var db = new AppDbContext();
            var joins = db.EventosParticipantes.Where(x => x.ParticipanteId == participanteId);
            db.EventosParticipantes.RemoveRange(joins);

            var p = await db.Participantes.FindAsync(participanteId);
            if (p != null) db.Participantes.Remove(p);

            await db.SaveChangesAsync();
        }

        // ======== CRUD: Fornecedor ========

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

        public async Task AtualizarFornecedorAsync(Fornecedor f)
        {
            using var db = new AppDbContext();
            db.Fornecedores.Update(f);
            await db.SaveChangesAsync();
        }

        public async Task RemoverFornecedorAsync(int fornecedorId)
        {
            using var db = new AppDbContext();
            var joins = db.EventosFornecedores.Where(x => x.FornecedorId == fornecedorId);
            db.EventosFornecedores.RemoveRange(joins);

            var f = await db.Fornecedores.FindAsync(fornecedorId);
            if (f != null) db.Fornecedores.Remove(f);

            await db.SaveChangesAsync();
        }
    }
}