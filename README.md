# Indt.Seguros

Projeto de exemplo para avaliação técnica de arquitetura em .NET, simulando um domínio simples de **seguros** com:

- **Propostas** de seguro
- **Contratações** vinculadas a propostas aprovadas

A solução foi construída com **arquitetura em camadas / hexagonal “light”**, usando **.NET 8**, **ASP.NET Core Web API** e **Dapper** para acesso ao banco de dados SQL Server LocalDB.

---

## ⚙️ Tecnologias utilizadas

- [.NET 8](https://dotnet.microsoft.com/)
- ASP.NET Core Web API
- Dapper
- SQL Server LocalDB
- xUnit + Moq (testes unitários)
- Swagger / OpenAPI

---

## 🧱 Arquitetura e estrutura de projetos

Solution: **`Indt.Seguros`**

Projetos:

- **`PropostaService.Api`**
  - Camada de **API** (controllers, configuração de DI, Swagger, etc.)
- **`PropostaService.Application`**
  - **Casos de uso** (Application Services)
  - DTOs
  - Interfaces de repositório (ports)
- **`PropostaService.Domain`**
  - Entidades de domínio: `Proposta`, `Contratacao`
  - Enum de status: `StatusProposta` (`EmAnalise`, `Aprovada`, `Rejeitada`)
- **`PropostaService.Infrastructure`**
  - Implementações de repositório usando **Dapper**
  - Configuração de acesso a dados via `IDbConnection`
- **`IndtSeguros.Tests`**
  - Testes unitários para `PropostaAppService` e `ContratacaoAppService`
  - xUnit + Moq

A **API** fala apenas com a **camada Application**, que por sua vez conversa com o **Domain** e depende de **interfaces** de repositório.  
As implementações concretas com Dapper ficam isoladas em **Infrastructure**, seguindo o conceito de **Ports & Adapters (arquitetura hexagonal)**.

---

## 🗄️ Modelagem de dados

Banco utilizado: **`IndtSeguros`** (SQL Server LocalDB).

### Script SQL

```sql
CREATE DATABASE IndtSeguros;
GO

USE IndtSeguros;
GO

CREATE TABLE Propostas (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    NomeCliente  VARCHAR(200) NOT NULL,
    Produto      VARCHAR(100) NOT NULL,
    Premio       DECIMAL(18,2) NOT NULL,
    Status       VARCHAR(20) NOT NULL, -- EmAnalise, Aprovada, Rejeitada
    DataCriacao  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

CREATE TABLE Contratacoes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PropostaId      INT NOT NULL,
    DataContratacao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Contratacoes_Propostas
        FOREIGN KEY (PropostaId) REFERENCES Propostas(Id)
);
GO
