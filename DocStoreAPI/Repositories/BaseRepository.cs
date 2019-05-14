﻿using DocStoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocStoreAPI.Repositories
{
    public class BaseRepository
    {
        private readonly DocStoreContext _context;
        private List<AuditEntry> OnBeforeSaveChanges()
        {
            _context.ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                // if is an audit log or is an AccessLog or is detched or is unchanged
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || entry.Entity is AccessLogEntity || entry.Entity is UpdateState)
                    continue;

                var auditEntry = new AuditEntry(entry);

                auditEntry.ObjectType = entry.Metadata.Name;
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType != typeof(UpdateState) || property.Metadata.Name == "LastViewed")
                        continue;

                    string propertyName = property.Metadata.Name;

                    if (property.IsTemporary)
                    {
                        // value will be generated by the database, get the value after saving
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            // Ensure we don't add have any blank audit entries
            auditEntries = auditEntries.Where(ae => ae.PropertyChangedCount != 0).ToList();

            // Save audit entities that have all the modifications
            foreach (var auditEntry in auditEntries.Where(e => !e.HasTemporaryProperties))
            {
                _context.AuditItems.Add(auditEntry.ToAudit());
            }


            // keep a list of entries where the value of some properties are unknown at this step
            return auditEntries.Where(e => e.HasTemporaryProperties).ToList();
        }

        private void OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            foreach (var auditEntry in auditEntries)
            {
                // Get the final value of the temporary properties
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                // Save the Audit entry
                _context.AuditItems.Add(auditEntry.ToAudit());
            }

            _context.SaveChanges();
        }

        private Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                _context.AuditItems.Add(auditEntry.ToAudit());
            }
            return _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = _context.SaveChanges();
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        public int SaveChangesWoTracking()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await _context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await OnAfterSaveChangesAsync(auditEntries);
            return result;
        }

        public async Task<int> SaveChangesWoTrackingAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}