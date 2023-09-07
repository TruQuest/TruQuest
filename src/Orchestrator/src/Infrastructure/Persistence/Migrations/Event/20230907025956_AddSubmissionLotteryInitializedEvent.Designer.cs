﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    [DbContext(typeof(EventDbContext))]
    [Migration("20230907025956_AddSubmissionLotteryInitializedEvent")]
    partial class AddSubmissionLotteryInitializedEvent
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("truquest_events")
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Aggregates.Events.ActionableThingRelatedEvent", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<IReadOnlyDictionary<string, object>>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("TxnHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash")
                        .IsUnique();

                    b.ToTable("ActionableThingRelatedEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.BlockProcessedEvent", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<long?>("BlockNumber")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("BlockProcessedEvent", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.CastedAcceptancePollVoteEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("TxnHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash")
                        .IsUnique();

                    b.ToTable("CastedAcceptancePollVoteEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.CastedAssessmentPollVoteEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("TxnHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash")
                        .IsUnique();

                    b.ToTable("CastedAssessmentPollVoteEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.JoinedThingAssessmentVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long?>("Nonce")
                        .HasColumnType("bigint");

                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("TxnHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserData")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash")
                        .IsUnique();

                    b.ToTable("JoinedThingAssessmentVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.JoinedThingSubmissionVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long?>("Nonce")
                        .HasColumnType("bigint");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("TxnHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash")
                        .IsUnique();

                    b.ToTable("JoinedThingSubmissionVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.ThingAssessmentVerifierLotterySpotClaimedEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long?>("Nonce")
                        .HasColumnType("bigint");

                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("TxnHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash")
                        .IsUnique();

                    b.ToTable("ThingAssessmentVerifierLotterySpotClaimedEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.ThingSubmissionVerifierLotteryInitializedEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<string>("DataHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("TxnHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserXorDataHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash")
                        .IsUnique();

                    b.ToTable("ThingSubmissionVerifierLotteryInitializedEvents", "truquest_events");
                });
#pragma warning restore 612, 618
        }
    }
}
