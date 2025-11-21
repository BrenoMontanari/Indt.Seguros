USE IndtSeguros;
GO

CREATE TABLE Propostas (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    NomeCliente  VARCHAR(200) NOT NULL,
    Produto      VARCHAR(100) NOT NULL,
    Premio       DECIMAL(18,2) NOT NULL,
    Status       VARCHAR(20) NOT NULL,
    DataCriacao  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

CREATE TABLE Contratacoes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PropostaId      INT NOT NULL,
    DataContratacao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Contratacoes_Propostas
        FOREIGN KEY (PropostaId) REFERENCES Propostas(Id)
        -- opcional: comportamento de delete
        -- ON DELETE NO ACTION   -- padrão
        -- ON DELETE CASCADE     -- se quiser apagar contratações ao apagar a proposta
);
GO