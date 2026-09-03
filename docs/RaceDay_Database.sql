/*
========================================================
RaceDay Event Management System
Part 1 - SQL Database Script
========================================================
*/

-- ======================================================
-- 1. Create Database
-- ======================================================

IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

-- ======================================================
-- 2. Drop Existing Tables
-- This allows the script to be rerun cleanly.
-- Tables are dropped in reverse dependency order.
-- ======================================================

IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL
    DROP TABLE dbo.Results;
GO

IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrolments;
GO

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;
GO

IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL
    DROP TABLE dbo.Events;
GO

IF OBJECT_ID('dbo.Locations', 'U') IS NOT NULL
    DROP TABLE dbo.Locations;
GO

IF OBJECT_ID('dbo.EventTypes', 'U') IS NOT NULL
    DROP TABLE dbo.EventTypes;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

-- ======================================================
-- 3. Create Users Table
-- ======================================================

CREATE TABLE dbo.Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    PhoneNumber VARCHAR(20),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO

-- ======================================================
-- 4. Create EventTypes Table
-- ======================================================

CREATE TABLE dbo.EventTypes
(
    EventTypeID INT IDENTITY(1,1) PRIMARY KEY,
    EventTypeName VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255)
);
GO

-- ======================================================
-- 5. Create Locations Table
-- ======================================================

CREATE TABLE dbo.Locations
(
    LocationID INT IDENTITY(1,1) PRIMARY KEY,
    VenueName VARCHAR(100) NOT NULL,
    StreetAddress VARCHAR(150) NOT NULL,
    City VARCHAR(50) NOT NULL,
    Province VARCHAR(50) NOT NULL,
    PostalCode VARCHAR(10) NOT NULL
);
GO

-- ======================================================
-- 6. Create Events Table
-- ======================================================

CREATE TABLE dbo.Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    EventDate DATE NOT NULL,
    EventTypeID INT NOT NULL,
    LocationID INT NOT NULL,
    OrganiserID INT NOT NULL,

    CONSTRAINT FK_Events_EventTypes
        FOREIGN KEY (EventTypeID)
        REFERENCES dbo.EventTypes(EventTypeID),

    CONSTRAINT FK_Events_Locations
        FOREIGN KEY (LocationID)
        REFERENCES dbo.Locations(LocationID),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES dbo.Users(UserID)
);
GO

-- ======================================================
-- 7. Create Categories Table
-- ======================================================

CREATE TABLE dbo.Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(255),
    MaxParticipants INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID)
        REFERENCES dbo.Events(EventID),

    CONSTRAINT UQ_Categories_Event_Category
        UNIQUE (EventID, CategoryName),

    CONSTRAINT CK_Categories_MaxParticipants
        CHECK (MaxParticipants > 0),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0)
);
GO

-- ======================================================
-- 8. Create Enrolments Table
-- ======================================================

CREATE TABLE dbo.Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL DEFAULT 'Active',

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES dbo.Users(UserID),

    CONSTRAINT FK_Enrolments_Event
        FOREIGN KEY (EventID)
        REFERENCES dbo.Events(EventID),

    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryID)
        REFERENCES dbo.Categories(CategoryID),

    CONSTRAINT CK_Enrolments_Status
        CHECK (Status IN ('Active', 'Cancelled', 'Completed')),

    CONSTRAINT UQ_Enrolments_Participant_Event
        UNIQUE (ParticipantID, EventID)
);
GO

-- ======================================================
-- 9. Create Results Table
-- ======================================================

CREATE TABLE dbo.Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    FinishTime TIME,
    FinishingPosition INT,
    RecordedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentID)
        REFERENCES dbo.Enrolments(EnrolmentID),

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentID),

    CONSTRAINT CK_Results_FinishingPosition
        CHECK (
            FinishingPosition IS NULL
            OR FinishingPosition > 0
        )
);
GO

-- ======================================================
-- 10. Insert Event Types
-- ======================================================

INSERT INTO dbo.EventTypes
    (EventTypeName, Description)
VALUES
    ('Road Running',
     'Road running events including races and marathons.'),

    ('Walking',
     'Organised walking events for participants of different fitness levels.'),

    ('Cycling',
     'Road cycling events and races for cycling participants.');
GO

-- ======================================================
-- 11. Insert Locations
-- ======================================================

