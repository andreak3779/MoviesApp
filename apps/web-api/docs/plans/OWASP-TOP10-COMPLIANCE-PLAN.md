# OWASP Top 10 API Security Compliance Plan

## Project Overview
**Application:** MoviesApi  
**Framework:** .NET 10.0 Web API  
**Database:** SQLite with Entity Framework Core  
**Current Status:** No security measures implemented

---

## Executive Summary

This plan addresses all OWASP Top 10 API Security vulnerabilities (2023 edition) for the MoviesApi project. The API currently lacks authentication, authorization, input validation, rate limiting, and security headers. Implementation is organized into **4 phases** with **15 work items** estimated at **3-4 days** total effort.

---

## Current Security Gaps

### Critical Issues
- ❌ No authentication or authorization
- ❌ No input validation beyond basic model binding
- ❌ No rate limiting or throttling
- ❌ No security headers (CORS, CSP, HSTS, etc.)
- ❌ No audit logging or security monitoring
- ❌ Swagger exposed without protection
- ❌ No API versioning strategy

### Medium Issues
- ❌ No request size limits
- ❌ No structured error handling
- ❌ Database connection string not encrypted
- ❌ No dependency vulnerability scanning
- ❌ No security testing in CI/CD

---

## OWASP Top 10 2023 Compliance Plan

### **API1:2023 - Broken Object Level Authorization (BOLA)**

**Risk:** Users can access/modify resources they shouldn't have access to by manipulating IDs (e.g., `/api/movie/123` → `/api/movie/124`)

**Current State:** No authorization checks on any endpoint

**Remediation:**
1. Implement resource-level authorization checks in all controller methods
2. Add ownership validation before CRUD operations
3. Use GUIDs instead of sequential integers for resource IDs
4. Implement authorization policies for different user roles

**Implementation Tasks:**
- [ ] **Task 1.1:** Add authorization attributes to all controller endpoints
  - File: `controllers/MovieController.cs`, `FilmGenresController.cs`, `MediaTypesController.cs`
  - Add `[Authorize]` attribute to all actions
  - Estimated: 2 hours

- [ ] **Task 1.2:** Implement ownership checks in repository layer
  - File: `Repositories/MovieRepository.cs`
  - Add `userId` parameter to all methods
  - Verify user owns resource before operations
  - Estimated: 3 hours

- [ ] **Task 1.3:** Migrate from integer IDs to GUIDs
  - Files: `Models/Movie.cs`, `DTOs/MovieDtos.cs`, all controllers
  - Update database schema
  - Estimated: 4 hours

---

### **API2:2023 - Broken Authentication**

**Risk:** No authentication mechanism allows anyone to access all endpoints

**Current State:** No authentication implemented

**Remediation:**
1. Implement JWT-based authentication
2. Add secure token management (refresh tokens, expiration)
3. Implement password policies and secure storage
4. Add multi-factor authentication (optional)

**Implementation Tasks:**
- [ ] **Task 2.1:** Add ASP.NET Core Identity with JWT
  - Files: `Program.cs`, new `Services/AuthService.cs`
  - Install packages: `Microsoft.AspNetCore.Authentication.JwtBearer`
  - Configure JWT bearer authentication
  - Estimated: 4 hours

- [ ] **Task 2.2:** Create authentication endpoints
  - New file: `controllers/AuthController.cs`
  - Implement `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`
  - Add password hashing with PBKDF2 or bcrypt
  - Estimated: 4 hours

- [ ] **Task 2.3:** Implement token management
  - Add refresh token rotation
  - Configure token expiration (access: 15min, refresh: 7days)
  - Store refresh tokens securely (hashed in database)
  - Estimated: 3 hours

---

### **API3:2023 - Broken Object Property Level Authorization**

**Risk:** Users can modify properties they shouldn't (e.g., role, price, status)

**Current State:** All properties in DTOs are writable

**Remediation:**
1. Use separate DTOs for read/write operations
2. Implement property-level authorization
3. Whitelist allowed properties per user role

