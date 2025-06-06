﻿// <auto-generated />
using System;
using CalorieTracker.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CalorieTracker.API.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.4");

            modelBuilder.Entity("CalorieTracker.API.Models.DailyLog", b =>
                {
                    b.Property<Guid>("DailyLogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("KcalsBurn")
                        .HasColumnType("INTEGER");

                    b.Property<int>("KcalsIntake")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("DailyLogId");

                    b.HasIndex("UserId", "Date")
                        .IsUnique()
                        .HasDatabaseName("IX_DailyLogs_UserId_Date");

                    b.ToTable("DailyLogs");
                });

            modelBuilder.Entity("CalorieTracker.API.Models.Goal", b =>
                {
                    b.Property<Guid>("GoalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("TargetKcals")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TimeWindowDays")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("GoalId");

                    b.HasIndex("UserId")
                        .HasDatabaseName("IX_Goals_UserId");

                    b.ToTable("Goals");
                });
#pragma warning restore 612, 618
        }
    }
}
