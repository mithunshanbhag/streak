PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS Checkins;

DROP TABLE IF EXISTS Habits;

CREATE TABLE IF NOT EXISTS Habits (
    Id INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Emoji TEXT NULL,
    Description TEXT NULL,
    CONSTRAINT PK_Habits PRIMARY KEY (Id),
    CONSTRAINT CK_Habits_Name_Length CHECK (length (trim(Name)) BETWEEN 1 AND 30),
    CONSTRAINT CK_Habits_Description_Length CHECK (Description IS NULL OR length(Description) <= 500),
    CONSTRAINT UQ_Habits_Name_CaseInsensitive UNIQUE (Name COLLATE NOCASE)
) STRICT;

CREATE TABLE IF NOT EXISTS Checkins (
    CheckinDate TEXT NOT NULL,
    HabitId INTEGER NOT NULL,
    Notes TEXT NULL,
    ProofImageUri TEXT NULL,
    ProofImageDisplayName TEXT NULL,
    ProofImageSizeBytes INTEGER NULL,
    ProofImageModifiedOn TEXT NULL,
    CONSTRAINT PK_Checkins PRIMARY KEY (HabitId, CheckinDate),
    CONSTRAINT FK_Checkins_Habits FOREIGN KEY (HabitId) REFERENCES Habits (Id) ON DELETE CASCADE ON UPDATE CASCADE,
    -- Because CheckinDate is stored as TEXT, we need to ensure it follows the 'YYYY-MM-DD' format and represents a valid date.
    -- Note: SQLite does not have a native DATE type, so we use a CHECK constraint to enforce the format and validity of the date string.
    CONSTRAINT CK_Checkins_CheckinDate CHECK (
        length (CheckinDate) = 10
        AND CheckinDate GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'
        AND strftime ('%Y-%m-%d', CheckinDate) IS NOT NULL
        AND strftime ('%Y-%m-%d', CheckinDate) = CheckinDate
    ),
    -- Keep this limit in sync with CoreConstants.CheckinNotesMaxLength.
    CONSTRAINT CK_Checkins_Notes_Length CHECK (Notes IS NULL OR length(Notes) <= 50)
) STRICT;

CREATE TABLE IF NOT EXISTS AutomatedBackupSettings (
    Id INTEGER NOT NULL,
    IsEnabled INTEGER NOT NULL DEFAULT 0,
    IsCloudEnabled INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT PK_AutomatedBackupSettings PRIMARY KEY (Id),
    CONSTRAINT CK_AutomatedBackupSettings_IsEnabled CHECK (IsEnabled IN (0, 1)),
    CONSTRAINT CK_AutomatedBackupSettings_IsCloudEnabled CHECK (IsCloudEnabled IN (0, 1))
) STRICT;

INSERT INTO AutomatedBackupSettings (Id, IsEnabled, IsCloudEnabled)
VALUES (1, 0, 0)
ON CONFLICT(Id) DO NOTHING;

CREATE TABLE IF NOT EXISTS ReminderSettings (
    Id INTEGER NOT NULL,
    IsEnabled INTEGER NOT NULL DEFAULT 0,
    TimeLocal TEXT NOT NULL DEFAULT '21:00:00',
    CONSTRAINT PK_ReminderSettings PRIMARY KEY (Id),
    CONSTRAINT CK_ReminderSettings_IsEnabled CHECK (IsEnabled IN (0, 1)),
    CONSTRAINT CK_ReminderSettings_TimeLocal CHECK (
        length(TimeLocal) = 8
        AND TimeLocal GLOB '[0-2][0-9]:[0-5][0-9]:[0-5][0-9]'
        AND time(TimeLocal) IS NOT NULL
        AND time(TimeLocal) = TimeLocal
    )
) STRICT;

INSERT INTO ReminderSettings (Id, IsEnabled, TimeLocal)
VALUES (1, 0, '21:00:00')
ON CONFLICT(Id) DO NOTHING;
