﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Domain.Aggregates.Events;
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
    [Migration("20231107003132_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("truquest_events")
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "truquest_events", "thing_event_type", new[] { "funded", "validation_verifier_lottery_failed", "validation_verifier_lottery_succeeded", "validation_poll_finalized", "settlement_proposal_funded", "settlement_proposal_assessment_verifier_lottery_failed", "settlement_proposal_assessment_verifier_lottery_succeeded", "settlement_proposal_assessment_poll_finalized" });
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

                    b.Property<ThingEventType>("Type")
                        .HasColumnType("truquest_events.thing_event_type");

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

            modelBuilder.Entity("Domain.Aggregates.Events.CastedSettlementProposalAssessmentPollVoteEvent", b =>
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

                    b.Property<int>("LogIndex")
                        .HasColumnType("integer");

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
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash", "LogIndex")
                        .IsUnique();

                    b.ToTable("CastedSettlementProposalAssessmentPollVoteEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.CastedThingValidationPollVoteEvent", b =>
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

                    b.Property<int>("LogIndex")
                        .HasColumnType("integer");

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
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash", "LogIndex")
                        .IsUnique();

                    b.ToTable("CastedThingValidationPollVoteEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("LogIndex")
                        .HasColumnType("integer");

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
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash", "LogIndex")
                        .IsUnique();

                    b.ToTable("ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.JoinedSettlementProposalAssessmentVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("LogIndex")
                        .HasColumnType("integer");

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
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash", "LogIndex")
                        .IsUnique();

                    b.ToTable("JoinedSettlementProposalAssessmentVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.JoinedThingValidationVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<long>("L1BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("LogIndex")
                        .HasColumnType("integer");

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
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TxnHash", "LogIndex")
                        .IsUnique();

                    b.ToTable("JoinedThingValidationVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.SettlementProposalAssessmentVerifierLotteryInitializedEvent", b =>
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

                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

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

                    b.ToTable("SettlementProposalAssessmentVerifierLotteryInitializedEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.ThingValidationVerifierLotteryInitializedEvent", b =>
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

                    b.ToTable("ThingValidationVerifierLotteryInitializedEvents", "truquest_events");
                });
#pragma warning restore 612, 618
        }
    }
}