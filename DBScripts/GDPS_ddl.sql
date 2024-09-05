CREATE DATABASE JobPortalGDPS;

USE JobPortalGDPS;

-- Table to store users
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    PhoneNumber NVARCHAR(20)
);

-- Table to store job postings
CREATE TABLE Jobs (
    JobId INT PRIMARY KEY IDENTITY,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL
);

-- Table to store applications
CREATE TABLE Applications (
    ApplicationId INT PRIMARY KEY IDENTITY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    JobId INT FOREIGN KEY REFERENCES Jobs(JobId),
    Resume VARBINARY(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);