IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [AboutSections] (
        [Id] int NOT NULL IDENTITY,
        [SectionKey] nvarchar(50) NOT NULL,
        [Title] nvarchar(150) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [ImagePath] nvarchar(250) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [LastModifiedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AboutSections] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [ContactQueries] (
        [Id] int NOT NULL IDENTITY,
        [SenderName] nvarchar(100) NOT NULL,
        [SenderEmail] nvarchar(100) NOT NULL,
        [SenderPhone] nvarchar(30) NULL,
        [Subject] nvarchar(150) NOT NULL,
        [Message] nvarchar(2000) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [Status] nvarchar(25) NOT NULL,
        [AdminNotes] nvarchar(1000) NULL,
        [UserId] nvarchar(450) NULL,
        CONSTRAINT [PK_ContactQueries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [Invitations] (
        [Id] int NOT NULL IDENTITY,
        [SenderUserId] nvarchar(450) NOT NULL,
        [RecipientEmail] nvarchar(100) NOT NULL,
        [InviteMessage] nvarchar(500) NULL,
        [SentAt] datetime2 NOT NULL,
        [DeliveryStatus] nvarchar(25) NOT NULL,
        CONSTRAINT [PK_Invitations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [NGOs] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [RegistrationNumber] nvarchar(50) NULL,
        [Description] nvarchar(1000) NULL,
        [Address] nvarchar(250) NULL,
        [Email] nvarchar(100) NULL,
        [Phone] nvarchar(30) NULL,
        [WebsiteUrl] nvarchar(250) NULL,
        [LogoPath] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NGOs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [Partners] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [PartnerType] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [WebsiteUrl] nvarchar(250) NULL,
        [LogoPath] nvarchar(250) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Partners] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [Programmes] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(150) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [EventDate] datetime2 NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [CoordinatorName] nvarchar(100) NULL,
        [CoordinatorContact] nvarchar(50) NULL,
        [BannerImagePath] nvarchar(250) NULL,
        [Status] nvarchar(25) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Programmes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [Causes] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(150) NOT NULL,
        [ShortDescription] nvarchar(300) NOT NULL,
        [FullDescription] nvarchar(max) NOT NULL,
        [TargetAmount] decimal(18,2) NOT NULL,
        [CurrentAmount] decimal(18,2) NOT NULL,
        [ImagePath] nvarchar(250) NULL,
        [IsFeatured] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [NgoId] int NOT NULL,
        CONSTRAINT [PK_Causes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Causes_NGOs_NgoId] FOREIGN KEY ([NgoId]) REFERENCES [NGOs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [GalleryImages] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(100) NOT NULL,
        [Caption] nvarchar(250) NULL,
        [AltText] nvarchar(150) NOT NULL,
        [FilePath] nvarchar(250) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [ProgrammeId] int NULL,
        CONSTRAINT [PK_GalleryImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GalleryImages_Programmes_ProgrammeId] FOREIGN KEY ([ProgrammeId]) REFERENCES [Programmes] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [ProgrammeInterests] (
        [Id] int NOT NULL IDENTITY,
        [ProgrammeId] int NOT NULL,
        [UserId] nvarchar(450) NULL,
        [ApplicantName] nvarchar(100) NOT NULL,
        [ApplicantEmail] nvarchar(100) NOT NULL,
        [ApplicantPhone] nvarchar(30) NOT NULL,
        [VolunteerNote] nvarchar(500) NULL,
        [Status] nvarchar(25) NOT NULL,
        [SubmissionDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ProgrammeInterests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProgrammeInterests_Programmes_ProgrammeId] FOREIGN KEY ([ProgrammeId]) REFERENCES [Programmes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE TABLE [Donations] (
        [Id] int NOT NULL IDENTITY,
        [TransactionReference] nvarchar(60) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [DonationDate] datetime2 NOT NULL,
        [DonorName] nvarchar(100) NOT NULL,
        [DonorEmail] nvarchar(100) NOT NULL,
        [DonorPhone] nvarchar(30) NULL,
        [PaymentStatus] nvarchar(25) NOT NULL,
        [IsSimulated] bit NOT NULL,
        [CauseId] int NOT NULL,
        [UserId] nvarchar(450) NULL,
        CONSTRAINT [PK_Donations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Donations_Causes_CauseId] FOREIGN KEY ([CauseId]) REFERENCES [Causes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AboutSections_SectionKey] ON [AboutSections] ([SectionKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Causes_NgoId] ON [Causes] ([NgoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Donations_CauseId] ON [Donations] ([CauseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Donations_TransactionReference] ON [Donations] ([TransactionReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GalleryImages_ProgrammeId] ON [GalleryImages] ([ProgrammeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProgrammeInterests_ProgrammeId_ApplicantEmail] ON [ProgrammeInterests] ([ProgrammeId], [ApplicantEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013091636_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013091636_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013093541_AddIdentity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013093541_AddIdentity', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014053310_AddProgrammeDatesAndAudit'
)
BEGIN
    ALTER TABLE [Programmes] ADD [EndDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014053310_AddProgrammeDatesAndAudit'
)
BEGIN
    ALTER TABLE [Programmes] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014053310_AddProgrammeDatesAndAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014053310_AddProgrammeDatesAndAudit', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015080938_AddQueryReplyFields'
)
BEGIN
    ALTER TABLE [ContactQueries] ADD [AdminReply] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015080938_AddQueryReplyFields'
)
BEGIN
    ALTER TABLE [ContactQueries] ADD [RepliedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015080938_AddQueryReplyFields'
)
BEGIN
    CREATE INDEX [IX_ContactQueries_UserId] ON [ContactQueries] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015080938_AddQueryReplyFields'
)
BEGIN
    ALTER TABLE [ContactQueries] ADD CONSTRAINT [FK_ContactQueries_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015080938_AddQueryReplyFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261015080938_AddQueryReplyFields', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015083642_AddDonationUserAndAuditFields'
)
BEGIN
    ALTER TABLE [Donations] ADD [PaymentNotes] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015083642_AddDonationUserAndAuditFields'
)
BEGIN
    ALTER TABLE [Donations] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015083642_AddDonationUserAndAuditFields'
)
BEGIN
    CREATE INDEX [IX_Donations_UserId] ON [Donations] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015083642_AddDonationUserAndAuditFields'
)
BEGIN
    ALTER TABLE [Donations] ADD CONSTRAINT [FK_Donations_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015083642_AddDonationUserAndAuditFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261015083642_AddDonationUserAndAuditFields', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015085841_AddProgrammeInterestFieldsAndUserRel'
)
BEGIN
    ALTER TABLE [ProgrammeInterests] ADD [AdminNotes] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015085841_AddProgrammeInterestFieldsAndUserRel'
)
BEGIN
    ALTER TABLE [ProgrammeInterests] ADD [InterestType] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015085841_AddProgrammeInterestFieldsAndUserRel'
)
BEGIN
    ALTER TABLE [ProgrammeInterests] ADD [Message] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015085841_AddProgrammeInterestFieldsAndUserRel'
)
BEGIN
    ALTER TABLE [ProgrammeInterests] ADD [ReviewedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015085841_AddProgrammeInterestFieldsAndUserRel'
)
BEGIN
    CREATE INDEX [IX_ProgrammeInterests_UserId] ON [ProgrammeInterests] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015085841_AddProgrammeInterestFieldsAndUserRel'
)
BEGIN
    ALTER TABLE [ProgrammeInterests] ADD CONSTRAINT [FK_ProgrammeInterests_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261015085841_AddProgrammeInterestFieldsAndUserRel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261015085841_AddProgrammeInterestFieldsAndUserRel', N'10.0.12');
END;

COMMIT;
GO