**Implementation Tasks:**
- [ ] **Task 3.1:** Create role-based DTOs
  - Files: `DTOs/MovieDtos.cs`
  - Separate `MovieCreateDto`, `MovieUpdateDto`, `MovieResponseDto`
  - Remove sensitive properties from update DTOs
  - Estimated: 2 hours

- [ ] **Task 3.2:** Implement property filtering in controllers
  - Use AutoMapper with profiles per role
  - Only map allowed properties
  - Estimated: 3 hours

---

### **API4:2023 - Unrestricted Resource Consumption**

**Risk:** API can be overwhelmed by large payloads, too many requests, or expensive queries

**Current State:** No rate limiting, no pagination, no request size limits

**Remediation:**
1. Implement rate limiting per user/IP
2. Add pagination to all list endpoints
3. Set request size limits
4. Implement query complexity limits

**Implementation Tasks:**
- [ ] **Task 4.1:** Add ASP.NET Core rate limiting middleware
  - File: `Program.cs`
  - Configure fixed window rate limiting (e.g., 100 requests/minute)
  - Install: `Microsoft.AspNetCore.RateLimiting`
  - Estimated: 2 hours

- [ ] **Task 4.2:** Implement pagination
  - Files: All controller GET endpoints, `DTOs/MovieDtos.cs`
  - Add `PagedResponse<T>` DTO
  - Add `page` and `pageSize` query parameters
  - Limit max page size to 100
  - Estimated: 3 hours

- [ ] **Task 4.3:** Configure request size limits
  - File: `Program.cs`
  - Set `FormOptions.ValueLengthLimit` and `MultipartBodyLengthLimit`
  - Estimated: 1 hour

---

### **API5:2023 - Broken Function Level Authorization**

**Risk:** Regular users can access admin endpoints

**Current State:** No role-based access control

**Remediation:**
1. Implement role-based authorization policies
2. Protect admin endpoints with `[Authorize(Roles = "Admin")]`
3. Audit all endpoints for proper authorization

**Implementation Tasks:**
- [ ] **Task 5.1:** Define authorization policies
  - File: `Program.cs`
  - Create policies: `AdminOnly`, `UserOnly`, `Moderator`
  - Estimated: 2 hours

- [ ] **Task 5.2:** Apply policies to controllers
  - Add `[Authorize(Policy = "AdminOnly")]` to admin endpoints
  - Create admin-specific controllers if needed
  - Estimated: 2 hours

---

### **API6:2023 - Unrestricted Access to Sensitive Business Flows**

**Risk:** Automated abuse of business flows (e.g., bulk movie creation, scraping)

**Current State:** No protection against automated abuse

**Remediation:**
1. Implement CAPTCHA for sensitive operations
2. Add bot detection
3. Rate limit by business flow (not just IP)
4. Monitor for unusual patterns

**Implementation Tasks:**
- [ ] **Task 6.1:** Add enhanced rate limiting for write operations
  - File: `Program.cs`
  - Stricter limits for POST/PUT/DELETE (e.g., 10/minute)
  - Estimated: 1 hour

- [ ] **Task 6.2:** Implement audit logging for sensitive operations
  - New file: `Services/AuditLogService.cs`
  - Log all CREATE/UPDATE/DELETE operations
  - Estimated: 3 hours

---

### **API7:2023 - Server Side Request Forgery (SSRF)**

**Risk:** API can be tricked into making requests to internal resources

**Current State:** No external URL processing (low risk currently)

**Remediation:**
1. Validate and whitelist external URLs if added in future
2. Disable unnecessary URL schemes
3. Use allowlists for permitted domains

**Implementation Tasks:**
- [ ] **Task 7.1:** Add URL validation utilities (if needed)
  - New file: `Helpers/UrlValidator.cs`
  - Validate schemes (http/https only)
  - Block internal IP ranges
  - Estimated: 2 hours (only if URL features added)

**Priority:** LOW - Defer until URL processing features are added

---

### **API8:2023 - Security Misconfiguration**

**Risk:** Default configurations expose sensitive information or weaken security

