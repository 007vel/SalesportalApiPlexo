CREATE TABLE [dbo].[TrainingHubLinks] (
    [OId]                        INT            IDENTITY (1, 1) NOT NULL,
    [ProductVideoEnglishLink]    NVARCHAR (500) NULL,
    [ProductVideoSpanishLink]    NVARCHAR (500) NULL,
    [DashboardVideoEnglishLink]  NVARCHAR (500) NULL,
    [DashboardVideoSpanishLink]  NVARCHAR (500) NULL,
    [KnowledgeBaseLink]          NVARCHAR (500) NULL,
    [SalesLeadLink]              NVARCHAR (500) NULL,
    PRIMARY KEY CLUSTERED ([OId] ASC)
);
