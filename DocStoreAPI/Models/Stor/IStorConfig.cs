﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DocStore.API;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DocStore.API.Models.Stor
{
    public interface IStorConfig
    {
        string ShortName { get; set; }
        string StorType { get; }
        IStor GetStorFromConfig();
    }
}