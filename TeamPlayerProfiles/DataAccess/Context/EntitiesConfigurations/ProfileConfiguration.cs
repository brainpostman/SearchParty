﻿using DataAccess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.EntitiesConfigurations
{
    public abstract class ProfileConfiguration<T> : IEntityTypeConfiguration<T> where T : class, IProfile
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue(string.Empty);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(150)
                .HasDefaultValue(string.Empty);

            builder.Property(e => e.Displayed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
