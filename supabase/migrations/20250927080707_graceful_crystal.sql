-- Enable PostGIS extension
CREATE EXTENSION IF NOT EXISTS postgis;

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email TEXT UNIQUE NOT NULL,
    username TEXT UNIQUE NOT NULL,
    profile_picture_url TEXT,
    bio TEXT,
    is_private BOOLEAN DEFAULT FALSE,
    keycloak_subject TEXT UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_keycloak_subject ON users(keycloak_subject);

-- Follows table
CREATE TABLE follows (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    follower_id UUID REFERENCES users(id) ON DELETE CASCADE,
    followee_id UUID REFERENCES users(id) ON DELETE CASCADE,
    status TEXT NOT NULL DEFAULT 'accepted',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE UNIQUE INDEX uq_follows_pair ON follows(follower_id, followee_id) WHERE deleted_at IS NULL;

-- Vehicles table
CREATE TABLE vehicles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_id UUID REFERENCES users(id) ON DELETE CASCADE,
    name TEXT,
    type TEXT DEFAULT 'car',
    design_meta JSONB,
    model_3d_url TEXT,
    max_speed DOUBLE PRECISION DEFAULT 200.0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

-- Achievements table
CREATE TABLE achievements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key TEXT UNIQUE NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    icon_url TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE user_achievements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    achievement_id UUID REFERENCES achievements(id) ON DELETE CASCADE,
    awarded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    progress JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE UNIQUE INDEX uq_user_achievements ON user_achievements(user_id, achievement_id) WHERE deleted_at IS NULL;

-- Posts table
CREATE TABLE posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    author_id UUID REFERENCES users(id) ON DELETE CASCADE,
    text TEXT,
    visibility TEXT DEFAULT 'public',
    likes_count INTEGER DEFAULT 0,
    comments_count INTEGER DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_posts_author_created ON posts(author_id, created_at DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_posts_created ON posts(created_at DESC) WHERE deleted_at IS NULL AND visibility = 'public';

CREATE TABLE post_media (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID REFERENCES posts(id) ON DELETE CASCADE,
    media_url TEXT NOT NULL,
    media_type TEXT DEFAULT 'image',
    order_index INTEGER DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE post_likes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID REFERENCES posts(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE UNIQUE INDEX uq_post_likes ON post_likes(post_id, user_id) WHERE deleted_at IS NULL;

CREATE TABLE post_comments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID REFERENCES posts(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    text TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

-- Stories table
CREATE TABLE stories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    author_id UUID REFERENCES users(id) ON DELETE CASCADE,
    media_url TEXT NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_stories_expires ON stories(expires_at);
CREATE INDEX idx_stories_author ON stories(author_id, created_at DESC) WHERE deleted_at IS NULL;

-- Events table
CREATE TABLE events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    host_id UUID REFERENCES users(id) ON DELETE CASCADE,
    title TEXT NOT NULL,
    description TEXT,
    location GEOGRAPHY(POINT, 4326),
    start_at TIMESTAMPTZ,
    end_at TIMESTAMPTZ,
    visibility TEXT DEFAULT 'public',
    join_policy TEXT DEFAULT 'auto',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_events_location ON events USING GIST(location) WHERE deleted_at IS NULL;
CREATE INDEX idx_events_start_at ON events(start_at) WHERE deleted_at IS NULL;

CREATE TABLE event_contributors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id UUID REFERENCES events(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    role TEXT DEFAULT 'contributor',
    granted_by UUID REFERENCES users(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

-- Races table
CREATE TABLE races (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    host_id UUID REFERENCES users(id) ON DELETE CASCADE,
    name TEXT,
    start_point GEOGRAPHY(POINT, 4326) NOT NULL,
    end_point GEOGRAPHY(POINT, 4326) NOT NULL,
    route GEOMETRY(LINESTRING, 4326),
    status TEXT DEFAULT 'created',
    scheduled_at TIMESTAMPTZ,
    started_at TIMESTAMPTZ,
    finished_at TIMESTAMPTZ,
    event_id UUID REFERENCES events(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_races_start_point ON races USING GIST(start_point) WHERE deleted_at IS NULL;
CREATE INDEX idx_races_route ON races USING GIST(route) WHERE deleted_at IS NULL;
CREATE INDEX idx_races_status ON races(status) WHERE deleted_at IS NULL;

CREATE TABLE race_racers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    race_id UUID REFERENCES races(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    start_order INTEGER NOT NULL,
    vehicle_id UUID REFERENCES vehicles(id),
    status TEXT DEFAULT 'invited',
    joined_at TIMESTAMPTZ,
    finish_time INTERVAL,
    final_distance DOUBLE PRECISION,
    final_rank INTEGER,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE UNIQUE INDEX uq_race_racers ON race_racers(race_id, user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_race_racers_race ON race_racers(race_id) WHERE deleted_at IS NULL;

-- Racer location updates (append-only, no soft delete)
CREATE TABLE racer_location_updates (
    id BIGSERIAL PRIMARY KEY,
    race_id UUID REFERENCES races(id) ON DELETE CASCADE,
    racer_user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    speed DOUBLE PRECISION,
    heading DOUBLE PRECISION,
    recorded_at TIMESTAMPTZ NOT NULL,
    is_suspicious BOOLEAN DEFAULT FALSE,
    suspicious_reason TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_racer_location_race_time ON racer_location_updates(race_id, recorded_at DESC);
CREATE INDEX idx_racer_location_user_time ON racer_location_updates(racer_user_id, recorded_at DESC);