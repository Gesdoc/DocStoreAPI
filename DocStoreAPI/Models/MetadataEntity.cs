﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DocStoreAPI.Models
{
    public class MetadataEntity : EntityBase
    {
        //Id is From Entity Base
        [Required]
        public string Name { get; set; }
        [Required]
        public int Version { get; set; }
        [Required]
        public string MD5Hash { get; set; } //D10CFB0D2819A862937A6D66E9CAE223
        [Required]
        public string StorName { get; set; }
        [Required]
        public string Extension { get; set; }
        [Required]
        public string BuisnessArea { get; set; }

        public int BuisnessAreaEntityID { get; set; }
        public BuisnessAreaEntity BuisnessAreaEntity { get; set; }

        public List<CustomMetadataEntity> CustomMetadata { get; set; }
        public List<DocumentVersionEntity> Versions { get; set; }

        [Required]
        public LockState Locked { get; set; }
        [Required]
        public ArchiveState Archive { get; set; }
        [Required]
        public UpdateState Created { get; set; }
        [Required]
        public UpdateState LastUpdate { get; set; }
        [Required]
        public DateTime LastViewed { get; set; }

        public MetadataEntity()
        {

        }

        public MetadataEntity(string fileName, int version, string extension, string buisnessArea, string storName, string userName)
        {
            this.Name = fileName;
            this.Version = version;
            this.Extension = extension;
            this.BuisnessArea = buisnessArea;
            this.StorName = storName;
            this.CustomMetadata = new List<CustomMetadataEntity>();
            this.Versions = new List<DocumentVersionEntity>();
            this.Locked = new LockState();
            this.Archive = new ArchiveState();
            this.LastUpdate = new UpdateState(userName);
            this.Created = new UpdateState(userName);
            this.LastViewed = DateTime.UtcNow;
        }

        public string GetFileName()
        {
            return string.Format("{0}.{1}", this.Name, this.Extension);
        }
        public string GetServerFileName()
        {
            return string.Format("{0}.v{1}.{2}.{3}",
                this.Id.ToString(), this.Version.ToString(), this.Name, this.Extension);
        }
    }
}
