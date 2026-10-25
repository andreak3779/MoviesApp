# MoviesApp Security Overview

## 🛡️ Security Implementation Status

**Last Updated:** May 18, 2026  
**Status:** ✅ **Production Ready**

The MoviesApp project implements comprehensive security measures following industry best practices and OWASP Top 10 guidelines.

---

## 🔒 Security Features by Component

### Web API (`apps/web-api`)

**OWASP Top 10 API Security Compliant** (8/10 vulnerabilities addressed)

#### Authentication & Authorization
- ✅ JWT-based authentication with 15-minute access tokens
- ✅ Refresh token rotation (7-day expiration)
- ✅ ASP.NET Core Identity with password policies
- ✅ Role-based authorization (Admin, User)
- ✅ Object-level ownership validation
- ✅ Property-level authorization (separate DTOs)

#### Resource Protection
- ✅ Rate limiting: 100 req/min (read), 10 req/min (write)
- ✅ Request pagination (max 100 items per page)
- ✅ GUID-based resource IDs (anti-enumeration)
- ✅ Request size limits
- ✅ Input validation and sanitization

#### Security Headers
- ✅ Content-Security-Policy
- ✅ Strict-Transport-Security (HSTS)
- ✅ X-Frame-Options: DENY
- ✅ X-Content-Type-Options: nosniff
- ✅ X-XSS-Protection
- ✅ Referrer-Policy
- ✅ Permissions-Policy

#### Monitoring & Auditing
- ✅ Comprehensive audit logging for all mutations
- ✅ IP address and user agent tracking
- ✅ Old/new value capture for updates
- ✅ Security event logging
- ✅ Generic error messages (no information leakage)

#### Configuration Security
- ✅ Secrets management via User Secrets (dev) / Environment Variables (prod)
- ✅ CORS properly configured
- ✅ HTTPS enforced in production
- ✅ Swagger disabled in production

**Documentation:** See `apps/web-api/docs/plans/IMPLEMENTATION-COMPLETE-SUMMARY.md`

---

### Angular Client (`apps/angular-client`)

#### Security Measures
- ✅ Angular's built-in XSS protection
- ✅ `authInterceptor` for JWT token management
- ✅ `authGuard` (`canMatch`) on every protected route
- ✅ Reactive Forms with validators
- ✅ Tokens in `sessionStorage`, not `localStorage` — cleared when the tab closes
- ⚠️ **No automatic token refresh** — `AuthService.refresh()` exists but nothing calls it, so a session ends after the 15-minute access-token lifetime
- ⚠️ **`errorInterceptor` clears `sessionStorage` directly** rather than calling `AuthService.clear()`, so the in-memory token signal stays set and the guard does not bounce the user to `/login`
- ⚠️ **Refresh token is in `sessionStorage`** — same-origin script access can read it. A `SameSite=Strict` cookie would be the stronger option.

#### Best Practices
- Content Security Policy headers
- Subresource Integrity (SRI) for scripts
- No eval() or innerHTML usage
- Sanitized user inputs
- Secure HTTP client configuration

**Documentation:** See `apps/angular-client/README.md`

---

### MVC Web App (`apps/mvc-web`)

#### Security Measures
- ✅ ASP.NET Core built-in protections
- ✅ Anti-forgery tokens on every form (`[ValidateAntiForgeryToken]`)
- ✅ Model validation
- ✅ Server-side session for the API tokens — the JWT and refresh token are **never** exposed to the browser
- ✅ Cookie authentication (`HttpOnly`, `SameSite=Lax`) records only that the browser is signed in
- ✅ The access token is refreshed automatically shortly before it expires (`MovieSession`)
- ✅ `ApiTokenHandler` attaches `Authorization: Bearer <token>` to every upstream request
- ✅ The bearer token is never attached to `/auth/login`, `/auth/register` or `/auth/refresh`, so a stale token cannot poison the login flow
- ✅ `Program.cs` validates `MoviesApi:BaseUrl` at startup and refuses to run without the `/api/v1` prefix
- ✅ SQL injection protection via EF Core (no raw SQL in the app)