**Current State:** 
- Swagger enabled in development only (good)
- No security headers
- Connection string in plaintext
- Generic error messages may leak info

**Remediation:**
1. Add security headers middleware
2. Implement global exception handling
3. Secure configuration management
4. Disable unnecessary features

**Implementation Tasks:**
- [ ] **Task 8.1:** Add security headers middleware
  - New file: `Middleware/SecurityHeadersMiddleware.cs`
  - Add headers: X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Content-Security-Policy, Strict-Transport-Security
  - Estimated: 2 hours

- [ ] **Task 8.2:** Implement global exception handling
  - New file: `Middleware/ExceptionHandlingMiddleware.cs`
  - Return generic error messages
  - Log detailed errors internally
  - Estimated: 2 hours

- [ ] **Task 8.3:** Secure configuration
  - File: `appsettings.json`
  - Move connection strings to User Secrets (dev) / Key Vault (prod)
  - Use environment variables for sensitive config
  - Estimated: 2 hours

- [ ] **Task 8.4:** Configure CORS properly
  - File: `Program.cs`
  - Restrict to specific origins (not `*`)
  - Estimated: 1 hour

---

### **API9:2023 - Improper Inventory Management**

**Risk:** Undocumented or deprecated endpoints create security blind spots

**Current State:**
- No API versioning
- Swagger only in development
- No API documentation in production

**Remediation:**
1. Implement API versioning
2. Maintain API documentation
3. Track all endpoints and deprecations

**Implementation Tasks:**
- [ ] **Task 9.1:** Add API versioning
  - Install: `Microsoft.AspNetCore.Mvc.Versioning`
  - Configure versioning in `Program.cs`
  - Add version to route: `api/v1/[controller]`
  - Estimated: 3 hours

- [ ] **Task 9.2:** Configure Swagger for production (protected)
  - File: `Program.cs`
  - Add authentication to Swagger UI
  - Or disable in production entirely
  - Estimated: 2 hours

---

### **API10:2023 - Unsafe Consumption of APIs**

**Risk:** Third-party API data is trusted without validation

**Current State:** No external API integrations (low risk currently)

**Remediation:**
1. Validate all external API responses
2. Implement timeout and retry policies
3. Use circuit breaker pattern

**Implementation Tasks:**
- [ ] **Task 10.1:** Add response validation utilities (if needed)
  - New file: `Helpers/ApiResponseValidator.cs`
  - Validate schema, types, ranges
  - Estimated: 2 hours (only if external APIs added)

**Priority:** LOW - Defer until external API integrations are added

---

## Implementation Phases

### **Phase 1: Foundation (Day 1)**
**Goal:** Basic authentication and authorization

**Tasks:**
- Task 2.1: Add ASP.NET Core Identity with JWT (4h)
- Task 2.2: Create authentication endpoints (4h)
- Task 2.3: Implement token management (3h)
- Task 5.1: Define authorization policies (2h)
- Task 8.4: Configure CORS properly (1h)

**Estimated:** 14 hours (~2 days)

---

### **Phase 2: Authorization & Validation (Day 2-3)**
**Goal:** Protect resources and validate input

**Tasks:**
- Task 1.1: Add authorization attributes (2h)
- Task 1.2: Implement ownership checks (3h)
- Task 3.1: Create role-based DTOs (2h)
- Task 3.2: Implement property filtering (3h)
- Task 4.3: Configure request size limits (1h)
- Task 8.2: Implement global exception handling (2h)

**Estimated:** 13 hours (~1.5 days)

---

### **Phase 3: Resource Protection (Day 3-4)**
**Goal:** Prevent abuse and resource exhaustion

**Tasks:**
- Task 4.1: Add rate limiting middleware (2h)
- Task 4.2: Implement pagination (3h)
- Task 5.2: Apply policies to controllers (2h)
- Task 6.1: Enhanced rate limiting for writes (1h)
- Task 6.2: Implement audit logging (3h)
- Task 8.1: Add security headers (2h)

**Estimated:** 13 hours (~1.5 days)

---