INSERT INTO dbo.Locations
    (VenueName, StreetAddress, City, Province, PostalCode)
VALUES
    ('Marks Park Sports Club',
     'Emmarentia Road',
     'Johannesburg',
     'Gauteng',
     '2195'),

    ('LC de Villiers Sports Grounds',
     'University Road',
     'Pretoria',
     'Gauteng',
     '0002'),

    ('Moses Mabhida Stadium',
     '44 Isaiah Ntshangase Road',
     'Durban',
     'KwaZulu-Natal',
     '4001');
GO

-- ======================================================
-- 12. Insert Users
-- ======================================================

INSERT INTO dbo.Users
    (FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
VALUES
    ('Thabo',
     'Mokoena',
     'thabo.mokoena@raceday.co.za',
     'HASH_Thabo123',
     'Organiser',
     '0825551001'),

    ('Lerato',
     'Dlamini',
     'lerato.dlamini@raceday.co.za',
     'HASH_Lerato123',
     'Organiser',
     '0835551002'),

    ('Sipho',
     'Ndlovu',
     'sipho.ndlovu@email.com',
     'HASH_Sipho123',
     'Participant',
     '0845551003'),

    ('Nomsa',
     'Khumalo',
     'nomsa.khumalo@email.com',
     'HASH_Nomsa123',
     'Participant',
     '0855551004');
GO

-- ======================================================
-- 13. Insert Events
-- ======================================================

INSERT INTO dbo.Events
    (EventName, Description, EventDate, EventTypeID,
     LocationID, OrganiserID)
VALUES
    ('Johannesburg City Run',
     'A road running event through Johannesburg for runners of different abilities.',
     '2026-10-18',
     1,
     1,
     1),

    ('Pretoria Spring Walk',
     'A community walking event promoting health and active lifestyles.',
     '2026-11-08',
     2,
     2,
     2),

    ('Durban Coastal Cycle',
     'A scenic road cycling event along the Durban coastline.',
     '2026-12-06',
     3,
     3,
     1);
GO

-- ======================================================
-- 14. Insert Categories
-- ======================================================

INSERT INTO dbo.Categories
    (EventID, CategoryName, Description,
     MaxParticipants, EntryFee)
VALUES
    (1,
     '10 km Open',
     '10 kilometre road running category for open participants.',
     500,
     150.00),

    (1,
     '21 km Half Marathon',
     'Half marathon road running category.',
     400,
     250.00),

    (2,
     '5 km Community Walk',
     '5 kilometre community walking category.',
     300,
     80.00),

    (2,
     '10 km Challenge Walk',
     '10 kilometre walking challenge for participants.',
     250,
     120.00),

    (3,
     '50 km Road Cycle',
     '50 kilometre road cycling category.',
     300,
     200.00),

    (3,
     '100 km Road Cycle',
     '100 kilometre road cycling category for experienced cyclists.',
     200,
     300.00);
GO

-- ======================================================
-- 15. Insert Enrolments
-- ======================================================

INSERT INTO dbo.Enrolments
    (ParticipantID, EventID, CategoryID, Status)
VALUES
    (3, 1, 1, 'Completed'),
    (4, 1, 2, 'Completed'),
    (3, 2, 3, 'Completed'),
    (4, 3, 5, 'Active');
GO

-- ======================================================
-- 16. Insert Results
-- ======================================================

INSERT INTO dbo.Results
    (EnrolmentID, FinishTime, FinishingPosition)
VALUES
    (1, '00:52:18', 47),
    (2, '01:48:42', 89),
    (3, '00:58:31', 35);
GO

-- ======================================================
-- 17. Final Verification
-- ======================================================

SELECT 'Organisers' AS DataType, COUNT(*) AS Total
FROM dbo.Users
WHERE Role = 'Organiser'

UNION ALL

SELECT 'Participants', COUNT(*)
FROM dbo.Users
WHERE Role = 'Participant'

UNION ALL

SELECT 'Events', COUNT(*)
FROM dbo.Events

UNION ALL

SELECT 'Categories', COUNT(*)
FROM dbo.Categories

UNION ALL

SELECT 'Enrolments', COUNT(*)
FROM dbo.Enrolments

UNION ALL

SELECT 'Results', COUNT(*)
FROM dbo.Results;
GO