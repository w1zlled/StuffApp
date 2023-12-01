﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StuffApp.Models;
using StuffApp.Models.Data;

namespace StuffApp.Models
{
    public class AppCtx : IdentityDbContext<User>
    {
        public AppCtx(DbContextOptions<AppCtx> options) : base(options) { 
            Database.EnsureCreated();
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PostStatus> PostStatus { get; set; }
        /*public DbSet<StuffApp.Models.Data.PostStatus> PostStatus { get; set; } = default!;*/

    }
}