using Microsoft.EntityFrameworkCore;
using Notifications.Entities;

namespace Notifications.Data;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();
}
