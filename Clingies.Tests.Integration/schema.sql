CREATE TABLE IF NOT EXISTS Clingies (
    Id TEXT PRIMARY KEY,
    Title TEXT,
    Content TEXT,
    IsDeleted BOOLEAN,
    IsPinned BOOLEAN,
    PositionX REAL,
    PositionY REAL,
    Width REAL,
    Height REAL,
    CreatedAt TEXT,
    ModifiedAt TEXT,
    IsRolled BOOLEAN,
    IsLocked BOOLEAN
);
