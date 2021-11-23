CREATE TABLE [AppDbo].[NavData] (
    [NavDataId]      BIGINT         IDENTITY (1, 1) NOT NULL,
    [Key]            NVARCHAR (50)  NULL,
    [GroupId]        NVARCHAR (50)  NULL,
    [SourceType]     NVARCHAR (50)  NULL,
    [Timestamp]      NVARCHAR (50)  NULL,
    [Quality]        NVARCHAR (50)  NULL,
    [Chksum]         NVARCHAR (50)  NULL,
    [AisMessage]     NVARCHAR (50)  NOT NULL,
    [AisMessageType] NVARCHAR (50)  NOT NULL,
    [AisMessageJson] NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_NavData] PRIMARY KEY CLUSTERED ([NavDataId] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_NavData_AisMessageType]
    ON [AppDbo].[NavData]([AisMessageType] ASC);

