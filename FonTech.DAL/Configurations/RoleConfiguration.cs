using FonTech.Domain.Entity;
using FonTech.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FonTech.DAL.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Name).HasMaxLength(20);

            builder.HasData(new List<Role>
            {
                new Role
                {
                    Id = 1,
                    Name = nameof(Roles.User)
                },
                new Role
                {
                    Id = 2,
                    Name = nameof(Roles.Moderator)
                },
                new Role
                {
                    Id = 3,
                    Name = nameof(Roles.Admin)
                }
            });
        }
    }
}
