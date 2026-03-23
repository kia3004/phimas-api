-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Mar 22, 2026 at 01:17 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `phimas`
--

-- --------------------------------------------------------

--
-- Table structure for table `health_records`
--

CREATE TABLE `health_records` (
  `RecordID` int(11) NOT NULL,
  `PatientID` int(11) NOT NULL,
  `BHWID` int(11) NOT NULL,
  `DateRecorded` date NOT NULL,
  `Disease` varchar(100) NOT NULL,
  `Symptoms` varchar(255) NOT NULL,
  `Status` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `households`
--

CREATE TABLE `households` (
  `HouseholdID` int(11) NOT NULL,
  `Address` varchar(200) NOT NULL,
  `RiskScore` decimal(5,2) NOT NULL DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `household_members`
--

CREATE TABLE `household_members` (
  `PatientID` int(11) NOT NULL,
  `HouseholdID` int(11) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `ContactNumber` varchar(15) NOT NULL,
  `IsEmergencyContact` tinyint(1) NOT NULL DEFAULT 0,
  `EmergencyContactHouseholdID` int(11) GENERATED ALWAYS AS (case when `IsEmergencyContact` = 1 then `HouseholdID` else NULL end) STORED
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `inventory`
--

CREATE TABLE `inventory` (
  `ItemID` int(11) NOT NULL,
  `ItemName` varchar(100) NOT NULL,
  `Unit` varchar(50) NOT NULL,
  `MinimumThreshold` int(11) NOT NULL,
  `CurrentStock` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `predictive_analysis`
--

CREATE TABLE `predictive_analysis` (
  `AnalyticsID` int(11) NOT NULL,
  `DateGenerated` date NOT NULL,
  `Disease` varchar(100) NOT NULL,
  `PredictedCases` int(11) NOT NULL,
  `HighRiskBarangay` varchar(100) NOT NULL,
  `ConfidenceScore` decimal(5,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `reports`
--

CREATE TABLE `reports` (
  `ReportID` int(11) NOT NULL,
  `DateGenerated` date NOT NULL,
  `GeneratedBy` int(11) NOT NULL,
  `PatientID` int(11) NOT NULL,
  `ReportType` varchar(50) NOT NULL,
  `Content` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `task_assignments`
--

CREATE TABLE `task_assignments` (
  `TaskAssignmentID` int(11) NOT NULL,
  `BHWID` int(11) NOT NULL,
  `HouseholdID` int(11) NOT NULL,
  `TaskDate` date NOT NULL,
  `Status` varchar(50) NOT NULL,
  `Priority` varchar(20) NOT NULL,
  `Description` varchar(200) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `UserID` int(11) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `Role` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `ContactNumber` varchar(15) NOT NULL,
  `IsAvailable` tinyint(1) NOT NULL DEFAULT 0,
  `ProfilePicture` varchar(200) DEFAULT NULL,
  `AssignedArea` varchar(200) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `health_records`
--
ALTER TABLE `health_records`
  ADD PRIMARY KEY (`RecordID`),
  ADD KEY `idx_health_records_patient` (`PatientID`),
  ADD KEY `idx_health_records_bhw` (`BHWID`);

--
-- Indexes for table `households`
--
ALTER TABLE `households`
  ADD PRIMARY KEY (`HouseholdID`),
  ADD KEY `idx_households_address` (`Address`);

--
-- Indexes for table `household_members`
--
ALTER TABLE `household_members`
  ADD PRIMARY KEY (`PatientID`),
  ADD UNIQUE KEY `uq_household_members_person` (`HouseholdID`,`FullName`,`ContactNumber`),
  ADD UNIQUE KEY `uq_household_members_emergency_contact` (`EmergencyContactHouseholdID`),
  ADD KEY `idx_household_members_household` (`HouseholdID`);

--
-- Indexes for table `inventory`
--
ALTER TABLE `inventory`
  ADD PRIMARY KEY (`ItemID`);

--
-- Indexes for table `predictive_analysis`
--
ALTER TABLE `predictive_analysis`
  ADD PRIMARY KEY (`AnalyticsID`);

--
-- Indexes for table `reports`
--
ALTER TABLE `reports`
  ADD PRIMARY KEY (`ReportID`),
  ADD KEY `idx_reports_generated_by` (`GeneratedBy`),
  ADD KEY `idx_reports_patient` (`PatientID`);

--
-- Indexes for table `task_assignments`
--
ALTER TABLE `task_assignments`
  ADD PRIMARY KEY (`TaskAssignmentID`),
  ADD KEY `idx_task_assignments_bhw` (`BHWID`),
  ADD KEY `idx_task_assignments_household` (`HouseholdID`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`UserID`),
  ADD UNIQUE KEY `uq_users_email` (`Email`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `health_records`
--
ALTER TABLE `health_records`
  MODIFY `RecordID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `households`
--
ALTER TABLE `households`
  MODIFY `HouseholdID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `household_members`
--
ALTER TABLE `household_members`
  MODIFY `PatientID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `inventory`
--
ALTER TABLE `inventory`
  MODIFY `ItemID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `predictive_analysis`
--
ALTER TABLE `predictive_analysis`
  MODIFY `AnalyticsID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `reports`
--
ALTER TABLE `reports`
  MODIFY `ReportID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `task_assignments`
--
ALTER TABLE `task_assignments`
  MODIFY `TaskAssignmentID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `UserID` int(11) NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `health_records`
--
ALTER TABLE `health_records`
  ADD CONSTRAINT `fk_health_records_bhw` FOREIGN KEY (`BHWID`) REFERENCES `users` (`UserID`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_health_records_patient` FOREIGN KEY (`PatientID`) REFERENCES `household_members` (`PatientID`) ON UPDATE CASCADE;

--
-- Constraints for table `household_members`
--
ALTER TABLE `household_members`
  ADD CONSTRAINT `fk_household_members_household` FOREIGN KEY (`HouseholdID`) REFERENCES `households` (`HouseholdID`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `reports`
--
ALTER TABLE `reports`
  ADD CONSTRAINT `fk_reports_generated_by` FOREIGN KEY (`GeneratedBy`) REFERENCES `users` (`UserID`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_reports_patient` FOREIGN KEY (`PatientID`) REFERENCES `household_members` (`PatientID`) ON UPDATE CASCADE;

--
-- Constraints for table `task_assignments`
--
ALTER TABLE `task_assignments`
  ADD CONSTRAINT `fk_task_assignments_bhw` FOREIGN KEY (`BHWID`) REFERENCES `users` (`UserID`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_task_assignments_household` FOREIGN KEY (`HouseholdID`) REFERENCES `households` (`HouseholdID`) ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
