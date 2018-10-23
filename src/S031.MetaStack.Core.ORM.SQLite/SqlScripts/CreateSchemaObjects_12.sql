PRAGMA foreign_keys = off;
-- Table: SysAreas
CREATE TABLE SysAreas (
    ID            INTEGER                      NOT NULL,
    SchemaName    NVARCHAR (60)                NOT NULL,
    SchemaOwner   NVARCHAR (30)                NOT NULL,
    SchemaVersion NVARCHAR (30)                NOT NULL,
    SchemaConfig  NTEXT,
    IsDefault     BIT                          NOT NULL,
    UpdateTime    [DATETIME CURRENT_TIMESTAMP] NOT NULL,
    DateBegin     DATETIME                     NOT NULL,
    DateEnd       DATETIME,
    CONSTRAINT PK_SYSAREAS PRIMARY KEY (
        ID
    )
);
-- Table: SysSchemas
CREATE TABLE SysSchemas (
    ID           INTEGER                      NOT NULL,
    UID          UNIQUEIDENTIFIER             NOT NULL,
    SysAreaID    INT                          NOT NULL,
    ObjectType   NVARCHAR (30)                NOT NULL,
    ObjectName   NVARCHAR (60)                NOT NULL,
    ObjectSchema NTEXT                        NOT NULL,
    DbObjectName NVARCHAR (60)                NOT NULL,
    UpdateTime   [DATETIME CURRENT_TIMESTAMP] NOT NULL,
    Version      NVARCHAR (30)                NOT NULL,
    SyncState    INT                          NOT NULL,
    DateBegin    DATETIME                     NOT NULL,
    DateEnd      DATETIME,
    PreviosID    INT,
    CONSTRAINT PK_SYSSCHEMAS PRIMARY KEY (
        ID
    ),
    FOREIGN KEY (
        SysAreaID
    )
    REFERENCES SysAreas (ID) ON DELETE NO ACTION
                             ON UPDATE NO ACTION,
    FOREIGN KEY (
        PreviosID
    )
    REFERENCES SysSchemas (ID) ON DELETE NO ACTION
                               ON UPDATE NO ACTION
);
-- Index: AK1_SysAreas
CREATE UNIQUE INDEX AK1_SysAreas ON SysAreas (
    SchemaName ASC
);
-- Index: IE1_SysSchemas
CREATE INDEX IE1_SysSchemas ON SysSchemas (
    SysAreaID ASC, ObjectName ASC
);
-- Index: IE2_SysSchemas
CREATE INDEX IE2_SysSchemas ON SysSchemas (
    SysAreaID ASC, DbObjectName ASC
);
-- Index: IE3_SysSchemas
CREATE INDEX IE3_SysSchemas ON SysSchemas (
    PreviosID ASC
);
-- View: V_SysSchemas
CREATE VIEW V_SysSchemas AS
    SELECT S.ID,
           S.UID,
           S.SysAreaID,
           A.SchemaName,
           A.SchemaOwner,
           A.SchemaVersion,
           A.IsDefault,
           S.ObjectType,
           S.ObjectName,
           S.ObjectSchema,
           S.DbObjectName,
           S.UpdateTime,
           S.Version,
           S.SyncState,
           S.DateBegin,
           S.DateEnd
      FROM SysAreas A
           INNER JOIN
           SysSchemas S ON A.ID = S.SysAreaID;
PRAGMA foreign_keys = on;
