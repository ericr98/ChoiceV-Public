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
-- Table structure for table `configanalysespots`
--

DROP TABLE IF EXISTS `configanalysespots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configanalysespots` (
  `id` int NOT NULL AUTO_INCREMENT,
  `position` text NOT NULL,
  `width` float NOT NULL,
  `height` float NOT NULL,
  `rotation` float NOT NULL,
  `trackVehicles` int NOT NULL,
  `codeItem` varchar(45) NOT NULL,
  `analyseEnd` datetime DEFAULT NULL,
  `inventoryId` int NOT NULL DEFAULT '-1',
  `data` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configanalysespots`
--

LOCK TABLES `configanalysespots` WRITE;
/*!40000 ALTER TABLE `configanalysespots` DISABLE KEYS */;
INSERT INTO `configanalysespots` VALUES (13,'{\"X\":831.1517,\"Y\":-1304.0571,\"Z\":20.231445}',1.07528,1.34595,0,0,'DnaSpot','2024-10-29 19:23:33',23403,NULL),(14,'{\"X\":830.5978,\"Y\":-1305.0725,\"Z\":20.231445}',1.48128,0.804606,0,0,'WeaponSpot','2024-10-29 19:24:06',23404,NULL),(18,'{\"X\":-423.142,\"Y\":6170.472,\"Z\":31.470337}',13.3918,8.6486,45.5818,1,'CarPaintSpot','2024-11-06 00:51:35',29983,NULL),(19,'{\"X\":436.39664,\"Y\":-1178.2407,\"Z\":29.380981}',18.8042,12.4931,0,1,'CarPaintSpot','2024-10-19 15:52:45',30010,NULL),(20,'{\"X\":-427.3749,\"Y\":5999.39,\"Z\":31.706177}',1.21061,1.21061,0,0,'DnaSpot',NULL,30489,NULL),(21,'{\"X\":-425.93405,\"Y\":5998.0483,\"Z\":31.706177}',1.48128,1.34595,44.4067,0,'WeaponSpot',NULL,30490,NULL);
/*!40000 ALTER TABLE `configanalysespots` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:08:28
