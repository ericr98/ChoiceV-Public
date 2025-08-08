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
-- Table structure for table `systemstotabs`
--

DROP TABLE IF EXISTS `systemstotabs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `systemstotabs` (
  `systemId` int NOT NULL,
  `tabsName` varchar(100) NOT NULL,
  PRIMARY KEY (`systemId`,`tabsName`),
  KEY `fk_systemstotabs_tabName_idx` (`tabsName`),
  CONSTRAINT `fk_systemstotabs_systemId` FOREIGN KEY (`systemId`) REFERENCES `systems` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_systemstotabs_tabName` FOREIGN KEY (`tabsName`) REFERENCES `tabs` (`codeName`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `systemstotabs`
--

LOCK TABLES `systemstotabs` WRITE;
/*!40000 ALTER TABLE `systemstotabs` DISABLE KEYS */;
INSERT INTO `systemstotabs` VALUES (1,'companies/bulletinBoard'),(2,'companies/bulletinBoard'),(3,'companies/bulletinBoard'),(4,'companies/bulletinBoard'),(5,'companies/bulletinBoard'),(6,'companies/bulletinBoard'),(9,'companies/bulletinBoard'),(10,'companies/bulletinBoard'),(11,'companies/bulletinBoard'),(12,'companies/bulletinBoard'),(13,'companies/bulletinBoard'),(14,'companies/bulletinBoard'),(15,'companies/bulletinBoard'),(16,'companies/bulletinBoard'),(17,'companies/bulletinBoard'),(18,'companies/bulletinBoard'),(19,'companies/bulletinBoard'),(20,'companies/bulletinBoard'),(21,'companies/bulletinBoard'),(22,'companies/bulletinBoard'),(23,'companies/bulletinBoard'),(25,'companies/bulletinBoard'),(26,'companies/bulletinBoard'),(28,'companies/bulletinBoard'),(29,'companies/bulletinBoard'),(30,'companies/bulletinBoard'),(31,'companies/bulletinBoard'),(32,'companies/bulletinBoard'),(23,'companies/companyFileSelect'),(25,'companies/companyFileSelect'),(26,'companies/companyFileSelect'),(28,'companies/companyFileSelect'),(29,'companies/companyFileSelect'),(30,'companies/companyFileSelect'),(31,'companies/companyFileSelect'),(4,'companies/companyFilesList'),(5,'companies/companyFilesList'),(9,'companies/companyFilesList'),(10,'companies/companyFilesList'),(11,'companies/companyFilesList'),(12,'companies/companyFilesList'),(13,'companies/companyFilesList'),(14,'companies/companyFilesList'),(15,'companies/companyFilesList'),(16,'companies/companyFilesList'),(17,'companies/companyFilesList'),(18,'companies/companyFilesList'),(19,'companies/companyFilesList'),(20,'companies/companyFilesList'),(21,'companies/companyFilesList'),(22,'companies/companyFilesList'),(23,'companies/companyFilesList'),(25,'companies/companyFilesList'),(26,'companies/companyFilesList'),(28,'companies/companyFilesList'),(29,'companies/companyFilesList'),(30,'companies/companyFilesList'),(31,'companies/companyFilesList'),(4,'companies/customerSelect'),(5,'companies/customerSelect'),(9,'companies/customerSelect'),(10,'companies/customerSelect'),(11,'companies/customerSelect'),(12,'companies/customerSelect'),(13,'companies/customerSelect'),(14,'companies/customerSelect'),(15,'companies/customerSelect'),(16,'companies/customerSelect'),(17,'companies/customerSelect'),(18,'companies/customerSelect'),(19,'companies/customerSelect'),(20,'companies/customerSelect'),(21,'companies/customerSelect'),(22,'companies/customerSelect'),(23,'companies/customerSelect'),(25,'companies/customerSelect'),(26,'companies/customerSelect'),(28,'companies/customerSelect'),(29,'companies/customerSelect'),(30,'companies/customerSelect'),(31,'companies/customerSelect'),(4,'companies/customersList'),(5,'companies/customersList'),(9,'companies/customersList'),(10,'companies/customersList'),(11,'companies/customersList'),(12,'companies/customersList'),(13,'companies/customersList'),(14,'companies/customersList'),(15,'companies/customersList'),(16,'companies/customersList'),(17,'companies/customersList'),(18,'companies/customersList'),(19,'companies/customersList'),(20,'companies/customersList'),(21,'companies/customersList'),(22,'companies/customersList'),(23,'companies/customersList'),(25,'companies/customersList'),(26,'companies/customersList'),(28,'companies/customersList'),(29,'companies/customersList'),(30,'companies/customersList'),(31,'companies/customersList'),(2,'companies/documentsList'),(3,'companies/documentsList'),(4,'companies/documentsList'),(5,'companies/documentsList'),(6,'companies/documentsList'),(9,'companies/documentsList'),(10,'companies/documentsList'),(11,'companies/documentsList'),(12,'companies/documentsList'),(13,'companies/documentsList'),(14,'companies/documentsList'),(15,'companies/documentsList'),(16,'companies/documentsList'),(17,'companies/documentsList'),(18,'companies/documentsList'),(19,'companies/documentsList'),(20,'companies/documentsList'),(21,'companies/documentsList'),(22,'companies/documentsList'),(23,'companies/documentsList'),(25,'companies/documentsList'),(26,'companies/documentsList'),(28,'companies/documentsList'),(29,'companies/documentsList'),(30,'companies/documentsList'),(31,'companies/documentsList'),(4,'companies/employeeSelect'),(5,'companies/employeeSelect'),(9,'companies/employeeSelect'),(10,'companies/employeeSelect'),(11,'companies/employeeSelect'),(12,'companies/employeeSelect'),(13,'companies/employeeSelect'),(14,'companies/employeeSelect'),(15,'companies/employeeSelect'),(16,'companies/employeeSelect'),(17,'companies/employeeSelect'),(18,'companies/employeeSelect'),(19,'companies/employeeSelect'),(20,'companies/employeeSelect'),(21,'companies/employeeSelect'),(22,'companies/employeeSelect'),(23,'companies/employeeSelect'),(29,'companies/employeeSelect'),(30,'companies/employeeSelect'),(31,'companies/employeeSelect'),(2,'companies/employeesList'),(3,'companies/employeesList'),(4,'companies/employeesList'),(5,'companies/employeesList'),(6,'companies/employeesList'),(9,'companies/employeesList'),(10,'companies/employeesList'),(11,'companies/employeesList'),(12,'companies/employeesList'),(13,'companies/employeesList'),(14,'companies/employeesList'),(15,'companies/employeesList'),(16,'companies/employeesList'),(17,'companies/employeesList'),(18,'companies/employeesList'),(19,'companies/employeesList'),(20,'companies/employeesList'),(21,'companies/employeesList'),(22,'companies/employeesList'),(23,'companies/employeesList'),(25,'companies/employeesList'),(26,'companies/employeesList'),(28,'companies/employeesList'),(29,'companies/employeesList'),(30,'companies/employeesList'),(31,'companies/employeesList'),(32,'companies/employeesList'),(2,'companies/executive/executiveControlCenter'),(3,'companies/executive/executiveControlCenter'),(4,'companies/executive/executiveControlCenter'),(5,'companies/executive/executiveControlCenter'),(23,'companies/executive/executiveControlCenter'),(23,'companies/executive/executiveFileSelect'),(26,'companies/executive/executiveFileSelect'),(2,'companies/executive/executiveFilesList'),(3,'companies/executive/executiveFilesList'),(6,'companies/executive/executiveFilesList'),(23,'companies/executive/executiveFilesList'),(26,'companies/executive/executiveFilesList'),(23,'companies/executive/executivePersonFileSelect'),(2,'companies/executive/executivePersonFilesList'),(3,'companies/executive/executivePersonFilesList'),(6,'companies/executive/executivePersonFilesList'),(11,'companies/executive/executivePersonFilesList'),(23,'companies/executive/executivePersonFilesList'),(11,'companies/executive/executiveVehicleSelect'),(23,'companies/executive/executiveVehicleSelect'),(2,'companies/executive/executiveVehiclesList'),(3,'companies/executive/executiveVehiclesList'),(6,'companies/executive/executiveVehiclesList'),(11,'companies/executive/executiveVehiclesList'),(23,'companies/executive/executiveVehiclesList'),(2,'companies/logList'),(3,'companies/logList'),(4,'companies/logList'),(5,'companies/logList'),(6,'companies/logList'),(9,'companies/logList'),(10,'companies/logList'),(11,'companies/logList'),(12,'companies/logList'),(13,'companies/logList'),(14,'companies/logList'),(15,'companies/logList'),(16,'companies/logList'),(17,'companies/logList'),(18,'companies/logList'),(19,'companies/logList'),(20,'companies/logList'),(21,'companies/logList'),(22,'companies/logList'),(23,'companies/logList'),(26,'companies/logList'),(28,'companies/logList'),(29,'companies/logList'),(30,'companies/logList'),(31,'companies/logList'),(23,'companies/rankSelect'),(28,'companies/rankSelect'),(30,'companies/rankSelect'),(31,'companies/rankSelect'),(1,'companies/ranksList'),(2,'companies/ranksList'),(3,'companies/ranksList'),(4,'companies/ranksList'),(5,'companies/ranksList'),(6,'companies/ranksList'),(9,'companies/ranksList'),(10,'companies/ranksList'),(11,'companies/ranksList'),(12,'companies/ranksList'),(13,'companies/ranksList'),(14,'companies/ranksList'),(15,'companies/ranksList'),(16,'companies/ranksList'),(17,'companies/ranksList'),(18,'companies/ranksList'),(19,'companies/ranksList'),(20,'companies/ranksList'),(21,'companies/ranksList'),(22,'companies/ranksList'),(23,'companies/ranksList'),(25,'companies/ranksList'),(26,'companies/ranksList'),(28,'companies/ranksList'),(29,'companies/ranksList'),(30,'companies/ranksList'),(31,'companies/ranksList'),(32,'companies/ranksList'),(1,'support/supportDashboard'),(1,'support/supportFilesList'),(1,'support/supportPlayerList');
/*!40000 ALTER TABLE `systemstotabs` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:06:09
