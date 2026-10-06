-- FarmMonitoring schema (SPEC section 3.2).
-- Executed once by the Postgres container on first start (/docker-entrypoint-initdb.d).
-- Idempotent: safe to run again against an existing database.

CREATE TABLE IF NOT EXISTS weather_readings (
    id                        uuid             PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id                  uuid             NOT NULL,
    client_id                 text             NOT NULL,
    property_id               uuid             NOT NULL,
    property_name             text             NOT NULL,
    latitude                  double precision NOT NULL,
    longitude                 double precision NOT NULL,
    observed_at               timestamptz      NOT NULL,
    collected_at              timestamptz      NOT NULL,
    temperature_celsius       double precision NOT NULL,
    relative_humidity_percent double precision NOT NULL,
    precipitation_mm          double precision NOT NULL,
    soil_moisture             double precision NOT NULL,
    created_at                timestamptz      NOT NULL DEFAULT now(),
    CONSTRAINT uq_weather_readings_event_id UNIQUE (event_id)
);

CREATE INDEX IF NOT EXISTS ix_weather_readings_property_id_observed_at
    ON weather_readings (property_id, observed_at DESC);

CREATE TABLE IF NOT EXISTS alert_rules (
    id          uuid             PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id uuid             NOT NULL,
    variable    text             NOT NULL,
    operator    text             NOT NULL,
    threshold   double precision NOT NULL,
    severity    text             NOT NULL,
    is_active   boolean          NOT NULL DEFAULT true,
    created_at  timestamptz      NOT NULL DEFAULT now(),
    updated_at  timestamptz      NOT NULL DEFAULT now(),
    CONSTRAINT ck_alert_rules_variable CHECK (variable IN ('temperature', 'humidity', 'precipitation', 'soil_moisture')),
    CONSTRAINT ck_alert_rules_operator CHECK (operator IN ('>', '<', '>=', '<=')),
    CONSTRAINT ck_alert_rules_severity CHECK (severity IN ('low', 'medium', 'high', 'critical'))
);

CREATE INDEX IF NOT EXISTS ix_alert_rules_property_id_active
    ON alert_rules (property_id) WHERE is_active;

CREATE TABLE IF NOT EXISTS alerts (
    id              uuid             PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id        uuid             NOT NULL,
    source_event_id uuid             NOT NULL,
    rule_id         uuid             NULL REFERENCES alert_rules (id) ON DELETE SET NULL,
    property_id     uuid             NOT NULL,
    property_name   text             NOT NULL,
    variable        text             NOT NULL,
    operator        text             NOT NULL,
    threshold       double precision NOT NULL,
    measured_value  double precision NOT NULL,
    severity        text             NOT NULL,
    observed_at     timestamptz      NOT NULL,
    created_at      timestamptz      NOT NULL DEFAULT now(),
    CONSTRAINT uq_alerts_event_id UNIQUE (event_id),
    CONSTRAINT uq_alerts_source_event_id_rule_id UNIQUE (source_event_id, rule_id)
);

CREATE TABLE IF NOT EXISTS notifications (
    id             uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    alert_id       uuid        NOT NULL REFERENCES alerts (id),
    alert_event_id uuid        NOT NULL,
    property_id    uuid        NOT NULL,
    severity       text        NOT NULL,
    title          text        NOT NULL,
    message        text        NOT NULL,
    created_at     timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_notifications_alert_event_id UNIQUE (alert_event_id)
);

CREATE TABLE IF NOT EXISTS processed_events (
    event_id     uuid        NOT NULL,
    consumer     text        NOT NULL,
    processed_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_processed_events PRIMARY KEY (event_id, consumer)
);

-- Seed: default rules for every property (SPEC section 4).
-- The temperature > -50 rule is a disabled demo rule; activate it via the API to trigger alerts on demand.
INSERT INTO alert_rules (property_id, variable, operator, threshold, severity, is_active)
SELECT p.property_id, r.variable, r.operator, r.threshold, r.severity, r.is_active
FROM (VALUES
    ('11111111-1111-1111-1111-111111111111'::uuid),
    ('22222222-2222-2222-2222-222222222222'::uuid),
    ('33333333-3333-3333-3333-333333333333'::uuid),
    ('44444444-4444-4444-4444-444444444444'::uuid),
    ('55555555-5555-5555-5555-555555555555'::uuid)
) AS p (property_id)
CROSS JOIN (VALUES
    ('temperature',   '>', 30::double precision,   'high',     true),
    ('humidity',      '<', 40::double precision,   'medium',   true),
    ('precipitation', '>', 5::double precision,    'high',     true),
    ('soil_moisture', '<', 0.15::double precision, 'critical', true),
    ('temperature',   '>', -50::double precision,  'low',      false)
) AS r (variable, operator, threshold, severity, is_active)
WHERE NOT EXISTS (
    SELECT 1
    FROM alert_rules existing
    WHERE existing.property_id = p.property_id
      AND existing.variable = r.variable
      AND existing.operator = r.operator
      AND existing.threshold = r.threshold
);
