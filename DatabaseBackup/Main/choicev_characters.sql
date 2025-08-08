-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: game.choicev.net    Database: choicev
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
-- Table structure for table `characters`
--

DROP TABLE IF EXISTS `characters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characters` (
  `id` int NOT NULL AUTO_INCREMENT,
  `bankaccount` bigint NOT NULL DEFAULT '-1',
  `accountId` int NOT NULL DEFAULT '-1',
  `title` varchar(45) NOT NULL,
  `firstname` varchar(45) NOT NULL DEFAULT 'Joe',
  `middleNames` varchar(255) NOT NULL,
  `lastname` varchar(45) NOT NULL DEFAULT 'Doe',
  `age` int NOT NULL DEFAULT '0',
  `hunger` double NOT NULL DEFAULT '100',
  `thirst` double NOT NULL DEFAULT '100',
  `energy` double NOT NULL DEFAULT '100',
  `health` int NOT NULL DEFAULT '100',
  `birthdate` date NOT NULL DEFAULT '2008-01-02',
  `originState` varchar(45) NOT NULL,
  `dead` int NOT NULL DEFAULT '0',
  `position` varchar(255) NOT NULL DEFAULT '{"X":0,"Y":0,"Z":0}',
  `rotation` varchar(255) NOT NULL DEFAULT '{"X":0.0,"Y":0.0,"Z":0.0}',
  `gender` char(1) NOT NULL DEFAULT 'M',
  `jobData` mediumtext,
  `cash` decimal(10,2) NOT NULL DEFAULT '0.00',
  `state` int NOT NULL DEFAULT '0',
  `stateUpdateDate` datetime DEFAULT NULL,
  `lastLogin` datetime NOT NULL DEFAULT '2008-01-02 00:00:00',
  `lastLogout` datetime NOT NULL DEFAULT '2008-01-02 00:00:00',
  `currentPlayTime` int NOT NULL DEFAULT '0',
  `currentPlayTimeDate` datetime NOT NULL DEFAULT '2008-01-02 00:00:00',
  `socialSecurityNumber` varchar(20) NOT NULL DEFAULT '-1',
  `dimension` int NOT NULL DEFAULT '0',
  `island` int NOT NULL DEFAULT '0',
  `loginToken` varchar(45) DEFAULT NULL,
  `flag` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `fk_accounts_characters_accountId_idx` (`accountId`),
  CONSTRAINT `fk_accounts_characters_accountId` FOREIGN KEY (`accountId`) REFERENCES `accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1160 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characters`
--

LOCK TABLES `characters` WRITE;
/*!40000 ALTER TABLE `characters` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:53
