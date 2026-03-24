PRAGMA
foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Habits
(
    Id
    TEXT
    NOT
    NULL
    PRIMARY
    KEY,
    Name
    TEXT
    NOT
    NULL
    COLLATE
    NOCASE,
    Emoji
    TEXT
    NULL,
    SortOrder
    INTEGER
    NOT
    NULL
    DEFAULT
    0,
    CreatedAtUtc
    TEXT
    NOT
    NULL
    DEFAULT
    CURRENT_TIMESTAMP,
    UpdatedAtUtc
    TEXT
    NULL,
    CONSTRAINT
    CK_Habits_Name_Length
    CHECK (
    length
(
    Name
) BETWEEN 1 AND 30),
    CONSTRAINT UQ_Habits_Name UNIQUE
(
    Name
)
    );

CREATE TABLE IF NOT EXISTS Checkins
(
    Id
    TEXT
    NOT
    NULL
    PRIMARY
    KEY,
    HabitId
    TEXT
    NOT
    NULL,
    CheckinDate
    TEXT
    NOT
    NULL,
    CreatedAtUtc
    TEXT
    NOT
    NULL
    DEFAULT
    CURRENT_TIMESTAMP,
    UpdatedAtUtc
    TEXT
    NULL,
    CONSTRAINT UQ_Checkins_HabitId_CheckinDate UNIQUE
(
    HabitId,
    CheckinDate
),
    CONSTRAINT FK_Checkins_Habits_HabitId FOREIGN KEY
(
    HabitId
)
    REFERENCES Habits
(
    Id
)
    ON DELETE CASCADE
    );

CREATE TABLE IF NOT EXISTS AppSettings
(
    Id
    INTEGER
    NOT
    NULL
    PRIMARY
    KEY,
    IsReminderEnabled
    INTEGER
    NOT
    NULL
    DEFAULT
    1,
    ReminderTimeLocal
    TEXT
    NOT
    NULL
    DEFAULT
    '21:00:00',
    UpdatedAtUtc
    TEXT
    NOT
    NULL
    DEFAULT
    CURRENT_TIMESTAMP,
    CONSTRAINT
    CK_AppSettings_Id
    CHECK
(
    Id =
    1
),
    CONSTRAINT CK_AppSettings_IsReminderEnabled CHECK
(
    IsReminderEnabled
    IN
(
    0,
    1
))
    );

INSERT
OR IGNORE INTO AppSettings (Id, IsReminderEnabled, ReminderTimeLocal)
VALUES (1, 1, '21:00:00');