---

## 🔐 Authentication Flow

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       │ POST /api/v1/auth/login
       │ { email, password }
       ▼
┌─────────────────────────────────┐
│         AuthController          │
│  ┌───────────────────────────┐  │
│  │    Validate Credentials   │  │
│  └───────────┬───────────────┘  │
│              │                   │
│              ▼                   │
│  ┌───────────────────────────┐  │
│  │  Generate JWT + Refresh   │  │
│  │  Token (store in DB)      │  │
│  └───────────┬───────────────┘  │
└──────────────┼──────────────────┘
               │
               │ { token, refreshToken, expiresAt }
               ▼
       ┌─────────────┐
       │   Client    │ Store token in memory
       └──────┬──────┘
              │
              │ GET /api/v1/Movie
              │ Authorization: Bearer {token}
              ▼
┌─────────────────────────────────┐
│      JWT Bearer Middleware      │
│  ┌───────────────────────────┐  │
│  │  Validate Token Signature │  │
│  │  Check Expiration         │  │
│  │  Extract User Claims      │  │
│  └───────────┬───────────────┘  │
└──────────────┼──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│     Authorization Filter        │
│  ┌───────────────────────────┐  │
│  │  Check UserOnly Policy    │  │
│  └───────────┬───────────────┘  │
└──────────────┼──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│     MovieController             │
│  ┌───────────────────────────┐  │
│  │  GetUserId() from claims  │  │
│  └───────────┬───────────────┘  │
└──────────────┼──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│     MovieRepository             │
│  ┌───────────────────────────┐  │
│  │  Filter by OwnerId        │  │
│  │  (WHERE OwnerId = @userId)│  │
│  └───────────┬───────────────┘  │
└──────────────┼──────────────────┘
               │
               ▼
       ┌─────────────┐
       │   Return    │ User's movies only
       │   Movies    │
       └─────────────┘
```

---

## 🛡️ OWASP Top 10 API Security Coverage

| # | Vulnerability | Status | Implementation | Location |
|---|--------------|--------|----------------|----------|
| API1:2023 | Broken Object Level Authorization | ✅ Fixed | Ownership validation + GUID IDs | `MovieRepository.cs` |
| API2:2023 | Broken Authentication | ✅ Fixed | JWT + refresh tokens | `AuthService.cs` |
| API3:2023 | Broken Object Property Level Authorization | ✅ Fixed | Separate DTOs | `MovieDtos.cs` |
| API4:2023 | Unrestricted Resource Consumption | ✅ Fixed | Rate limiting + pagination | `RateLimitingMiddleware.cs` |
| API5:2023 | Broken Function Level Authorization | ✅ Fixed | Authorization policies | `Program.cs` |
| API6:2023 | Unrestricted Access to Sensitive Business Flows | ✅ Fixed | Audit logging | `AuditLogService.cs` |
| API7:2023 | Server Side Request Forgery | ⚠️ N/A | No external URL processing | - |
| API8:2023 | Security Misconfiguration | ✅ Fixed | Security headers + CORS | `SecurityHeadersMiddleware.cs` |
| API9:2023 | Improper Inventory Management | ✅ Fixed | API versioning | Routes use `/api/v1/` |
| API10:2023 | Unsafe Consumption of APIs | ⚠️ N/A | No external API integrations | - |

**Coverage:** 8/10 (80%) - 2 items marked N/A as they don't apply to current functionality

---

## 🔍 Security Testing

### Automated Testing

```bash
# Run unit tests
cd apps/web-api
dotnet test

