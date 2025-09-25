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
        // construtor usado nos TESTES (InMemory) e também em DI, se quiser
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // construtor usado pela aplicação WPF quando você não injeta opções
        public AppDbContext() { }

        // (opcional) se quiser centralizar a string:
        private const string CONN =
            "Server=localhost;Database=PDVNetEventosDb;Trusted_Connection=True;TrustServerCertificate=True;";

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
                optionsBuilder.UseSqlServer(CONN);
            }
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Tabelas de junção (chave composta)
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

            // Seed de Tipos de Evento (InMemory só aplica se usar EnsureCreated;
            // em testes, prefira popular explicitamente quando necessário)
            b.Entity<TipoEvento>().HasData(
                new TipoEvento { Id = 1, Descricao = "Congresso" },
                new TipoEvento { Id = 2, Descricao = "Workshop" },
                new TipoEvento { Id = 3, Descricao = "Interno" }
            );
        }
    }
}


