/**
 * API Types - Contratos compartidos Frontend/Backend
 *
 * Este archivo define los tipos que deben ser consistentes entre
 * el frontend (TypeScript) y el backend (.NET).
 *
 * @version 1.0
 * @date 2025-12-23
 */

// ============================================================
// COMMON TYPES
// ============================================================

/** UUID string format */
export type UUID = string;

/** ISO 8601 date string */
export type ISODate = string;

/** Decimal number with 2 decimal places */
export type Money = number;

/** Paginated response wrapper */
export interface PagedResult<T> {
  data: T[];
  meta: PaginationMeta;
}

export interface PaginationMeta {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
}

/** API Error response */
export interface ApiError {
  error: {
    code: string;
    message: string;
    details?: FieldError[];
    requestId: string;
  };
}

export interface FieldError {
  field: string;
  message: string;
}

/** Problem Details (RFC 7807) */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
}

// ============================================================
// AUTH
// ============================================================

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nombre: string;
  organizacion: {
    nombre: string;
    rfc?: string;
  };
}

export interface OAuthRequest {
  code: string;
  redirectUri: string;
}

export interface TokenPair {
  accessToken: string;
  refreshToken: string;
  expiresIn: number; // seconds
}

export interface AuthResponse {
  user: User;
  organization: OrganizationSummary;
  tokens: TokenPair;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

// ============================================================
// USER
// ============================================================

export interface User {
  id: UUID;
  email: string;
  nombre: string;
  role: UserRole;
  organizationId: UUID;
  permissions?: string[];
  createdAt?: ISODate;
  lastLogin?: ISODate;
}

export type UserRole = 'owner' | 'admin' | 'manager' | 'collector' | 'viewer';

export interface UserInviteRequest {
  email: string;
  nombre: string;
  role: UserRole;
}

export interface UserInviteResponse {
  id: UUID;
  email: string;
  status: 'invited';
  invitedBy: string;
  expiresAt: ISODate;
}

// ============================================================
// ORGANIZATION
// ============================================================

export interface Organization {
  id: UUID;
  nombre: string;
  rfc?: string;
  plan: OrganizationPlan;
  createdAt: ISODate;
  settings: OrganizationSettings;
  stats: OrganizationStats;
}

export interface OrganizationSummary {
  id: UUID;
  nombre: string;
  plan?: OrganizationPlan;
}

export type OrganizationPlan = 'free' | 'starter' | 'professional' | 'enterprise';

export interface OrganizationSettings {
  timezone: string;
  currency: string;
  emailDomain?: string;
  locale: string;
}

export interface OrganizationStats {
  usersCount: number;
  connectorsCount: number;
  lastSyncAt?: ISODate;
}

export interface UpdateOrganizationRequest {
  nombre?: string;
  rfc?: string;
  settings?: Partial<OrganizationSettings>;
}

// ============================================================
// CONNECTOR
// ============================================================

export interface Connector {
  id: UUID;
  nombre: string;
  tipo: ConnectorType;
  version: string;
  status: ConnectorStatus;
  lastHeartbeat?: ISODate;
  lastSync?: ISODate;
  empresas: ConnectorEmpresa[];
}

export type ConnectorType = 'aspel-sae' | 'contpaqi' | 'custom';
export type ConnectorStatus = 'online' | 'offline' | 'error' | 'pending';

export interface ConnectorEmpresa {
  id: string;
  nombre: string;
  baseDatos?: string;
}

export interface LinkCodeResponse {
  code: string;
  expiresAt: ISODate;
}

export interface RegisterConnectorRequest {
  linkCode: string;
  nombre: string;
  machineFingerprint: string;
  version: string;
  empresas: ConnectorEmpresa[];
}

export interface RegisterConnectorResponse {
  connectorId: UUID;
  tokens: TokenPair;
  syncConfig: SyncConfig;
}

export interface SyncConfig {
  intervalMinutes: number;
  endpoints: {
    sync: string;
    heartbeat: string;
  };
}

export interface HeartbeatRequest {
  status: 'healthy' | 'degraded' | 'unhealthy';
  uptime: number;
  memoryUsage: number;
  lastSyncStatus: 'success' | 'partial' | 'failed';
  empresasOnline: string[];
}

export interface HeartbeatResponse {
  ack: boolean;
  serverTime: ISODate;
  commands: ConnectorCommand[];
}

export interface ConnectorCommand {
  type: 'sync_now' | 'update_config' | 'restart';
  payload?: Record<string, unknown>;
}

// ============================================================
// SYNC
// ============================================================

export interface SyncCarteraRequest {
  empresaId: string;
  timestamp: ISODate;
  checksum: string;
  data: SyncCarteraData;
}

export interface SyncCarteraData {
  resumen: CarteraResumen;
  antiguedad: RangoAntiguedad[];
  clientes: SyncCliente[];
}

export interface SyncCliente {
  clave: string;
  nombre: string;
  rfc?: string;
  saldoTotal: Money;
  saldoVencido: Money;
  diasMaxVencido: number;
  facturas: SyncFactura[];
  contactos?: SyncContacto[];
}

export interface SyncFactura {
  folio: string;
  fecha: ISODate;
  vencimiento: ISODate;
  total: Money;
  saldo: Money;
  diasVencido: number;
}

export interface SyncContacto {
  nombre: string;
  email?: string;
  telefono?: string;
}

export interface SyncCarteraResponse {
  success: boolean;
  syncId: string;
  processedAt: ISODate;
  stats: SyncStats;
}

export interface SyncStats {
  clientesActualizados: number;
  facturasActualizadas: number;
  nuevos: number;
  modificados: number;
  sinCambios: number;
}

// ============================================================
// CARTERA
// ============================================================

export interface CarteraResumen {
  totalCartera: Money;
  carteraVigente: Money;
  carteraVencida: Money;
  porcentajeVencido: number;
  clientesConSaldo: number;
  facturasActivas: number;
  promedioAntiguedad?: number;
  variacion?: CarteraVariacion;
  lastSync?: ISODate;
}

export interface CarteraVariacion {
  cartera: number; // porcentaje
  vencida: number; // porcentaje
  periodo: string;
}

export interface RangoAntiguedad {
  rango: RangoAntiguedadType;
  label: string;
  monto: Money;
  facturas: number;
  porcentaje: number;
}

export type RangoAntiguedadType = 'vigente' | '1-30' | '31-60' | '61-90' | '90+';

// ============================================================
// CLIENTE
// ============================================================

export interface Cliente {
  id: UUID;
  clave: string;
  nombre: string;
  rfc?: string;
  saldoTotal: Money;
  saldoVencido: Money;
  diasMaxVencido: number;
  facturasActivas: number;
  ultimoPago?: ISODate;
}

export interface ClienteDetail extends Cliente {
  direccion?: Direccion;
  contactos: Contacto[];
  resumenCartera: ClienteResumenCartera;
  facturas: Factura[];
  historialCobranza: HistorialCobranza[];
}

export interface Direccion {
  calle?: string;
  colonia?: string;
  ciudad?: string;
  estado?: string;
  cp?: string;
  pais?: string;
}

export interface Contacto {
  id: UUID;
  nombre: string;
  email?: string;
  telefono?: string;
  principal: boolean;
}

export interface ClienteResumenCartera {
  saldoTotal: Money;
  saldoVencido: Money;
  saldoVigente: Money;
  facturasActivas: number;
  diasMaxVencido: number;
}

export interface GetClientesParams {
  page?: number;
  limit?: number;
  search?: string;
  conSaldo?: boolean;
  sort?: string;
}

export interface CreateClienteRequest {
  clave: string;
  nombre: string;
  rfc?: string;
  email?: string;
  telefono?: string;
}

export interface UpdateClienteRequest {
  nombre?: string;
  rfc?: string;
  direccion?: Partial<Direccion>;
}

// ============================================================
// FACTURA
// ============================================================

export interface Factura {
  id: UUID;
  folio: string;
  cliente?: ClienteSummary;
  fecha: ISODate;
  vencimiento: ISODate;
  total: Money;
  saldo: Money;
  diasVencido: number;
  rangoAntiguedad: RangoAntiguedadType;
  status: FacturaStatus;
}

export interface ClienteSummary {
  id: UUID;
  clave: string;
  nombre: string;
}

export type FacturaStatus = 'vigente' | 'vencida' | 'pagada' | 'cancelada';

export interface GetFacturasVencidasParams {
  page?: number;
  limit?: number;
  sort?: string;
  rangoAntiguedad?: RangoAntiguedadType;
  clienteId?: UUID;
}

// ============================================================
// COBRANZA
// ============================================================

export interface Plantilla {
  id: UUID;
  nombre: string;
  asunto: string;
  cuerpo: string;
  variables: string[];
  activa: boolean;
  usadaEn: number;
}

export interface CreatePlantillaRequest {
  nombre: string;
  asunto: string;
  cuerpo: string;
  activa?: boolean;
}

export interface UpdatePlantillaRequest {
  nombre?: string;
  asunto?: string;
  cuerpo?: string;
  activa?: boolean;
}

export interface EnviarRecordatorioRequest {
  clienteId: UUID;
  plantillaId: UUID;
  destinatarios: Destinatario[];
  facturasIncluir?: UUID[];
  programarPara?: ISODate;
  notas?: string;
}

export interface Destinatario {
  email: string;
  nombre?: string;
}

export interface EnviarRecordatorioResponse {
  id: UUID;
  status: RecordatorioStatus;
  scheduledFor?: ISODate;
  estimatedDelivery: ISODate;
}

export type RecordatorioStatus = 'queued' | 'sent' | 'delivered' | 'failed' | 'opened';

export interface HistorialCobranza {
  id: UUID;
  tipo: 'email' | 'sms' | 'whatsapp' | 'call';
  fecha: ISODate;
  asunto?: string;
  destinatario: string;
  status: RecordatorioStatus;
  abierto?: boolean;
  fechaApertura?: ISODate;
  enviadoPor?: UserSummary;
  plantilla?: string;
}

export interface UserSummary {
  id: UUID;
  nombre: string;
}

export interface GetHistorialParams {
  clienteId?: UUID;
  desde?: ISODate;
  hasta?: ISODate;
  status?: RecordatorioStatus;
  page?: number;
  limit?: number;
}

// ============================================================
// WEBHOOKS (Future)
// ============================================================

export interface Webhook {
  id: UUID;
  url: string;
  eventos: WebhookEvent[];
  activo: boolean;
  secret: string;
  createdAt: ISODate;
}

export type WebhookEvent =
  | 'sync.completado'
  | 'sync.fallido'
  | 'cliente.vencido'
  | 'factura.vencida'
  | 'recordatorio.enviado';

export interface CreateWebhookRequest {
  url: string;
  eventos: WebhookEvent[];
  secret: string;
}

// ============================================================
// SETTINGS
// ============================================================

export interface EmailSettings {
  metodo: 'oauth_google' | 'oauth_microsoft' | 'smtp' | 'transactional';
  smtpConfig?: SmtpConfig;
  firma?: string;
}

export interface SmtpConfig {
  host: string;
  port: number;
  username: string;
  password: string;
  useSsl: boolean;
}

export interface NotificationSettings {
  alertaFacturasVencer: boolean;
  diasAnticipacion: number;
  resumenDiario: boolean;
  resumenSemanal: boolean;
}

// ============================================================
// API CLIENT HELPER TYPES
// ============================================================

export interface ApiResponse<T> {
  data: T;
  status: number;
  ok: boolean;
}

export interface ApiClientConfig {
  baseUrl: string;
  getToken: () => string | null;
  onTokenRefresh?: (refreshToken: string) => Promise<string>;
  onUnauthorized?: () => void;
}

// ============================================================
// CARTERA ENDPOINTS RESPONSES (M3)
// ============================================================

export interface CarteraResumenResponse {
  totalCartera: Money;
  carteraVigente: Money;
  carteraVencida: Money;
  porcentajeVencido: number;
  clientesConSaldo: number;
  facturasActivas: number;
  ultimaSincronizacion?: ISODate;
}

export interface CarteraAntiguedadResponse {
  rangos: RangoAntiguedadItem[];
  total: Money;
}

export interface RangoAntiguedadItem {
  rango: string;
  label: string;
  monto: Money;
  facturas: number;
  porcentaje: number;
}

export interface ClienteListItem {
  id: UUID;
  clave: string;
  nombre: string;
  saldoTotal: Money;
  saldoVencido: Money;
  diasMaxVencido: number;
  facturasActivas: number;
}

export interface ClientesListResponse {
  items: ClienteListItem[];
  meta: ClientesPaginationMeta;
}

export interface ClientesPaginationMeta {
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

// ============================================================
// CLIENTE DETAIL RESPONSES (M3)
// ============================================================

export interface ClienteDetailResponse {
  id: UUID;
  clave: string;
  nombre: string;
  rfc?: string;
  email?: string;
  telefono?: string;
  direccion?: DireccionDto;
  saldoTotal: Money;
  saldoVencido: Money;
  diasMaxVencido: number;
  facturasActivas: number;
  ultimoPago?: ISODate;
  lastSyncAt?: ISODate;
  contactos: ContactoDto[];
  facturas: FacturaDto[];
}

export interface DireccionDto {
  calle?: string;
  colonia?: string;
  ciudad?: string;
  estado?: string;
  codigoPostal?: string;
}

export interface ContactoDto {
  id: UUID;
  nombre: string;
  email?: string;
  telefono?: string;
  principal: boolean;
}

export interface FacturaDto {
  id: UUID;
  folio: string;
  fecha: ISODate;
  vencimiento: ISODate;
  total: Money;
  saldo: Money;
  diasVencido: number;
  status: string;
}
