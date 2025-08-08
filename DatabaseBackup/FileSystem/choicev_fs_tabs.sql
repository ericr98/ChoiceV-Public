-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: game.choicev.net    Database: choicev_fs
-- ------------------------------------------------------
-- Server version	8.0.25

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `tabs`
--

DROP TABLE IF EXISTS `tabs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tabs` (
  `codeName` varchar(100) NOT NULL,
  `permission` varchar(45) DEFAULT NULL,
  `sideBarName` varchar(45) DEFAULT NULL,
  `isStartTab` int NOT NULL DEFAULT '0',
  `parentTab` varchar(100) DEFAULT NULL,
  `order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`codeName`),
  KEY `fk_tabs_parentTab_idx` (`parentTab`),
  KEY `fk_tabs_permission_idx` (`permission`),
  CONSTRAINT `fk_tabs_parentTab` FOREIGN KEY (`parentTab`) REFERENCES `tabs` (`codeName`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_tabs_permission` FOREIGN KEY (`permission`) REFERENCES `permission_single_permissions` (`identifier`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tabs`
--

LOCK TABLES `tabs` WRITE;
/*!40000 ALTER TABLE `tabs` DISABLE KEYS */;
INSERT INTO `tabs` VALUES ('companies/bulletinBoard',NULL,'Schwarzes Brett',1,NULL,0),('companies/companyFileSelect','FILES_ACCESS',NULL,0,'companies/companyFilesList',-1),('companies/companyFilesList','FILES_ACCESS','Auftragsbuch',0,NULL,2),('companies/customerSelect',NULL,NULL,0,'companies/customersList',-1),('companies/customersList',NULL,'Kundenkartei',0,NULL,3),('companies/documentsList',NULL,'Dokumente',0,NULL,1),('companies/employeeSelect',NULL,NULL,0,'companies/employeesList',-1),('companies/employeesList','EMPLOYEE_ACCESS','Mitarbeiterverwaltung',0,NULL,10),('companies/executive/executiveControlCenter',NULL,'Leitstellenblatt',0,NULL,3),('companies/executive/executiveFileSelect','FILES_ACCESS',NULL,0,'companies/executive/executiveFilesList',-1),('companies/executive/executiveFilesList','FILES_ACCESS','Fallakten',0,NULL,2),('companies/executive/executivePersonFileSelect','FILES_ACCESS',NULL,0,'companies/executive/executivePersonFilesList',-1),('companies/executive/executivePersonFilesList','FILES_ACCESS','Personenakten',0,NULL,2),('companies/executive/executiveVehicleSelect',NULL,NULL,0,'companies/executive/executiveVehiclesList',-1),('companies/executive/executiveVehiclesList',NULL,'Fahrzeugakten',0,NULL,2),('companies/logList','LOGS_ACCESS','Logs',0,NULL,11),('companies/rankSelect','RANK_ACCESS',NULL,0,'companies/ranksList',-1),('companies/ranksList','RANK_ACCESS','Rangeinstellungen',0,NULL,10),('support/supportCharacterSelect',NULL,NULL,0,'support/supportPlayerList',-1),('support/supportDashboard',NULL,'Dashboard',1,NULL,0),('support/supportFilesList',NULL,'Supportakten',0,NULL,1),('support/supportFilesSelect',NULL,NULL,0,'support/supportFilesList',-1),('support/supportPlayerList',NULL,'Spielerakten',0,NULL,1),('support/supportPlayerSelect',NULL,NULL,0,'support/supportPlayerList',-1);
/*!40000 ALTER TABLE `tabs` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:06:45
