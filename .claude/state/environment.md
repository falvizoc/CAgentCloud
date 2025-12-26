# Entorno de Desarrollo

## Configuración Actual

- **Terminal**: WSL Ubuntu
- **Docker**: Ejecutar desde terminal WSL, no PowerShell/cmd
- **Paths**: Usar paths de Windows para archivos, pero comandos nativos de Ubuntu/WSL
- **Node.js local**: v16.14.0 (obsoleto, no usar)
- **Node.js en Docker**: v20+ (usar para builds)

## Comandos Docker

Desde WSL Ubuntu, usar:
```bash
docker compose -f /path/to/docker-compose.yml <command>
```

**NO usar**:
- `powershell.exe`
- `cmd.exe`
- Paths con `c:\`

## Notas

- El usuario ejecutará comandos Docker desde su terminal WSL Ubuntu
- Para builds del frontend, ejecutar dentro del contenedor Docker