### **Phase 4: Hardening & Documentation (Day 4-5)**
**Goal:** Complete security hardening

**Tasks:**
- Task 1.3: Migrate to GUIDs (4h)
- Task 8.3: Secure configuration (2h)
- Task 9.1: Add API versioning (3h)
- Task 9.2: Configure protected Swagger (2h)
- Task 7.1: URL validation (defer if not needed)
- Task 10.1: Response validation (defer if not needed)

**Estimated:** 11 hours (~1.5 days)

---

## Total Effort Summary

| Phase | Duration | Tasks |
|-------|----------|-------|
| Phase 1: Foundation | 2 days | Authentication, JWT, CORS |
| Phase 2: Authorization | 1.5 days | BOLA, property protection, validation |
| Phase 3: Resource Protection | 1.5 days | Rate limiting, pagination, logging |
| Phase 4: Hardening | 1.5 days | GUIDs, versioning, config |
| **Total** | **6.5 days** | **25 tasks** |

**Recommended:** Spread over 2 weeks to allow for testing and refinement

---

## Required NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="10.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer" Version="10.*" />
<PackageReference Include="AutoMapper" Version="13.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />
```

---

## Testing Strategy

### Security Testing Checklist
- [ ] Penetration testing with OWASP ZAP
- [ ] JWT token validation tests
- [ ] Authorization bypass attempts
- [ ] Rate limiting effectiveness
- [ ] SQL injection testing (though EF Core protects)
- [ ] XSS testing in error messages
- [ ] CORS misconfiguration testing
- [ ] Header security validation

### Automated Tests
- [ ] Unit tests for authorization logic
- [ ] Integration tests for authentication flows
- [ ] API endpoint security tests
- [ ] Rate limiting tests

---

## Success Criteria

✅ **Phase 1 Complete:**
- All endpoints require authentication
- JWT tokens issued and validated
- Users can register and login

✅ **Phase 2 Complete:**
- Users can only access their own resources
- Input validation on all endpoints
- Generic error messages returned

✅ **Phase 3 Complete:**
- Rate limiting active (100 req/min read, 10 req/min write)
- Pagination on all list endpoints
- Security headers present
- Audit logs for all mutations

✅ **Phase 4 Complete:**
- API versioned (v1)
- Resource IDs use GUIDs
- Configuration secured
- Swagger protected or disabled in production

---

## Risk Mitigation

### High Priority (Implement First)
1. Authentication (API2)
2. Authorization (API1, API5)
3. Rate limiting (API4)

### Medium Priority
4. Input validation (API3)
5. Security headers (API8)
6. Audit logging (API6)

### Low Priority (Defer)
7. SSRF protection (API7) - only if URL processing added
8. External API validation (API10) - only if 3rd party APIs used

---

## Maintenance & Monitoring

### Ongoing Tasks
- [ ] Monthly dependency vulnerability scans
- [ ] Quarterly security audit
- [ ] Annual penetration testing
- [ ] Continuous monitoring of audit logs
- [ ] Rate limit alerting

### Tools to Consider
- **Dependency scanning:** GitHub Dependabot, Snyk
- **Static analysis:** SonarQube with security rules
- **Dynamic testing:** OWASP ZAP, Burp Suite
- **Monitoring:** Application Insights with security alerts

---

## References

- OWASP API Security Top 10 2023: https://owasp.org/www-project-api-security/
- ASP.NET Core Security Documentation: https://learn.microsoft.com/en-us/aspnet/core/security/
- JWT Best Practices: https://datatracker.ietf.org/doc/html/rfc8725
- Rate Limiting in .NET: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limiter

---

## Next Steps

1. **Review and approve this plan** with stakeholders
2. **Set up development environment** with required packages
3. **Start Phase 1** - Authentication foundation
4. **Create security test suite** alongside implementation
5. **Document all changes** in API documentation
6. **Plan production rollout** with backward compatibility

---

**Document Version:** 1.0  
**Created:** 2026-05-18  
**Last Updated:** 2026-05-18  
**Owner:** Security Team
