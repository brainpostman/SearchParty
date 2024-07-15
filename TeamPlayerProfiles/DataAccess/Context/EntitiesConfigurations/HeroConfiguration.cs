﻿using DataAccess.Entities;
using DataAccess.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configurations
{
    public class HeroConfiguration : IEntityTypeConfiguration<Hero>
    {
        public void Configure(EntityTypeBuilder<Hero> builder)
        {
            builder.Property(e => e.Name)
                .IsRequired();

            builder.Property(e => e.MainStat)
                .IsRequired()
                .HasConversion(e => e.ToString(), e => Enum.Parse<MainStat>(e));

            builder.HasMany(e => e.Players)
                .WithMany();

            SeedData(builder);
        }

        private void SeedData(EntityTypeBuilder<Hero> builder)
        {
            builder.HasData(new Hero { Id = 1, Name = "Juggernaut", MainStat = MainStat.Agility });
            builder.HasData(new Hero { Id = 2, Name = "Pudge", MainStat = MainStat.Strength });
            builder.HasData(new Hero { Id = 3, Name = "Crystal Maiden", MainStat = MainStat.Intelligence });
            builder.HasData(new Hero { Id = 4, Name = "Invoker", MainStat = MainStat.Universal });
            builder.HasData(new Hero { Id = 5, Name = "Meepo", MainStat = MainStat.Agility });
        }
    }
}
