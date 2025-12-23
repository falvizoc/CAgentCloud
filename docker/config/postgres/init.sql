-- =============================================================================
-- CobranzaCloud - PostgreSQL Initialization Script
-- =============================================================================
-- Este script se ejecuta automáticamente al crear el container por primera vez
-- =============================================================================

-- Crear extensiones útiles
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";  -- Para búsquedas de texto

-- Configuraciones de timezone
SET timezone = 'America/Mexico_City';

-- Mensaje de confirmación
DO $$
BEGIN
    RAISE NOTICE 'CobranzaCloud database initialized successfully';
END $$;
