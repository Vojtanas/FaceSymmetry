CREATE DATABASE  IF NOT EXISTS `facesymmetry` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;
USE `facesymmetry`;
-- MySQL dump 10.13  Distrib 5.7.12, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: facesymmetry
-- ------------------------------------------------------
-- Server version	5.7.17-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `analysis_exercise`
--

DROP TABLE IF EXISTS `analysis_exercise`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `analysis_exercise` (
  `analysis_exerciseID` int(11) NOT NULL AUTO_INCREMENT,
  `examinationAnalysisID` int(11) NOT NULL,
  `area` varchar(45) NOT NULL,
  `mean` decimal(7,3) NOT NULL,
  `max` decimal(7,3) NOT NULL,
  `min` decimal(7,3) NOT NULL,
  `median` decimal(7,3) NOT NULL,
  `variance` decimal(7,3) NOT NULL,
  `std_dev` decimal(7,3) NOT NULL,
  PRIMARY KEY (`analysis_exerciseID`),
  KEY `examinationAnalysisID_idx` (`examinationAnalysisID`),
  CONSTRAINT `examinationAnalysisID` FOREIGN KEY (`examinationAnalysisID`) REFERENCES `examination_analysis` (`analysisID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=814 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `examination`
--

DROP TABLE IF EXISTS `examination`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `examination` (
  `examinationID` int(11) NOT NULL AUTO_INCREMENT,
  `dir` varchar(1000) CHARACTER SET utf8 DEFAULT NULL,
  `patientID` int(11) NOT NULL,
  `guid` varchar(150) CHARACTER SET utf8 NOT NULL,
  `created` datetime NOT NULL,
  `notes` text,
  `exercises` text,
  PRIMARY KEY (`examinationID`),
  UNIQUE KEY `guid_UNIQUE` (`guid`),
  KEY `examination_patientID_idx` (`patientID`),
  CONSTRAINT `examination_patientID` FOREIGN KEY (`patientID`) REFERENCES `patient` (`patientID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `examination_analysis`
--

DROP TABLE IF EXISTS `examination_analysis`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `examination_analysis` (
  `analysisID` int(11) NOT NULL AUTO_INCREMENT,
  `examinationID` int(11) NOT NULL,
  `count` int(11) NOT NULL,
  `step` decimal(15,13) NOT NULL,
  `interpolation` varchar(45) NOT NULL,
  `exercise` varchar(45) NOT NULL,
  `description` text,
  PRIMARY KEY (`analysisID`),
  KEY `examinationID_idx` (`examinationID`),
  CONSTRAINT `examinationAnalysis_examinationID` FOREIGN KEY (`examinationID`) REFERENCES `examination` (`examinationID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=272 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `exercise`
--

DROP TABLE IF EXISTS `exercise`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `exercise` (
  `excerciseID` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) CHARACTER SET utf8 NOT NULL,
  `description` text CHARACTER SET utf8,
  `active` tinyint(1) NOT NULL,
  PRIMARY KEY (`excerciseID`),
  UNIQUE KEY `name_UNIQUE` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `patient`
--

DROP TABLE IF EXISTS `patient`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `patient` (
  `patientID` int(11) NOT NULL AUTO_INCREMENT,
  `first_name` varchar(100) CHARACTER SET utf8 DEFAULT NULL,
  `second_name` varchar(100) CHARACTER SET utf8 DEFAULT NULL,
  `surname` varchar(100) CHARACTER SET utf8 NOT NULL,
  `date_of_birth` date DEFAULT NULL,
  `personal_id` varchar(100) CHARACTER SET utf8 DEFAULT NULL,
  `gender` enum('M','F','Male','Female','') CHARACTER SET utf8 DEFAULT NULL,
  `notes` mediumtext CHARACTER SET utf8,
  `guid` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`patientID`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2017-04-27 12:12:24