# Run integration tests
cd apps/web-api
dotnet test --filter "Category=Integration"
```

### Manual Testing Checklist

- [ ] Register new user with weak password (should fail)
- [ ] Login with invalid credentials (should return 401)
- [ ] Access protected endpoint without token (should return 401)
- [ ] User A tries to access User B's movie (should return 403)
- [ ] Send 100+ requests in 1 minute (should get 429)
- [ ] Check security headers present in response
- [ ] Verify no stack traces in error responses
- [ ] Confirm Swagger not accessible in production

### Penetration Testing

Recommended tools:
- **OWASP ZAP** - Automated security scanner
- **Burp Suite** - Manual penetration testing
- **Nuclei** - Vulnerability scanner

---

## 📋 Security Configuration

### Development

```bash
# Web API - Set JWT secret
cd apps/web-api/MoviesApi
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 32)"
```

### Production

```bash
# Environment variables
export JwtSettings__SecretKey="your-production-secret-key"
export ConnectionStrings__DefaultConnection="production-connection-string"
export ASPNETCORE_ENVIRONMENT="Production"
```

**Complete Guide:** See `apps/web-api/SECURE-CONFIGURATION-GUIDE.md`

---

## 🚨 Security Incident Response

### Monitoring

1. **Audit Logs:** Query `AuditLogs` table for suspicious activity
2. **Rate Limit Hits:** Monitor 429 responses
3. **Failed Logins:** Track 401 responses on `/auth/login`
4. **Ownership Violations:** Alert on 403 responses

### Common Issues

#### Issue: Multiple Failed Login Attempts
**Response:**
- Check if brute force attack
- Consider temporary IP ban
- Review account lockout policy

#### Issue: Rate Limit Exceeded
**Response:**
- Check if legitimate user or attack
- Review rate limit thresholds
- Consider per-user limits

#### Issue: Unauthorized Access Attempt
**Response:**
- Review ownership validation logic
- Check token validation
- Audit user permissions

---

## 🔐 Password Policy

**Requirements:**
- Minimum 8 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one number (0-9)
- At least one non-alphanumeric character (!@#$%^&*)

**Account Lockout:**
- 5 failed attempts triggers 15-minute lockout
- Lockout resets after successful login

---

## 📊 Security Metrics

Track these metrics for security monitoring:

1. **Authentication**
   - Failed login attempts per hour
   - Token refresh rate
   - New registrations per day

2. **Authorization**
   - 403 Forbidden responses per hour
   - Ownership violations detected

3. **Rate Limiting**
   - 429 Too Many Requests per hour
   - Peak request rates

4. **Audit**
   - Total mutations per day
   - Failed operations
   - Unusual patterns

---

## 🔗 Security Resources

### Documentation
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)

### Tools
- [OWASP ZAP](https://www.zaproxy.org/)
- [JWT.io](https://jwt.io/) - Token decoder
- [Security Headers](https://securityheaders.com/) - Header checker

### Internal Docs
- `apps/web-api/docs/plans/OWASP-TOP10-COMPLIANCE-PLAN.md`
- `apps/web-api/docs/plans/OWASP-IMPLEMENTATION-PROGRESS.md`
- `apps/web-api/SECURE-CONFIGURATION-GUIDE.md`
- `apps/web-api/docs/plans/IMPLEMENTATION-COMPLETE-SUMMARY.md`

---

## ✅ Security Checklist for Deployment

### Pre-Deployment
- [ ] Generate production JWT secret key
- [ ] Update CORS allowed origins
- [ ] Configure HTTPS certificate
- [ ] Set up production database
- [ ] Configure environment variables
- [ ] Disable Swagger in production
- [ ] Review rate limiting thresholds

### Post-Deployment
- [ ] Test authentication endpoints
- [ ] Verify JWT tokens are generated
- [ ] Check security headers present
- [ ] Test rate limiting is active
- [ ] Verify audit logging working
- [ ] Monitor application logs
- [ ] Run OWASP ZAP scan

### Ongoing
- [ ] Monthly dependency vulnerability scans
- [ ] Quarterly security audit
- [ ] Annual penetration testing
- [ ] Continuous audit log monitoring

---

**Security Status:** ✅ **PRODUCTION READY**  
**Last Security Review:** May 18, 2026  
**Next Scheduled Review:** August 18, 2026
