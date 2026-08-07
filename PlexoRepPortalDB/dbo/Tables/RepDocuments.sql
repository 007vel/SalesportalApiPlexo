CREATE TABLE [dbo].[RepDocuments] (
    [OId]         INT            IDENTITY (1, 1) NOT NULL,
    [RepId]       NVARCHAR (30)  NOT NULL,
    [Kind]        NVARCHAR (20)  NOT NULL,
    [FileName]    NVARCHAR (300) NOT NULL,
    [FilePath]    NVARCHAR (500) NOT NULL,
    [UploadedAt]  DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([OId] ASC),
    CONSTRAINT [FK_RepDocuments_Reps] FOREIGN KEY ([RepId]) REFERENCES [dbo].[Reps] ([RepId]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_RepDocuments_RepId]
    ON [dbo].[RepDocuments]([RepId] ASC);
