﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    [DbContext(typeof(EventDbContext))]
    partial class EventDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

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

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CastedAcceptancePollVoteEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.JoinedThingAssessmentVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Nonce")
                        .HasColumnType("numeric");

                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("JoinedThingAssessmentVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.JoinedVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Nonce")
                        .HasColumnType("numeric");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("JoinedVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.PreJoinedThingAssessmentVerifierLotteryEvent", b =>
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

                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("PreJoinedThingAssessmentVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.PreJoinedVerifierLotteryEvent", b =>
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

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("PreJoinedVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.ThingAssessmentVerifierLotterySpotClaimedEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ThingAssessmentVerifierLotterySpotClaimedEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.QM.VerifierLotteryWinnerQm", b =>
                {
                    b.Property<decimal>("Index")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable((string)null);

                    b.ToView("VerifierLotteryWinners", "truquest_events");
                });
#pragma warning restore 612, 618
        }
    }
}
