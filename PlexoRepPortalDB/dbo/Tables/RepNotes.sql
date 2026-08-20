CREATE TABLE [dbo].[RepNotes] (
    [OId]         INT            IDENTITY (1, 1) NOT NULL,
    [RepId]       NVARCHAR (30)  NOT NULL,
    [Kind]        NVARCHAR (20)  NOT NULL,
    [Text]        NVARCHAR (MAX) NULL,
    [CreatedAt]   DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]   DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([OId] ASC),
    CONSTRAINT [FK_RepNotes_Reps] FOREIGN KEY ([RepId]) REFERENCES [dbo].[Reps] ([RepId]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RepNotes_RepId_Kind]
    ON [dbo].[RepNotes]([RepId] ASC, [Kind] ASC);
