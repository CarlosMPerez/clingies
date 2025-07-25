CREATE TABLE IF NOT EXISTS system_menu (
    [id] TEXT PRIMARY KEY,
    [menu_type] TEXT,
    [parent_id] TEXT,
    [label] TEXT,
    [tooltip] TEXT,
    [enabled] BOOLEAN,
    [separator] BOOLEAN,
    [sort_order] INTEGER
);