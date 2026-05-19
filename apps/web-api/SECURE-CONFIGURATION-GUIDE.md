# Secure Configuration Guide

## Overview
This guide explains how to securely configure the MoviesApi for development and production environments.

---

## 🔐 Secrets Management

### Development Environment

**Use .NET User Secrets:**

```bash
cd /home/user/src/MoviesApp/apps/web-api/MoviesApi

# Initialize user secrets
dotnet user-secrets init

# Set JWT Secret Key (generate a secure random string)
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_SUPER_SECRET_KEY_MIN_32_CHARS_RANDOM"

# Set connection string (optional - can use default)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=movies.db"
```

**Generate a Secure Secret Key:**
```bash
# Using OpenSSL
openssl rand -base64 32

# Using .NET
dotnet new console -n KeyGen && cd KeyGen
echo 'Console.WriteLine(Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));' > Program.cs
dotnet run
```

### Production Environment

**Use Environment Variables:**

```bash
# Linux/Mac
export JwtSettings__SecretKey="your-production-secret-key-here"
export ConnectionStrings__DefaultConnection="Data Source=production.db"

# Or in Docker/docker-compose
environment:
  - JwtSettings__SecretKey=your-production-secret-key-here
  - ConnectionStrings__DefaultConnection=Data Source=production.db
```

**Azure Key Vault (Recommended for Azure):**
```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["KeyVault:Uri"]),
    new DefaultAzureCredential());
```

---

## 📋 Required Configuration Variables

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `JwtSettings:SecretKey` | JWT signing key (min 32 chars) | ✅ Yes | - |
| `JwtSettings:Issuer` | JWT issuer | ❌ No | MoviesApi |
| `JwtSettings:Audience` | JWT audience | ❌ No | MoviesAppUsers |
| `JwtSettings:AccessTokenExpirationMinutes` | Access token lifetime | ❌ No | 15 |
| `JwtSettings:RefreshTokenExpirationDays` | Refresh token lifetime | ❌ No | 7 |
| `ConnectionStrings:DefaultConnection` | Database connection | ✅ Yes | Data Source=movies.db |
| `EnableHttpsRedirection` | Enable HTTPS redirect | ❌ No | true |

---

## 🔒 Security Best Practices

### 1. JWT Secret Key
- **Minimum length:** 32 characters
- **Recommended:** 64+ characters
- **Format:** Cryptographically secure random string
- **Never:** Use predictable values, commit to source control

**Good Example:**
```
X8j2Kp9mNq4rT7wY0zL3vB6cD1eF5gH8iA2kM5nP9sR3tU7xW0yZ4
```

**Bad Examples:**
```
secret123
password
my-secret-key
```

### 2. Connection Strings
- Use relative paths for SQLite in development
- Use absolute paths in production
- Consider using PostgreSQL/MySQL for production

**Production Example (PostgreSQL):**
```
Host=localhost;Database=moviesdb;Username=moviesapi;Password=secure-password
```

### 3. HTTPS Configuration
```csharp
// Already configured in Program.cs
app.UseHttpsRedirection();
app.UseHsts(); // Production only
```

### 4. CORS Configuration
Update allowed origins in `Program.cs`:
```csharp
policy.WithOrigins(
    "https://your-production-domain.com",
    "https://admin.your-domain.com"
)
```

---

## 🚀 Deployment Checklist

### Pre-Deployment

- [ ] Generate production JWT secret key
- [ ] Store secrets in environment variables or Key Vault
- [ ] Update CORS allowed origins
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure HTTPS certificate
- [ ] Update connection string for production database
- [ ] Review and update rate limiting thresholds

### Post-Deployment

- [ ] Test authentication endpoints
- [ ] Verify JWT tokens are being generated
- [ ] Check security headers are present
- [ ] Test rate limiting is active
- [ ] Verify audit logging is working
- [ ] Confirm Swagger is disabled/protected
- [ ] Monitor application logs

---

## 📝 Example Production Configuration

**Docker Compose:**
```yaml
version: '3.8'
services:
  moviesapi:
    image: moviesapi:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - ConnectionStrings__DefaultConnection=Data Source=/data/movies.db
    volumes:
      - movies_data:/data
    ports:
      - "8080:8080"

volumes:
  movies_data:
```

**Environment File (.env):**
```bash
JWT_SECRET_KEY=X8j2Kp9mNq4rT7wY0zL3vB6cD1eF5gH8iA2kM5nP9sR3tU7xW0yZ4
ASPNETCORE_ENVIRONMENT=Production
```

---

## 🔍 Configuration Validation

The application will fail to start if required configuration is missing:

```
InvalidOperationException: JWT SecretKey not configured
```

**Test Configuration:**
```bash
# Check if secrets are set (development)
dotnet user-secrets list

# Test API health
curl http://localhost:5000/health
```

---

## 📊 Monitoring Configuration

**Application Insights (Optional):**
```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

**Serilog for Enhanced Logging:**
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

---

## ⚠️ Common Issues

### Issue: JWT Token Validation Fails
**Cause:** Secret key mismatch between environments  
**Solution:** Ensure same secret key is used for token generation and validation

### Issue: CORS Errors
**Cause:** Frontend origin not in allowed list  
**Solution:** Add frontend URL to CORS policy in Program.cs

### Issue: Database Connection Fails
**Cause:** Connection string not configured  
**Solution:** Set `ConnectionStrings:DefaultConnection` environment variable

---

## 📚 References

- [.NET Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/)
- [OWASP Configuration Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Configuration_Cheat_Sheet.html)

---

**Last Updated:** 2026-05-18  
**Version:** 1.0
