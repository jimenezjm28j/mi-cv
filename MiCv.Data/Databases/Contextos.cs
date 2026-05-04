using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiCv.Data.Databases;

public class MiCvContext(IConfiguration configuration) : MiCv.Context.MiCvContext
{
    public readonly IConfiguration configuration = configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseSqlServer(configuration["ConnectionsString:MiCv"]);
        }
    }
}
