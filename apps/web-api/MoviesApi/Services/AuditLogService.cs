using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Data;
using MoviesApi.Models;

namespace MoviesApi.Services;

public interface IAuditLogService
{
    Task LogAsync(string userId, string action, string entityType, string? entityId, 
        object? oldValues, object? newValues, string ipAddress, string userAgent, string? endpoint = null);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
}

public class AuditLogService : IAuditLogService
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(MoviesDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string userId, string action, string entityType, string? entityId,
        object? oldValues, object? newValues, string ipAddress, string userAgent, string? endpoint = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Endpoint = endpoint,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log entry");
            // Don't throw - audit logging should not break the main operation
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}
