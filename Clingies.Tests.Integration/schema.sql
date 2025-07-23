CREATE TABLE IF NOT EXISTS Clingies (
    id TEXT PRIMARY KEY,
    title TEXT,
    content TEXT,
    is_deleted BOOLEAN,
    is_pinned BOOLEAN,
    position_X REAL,
    position_y REAL,
    width REAL,
    height REAL,
    created_at TEXT,
    modified_at TEXT,
    is_rolled BOOLEAN,
    is_locked BOOLEAN
);
