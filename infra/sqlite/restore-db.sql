PRAGMA
foreign_keys = ON;

DROP TABLE IF EXISTS Checkins;
DROP TABLE IF EXISTS Habits;

CREATE TABLE IF NOT EXISTS Habits
(
    Id
    INTEGER
    NOT
    NULL,
    Name
    TEXT
    NOT
    NULL,
    Emoji
    TEXT
    NULL,
    CONSTRAINT
    PK_Habits
    PRIMARY
    KEY
(
    Id
),
    CONSTRAINT CK_Habits_Name_Length CHECK
(
    length (
    trim
(
    Name
)) BETWEEN 1 AND 30),
    CONSTRAINT UQ_Habits_Name_CaseInsensitive UNIQUE
(
    Name
    COLLATE
    NOCASE
)
    ) STRICT;

CREATE TABLE IF NOT EXISTS Checkins
(
    CheckinDate
    TEXT
    NOT
    NULL,
    HabitId
    INTEGER
    NOT
    NULL,
    IsDone
    INTEGER
    NOT
    NULL
    DEFAULT
    0,
    LastUpdatedUtc
    TEXT
    NULL
    DEFAULT
    CURRENT_TIMESTAMP,
    CONSTRAINT
    PK_Checkins
    PRIMARY
    KEY
(
    HabitId,
    CheckinDate
),
    CONSTRAINT FK_Checkins_Habits FOREIGN KEY
(
    HabitId
) REFERENCES Habits
(
    Id
)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
    CONSTRAINT CK_Checkins_IsDone CHECK
(
    IsDone
    IN
(
    0,
    1
)),
    -- Because CheckinDate is stored as TEXT, we need to ensure it follows the 'YYYY-MM-DD' format and represents a valid date.
    -- Note: SQLite does not have a native DATE type, so we use a CHECK constraint to enforce the format and validity of the date string.
    CONSTRAINT CK_Checkins_CheckinDate CHECK
(
    length
(
    CheckinDate
) = 10
    AND CheckinDate GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'
    AND strftime
(
    '%Y-%m-%d',
    CheckinDate
) IS NOT NULL
    AND strftime
(
    '%Y-%m-%d',
    CheckinDate
) = CheckinDate)
    ) STRICT;
