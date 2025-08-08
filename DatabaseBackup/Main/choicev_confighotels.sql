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
-- Table structure for table `confighotels`
--

DROP TABLE IF EXISTS `confighotels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `confighotels` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `phoneNumber` bigint DEFAULT NULL,
  `bankAccount` bigint DEFAULT NULL,
  `checkInCollPosition` text NOT NULL,
  `checkInCollHeight` float NOT NULL,
  `checkInCollWidth` float NOT NULL,
  `checkInCollRotation` float NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_confighotels_bankAccount_idx` (`bankAccount`),
  KEY `fk_confighotels_phoneNumber_idx` (`phoneNumber`),
  CONSTRAINT `fk_confighotels_bankAccount` FOREIGN KEY (`bankAccount`) REFERENCES `bankaccounts` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_confighotels_phoneNumber` FOREIGN KEY (`phoneNumber`) REFERENCES `phonenumbers` (`number`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `confighotels`
--

LOCK TABLES `confighotels` WRITE;
/*!40000 ALTER TABLE `confighotels` DISABLE KEYS */;
INSERT INTO `confighotels` VALUES (3,'Wisdahl Hotel',12100924338,655190960,'{\"X\":56.16044,\"Y\":-1010.6901,\"Z\":28.330444}',3,5.2,17.6),(4,'Bonds Hotel',12100798485,655638407,'{\"X\":329.3011,\"Y\":-808.4176,\"Z\":29.414673}',0,0,0),(6,'The Chicken Coop',12100796393,655345118,'{\"X\":-149.92088,\"Y\":6300.923,\"Z\":31.470337}',0,0,0);
/*!40000 ALTER TABLE `confighotels` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:08:41
