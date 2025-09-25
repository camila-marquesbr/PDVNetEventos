## PDVNetEventos

Aplicação **WPF + EF Core (SQL Server)** para gestão de **Eventos**, **Participantes** e **Fornecedores**, com:
- **Integração de CEP (ViaCEP)** para buscar e salvar endereço do evento
- **CRUD separado** de **Tipos de Evento**
- **Regras de negócio** (orçamento, lotação, conflitos de data)
- **Relatórios** (agenda do participante, top fornecedores, tipos de participante, saldo dos eventos)

## Sumário
- [Requisitos](#requisitos)
- [Configuração do Banco (connection-string)](#configuração-do-banco-connection-string)
- [Migrations & Base de Dados](#migrations--base-de-dados)
- [Como Rodar](#como-rodar)
- [Como Usar (fluxo-rápido)](#como-usar-fluxo-rápido)
- [Funcionalidades](#funcionalidades)
- [Regras de Negócio](#regras-de-negócio)
- [Relatórios](#relatórios)
- [Integração de CEP](#integração-de-cep)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Dicas & Troubleshooting](#dicas--troubleshooting)
- [Licença](#licença)

---

## Requisitos
- **.NET SDK** 7.0 ou 8.0  
- **SQL Server** (LocalDB ou servidor)  
- **Ferramentas EF Core (CLI):**
  ```bash
  dotnet tool install --global dotnet-ef
  
**(Opcional) Visual Studio 2022 com workload .NET Desktop**

# Configuração do Banco (connection string)
Edite a connection string no AppDbContext (ou no local onde você a definiu). Exemplos:

**LocalDB:**
Server=(localdb)\MSSQLLocalDB;Database=PDVNetEventosDb;Trusted_Connection=True;TrustServerCertificate=True

**SQL Server (usuário/senha):**
Server=localhost;Database=PDVNetEventosDb;User Id=sa;Password=SuaSenhaSegura;TrustServerCertificate=True

⚠️ Não commitar senhas reais. Para avaliação, deixe um exemplo claro no README e/ou use variáveis de ambiente.


# Migrations & Base de Dados

Com a connection string configurada:

1. (Opcional) Ver as migrations disponíveis:
dotnet ef migrations list


2. Criar/atualizar o banco:
dotnet ef database update


3. (Opcional) Resetar o banco em desenvolvimento:
dotnet ef database drop -f
dotnet ef database update


*Se você alterou entidades, gere uma migration nova:*
dotnet ef migrations add NomeDaMigration
dotnet ef database update

# Como Rodar
**Via Visual Studio**
Abrir a solução → F5.

Via CLI
Na pasta do projeto WPF:

dotnet build
dotnet run --project PDVNetEventos

# Como Usar (fluxo rápido)

1.Tipos de Evento → crie alguns (ex.: Workshop, Palestra).

2. Eventos → cadastre um evento: informe Observações (obrigatório), CEP (busca ViaCEP) e demais dados.

3. Participantes / Fornecedores → cadastre e vincule aos eventos.

  - O sistema bloqueia: orçamento estourado, lotação máxima e conflito de datas.

4. Relatórios → acesse pela Home:

   - Agenda por participante

   - Top fornecedores (Qtde/Total)

   - Tipos de participante

  - Saldo dos eventos (Orçamento − Gasto)

# Funcionalidades

  - Eventos (com Observações + Endereço via CEP)

  - Participantes (Nome, CPF, Telefone, Tipo)

  - Fornecedores (Nome do Serviço, CNPJ, valores acordados por evento)

  - Tipos de Evento (CRUD separado e referenciado nos eventos)

  - Relatórios (4 telas dedicadas)

  - Home organizada por Cadastros e Relatórios

# Regras de Negócio

  **Implementadas em Services/EventService.cs:**

  - Total do evento = soma dos ValorAcordado dos fornecedores

  - Orçamento: impede vínculo/atualização que ultrapasse OrcamentoMaximo

  - Conflito de datas: participante não pode estar em eventos com períodos que se sobrepõem

  - Lotação máxima: impede vínculo quando CapacidadeMaxima é atingida

# Relatórios

 **Implementados via Services/RelatoriosService.cs e VMs/Views dedicados:**

  - Agenda do Participante (filtro por participante)

  - Fornecedores Mais Utilizados (Quantidade e Total gasto)

  - Tipos de Participante (distribuição por enum)

  - Saldo dos Eventos (Orçamento, Gasto, Saldo)

# Integração de CEP

  - Serviço: Services/Cep/ViaCepService.cs

  - Sub-ViewModel: ViewModels/EnderecoFormViewModel.cs (com comando Buscar CEP)

  - Uso no Evento: cadastroEventoViewModel + cadastroEvento.xaml

  - Persistência do endereço no Evento (Cep, Logradouro, Complemento, Bairro, Localidade, Uf)

## Estrutura do Projeto

No projeto, foi aplicado o princípio **SRP (Single Responsibility Principle)**, garantindo que cada classe tivesse uma responsabilidade única.  

- `EventService` centraliza regras de negócio de eventos.  
- `ViaCepService` é responsável apenas pela integração com a API de CEP.  
- `EnderecoFormViewModel` lida apenas com os campos e comandos de endereço.  
- `cadastroEventoViewModel` e outros ViewModels cuidam exclusivamente da lógica de tela.  

Esse desacoplamento facilita a manutenção, os testes e a evolução do sistema.

- **Data/**
  - **Entities/**
    - Evento.cs
    - Participante.cs
    - Fornecedor.cs
    - TipoEvento.cs
    - (classes de associação)
  - **Migrations/**
  - AppDbContext.cs
- **Services/**
  - EventService.cs
  - RelatoriosService.cs
  - **Cep/**
    - ICepService.cs
    - ViaCepService.cs
- **ViewModels/**
  - cadastroEventoViewModel.cs
  - cadastroParticipanteViewModel.cs
  - cadastroFornecedorViewModel.cs
  - EnderecoFormViewModel.cs
  - ListarEventosViewModel.cs
  - (VMs de relatórios e tipos de evento)
- **Views/**
  - MainWindow.xaml (+ .cs)
  - (XAMLs de cadastros, listagens, relatórios, CRUD de Tipo de Evento)

## Autenticação (mock “JWT”)

Este projeto inclui um **login simples** para fins de demonstração. Não há emissão de JWT real; é gerado um **token de sessão mock** (GUID) após credenciais válidas.

### Como funciona
- Serviço: `Services/Auth/MockAuthService.cs` implementa `IAuthService`.
- Usuários de exemplo:
  - `admin / 123`
  - `user / 123`
- Ao logar com sucesso, o serviço preenche:
  - `CurrentUser` e `CurrentToken` (ex.: `mock-jwt-<guid>`).
- O app inicia na tela de **Login** e, após autenticar, abre a `MainWindow`.

### Arquivos relevantes
- `Services/Auth/IAuthService.cs` – contrato do serviço de autenticação
- `Services/Auth/MockAuthService.cs` – implementação mock
- `Views/LoginWindow.xaml` e `LoginWindow.xaml.cs` – UI de login
- `App.xaml.cs` – fluxo de startup abrindo o Login

### Início da aplicação
O `App.xaml` **não** usa `StartupUri`. O fluxo é controlado em `App.xaml.cs`:

```csharp
// App.xaml.cs (trecho)
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    IAuthService auth = new MockAuthService();

    var login = new LoginWindow(auth);
    login.Show();
}

## Testes (xUnit + EF Core InMemory)

Rodar todos os testes:
```bash
dotnet test
Stack:

xUnit

Microsoft.EntityFrameworkCore.InMemory

FluentAssertions

coverlet (coleta de cobertura)





