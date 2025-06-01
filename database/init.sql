CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE todo_lists (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE todo_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    list_id UUID REFERENCES todo_lists(id) ON DELETE CASCADE,
    item_name VARCHAR(255) NOT NULL,
    content TEXT,
    media_url VARCHAR(255),
    media_type INT CHECK (media_type IN (0, 1)),
    due_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    item_status INT CHECK (item_status IN (0, 1, 2)),
    item_priority INT CHECK (item_status IN (0, 1, 2, 3)),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE todo_list_shares (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    list_id UUID REFERENCES todo_lists(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    permission INT CHECK (permission IN (0, 1)),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
