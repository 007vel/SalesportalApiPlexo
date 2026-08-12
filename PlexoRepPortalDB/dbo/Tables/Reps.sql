CREATE TABLE [dbo].[Reps] (
    [OId]           INT            IDENTITY (1, 1) NOT NULL,
    [RepId]         NVARCHAR (30)  NOT NULL,
    [FullName]      NVARCHAR (200) NOT NULL,
    [Email]         NVARCHAR (256) NOT NULL,
    [Phone]         NVARCHAR (30)  NULL,
    [Address]       NVARCHAR (300) NULL,
    [City]          NVARCHAR (100) NULL,
    [State]         NVARCHAR (50)  NULL,
    [Zip]           NVARCHAR (20)  NULL,
    [GoogleLink]    NVARCHAR (500) NULL,
    [ResourceLink]  NVARCHAR (500) NULL,
    [PricingSheetLink] NVARCHAR (500) NULL,
    [PowerPointLink]   NVARCHAR (500) NULL,
    [Status]        TINYINT        DEFAULT ((1)) NOT NULL,
    [PassedCertification] BIT      DEFAULT ((0)) NOT NULL,
    [BusinessCardsSent]   BIT      DEFAULT ((0)) NOT NULL,
    [ConsultantFeePaid]   BIT      DEFAULT ((0)) NOT NULL,
    [CreatedAt]     DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]     DATETIME2 (7)  DEFAULT (sysutcdatetime()) NOT NULL,
    PRIMARY KEY CLUSTERED ([OId] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Reps_RepId]
    ON [dbo].[Reps]([RepId] ASC);
