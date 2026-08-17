CREATE TABLE [dbo].[RepBankDetails] (
    [OId]                     INT            IDENTITY (1, 1) NOT NULL,
    [RepId]                   NVARCHAR (30)  NULL,
    [BankName]                NVARCHAR (500) NULL,
    [RoutingNumber]           NVARCHAR (500) NULL,
    [AccountNumber]           NVARCHAR (500) NULL,
    [CreatedAt]               DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]               DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([OId] ASC),
    CONSTRAINT [FK_RepBankDetails_Reps] FOREIGN KEY ([RepId]) REFERENCES [dbo].[Reps] ([RepId]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RepBankDetails_RepId]
    ON [dbo].[RepBankDetails]([RepId] ASC);
