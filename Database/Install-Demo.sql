-- Optional SSMS install. Single batch: never modifies an existing database.
USE master;
IF DB_ID(N'LingoCenterDemo') IS NOT NULL
    THROW 51000, 'Database already exists. Installation cancelled.', 1;
EXEC(N'CREATE DATABASE LingoCenterDemo');
EXEC(N'USE LingoCenterDemo;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N''dbo.Users'', N''U'') IS NOT NULL
    THROW 51001, ''Schema already exists; installation cancelled.'', 1;

create table [dbo].[Attendances] (
    [Id] [int] not null identity,
    [ClassSessionId] [int] not null,
    [EnrollmentId] [int] not null,
    [Status] [int] not null,
    primary key ([Id])
);
create table [dbo].[CenterClasses] (
    [Id] [int] not null identity,
    [Code] [nvarchar](30) not null,
    [CourseId] [int] not null,
    [TeacherId] [int] not null,
    [Room] [nvarchar](40) not null,
    [Capacity] [int] not null,
    [Status] [int] not null,
    primary key ([Id])
);
create table [dbo].[ClassSessions] (
    [Id] [int] not null identity,
    [CenterClassId] [int] not null,
    [StartsAt] [datetime] not null,
    [EndsAt] [datetime] not null,
    [Topic] [nvarchar](200) not null,
    primary key ([Id])
);
create table [dbo].[Courses] (
    [Id] [int] not null identity,
    [Name] [nvarchar](120) not null,
    [Level] [int] not null,
    [Description] [nvarchar](2000) not null,
    [DurationWeeks] [int] not null,
    [TuitionFee] [decimal](18, 0) not null,
    [Syllabus] [nvarchar](4000) not null,
    [IsActive] [bit] not null,
    primary key ([Id])
);
create table [dbo].[Enrollments] (
    [Id] [int] not null identity,
    [StudentId] [int] not null,
    [CenterClassId] [int] not null,
    [Status] [int] not null,
    [EnrolledAt] [datetime] not null,
    [DueDate] [datetime] not null,
    [AgreedFee] [decimal](18, 0) not null,
    [Midterm] [decimal](4, 2) null,
    [Final] [decimal](4, 2) null,
    [ScoresPublished] [bit] not null,
    primary key ([Id])
);
create table [dbo].[Notifications] (
    [Id] [int] not null identity,
    [UserId] [int] not null,
    [Message] [nvarchar](400) not null,
    [EventKey] [nvarchar](100) not null,
    [RecipientKey] [int] not null,
    [CreatedAt] [datetime] not null,
    [IsRead] [bit] not null,
    primary key ([Id])
);
create table [dbo].[Payments] (
    [Id] [int] not null identity,
    [EnrollmentId] [int] not null,
    [Amount] [decimal](18, 0) not null,
    [Status] [int] not null,
    [CreatedAt] [datetime] not null,
    [PaidAt] [datetime] null,
    [Reference] [nvarchar](120) not null,
    [ReceiptNo] [nvarchar](30) not null,
    [RecordedById] [int] null,
    primary key ([Id])
);
create table [dbo].[Users] (
    [Id] [int] not null identity,
    [FullName] [nvarchar](120) not null,
    [Email] [nvarchar](150) not null,
    [Phone] [nvarchar](30) null,
    [PasswordHash] [nvarchar](max) not null,
    [Role] [int] not null,
    [IsActive] [bit] not null,
    [FailedLogins] [int] not null,
    [LockedUntilUtc] [datetime] null,
    [SecurityStamp] [nvarchar](max) null,
    primary key ([Id])
);
alter table [dbo].[Attendances] add constraint [Attendance_ClassSession] foreign key ([ClassSessionId]) references [dbo].[ClassSessions]([Id]);
alter table [dbo].[Attendances] add constraint [Attendance_Enrollment] foreign key ([EnrollmentId]) references [dbo].[Enrollments]([Id]);
alter table [dbo].[CenterClasses] add constraint [CenterClass_Course] foreign key ([CourseId]) references [dbo].[Courses]([Id]);
alter table [dbo].[CenterClasses] add constraint [CenterClass_Teacher] foreign key ([TeacherId]) references [dbo].[Users]([Id]);
alter table [dbo].[ClassSessions] add constraint [ClassSession_CenterClass] foreign key ([CenterClassId]) references [dbo].[CenterClasses]([Id]);
alter table [dbo].[Enrollments] add constraint [Enrollment_CenterClass] foreign key ([CenterClassId]) references [dbo].[CenterClasses]([Id]);
alter table [dbo].[Enrollments] add constraint [Enrollment_Student] foreign key ([StudentId]) references [dbo].[Users]([Id]);
alter table [dbo].[Notifications] add constraint [Notification_User] foreign key ([UserId]) references [dbo].[Users]([Id]);
alter table [dbo].[Payments] add constraint [Payment_Enrollment] foreign key ([EnrollmentId]) references [dbo].[Enrollments]([Id]);
alter table [dbo].[Payments] add constraint [Payment_RecordedBy] foreign key ([RecordedById]) references [dbo].[Users]([Id]);


CREATE UNIQUE INDEX UX_UserEmail ON dbo.Users(Email);
CREATE UNIQUE INDEX UX_ClassCode ON dbo.CenterClasses(Code);
CREATE UNIQUE INDEX UX_StudentClass ON dbo.Enrollments(StudentId,CenterClassId);
CREATE UNIQUE INDEX UX_Attendance ON dbo.Attendances(ClassSessionId,EnrollmentId);
CREATE UNIQUE INDEX UX_ReceiptNo ON dbo.Payments(ReceiptNo);
CREATE UNIQUE INDEX UX_Notice ON dbo.Notifications(RecipientKey,EventKey);
CREATE INDEX IX_ClassTeacher ON dbo.CenterClasses(TeacherId);
CREATE INDEX IX_ClassCourse ON dbo.CenterClasses(CourseId);
CREATE INDEX IX_SessionClass ON dbo.ClassSessions(CenterClassId);
CREATE INDEX IX_EnrollmentClass ON dbo.Enrollments(CenterClassId);
CREATE INDEX IX_PaymentEnrollment ON dbo.Payments(EnrollmentId);
ALTER TABLE dbo.Payments ADD CONSTRAINT CK_Payment_Amount CHECK(Amount>0);
ALTER TABLE dbo.Enrollments ADD CONSTRAINT CK_Scores CHECK((Midterm IS NULL OR Midterm BETWEEN 0 AND 10) AND (Final IS NULL OR Final BETWEEN 0 AND 10));
ALTER TABLE dbo.ClassSessions ADD CONSTRAINT CK_Session_Time CHECK(EndsAt>StartsAt);
ALTER TABLE dbo.CenterClasses ADD CONSTRAINT CK_Capacity CHECK(Capacity>0);

DECLARE @Now datetime=DATEADD(hour,7,GETUTCDATE());
DECLARE @Monday datetime=DATEADD(day,-DATEDIFF(day,0,CONVERT(date,@Now))%7,CONVERT(datetime,CONVERT(date,@Now)));
SET IDENTITY_INSERT dbo.Users ON;
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (1,N''Nguyễn Minh Anh'',N''admin@lingocenter.test'',NULL,N''100000.AnDSeRkRnVovzzPPd2kZrw==.VNXoFhTkM8MWXolqo8/SU31RrJ4KThX7+y1Akcv/x/0='',2,1,0,N''0e20f93e-9470-45bd-9cc7-9480d2a92f3a'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (2,N''Sarah Nguyễn'',N''teacher1@lingocenter.test'',NULL,N''100000.owhplLcB85/6EcQPVC84RQ==.YaP6U2xpDab0r7OPodSCyoFnKVdmiED7OREX4w0zsh0='',1,1,0,N''243b6c51-2ebd-4a28-b58c-2434de993dc0'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (3,N''David Trần'',N''teacher2@lingocenter.test'',NULL,N''100000.UR8727kmp1fidLW/rt7WEw==.mgx/4ChygdcRVqLOBuIuRQF10UiMB7QlN6Fi8GJoD9w='',1,1,0,N''eba98a8b-fcd7-4f70-8970-9bf58bb91237'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (4,N''Emma Lê'',N''teacher3@lingocenter.test'',NULL,N''100000.0mj4CU0cNPDDN/FhAUDztw==.yD0QT8o/txUgC25Z0FrFKSYJTxvo3pmkuFbXdIX70rs='',1,1,0,N''4eaab7dd-5243-4760-9458-5ce6907c498c'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (5,N''James Phạm'',N''teacher4@lingocenter.test'',NULL,N''100000.GByILhX25k1NYB/xw6Mslw==.Z2oh6a9q1ciWFR0m2s13ihmJe/AV/BQxmlahJR4uTx8='',1,1,0,N''e4f1676b-e31a-49ae-93c8-62bd686f60a0'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (6,N''Olivia Võ'',N''teacher5@lingocenter.test'',NULL,N''100000.aB8n1BrIW20B3o8l4ckT0A==.K/k+KgR8edbskIIMDqRIoe/HDqu4EiV7XWLtuAVi7XA='',1,1,0,N''1669910a-8f03-4b52-913f-721f0a968a35'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (7,N''Nguyễn Hoàng An'',N''student1@lingocenter.test'',NULL,N''100000.Ydu/LsXtMVBamakcqOUmUQ==.wtlluqzryVGXO7Dvq/PZSioaB8W/HmxRr3ZT6tcLrDU='',0,1,0,N''cfd088a1-8b40-41c4-ba54-e5baab22b9b3'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (8,N''Trần Bảo Anh'',N''student2@lingocenter.test'',NULL,N''100000./rtGaS9NmlMwY0Xp/+japw==.4xIFat29Lqnnwal4B9g3zdQkxmo+s2YW215wYWRZF00='',0,1,0,N''ad617490-74ee-4ff0-918d-7c317f2a63e5'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (9,N''Lê Minh Châu'',N''student3@lingocenter.test'',NULL,N''100000.KGQ8AiOoIOb2y2b7ppyU4g==.N8Sc+f2nt/V4ebxjXw6Mp1Dawehud3PfHchT37twN/M='',0,1,0,N''5c0135e5-08fd-4d58-8f5f-6876386d5b03'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (10,N''Phạm Gia Hân'',N''student4@lingocenter.test'',NULL,N''100000.HnR87CB6Hi+1JC2B+h5u6A==.yqywhuafqH/HsIkB16pWqBAkWgaU6wEOwYEMTI/0R6c='',0,1,0,N''40213c96-9404-4bba-9ed0-6d66766d5daf'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (11,N''Võ Quốc Huy'',N''student5@lingocenter.test'',NULL,N''100000.Epfgfnzh1zjfiFBrj4y7Ow==.FQNonl2cxCNPz4XjMKh+HtAPPzRwgJ8NDsTbzBLFRj4='',0,1,0,N''379912e4-6de9-4dde-a198-680cae2c8e2e'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (12,N''Đặng Thảo Linh'',N''student6@lingocenter.test'',NULL,N''100000.n8Op70ugim/rPgm02fHriA==.JrGja936Pyh8cYZ+yDBKji8UQJapT5VEztt9VagnWuI='',0,1,0,N''ebfe5d3e-c1d4-44d6-bd49-843f532fe972'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (13,N''Bùi Nhật Minh'',N''student7@lingocenter.test'',NULL,N''100000.w6M+aZlXAHOtNAL/61NJ5Q==.aTarzauJt5WggKKuaByovTItxW/7oCdgfGZw7djxUok='',0,1,0,N''badb9ae8-9761-4b82-86d5-f8714fba8a64'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (14,N''Đỗ Khánh Ngọc'',N''student8@lingocenter.test'',NULL,N''100000.A0Fn3hyRhZOP2kzhgX8ezA==.q61omiVKvi1i8hiytTOh6gv7S/bASwZXl7QI3KllyM0='',0,1,0,N''48e4d845-68c0-44e1-b52e-377ddb8f6fb9'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (15,N''Hồ Anh Quân'',N''student9@lingocenter.test'',NULL,N''100000.Qzg1zO8uWNejWL8YE2qyjw==.xxJCrKwuGr5DvqavYBySYIpQVOeREJC8t20DyFsNSu4='',0,1,0,N''4d4c3394-4a06-4434-a8dd-36d8f216d914'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (16,N''Ngô Phương Thảo'',N''student10@lingocenter.test'',NULL,N''100000.MtxTUbNvfmsp5h3v61Gyvg==.holp3sgJV9KBP73ckua6TrI3SPypfA6V1OxTLt9NAjg='',0,1,0,N''834429b0-c9a9-4b12-8674-6aedffac6bd6'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (17,N''Dương Đức Trung'',N''student11@lingocenter.test'',NULL,N''100000.234TzDDla4zbIBKbjSfuUQ==.1pRSWzSpHnEkq0m6RKwkJiieXNXNlM5HnIcpCQH2CC0='',0,1,0,N''fd947920-81e9-4853-9915-5e31cc8bed86'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (18,N''Lý Ngọc Ánh'',N''student12@lingocenter.test'',NULL,N''100000.1P9zC0czKG6wDwkKVCH3mg==.13RphCpIO0uzpD7FQzgITxZOLxnJuq0mIRtmg4sjihY='',0,1,0,N''14348d38-c973-416b-8f5b-1385a5d2c348'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (19,N''Nguyễn Tuấn Kiệt'',N''student13@lingocenter.test'',NULL,N''100000.RXGeJAI1GWc9o5GGqJU1wQ==.RUedpYqhze3tZcXEBQt8YibN47M0q6ObfZcKDcLH0OA='',0,1,0,N''4c7f3f7c-00a4-43c9-a132-f6d31bd1592f'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (20,N''Trần Hà My'',N''student14@lingocenter.test'',NULL,N''100000.ACtAGkKEP6mmVbQpOr0YVQ==.sYRdrJk8eW1HR5ewff9YNO/LGIErV8oEfjasETCu5VQ='',0,1,0,N''a87ff35d-75dd-43e3-9ef6-83c94e461978'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (21,N''Lê Thanh Nam'',N''student15@lingocenter.test'',NULL,N''100000.o4EpwqEGsEnVf7NWOB9Xhw==.N9quKI1QvIe6xChA0qxWIjLvhrhqBSEWkAzBf9ojlXA='',0,1,0,N''b6e031ca-c52e-4b6e-ab84-54b1097f6308'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (22,N''Phạm Quỳnh Như'',N''student16@lingocenter.test'',NULL,N''100000.vPkFr+0h0I/Nbp4WlyQVhQ==.FeW32P2O58qCwKyZ6HVPYW8SgaJA35zcfCLyYuW46KI='',0,1,0,N''7ffc6de1-5c85-4e2c-9f29-c9f8be92aa77'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (23,N''Võ Hải Phong'',N''student17@lingocenter.test'',NULL,N''100000.ngqWh/gjuWqdKeGvntCLGg==.e3VqahsOCXoH/Fs1M43OL53F1WMNUPr4pnEXyafBdHo='',0,1,0,N''286d709d-baa5-4587-9846-1948d9cfc684'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (24,N''Đặng Yến Nhi'',N''student18@lingocenter.test'',NULL,N''100000.IGQwQt302PG2Lm6xPdfAwQ==.8kByl+G6xb5fB8dE3YntCIwdJYvieAm945QXiqw1vsc='',0,1,0,N''42fd104b-4daf-4809-af0f-7dcd094362c0'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (25,N''Bùi Đức Anh'',N''student19@lingocenter.test'',NULL,N''100000.w7+VTMN2pqNkhndvXy+GGA==.gdBNBUO2oPQjd4vybRPp4xGwC7YwMMrWwxo0Aso1dGQ='',0,1,0,N''4ec61da1-84c8-4b6e-98b0-0a700d3327b2'');
INSERT dbo.Users (Id,FullName,Email,Phone,PasswordHash,Role,IsActive,FailedLogins,SecurityStamp) VALUES (26,N''Đỗ Bảo Trân'',N''student20@lingocenter.test'',NULL,N''100000.5PRUwEhEkTKKqWTR75eFNw==.eyzg6ZgNCyLtGA3ZLVDNwX/RBTAWYErzb4vXRQJBVF0='',0,1,0,N''f8e2ad6a-62de-4858-a586-86e40f3eee15'');
SET IDENTITY_INSERT dbo.Users OFF;SET IDENTITY_INSERT dbo.Courses ON;
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (1,N''English Foundations'',0,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',8,2400000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (2,N''Everyday English'',1,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',9,2850000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (3,N''Confident Communication'',2,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',10,3300000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (4,N''English for Success'',3,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',11,3750000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (5,N''Advanced Fluency'',4,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',12,4200000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (6,N''IELTS Mastery 6.5+'',5,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',13,4650000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (7,N''TOEIC Accelerator 750+'',6,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',14,5100000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
INSERT dbo.Courses (Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive) VALUES (8,N''IELTS Foundation'',5,N''Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.'',15,5550000,N''Module 1: Vocabulary & pronunciation
Module 2: Listening & speaking
Module 3: Reading & writing
Module 4: Real-life projects
Midterm assessment (40%)
Final assessment (60%)'',1);
SET IDENTITY_INSERT dbo.Courses OFF;SET IDENTITY_INSERT dbo.CenterClasses ON;
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (1,N''LC-101'',1,2,N''Studio 01'',3,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (2,N''LC-102'',2,3,N''Studio 02'',18,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (3,N''LC-103'',3,4,N''Studio 03'',20,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (4,N''LC-104'',4,5,N''Studio 04'',16,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (5,N''LC-105'',5,6,N''Studio 05'',18,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (6,N''LC-106'',6,2,N''Studio 06'',20,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (7,N''LC-107'',7,3,N''Studio 07'',16,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (8,N''LC-108'',8,4,N''Studio 08'',18,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (9,N''LC-109'',1,5,N''Studio 09'',20,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (10,N''LC-110'',2,6,N''Studio 10'',16,0);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (11,N''LC-111'',3,2,N''Studio 11'',18,1);
INSERT dbo.CenterClasses (Id,Code,CourseId,TeacherId,Room,Capacity,Status) VALUES (12,N''LC-112'',4,3,N''Studio 12'',20,1);
SET IDENTITY_INSERT dbo.CenterClasses OFF;SET IDENTITY_INSERT dbo.Enrollments ON;
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (1,7,1,2,DATEADD(day,-40,@Now),DATEADD(day,-3,CONVERT(date,@Now)),2400000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (2,10,2,1,DATEADD(day,-40,@Now),DATEADD(day,-2,CONVERT(date,@Now)),2850000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (3,13,3,4,DATEADD(day,-39,@Now),DATEADD(day,-1,CONVERT(date,@Now)),3300000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (4,16,4,2,DATEADD(day,-39,@Now),DATEADD(day,0,CONVERT(date,@Now)),3750000,9,6,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (5,19,5,2,DATEADD(day,-38,@Now),DATEADD(day,1,CONVERT(date,@Now)),4200000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (6,22,6,2,DATEADD(day,-38,@Now),DATEADD(day,2,CONVERT(date,@Now)),4650000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (7,7,7,2,DATEADD(day,-37,@Now),DATEADD(day,3,CONVERT(date,@Now)),5100000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (8,10,8,0,DATEADD(day,-37,@Now),DATEADD(day,4,CONVERT(date,@Now)),5550000,9,6,0);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (9,13,9,1,DATEADD(day,-36,@Now),DATEADD(day,-3,CONVERT(date,@Now)),2400000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (10,16,10,4,DATEADD(day,-36,@Now),DATEADD(day,-2,CONVERT(date,@Now)),2850000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (11,19,11,3,DATEADD(day,-35,@Now),DATEADD(day,-1,CONVERT(date,@Now)),3300000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (12,22,12,3,DATEADD(day,-35,@Now),DATEADD(day,0,CONVERT(date,@Now)),3750000,9,6,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (13,8,1,2,DATEADD(day,-34,@Now),DATEADD(day,1,CONVERT(date,@Now)),2400000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (14,11,2,2,DATEADD(day,-34,@Now),DATEADD(day,2,CONVERT(date,@Now)),2850000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (15,14,3,0,DATEADD(day,-33,@Now),DATEADD(day,3,CONVERT(date,@Now)),3300000,8,9,0);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (16,17,4,1,DATEADD(day,-33,@Now),DATEADD(day,4,CONVERT(date,@Now)),3750000,9,6,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (17,20,5,4,DATEADD(day,-32,@Now),DATEADD(day,-3,CONVERT(date,@Now)),4200000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (18,23,6,2,DATEADD(day,-32,@Now),DATEADD(day,-2,CONVERT(date,@Now)),4650000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (19,8,7,2,DATEADD(day,-31,@Now),DATEADD(day,-1,CONVERT(date,@Now)),5100000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (20,11,8,2,DATEADD(day,-31,@Now),DATEADD(day,0,CONVERT(date,@Now)),5550000,9,6,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (21,14,9,2,DATEADD(day,-30,@Now),DATEADD(day,1,CONVERT(date,@Now)),2400000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (22,17,10,0,DATEADD(day,-30,@Now),DATEADD(day,2,CONVERT(date,@Now)),2850000,7,8,0);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (23,20,11,3,DATEADD(day,-29,@Now),DATEADD(day,3,CONVERT(date,@Now)),3300000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (24,23,12,3,DATEADD(day,-29,@Now),DATEADD(day,4,CONVERT(date,@Now)),3750000,9,6,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (25,9,1,2,DATEADD(day,-28,@Now),DATEADD(day,-3,CONVERT(date,@Now)),2400000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (26,12,2,2,DATEADD(day,-28,@Now),DATEADD(day,-2,CONVERT(date,@Now)),2850000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (27,15,3,2,DATEADD(day,-27,@Now),DATEADD(day,-1,CONVERT(date,@Now)),3300000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (28,18,4,2,DATEADD(day,-27,@Now),DATEADD(day,0,CONVERT(date,@Now)),3750000,9,6,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (29,21,5,0,DATEADD(day,-26,@Now),DATEADD(day,1,CONVERT(date,@Now)),4200000,6,7,0);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (30,24,6,1,DATEADD(day,-26,@Now),DATEADD(day,2,CONVERT(date,@Now)),4650000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (31,9,7,4,DATEADD(day,-25,@Now),DATEADD(day,3,CONVERT(date,@Now)),5100000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (32,12,8,2,DATEADD(day,-25,@Now),DATEADD(day,4,CONVERT(date,@Now)),5550000,9,6,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (33,15,9,2,DATEADD(day,-24,@Now),DATEADD(day,-3,CONVERT(date,@Now)),2400000,6,7,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (34,18,10,2,DATEADD(day,-24,@Now),DATEADD(day,-2,CONVERT(date,@Now)),2850000,7,8,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (35,21,11,3,DATEADD(day,-23,@Now),DATEADD(day,-1,CONVERT(date,@Now)),3300000,8,9,1);
INSERT dbo.Enrollments (Id,StudentId,CenterClassId,Status,EnrolledAt,DueDate,AgreedFee,Midterm,Final,ScoresPublished) VALUES (36,24,12,3,DATEADD(day,-23,@Now),DATEADD(day,0,CONVERT(date,@Now)),3750000,9,6,1);
SET IDENTITY_INSERT dbo.Enrollments OFF;SET IDENTITY_INSERT dbo.ClassSessions ON;
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (1,1,DATEADD(hour,8,DATEADD(day,-14,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-14,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (2,1,DATEADD(hour,8,DATEADD(day,-7,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-7,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (3,1,DATEADD(hour,8,DATEADD(day,0,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,0,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (4,1,DATEADD(hour,8,DATEADD(day,7,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,7,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (5,1,DATEADD(hour,8,DATEADD(day,14,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,14,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (6,1,DATEADD(hour,8,DATEADD(day,21,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,21,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (7,2,DATEADD(hour,8,DATEADD(day,-13,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-13,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (8,2,DATEADD(hour,8,DATEADD(day,-6,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-6,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (9,2,DATEADD(hour,8,DATEADD(day,1,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,1,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (10,2,DATEADD(hour,8,DATEADD(day,8,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,8,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (11,2,DATEADD(hour,8,DATEADD(day,15,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,15,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (12,2,DATEADD(hour,8,DATEADD(day,22,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,22,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (13,3,DATEADD(hour,8,DATEADD(day,-12,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-12,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (14,3,DATEADD(hour,8,DATEADD(day,-5,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-5,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (15,3,DATEADD(hour,8,DATEADD(day,2,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,2,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (16,3,DATEADD(hour,8,DATEADD(day,9,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,9,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (17,3,DATEADD(hour,8,DATEADD(day,16,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,16,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (18,3,DATEADD(hour,8,DATEADD(day,23,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,23,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (19,4,DATEADD(hour,8,DATEADD(day,-11,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-11,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (20,4,DATEADD(hour,8,DATEADD(day,-4,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-4,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (21,4,DATEADD(hour,8,DATEADD(day,3,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,3,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (22,4,DATEADD(hour,8,DATEADD(day,10,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,10,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (23,4,DATEADD(hour,8,DATEADD(day,17,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,17,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (24,4,DATEADD(hour,8,DATEADD(day,24,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,24,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (25,5,DATEADD(hour,8,DATEADD(day,-10,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-10,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (26,5,DATEADD(hour,8,DATEADD(day,-3,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-3,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (27,5,DATEADD(hour,8,DATEADD(day,4,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,4,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (28,5,DATEADD(hour,8,DATEADD(day,11,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,11,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (29,5,DATEADD(hour,8,DATEADD(day,18,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,18,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (30,5,DATEADD(hour,8,DATEADD(day,25,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,25,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (31,6,DATEADD(hour,8,DATEADD(day,-9,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-9,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (32,6,DATEADD(hour,8,DATEADD(day,-2,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,-2,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (33,6,DATEADD(hour,8,DATEADD(day,5,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,5,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (34,6,DATEADD(hour,8,DATEADD(day,12,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,12,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (35,6,DATEADD(hour,8,DATEADD(day,19,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,19,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (36,6,DATEADD(hour,8,DATEADD(day,26,@Monday)),DATEADD(hour,2,DATEADD(hour,8,DATEADD(day,26,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (37,7,DATEADD(hour,11,DATEADD(day,-14,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-14,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (38,7,DATEADD(hour,11,DATEADD(day,-7,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-7,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (39,7,DATEADD(hour,11,DATEADD(day,0,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,0,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (40,7,DATEADD(hour,11,DATEADD(day,7,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,7,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (41,7,DATEADD(hour,11,DATEADD(day,14,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,14,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (42,7,DATEADD(hour,11,DATEADD(day,21,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,21,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (43,8,DATEADD(hour,11,DATEADD(day,-13,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-13,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (44,8,DATEADD(hour,11,DATEADD(day,-6,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-6,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (45,8,DATEADD(hour,11,DATEADD(day,1,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,1,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (46,8,DATEADD(hour,11,DATEADD(day,8,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,8,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (47,8,DATEADD(hour,11,DATEADD(day,15,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,15,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (48,8,DATEADD(hour,11,DATEADD(day,22,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,22,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (49,9,DATEADD(hour,11,DATEADD(day,-12,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-12,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (50,9,DATEADD(hour,11,DATEADD(day,-5,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-5,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (51,9,DATEADD(hour,11,DATEADD(day,2,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,2,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (52,9,DATEADD(hour,11,DATEADD(day,9,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,9,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (53,9,DATEADD(hour,11,DATEADD(day,16,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,16,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (54,9,DATEADD(hour,11,DATEADD(day,23,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,23,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (55,10,DATEADD(hour,11,DATEADD(day,-11,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-11,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (56,10,DATEADD(hour,11,DATEADD(day,-4,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-4,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (57,10,DATEADD(hour,11,DATEADD(day,3,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,3,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (58,10,DATEADD(hour,11,DATEADD(day,10,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,10,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (59,10,DATEADD(hour,11,DATEADD(day,17,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,17,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (60,10,DATEADD(hour,11,DATEADD(day,24,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,24,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (61,11,DATEADD(hour,11,DATEADD(day,-10,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-10,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (62,11,DATEADD(hour,11,DATEADD(day,-3,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-3,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (63,11,DATEADD(hour,11,DATEADD(day,4,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,4,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (64,11,DATEADD(hour,11,DATEADD(day,11,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,11,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (65,11,DATEADD(hour,11,DATEADD(day,18,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,18,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (66,11,DATEADD(hour,11,DATEADD(day,25,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,25,@Monday))),N''Unit 6: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (67,12,DATEADD(hour,11,DATEADD(day,-9,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-9,@Monday))),N''Unit 1: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (68,12,DATEADD(hour,11,DATEADD(day,-2,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,-2,@Monday))),N''Unit 2: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (69,12,DATEADD(hour,11,DATEADD(day,5,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,5,@Monday))),N''Unit 3: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (70,12,DATEADD(hour,11,DATEADD(day,12,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,12,@Monday))),N''Unit 4: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (71,12,DATEADD(hour,11,DATEADD(day,19,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,19,@Monday))),N''Unit 5: Communication workshop'');
INSERT dbo.ClassSessions (Id,CenterClassId,StartsAt,EndsAt,Topic) VALUES (72,12,DATEADD(hour,11,DATEADD(day,26,@Monday)),DATEADD(hour,2,DATEADD(hour,11,DATEADD(day,26,@Monday))),N''Unit 6: Communication workshop'');
SET IDENTITY_INSERT dbo.ClassSessions OFF;
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (1,1,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (1,13,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (1,25,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (2,1,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (2,13,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (2,25,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (7,2,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (7,14,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (7,26,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (8,2,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (8,14,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (8,26,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (13,15,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (13,27,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (14,15,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (14,27,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (19,4,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (19,16,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (19,28,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (20,4,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (20,16,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (20,28,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (25,5,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (25,29,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (26,5,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (26,29,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (31,6,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (31,18,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (31,30,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (32,6,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (32,18,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (32,30,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (37,7,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (37,19,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (38,7,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (38,19,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (43,8,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (43,20,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (43,32,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (44,8,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (44,20,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (44,32,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (49,9,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (49,21,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (49,33,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (50,9,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (50,21,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (50,33,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (55,22,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (55,34,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (56,22,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (56,34,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (61,11,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (61,23,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (61,35,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (62,11,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (62,23,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (62,35,1);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (67,12,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (67,24,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (67,36,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (68,12,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (68,24,0);
INSERT dbo.Attendances (ClassSessionId,EnrollmentId,Status) VALUES (68,36,0);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (1,2400000,0,DATEADD(day,-60,@Now),NULL,N''DEMO-TRANSFER-001'',N''LC-DEMO-0001'',NULL);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (2,1425000,2,DATEADD(day,-58,@Now),NULL,N''DEMO-TRANSFER-002'',N''LC-DEMO-0002'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (3,3300000,1,DATEADD(day,-56,@Now),DATEADD(day,-56,@Now),N''DEMO-TRANSFER-003'',N''LC-DEMO-0003'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (4,1875000,1,DATEADD(day,-54,@Now),DATEADD(day,-54,@Now),N''DEMO-TRANSFER-004'',N''LC-DEMO-0004'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (5,4200000,1,DATEADD(day,-52,@Now),DATEADD(day,-52,@Now),N''DEMO-TRANSFER-005'',N''LC-DEMO-0005'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (6,2325000,1,DATEADD(day,-50,@Now),DATEADD(day,-50,@Now),N''DEMO-TRANSFER-006'',N''LC-DEMO-0006'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (7,5100000,0,DATEADD(day,-48,@Now),NULL,N''DEMO-TRANSFER-007'',N''LC-DEMO-0007'',NULL);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (8,2775000,2,DATEADD(day,-46,@Now),NULL,N''DEMO-TRANSFER-008'',N''LC-DEMO-0008'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (9,2400000,1,DATEADD(day,-44,@Now),DATEADD(day,-44,@Now),N''DEMO-TRANSFER-009'',N''LC-DEMO-0009'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (10,1425000,1,DATEADD(day,-42,@Now),DATEADD(day,-42,@Now),N''DEMO-TRANSFER-010'',N''LC-DEMO-0010'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (11,3300000,1,DATEADD(day,-40,@Now),DATEADD(day,-40,@Now),N''DEMO-TRANSFER-011'',N''LC-DEMO-0011'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (12,1875000,1,DATEADD(day,-38,@Now),DATEADD(day,-38,@Now),N''DEMO-TRANSFER-012'',N''LC-DEMO-0012'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (13,2400000,0,DATEADD(day,-36,@Now),NULL,N''DEMO-TRANSFER-013'',N''LC-DEMO-0013'',NULL);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (14,1425000,2,DATEADD(day,-34,@Now),NULL,N''DEMO-TRANSFER-014'',N''LC-DEMO-0014'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (15,3300000,1,DATEADD(day,-32,@Now),DATEADD(day,-32,@Now),N''DEMO-TRANSFER-015'',N''LC-DEMO-0015'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (16,1875000,1,DATEADD(day,-30,@Now),DATEADD(day,-30,@Now),N''DEMO-TRANSFER-016'',N''LC-DEMO-0016'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (17,4200000,1,DATEADD(day,-28,@Now),DATEADD(day,-28,@Now),N''DEMO-TRANSFER-017'',N''LC-DEMO-0017'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (18,2325000,1,DATEADD(day,-26,@Now),DATEADD(day,-26,@Now),N''DEMO-TRANSFER-018'',N''LC-DEMO-0018'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (19,5100000,0,DATEADD(day,-24,@Now),NULL,N''DEMO-TRANSFER-019'',N''LC-DEMO-0019'',NULL);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (20,2775000,2,DATEADD(day,-22,@Now),NULL,N''DEMO-TRANSFER-020'',N''LC-DEMO-0020'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (21,2400000,1,DATEADD(day,-20,@Now),DATEADD(day,-20,@Now),N''DEMO-TRANSFER-021'',N''LC-DEMO-0021'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (22,1425000,1,DATEADD(day,-18,@Now),DATEADD(day,-18,@Now),N''DEMO-TRANSFER-022'',N''LC-DEMO-0022'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (23,3300000,1,DATEADD(day,-16,@Now),DATEADD(day,-16,@Now),N''DEMO-TRANSFER-023'',N''LC-DEMO-0023'',1);
INSERT dbo.Payments (EnrollmentId,Amount,Status,CreatedAt,PaidAt,Reference,ReceiptNo,RecordedById) VALUES (24,1875000,1,DATEADD(day,-14,@Now),DATEADD(day,-14,@Now),N''DEMO-TRANSFER-024'',N''LC-DEMO-0024'',1);
COMMIT TRANSACTION;
SELECT N''LingoCenter ready'' AS Result;');
