CREATE TABLE [dbo].[TrainingHubDocuments] (
    [OId]         INT            IDENTITY (1, 1) NOT NULL,
    [RoleId]      NVARCHAR (20)  NULL,
    [Title]       NVARCHAR (200) NOT NULL,
    [Category]    NVARCHAR (100) NULL,
    [Description] NVARCHAR (1000) NULL,
    [FileType]    NVARCHAR (20)  NOT NULL,
    [FileName]    NVARCHAR (300) NOT NULL,
    [FilePath]    NVARCHAR (500) NOT NULL,
    [Length]      NVARCHAR (50)  NULL,
    [UploadedBy]  NVARCHAR (10)  DEFAULT ('Rep') NOT NULL,
    [UploadedAt]  DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    [Language] VARCHAR(50) NOT NULL DEFAULT ('English'),
    [HubType]     NVARCHAR (20)  NOT NULL DEFAULT ('Training'),
    PRIMARY KEY CLUSTERED ([OId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_TrainingHubDocuments_RoleId]
    ON [dbo].[TrainingHubDocuments]([RoleId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_TrainingHubDocuments_HubType]
    ON [dbo].[TrainingHubDocuments]([HubType] ASC);
