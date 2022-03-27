using System;
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
        public virtual DbSet<FutureElection> FutureElections { get; set; } = null!;
        public virtual DbSet<OfficeList> OfficeLists { get; set; } = null!;
        public virtual DbSet<OfficeType> OfficeTypes { get; set; } = null!;
        public virtual DbSet<PartyList> PartyLists { get; set; } = null!;
        public virtual DbSet<PastCandidate> PastCandidates { get; set; } = null!;
        public virtual DbSet<PastElection> PastElections { get; set; } = null!;
        public virtual DbSet<PastResult> PastResults { get; set; } = null!;
        public virtual DbSet<PastTurnout> PastTurnouts { get; set; } = null!;
        public virtual DbSet<Person> People { get; set; } = null!;
        public virtual DbSet<PlatformTopic> PlatformTopics { get; set; } = null!;
        public virtual DbSet<ProvinceList> ProvinceLists { get; set; } = null!;
        public virtual DbSet<RidingList> RidingLists { get; set; } = null!;
        public virtual DbSet<RoleList> RoleLists { get; set; } = null!;
        public virtual DbSet<SocialMediaList> SocialMediaLists { get; set; } = null!;
        public virtual DbSet<VolunteerApplication> VolunteerApplications { get; set; } = null!;
        public virtual DbSet<VoterList> VoterLists { get; set; } = null!;
        public virtual DbSet<VoterSecurity> VoterSecurities { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=smartvoting.cbh05r7ygr2w.us-east-1.rds.amazonaws.com;Port=5432;Database=postgres;User Id=svAdmin;Password=w3VSuKPW5jBESFBqnAcAaPEeAacknG5C");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.HasKey(e => e.KeyId)
                    .HasName("api_keys_pk");

                entity.ToTable("api_keys");

                entity.HasIndex(e => e.AuthKey, "api_keys_auth_key_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.KeyName, "api_keys_key_name_uindex")
                    .IsUnique();

                entity.Property(e => e.KeyId)
                    .HasColumnName("key_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.AuthKey)
                    .HasColumnName("auth_key")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.IsDevelopment).HasColumnName("is_development");

                entity.Property(e => e.IsProduction).HasColumnName("is_production");

                entity.Property(e => e.KeyName)
                    .HasColumnType("character varying")
                    .HasColumnName("key_name");

                entity.Property(e => e.KeyTtl)
                    .HasColumnName("key_ttl")
                    .HasDefaultValueSql("3");
            });

            modelBuilder.Entity<FutureElection>(entity =>
            {
                entity.HasKey(e => e.ElectionId)
                    .HasName("future_elections_pk");

                entity.ToTable("future_elections");

                entity.HasIndex(e => e.AuthKey, "future_elections_api_key_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.ElectionDate, "future_elections_election_date_uindex")
                    .IsUnique();

                entity.Property(e => e.ElectionId)
                    .HasColumnName("election_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.AuthKey).HasColumnName("auth_key");

                entity.Property(e => e.ElectionDate).HasColumnName("election_date");

                entity.Property(e => e.EndTime).HasColumnName("end_time");

                entity.Property(e => e.StartTime).HasColumnName("start_time");
            });

            modelBuilder.Entity<OfficeList>(entity =>
            {
                entity.HasKey(e => e.OfficeId)
                    .HasName("office_list_pk");

                entity.ToTable("office_list");

                entity.Property(e => e.OfficeId)
                    .HasColumnName("office_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.City)
                    .HasColumnType("character varying")
                    .HasColumnName("city");

                entity.Property(e => e.IsPublic).HasColumnName("is_public");

                entity.Property(e => e.PoBox)
                    .HasColumnType("character varying")
                    .HasColumnName("po_box");

                entity.Property(e => e.PostCode)
                    .HasMaxLength(7)
                    .HasColumnName("post_code");

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.StreetName)
                    .HasColumnType("character varying")
                    .HasColumnName("street_name");

                entity.Property(e => e.StreetNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("street_number");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UnitNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("unit_number");

                entity.Property(e => e.Updated)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<OfficeType>(entity =>
            {
                entity.HasKey(e => e.TypeId)
                    .HasName("office_type_pk");

                entity.ToTable("office_type");

                entity.HasIndex(e => e.TypeId, "office_type_type_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.TypeName, "office_type_type_name_uindex")
                    .IsUnique();

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.TypeName)
                    .HasColumnType("character varying")
                    .HasColumnName("type_name");
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

                entity.Property(e => e.DeregisterReason)
                    .HasColumnType("character varying")
                    .HasColumnName("deregister_reason");

                entity.Property(e => e.EmailAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("email_address");

                entity.Property(e => e.FaxNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("fax_number");

                entity.Property(e => e.IsRegistered).HasColumnName("is_registered");

                entity.Property(e => e.OfficeId).HasColumnName("office_id");

                entity.Property(e => e.PartyDomain)
                    .HasColumnType("character varying")
                    .HasColumnName("party_domain");

                entity.Property(e => e.PartyName)
                    .HasColumnType("character varying")
                    .HasColumnName("party_name");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("phone_number");

                entity.Property(e => e.SocialId).HasColumnName("social_id");

                entity.Property(e => e.Updated)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<PastCandidate>(entity =>
            {
                entity.HasKey(e => e.CandidateId)
                    .HasName("past_candidates_pk");

                entity.ToTable("past_candidates");

                entity.HasIndex(e => e.CandidateId, "past_candidates_candidate_id_uindex")
                    .IsUnique();

                entity.Property(e => e.CandidateId).HasColumnName("candidate_id");

                entity.Property(e => e.FirstName)
                    .HasColumnType("character varying")
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasColumnType("character varying")
                    .HasColumnName("last_name");

                entity.Property(e => e.PartyId).HasColumnName("party_id");

                entity.Property(e => e.RidingId).HasColumnName("riding_id");
            });

            modelBuilder.Entity<PastElection>(entity =>
            {
                entity.HasKey(e => e.ElectionId)
                    .HasName("past_elections_pk");

                entity.ToTable("past_elections");

                entity.HasIndex(e => e.ElectionId, "past_elections_election_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.ElectionYear, "past_elections_election_year_uindex")
                    .IsUnique();

                entity.Property(e => e.ElectionId).HasColumnName("election_id");

                entity.Property(e => e.ElectionDate).HasColumnName("election_date");

                entity.Property(e => e.ElectionType)
                    .HasColumnType("character varying")
                    .HasColumnName("election_type");

                entity.Property(e => e.ElectionYear).HasColumnName("election_year");

                entity.Property(e => e.InvalidVotes).HasColumnName("invalid_votes");

                entity.Property(e => e.TotalElectors).HasColumnName("total_electors");

                entity.Property(e => e.ValidVotes).HasColumnName("valid_votes");
            });

            modelBuilder.Entity<PastResult>(entity =>
            {
                entity.HasKey(e => e.EntryId)
                    .HasName("past_results_pk");

                entity.ToTable("past_results");

                entity.Property(e => e.EntryId)
                    .HasColumnName("entry_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.CandidateId).HasColumnName("candidate_id");

                entity.Property(e => e.Elected).HasColumnName("elected");

                entity.Property(e => e.ElectionId).HasColumnName("election_id");

                entity.Property(e => e.RidingId).HasColumnName("riding_id");

                entity.Property(e => e.TotalVotes).HasColumnName("total_votes");
            });

            modelBuilder.Entity<PastTurnout>(entity =>
            {
                entity.HasKey(e => e.EntryId)
                    .HasName("past_turnout_pk");

                entity.ToTable("past_turnout");

                entity.Property(e => e.EntryId)
                    .HasColumnName("entry_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.ElectionId).HasColumnName("election_id");

                entity.Property(e => e.InvalidVotes).HasColumnName("invalid_votes");

                entity.Property(e => e.RidingId).HasColumnName("riding_id");

                entity.Property(e => e.TotalElectors).HasColumnName("total_electors");

                entity.Property(e => e.ValidVotes).HasColumnName("valid_votes");
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("people");

                entity.HasIndex(e => e.PersonId, "people_person_id_uindex")
                    .IsUnique();

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.AccountActive).HasColumnName("account_active");

                entity.Property(e => e.EmailAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("email_address");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(32)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(32)
                    .HasColumnName("last_name");

                entity.Property(e => e.PartyId).HasColumnName("party_id");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("phone_number");

                entity.Property(e => e.PwdHash)
                    .HasColumnType("character varying")
                    .HasColumnName("pwd_hash")
                    .HasDefaultValueSql("'E48EC0B9FD8FCF305CA1090E7A916AD5F8C1497A5F609B438D308CCB909A2384'::character varying");

                entity.Property(e => e.RidingId)
                    .HasColumnName("riding_id")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.SocialId).HasColumnName("social_id");

                entity.Property(e => e.Updated)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("now()");
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

                entity.Property(e => e.OfficeId).HasColumnName("office_id");

                entity.Property(e => e.RidingEmail)
                    .HasColumnType("character varying")
                    .HasColumnName("riding_email");

                entity.Property(e => e.RidingFax)
                    .HasColumnType("character varying")
                    .HasColumnName("riding_fax");

                entity.Property(e => e.RidingName)
                    .HasColumnType("character varying")
                    .HasColumnName("riding_name");

                entity.Property(e => e.RidingPhone)
                    .HasColumnType("character varying")
                    .HasColumnName("riding_phone");
            });

            modelBuilder.Entity<RoleList>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("role_list_pk");

                entity.ToTable("role_list");

                entity.HasIndex(e => e.RoleCode, "role_list_role_code_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.RoleId, "role_list_role_id_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.RoleTitle, "role_list_role_title_uindex")
                    .IsUnique();

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.RoleCode)
                    .HasMaxLength(2)
                    .HasColumnName("role_code");

                entity.Property(e => e.RoleGroup)
                    .HasMaxLength(2)
                    .HasColumnName("role_group");

                entity.Property(e => e.RoleTitle)
                    .HasColumnType("character varying")
                    .HasColumnName("role_title");
            });

            modelBuilder.Entity<SocialMediaList>(entity =>
            {
                entity.HasKey(e => e.SocialId)
                    .HasName("social_media_list_pk");

                entity.ToTable("social_media_list");

                entity.Property(e => e.SocialId)
                    .HasColumnName("social_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.FacebookId)
                    .HasColumnType("character varying")
                    .HasColumnName("facebook_id");

                entity.Property(e => e.FlickrId)
                    .HasColumnType("character varying")
                    .HasColumnName("flickr_id");

                entity.Property(e => e.InstagramId)
                    .HasColumnType("character varying")
                    .HasColumnName("instagram_id");

                entity.Property(e => e.SnapchatId)
                    .HasColumnType("character varying")
                    .HasColumnName("snapchat_id");

                entity.Property(e => e.TiktokId)
                    .HasColumnType("character varying")
                    .HasColumnName("tiktok_id");

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
                    .HasColumnName("updated")
                    .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<VoterList>(entity =>
            {
                entity.HasKey(e => e.VoterId)
                    .HasName("voter_list_pk");

                entity.ToTable("voter_list");

                entity.Property(e => e.VoterId)
                    .HasColumnName("voter_id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.BirthDate).HasColumnName("birth_date");

                entity.Property(e => e.City)
                    .HasColumnType("character varying")
                    .HasColumnName("city");

                entity.Property(e => e.EmailAddress)
                    .HasColumnType("character varying")
                    .HasColumnName("email_address");

                entity.Property(e => e.FirstName)
                    .HasColumnType("character varying")
                    .HasColumnName("first_name");

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.LastName)
                    .HasColumnType("character varying")
                    .HasColumnName("last_name");

                entity.Property(e => e.MiddleName)
                    .HasColumnType("character varying")
                    .HasColumnName("middle_name");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("phone_number");

                entity.Property(e => e.PostCode)
                    .HasMaxLength(7)
                    .HasColumnName("post_code");

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.RidingId).HasColumnName("riding_id");

                entity.Property(e => e.StreetName)
                    .HasColumnType("character varying")
                    .HasColumnName("street_name");

                entity.Property(e => e.StreetNumber).HasColumnName("street_number");

                entity.Property(e => e.UnitNumber)
                    .HasColumnType("character varying")
                    .HasColumnName("unit_number");

                entity.Property(e => e.VoteCast).HasColumnName("vote_cast");
            });

            modelBuilder.Entity<VoterSecurity>(entity =>
            {
                entity.HasKey(e => e.VoterId)
                    .HasName("voter_security_pk");

                entity.ToTable("voter_security");

                entity.HasIndex(e => e.CardId, "voter_security_card_id_uindex")
                    .IsUnique();

                entity.Property(e => e.VoterId)
                    .ValueGeneratedNever()
                    .HasColumnName("voter_id");

                entity.Property(e => e.CardId)
                    .HasMaxLength(12)
                    .HasColumnName("card_id");

                entity.Property(e => e.CardPin).HasColumnName("card_pin");

                entity.Property(e => e.EmailPin).HasColumnName("email_pin");

                entity.Property(e => e.Sin).HasColumnName("sin");

                entity.Property(e => e.Tax10100).HasColumnName("tax_10100");

                entity.Property(e => e.Tax12000).HasColumnName("tax_12000");

                entity.Property(e => e.Tax12900).HasColumnName("tax_12900");

                entity.Property(e => e.Tax14299).HasColumnName("tax_14299");

                entity.Property(e => e.Tax15000).HasColumnName("tax_15000");

                entity.Property(e => e.Tax23600).HasColumnName("tax_23600");

                entity.Property(e => e.Tax24400).HasColumnName("tax_24400");

                entity.Property(e => e.Tax26000).HasColumnName("tax_26000");

                entity.Property(e => e.Tax31220).HasColumnName("tax_31220");

                entity.Property(e => e.Tax58240).HasColumnName("tax_58240");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
