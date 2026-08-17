/*
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:      :r .\myfile.sql
 Use SQLCMD syntax to reference a variable in the post-deployment script.
 Example:      :setvar TableName MyTable
               SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*/

IF NOT EXISTS (SELECT 1 FROM [dbo].[TrainingHubLinks])
BEGIN
    INSERT INTO [dbo].[TrainingHubLinks]
        ([ProductVideoEnglishLink], [ProductVideoSpanishLink], [DashboardVideoEnglishLink], [DashboardVideoSpanishLink], [KnowledgeBaseLink])
    VALUES
        ('https://youtu.be/aDLTQ7CvKf8?si=9SZ9dzFyPdoeYMnk',
         'https://youtu.be/YM2k7AhPb4U?si=fnUJly_SKyUqn-Lg',
         'https://youtu.be/j9ShHGhQtZ8?si=ptRROP4AqK4Jj4VC',
         'https://youtu.be/j9ShHGhQtZ8?si=ptRROP4AqK4Jj4VC',
         'https://help.digitalwallet.cards/');
END
