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
-- Table structure for table `configprisons`
--

DROP TABLE IF EXISTS `configprisons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configprisons` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `outlines` text,
  `registerSpots` text,
  `belongingsSpots` text,
  `shops` text,
  `getOutPoints` text,
  `npcGuardPed` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configprisons`
--

LOCK TABLES `configprisons` WRITE;
/*!40000 ALTER TABLE `configprisons` DISABLE KEYS */;
INSERT INTO `configprisons` VALUES (1,'Rayton Canyon','{\"X\":-1026.2902,\"Y\":4898.136,\"Z\":172.48645}#-92.92362#112.92362#50.466583#true#false#false','{\"X\":-1038.5304,\"Y\":4875.362,\"Z\":172.4696}#5.0573816#5.3318963#51.096924#true#false#false','{\"X\":-1042.3668,\"Y\":4874.8877,\"Z\":172.4696}#2.8107367#3.2600656#19.321136#true#false#true',NULL,'{\"X\":-1037.8987,\"Y\":4895.4663,\"Z\":172.48645}#1.4573838#2.1827493#52.003754#true#false#false|-1038.5011|4894.233|172.48645|Links rum weiter!|False---{\"X\":-1036.7905,\"Y\":4892.454,\"Z\":172.48645}#1.2155681#2.1882105#50.742798#true#false#false|-1036.1934|4893.4946|172.48645|Gehe den Flur in einer U-Form weiter.|False---{\"X\":-1037.9849,\"Y\":4871.449,\"Z\":172.48645}#2.6919775#1.3132129#52.28067#true#false#false|-1038.6461|4872.6064|172.48645|Bitte nehme deine Sachen aus deiner Kiste im Regal.|False---{\"X\":-1041.4989,\"Y\":4877.2695,\"Z\":172.48645}#2.439779#1.1147511#50.76544#true#false#false|-1042.378|4877.9473|172.48645|Gehe nach Links und warte vor der Tür.|False---{\"X\":-1044.9994,\"Y\":4876.065,\"Z\":172.48645}#1.4073776#2.055779#50.13391#true#false#false|-1045.9912|4874.7427|172.53699|Du bist frei. Bis zum nächsten mal.|True',NULL);
/*!40000 ALTER TABLE `configprisons` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:11:41
