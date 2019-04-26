﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocStoreAPI.Models
{
    public class DocStoreContext : DbContext
    {
        public DbSet<AccessControlEntity> AcessControlEntity { get; set; }
        public DbSet<GroupEntity> GroupEntities { get; set; }
        public DbSet<MetadataEntity> MetadataEntities { get; set; }
        public DbSet<DocumentVersionEntity> DocumentVersions { get; set; }
        public DbSet<AccessLogEntity> AccessLogEntities { get; set; }
        public DbSet<BuisnessAreaEntity> BuisnessAreas { get; set; }

        public DbSet<Audit> AuditItems { get; set; }

        public DocStoreContext(DbContextOptions<DocStoreContext> options) : base(options)
        {

        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await OnAfterSaveChangesAsync(auditEntries);
            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MetadataEntity>()
                .HasKey(me => me.Id)
                .ForSqlServerIsClustered();
            modelBuilder.Entity<MetadataEntity>()
                .Property(me => me.Id)
                .UseSqlServerIdentityColumn();
            modelBuilder.Entity<MetadataEntity>()
                .Property(me => me.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<MetadataEntity>()
                .Property(me => me.StorName)
                .HasMaxLength(20)
                .IsRequired();
            modelBuilder.Entity<MetadataEntity>()
                .Property(me => me.Extension)
                .HasMaxLength(10)
                .IsRequired();
            modelBuilder.Entity<MetadataEntity>()
                .Property(me => me.BuisnessArea)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            modelBuilder.Entity<MetadataEntity>()
                .OwnsOne(p => p.Locked);
            modelBuilder.Entity<MetadataEntity>()
                .OwnsOne(p => p.Archive);
            modelBuilder.Entity<MetadataEntity>()
                .OwnsOne(p => p.Created);
            modelBuilder.Entity<MetadataEntity>()
                .OwnsOne(p => p.LastUpdate);
            modelBuilder.Entity<MetadataEntity>()
                .HasOne(me => me.BuisnessAreaEntity)
                .WithMany()
                .HasForeignKey(me => me.BuisnessAreaEntityID)
                .IsRequired()
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<MetadataEntity>()
                .HasMany<CustomMetadataEntity>(p => p.CustomMetadata)
                .WithOne(e => e.Document)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MetadataEntity>()
                .HasMany<DocumentVersionEntity>(p => p.Versions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LockState>()
                .Property(ls => ls.By)
                .IsRequired(false);
            modelBuilder.Entity<LockState>()
                .Property(ls => ls.At)
                .IsRequired(false);
            modelBuilder.Entity<LockState>()
                .Property(ls => ls.Expiration)
                .IsRequired(false);

            modelBuilder.Entity<ArchiveState>()
                .Property(ar => ar.By)
                .IsRequired(false);
            modelBuilder.Entity<ArchiveState>()
                .Property(ar => ar.At)
                .IsRequired(false);

            modelBuilder.Entity<AccessControlEntity>()
                .HasKey(ace => ace.Id)
                .ForSqlServerIsClustered();
            modelBuilder.Entity<AccessControlEntity>()
                .Property(ace => ace.Id)
                .UseSqlServerIdentityColumn();
            modelBuilder.Entity<AccessControlEntity>()
                .Property(ace => ace.GroupName)
                .HasMaxLength(20)
                .IsRequired()
                .IsUnicode(false);
            modelBuilder.Entity<AccessControlEntity>()
                .Property(ace => ace.BusinessArea)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();
            modelBuilder.Entity<AccessControlEntity>()
                .HasOne(ace => ace.Group)
                .WithMany(ge => ge.RelevantACEs);

            modelBuilder.Entity<GroupEntity>()
                .HasKey(ge => ge.Id)
                .ForSqlServerIsClustered();
            modelBuilder.Entity<GroupEntity>()
                .Property(ge => ge.Id)
                .UseSqlServerIdentityColumn();
            modelBuilder.Entity<GroupEntity>()
                .HasIndex(ge => ge.Name)
                .ForSqlServerIsClustered(false)
                .IsUnique();
            modelBuilder.Entity<GroupEntity>()
                .Property(ge => ge.Name)
                .IsRequired()
                .HasMaxLength(20);
            modelBuilder.Entity<GroupEntity>()
                .HasMany(ge => ge.RelevantACEs)
                .WithOne(ace => ace.Group);


            modelBuilder.Entity<DocumentVersionEntity>()
                .HasKey(dve => dve.Id)
                .ForSqlServerIsClustered();
            modelBuilder.Entity<DocumentVersionEntity>()
                .Property(dve => dve.Id)
                .UseSqlServerIdentityColumn();


            modelBuilder.Entity<BuisnessAreaEntity>()
                .HasKey(me => me.Id)
                .ForSqlServerIsClustered();
            modelBuilder.Entity<BuisnessAreaEntity>()
                .Property(me => me.Id)
                .UseSqlServerIdentityColumn();
            modelBuilder.Entity<BuisnessAreaEntity>()
                .HasIndex(bae => bae.Name)
                .ForSqlServerIsClustered(false)
                .IsUnique();
            modelBuilder.Entity<BuisnessAreaEntity>()
                .Property(bae => bae.Name)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();
            modelBuilder.Entity<BuisnessAreaEntity>()
                .HasMany(bae => bae.RelevantAccessControlEntities)
                .WithOne(ace => ace.BuisnessAreaEntity)
                .OnDelete(DeleteBehavior.Cascade);


        }

        public override int SaveChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges();
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                // if is an audit log or is an AccessLog or is detched or is unchanged
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || entry.Entity is AccessLogEntity)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Metadata.Relational().TableName;
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (propertyName.StartsWith("Created") || propertyName.StartsWith("LastUpdate"))
                        continue;

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

            // Save audit entities that have all the modifications
            foreach (var auditEntry in auditEntries.Where(e => !e.HasTemporaryProperties))
            {
                AuditItems.Add(auditEntry.ToAudit());
            }

            // keep a list of entries where the value of some properties are unknown at this step
            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

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
                AuditItems.Add(auditEntry.ToAudit());
            }

            return SaveChangesAsync();
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
                AuditItems.Add(auditEntry.ToAudit());
            }

            SaveChanges();
        }
    }
}