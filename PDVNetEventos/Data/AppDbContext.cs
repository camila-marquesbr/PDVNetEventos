using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Data.Entities;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace PDVNetEventos.Data
{
    public class AppDbContext : DbContext
    {
        // Ajuste a conexão conforme sua instalação (use SQLEXPRESS se for o seu caso)
        private const string CONN =
            "Server=localhost;Database=PDVNetEventosDb;Trusted_Connection=True;TrustServerCertificate=True";

        public DbSet<Evento> Eventos => Set<Evento>();
        public DbSet<TipoEvento> TiposEvento => Set<TipoEvento>();
        public DbSet<Participante> Participantes => Set<Participante>();
        public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
        public DbSet<EventoFornecedor> EventosFornecedores => Set<EventoFornecedor>();
        public DbSet<EventoParticipante> EventosParticipantes => Set<EventoParticipante>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost;Database=PDVNetEventosDb;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }
        protected override void OnModelCreating(ModelBuilder b)
        {
            // Tabelas de junção (chaves compostas)
            b.Entity<EventoFornecedor>().HasKey(x => new { x.EventoId, x.FornecedorId });
            b.Entity<EventoParticipante>().HasKey(x => new { x.EventoId, x.ParticipanteId });

            // Índices/Unicidade
            b.Entity<Participante>().HasIndex(x => x.CPF).IsUnique();
            b.Entity<Fornecedor>().HasIndex(x => x.CNPJ).IsUnique();

            // Tipos numéricos
            b.Entity<Evento>().Property(e => e.OrcamentoMaximo).HasColumnType("decimal(18,2)");
            b.Entity<EventoFornecedor>().Property(e => e.ValorAcordado).HasColumnType("decimal(18,2)");

            // Constraint simples
            b.Entity<Evento>()
             .ToTable(tb => tb.HasCheckConstraint("CK_Evento_Datas", "[DataInicio] <= [DataFim]"));

            // Seed de tipos de evento
            b.Entity<TipoEvento>().HasData(
                new TipoEvento { Id = 1, Descricao = "Congresso" },
                new TipoEvento { Id = 2, Descricao = "Workshop" },
                new TipoEvento { Id = 3, Descricao = "Interno" }
            );
        }

    }
}


