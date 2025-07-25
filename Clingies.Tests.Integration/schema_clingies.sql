CREATE TABLE IF NOT EXISTS clingies (
    [id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [title] TEXT,
    [content] TEXT,
    [position_x] REAL,
    [position_y] REAL,
    [width] REAL,
    [height] REAL,
    [style] TEXT,
    [is_pinned] BOOLEAN,
    [is_locked] BOOLEAN,
    [is_rolled] BOOLEAN,
    [is_deleted] BOOLEAN,
    [created_at] TEXT,
    [modified_at] TEXT
);
