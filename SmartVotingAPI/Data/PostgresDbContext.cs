﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartVotingAPI.Models.Postgres;

namespace SmartVotingAPI.Data
{
    public partial class PostgresDbContext : DbContext
    {
        public PostgresDbContext()
        {
        }

        public PostgresDbContext(DbContextOptions<PostgresDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApiKey> ApiKeys { get; set; } = null!;
        public virtual DbSet<ElectionOfficial> ElectionOfficials { get; set; } = null!;
        public virtual DbSet<PartyList> PartyLists { get; set; } = null!;
        public virtual DbSet<PartyStaff> PartyStaffs { get; set; } = null!;
        public virtual DbSet<PlatformTopic> PlatformTopics { get; set; } = null!;
        public virtual DbSet<ProvinceList> ProvinceLists { get; set; } = null!;
        public virtual DbSet<PwdHash> PwdHashes { get; set; } = null!;
        public virtual DbSet<RidingList> RidingLists { get; set; } = null!;
        public virtual DbSet<RoleList> RoleLists { get; set; } = null!;
        public virtual DbSet<VolunteerApplication> VolunteerApplications { get; set; } = null!;
        public virtual DbSet<VoterList> VoterLists { get; set; } = null!;
        public virtual DbSet<VoterSecurity> VoterSecurities { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=smartvoting.cbh05r7ygr2w.us-east-1.rds.amazonaws.com;Port=5432;Database=postgres;User Id=svAdmin;Password=w3VSuKPW5jBESFBqnAcAaPEeAacknG5C");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("api_keys_pkey");

                entity.ToTable("api_keys");

                entity.HasIndex(e => e.Name, "api_keys_name_key")
                    .IsUnique();

                entity.Property(e => e.Key)
                    .HasColumnName("key")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created).HasColumnName("created");

                entity.Property(e => e.Name)
                    .HasMaxLength(32)
                    .HasColumnName("name");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Updated).HasColumnName("updated");
            });

            modelBuilder.Entity<ElectionOfficial>(entity =>
            {
                entity.HasKey(e => e.EmployeeId)
                    .HasName("election_officials_pk");

                entity.ToTable("election_officials");

                entity.HasIndex(e => e.EmployeeId, "election_officials_employee_id_uindex")
                    .IsUnique();

                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(129)
                    .HasColumnName("email_address");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(64)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(64)
                    .HasColumnName("last_name");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(16)
                    .HasColumnName("phone_number");

                entity.Property(e => e.RoleId).HasColumnName("role_id");
            });

            modelBuilder.Entity<PartyList>(entity =>
            {
                entity.HasKey(e => e.PartyId)
                    .HasName("party_list_pk");

                entity.ToTable("party_list");

                entity.HasIndex(e => e.PartyId, "party_list_party_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.PartyName, "party_list_party_name_uindex")
                    .IsUnique();

                entity.Property(e => e.PartyId).HasColumnName("party_id");

                entity.Property(e => e.Created)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created");

                entity.Property(e => e.DeregisterReason)
                    .HasColumnType("character varying")
                    .HasColumnName("deregister_reason");

                entity.Property(e => e.EmailAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("email_address");

                entity.Property(e => e.FacebookId)
                    .HasColumnType("character varying")
                    .HasColumnName("facebook_id");

                entity.Property(e => e.FaxNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("fax_number");

                entity.Property(e => e.FlickrId)
                    .HasColumnType("character varying")
                    .HasColumnName("flickr_id");

                entity.Property(e => e.HeadOffice)
                    .HasColumnType("character varying")
                    .HasColumnName("head_office");

                entity.Property(e => e.InstagramId)
                    .HasColumnType("character varying")
                    .HasColumnName("instagram_id");

                entity.Property(e => e.IsRegistered).HasColumnName("is_registered");

                entity.Property(e => e.PartyDomain)
                    .HasColumnType("character varying")
                    .HasColumnName("party_domain");

                entity.Property(e => e.PartyName)
                    .HasColumnType("character varying")
                    .HasColumnName("party_name");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("phone_number");

                entity.Property(e => e.SnapchatId)
                    .HasColumnType("character varying")
                    .HasColumnName("snapchat_id");

                entity.Property(e => e.TwitterId)
                    .HasColumnType("character varying")
                    .HasColumnName("twitter_id");

                entity.Property(e => e.Updated)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.YoutubeId)
                    .HasColumnType("character varying")
                    .HasColumnName("youtube_id");
            });

            modelBuilder.Entity<PartyStaff>(entity =>
            {
                entity.HasKey(e => e.EntryId)
                    .HasName("table_name_pk");

                entity.ToTable("party_staff");

                entity.HasIndex(e => e.EntryId, "table_name_entry_id_uindex")
                    .IsUnique();

                entity.Property(e => e.EntryId)
                    .HasColumnName("entry_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(65)
                    .HasColumnName("email_address");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(32)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(32)
                    .HasColumnName("last_name");

                entity.Property(e => e.PartyId).HasColumnName("party_id");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(16)
                    .HasColumnName("phone_number");

                entity.Property(e => e.RidingId).HasColumnName("riding_id");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<PlatformTopic>(entity =>
            {
                entity.HasKey(e => e.TopicId)
                    .HasName("platform_topics_pk");

                entity.ToTable("platform_topics");

                entity.HasIndex(e => e.TopicId, "platform_topics_topic_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.TopicTitle, "platform_topics_topic_title_uindex")
                    .IsUnique();

                entity.Property(e => e.TopicId).HasColumnName("topic_id");

                entity.Property(e => e.TopicTitle)
                    .HasColumnType("character varying")
                    .HasColumnName("topic_title");
            });

            modelBuilder.Entity<ProvinceList>(entity =>
            {
                entity.HasKey(e => e.ProvinceId)
                    .HasName("province_list_pk");

                entity.ToTable("province_list");

                entity.HasIndex(e => e.ProvinceId, "province_list_province_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.ProvinceName, "province_list_province_name_uindex")
                    .IsUnique();

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.ProvinceName)
                    .HasMaxLength(32)
                    .HasColumnName("province_name");
            });

            modelBuilder.Entity<PwdHash>(entity =>
            {
                entity.HasKey(e => e.AccountId)
                    .HasName("pwd_hash_pk");

                entity.ToTable("pwd_hash");

                entity.HasIndex(e => e.AccountId, "pwd_hash_account_id_uindex")
                    .IsUnique();

                entity.Property(e => e.AccountId)
                    .ValueGeneratedNever()
                    .HasColumnName("account_id");

                entity.Property(e => e.PwdHash1)
                    .HasColumnType("character varying")
                    .HasColumnName("pwd_hash");
            });

            modelBuilder.Entity<RidingList>(entity =>
            {
                entity.HasKey(e => e.RidingId)
                    .HasName("riding_list_pk");

                entity.ToTable("riding_list");

                entity.HasIndex(e => e.RidingId, "riding_list_riding_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.RidingName, "riding_list_riding_name_uindex")
                    .IsUnique();

                entity.Property(e => e.RidingId)
                    .ValueGeneratedNever()
                    .HasColumnName("riding_id");

                entity.Property(e => e.EmailAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("email_address");

                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");

                entity.Property(e => e.FaxNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("fax_number");

                entity.Property(e => e.OfficeAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("office_address");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("phone_number");

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.RidingName)
                    .HasColumnType("character varying")
                    .HasColumnName("riding_name");

                entity.Property(e => e.Updated)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<RoleList>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("role_list_pk");

                entity.ToTable("role_list");

                entity.HasIndex(e => e.RoleId, "role_list_role_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.RoleTitle, "role_list_role_title_uindex")
                    .IsUnique();

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.RoleGroup)
                    .HasColumnType("character varying")
                    .HasColumnName("role_group");

                entity.Property(e => e.RoleTitle)
                    .HasColumnType("character varying")
                    .HasColumnName("role_title");
            });

            modelBuilder.Entity<VolunteerApplication>(entity =>
            {
                entity.HasKey(e => e.ApplicationId)
                    .HasName("volunteer_applications_pk");

                entity.ToTable("volunteer_applications");

                entity.HasIndex(e => e.ApplicationId, "volunteer_applications_application_id_uindex")
                    .IsUnique();

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.EmailAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("email_address");

                entity.Property(e => e.FirstName)
                    .HasColumnType("character varying")
                    .HasColumnName("first_name");

                entity.Property(e => e.IsApproved).HasColumnName("is_approved");

                entity.Property(e => e.LastName)
                    .HasColumnType("character varying")
                    .HasColumnName("last_name");

                entity.Property(e => e.LegalResident).HasColumnName("legal_resident");

                entity.Property(e => e.PartyId).HasColumnName("party_id");

                entity.Property(e => e.PartyMember).HasColumnName("party_member");

                entity.Property(e => e.PastVolunteer).HasColumnName("past_volunteer");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("phone_number");

                entity.Property(e => e.RidingId).HasColumnName("riding_id");

                entity.Property(e => e.Submitted)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("submitted");

                entity.Property(e => e.Updated)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("updated");
            });

            modelBuilder.Entity<VoterList>(entity =>
            {
                entity.HasKey(e => e.VoterId)
                    .HasName("voter_list_pk");

                entity.ToTable("voter_list");

                entity.HasIndex(e => e.VoterId, "voter_list_voter_id_uindex")
                    .IsUnique();

                entity.Property(e => e.VoterId)
                    .ValueGeneratedNever()
                    .HasColumnName("voter_id");

                entity.Property(e => e.EmailAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("email_address");

                entity.Property(e => e.FirstName)
                    .HasColumnType("character varying")
                    .HasColumnName("first_name");

                entity.Property(e => e.HomeAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("home_address");

                entity.Property(e => e.LastName)
                    .HasColumnType("character varying")
                    .HasColumnName("last_name");

                entity.Property(e => e.MiddleName)
                    .HasColumnType("character varying")
                    .HasColumnName("middle_name");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("phone_number");

                entity.Property(e => e.RidingId).HasColumnName("riding_id");
            });

            modelBuilder.Entity<VoterSecurity>(entity =>
            {
                entity.HasKey(e => e.CardId)
                    .HasName("voter_security_pk");

                entity.ToTable("voter_security");

                entity.HasIndex(e => e.CardId, "voter_security_card_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.EmailPin, "voter_security_email_pin_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.VoterId, "voter_security_voter_id_uindex")
                    .IsUnique();

                entity.Property(e => e.CardId)
                    .HasMaxLength(16)
                    .HasColumnName("card_id");

                entity.Property(e => e.BirthDate).HasColumnName("birth_date");

                entity.Property(e => e.CardPin).HasColumnName("card_pin");

                entity.Property(e => e.EmailPin)
                    .HasColumnType("character varying")
                    .HasColumnName("email_pin");

                entity.Property(e => e.Sin).HasColumnName("sin");

                entity.Property(e => e.Tax10100).HasColumnName("tax_10100");

                entity.Property(e => e.Tax12000).HasColumnName("tax_12000");

                entity.Property(e => e.Tax12900).HasColumnName("tax_12900");

                entity.Property(e => e.Tax14299).HasColumnName("tax_14299");

                entity.Property(e => e.Tax15000).HasColumnName("tax_15000");

                entity.Property(e => e.Tax23600).HasColumnName("tax_23600");

                entity.Property(e => e.Tax24400).HasColumnName("tax_24400");

                entity.Property(e => e.Tax26000).HasColumnName("tax_26000");

                entity.Property(e => e.Tax31270).HasColumnName("tax_31270");

                entity.Property(e => e.Tax58240).HasColumnName("tax_58240");

                entity.Property(e => e.VoterId).HasColumnName("voter_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}